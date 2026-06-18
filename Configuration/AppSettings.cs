namespace SubConvert.Configuration;

public class AppSettings
{
    // ── GitHub 认证与仓库配置 ──
    public string GitHubOwner { get; set; } = "";
    public string GitHubToken { get; set; } = "";
    public string RepoName { get; set; } = "SubConfigHub";
    public string SubconfigsFolder { get; set; } = "clashConfigs";
    public string OutputBaseFolder { get; set; } = "singboxConfigs";
    public string LocalOutputFile { get; set; } = "config.json";

    // ── 核心代理组常量 ──
    public string MainProxyGroup { get; set; } = "🚀 PROXIES";
    public string Direct { get; set; } = "DIRECT";
}