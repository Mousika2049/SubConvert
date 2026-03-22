using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LinkCubeConvert
{
    public class ClashConfig
    {
        [YamlMember(Alias = "mixed-port")] public int MixedPort { get; set; }
        [YamlMember(Alias = "proxies")] public List<Dictionary<string, object>> Proxies { get; set; }
        [YamlMember(Alias = "proxy-groups")] public List<ClashProxyGroup> ProxyGroups { get; set; }
    }
    public class ClashProxyGroup
    {
        [YamlMember(Alias = "name")] public string Name { get; set; }
        [YamlMember(Alias = "type")] public string Type { get; set; }
        [YamlMember(Alias = "proxies")] public List<string> Proxies { get; set; }
    }

    public class SingboxConfig
    {
        public LogConfig log { get; set; } = new LogConfig();
        public DnsConfig dns { get; set; } = new DnsConfig();
        public List<Inbound> inbounds { get; set; } = new List<Inbound>();
        public List<Outbound> outbounds { get; set; } = new List<Outbound>();
        public RouteConfig route { get; set; } = new RouteConfig();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ExperimentalConfig experimental { get; set; }
    }

    public class LogConfig
    {
        public string level { get; set; } = "warn";
        public bool timestamp { get; set; } = true;
    }

    public class DnsConfig
    {
        public List<DnsServer> servers { get; set; } = new List<DnsServer>();
        public List<DnsRule> rules { get; set; } = new List<DnsRule>();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string strategy { get; set; } = "ipv4_only";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? independent_cache { get; set; }
    }
    public class DnsServer
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string detour { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string inet4_range { get; set; }
    }
    public class DnsRule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbound { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> query_type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? disable_cache { get; set; }
    }

    public class Inbound
    {
        public string type { get; set; }
        public string tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string listen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? listen_port { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interface_name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> address { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_route { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? strict_route { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string stack { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? mtu { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? endpoint_independent_nat { get; set; }
    }

    /*// 【新增】：多路复用并发控制数据结构
    public class OutboundMultiplex
    {
        public bool enabled { get; set; } = true;
        public string protocol { get; set; } = "h2mux"; // 使用更先进的 h2mux
        public int max_connections { get; set; } = 4;   // 核心！最多开 4 条车道防队头阻塞
        public int min_streams { get; set; } = 4;       // 每条车道至少跑 4 个请求才开新车道
        // 【核心新增】：开启随机垃圾数据填充，彻底粉碎 GFW 封包大小特征！
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? padding { get; set; }
    }
    */
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

        public string tcp_keep_alive { get; set; }
        public string tcp_keep_alive_interval { get; set; }
        public bool? tcp_fast_open { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public OutboundMultiplex multiplex { get; set; }

        // 【核心新增】：拨号超时控制，拒绝死等
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string connect_timeout { get; set; }

        // 【核心新增】：UrlTest 自动切换测速三件套
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interval { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? tolerance { get; set; }
    }

    public class Utls
    {
        public bool enabled { get; set; } = true;
        public string fingerprint { get; set; } = "chrome";
    }

    public class OutboundTls
    {
        public bool enabled { get; set; } = true;
        public string server_name { get; set; }
        public bool insecure { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Utls utls { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> alpn { get; set; }
    }

    public class RouteConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = new List<SingboxRuleSet>();
        public List<RouteRule> rules { get; set; } = new List<RouteRule>();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; }
        public bool auto_detect_interface { get; set; } = true;
        public string default_domain_resolver { get; set; }
    }
    public class SingboxRuleSet
    {
        public string type { get; set; }
        public string tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<HeadlessRule> rules { get; set; }
    }
    public class HeadlessRule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_keyword { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> process_name { get; set; }
    }
    public class RouteRule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string outbound { get; set; }

        // 【核心新增】：匹配 1.13.x 全新 sniff 动作的超时控制字段
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string timeout { get; set; }

        // 【核心新增】：解析动作的寻址策略字段
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string strategy { get; set; }
    }


    public class ExperimentalConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public CacheFileConfig cache_file { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ClashApiConfig clash_api { get; set; }
    }
    public class CacheFileConfig
    {
        public bool enabled { get; set; }
        public string path { get; set; }
    }
    public class ClashApiConfig
    {
        public string external_controller { get; set; }
        public string external_ui { get; set; }
        public string secret { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = "1.yaml";
            string outputFile = "config.json";

            if (!File.Exists(inputFile)) return;

            var yamlContent = File.ReadAllText(inputFile);
            var deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
            ClashConfig clashConfig;
            try { clashConfig = deserializer.Deserialize<ClashConfig>(yamlContent); } catch { return; }

            var sbConfig = new SingboxConfig();

            sbConfig.experimental = new ExperimentalConfig { cache_file = new CacheFileConfig { enabled = true, path = "cache.db" }, clash_api = new ClashApiConfig { external_controller = "127.0.0.1:9999", external_ui = "ui", secret = "314159" } };

            // 【完全净化的 Inbound】：拔除所有废弃的 sniff 字段，仅保留 MTU 和内核必要的物理参数
            sbConfig.inbounds.Add(new Inbound 
            { 
                type = "tun", 
                tag = "tun-in", 
                interface_name = "tun0", 
                address = new List<string> { "172.19.0.1/30", "fd00::1/126" }, 
                auto_route = true, 
                strict_route = true, 
                stack = "system", 
                endpoint_independent_nat = true, 
                mtu = 1350 
            });
            sbConfig.inbounds.Add(new Inbound { type = "mixed", tag = "mixed-in", listen = "127.0.0.1", listen_port = clashConfig.MixedPort });

            sbConfig.outbounds.Add(new Outbound { type = "direct", tag = "DIRECT" });
            sbConfig.outbounds.Add(new Outbound { type = "block", tag = "BLOCK" });

            HashSet<string> proxyServerDomains = new HashSet<string>();
            List<string> allNodeNames = new List<string>();

            if (clashConfig.Proxies != null)
            {
                foreach (var p in clashConfig.Proxies)
                {
                    if (p.ContainsKey("type") && p["type"].ToString() == "trojan")
                    {
                        string name = p["name"].ToString();
                        string server = p["server"].ToString();
                        allNodeNames.Add(name);

                        if (!IsIpAddress(server)) proxyServerDomains.Add(server);

                        sbConfig.outbounds.Add(new Outbound
                        {
                            type = "trojan",
                            tag = name,
                            server = server,
                            server_port = int.Parse(p["port"].ToString()),
                            password = p["password"].ToString(),

                            tcp_keep_alive = "15s",
                            tcp_keep_alive_interval = "15s",
                            tcp_fast_open = true,

                            // 5秒连不上直接掐断
                            connect_timeout = "5s",

                            // 开启并发多路复用！既干掉 90% 的握手延迟，又通过 4 并发避开 5G 丢包时的队头阻塞
                            //multiplex = new OutboundMultiplex { enabled = true, protocol = "h2mux", max_connections = 4, min_streams = 4, padding = true },

                            tls = new OutboundTls
                            {
                                enabled = true,
                                server_name = p.ContainsKey("sni") ? p["sni"].ToString() : server,
                                insecure = p.ContainsKey("skip-cert-verify") && bool.Parse(p["skip-cert-verify"].ToString()),
                                utls = new Utls { enabled = true, fingerprint = "chrome" },
                                alpn = new List<string> { "h2", "http/1.1" }
                            }
                        });
                    }
                }
            }

            List<string> finalRegionGroupNames = new List<string>();
            List<Outbound> regionOutbounds = new List<Outbound>();

            var regionMappings = new Dictionary<string, string[]>
            {
                { "🇭🇰", new[] { "hk", "香港", "hongkong", "🇭🇰" } },
                { "🇸🇬", new[] { "sg", "狮城", "新加坡", "singapore", "🇸🇬" } },
                { "🇯🇵", new[] { "jp", "日本", "japan", "tokyo", "🇯🇵" } },
                { "🇺🇸", new[] { "us", "美国", "america", "usa", "🇺🇸" } },
                { "🇹🇼", new[] { "tw", "台湾", "taiwan", "taipei", "🇹🇼" } }
            };

            if (clashConfig.ProxyGroups != null)
            {
                foreach (var g in clashConfig.ProxyGroups)
                {
                    string lowerName = g.Name.ToLower();
                    string matchedEmoji = null;

                    foreach (var kvp in regionMappings) { if (kvp.Value.Any(kw => lowerName.Contains(kw))) { matchedEmoji = kvp.Key; break; } }

                    if (matchedEmoji != null)
                    {
                        string formattedGroupName = g.Name;
                        if (!formattedGroupName.Contains(matchedEmoji)) formattedGroupName = $"{matchedEmoji} {formattedGroupName}";

                        finalRegionGroupNames.Add(formattedGroupName);

                        string outType = g.Type == "select" ? "selector" : "urltest";
                        var mappedOutbounds = g.Proxies.Select(p => p == "REJECT" ? "BLOCK" : p).ToList();

                        // 【完美修复】：只有当类型是 urltest 时，才注入测速参数，唤醒后台容灾探针！
                        regionOutbounds.Add(new Outbound
                        {
                            type = outType,
                            tag = formattedGroupName,
                            outbounds = mappedOutbounds,
                            @default = outType == "selector" ? mappedOutbounds.FirstOrDefault() : null,
                            url = outType == "urltest" ? "https://www.gstatic.com/generate_204" : null,
                            interval = outType == "urltest" ? "3m" : null,
                            tolerance = outType == "urltest" ? 50 : null
                        });
                    }
                }
            }

            string mainProxyGroup = "🚀 PROXIES";
            List<string> mainGroupOptions = new List<string>();
            mainGroupOptions.AddRange(finalRegionGroupNames);
            mainGroupOptions.Add("DIRECT");
            mainGroupOptions.AddRange(allNodeNames);

            var mainProxyOutbound = new Outbound { type = "selector", tag = mainProxyGroup, outbounds = mainGroupOptions, @default = finalRegionGroupNames.FirstOrDefault() ?? allNodeNames.FirstOrDefault() };

            List<string> customGroupOptions = new List<string> { mainProxyGroup };
            customGroupOptions.AddRange(mainGroupOptions);

            string usGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? mainProxyGroup;
            string sgGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇸🇬")) ?? mainProxyGroup;

            var specialGroups = new Dictionary<string, string>
            {
                { "🎬 YouTube", mainProxyGroup }, { "🎵 Spotify", mainProxyGroup }, { "🎮 Steam", mainProxyGroup }, { "🎮 Epic", mainProxyGroup }, { "🤖 OpenAI", usGroup }, { "🪟 Microsoft", mainProxyGroup }, { "✈️ Telegram", sgGroup }, { "📚 Wikipedia", mainProxyGroup }
            };

            List<Outbound> serviceOutbounds = new List<Outbound>();
            foreach (var group in specialGroups) { serviceOutbounds.Add(new Outbound { type = "selector", tag = group.Key, outbounds = customGroupOptions, @default = group.Value }); }

            sbConfig.outbounds.Add(mainProxyOutbound);
            sbConfig.outbounds.AddRange(regionOutbounds);
            sbConfig.outbounds.AddRange(serviceOutbounds);

            sbConfig.dns.strategy = "ipv4_only";
            sbConfig.dns.independent_cache = true;

            sbConfig.dns.servers.Add(new DnsServer { tag = "remote", type = "https", server = "1.1.1.1", detour = mainProxyGroup });
            sbConfig.dns.servers.Add(new DnsServer { tag = "local", type = "https", server = "223.5.5.5" });

            sbConfig.dns.final = "remote";

            if (proxyServerDomains.Count > 0) sbConfig.dns.rules.Add(new DnsRule { domain = proxyServerDomains.ToList(), server = "local" });

            sbConfig.dns.rules.Add(new DnsRule { rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, server = "local" });

            sbConfig.route.default_domain_resolver = "local";
            sbConfig.route.final = mainProxyGroup;

            sbConfig.route.rule_set = new List<SingboxRuleSet>
            {
                new SingboxRuleSet { tag = "geoip-private", type = "inline", rules = new List<HeadlessRule> { new HeadlessRule { ip_cidr = new List<string> { "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8", "224.0.0.0/4", "255.255.255.255/32", "ff00::/8" } } } },
                CreateRemoteRuleSet("geosite-category-ads-all", "geosite", "geosite-category-ads-all", mainProxyGroup),
                CreateRemoteRuleSet("geosite-category-pt", "geosite", "geosite-category-pt", mainProxyGroup),
                CreateRemoteRuleSet("geosite-cn", "geosite", "geosite-cn", mainProxyGroup),
                CreateRemoteRuleSet("geoip-cn", "geoip", "geoip-cn", mainProxyGroup),
                CreateRemoteRuleSet("geosite-youtube", "geosite", "geosite-youtube", mainProxyGroup),
                CreateRemoteRuleSet("geosite-spotify", "geosite", "geosite-spotify", mainProxyGroup),
                CreateRemoteRuleSet("geosite-steam", "geosite", "geosite-steam", mainProxyGroup),
                CreateRemoteRuleSet("geosite-epicgames", "geosite", "geosite-epicgames", mainProxyGroup),
                CreateRemoteRuleSet("geosite-openai", "geosite", "geosite-openai", mainProxyGroup),
                CreateRemoteRuleSet("geosite-microsoft", "geosite", "geosite-microsoft", mainProxyGroup),
                CreateRemoteRuleSet("geosite-telegram", "geosite", "geosite-telegram", mainProxyGroup),
                CreateRemoteRuleSet("geoip-telegram", "geoip", "geoip-tg", mainProxyGroup),
                CreateRemoteRuleSet("geosite-wikimedia", "geosite", "geosite-wikimedia", mainProxyGroup),
            };

            sbConfig.route.rules = new List<RouteRule>
            {
                // 1. 全局拦截与系统规则 (最高优先级)
                new RouteRule { action = "sniff", timeout = "300ms" },
                new RouteRule { port = new List<int> { 53 }, action = "hijack-dns" },
                new RouteRule { ip_cidr = new List<string> { "::/0" }, action = "reject" },

                // 2. 纯域名匹配阶段 (Fast Path) - 0延迟，绝不触发额外 DNS 查询
                new RouteRule { rule_set = new List<string> { "geosite-category-ads-all" }, action = "reject" },
                new RouteRule { rule_set = new List<string> { "geosite-youtube" }, outbound = "🎬 YouTube" },
                new RouteRule { rule_set = new List<string> { "geosite-spotify" }, outbound = "🎵 Spotify" },
                new RouteRule { rule_set = new List<string> { "geosite-steam" }, outbound = "🎮 Steam" },
                new RouteRule { rule_set = new List<string> { "geosite-epicgames" }, outbound = "🎮 Epic" },
                new RouteRule { rule_set = new List<string> { "geosite-openai" }, outbound = "🤖 OpenAI" },
                new RouteRule { rule_set = new List<string> { "geosite-microsoft" }, outbound = "🪟 Microsoft" },
                new RouteRule { rule_set = new List<string> { "geosite-telegram" }, outbound = "✈️ Telegram" },
                new RouteRule { rule_set = new List<string> { "geosite-wikimedia" }, outbound = "📚 Wikipedia" },
                
                // 将国内外直连的域名规则提取到 resolve 之前
                new RouteRule { rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, outbound = "DIRECT" },

                // 基于端口的物理封杀 (不需要 DNS)
                new RouteRule { port = new List<int> { 443 }, network = new List<string> { "udp" }, action = "reject" },
                new RouteRule { ip_cidr = new List<string> { "223.5.5.5/32", "1.1.1.1/32" }, outbound = "DIRECT" },

                // 3. 【分水岭】：强制 DNS 解析！ 将剩下所有不认识的未知域名，强制转换为 IP，防止绕过。
                new RouteRule { action = "resolve", strategy = "ipv4_only" },

                // 4. 纯 IP 匹配阶段 (Slow Path) - 依赖分水岭解析出的真实 IP
                new RouteRule { rule_set = new List<string> { "geoip-private", "geoip-cn" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "geoip-telegram" }, outbound = "✈️ Telegram" }
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, jsonOptions));
            Console.WriteLine($"[SUCCESS] Saved to {outputFile}");
        }

        static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName, string downloadDetour)
        {
            return new SingboxRuleSet { tag = tag, type = "remote", format = "binary", url = $"https://raw.githubusercontent.com/SagerNet/sing-{repoType}/rule-set/{fileName}.srs", download_detour = downloadDetour };
        }

        static bool IsIpAddress(string host) => System.Net.IPAddress.TryParse(host, out _);
    }
}