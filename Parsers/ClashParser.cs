using SubConvert.Models.Clash;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SubConvert.Parsers;

public interface IClashParser
{
    ClashConfig? Parse(string yamlContent);
}

// 移除 static，实现接口
public class ClashParser : IClashParser
{
    public ClashConfig? Parse(string yamlContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<ClashConfig>(yamlContent);
    }
}