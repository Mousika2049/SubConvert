using SubConvert.Builders;
using SubConvert.Converters;
using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Parsers;

namespace SubConvert.Services;

public static class ConversionService
{
    private static readonly IReadOnlyList<IProxyConverter> Converters =
    [
        new TrojanConverter(),
        new VlessConverter()
    ];

    public static SingboxConfig Convert(string yamlContent, TargetPlatform platform)
    {
        var clashConfig = ClashParser.Parse(yamlContent)
            ?? throw new InvalidOperationException("YAML 解析失败，请检查文件内容。");

        var config = new SingboxConfigBuilder(platform, Converters)
            .WithDefaultInbounds()
            .WithDirectOutbound()
            .WithProxyNodes(clashConfig)
            .WithRegionOutbounds()
            .WithProxyGroups()
            .WithDns()
            .WithRouting()
            .Build();

        // 计算 JSON 哈希以生成 CacheId
        string json = ConfigSerializer.Serialize(config);
        string hashId = ConfigSerializer.GetContentHash(json);

        return config with
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
    }
}