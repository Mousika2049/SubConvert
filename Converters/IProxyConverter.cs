using SubConvert.Models.Singbox;

namespace SubConvert.Converters;

public interface IProxyConverter
{
    bool CanHandle(string proxyType);
    Outbound Convert(Dictionary<string, object> proxy, string name, string server, int port, OutboundTls? tlsConfig);
}