using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Services;

namespace SubConvert.Workflows;

// 注入 ILogger<GitHubWorkflow>
public class GitHubWorkflow(ConversionService conversionService, IOptions<AppSettings> options, ILogger<GitHubWorkflow> logger)
{
    private readonly AppSettings _appSettings = options.Value;

    public async Task ProcessBatchAsync(GitHubService github, List<(string DisplayName, string RepoPath)> files, TargetPlatform platform, string owner)
    {
        logger.LogInformation("开始批量处理 {Count} 个机场配置...", files.Count);
        int success = 0, failed = 0;

        foreach (var (displayName, repoPath) in files)
        {
            logger.LogInformation("──────────────────────────────────────");
            logger.LogInformation("处理中：{DisplayName}", displayName);
            try
            {
                string yamlContent = await github.DownloadFileAsync(repoPath);
                var result = conversionService.Convert(yamlContent, platform);
                
                string targetPath = $"{_appSettings.OutputBaseFolder}/{displayName}/{platform}/config.json";
                string commitMsg = $"chore: update {displayName} sing-box config [{platform}]";

                logger.LogInformation("正在上传到 {Repo}/{TargetPath}...", _appSettings.RepoName, targetPath);
                await github.UploadFileAsync(targetPath, result.JsonContent, commitMsg);
                
                logger.LogInformation("上传成功: {DisplayName} -> {Owner}/{Repo}/{TargetPath}", displayName, owner, _appSettings.RepoName, targetPath);
                success++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{DisplayName} 处理失败：{Message}", displayName, ex.Message);
                failed++;
            }
        }

        logger.LogInformation("══════════════════════════════════════");
        logger.LogInformation("批量处理完成：{Success} 成功，{Failed} 失败。", success, failed);
    }

    public async Task ProcessSingleAsync(GitHubService github, string displayName, string repoPath, TargetPlatform platform)
    {
        logger.LogInformation("正在下载 {DisplayName} 配置...", displayName);
        try
        {
            string yamlContent = await github.DownloadFileAsync(repoPath);
            var result = conversionService.Convert(yamlContent, platform);

            await File.WriteAllTextAsync(_appSettings.LocalOutputFile, result.JsonContent);
            logger.LogInformation("生成成功: {DisplayName} ({Platform}) -> {LocalOutputFile}", displayName, platform, _appSettings.LocalOutputFile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理失败：{Message}", ex.Message);
        }
    }
}