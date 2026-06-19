using SubConvert.Builders;
using SubConvert.Configuration;
using SubConvert.Converters;
using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Parsers;

namespace SubConvert.Services;

public static class ConversionService
{
    // 提供一套开箱即用的默认转换器（兜底）
    private static readonly IReadOnlyList<IProxyConverter> DefaultConverters =
    [
        new TrojanConverter(),
        new VlessConverter(),
        new Hysteria2Converter()
    ];

    // 新增 IEnumerable<IProxyConverter>? 参数，彻底开放扩展能力
    public static ConversionResult Convert(
        string yamlContent, 
        TargetPlatform platform, 
        AppSettings appSettings,
        IEnumerable<IProxyConverter>? converters = null)
    {
        var clashConfig = ClashParser.Parse(yamlContent)
            ?? throw new InvalidOperationException("YAML 解析失败，请检查文件内容。");

        // 如果调用方注入了自定义转换器，就使用调用方的；否则使用默认列表
        var actualConverters = converters ?? DefaultConverters;

        // 将最终决定的转换器列表透传给 Builder
        var baseConfig = new SingboxConfigBuilder(platform, actualConverters, appSettings).Build(clashConfig);

        string hashId = ConfigSerializer.GetContentHash(yamlContent + platform);

        var finalConfig = baseConfig with
        {
            Experimental = new ExperimentalConfig
            {
                CacheFile = new CacheFileConfig
                {
                    Enabled = true,
                    Path = "cache.db",
                    CacheId = hashId
                },
                ClashApi = platform != TargetPlatform.Android
                    ? new ClashApiConfig
                    {
                        ExternalController = "127.0.0.1:9090",
                        ExternalUi = platform == TargetPlatform.Windows ? "ui" : "/etc/sing-box/ui",
                        Secret = "127001"
                    }
                    : null
            }
        };

        string finalJson = ConfigSerializer.Serialize(finalConfig);

        return new ConversionResult(finalConfig, finalJson);
    }
}