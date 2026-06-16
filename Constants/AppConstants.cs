using System.Text.RegularExpressions;

namespace SubConvert.Constants;

public static class AppConstants
{
    public const string MainProxyGroup = "🚀 PROXIES";
    public const string Direct = "DIRECT";

    public static readonly Dictionary<string, Regex> RegionRegexes = new()
    {
        { "🇭🇰 香港", new Regex(@"(?i)香港|hong\s?kong|深港|🇭🇰|(?<![a-zA-Z])hkg?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇸🇬 狮城", new Regex(@"(?i)狮城|新加坡|singapore|🇸🇬|(?<![a-zA-Z])sgp?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇯🇵 日本", new Regex(@"(?i)日本|japan|tokyo|东京|大阪|🇯🇵|(?<![a-zA-Z])jpn?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
        { "🇺🇸 美国", new Regex(@"(?i)美国|america|洛杉矶|硅谷|🇺🇸|(?<![a-zA-Z])usa?\d*(?![a-zA-Z])", RegexOptions.Compiled) },
    };
}