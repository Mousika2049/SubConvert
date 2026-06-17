using SubConvert.Models.Singbox;

namespace SubConvert.Converters;

public interface IProxyConverter
{
    bool CanHandle(string proxyType);
    
    // 只有字典，干净利落
    Outbound Convert(Dictionary<string, object> proxy);
}