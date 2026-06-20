using SubConvert.Models;
using SubConvert.Models.Clash;
using SubConvert.Models.Singbox;

namespace SubConvert.Builders;

public class BuildContext
{
    // ── 运行时动态参数 ──
    public TargetPlatform Platform { get; init; } // 新增：将平台信息放入上下文

    // ── 最终产出数据源 ──
    public LogConfig Log { get; } = new();
    public DnsConfig Dns { get; } = new();
    public List<Inbound> Inbounds { get; } = [];
    public RouteConfig Route { get; set; } = new();

    // ── 输入原材料与中间计算状态 ──
    public ClashConfig? RawClashConfig { get; set; }
    public HashSet<string> ProxyServerDomains { get; } = [];
    public List<string> AllNodeNames { get; } = [];
    public Dictionary<RegionId, string> GeneratedRegions { get; } = [];

    // ── 出站代理分类存放 ──
    public Outbound? DirectOutbound { get; set; }
    public List<Outbound> NodeOutbounds { get; } = [];
    public List<Outbound> RegionOutbounds { get; } = [];
    public List<Outbound> MainOutbounds { get; } = [];
    public List<Outbound> ServiceOutbounds { get; } = [];
}