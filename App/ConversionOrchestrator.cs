using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubConvert.Builders;
using SubConvert.Models.Singbox;
using SubConvert.Parsers;
using SubConvert.Services;

namespace SubConvert.App;

public static class ConversionOrchestrator
{
    // 仓库结构常量
    private const string Repo = "SubConfigHub";
    private const string SubconfigsFolder = "clashConfigs";
    private const string OutputBaseFolder = "singboxConfigs";
    private const string LocalOutputFile = "config.json";

    // ── 辅助方法 ──────────────────────────────────────────────────────────────

    private static string GetContentHash(string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes)[..8].ToLower();
    }

    /// <summary>将 YAML 字符串转换为 SingboxConfig 对象。</summary>
    private static SingboxConfig ConvertYaml(string yamlContent, string platform)
    {
        var clashConfig = ClashParser.Parse(yamlContent)
            ?? throw new InvalidOperationException("YAML 解析失败，请检查文件内容。");

        var config = new SingboxConfigBuilder(platform)
            .WithDefaultInbounds()
            .WithDirectOutbound()
            .WithProxyNodes(clashConfig)
            .WithRegionOutbounds()
            .WithProxyGroups()
            .WithDns()
            .WithRouting()
            .Build();

        string json = SerializeConfig(config);
        string hashId = GetContentHash(json);

        return config with
        {
            Experimental = new ExperimentalConfig
            {
                CacheFile = new CacheFileConfig
                {
                    Enabled = true,
                    Path = "cache.db",
                    CacheId = hashId
                },
                ClashApi = platform != "Android"
                    ? new ClashApiConfig
                    {
                        ExternalController = "127.0.0.1:9090",
                        ExternalUi = platform == "Windows" ? "ui" : "/etc/sing-box/ui",
                        Secret = "127001"
                    }
                    : null
            }
        };
    }

    /// <summary>将 SingboxConfig 序列化为格式化 JSON 字符串。</summary>
    private static string SerializeConfig(SingboxConfig config)
    {
        var opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(config, opts);
    }

    /// <summary>从环境变量读取值；若不存在则在控制台提示用户输入。</summary>
    private static string RequireInput(string envVar, string prompt, bool secret = false)
    {
        string? value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine($"[INFO] 已从环境变量 {envVar} 读取配置。");
            return value.Trim();
        }

        Console.Write(prompt);
        if (secret)
        {
            // 读取密码时隐藏输入
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);
                else if (key.Key != ConsoleKey.Backspace)
                    sb.Append(key.KeyChar);
            }
            Console.WriteLine();
            return sb.ToString().Trim();
        }

        return Console.ReadLine()?.Trim() ?? "";
    }

    // ── 核心流程 ──────────────────────────────────────────────────────────────

    public static async Task RunAsync()
    {
        // 1. 获取认证信息
        //string owner = RequireInput("GITHUB_OWNER", "请输入 GitHub 用户名 (仓库所有者): ");
        //string token = RequireInput("GITHUB_TOKEN", "请输入 GitHub Personal Access Token: ", secret: true);
        string owner = "Mousika2049";
        string token = "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";

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("[ERROR] 用户名或 Token 不能为空，程序退出。");
            return;
        }

        var github = new GitHubService(token, owner, Repo);

        // 2. 获取仓库内 YAML 文件列表
        Console.WriteLine($"\n[INFO] 正在获取 {owner}/{Repo}/{SubconfigsFolder} 文件列表...");
        List<(string DisplayName, string RepoPath)> files;
        try
        {
            files = await github.ListYamlFilesAsync(SubconfigsFolder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 获取文件列表失败：{ex.Message}");
            return;
        }

        if (files.Count == 0)
        {
            Console.WriteLine($"[ERROR] {SubconfigsFolder}/ 文件夹内未找到任何 YAML 文件。");
            return;
        }

        // 3. 选择平台
        Console.WriteLine("\n请选择要生成配置的平台：");
        Console.WriteLine("1. Windows");
        Console.WriteLine("2. Android");
        Console.WriteLine("3. Linux");
        Console.Write("请输入选项 (1/2/3): ");
        string platform = Console.ReadLine()?.Trim() switch
        {
            "2" => "Android",
            "3" => "Linux",
            _ => "Windows"
        };
        Console.WriteLine($"[INFO] 目标平台：{platform}");

        // 4. 选择机场
        Console.WriteLine("\n请选择要处理的机场配置：");
        for (int i = 0; i < files.Count; i++)
            Console.WriteLine($"  {i + 1}. {files[i].DisplayName}");
        Console.WriteLine($"  {files.Count + 1}. 以上所有（批量转换并上传至 GitHub）");
        Console.Write($"\n请输入选项 (1-{files.Count + 1}): ");

        if (!int.TryParse(Console.ReadLine()?.Trim(), out int selection)
            || selection < 1
            || selection > files.Count + 1)
        {
            Console.WriteLine("[ERROR] 无效选项，程序退出。");
            return;
        }

        bool allMode = selection == files.Count + 1;

        // 5a. 批量模式 —— 转换全部并上传到 GitHub
        if (allMode)
        {
            Console.WriteLine($"\n[INFO] 开始批量处理 {files.Count} 个机场配置...");
            int success = 0, failed = 0;

            foreach (var (displayName, repoPath) in files)
            {
                Console.WriteLine($"\n──────────────────────────────────────");
                Console.WriteLine($"[INFO] 处理中：{displayName}");
                try
                {
                    string yamlContent = await github.DownloadFileAsync(repoPath);
                    SingboxConfig config = ConvertYaml(yamlContent, platform);
                    string jsonContent = SerializeConfig(config);

                    string targetPath = $"{OutputBaseFolder}/{displayName}/{platform}/config.json";
                    string commitMsg = $"chore: update {displayName} sing-box config [{platform}]";

                    Console.WriteLine($"[INFO] 正在上传到 {Repo}/{targetPath}...");
                    await github.UploadFileAsync(targetPath, jsonContent, commitMsg);
                    Console.WriteLine($"[SUCCESS] {displayName} -> {owner}/{Repo}/{targetPath}");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {displayName} 处理失败：{ex.Message}");
                    failed++;
                }
            }

            Console.WriteLine($"\n══════════════════════════════════════");
            Console.WriteLine($"[DONE] 批量处理完成：{success} 成功，{failed} 失败。");
        }
        // 5b. 单机场模式 —— 转换并保存到本地
        else
        {
            var (displayName, repoPath) = files[selection - 1];
            Console.WriteLine($"\n[INFO] 正在下载 {displayName} 配置...");
            try
            {
                string yamlContent = await github.DownloadFileAsync(repoPath);
                SingboxConfig config = ConvertYaml(yamlContent, platform);
                string jsonContent = SerializeConfig(config);

                File.WriteAllText(LocalOutputFile, jsonContent);
                Console.WriteLine($"[SUCCESS] {displayName} ({platform}) -> {LocalOutputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 处理失败：{ex.Message}");
            }
        }
    }
}