using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Services;
using SubConvert.Ui;
using SubConvert.Workflows;

namespace SubConvert.App;

// 注入 IUserInterface (变量名定为 ui)
public class ConversionOrchestrator(IOptions<AppSettings> options, GitHubWorkflow workflow, ILogger<ConversionOrchestrator> logger, IUserInterface ui)
{
    public async Task RunAsync()
    {
        var appSettings = options.Value;

        // 使用注入的 ui 实例调用，而不是静态调用
        string owner = !string.IsNullOrWhiteSpace(appSettings.GitHubOwner) 
            ? appSettings.GitHubOwner 
            : ui.RequireInput("SUBCONVERT_GITHUB_OWNER", "请输入 GitHub 用户名 (仓库所有者): ");

        string token = !string.IsNullOrWhiteSpace(appSettings.GitHubToken) 
            ? appSettings.GitHubToken 
            : ui.RequireInput("SUBCONVERT_GITHUB_TOKEN", "请输入 GitHub Personal Access Token: ", secret: true);

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(token))
        {
            logger.LogError("用户名或 Token 不能为空，程序退出。");
            return;
        }

        var github = new GitHubService(token, owner, appSettings.RepoName);
        
        logger.LogInformation("正在获取 {Owner}/{RepoName}/{Folder} 文件列表...", owner, appSettings.RepoName, appSettings.SubconfigsFolder);
        
        List<(string DisplayName, string RepoPath)> files;
        try
        {
            files = await github.ListYamlFilesAsync(appSettings.SubconfigsFolder);
        }        
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文件列表失败：{Message}", ex.Message);
            return;
        }

        if (files.Count == 0)
        {
            logger.LogError("{Folder}/ 文件夹内未找到任何 YAML 文件。", appSettings.SubconfigsFolder);
            return;
        }

        // 使用注入的 ui 实例调用
        TargetPlatform platform = ui.SelectPlatform();
        int selection = ui.SelectAirport(files);
        
        if (selection < 1)
        {
            logger.LogError("无效选项，程序退出。");
            return;
        }

        bool allMode = selection == files.Count + 1;
        if (allMode)
        {
            await workflow.ProcessBatchAsync(github, files, platform, owner);
        }
        else
        {
            var (displayName, repoPath) = files[selection - 1];
            await workflow.ProcessSingleAsync(github, displayName, repoPath, platform);
        }
    }
}