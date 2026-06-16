using SubConvert.Models.Singbox;

namespace SubConvert.Converters;

public class TrojanConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "trojan";

    public Outbound Convert(Dictionary<string, object> p, string name, string server, int port, OutboundTls? tlsConfig)
    {
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