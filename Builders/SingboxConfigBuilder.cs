using Microsoft.Extensions.Options;
using SubConvert.Configuration;
using SubConvert.Models;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;

namespace SubConvert.Builders;

public interface ISingboxConfigBuilder
{
    SingboxConfig Build(ClashConfig clashConfig, TargetPlatform platform);
}

// 注入流水线组件和配置
public class SingboxConfigBuilder(
    IEnumerable<IConfigComponentBuilder> pipeline, 
    IOptions<AppSettings> options) : ISingboxConfigBuilder
{
    private readonly AppSettings _appSettings = options.Value;

    public SingboxConfig Build(ClashConfig clashConfig, TargetPlatform platform)
    {
        // 1. 初始化带有动态参数的上下文
        var ctx = new BuildContext
        {
            RawClashConfig = clashConfig,
            Platform = platform,
            Route = new RouteConfig { Final = _appSettings.MainProxyGroup }
        };

        // 2. 执行 DI 容器提供的所有流水线组件
        foreach (var component in pipeline)
        {
            component.Build(ctx);
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