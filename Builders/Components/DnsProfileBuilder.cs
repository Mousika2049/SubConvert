using SubConvert.Configuration;
using SubConvert.Models.Singbox;
using Microsoft.Extensions.Options;

namespace SubConvert.Builders.Components;

public class DnsProfileBuilder(IOptions<AppSettings> options) : IConfigComponentBuilder
{
    private readonly AppSettings appSettings = options.Value;
    public void Build(BuildContext ctx)
    {
        ctx.Dns.Servers.AddRange([
            new DnsServer { Tag = "bootstrap", Type = "local" },
            new DnsServer { Tag = "node-resolver", Type = "https", Server = "223.5.5.5" },
            new DnsServer { Tag = "remote", Type = "https", Server = "1.1.1.1", Detour = appSettings.MainProxyGroup },
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
}