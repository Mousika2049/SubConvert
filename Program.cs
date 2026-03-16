using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class ClashProxyGroup { [YamlMember(Alias = "name")] public string Name { get; set; } [YamlMember(Alias = "type")] public string Type { get; set; } [YamlMember(Alias = "proxies")] public List<string> Proxies { get; set; } }

    public class SingboxConfig { public LogConfig log { get; set; } = new LogConfig(); public DnsConfig dns { get; set; } = new DnsConfig(); public List<Inbound> inbounds { get; set; } = new List<Inbound>(); public List<Outbound> outbounds { get; set; } = new List<Outbound>(); public RouteConfig route { get; set; } = new RouteConfig(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ExperimentalConfig experimental { get; set; } }

    public class LogConfig { public string level { get; set; } = "info"; public bool timestamp { get; set; } = true; }

    public class DnsConfig { public List<DnsServer> servers { get; set; } = new List<DnsServer>(); public List<DnsRule> rules { get; set; } = new List<DnsRule>(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string strategy { get; set; } = "ipv4_only"; [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? independent_cache { get; set; } }
    public class DnsServer { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string type { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string detour { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string inet4_range { get; set; } }
    public class DnsRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbound { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> query_type { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? disable_cache { get; set; } }

    // 【优化】：补充了 sniff_override_destination 字段
    public class Inbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string listen { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? listen_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interface_name { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> address { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? strict_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string stack { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? sniff { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? sniff_override_destination { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? mtu { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? endpoint_independent_nat { get; set; } }

    public class Outbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string password { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbounds { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string @default { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public OutboundTls tls { get; set; } }
    public class OutboundTls { public bool enabled { get; set; } = true; public string server_name { get; set; } public bool insecure { get; set; } }
    public class RouteConfig { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = new List<SingboxRuleSet>(); public List<RouteRule> rules { get; set; } = new List<RouteRule>(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; } public bool auto_detect_interface { get; set; } = true; public string default_domain_resolver { get; set; } }
    public class SingboxRuleSet { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<HeadlessRule> rules { get; set; } }
    public class HeadlessRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_keyword { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> process_name { get; set; } }
    public class RouteRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string outbound { get; set; } }

    public class ExperimentalConfig { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public CacheFileConfig cache_file { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ClashApiConfig clash_api { get; set; } }
    public class CacheFileConfig { public bool enabled { get; set; } public string path { get; set; } }
    public class ClashApiConfig { public string external_controller { get; set; } public string external_ui { get; set; } public string secret { get; set; } }

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

            // 【优化】：加入 sniff_override_destination = true 强行拨乱反正
            sbConfig.inbounds.Add(new Inbound { type = "tun", tag = "tun-in", interface_name = "tun0", address = new List<string> { "172.19.0.1/30", "fd00::1/126" }, auto_route = true, strict_route = true, stack = "system", sniff = true, sniff_override_destination = true, endpoint_independent_nat = true });
            sbConfig.inbounds.Add(new Inbound { type = "mixed", tag = "mixed-in", listen = "127.0.0.1", listen_port = clashConfig.MixedPort, sniff = true });

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

                        sbConfig.outbounds.Add(new Outbound { type = "trojan", tag = name, server = server, server_port = int.Parse(p["port"].ToString()), password = p["password"].ToString(), tls = new OutboundTls { enabled = true, server_name = p.ContainsKey("sni") ? p["sni"].ToString() : server, insecure = p.ContainsKey("skip-cert-verify") && bool.Parse(p["skip-cert-verify"].ToString()) } });
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

                        regionOutbounds.Add(new Outbound { type = outType, tag = formattedGroupName, outbounds = mappedOutbounds, @default = outType == "selector" ? mappedOutbounds.FirstOrDefault() : null });
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
            foreach (var group in specialGroups) { serviceOutbounds.Add(new Outbound { type = "selector", tag = group.Key, outbounds = customGroupOptions, @default = group.Value }); }

            sbConfig.outbounds.Add(mainProxyOutbound);
            sbConfig.outbounds.AddRange(regionOutbounds);
            sbConfig.outbounds.AddRange(serviceOutbounds);

            sbConfig.dns.strategy = "ipv4_only";
            sbConfig.dns.independent_cache = true;

            sbConfig.dns.servers.Add(new DnsServer { tag = "remote", type = "tls", server = "1.1.1.1", detour = mainProxyGroup });
            sbConfig.dns.servers.Add(new DnsServer { tag = "local", type = "https", server = "1.12.12.12" });

            // 保留你基准代码中的 FakeIP
            sbConfig.dns.servers.Add(new DnsServer { tag = "fakeip", type = "fakeip", inet4_range = "198.18.0.0/15" });

            sbConfig.dns.final = "remote";

            if (proxyServerDomains.Count > 0) sbConfig.dns.rules.Add(new DnsRule { domain = proxyServerDomains.ToList(), server = "local" });

            sbConfig.dns.rules.Add(new DnsRule { rule_set = new List<string> { "geosite-cn", "geosite-category-pt" }, server = "local" });

            sbConfig.dns.rules.Add(new DnsRule { query_type = new List<string> { "A", "AAAA" }, server = "fakeip" });

            sbConfig.route.default_domain_resolver = "local";
            sbConfig.route.final = mainProxyGroup;

            sbConfig.route.rule_set = new List<SingboxRuleSet>
            {
                // 【优化】：加入局域网广播防黑洞地址，干掉 FATAL 报错噪音
                new SingboxRuleSet { tag = "geoip-private", type = "inline", rules = new List<HeadlessRule> { new HeadlessRule { ip_cidr = new List<string> { "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8", "224.0.0.0/4", "255.255.255.255/32", "ff00::/8" } } } },

                new SingboxRuleSet { tag = "airport-domains", type = "inline", rules = proxyServerDomains.Any() ? new List<HeadlessRule> { new HeadlessRule { domain = proxyServerDomains.ToList() } } : new List<HeadlessRule>() },

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
                // 【优化】：移除了 protocol = dns 这条模糊劫持，现在只精准劫持真正的网页 53 端口查询
                new RouteRule { port = new List<int> { 53 }, action = "hijack-dns" },

                new RouteRule { ip_cidr = new List<string> { "::/0" }, outbound = "BLOCK" },

                // ====================================================================
                // 【乾坤大挪移核心修复】：优先把国内域名、中国 IP、PT 站点放行。
                // 这样阿里云、淘宝即使发出了 UDP 443 请求，也会在这里直接走物理网卡，
                // 完美避开了下方对国外流量的 UDP 封杀规则！
                // ====================================================================
                new RouteRule { rule_set = new List<string> { "geosite-cn", "geoip-cn" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "geosite-category-pt" }, outbound = "DIRECT" },

                new RouteRule { port = new List<int> { 443 }, network = new List<string> { "udp" }, outbound = "BLOCK" },
                new RouteRule { ip_cidr = new List<string> { "1.12.12.12/32", "1.1.1.1/32" }, outbound = "DIRECT" },

                new RouteRule { rule_set = new List<string> { "geoip-private" }, outbound = "DIRECT" },
                new RouteRule { rule_set = new List<string> { "airport-domains" }, outbound = "DIRECT" },

                new RouteRule { rule_set = new List<string> { "geosite-category-ads-all" }, outbound = "BLOCK" },

                new RouteRule { rule_set = new List<string> { "geosite-youtube" }, outbound = "🎬 YouTube" },
                new RouteRule { rule_set = new List<string> { "geosite-spotify" }, outbound = "🎵 Spotify" },
                new RouteRule { rule_set = new List<string> { "geosite-steam" }, outbound = "🎮 Steam" },
                new RouteRule { rule_set = new List<string> { "geosite-epicgames" }, outbound = "🎮 Epic" },
                new RouteRule { rule_set = new List<string> { "geosite-openai" }, outbound = "🤖 OpenAI" },
                new RouteRule { rule_set = new List<string> { "geosite-microsoft" }, outbound = "🪟 Microsoft" },
                new RouteRule { rule_set = new List<string> { "geosite-telegram", "geoip-telegram" }, outbound = "✈️ Telegram" },
                new RouteRule { rule_set = new List<string> { "geosite-wikimedia" }, outbound = "📚 Wikipedia" }
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, jsonOptions));
            Console.WriteLine($"[SUCCESS] Saved to {outputFile}");
            Console.WriteLine($"[INFO] Base layout restored. Applied DOMESTIC FIRST routing priority and removed DNS protocol hijack noise.");
        }

        static SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName, string downloadDetour)
        {
            return new SingboxRuleSet { tag = tag, type = "remote", format = "binary", url = $"https://raw.githubusercontent.com/SagerNet/sing-{repoType}/rule-set/{fileName}.srs", download_detour = downloadDetour };
        }

        static bool IsIpAddress(string host) => System.Net.IPAddress.TryParse(host, out _);
    }
}