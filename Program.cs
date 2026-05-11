using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SubConvert;

public record ClashConfig
{
    [YamlMember(Alias = "proxies")]
    public List<Dictionary<string, object>> Proxies { get; init; } = [];
}

public record SingboxConfig
{
    [JsonPropertyName("log")] public LogConfig Log { get; init; } = new();
    [JsonPropertyName("dns")] public DnsConfig Dns { get; init; } = new();
    [JsonPropertyName("inbounds")] public List<Inbound> Inbounds { get; init; } = [];
    [JsonPropertyName("outbounds")] public List<Outbound> Outbounds { get; init; } = [];
    [JsonPropertyName("route")] public RouteConfig Route { get; init; } = new();
    [JsonPropertyName("experimental")] public ExperimentalConfig? Experimental { get; init; }
}

public record LogConfig
{
    [JsonPropertyName("level")] public string Level { get; init; } = "warn";
    [JsonPropertyName("timestamp")] public bool Timestamp { get; init; } = true;
}

public record DnsConfig
{
    [JsonPropertyName("servers")] public List<DnsServer> Servers { get; init; } = [];
    [JsonPropertyName("rules")] public List<DnsRule> Rules { get; init; } = [];
    [JsonPropertyName("final")] public string? Final { get; init; } = "remote";
    [JsonPropertyName("strategy")] public string? Strategy { get; init; } = "ipv4_only";
    [JsonPropertyName("reverse_mapping")] public bool? ReverseMapping { get; init; } = true;
}

public record DnsServer
{
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("detour")] public string? Detour { get; init; }
    // 【Linux 专属优化字段】
    [JsonPropertyName("prefer_go")] public bool? PreferGo { get; init; }
}

public record DnsRule
{
    [JsonPropertyName("rule_set")] public List<string>? RuleSet { get; init; }
    [JsonPropertyName("domain")] public List<string>? Domain { get; init; }
    [JsonPropertyName("domain_suffix")] public List<string>? DomainSuffix { get; init; }
    [JsonPropertyName("query_type")] public List<string>? QueryType { get; init; }
    [JsonPropertyName("action")] public string? Action { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("rcode")] public string? Rcode { get; init; }
}

public record Inbound
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("listen")] public string? Listen { get; init; }
    [JsonPropertyName("listen_port")] public int? ListenPort { get; init; }
    [JsonPropertyName("address")] public List<string>? Address { get; init; }
    [JsonPropertyName("auto_route")] public bool? AutoRoute { get; init; }
    [JsonPropertyName("strict_route")] public bool? StrictRoute { get; init; }
    [JsonPropertyName("stack")] public string? Stack { get; init; }
    [JsonPropertyName("mtu")] public int? Mtu { get; init; }
    [JsonPropertyName("auto_redirect")] public bool? AutoRedirect { get; init; }
}

public record Outbound
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("server_port")] public int? ServerPort { get; init; }
    // VLESS 核心字段
    [JsonPropertyName("uuid")] public string? Uuid { get; init; }
    [JsonPropertyName("flow")] public string? Flow { get; init; }
    [JsonPropertyName("packet_encoding")] public string? PacketEncoding { get; init; }
    [JsonPropertyName("password")] public string? Password { get; init; }
    [JsonPropertyName("outbounds")] public List<string>? Outbounds { get; init; }
    [JsonPropertyName("default")] public string? Default { get; init; }
    [JsonPropertyName("tls")] public OutboundTls? Tls { get; init; }
    [JsonPropertyName("domain_resolver")] public string? DomainResolver { get; init; }
    [JsonPropertyName("connect_timeout")] public string? ConnectTimeout { get; init; }
    [JsonPropertyName("interrupt_exist_connections")] public bool? InterruptExistConnections { get; init; }
}

public record Utls
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    [JsonPropertyName("fingerprint")] public string? Fingerprint { get; init; }
}

public record OutboundTls
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; } = true;
    [JsonPropertyName("server_name")] public string? ServerName { get; init; }
    [JsonPropertyName("insecure")] public bool? Insecure { get; init; }
    [JsonPropertyName("utls")] public Utls? Utls { get; init; }
    [JsonPropertyName("alpn")] public List<string>? Alpn { get; init; }
    [JsonPropertyName("min_version")] public string? MinVersion { get; init; }
    // REALITY 支持
    [JsonPropertyName("reality")] public OutboundReality? Reality { get; init; }
}
// REALITY 配置模型
public record OutboundReality
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    [JsonPropertyName("public_key")] public string? PublicKey { get; init; }
    [JsonPropertyName("short_id")] public string? ShortId { get; init; }
}
public record RouteConfig
{
    [JsonPropertyName("rule_set")] public List<SingboxRuleSet> RuleSet { get; init; } = [];
    [JsonPropertyName("rules")] public List<RouteRule> Rules { get; init; } = [];
    [JsonPropertyName("final")] public string? Final { get; init; } = Constants.MainProxyGroup;
    [JsonPropertyName("auto_detect_interface")] public bool? AutoDetectInterface { get; init; } = true;
}

public record SingboxRuleSet
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("format")] public string? Format { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("download_detour")] public string? DownloadDetour { get; init; }
    [JsonPropertyName("update_interval")] public string? UpdateInterval { get; init; }
}

public record RouteRule
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("mode")] public string? Mode { get; init; }
    [JsonPropertyName("rules")] public List<RouteRule>? Rules { get; init; }
    [JsonPropertyName("inbound")] public List<string>? Inbound { get; init; }
    [JsonPropertyName("protocol")] public List<string>? Protocol { get; init; }
    [JsonPropertyName("port")] public List<int>? Port { get; init; }
    [JsonPropertyName("network")] public List<string>? Network { get; init; }
    [JsonPropertyName("action")] public string? Action { get; init; }
    [JsonPropertyName("rule_set")] public List<string>? RuleSet { get; init; }
    [JsonPropertyName("ip_cidr")] public List<string>? IpCidr { get; init; }
    [JsonPropertyName("ip_is_private")] public bool? IpIsPrivate { get; init; }
    [JsonPropertyName("outbound")] public string? Outbound { get; init; }
    [JsonPropertyName("timeout")] public string? Timeout { get; init; }
}

public record ExperimentalConfig
{
    [JsonPropertyName("cache_file")] public CacheFileConfig? CacheFile { get; init; }
    [JsonPropertyName("clash_api")] public ClashApiConfig? ClashApi { get; init; }
}

public record CacheFileConfig
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    [JsonPropertyName("path")] public string? Path { get; init; }
    [JsonPropertyName("cache_id")] public string? CacheId { get; init; }
}

public record ClashApiConfig
{
    [JsonPropertyName("external_controller")] public string? ExternalController { get; init; }
    [JsonPropertyName("external_ui")] public string? ExternalUi { get; init; }
    [JsonPropertyName("secret")] public string? Secret { get; init; }
}

public static class Constants
{
    public const string MainProxyGroup = "🚀 PROXIES";
    public const string Direct = "DIRECT";

    public static readonly Dictionary<string, Regex> RegionRegexes = new()
    {
        { "🇭🇰 香港", new Regex(@"(?i)香港|hong\s?kong|深港|🇭🇰|(?<![a-zA-Z])hkg?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇸🇬 狮城", new Regex(@"(?i)狮城|新加坡|singapore|🇸🇬|(?<![a-zA-Z])sgp?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇯🇵 日本", new Regex(@"(?i)日本|japan|tokyo|东京|大阪|🇯🇵|(?<![a-zA-Z])jpn?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇺🇸 美国", new Regex(@"(?i)美国|america|洛杉矶|硅谷|🇺🇸|(?<![a-zA-Z])usa?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
    };
}

public static class ClashParser
{
    public static ClashConfig? Parse(string yamlContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        try
        {
            return deserializer.Deserialize<ClashConfig>(yamlContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] YAML Parsing failed: {ex.Message}");
            return null;
        }
    }
}

public class SingboxConfigBuilder(string platform)
{
    private readonly string _platform = platform;
    private readonly LogConfig _log = new();
    private readonly DnsConfig _dns = new();
    private readonly List<Inbound> _inbounds = [];
    private readonly RouteConfig _route = new();
    private readonly HashSet<string> _proxyServerDomains = [];
    private readonly List<string> _allNodeNames = [];
    private readonly List<string> _finalRegionGroupNames = [];
    
    private Outbound? _directOutbound;
    private readonly List<Outbound> _nodeOutbounds = [];
    private readonly List<Outbound> _regionOutbounds = [];
    private readonly List<Outbound> _mainOutbounds = [];
    private readonly List<Outbound> _serviceOutbounds = [];

    public SingboxConfigBuilder WithDefaultInbounds()
    {
        _inbounds.Add(new Inbound
        {
            Type = "tun",
            Tag = "tun-in",
            Address = ["172.19.0.1/30", "fd00::1/126"],
            AutoRoute = true,
            AutoRedirect = _platform == "Linux" ? true : null,
            StrictRoute = true,
            Stack = _platform switch
            {
                "Windows" => "mixed",
                "Linux" => "system",
                "Android" => "gvisor",
                _ => null
            },
            Mtu = 1480,
        });
        _inbounds.Add(new Inbound
        {
            Type = "mixed",
            Tag = "mixed-in",
            Listen = "127.0.0.1",
            ListenPort = 8848
        });
        return this;
    }

    public SingboxConfigBuilder WithDirectOutbound()
    {
        _directOutbound = new Outbound
        {
            Type = "direct",
            Tag = Constants.Direct,
            DomainResolver = "local"
        };
        return this;
    }

    public SingboxConfigBuilder WithProxyNodes(ClashConfig clashConfig)
    {
        if (clashConfig.Proxies == null) return this;

        foreach (var p in clashConfig.Proxies)
        {
            if (!p.TryGetValue("type", out var typeObj)) continue;
            string type = typeObj.ToString()!;

            if (type != "trojan" && type != "vless") continue;

            string name = p["name"].ToString()!;
            string server = p["server"].ToString()!;
            int port = int.Parse(p["port"].ToString()!);

            _allNodeNames.Add(name);
            if (!IPAddress.TryParse(server, out _)) _proxyServerDomains.Add(server);

            OutboundTls? tlsConfig = ExtractTlsConfig(p, type, server);

            Outbound? outbound = type switch
            {
                "trojan" => BuildTrojanOutbound(p, name, server, port, tlsConfig),
                "vless" => BuildVlessOutbound(p, name, server, port, tlsConfig),
                _ => null // 兜底安全策略
            };

            if (outbound != null)
            {
                _nodeOutbounds.Add(outbound);
            }
        }
        return this;
    }
    private Outbound BuildTrojanOutbound(Dictionary<string, object> p, string name, string server, int port, OutboundTls? tlsConfig)
    {
        return new Outbound
        {
            Type = "trojan",
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Password = p.TryGetValue("password", out var pwd) ? pwd.ToString() : "",
            Tls = tlsConfig
        };
    }

    private Outbound BuildVlessOutbound(Dictionary<string, object> p, string name, string server, int port, OutboundTls? tlsConfig)
    {
        return new Outbound
        {
            Type = "vless",
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Uuid = p.TryGetValue("uuid", out var id) ? id.ToString() : "",
            Flow = p.TryGetValue("flow", out var f) ? f.ToString() : null,
            PacketEncoding = "xudp",
            Tls = tlsConfig
        };
    }

    private OutboundTls? ExtractTlsConfig(Dictionary<string, object> p, string type, string server)
    {
        bool isTls = p.TryGetValue("tls", out var tlsObj) && bool.TryParse(tlsObj.ToString(), out var b) && b;
        bool isReality = p.ContainsKey("reality-opts");

        // 如果既没标 tls，也不是 reality，且不是天生走 tls 的 trojan，则返回 null
        if (type != "trojan" && !isTls && !isReality) return null;

        // 提取 Reality 专属
        OutboundReality? realityConfig = null;
        if (isReality && p["reality-opts"] is Dictionary<object, object> realityOpts)
        {
            realityConfig = new OutboundReality
            {
                Enabled = true,
                PublicKey = realityOpts.TryGetValue("public-key", out var pk) ? pk.ToString() : null,
                ShortId = realityOpts.TryGetValue("short-id", out var sid) ? sid.ToString() : null
            };
        }

        // 提取指纹
        string fp = p.TryGetValue("client-fingerprint", out var fpObj) ? fpObj.ToString()! : "firefox";

        // 优雅处理 sni、servername 与 fallback 逻辑，杜绝空指针异常
        string serverName = server; // 默认 fallback 到 server
        if (p.TryGetValue("sni", out var sniObj))
            serverName = sniObj.ToString()!;
        else if (p.TryGetValue("servername", out var snObj))
            serverName = snObj.ToString()!;

        return new OutboundTls
        {
            Enabled = true,
            ServerName = serverName,
            Insecure = p.TryGetValue("skip-cert-verify", out var skipCert) && bool.TryParse(skipCert.ToString(), out var insec) ? insec : null,
            Utls = new Utls { Enabled = true, Fingerprint = fp },
            Alpn = ["h2", "http/1.1"],
            MinVersion = "1.3",
            Reality = realityConfig
        };
    }
    public SingboxConfigBuilder WithRegionOutbounds()
    {
        foreach (var region in Constants.RegionRegexes)
        {
            string groupName = region.Key;
            var matchedNodes = _allNodeNames.Where(name => region.Value.IsMatch(name)).ToList();

            if (matchedNodes.Count >= 2)
            {
                _finalRegionGroupNames.Add(groupName);
                _regionOutbounds.Add(new Outbound
                {
                    Type = "selector",
                    Tag = groupName,
                    Outbounds = matchedNodes,
                    Default = matchedNodes.FirstOrDefault(),
                    InterruptExistConnections = true
                });
            }
        }
        return this;
    }

    public SingboxConfigBuilder WithProxyGroups()
    {
        var mainGroupOptions = new List<string>(_finalRegionGroupNames);
        mainGroupOptions.AddRange(_allNodeNames);
        mainGroupOptions.Add(Constants.Direct);

        _mainOutbounds.Add(new Outbound
        {
            Type = "selector",
            Tag = Constants.MainProxyGroup,
            Outbounds = mainGroupOptions,
            Default = _finalRegionGroupNames.FirstOrDefault() ?? _allNodeNames.FirstOrDefault() ?? Constants.Direct,
            InterruptExistConnections = true
        });

        var serviceGroupOptions = new List<string> { Constants.MainProxyGroup };
        serviceGroupOptions.AddRange(_finalRegionGroupNames);
        serviceGroupOptions.AddRange(_allNodeNames);
        serviceGroupOptions.Add(Constants.Direct);

        string usGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? Constants.MainProxyGroup;
        string sgGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇸🇬")) ?? Constants.MainProxyGroup;
        string hkGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇭🇰")) ?? Constants.MainProxyGroup;

        var specialGroups = new Dictionary<string, string>
        {
            { "🎵 Spotify", Constants.MainProxyGroup },
            { "🎮 Steam", hkGroup },
            { "🤖 AI", usGroup },
            { "🪟 Microsoft", hkGroup },
            { "✈️ Telegram", Constants.MainProxyGroup },
        };

        _serviceOutbounds.AddRange(specialGroups.Select(group => new Outbound
        {
            Type = "selector",
            Tag = group.Key,
            Outbounds = serviceGroupOptions,
            Default = group.Value,
            InterruptExistConnections = true
        }));

        return this;
    }

    public SingboxConfigBuilder WithDns()
    {
        _dns.Servers.AddRange([
            new DnsServer { Tag = "bootstrap", Type = "local", PreferGo = _platform == "Linux" ? true : null },
            new DnsServer { Tag = "node-resolver", Type = "udp", Server = "223.5.5.5" },
            new DnsServer { Tag = "remote", Type = "https", Server = "1.1.1.1", Detour = Constants.MainProxyGroup },
            new DnsServer { Tag = "local", Type = "https", Server = "223.5.5.5" }
        ]);

        _dns.Rules.Add(new DnsRule { QueryType = ["AAAA"], Action = "predefined", Rcode = "NOERROR" });
        _dns.Rules.Add(new DnsRule { RuleSet = ["geosite-category-ads-all"], Action = "predefined", Rcode = "NOERROR" });

        if (_proxyServerDomains.Count > 0)
        {
            _dns.Rules.Add(new DnsRule { Domain = [.. _proxyServerDomains], Action = "route", Server = "node-resolver" });
        }

        _dns.Rules.Add(new DnsRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Server = "local" });

        return this;
    }

    public SingboxConfigBuilder WithRouting()
    {
        _route.RuleSet.AddRange([
            CreateRemoteRuleSet("geosite-category-ads-all", "geosite", "category-ads-all"),
            CreateRemoteRuleSet("geosite-category-pt", "geosite", "category-pt"),
            CreateRemoteRuleSet("geosite-cn", "geosite", "cn"),
            CreateRemoteRuleSet("geoip-cn", "geoip", "cn"),
            CreateRemoteRuleSet("geosite-spotify", "geosite", "spotify"),
            CreateRemoteRuleSet("geosite-steam", "geosite", "steam"),
            CreateRemoteRuleSet("geosite-category-ai-!cn", "geosite", "category-ai-!cn"),
            CreateRemoteRuleSet("geosite-microsoft", "geosite", "microsoft"),
            CreateRemoteRuleSet("geosite-telegram", "geosite", "telegram"),
            CreateRemoteRuleSet("geoip-telegram", "geoip", "telegram"),
        ]);

        _route.Rules.AddRange([
            new RouteRule
            {
                Type = "logical",
                Mode = "and",
                Rules =
                [
                    new RouteRule { Inbound = ["tun-in", "mixed-in"] },
                    new RouteRule
                    {
                        Type = "logical",
                        Mode = "or",
                        Rules = [ new RouteRule { Protocol = ["dns"] }, new RouteRule { Port = [53] } ]
                    }
                ],
                Action = "hijack-dns"
            },
            new RouteRule { IpIsPrivate = true, Action = "route", Outbound = Constants.Direct },
            new RouteRule { IpCidr = ["::/0"], Action = "reject" },
            new RouteRule { IpCidr = ["223.5.5.5/32"], Action = "route", Outbound = Constants.Direct },
            new RouteRule { Port = [3478, 3479, 19302, 19303], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Port = [443], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Action = "sniff", Timeout = "300ms" },
            new RouteRule { Protocol = ["ssh"], Action = "route", Outbound = Constants.Direct },
            new RouteRule { RuleSet = ["geosite-category-ads-all"], Action = "reject" },
            new RouteRule { RuleSet = ["geosite-spotify"], Action = "route", Outbound = "🎵 Spotify" },
            new RouteRule { RuleSet = ["geosite-steam"], Action = "route", Outbound = "🎮 Steam" },
            new RouteRule { RuleSet = ["geosite-category-ai-!cn"], Action = "route", Outbound = "🤖 AI" },
            new RouteRule { RuleSet = ["geosite-microsoft"], Action = "route", Outbound = "🪟 Microsoft" },
            new RouteRule { RuleSet = ["geosite-telegram"], Action = "route", Outbound = "✈️ Telegram" },
            new RouteRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Outbound = Constants.Direct },
            new RouteRule { Inbound = ["mixed-in"], Action = "resolve" },
            new RouteRule { RuleSet = ["geoip-telegram"], Action = "route", Outbound = "✈️ Telegram" },
            new RouteRule { RuleSet = ["geoip-cn"], Action = "route", Outbound = Constants.Direct }
        ]);

        return this;
    }

    public SingboxConfig Build()
    {
        var orderedOutbounds = new List<Outbound>();
        orderedOutbounds.AddRange(_mainOutbounds);    // 1. 先是 Proxies 组
        orderedOutbounds.AddRange(_regionOutbounds);  // 2. 然后是 分地区组
        orderedOutbounds.AddRange(_serviceOutbounds); // 3. 接着是 各大服务组
        orderedOutbounds.AddRange(_nodeOutbounds);    // 4. 然后是 各个底层节点
        if (_directOutbound != null)
        {
            orderedOutbounds.Add(_directOutbound);    // 5. 最后是 DIRECT 直连
        }

        return new SingboxConfig
        {
            Log = _log,
            Dns = _dns,
            Inbounds = _inbounds,
            Outbounds = orderedOutbounds, // 传入排序好的集合
            Route = _route,
        };
    }

    private static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName) => new()
    {
        Tag = tag,
        Type = "remote",
        Format = "binary",
        Url = $"https://fastly.jsdelivr.net/gh/MetaCubeX/meta-rules-dat@sing/geo/{repoType}/{fileName}.srs",
        DownloadDetour = Constants.Direct,
        UpdateInterval = "1d"
    };
}

// ── GitHub API 响应模型 ────────────────────────────────────────────────────────

public record GitHubContentItem
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("path")] public string Path { get; init; } = "";
    [JsonPropertyName("type")] public string Type { get; init; } = "";
    [JsonPropertyName("sha")] public string? Sha { get; init; }
    // 单文件 GET 时才存在，内容为 Base64（带换行），列目录时为 null
    [JsonPropertyName("content")] public string? Content { get; init; }
}

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

        return items
            .Where(i => i.Type == "file" &&
                        i.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            .Select(i => (
                DisplayName: Path.GetFileNameWithoutExtension(i.Name),
                RepoPath: i.Path))
            .ToList();
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

// ── 主程序 ────────────────────────────────────────────────────────────────────

class Program
{
    // 仓库结构常量，按需修改
    private const string Repo = "SubConfigHub";
    private const string SubconfigsFolder = "subconfigs";
    private const string OutputBaseFolder = "singboxConfigs";
    private const string LocalOutputFile = "config.json";

    // ── 辅助方法 ──────────────────────────────────────────────────────────────

    static string GetContentHash(string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes)[..8].ToLower();
    }

    /// <summary>将 YAML 字符串转换为 SingboxConfig 对象。</summary>
    static SingboxConfig ConvertYaml(string yamlContent, string platform)
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
    static string SerializeConfig(SingboxConfig config)
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
    static string RequireInput(string envVar, string prompt, bool secret = false)
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

    // ── 入口 ──────────────────────────────────────────────────────────────────

    static async Task Main()
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