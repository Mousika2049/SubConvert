using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Services;
using SubConvert.Ui;
using SubConvert.Workflows;

namespace SubConvert.App;

public static class ConversionOrchestrator
{
    public static async Task RunAsync()
    {
        // 1. 获取认证信息 (AppSettings 兜底 -> 环境变量 -> UI 交互)
        string owner = !string.IsNullOrWhiteSpace(AppSettings.DefaultGitHubOwner) 
            ? AppSettings.DefaultGitHubOwner 
            : ConsoleUi.RequireInput("GITHUB_OWNER", "请输入 GitHub 用户名 (仓库所有者): ");

        string token = !string.IsNullOrWhiteSpace(AppSettings.DefaultGitHubToken) 
            ? AppSettings.DefaultGitHubToken 
            : ConsoleUi.RequireInput("GITHUB_TOKEN", "请输入 GitHub Personal Access Token: ", secret: true);

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("[ERROR] 用户名或 Token 不能为空，程序退出。");
            return;
        }

        var github = new GitHubService(token, owner, AppSettings.RepoName);

        // 2. 获取仓库内 YAML 文件列表
        Console.WriteLine($"\n[INFO] 正在获取 {owner}/{AppSettings.RepoName}/{AppSettings.SubconfigsFolder} 文件列表...");
        List<(string DisplayName, string RepoPath)> files;
        try
        {
            files = await github.ListYamlFilesAsync(AppSettings.SubconfigsFolder);
        }        
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 获取文件列表失败：{ex.Message}");
            return;
        }

        if (files.Count == 0)
        {
            Console.WriteLine($"[ERROR] {AppSettings.SubconfigsFolder}/ 文件夹内未找到任何 YAML 文件。");
            return;
        }

        // 3. 交互式选择平台
        TargetPlatform platform = ConsoleUi.SelectPlatform();

        // 4. 交互式选择机场
        int selection = ConsoleUi.SelectAirport(files);
        if (selection < 1)
        {
            Console.WriteLine("[ERROR] 无效选项，程序退出。");
            return;
        }

        // 5. 将任务分发给工作流执行
        bool allMode = selection == files.Count + 1;
        if (allMode)
        {
            await GitHubWorkflow.ProcessBatchAsync(github, files, platform, owner);
        }
        else
        {
            var (displayName, repoPath) = files[selection - 1];
            await GitHubWorkflow.ProcessSingleAsync(github, displayName, repoPath, platform);
        }
    }
}