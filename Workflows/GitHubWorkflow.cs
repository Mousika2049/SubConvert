using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Services;

namespace SubConvert.Workflows;

public static class GitHubWorkflow
{
    // 新增参数 AppSettings appSettings
    public static async Task ProcessBatchAsync(GitHubService github, List<(string DisplayName, string RepoPath)> files, TargetPlatform platform, string owner, AppSettings appSettings)
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
                
                // 传入 appSettings
                var result = ConversionService.Convert(yamlContent, platform, appSettings);
                
                // 替换为实例调用 appSettings
                string targetPath = $"{appSettings.OutputBaseFolder}/{displayName}/{platform}/config.json";
                string commitMsg = $"chore: update {displayName} sing-box config [{platform}]";

                Console.WriteLine($"[INFO] 正在上传到 {appSettings.RepoName}/{targetPath}...");
                await github.UploadFileAsync(targetPath, result.JsonContent, commitMsg);
                Console.WriteLine($"[SUCCESS] {displayName} -> {owner}/{appSettings.RepoName}/{targetPath}");
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

    // 新增参数 AppSettings appSettings
    public static async Task ProcessSingleAsync(GitHubService github, string displayName, string repoPath, TargetPlatform platform, AppSettings appSettings)
    {
        Console.WriteLine($"\n[INFO] 正在下载 {displayName} 配置...");
        try
        {
            string yamlContent = await github.DownloadFileAsync(repoPath);
            
            // 传入 appSettings
            var result = ConversionService.Convert(yamlContent, platform, appSettings);

            // 替换为实例调用 appSettings
            await File.WriteAllTextAsync(appSettings.LocalOutputFile, result.JsonContent);
            Console.WriteLine($"[SUCCESS] {displayName} ({platform}) -> {appSettings.LocalOutputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 处理失败：{ex.Message}");
        }
    }
}