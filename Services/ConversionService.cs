using SubConvert.Builders;
using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Parsers;

namespace SubConvert.Services;

public class ConversionService(
    IClashParser clashParser,
    ISingboxConfigBuilder configBuilder,
    IConfigSerializer configSerializer)
{
    public ConversionResult Convert(string yamlContent, TargetPlatform platform)
    {
        var clashConfig = clashParser.Parse(yamlContent)
            ?? throw new InvalidOperationException("YAML 解析失败，请检查文件内容。");

        // 使用注入的 builder
        var baseConfig = configBuilder.Build(clashConfig, platform);

        // 使用注入的 serializer
        string hashId = configSerializer.GetContentHash(yamlContent + platform);

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

        string finalJson = configSerializer.Serialize(finalConfig);

        return new ConversionResult(finalConfig, finalJson);
    }
}