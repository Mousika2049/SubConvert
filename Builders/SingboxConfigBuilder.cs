using System.Net;
using SubConvert.Configuration;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;
using SubConvert.Converters;

namespace SubConvert.Builders;

public enum TargetPlatform
{
    Windows,
    Android,
    Linux
}

public class SingboxConfigBuilder(TargetPlatform platform, IEnumerable<IProxyConverter> converters)
{
    private readonly TargetPlatform _platform = platform;
    private readonly IEnumerable<IProxyConverter> _converters = converters;
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
            Tag = AppSettings.Direct,
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

            // 核心变动：动态查找匹配的 Converter，告别 switch
            var converter = _converters.FirstOrDefault(c => c.CanHandle(type));
            if (converter == null) continue;

            string name = p["name"].ToString()!;
            string server = p["server"].ToString()!;
            int port = int.Parse(p["port"].ToString()!);

            _allNodeNames.Add(name);
            if (!IPAddress.TryParse(server, out _)) _proxyServerDomains.Add(server);

            _nodeOutbounds.Add(converter.Convert(p, name, server, port));
        }
        return this;
    }

    public SingboxConfigBuilder WithRegionOutbounds()
    {
        foreach (var region in ProfileDefinitions.RegionRegexes)
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
        mainGroupOptions.Add(AppSettings.Direct);

        _mainOutbounds.Add(new Outbound
        {
            Type = "selector",
            Tag = AppSettings.MainProxyGroup,
            Outbounds = mainGroupOptions,
            Default = _finalRegionGroupNames.FirstOrDefault() ?? _allNodeNames.FirstOrDefault() ?? AppSettings.Direct,
            InterruptExistConnections = true
        });

        var serviceGroupOptions = new List<string> { AppSettings.MainProxyGroup };
        serviceGroupOptions.AddRange(_finalRegionGroupNames);
        serviceGroupOptions.AddRange(_allNodeNames);
        serviceGroupOptions.Add(AppSettings.Direct);

        string usGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? AppSettings.MainProxyGroup;
        string sgGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇸🇬")) ?? AppSettings.MainProxyGroup;
        string hkGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇭🇰")) ?? AppSettings.MainProxyGroup;

        var specialGroups = ProfileDefinitions.GetServiceGroupMappings(usGroup, hkGroup, AppSettings.MainProxyGroup);

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
            new DnsServer { Tag = "bootstrap", Type = "local" },
            new DnsServer { Tag = "node-resolver", Type = "https", Server = "223.5.5.5" },
            new DnsServer { Tag = "remote", Type = "https", Server = "1.1.1.1", Detour = AppSettings.MainProxyGroup },
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
            new RouteRule { IpIsPrivate = true, Action = "route", Outbound = AppSettings.Direct },
            new RouteRule { IpCidr = ["::/0"], Action = "reject" },
            new RouteRule { IpCidr = ["223.5.5.5/32"], Action = "route", Outbound = AppSettings.Direct },
            new RouteRule { Port = [3478, 3479, 19302, 19303], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Port = [443], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Action = "sniff", Timeout = "300ms" },
            new RouteRule { Protocol = ["ssh"], Action = "route", Outbound = AppSettings.Direct },
            new RouteRule { RuleSet = ["geosite-category-ads-all"], Action = "reject" },
            new RouteRule { RuleSet = ["geosite-spotify"], Action = "route", Outbound = "🎵 Spotify" },
            new RouteRule { RuleSet = ["geosite-steam"], Action = "route", Outbound = "🎮 Steam" },
            new RouteRule { RuleSet = ["geosite-category-ai-!cn"], Action = "route", Outbound = "🤖 AI" },
            new RouteRule { RuleSet = ["geosite-microsoft"], Action = "route", Outbound = "🪟 Microsoft" },
            new RouteRule { RuleSet = ["geosite-telegram"], Action = "route", Outbound = "✈️ Telegram" },
            new RouteRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Outbound = AppSettings.Direct },
            new RouteRule { Inbound = ["mixed-in"], Action = "resolve" },
            new RouteRule { RuleSet = ["geoip-telegram"], Action = "route", Outbound = "✈️ Telegram" },
            new RouteRule { RuleSet = ["geoip-cn"], Action = "route", Outbound = AppSettings.Direct }
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
        DownloadDetour = AppSettings.Direct,
        UpdateInterval = "1d"
    };
}