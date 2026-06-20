using System.Collections.Generic;
using SubConvert.Models;

namespace SubConvert.Converters;

public interface IProxyConverter
{
    bool CanHandle(string proxyType);
    
    // 契约变更：必须返回包含成功/失败状态的结果对象
    NodeConversionResult Convert(Dictionary<string, object> proxy);
}