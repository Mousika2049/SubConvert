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
        try
        {
            return deserializer.Deserialize<ClashConfig>(yamlContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] YAML Parsing failed: {ex.Message}");
            return null;
        }
    }
}