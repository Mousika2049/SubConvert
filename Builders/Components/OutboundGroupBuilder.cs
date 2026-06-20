using System.Net;
using SubConvert.Configuration;
using SubConvert.Converters;
using SubConvert.Models;
using SubConvert.Models.Singbox;

namespace SubConvert.Builders.Components;

public class OutboundGroupBuilder(
    IEnumerable<IProxyConverter> converters, 
    AppSettings appSettings) : IConfigComponentBuilder
{
    public void Build(BuildContext ctx)
    {
        BuildDirectOutbound(ctx);
        BuildProxyNodes(ctx); 
        BuildRegionOutbounds(ctx);
        BuildProxyGroups(ctx);
    }

    private void BuildDirectOutbound(BuildContext ctx)
    {
        ctx.DirectOutbound = new DirectOutbound
        {
            Tag = appSettings.Direct,
            DomainResolver = "local"
        };
    }

    private void BuildProxyNodes(BuildContext ctx)
    {
        if (ctx.RawClashConfig?.Proxies == null) return;

        foreach (var p in ctx.RawClashConfig.Proxies)
        {
            if (!p.TryGetValue("type", out var typeObj)) continue;
            string type = typeObj.ToString()!;

            var converter = converters.FirstOrDefault(c => c.CanHandle(type));
            if (converter == null) continue;

            // 核心改动：获取校验结果
            var result = converter.Convert(p);
            
            // 优雅降级：如果失败，只打印日志，不中断程序，直接跳过处理下一个
            if (!result.IsSuccess)
            {
                // 可以使用黄色的警告字体来引起用户的注意
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING] 跳过无效节点 -> {result.ErrorMessage}");
                Console.ResetColor();
                continue;
            }

            // 安全解包，走到这里肯定不为空
            ProxyOutbound outbound = result.Outbound!;

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
            var pattern = kvp.Value.Pattern;

            var matchedNodes = ctx.AllNodeNames.Where(name => pattern.IsMatch(name)).ToList();

            if (matchedNodes.Count >= 2)
            {
                // 记录逻辑 ID 到实际生成的显示名的映射
                ctx.GeneratedRegions[regionId] = groupName;
                
                ctx.RegionOutbounds.Add(new SelectorOutbound
                {
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
        mainGroupOptions.Add(appSettings.Direct);

        ctx.MainOutbounds.Add(new SelectorOutbound
        {
            Tag = appSettings.MainProxyGroup,
            Outbounds = mainGroupOptions,
            Default = ctx.GeneratedRegions.Values.FirstOrDefault() ?? ctx.AllNodeNames.FirstOrDefault() ?? appSettings.Direct,
            InterruptExistConnections = true
        });

        var serviceGroupOptions = new List<string> { appSettings.MainProxyGroup };
        serviceGroupOptions.AddRange(ctx.GeneratedRegions.Values);
        serviceGroupOptions.AddRange(ctx.AllNodeNames);
        serviceGroupOptions.Add(appSettings.Direct);

        // 自动化生成服务出站组
        foreach (var service in ProfileDefinitions.Services)
        {
            string defaultSelection = appSettings.MainProxyGroup;
            if (service.DefaultRegion.HasValue && ctx.GeneratedRegions.TryGetValue(service.DefaultRegion.Value, out string? generatedRegionName))
            {
                defaultSelection = generatedRegionName;
            }

            ctx.ServiceOutbounds.Add(new SelectorOutbound
            {
                Tag = service.Name,
                Outbounds = serviceGroupOptions,
                Default = defaultSelection,
                InterruptExistConnections = true
            });
        }
    }
}