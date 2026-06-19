using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class VlessConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "vless";

    public ProxyOutbound Convert(Dictionary<string, object> p)
    {
        // 1. 内部提取基础属性
        string name   = p.TryGetValue("name",   out var n) ? n.ToString()! : "unknown-vless";
        string server = p.TryGetValue("server", out var s) ? s.ToString()! : "";
        if (!p.TryGetValue("port", out var pt) || !int.TryParse(pt.ToString(), out int port))
        {
            return null; // 跳过无效节点，不崩溃整个转换
        }

        // 2. 提取 TLS 等协议特有属性
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server);

        // 3. 构建并返回
        return new VlessOutbound
        {
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Uuid = p.TryGetValue("uuid", out var id) ? id.ToString() : "",
            Flow = p.TryGetValue("flow", out var f) ? f.ToString() : null,
            PacketEncoding = "xudp",
            Tls = tlsConfig
        };
    }
}