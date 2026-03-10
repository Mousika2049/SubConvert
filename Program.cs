using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LinkCubeConvert
{
    // ==================== 极简数据结构 ====================
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

    public class SingboxConfig { public LogConfig log { get; set; } = new LogConfig(); public DnsConfig dns { get; set; } = new DnsConfig(); public List<Inbound> inbounds { get; set; } = new List<Inbound>(); public List<Outbound> outbounds { get; set; } = new List<Outbound>(); public RouteConfig route { get; set; } = new RouteConfig(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ExperimentalConfig experimental { get; set; } }

    public class LogConfig { public string level { get; set; } = "info"; public bool timestamp { get; set; } = true; }
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
    public class Inbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string listen { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? listen_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interface_name { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> address { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? strict_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string stack { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? sniff { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? mtu { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? endpoint_independent_nat { get; set; } }
    public class Outbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string password { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbounds { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string @default { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public OutboundTls tls { get; set; } }
    public class OutboundTls { public bool enabled { get; set; } = true; public string server_name { get; set; } public bool insecure { get; set; } }
    public class RouteConfig { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = new List<SingboxRuleSet>(); public List<RouteRule> rules { get; set; } = new List<RouteRule>(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; } public bool auto_detect_interface { get; set; } = true; public string default_domain_resolver { get; set; } }
    public class SingboxRuleSet { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<HeadlessRule> rules { get; set; } }
    public class HeadlessRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_keyword { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> process_name { get; set; } }
    public class RouteRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string outbound { get; set; } }

    public class ExperimentalConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public CacheFileConfig cache_file { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ClashApiConfig clash_api { get; set; }
    }
    public class CacheFileConfig { public bool enabled { get; set; } public string path { get; set; } }
    public class ClashApiConfig { public string external_controller { get; set; } public string external_ui { get; set; } public string secret { get; set; } }

    // ==================== 主程序 ====================
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = "1.yaml";
            string outputFile = "config.json";

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: {inputFile} not found.");
                return;
            }

            var yamlContent = File.ReadAllText(inputFile);
            var deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
            ClashConfig clashConfig;
            try { clashConfig = deserializer.Deserialize<ClashConfig>(yamlContent); }
            catch (Exception ex) { Console.WriteLine($"Parsed YAML failed: {ex.Message}"); return; }

            var sbConfig = new SingboxConfig();

            sbConfig.experimental = new ExperimentalConfig
            {
                cache_file = new CacheFileConfig { enabled = true, path = "cache.db" },
                clash_api = new ClashApiConfig { external_controller = "127.0.0.1:9090", external_ui = "ui", secret = "" }
            };

            sbConfig.inbounds.Add(new Inbound { type = "tun", tag = "tun-in", interface_name = "tun0", address = new List<string> { "172.19.0.1/30" }, auto_route = true, strict_route = true, stack = "system", sniff = true, mtu = 1500, endpoint_independent_nat = true });
            sbConfig.inbounds.Add(new Inbound { type = "mixed", tag = "mixed-in", listen = "127.0.0.1", listen_port = clashConfig.MixedPort, sniff = true });

            sbConfig.outbounds.Add(new Outbound { type = "direct", tag = "DIRECT" });
            sbConfig.outbounds.Add(new Outbound { type = "block", tag = "BLOCK" });

            // 3. 提取机场节点
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
                            tls = new OutboundTls { enabled = true, server_name = p.ContainsKey("sni") ? p["sni"].ToString() : server, insecure = p.ContainsKey("skip-cert-verify") && bool.Parse(p["skip-cert-verify"].ToString()) }
                        });
                    }
                }
            }

            // ==========================================================
            // 4. 精确提取机场原生的【分地区】策略组，并注入 Emoji
            // ==========================================================
            List<string> finalRegionGroupNames = new List<string>();
            List<Outbound> regionOutbounds = new List<Outbound>();

            // 特征字典：Emoji -> 对应的关键字匹配库
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

                    foreach (var kvp in regionMappings)
                    {
                        if (kvp.Value.Any(kw => lowerName.Contains(kw)))
                        {
                            matchedEmoji = kvp.Key;
                            break;
                        }
                    }

                    if (matchedEmoji != null)
                    {
                        // 如果原组名不包含该 Emoji，则在前面强行拼接
                        string formattedGroupName = g.Name;
                        if (!formattedGroupName.Contains(matchedEmoji))
                        {
                            formattedGroupName = $"{matchedEmoji} {formattedGroupName}";
                        }

                        finalRegionGroupNames.Add(formattedGroupName);

                        string outType = g.Type == "select" ? "selector" : "urltest";
                        var mappedOutbounds = g.Proxies.Select(p => p == "REJECT" ? "BLOCK" : p).ToList();

                        regionOutbounds.Add(new Outbound
                        {
                            type = outType,
                            tag = formattedGroupName, // 使用注入Emoji后的全新名称
                            outbounds = mappedOutbounds,
                            @default = outType == "selector" ? mappedOutbounds.FirstOrDefault() : null
                        });
                    }
                }
            }

            // ==========================================================
            // 5. 准备所有策略组数据 (控制界面呈现顺序)
            // ==========================================================
            string mainProxyGroup = "🚀 PROXIES";

            List<string> mainGroupOptions = new List<string>();
            mainGroupOptions.AddRange(finalRegionGroupNames); // 使用带Emoji的名称列表
            mainGroupOptions.Add("DIRECT");
            mainGroupOptions.AddRange(allNodeNames);

            var mainProxyOutbound = new Outbound
            {
                type = "selector",
                tag = mainProxyGroup,
                outbounds = mainGroupOptions,
                @default = finalRegionGroupNames.FirstOrDefault() ?? allNodeNames.FirstOrDefault()
            };

            List<string> customGroupOptions = new List<string> { mainProxyGroup };
            customGroupOptions.AddRange(mainGroupOptions);

            string usGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? mainProxyGroup;
            string sgGroup = finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇸🇬")) ?? mainProxyGroup;

            var specialGroups = new Dictionary<string, string>
            {
                { "🎬 YouTube", mainProxyGroup },
                { "🎵 Spotify", mainProxyGroup },
                { "🎮 Steam", mainProxyGroup },
                { "🎮 Epic", mainProxyGroup },
                { "🤖 OpenAI", usGroup },
                { "🪟 Microsoft", mainProxyGroup },
                { "✈️ Telegram", sgGroup }
            };

            List<Outbound> serviceOutbounds = new List<Outbound>();
            foreach (var group in specialGroups)
            {
                serviceOutbounds.Add(new Outbound { type = "selector", tag = group.Key, outbounds = customGroupOptions, @default = group.Value });
            }

            // ==========================================================
            // [极其关键] 严格按照 UI 展示要求的顺序进行 Append
            // 顺序：🚀 PROXIES -> 地区分组 -> 业务分组
            // ==========================================================
            sbConfig.outbounds.Add(mainProxyOutbound);         // 第 1 顺位
            sbConfig.outbounds.AddRange(regionOutbounds);      // 第 2 顺位
            sbConfig.outbounds.AddRange(serviceOutbounds);     // 第 3 顺位

            // 6. DNS 策略
            sbConfig.dns.strategy = "ipv4_only";
            sbConfig.dns.independent_cache = true;

            // 采用 TLS (DoT) 加密查询，通过主代理组发出
            sbConfig.dns.servers.Add(new DnsServer { tag = "remote", type = "tls", server = "1.1.1.1", detour = mainProxyGroup });
            // 使用 HTTPS 直连腾讯 DNS
            sbConfig.dns.servers.Add(new DnsServer { tag = "local", type = "https", server = "1.12.12.12" });
            // FakeIP 模块
            sbConfig.dns.servers.Add(new DnsServer { tag = "fakeip", type = "fakeip", inet4_range = "198.18.0.0/15" });

            sbConfig.dns.final = "remote";

            if (proxyServerDomains.Count > 0)
                sbConfig.dns.rules.Add(new DnsRule { domain = proxyServerDomains.ToList(), server = "local" });

            sbConfig.dns.rules.Add(new DnsRule { rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, server = "local" });
            sbConfig.dns.rules.Add(new DnsRule { query_type = new List<string> { "A", "AAAA" }, server = "fakeip" });

            // 7. Route 路由策略
            sbConfig.route.default_domain_resolver = "local";
            sbConfig.route.final = mainProxyGroup;

            sbConfig.route.rule_set = new List<SingboxRuleSet>
            {
                new SingboxRuleSet { tag = "geoip-private", type = "inline", rules = new List<HeadlessRule> { new HeadlessRule { ip_cidr = new List<string> { "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8" } } } },

                new SingboxRuleSet {
                    tag = "airport-domains",
                    type = "inline",
                    rules = proxyServerDomains.Any()
                            ? new List<HeadlessRule> { new HeadlessRule { domain = proxyServerDomains.ToList() } }
                            : new List<HeadlessRule>()
                },

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
            };

            sbConfig.route.rules = new List<RouteRule>
            {
                new RouteRule { port = new List<int> { 53 }, action = "hijack-dns" },
                new RouteRule { protocol = new List<string> { "dns" }, action = "hijack-dns" },
                new RouteRule { port = new List<int> { 443 }, network = new List<string> { "udp" }, outbound = "BLOCK" },
                new RouteRule { ip_cidr = new List<string> { "1.12.12.12/32", "1.1.1.1/32" }, outbound = "DIRECT" },

                new RouteRule { rule_set = new List<string> { "geoip-private" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "airport-domains" }, outbound = "DIRECT" },

                new RouteRule { rule_set = new List<string> { "geosite-category-ads-all" }, outbound = "BLOCK" },
                new RouteRule { rule_set = new List<string> { "geosite-category-pt" }, outbound = "DIRECT" },

                new RouteRule { rule_set = new List<string> { "geosite-youtube" }, outbound = "🎬 YouTube" },
                new RouteRule { rule_set = new List<string> { "geosite-spotify" }, outbound = "🎵 Spotify" },
                new RouteRule { rule_set = new List<string> { "geosite-steam" }, outbound = "🎮 Steam" },
                new RouteRule { rule_set = new List<string> { "geosite-epicgames" }, outbound = "🎮 Epic" },
                new RouteRule { rule_set = new List<string> { "geosite-openai" }, outbound = "🤖 OpenAI" },
                new RouteRule { rule_set = new List<string> { "geosite-microsoft" }, outbound = "🪟 Microsoft" },
                new RouteRule { rule_set = new List<string> { "geosite-telegram", "geoip-telegram" }, outbound = "✈️ Telegram" },

                new RouteRule { rule_set = new List<string> { "geosite-cn", "geoip-cn" }, outbound = "DIRECT" }
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, jsonOptions));

            Console.WriteLine($"[SUCCESS] Conversion complete! Saved to {outputFile}");
            Console.WriteLine($"[INFO] UI Array sorted: Main Proxy -> Emoji Regions -> Business Groups.");
        }

        static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName, string downloadDetour)
        {
            return new SingboxRuleSet { tag = tag, type = "remote", format = "binary", url = $"https://raw.githubusercontent.com/SagerNet/sing-{repoType}/rule-set/{fileName}.srs", download_detour = downloadDetour };
        }

        static bool IsIpAddress(string host) => System.Net.IPAddress.TryParse(host, out _);
    }
}