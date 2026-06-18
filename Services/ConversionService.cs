using SubConvert.Builders;
using SubConvert.Converters;
using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Parsers;
using SubConvert.Configuration;

namespace SubConvert.Services;

public static class ConversionService
{
    private static readonly IReadOnlyList<IProxyConverter> Converters =
    [
        new TrojanConverter(),
        new VlessConverter()
    ];

    // 新增参数 AppSettings appSettings
    public static ConversionResult Convert(string yamlContent, TargetPlatform platform, AppSettings appSettings)
    {
        var clashConfig = ClashParser.Parse(yamlContent)
            ?? throw new InvalidOperationException("YAML 解析失败，请检查文件内容。");

        // 将 appSettings 传给 Builder
        var baseConfig = new SingboxConfigBuilder(platform, Converters, appSettings).Build(clashConfig);

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