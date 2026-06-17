using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class VlessConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "vless";

    public Outbound Convert(Dictionary<string, object> p)
    {
        // 1. 内部提取基础属性
        string name = p["name"].ToString()!;
        string server = p["server"].ToString()!;
        int port = int.Parse(p["port"].ToString()!);

        // 2. 提取 TLS 等协议特有属性
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server);

        // 3. 构建并返回
        return new Outbound
        {
            Type = "vless",
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