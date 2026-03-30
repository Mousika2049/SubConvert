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
        public string strategy { get; set; } = "ipv4_only";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? independent_cache { get; set; }
    }

    public class DnsServer
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string detour { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string domain_resolver { get; set; }
    }

    public class DnsRule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> query_type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? disable_cache { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string rcode { get; set; }
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = new List<SingboxRuleSet>();
        public List<RouteRule> rules { get; set; } = new List<RouteRule>();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; }
        public bool auto_detect_interface { get; set; } = true;
    }
    public class SingboxRuleSet
    {
        public string type { get; set; }
        public string tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<HeadlessRule> rules { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string update_interval { get; set; }
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
        // 指定入口限制，防止网关级误杀
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> inbound { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
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
        public string path { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? store_rdrc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string rdrc_timeout { get; set; }
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

            sbConfig.experimental = new ExperimentalConfig
            {
                cache_file = new CacheFileConfig
                {
                    enabled = true,
                    path = "cache.db",
                    store_rdrc = true,
                    rdrc_timeout = "7d"
                },
                clash_api = new ClashApiConfig
                {
                    external_controller = "127.0.0.1:9999",
                    external_ui = "ui",
                    secret = ""
                }
            };

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
            sbConfig.inbounds.Add(new Inbound 
            { 
                type = "mixed", 
                tag = "mixed-in", 
                listen = "127.0.0.1", 
                listen_port = clashConfig.MixedPort 
            });

            // DIRECT 出站由最安全的 OS 底层 (bootstrap) 托底解析
            sbConfig.outbounds.Add(new Outbound 
            { 
                type = "direct", 
                tag = "DIRECT", 
                domain_resolver = "bootstrap" 
            });

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

                            // 代理节点域名坚决使用 local (223.5.5.5) 解析，防 ISP 污染！
                            domain_resolver = "local",

                            tcp_keep_alive = "15s",
                            tcp_keep_alive_interval = "15s",
                            tcp_fast_open = true,
                            connect_timeout = "5s",

                            tls = new OutboundTls
                            {
                                enabled = true,
                                server_name = p.ContainsKey("sni") ? p["sni"].ToString() : server,
                                insecure = (p.ContainsKey("skip-cert-verify") && bool.Parse(p["skip-cert-verify"].ToString())) ? true : (bool?)null,
                                utls = new Utls { enabled = true, fingerprint = "chrome" },
                                alpn = new List<string> { "h2", "http/1.1" },
                                min_version = "1.3"
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

                    foreach (var kvp in regionMappings) 
                    { 
                        if (kvp.Value.Any(kw => lowerName.Contains(kw))) 
                        { 
                            matchedEmoji = kvp.Key; break; 
                        } 
                    }

                    if (matchedEmoji != null)
                    {
                        string formattedGroupName = g.Name;
                        if (!formattedGroupName.Contains(matchedEmoji)) formattedGroupName = $"{matchedEmoji} {formattedGroupName}";

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
                            interval = outType == "urltest" ? "3m" : null,
                            tolerance = outType == "urltest" ? 50 : null,
                            idle_timeout = outType == "urltest" ? "30m" : null,
                            interrupt_exist_connections = true
                        });
                    }
                }
            }

            string mainProxyGroup = "🚀 PROXIES";
            List<string> mainGroupOptions = new List<string>();
            mainGroupOptions.AddRange(finalRegionGroupNames);
            mainGroupOptions.Add("DIRECT");
            mainGroupOptions.AddRange(allNodeNames);

            var mainProxyOutbound = new Outbound
            {
                type = "selector",
                tag = mainProxyGroup,
                outbounds = mainGroupOptions,
                @default = finalRegionGroupNames.FirstOrDefault() ?? allNodeNames.FirstOrDefault(),
                interrupt_exist_connections = true
            };

            string ruleUpdateGroup = "☁️ RULE-UPDATE";
            var ruleUpdateOutbound = new Outbound
            {
                type = "urltest",
                tag = ruleUpdateGroup,
                outbounds = allNodeNames,
                url = "https://www.gstatic.com/generate_204",
                interval = "3m",
                tolerance = 50,
                interrupt_exist_connections = true
            };

            sbConfig.outbounds.Add(ruleUpdateOutbound);

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
                { "✈️ Telegram", sgGroup },
                { "📚 Wikipedia", mainProxyGroup }
            };

            List<Outbound> serviceOutbounds = new List<Outbound>();
            foreach (var group in specialGroups)
            {
                serviceOutbounds.Add(new Outbound
                {
                    type = "selector",
                    tag = group.Key,
                    outbounds = customGroupOptions,
                    @default = group.Value,
                    interrupt_exist_connections = true
                });
            }

            sbConfig.outbounds.Add(mainProxyOutbound);
            sbConfig.outbounds.AddRange(regionOutbounds);
            sbConfig.outbounds.AddRange(serviceOutbounds);

            sbConfig.dns.strategy = "ipv4_only";
            sbConfig.dns.independent_cache = true;

            // 构建严格的 DAG（有向无环图）彻底消灭依赖死锁！
            // 1. 建立基于 OS 底层的 bootstrap 锚点
            sbConfig.dns.servers.Add(new DnsServer { tag = "bootstrap", type = "local" });
            // 2. DNS 服务器内部发包请求，统统交给最底层的 bootstrap 锚点去处理
            sbConfig.dns.servers.Add(new DnsServer 
            { 
                tag = "remote", 
                type = "https", 
                server = "1.1.1.1", 
                detour = mainProxyGroup, 
                domain_resolver = "bootstrap" 
            });
            sbConfig.dns.servers.Add(new DnsServer 
            { 
                tag = "local", 
                type = "https", 
                server = "223.5.5.5", 
                detour = "DIRECT", 
                domain_resolver = "bootstrap" 
            });

            sbConfig.dns.final = "remote";

            if (proxyServerDomains.Count > 0)
            {
                sbConfig.dns.rules.Add(new DnsRule
                {
                    domain = proxyServerDomains.ToList(),
                    server = "local"
                });
            }

            sbConfig.dns.rules.Add(new DnsRule
            {
                query_type = new List<string> { "HTTPS", "SVCB" },
                action = "predefined",
                rcode = "NOERROR"
            });

            sbConfig.dns.rules.Add(new DnsRule 
            { 
                domain_suffix = new List<string> { "msftconnecttest.com", "msftncsi.com", "wns.windows.com" }, 
                server = "local" 
            });
            sbConfig.dns.rules.Add(new DnsRule 
            { 
                rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, 
                server = "local" 
            });

            sbConfig.route.final = mainProxyGroup;

            sbConfig.route.rule_set = new List<SingboxRuleSet>
            {
                new SingboxRuleSet
                {
                    tag = "geoip-private",
                    type = "inline",
                    rules = new List<HeadlessRule> { new HeadlessRule { ip_cidr = new List<string>
                    {
                        "10.0.0.0/8",
                        "172.16.0.0/12",
                        "192.168.0.0/16",
                        "127.0.0.0/8",
                        "fd00::/8",
                        "224.0.0.0/4",
                        "255.255.255.255/32",
                        "ff00::/8"
                    } } }
                },
                CreateRemoteRuleSet("geosite-category-ads-all", "geosite", "geosite-category-ads-all", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-category-pt", "geosite", "geosite-category-pt", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-cn", "geosite", "geosite-cn", ruleUpdateGroup),
                CreateRemoteRuleSet("geoip-cn", "geoip", "geoip-cn", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-youtube", "geosite", "geosite-youtube", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-spotify", "geosite", "geosite-spotify", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-steam", "geosite", "geosite-steam", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-epicgames", "geosite", "geosite-epicgames", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-openai", "geosite", "geosite-openai", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-microsoft", "geosite", "geosite-microsoft", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-telegram", "geosite", "geosite-telegram", ruleUpdateGroup),
                CreateRemoteRuleSet("geoip-telegram", "geoip", "geoip-tg", ruleUpdateGroup),
                CreateRemoteRuleSet("geosite-wikimedia", "geosite", "geosite-wikimedia", ruleUpdateGroup),
            };

            sbConfig.route.rules = new List<RouteRule>
            {
                // 【优先级 0：L3/L4 物理层绝对阻断】(最快速度，不进嗅探)
                // 1. 瞬间秒杀 IPv6，杜绝 300ms 空转等待
                new RouteRule { ip_cidr = new List<string> { "::/0" }, action = "reject" },

                // 【优先级 1：受控的网关级劫持】(只作用于代理入口)
                // 2. 劫持 DNS，并严格限制入口，防止误杀旁路由宿主机流量
                new RouteRule { inbound = new List<string> { "tun-in", "mixed-in" }, port = new List<int> { 53 }, action = "hijack-dns" },

                // 【优先级 2：局域网物理直连】(防路由死锁)
                // 3. 局域网 IP 必须在嗅探前放行
                new RouteRule { rule_set = new List<string> { "geoip-private" }, outbound = "DIRECT" },

                // 【优先级 3：L7 协议嗅探器】(开始进行耗时操作)
                // 4. 只对代理入口流量开启嗅探
                new RouteRule { inbound = new List<string> { "tun-in", "mixed-in" }, action = "sniff", timeout = "300ms" },

                // 【优先级 4：基于协议的安全直连】(基于真实协议特征)
                // 5. 严格验证协议特征，只有真实 SSH 协议才能直连，废除盲目的 22 端口放行！
                new RouteRule { protocol = new List<string> { "ssh" }, outbound = "DIRECT" },

                // 【优先级 5：WebRTC 与 Fast Path (域名分流)】
                new RouteRule { port = new List<int> { 3478, 3479, 19302, 19303 }, network = new List<string> { "udp" }, action = "reject" },
                new RouteRule { rule_set = new List<string> { "geosite-category-ads-all" }, action = "reject" },

                new RouteRule { rule_set = new List<string> { "geosite-youtube" }, outbound = "🎬 YouTube" },
                new RouteRule { rule_set = new List<string> { "geosite-spotify" }, outbound = "🎵 Spotify" },
                new RouteRule { rule_set = new List<string> { "geosite-steam" }, outbound = "🎮 Steam" },
                new RouteRule { rule_set = new List<string> { "geosite-epicgames" }, outbound = "🎮 Epic" },
                new RouteRule { rule_set = new List<string> { "geosite-openai" }, outbound = "🤖 OpenAI" },
                new RouteRule { rule_set = new List<string> { "geosite-microsoft" }, outbound = "🪟 Microsoft" },
                new RouteRule { rule_set = new List<string> { "geosite-telegram" }, outbound = "✈️ Telegram" },
                new RouteRule { rule_set = new List<string> { "geosite-wikimedia" }, outbound = "📚 Wikipedia" },

                new RouteRule { rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, outbound = "DIRECT" },

                // 【优先级 6：Slow Path (纯 IP 匹配)】
                new RouteRule { port = new List<int> { 443 }, network = new List<string> { "udp" }, action = "reject" },
                new RouteRule { ip_cidr = new List<string> { "223.5.5.5/32", "1.1.1.1/32" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "geoip-cn" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "geoip-telegram" }, outbound = "✈️ Telegram" }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, jsonOptions));
            Console.WriteLine($"[SUCCESS] Saved to {outputFile}");
        }

        static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName, string downloadDetour)
        {
            return new SingboxRuleSet
            {
                tag = tag,
                type = "remote",
                format = "binary",
                url = $"https://raw.githubusercontent.com/SagerNet/sing-{repoType}/rule-set/{fileName}.srs",
                download_detour = downloadDetour,
                update_interval = "1d"
            };
        }

        static bool IsIpAddress(string host) => System.Net.IPAddress.TryParse(host, out _);
    }
}