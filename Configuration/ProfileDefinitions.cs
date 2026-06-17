using System.Text.RegularExpressions;

namespace SubConvert.Configuration;

public static class ProfileDefinitions
{
    // 区域节点正则匹配规则
    public static readonly IReadOnlyDictionary<string, Regex> RegionRegexes = new Dictionary<string, Regex>
    {
        { "🇭🇰 香港", new Regex(@"(?i)香港|hong\s?kong|深港|🇭🇰|(?<![a-zA-Z])hkg?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇸🇬 狮城", new Regex(@"(?i)狮城|新加坡|singapore|🇸🇬|(?<![a-zA-Z])sgp?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇯🇵 日本", new Regex(@"(?i)日本|japan|tokyo|东京|大阪|🇯🇵|(?<![a-zA-Z])jpn?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇺🇸 美国", new Regex(@"(?i)美国|america|洛杉矶|硅谷|🇺🇸|(?<![a-zA-Z])usa?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
    };

    // 服务组默认路由策略映射
    public static IReadOnlyDictionary<string, string> GetServiceGroupMappings(string usGroup, string hkGroup, string mainGroup)
    {
        return new Dictionary<string, string>
        {
            { ServiceGroupNames.Spotify, usGroup },
            { ServiceGroupNames.Steam, hkGroup },
            { ServiceGroupNames.Ai, usGroup },
            { ServiceGroupNames.Microsoft, hkGroup },
            { ServiceGroupNames.Telegram, mainGroup },
        };
    }
}