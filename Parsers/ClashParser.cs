using SubConvert.Models.Clash;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SubConvert.Parsers;

public static class ClashParser
{
    public static ClashConfig? Parse(string yamlContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // 移除 try/catch，直接反序列化。
        // 如果 YAML 语法错误，抛出的 YamlException 将直接透传到最外层；
        // 如果文件为空导致返回 null，将被调用方的 ?? throw 拦截。
        return deserializer.Deserialize<ClashConfig>(yamlContent);
    }
}