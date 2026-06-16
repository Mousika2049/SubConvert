using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class VlessConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "vless";

    public Outbound Convert(Dictionary<string, object> p, string name, string server, int port)
    {
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server);
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