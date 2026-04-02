using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LinkCubeConvert;

public class ClashConfig
{
    [YamlMember(Alias = "proxies")] required public List<Dictionary<string, object>> Proxies { get; set; }
    [YamlMember(Alias = "proxy-groups")] required public List<ClashProxyGroup> ProxyGroups { get; set; }
}

public class ClashProxyGroup
{
    [YamlMember(Alias = "name")] required public string Name { get; set; }
    [YamlMember(Alias = "type")] required public string Type { get; set; }
    [YamlMember(Alias = "proxies")] required public List<string> Proxies { get; set; }
}

public class SingboxConfig
{
    public LogConfig log { get; set; } = new();
    public DnsConfig dns { get; set; } = new();
    public List<Inbound> inbounds { get; set; } = [];
    public List<Outbound> outbounds { get; set; } = [];
    public RouteConfig route { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ExperimentalConfig experimental { get; set; }
}

public class LogConfig
{
    public string level { get; set; } = "warn";
    public bool timestamp { get; set; } = true;
}

public class DnsConfig
{
    public List<DnsServer> servers { get; set; } = [];
    public List<DnsRule> rules { get; set; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; }
    public string strategy { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? reverse_mapping { get; set; }
}

public class DnsServer
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string type { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string detour { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string domain_resolver { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string address_strategy { get; set; }
}

public class DnsRule
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> query_type { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string rcode { get; set; }
}

public class Inbound
{
    public string type { get; set; }
    public string tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string listen { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? listen_port { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> address { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_route { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? strict_route { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string stack { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? mtu { get; set; }
}

public class Outbound
{
    public string type { get; set; }
    public string tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string password { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbounds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string @default { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public OutboundTls tls { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string domain_resolver { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tcp_keep_alive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tcp_keep_alive_interval { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? tcp_fast_open { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string connect_timeout { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interval { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? tolerance { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string idle_timeout { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? interrupt_exist_connections { get; set; }
}

public class Utls
{
    public bool enabled { get; set; } = true;
    public string fingerprint { get; set; } = "chrome";
}

public class OutboundTls
{
    public bool enabled { get; set; } = true;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server_name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? insecure { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Utls utls { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> alpn { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string min_version { get; set; }
}

public class RouteConfig
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = [];
    public List<RouteRule> rules { get; set; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; }
    
    // 【平台隔离核心参数】：开放操作系统专属接管权限
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_detect_interface { get; set; }
}

public class SingboxRuleSet
{
    public string type { get; set; }
    public string tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string update_interval { get; set; }
}

public class HeadlessRule
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_keyword { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
}

public class RouteRule
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> inbound { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? ip_is_private { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string outbound { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string timeout { get; set; }
}

public class ExperimentalConfig
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public CacheFileConfig cache_file { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ClashApiConfig clash_api { get; set; }
}

public class CacheFileConfig
{
    public bool enabled { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string path { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string cache_id { get; set; }
}

public class ClashApiConfig
{
    public string external_controller { get; set; }
    public string external_ui { get; set; }
    public string secret { get; set; }
}

class Program
{
    private const string InputFile = "1.yaml";
    private const string MainProxyGroup = "🚀 PROXIES";

    static string GetContentHash(string content)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes)[..8].ToLower();
    }

    static void Main()
    {
        if (!TryLoadClashConfig(InputFile, out var clashConfig)) return;

        var yamlContent = File.ReadAllText(InputFile);
        string configHashId = GetContentHash(yamlContent); 

        // 【多态生成】：一次执行，吐出两大平台的极致专属优化配置
        GenerateForPlatform(clashConfig, configHashId, "windows");
        GenerateForPlatform(clashConfig, configHashId, "android");
    }

    static void GenerateForPlatform(ClashConfig clashConfig, string configHashId, string platform)
    {
        var sbConfig = CreateBaseSingboxConfig(configHashId);
        AddDefaultInbounds(sbConfig);
        AddDirectOutbound(sbConfig);

        var proxyServerDomains = new HashSet<string>();
        var allNodeNames = new List<string>();
        AddTrojanOutbounds(clashConfig, sbConfig, proxyServerDomains, allNodeNames);

        var (finalRegionGroupNames, regionOutbounds) = BuildRegionOutbounds(clashConfig);
        var mainGroupOptions = BuildMainGroupOptions(finalRegionGroupNames, allNodeNames);

        sbConfig.outbounds.Add(CreateMainProxyOutbound(mainGroupOptions, finalRegionGroupNames, allNodeNames));
        sbConfig.outbounds.AddRange(regionOutbounds);
        sbConfig.outbounds.AddRange(CreateServiceOutbounds(mainGroupOptions, finalRegionGroupNames));

        ConfigureDns(sbConfig, proxyServerDomains, MainProxyGroup);
        
        // 传递平台标识进行路由层原生接管
        ConfigureRoute(sbConfig, MainProxyGroup, platform);

        string outputFile = $"config_{platform}.json";
        File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, CreateJsonOptions()));
        Console.WriteLine($"[SUCCESS] Built {platform.ToUpper()} preset -> {outputFile}");
    }

    static bool TryLoadClashConfig(string inputFile, out ClashConfig clashConfig)
    {
        clashConfig = null!;
        if (!File.Exists(inputFile)) return false;

        var yamlContent = File.ReadAllText(inputFile);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        try
        {
            clashConfig = deserializer.Deserialize<ClashConfig>(yamlContent);
            return clashConfig != null;
        }
        catch
        {
            return false;
        }
    }

    static SingboxConfig CreateBaseSingboxConfig(string configHashId) => new()
    {
        experimental = new ExperimentalConfig
        {
            cache_file = new CacheFileConfig
            {
                enabled = true,
                path = "cache.db",
                cache_id = configHashId
            },
            clash_api = new ClashApiConfig
            {
                external_controller = "127.0.0.1:9999",
                external_ui = "ui",
                secret = "127001"
            }
        }
    };

    static void AddDefaultInbounds(SingboxConfig sbConfig)
    {
        sbConfig.inbounds.Add(new Inbound
        {
            type = "tun",
            tag = "tun-in",
            address = ["172.19.0.1/30", "fd00::1/126"],
            auto_route = true,
            strict_route = true,
            stack = "system",
            mtu = 1350
        });

        sbConfig.inbounds.Add(new Inbound
        {
            type = "mixed",
            tag = "mixed-in",
            listen = "127.0.0.1",
            listen_port = 8848
        });
    }

    static void AddDirectOutbound(SingboxConfig sbConfig)
    {
        sbConfig.outbounds.Add(new Outbound
        {
            type = "direct",
            tag = "DIRECT",
            domain_resolver = "local"
        });
    }

    static void AddTrojanOutbounds(
        ClashConfig clashConfig,
        SingboxConfig sbConfig,
        HashSet<string> proxyServerDomains,
        List<string> allNodeNames)
    {
        if (clashConfig.Proxies == null) return;

        foreach (var p in clashConfig.Proxies)
        {
            if (p.TryGetValue("type", out var typeObj) && typeObj.ToString() == "trojan")
            {
                string name = p["name"].ToString()!;
                string server = p["server"].ToString()!;
                allNodeNames.Add(name);

                if (!IPAddress.TryParse(server, out _)) proxyServerDomains.Add(server);

                sbConfig.outbounds.Add(new Outbound
                {
                    type = "trojan",
                    tag = name,
                    server = server,
                    server_port = int.Parse(p["port"].ToString()!),
                    password = p["password"].ToString(),

                    domain_resolver = "node-resolver",

                    tcp_keep_alive = "5m",
                    tcp_keep_alive_interval = "75s",
                    tcp_fast_open = true,
                    connect_timeout = "5s",

                    tls = new OutboundTls
                    {
                        enabled = true,
                        server_name = p.TryGetValue("sni", out var sniObj) ? sniObj.ToString() : server,
                        insecure = p.TryGetValue("skip-cert-verify", out var skipCert) && bool.TryParse(skipCert.ToString(), out var b) && b ? true : null,
                        utls = new Utls { enabled = true, fingerprint = "chrome" },
                        alpn = ["h2", "http/1.1"],
                        min_version = "1.3"
                    }
                });
            }
        }
    }

    static (List<string> finalRegionGroupNames, List<Outbound> regionOutbounds) BuildRegionOutbounds(ClashConfig clashConfig)
    {
        var finalRegionGroupNames = new List<string>();
        var regionOutbounds = new List<Outbound>();

        var regionMappings = new Dictionary<string, string[]>
        {
            { "🇭🇰", ["hk", "香港", "hongkong", "🇭🇰"] },
            { "🇸🇬", ["sg", "狮城", "新加坡", "singapore", "🇸🇬"] },
            { "🇯🇵", ["jp", "日本", "japan", "tokyo", "🇯🇵"] },
            { "🇺🇸", ["us", "美国", "america", "usa", "🇺🇸"] },
            { "🇹🇼", ["tw", "台湾", "taiwan", "taipei", "🇹🇼"] }
        };

        if (clashConfig.ProxyGroups == null) return (finalRegionGroupNames, regionOutbounds);

        foreach (var g in clashConfig.ProxyGroups)
        {
            string lowerName = g.Name.ToLower();
            var matchedEmoji = regionMappings.FirstOrDefault(kvp => kvp.Value.Any(lowerName.Contains)).Key;

            if (matchedEmoji != null)
            {
                string formattedGroupName = g.Name.Contains(matchedEmoji) ? g.Name : $"{matchedEmoji} {g.Name}";
                finalRegionGroupNames.Add(formattedGroupName);

                string outType = g.Type == "select" ? "selector" : "urltest";
                var mappedOutbounds = g.Proxies.Where(p => p != "REJECT").ToList();

                regionOutbounds.Add(new Outbound
                {
                    type = outType,
                    tag = formattedGroupName,
                    outbounds = mappedOutbounds,
                    @default = outType == "selector" ? mappedOutbounds.FirstOrDefault() : null,
                    
                    url = outType == "urltest" ? "https://www.gstatic.com/generate_204" : null,
                    interval = outType == "urltest" ? "10m" : null, 
                    tolerance = outType == "urltest" ? 50 : null,
                    idle_timeout = outType == "urltest" ? "30m" : null,
                    
                    interrupt_exist_connections = outType == "selector" ? true : null
                });
            }
        }

        return (finalRegionGroupNames, regionOutbounds);
    }

    static List<string> BuildMainGroupOptions(List<string> finalRegionGroupNames, List<string> allNodeNames)
    {
        var mainGroupOptions = new List<string>(finalRegionGroupNames);
        mainGroupOptions.Add("DIRECT");
        mainGroupOptions.AddRange(allNodeNames);
        return mainGroupOptions;
    }

    static Outbound CreateMainProxyOutbound(List<string> mainGroupOptions, List<string> finalRegionGroupNames, List<string> allNodeNames) => new()
    {
        type = "selector",
        tag = MainProxyGroup,
        outbounds = mainGroupOptions,
        @default = finalRegionGroupNames.FirstOrDefault() ?? allNodeNames.FirstOrDefault(),
        interrupt_exist_connections = true
    };

    static List<Outbound> CreateServiceOutbounds(List<string> mainGroupOptions, List<string> finalRegionGroupNames)
    {
        var customGroupOptions = new List<string> { MainProxyGroup };
        customGroupOptions.AddRange(mainGroupOptions);

        string usGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? MainProxyGroup;
        string sgGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇸🇬")) ?? MainProxyGroup;

        var specialGroups = new Dictionary<string, string>
        {
            { "🎬 YouTube", MainProxyGroup },
            { "🎵 Spotify", MainProxyGroup },
            { "🎮 Steam", MainProxyGroup },
            { "🤖 AI", usGroup },
            { "🪟 Microsoft", MainProxyGroup },
            { "✈️ Telegram", sgGroup },
            { "📚 Wikipedia", MainProxyGroup }
        };

        return [.. specialGroups.Select(group => new Outbound
        {
            type = "selector",
            tag = group.Key,
            outbounds = customGroupOptions,
            @default = group.Value,
            interrupt_exist_connections = true
        })];
    }

    static void ConfigureDns(SingboxConfig sbConfig, HashSet<string> proxyServerDomains, string mainProxyGroup)
    {
        sbConfig.dns.strategy = "ipv4_only";
        sbConfig.dns.reverse_mapping = true;

        sbConfig.dns.servers.Add(new DnsServer 
        { 
            tag = "bootstrap", 
            type = "local", 
        });  

        sbConfig.dns.servers.Add(new DnsServer
        {
            tag = "node-resolver",
            type = "https",
            server = "dns.alidns.com",
            detour = "DIRECT",
            domain_resolver = "bootstrap",
        });
        
        sbConfig.dns.servers.Add(new DnsServer
        {
            tag = "remote",
            type = "https",
            server = "cloudflare-dns.com",   
            detour = mainProxyGroup,
            domain_resolver = "bootstrap",

        });
        
        sbConfig.dns.servers.Add(new DnsServer
        {
            tag = "local",
            type = "https",
            server = "dns.alidns.com",
            detour = "DIRECT",
            domain_resolver = "bootstrap",
        });

        sbConfig.dns.final = "remote";

                sbConfig.dns.rules.Add(new DnsRule
        {
            query_type = ["AAAA"],
            action = "reject"
        });

        sbConfig.dns.rules.Add(new DnsRule
        {
            query_type = ["HTTPS", "SVCB"],
            action = "predefined",
            rcode = "NOERROR"
        });

        if (proxyServerDomains.Count > 0)
        {
            sbConfig.dns.rules.Add(new DnsRule
            {
                domain = [.. proxyServerDomains],
                action = "route",
                server = "node-resolver"
            });
        }

        sbConfig.dns.rules.Add(new DnsRule
        {
            domain_suffix = ["msftconnecttest.com", "msftncsi.com", "wns.windows.com"],
            action = "route",
            server = "local"
        });
        
        sbConfig.dns.rules.Add(new DnsRule
        {
            rule_set = ["geosite-cn", "geosite-category-pt"],
            action = "route",
            server = "local"
        });
    }

    static void ConfigureRoute(SingboxConfig sbConfig, string mainProxyGroup, string platform)
    {
        sbConfig.route.final = mainProxyGroup;
        
        // 【审查警告：已强行删除默认解析器兜底】1.14 彻底移除 default_domain_resolver，这里保持最干净状态。

        // 【平台专属优化拦截】
        if (platform == "windows")
        {
            sbConfig.route.auto_detect_interface = true;
        }
        else if (platform == "android")
        {
            sbConfig.route.auto_detect_interface = null;
        }

        sbConfig.route.rule_set =
        [
            CreateRemoteRuleSet("geosite-category-ads-all", "geosite", "geosite-category-ads-all"),
            CreateRemoteRuleSet("geosite-category-pt", "geosite", "geosite-category-pt"),
            CreateRemoteRuleSet("geosite-cn", "geosite", "geosite-cn"),
            CreateRemoteRuleSet("geoip-cn", "geoip", "geoip-cn"),
            CreateRemoteRuleSet("geosite-youtube", "geosite", "geosite-youtube"),
            CreateRemoteRuleSet("geosite-spotify", "geosite", "geosite-spotify"),
            CreateRemoteRuleSet("geosite-steam", "geosite", "geosite-steam"),
            CreateRemoteRuleSet("geosite-category-ai-!cn", "geosite", "geosite-category-ai-!cn"),
            CreateRemoteRuleSet("geosite-microsoft", "geosite", "geosite-microsoft"),
            CreateRemoteRuleSet("geosite-telegram", "geosite", "geosite-telegram"),
            CreateRemoteRuleSet("geoip-telegram", "geoip", "geoip-tg"),
            CreateRemoteRuleSet("geosite-wikimedia", "geosite", "geosite-wikimedia")
        ];

        sbConfig.route.rules =
        [
            new RouteRule { ip_cidr = ["::/0"], action = "reject" },
            new RouteRule { inbound = ["tun-in", "mixed-in"], port = [53], action = "hijack-dns" },
            new RouteRule { network = ["icmp"], action = "route", outbound = "DIRECT" },
            new RouteRule { ip_is_private = true, action = "route", outbound = "DIRECT" },
            new RouteRule { inbound = ["tun-in", "mixed-in"], action = "sniff", timeout = "100ms" },
            new RouteRule { protocol = ["ssh"], action = "route", outbound = "DIRECT" },
            new RouteRule { port = [3478, 3479, 19302, 19303], network = ["udp"], action = "reject" },
            new RouteRule { rule_set = ["geosite-category-ads-all"], action = "reject" },

            new RouteRule { rule_set = ["geosite-youtube"], action = "route", outbound = "🎬 YouTube" },
            new RouteRule { rule_set = ["geosite-spotify"], action = "route", outbound = "🎵 Spotify" },
            new RouteRule { rule_set = ["geosite-steam"], action = "route", outbound = "🎮 Steam" },
            new RouteRule { rule_set = ["geosite-category-ai-!cn"], action = "route", outbound = "🤖 AI" },
            new RouteRule { rule_set = ["geosite-microsoft"], action = "route", outbound = "🪟 Microsoft" },
            new RouteRule { rule_set = ["geosite-telegram"], action = "route", outbound = "✈️ Telegram" },
            new RouteRule { rule_set = ["geosite-wikimedia"], action = "route", outbound = "📚 Wikipedia" },
            new RouteRule { rule_set = ["geosite-cn", "geosite-category-pt"], action = "route", outbound = "DIRECT" },

            new RouteRule { inbound = ["mixed-in"], action = "resolve" },

            new RouteRule { inbound = ["tun-in", "mixed-in"], port = [443], network = ["udp"], action = "reject" },
            new RouteRule { ip_cidr = ["223.5.5.5/32", "1.1.1.1/32"], action = "route", outbound = "DIRECT" },
            new RouteRule { rule_set = ["geoip-cn"], action = "route", outbound = "DIRECT" },
            new RouteRule { rule_set = ["geoip-telegram"], action = "route", outbound = "✈️ Telegram" }
        ];
    }

    static JsonSerializerOptions CreateJsonOptions() => new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName) => new()
    {
        tag = tag,
        type = "remote",
        format = "binary",
        url = $"https://fastly.jsdelivr.net/gh/SagerNet/sing-{repoType}@rule-set/{fileName}.srs",
        download_detour = "DIRECT",
        update_interval = "1d"
    };
}