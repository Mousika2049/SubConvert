using SubConvert.Configuration;
using SubConvert.Converters;
using SubConvert.Models;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;
using SubConvert.Builders.Components;

namespace SubConvert.Builders;

public class SingboxConfigBuilder
{
    private readonly IEnumerable<IConfigComponentBuilder> _pipeline;
    private readonly AppSettings _appSettings;

    public SingboxConfigBuilder(
        TargetPlatform platform, 
        IEnumerable<IProxyConverter> converters, 
        AppSettings appSettings)
    {
        _appSettings = appSettings;

        // 声明式定义工序流水线
        _pipeline = new List<IConfigComponentBuilder>
        {
            new InboundBuilder(platform),
            new OutboundGroupBuilder(converters, appSettings),
            new DnsProfileBuilder(appSettings),
            new RouteProfileBuilder(appSettings)
        };
    }

    public SingboxConfig Build(ClashConfig clashConfig)
    {
        // 1. 开辟全新沙箱
        var ctx = new BuildContext
        {
            RawClashConfig = clashConfig,
            Route = new RouteConfig { Final = _appSettings.MainProxyGroup }
        };

        // 2. 流水线加工
        foreach (var builder in _pipeline)
        {
            builder.Build(ctx);
        }

        // 3. 收割成品组装
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
}