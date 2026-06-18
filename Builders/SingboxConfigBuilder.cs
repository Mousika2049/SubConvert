using System.Net;
using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;
using SubConvert.Converters;

namespace SubConvert.Builders;

// 1. 构造函数新增 AppSettings 参数
public class SingboxConfigBuilder(TargetPlatform platform, IEnumerable<IProxyConverter> converters, AppSettings appSettings)
{
    private readonly TargetPlatform _platform = platform;
    private readonly IEnumerable<IProxyConverter> _converters = converters;
    private readonly AppSettings _appSettings = appSettings; // 2. 保存为私有只读字段
    
    private readonly LogConfig _log = new();
    private readonly DnsConfig _dns = new();
    private readonly List<Inbound> _inbounds = [];
    
    // 3. 使用注入的 appSettings
    private readonly RouteConfig _route = new() { Final = appSettings.MainProxyGroup };
    
    private readonly HashSet<string> _proxyServerDomains = [];
    private readonly List<string> _allNodeNames = [];
    private readonly List<string> _finalRegionGroupNames = [];
    
    private Outbound? _directOutbound;
    private readonly List<Outbound> _nodeOutbounds = [];
    private readonly List<Outbound> _regionOutbounds = [];
    private readonly List<Outbound> _mainOutbounds = [];
    private readonly List<Outbound> _serviceOutbounds = [];

    public SingboxConfig Build(ClashConfig clashConfig)
    {
        BuildDefaultInbounds();
        BuildDirectOutbound();
        BuildProxyNodes(clashConfig); 
        BuildRegionOutbounds();
        BuildProxyGroups();
        BuildDns();                   
        BuildRouting();

        var orderedOutbounds = new List<Outbound>();
        orderedOutbounds.AddRange(_mainOutbounds);
        orderedOutbounds.AddRange(_regionOutbounds);
        orderedOutbounds.AddRange(_serviceOutbounds);
        orderedOutbounds.AddRange(_nodeOutbounds);
        if (_directOutbound != null)
        {
            orderedOutbounds.Add(_directOutbound);
        }

        return new SingboxConfig
        {
            Log = _log,
            Dns = _dns,
            Inbounds = _inbounds,
            Outbounds = orderedOutbounds,
            Route = _route,
        };
    }

    private void BuildDefaultInbounds()
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
    }

    private void BuildDirectOutbound()
    {
        _directOutbound = new Outbound
        {
            Type = "direct",
            Tag = _appSettings.Direct, // 修改这里
            DomainResolver = "local"
        };
    }

    private void BuildProxyNodes(ClashConfig clashConfig)
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
                _allNodeNames.Add(outbound.Tag);
            
            if (!string.IsNullOrEmpty(outbound.Server) && !IPAddress.TryParse(outbound.Server, out _))
                _proxyServerDomains.Add(outbound.Server);

            _nodeOutbounds.Add(outbound);
        }
    }

    private void BuildRegionOutbounds()
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
    }

    private void BuildProxyGroups()
    {
        var mainGroupOptions = new List<string>(_finalRegionGroupNames);
        mainGroupOptions.AddRange(_allNodeNames);
        mainGroupOptions.Add(_appSettings.Direct); // 修改这里

        _mainOutbounds.Add(new Outbound
        {
            Type = "selector",
            Tag = _appSettings.MainProxyGroup, // 修改这里
            Outbounds = mainGroupOptions,
            Default = _finalRegionGroupNames.FirstOrDefault() ?? _allNodeNames.FirstOrDefault() ?? _appSettings.Direct, // 修改这里
            InterruptExistConnections = true
        });

        var serviceGroupOptions = new List<string> { _appSettings.MainProxyGroup }; // 修改这里
        serviceGroupOptions.AddRange(_finalRegionGroupNames);
        serviceGroupOptions.AddRange(_allNodeNames);
        serviceGroupOptions.Add(_appSettings.Direct); // 修改这里

        string usGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇺🇸")) ?? _appSettings.MainProxyGroup; // 修改这里
        string hkGroup = _finalRegionGroupNames.FirstOrDefault(n => n.Contains("🇭🇰")) ?? _appSettings.MainProxyGroup; // 修改这里

        var specialGroups = ProfileDefinitions.GetServiceGroupMappings(usGroup, hkGroup, _appSettings.MainProxyGroup); // 修改这里

        _serviceOutbounds.AddRange(specialGroups.Select(group => new Outbound
        {
            Type = "selector",
            Tag = group.Key,
            Outbounds = serviceGroupOptions,
            Default = group.Value,
            InterruptExistConnections = true
        }));
    }

    private void BuildDns()
    {
        _dns.Servers.AddRange([
            new DnsServer { Tag = "bootstrap", Type = "local" },
            new DnsServer { Tag = "node-resolver", Type = "https", Server = "223.5.5.5" },
            new DnsServer { Tag = "remote", Type = "https", Server = "1.1.1.1", Detour = _appSettings.MainProxyGroup }, // 修改这里
            new DnsServer { Tag = "local", Type = "https", Server = "223.5.5.5" }
        ]);

        _dns.Rules.Add(new DnsRule { QueryType = ["AAAA"], Action = "predefined", Rcode = "NOERROR" });
        _dns.Rules.Add(new DnsRule { RuleSet = ["geosite-category-ads-all"], Action = "predefined", Rcode = "NOERROR" });

        if (_proxyServerDomains.Count > 0)
        {
            _dns.Rules.Add(new DnsRule { Domain = [.. _proxyServerDomains], Action = "route", Server = "node-resolver" });
        }

        _dns.Rules.Add(new DnsRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Server = "local" });
    }

    private void BuildRouting()
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
            new RouteRule { IpIsPrivate = true, Action = "route", Outbound = _appSettings.Direct }, // 修改这里
            new RouteRule { IpCidr = ["::/0"], Action = "reject" },
            new RouteRule { IpCidr = ["223.5.5.5/32"], Action = "route", Outbound = _appSettings.Direct }, // 修改这里
            new RouteRule { Port = [3478, 3479, 19302, 19303], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Port = [443], Network = ["udp"], Action = "reject" },
            new RouteRule { Inbound = ["tun-in", "mixed-in"], Action = "sniff", Timeout = "300ms" },
            new RouteRule { Protocol = ["ssh"], Action = "route", Outbound = _appSettings.Direct }, // 修改这里
            new RouteRule { RuleSet = ["geosite-category-ads-all"], Action = "reject" },
            new RouteRule { RuleSet = ["geosite-spotify"], Action = "route", Outbound = ServiceGroupNames.Spotify },
            new RouteRule { RuleSet = ["geosite-steam"], Action = "route", Outbound = ServiceGroupNames.Steam },
            new RouteRule { RuleSet = ["geosite-category-ai-!cn"], Action = "route", Outbound = ServiceGroupNames.Ai },
            new RouteRule { RuleSet = ["geosite-microsoft"], Action = "route", Outbound = ServiceGroupNames.Microsoft },
            new RouteRule { RuleSet = ["geosite-telegram"], Action = "route", Outbound = ServiceGroupNames.Telegram },
            new RouteRule { RuleSet = ["geosite-cn", "geosite-category-pt"], Action = "route", Outbound = _appSettings.Direct }, // 修改这里
            new RouteRule { Inbound = ["mixed-in"], Action = "resolve" },
            new RouteRule { RuleSet = ["geoip-telegram"], Action = "route", Outbound = ServiceGroupNames.Telegram },
            new RouteRule { RuleSet = ["geoip-cn"], Action = "route", Outbound = _appSettings.Direct } // 修改这里
        ]);
    }

    // 4. 移除 static 关键字，使其能访问 _appSettings
    private SingboxRuleSet CreateRemoteRuleSet(string tag, string repoType, string fileName) => new()
    {
        Tag = tag,
        Type = "remote",
        Format = "binary",
        Url = $"https://fastly.jsdelivr.net/gh/MetaCubeX/meta-rules-dat@sing/geo/{repoType}/{fileName}.srs",
        DownloadDetour = _appSettings.Direct, // 修改这里
        UpdateInterval = "1d"
    };
}