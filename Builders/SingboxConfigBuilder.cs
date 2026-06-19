using System.Net;
using System.Text.RegularExpressions;
using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;
using SubConvert.Converters;

namespace SubConvert.Builders;

public class SingboxConfigBuilder(TargetPlatform platform, IEnumerable<IProxyConverter> converters, AppSettings appSettings)
{
    // 1. 只有绝对不可变的依赖配置，彻底移除所有装配状态字段
    private readonly TargetPlatform _platform = platform;
    private readonly IEnumerable<IProxyConverter> _converters = converters;
    private readonly AppSettings _appSettings = appSettings;

    // 2. 引入内部类：将单次构建的所有状态封装在独立上下文中
    private class BuildContext
    {
        public LogConfig Log { get; } = new();
        public DnsConfig Dns { get; } = new();
        public List<Inbound> Inbounds { get; } = [];
        public RouteConfig Route { get; set; } = new(); // 在实例化时注入默认 Final

        public HashSet<string> ProxyServerDomains { get; } = [];
        public List<string> AllNodeNames { get; } = [];
        public List<string> FinalRegionGroupNames { get; } = [];
        // 记录逻辑 ID 到实际生成的显示名的映射
        public Dictionary<RegionId, string> GeneratedRegions { get; } = [];

        public Outbound? DirectOutbound { get; set; }
        public List<Outbound> NodeOutbounds { get; } = [];
        public List<Outbound> RegionOutbounds { get; } = [];
        public List<Outbound> MainOutbounds { get; } = [];
        public List<Outbound> ServiceOutbounds { get; } = [];
    }

    // ── 唯一暴露的公共入口 ──────────────────────────────────────────────────

    public SingboxConfig Build(ClashConfig clashConfig)
    {
        // 3. 每次构建都会开辟一块全新的独立沙箱上下文，绝不相互污染
        var ctx = new BuildContext
        {
            Route = new RouteConfig { Final = _appSettings.MainProxyGroup }
        };

        // 4. 将上下文像接力棒一样在流水线中传递
        BuildDefaultInbounds(ctx);
        BuildDirectOutbound(ctx);
        BuildProxyNodes(ctx, clashConfig); 
        BuildRegionOutbounds(ctx);
        BuildProxyGroups(ctx);
        BuildDns(ctx);                   
        BuildRouting(ctx);

        var orderedOutbounds = new List<Outbound>();
        orderedOutbounds.AddRange(ctx.MainOutbounds);
        orderedOutbounds.AddRange(ctx.RegionOutbounds);
        orderedOutbounds.AddRange(ctx.ServiceOutbounds);
        orderedOutbounds.AddRange(ctx.NodeOutbounds);
        if (ctx.DirectOutbound != null)
        {
            orderedOutbounds.Add(ctx.DirectOutbound);
        }

        return new SingboxConfig
        {
            Log = ctx.Log,
            Dns = ctx.Dns,
            Inbounds = ctx.Inbounds,
            Outbounds = orderedOutbounds,
            Route = ctx.Route,
        };
    }

    // ── 内部组装工序 (全部接收 BuildContext) ───────────────────────────────

    private void BuildDefaultInbounds(BuildContext ctx)
    {
        ctx.Inbounds.Add(new Inbound
        {
            Type = "tun",
            Tag = "tun-in",
            Address = ["172.19.0.1/30", "fd00::1/126"],
            AutoRoute = true,
            AutoRedirect = _platform == TargetPlatform.Linux ? true : null,
            StrictRoute = true,
            Stack = _platform switch
            {
                TargetPlatform.Windows => "mixed",
                TargetPlatform.Linux => "system",
                TargetPlatform.Android => "system",
                _ => null
            },
            Mtu = 1400
        });
        ctx.Inbounds.Add(new Inbound
        {
            Type = "mixed",
            Tag = "mixed-in",
            Listen = "127.0.0.1",
            ListenPort = 8848
        });
    }

    private void BuildDirectOutbound(BuildContext ctx)
    {
        ctx.DirectOutbound = new Outbound
        {
            Type = "direct",
            Tag = _appSettings.Direct,
            DomainResolver = "local"
        };
    }

    private void BuildProxyNodes(BuildContext ctx, ClashConfig clashConfig)
    {
        if (clashConfig.Proxies == null) return;

        foreach (var p in clashConfig.Proxies)
        {
            if (!p.TryGetValue("type", out var typeObj)) continue;
            string type = typeObj.ToString()!;

            var converter = _converters.FirstOrDefault(c => c.CanHandle(type));
            if (converter == null) continue;

            Outbound outbound = converter.Convert(p);
            if (outbound == null) continue;

            if (!string.IsNullOrEmpty(outbound.Tag))
                ctx.AllNodeNames.Add(outbound.Tag);
            
            if (!string.IsNullOrEmpty(outbound.Server) && !IPAddress.TryParse(outbound.Server, out _))
                ctx.ProxyServerDomains.Add(outbound.Server);

            ctx.NodeOutbounds.Add(outbound);
        }
    }

    private void BuildRegionOutbounds(BuildContext ctx)
    {
        foreach (var kvp in ProfileDefinitions.Regions)
        {
            RegionId regionId = kvp.Key;
            string groupName = kvp.Value.DisplayName;
            Regex pattern = kvp.Value.Pattern;

            var matchedNodes = ctx.AllNodeNames.Where(name => pattern.IsMatch(name)).ToList();

            if (matchedNodes.Count >= 2)
            {
                // 成功生成时，记录下这个 ID 对应的真实名称
                ctx.GeneratedRegions[regionId] = groupName;
                
                ctx.RegionOutbounds.Add(new Outbound
                {
                    Type = "selector",
                    Tag = groupName,
                    Outbounds = matchedNodes,
                    Default = matchedNodes.FirstOrDefault(),
                    InterruptExistConnections = true
                });
            }
        }
    }

    private void BuildProxyGroups(BuildContext ctx)
    {
        var mainGroupOptions = new List<string>(ctx.GeneratedRegions.Values);
        mainGroupOptions.AddRange(ctx.AllNodeNames);
        mainGroupOptions.Add(_appSettings.Direct);

        ctx.MainOutbounds.Add(new Outbound
        {
            Type = "selector",
            Tag = _appSettings.MainProxyGroup,
            Outbounds = mainGroupOptions,
            Default = ctx.GeneratedRegions.Values.FirstOrDefault() ?? ctx.AllNodeNames.FirstOrDefault() ?? _appSettings.Direct,
            InterruptExistConnections = true
        });

        var serviceGroupOptions = new List<string> { _appSettings.MainProxyGroup };
        serviceGroupOptions.AddRange(ctx.GeneratedRegions.Values);
        serviceGroupOptions.AddRange(ctx.AllNodeNames);
        serviceGroupOptions.Add(_appSettings.Direct);

        // 自动化生成服务出站组
        foreach (var service in ProfileDefinitions.Services)
        {
            string defaultSelection = _appSettings.MainProxyGroup;
            if (service.DefaultRegion.HasValue && ctx.GeneratedRegions.TryGetValue(service.DefaultRegion.Value, out string? generatedRegionName))
            {
                defaultSelection = generatedRegionName;
            }

            ctx.ServiceOutbounds.Add(new Outbound
            {
                Type = "selector",
                Tag = service.Name,
                Outbounds = serviceGroupOptions,
                Default = defaultSelection,
                InterruptExistConnections = true
            });
        }
    }

    private void BuildDns(BuildContext ctx)
    {
        ctx.Dns.Servers.AddRange([
            new DnsServer { Tag = "bootstrap", Type = "local" },
            new DnsServer { Tag = "node-resolver", Type = "https", Server = "223.5.5.5" },
            new DnsServer { Tag = "remote", Type = "https", Server = "1.1.1.1", Detour = _appSettings.MainProxyGroup },
            new DnsServer { Tag = "local", Type = "https", Server = "223.5.5.5" }
        ]);

        ctx.Dns.Rules.Add(new DnsRule { QueryType = ["AAAA"], Action = "predefined", Rcode = "NOERROR" });
        ctx.Dns.Rules.Add(new DnsRule { RuleSet = ["geosite-category-ads-all"], Action = "predefined", Rcode = "NOERROR" });

        if (ctx.ProxyServerDomains.Count > 0)
        {
            ctx.Dns.Rules.Add(new DnsRule { Domain = [.. ctx.ProxyServerDomains], Action = "route", Server = "node-resolver" });
        }

        ctx.Dns.Rules.Add(new DnsRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Server = "local" });
    }

    private void BuildRouting(BuildContext ctx)
    {
        ctx.Route.RuleSet.AddRange([
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

        var rules = new List<RouteRule>
        {
            new() {
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
            new() { IpIsPrivate = true, Action = "route", Outbound = _appSettings.Direct },
            new() { IpCidr = ["::/0"], Action = "reject" },
            new() { IpCidr = ["223.5.5.5/32"], Action = "route", Outbound = _appSettings.Direct },
            new() { Port = [3478, 3479, 19302, 19303], Network = ["udp"], Action = "reject" },
            new() { Inbound = ["tun-in", "mixed-in"], Port = [443], Network = ["udp"], Action = "reject" },
            new() { Inbound = ["tun-in", "mixed-in"], Action = "sniff", Timeout = "300ms" },
            new() { Protocol = ["ssh"], Action = "route", Outbound = _appSettings.Direct },
            new() { RuleSet = ["geosite-category-ads-all"], Action = "reject" }
        };

        // 自动化织入服务分流规则
        foreach (var service in ProfileDefinitions.Services)
        {
            if (service.RuleSets.Count > 0)
            {
                rules.Add(new() 
                { 
                    RuleSet = service.RuleSets, 
                    Action = "route", 
                    Outbound = service.Name 
                });
            }
        }

        // 添加剩余的基础直连和解析规则
        rules.AddRange([
            new RouteRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Outbound = _appSettings.Direct },
            new RouteRule { Inbound = ["mixed-in"], Action = "resolve" },
            new RouteRule { RuleSet = ["geoip-cn"], Action = "route", Outbound = _appSettings.Direct }
        ]);

        ctx.Route.Rules.AddRange(rules);
    }

    private SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName) => new()
    {
        Tag = tag,
        Type = "remote",
        Format = "binary",
        Url = $"https://fastly.jsdelivr.net/gh/MetaCubeX/meta-rules-dat@sing/geo/{repoType}/{fileName}.srs",
        DownloadDetour = _appSettings.Direct,
        UpdateInterval = "1d"
    };
}