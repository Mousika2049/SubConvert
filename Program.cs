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
        [YamlMember(Alias = "rules")] public List<string> Rules { get; set; }
    }
    public class ClashProxyGroup { [YamlMember(Alias = "name")] public string Name { get; set; } [YamlMember(Alias = "type")] public string Type { get; set; } [YamlMember(Alias = "proxies")] public List<string> Proxies { get; set; } }
    public class SingboxConfig { public LogConfig log { get; set; } = new LogConfig(); public DnsConfig dns { get; set; } = new DnsConfig(); public List<Inbound> inbounds { get; set; } = new List<Inbound>(); public List<Outbound> outbounds { get; set; } = new List<Outbound>(); public RouteConfig route { get; set; } = new RouteConfig(); }
    public class LogConfig { public string level { get; set; } = "info"; public bool timestamp { get; set; } = true; }
    public class DnsConfig { public List<DnsServer> servers { get; set; } = new List<DnsServer>(); public List<DnsRule> rules { get; set; } = new List<DnsRule>(); public string final { get; set; } public string strategy { get; set; } = "ipv4_only"; }
    public class DnsServer { public string tag { get; set; } public string type { get; set; } public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string detour { get; set; } }
    public class DnsRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbound { get; set; } public string server { get; set; } public bool? disable_cache { get; set; } }
    public class Inbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string listen { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? listen_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string interface_name { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> address { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? auto_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? strict_route { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string stack { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? sniff { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? mtu { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? endpoint_independent_nat { get; set; } }
    public class Outbound { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string server { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? server_port { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string password { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> outbounds { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string @default { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public OutboundTls tls { get; set; } }
    public class OutboundTls { public bool enabled { get; set; } = true; public string server_name { get; set; } public bool insecure { get; set; } }
    public class RouteConfig { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<SingboxRuleSet> rule_set { get; set; } = new List<SingboxRuleSet>(); public List<RouteRule> rules { get; set; } = new List<RouteRule>(); [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string final { get; set; } public bool auto_detect_interface { get; set; } = true; public string default_domain_resolver { get; set; } }
    public class SingboxRuleSet { public string type { get; set; } public string tag { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string format { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string url { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string download_detour { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<HeadlessRule> rules { get; set; } }
    public class HeadlessRule { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_suffix { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> domain_keyword { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; } [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> process_name { get; set; } }

    public class RouteRule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> protocol { get; set; }

        // 【新增】：用于端口精准拦截
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<int> port { get; set; }
        // 【新增】：用于指定网络协议 (tcp/udp)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> network { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string action { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> rule_set { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<string> ip_cidr { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string outbound { get; set; }
    }

    public class RuleCollector { public List<string> Domain { get; set; } = new List<string>(); public List<string> DomainSuffix { get; set; } = new List<string>(); public List<string> DomainKeyword { get; set; } = new List<string>(); public List<string> IpCidr { get; set; } = new List<string>(); public List<string> ProcessName { get; set; } = new List<string>(); }

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

            // 1. Inbounds
            sbConfig.inbounds.Add(new Inbound
            {
                type = "tun",
                tag = "tun-in",
                interface_name = "tun0",
                address = new List<string> { "172.19.0.1/30" },
                auto_route = true,
                strict_route = true,
                stack = "system",
                sniff = true,
                mtu = 1500,
                endpoint_independent_nat = true
            });
            sbConfig.inbounds.Add(new Inbound { type = "mixed", tag = "mixed-in", listen = "127.0.0.1", listen_port = clashConfig.MixedPort, sniff = true });

            // 2. Outbounds
            sbConfig.outbounds.Add(new Outbound { type = "direct", tag = "DIRECT" });
            sbConfig.outbounds.Add(new Outbound { type = "block", tag = "BLOCK" });

            HashSet<string> proxyServerDomains = new HashSet<string>();
            string mainProxyGroup = "Proxies";

            if (clashConfig.Proxies != null)
            {
                foreach (var p in clashConfig.Proxies)
                {
                    if (p.ContainsKey("type") && p["type"].ToString() == "trojan")
                    {
                        string server = p["server"].ToString();
                        if (!IsIpAddress(server)) proxyServerDomains.Add(server);
                        sbConfig.outbounds.Add(new Outbound
                        {
                            type = "trojan",
                            tag = p["name"].ToString(),
                            server = server,
                            server_port = int.Parse(p["port"].ToString()),
                            password = p["password"].ToString(),
                            tls = new OutboundTls { enabled = true, server_name = p.ContainsKey("sni") ? p["sni"].ToString() : server, insecure = p.ContainsKey("skip-cert-verify") && bool.Parse(p["skip-cert-verify"].ToString()) }
                        });
                    }
                }
            }

            if (clashConfig.ProxyGroups != null)
            {
                foreach (var g in clashConfig.ProxyGroups)
                {
                    if (g.Type == "select")
                    {
                        sbConfig.outbounds.Add(new Outbound { type = "selector", tag = g.Name, outbounds = g.Proxies, @default = g.Proxies.FirstOrDefault() });
                        if ((g.Name == "Proxies" || g.Name == "Proxy") && mainProxyGroup == "Proxies") mainProxyGroup = g.Name;
                    }
                }
            }

            // 3. DNS
            sbConfig.dns.strategy = "ipv4_only";
            sbConfig.dns.servers.Add(new DnsServer { tag = "google", type = "tcp", server = "8.8.8.8", detour = mainProxyGroup });
            sbConfig.dns.servers.Add(new DnsServer { tag = "local", type = "udp", server = "223.5.5.5", server_port = 53 });
            sbConfig.dns.final = "google";

            // 4. Route
            sbConfig.route.default_domain_resolver = "local";
            sbConfig.route.final = mainProxyGroup;

            // ================= 关键拦截逻辑修复 =================

            // 修复 1: 强制无脑拦截 53 端口，防止 UDP 嗅探延迟导致 DNS 泄漏到直连
            sbConfig.route.rules.Add(new RouteRule { port = new List<int> { 53 }, action = "hijack-dns" });
            sbConfig.route.rules.Add(new RouteRule { protocol = new List<string> { "dns" }, action = "hijack-dns" });

            // 修复 2: 屏蔽推特/油管滥用的 QUIC 协议 (UDP 443)，逼迫其回退到稳定的 TCP 加载图片
            sbConfig.route.rules.Add(new RouteRule
            {
                port = new List<int> { 443 },
                network = new List<string> { "udp" },
                outbound = "BLOCK"
            });
     

            // 基础直连防止死循环
            sbConfig.route.rules.Add(new RouteRule { ip_cidr = new List<string> { "223.5.5.5/32" }, outbound = "DIRECT" });
            // ====================================================

            var collectedRules = new Dictionary<string, RuleCollector>();
            HashSet<string> remoteGeoIpTags = new HashSet<string>();

            // 私有 IP 规则
            string privateIpTag = "geoip-private";
            sbConfig.route.rule_set.Add(new SingboxRuleSet
            {
                tag = privateIpTag,
                type = "inline",
                rules = new List<HeadlessRule>
                {
                    new HeadlessRule { ip_cidr = new List<string> { "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8" } }
                }
            });
            sbConfig.route.rules.Add(new RouteRule { rule_set = new List<string> { privateIpTag }, outbound = "DIRECT" });

            if (clashConfig.Rules != null)
            {
                foreach (var line in clashConfig.Rules)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length < 2) continue;

                    string type = parts[0].Trim().ToUpper();
                    if (type == "MATCH") continue;

                    if (parts.Length < 3) continue;
                    string value = parts[1].Trim();
                    string target = parts[2].Trim();

                    if (!collectedRules.ContainsKey(target)) collectedRules[target] = new RuleCollector();

                    if (type == "GEOIP")
                    {
                        string countryCode = value.ToLower();
                        string ruleSetTag = $"geoip-{countryCode}";
                        if (!remoteGeoIpTags.Contains(ruleSetTag))
                        {
                            sbConfig.route.rule_set.Add(new SingboxRuleSet
                            {
                                tag = ruleSetTag,
                                type = "remote",
                                format = "binary",
                                url = $"https://raw.githubusercontent.com/SagerNet/sing-geoip/rule-set/geoip-{countryCode}.srs",
                                download_detour = mainProxyGroup
                            });
                            remoteGeoIpTags.Add(ruleSetTag);
                        }
                        sbConfig.route.rules.Add(new RouteRule { rule_set = new List<string> { ruleSetTag }, outbound = target });
                        continue;
                    }

                    switch (type)
                    {
                        case "DOMAIN": collectedRules[target].Domain.Add(value); break;
                        case "DOMAIN-SUFFIX": collectedRules[target].DomainSuffix.Add(value); break;
                        case "DOMAIN-KEYWORD": collectedRules[target].DomainKeyword.Add(value); break;
                        case "IP-CIDR": case "IP-CIDR6": collectedRules[target].IpCidr.Add(value); break;
                        case "PROCESS-NAME": collectedRules[target].ProcessName.Add(value); break;
                    }
                }
            }

            foreach (var kvp in collectedRules)
            {
                string target = kvp.Key;
                RuleCollector collector = kvp.Value;
                collector.DomainSuffix = collector.DomainSuffix.Distinct().ToList();

                if (collector.Domain.Count == 0 && collector.DomainSuffix.Count == 0 &&
                    collector.DomainKeyword.Count == 0 && collector.IpCidr.Count == 0 &&
                    collector.ProcessName.Count == 0) continue;

                string cleanTarget = target.Replace(" ", "-").Replace(":", "");
                string ruleSetTag = $"rs-local-{cleanTarget}";

                var localRuleSet = new SingboxRuleSet
                {
                    type = "inline",
                    tag = ruleSetTag,
                    rules = new List<HeadlessRule>
                    {
                        new HeadlessRule
                        {
                            domain = collector.Domain.Any() ? collector.Domain : null,
                            domain_suffix = collector.DomainSuffix.Any() ? collector.DomainSuffix : null,
                            domain_keyword = collector.DomainKeyword.Any() ? collector.DomainKeyword : null,
                            ip_cidr = collector.IpCidr.Any() ? collector.IpCidr : null,
                            process_name = collector.ProcessName.Any() ? collector.ProcessName : null
                        }
                    }
                };
                sbConfig.route.rule_set.Add(localRuleSet);
                sbConfig.route.rules.Add(new RouteRule { rule_set = new List<string> { ruleSetTag }, outbound = target });
            }

            // 5. DNS Rules
            if (proxyServerDomains.Count > 0)
                sbConfig.dns.rules.Add(new DnsRule { domain = proxyServerDomains.ToList(), server = "local" });

            string directTag = $"rs-local-DIRECT";
            if (sbConfig.route.rule_set.Any(r => r.tag == directTag))
                sbConfig.dns.rules.Add(new DnsRule { rule_set = new List<string> { directTag }, server = "local" });

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(sbConfig, jsonOptions));

            Console.WriteLine($"Conversion complete! Saved to {outputFile}");
            Console.WriteLine($"Patches applied: Strict port 53 DNS Hijack & UDP 443 (QUIC) rejection.");
        }

        static bool IsIpAddress(string host) => System.Net.IPAddress.TryParse(host, out _);
    }
}