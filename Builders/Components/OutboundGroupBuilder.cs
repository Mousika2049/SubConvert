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

            // 契约强制返回包含 Server 属性的 ProxyOutbound
            ProxyOutbound outbound = converter.Convert(p);
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