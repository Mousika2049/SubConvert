using SubConvert.Models.Singbox;

namespace SubConvert.Converters;

public interface IProxyConverter
{
    bool CanHandle(string proxyType);
    
    // 强制返回 ProxyOutbound 而不是宽泛的 Outbound
    ProxyOutbound Convert(Dictionary<string, object> proxy);
}