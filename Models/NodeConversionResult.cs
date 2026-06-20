using SubConvert.Models.Singbox;

namespace SubConvert.Models;

public record NodeConversionResult(
    bool IsSuccess, 
    ProxyOutbound? Outbound, 
    string? ErrorMessage
)
{
    // 提供两个静态工厂方法，让代码更具语义化
    public static NodeConversionResult Success(ProxyOutbound outbound) 
        => new(true, outbound, null);

    public static NodeConversionResult Fail(string errorMsg) 
        => new(false, null, errorMsg);
}