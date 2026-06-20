using SubConvert.Models;
using SubConvert.Models.Singbox;

namespace SubConvert.Builders.Components;

// 移除构造函数中的 platform 参数
public class InboundBuilder : IConfigComponentBuilder
{
    public void Build(BuildContext ctx)
    {
        // 从上下文中读取运行时平台
        var platform = ctx.Platform;

        ctx.Inbounds.Add(new Inbound
        {
            Type = "tun",
            Tag = "tun-in",
            Address = ["172.19.0.1/30", "fd00::1/126"],
            AutoRoute = true,
            AutoRedirect = platform == TargetPlatform.Linux ? true : null,
            StrictRoute = true,
            Stack = platform switch
            {
                TargetPlatform.Windows => "mixed",
                TargetPlatform.Linux => "system",
                TargetPlatform.Android => "system",
                _ => "system"
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
}