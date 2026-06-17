using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class TrojanConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "trojan";

    public Outbound Convert(Dictionary<string, object> p)
    {
        string name = p["name"].ToString()!;
        string server = p["server"].ToString()!;
        int port = int.Parse(p["port"].ToString()!);
        
        // Trojan 默认 forceTls = true
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server, forceTls: true);
        return new Outbound
        {
            Type = "trojan",
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