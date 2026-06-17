using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SubConvert.Models.GitHub;

namespace SubConvert.Services;

// ── GitHub API 服务封装 ───────────────────────────────────────────────────────

public class GitHubService(string token, string owner, string repo)
{
    private static readonly HttpClient Http = new();

    // 每次请求都使用最新 token，避免跨实例污染
    private HttpRequestMessage NewRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.UserAgent.ParseAdd("SubConvert/1.0");
        req.Headers.Accept.ParseAdd("application/vnd.github+json");
        req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        return req;
    }

    /// <summary>列出指定文件夹下所有 .yaml 文件，返回 (显示名, 仓库路径) 列表。</summary>
    public async Task<List<(string DisplayName, string RepoPath)>> ListYamlFilesAsync(string folderPath)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{folderPath}";
        using var req = NewRequest(HttpMethod.Get, url);
        using var resp = await Http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        string json = await resp.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<GitHubContentItem>>(json) ?? [];

        return [.. items
            .Where(i => i.Type == "file" &&
                        i.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            .Select(i => (
                DisplayName: Path.GetFileNameWithoutExtension(i.Name),
                RepoPath: i.Path))];
    }

    /// <summary>下载指定路径的文件内容，返回 UTF-8 字符串。</summary>
    public async Task<string> DownloadFileAsync(string repoPath)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{repoPath}";
        using var req = NewRequest(HttpMethod.Get, url);
        using var resp = await Http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        string json = await resp.Content.ReadAsStringAsync();
        var item = JsonSerializer.Deserialize<GitHubContentItem>(json)
                   ?? throw new InvalidOperationException("GitHub API 返回了空响应。");

        // Content 字段是 Base64（GitHub 会插入换行，需先移除）
        string base64 = item.Content!.Replace("\n", "").Replace("\r", "");
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    /// <summary>
    /// 将文本内容上传（新建或覆盖）到仓库指定路径。
    /// 若文件已存在，自动获取其 sha 再执行更新。
    /// </summary>
    public async Task UploadFileAsync(string repoPath, string textContent, string commitMessage)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{repoPath}";

        // 先查询文件是否存在以获取 sha（更新文件时必须提供）
        string? existingSha = null;
        using (var checkReq = NewRequest(HttpMethod.Get, url))
        using (var checkResp = await Http.SendAsync(checkReq))
        {
            if (checkResp.IsSuccessStatusCode)
            {
                string existingJson = await checkResp.Content.ReadAsStringAsync();
                existingSha = JsonSerializer.Deserialize<GitHubContentItem>(existingJson)?.Sha;
            }
        }

        // 构造 PUT 请求体
        var body = new Dictionary<string, string?>
        {
            ["message"] = commitMessage,
            ["content"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(textContent))
        };
        if (existingSha != null)
            body["sha"] = existingSha;

        string bodyJson = JsonSerializer.Serialize(body);
        using var putReq = NewRequest(HttpMethod.Put, url);
        putReq.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        using var putResp = await Http.SendAsync(putReq);
        putResp.EnsureSuccessStatusCode();
    }
}