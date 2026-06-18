using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Services;
using SubConvert.Ui;
using SubConvert.Workflows;
using Microsoft.Extensions.Configuration;

namespace SubConvert.App;

public static class ConversionOrchestrator
{
    // 接收 args，使得工具支持命令行参数覆盖
    public static async Task RunAsync(string[] args)
    {
        // 1. 构建标准配置树：仅依赖环境变量和命令行参数
        IConfiguration config = new ConfigurationBuilder()
            // 支持环境变量，统一加前缀，例如 SUBCONVERT_GitHubToken
            .AddEnvironmentVariables(prefix: "SUBCONVERT_") 
            // 支持终端参数，例如 --GitHubOwner="MyName"
            .AddCommandLine(args)
            .Build();

        // 将配置绑定到我们的实体类上
        var appSettings = config.Get<AppSettings>() ?? new AppSettings();

        // 2. 凭证检查与回退控制台输入
        string owner = !string.IsNullOrWhiteSpace(appSettings.GitHubOwner) 
            ? appSettings.GitHubOwner 
            : ConsoleUi.RequireInput("GITHUB_OWNER", "请输入 GitHub 用户名 (仓库所有者): ");

        string token = !string.IsNullOrWhiteSpace(appSettings.GitHubToken) 
            ? appSettings.GitHubToken 
            : ConsoleUi.RequireInput("GITHUB_TOKEN", "请输入 GitHub Personal Access Token: ", secret: true);

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("[ERROR] 用户名或 Token 不能为空，程序退出。");
            return;
        }

        var github = new GitHubService(token, owner, appSettings.RepoName);
        
        // 2. 获取仓库内 YAML 文件列表
        Console.WriteLine($"\n[INFO] 正在获取 {owner}/{appSettings.RepoName}/{appSettings.SubconfigsFolder} 文件列表...");
        List<(string DisplayName, string RepoPath)> files;
        try
        {
            files = await github.ListYamlFilesAsync(appSettings.SubconfigsFolder);
        }        
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 获取文件列表失败：{ex.Message}");
            return;
        }

        if (files.Count == 0)
        {
            Console.WriteLine($"[ERROR] {appSettings.SubconfigsFolder}/ 文件夹内未找到任何 YAML 文件。");
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
            await GitHubWorkflow.ProcessBatchAsync(github, files, platform, owner, appSettings);
        }
        else
        {
            var (displayName, repoPath) = files[selection - 1];
            await GitHubWorkflow.ProcessSingleAsync(github, displayName, repoPath, platform, appSettings);
        }
    }
}