using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class TrojanConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "trojan";

    public ProxyOutbound Convert(Dictionary<string, object> p)
    {
        string name   = p.TryGetValue("name",   out var n) ? n.ToString()! : "unknown-trojan";
        string server = p.TryGetValue("server", out var s) ? s.ToString()! : "";
        if (!p.TryGetValue("port", out var pt) || !int.TryParse(pt.ToString(), out int port))
        {
            return null; // 跳过无效节点，不崩溃整个转换
        }

        // Trojan 默认 forceTls = true
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server, forceTls: true);
        return new TrojanOutbound
        {
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Password = p.TryGetValue("password", out var pwd) ? pwd.ToString() : "",
            Tls = tlsConfig
        };
    }
}