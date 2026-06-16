using YamlDotNet.Serialization;

namespace SubConvert.Models.Clash;

public record ClashConfig
{
    [YamlMember(Alias = "proxies")] public List<Dictionary<string, object>> Proxies { get; init; } = [];
}