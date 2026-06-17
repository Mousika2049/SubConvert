using System.Text.Json.Serialization;

namespace SubConvert.Models.Singbox;

public record DnsConfig
{
    [JsonPropertyName("servers")] public List<DnsServer> Servers { get; init; } = [];
    [JsonPropertyName("rules")] public List<DnsRule> Rules { get; init; } = [];
    [JsonPropertyName("final")] public string Final { get; init; } = "remote";
    [JsonPropertyName("strategy")] public string Strategy { get; init; } = "prefer_ipv4";
    [JsonPropertyName("reverse_mapping")] public bool ReverseMapping { get; init; } = true;
}

public record DnsServer
{
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("detour")] public string? Detour { get; init; }
}

public record DnsRule
{
    [JsonPropertyName("rule_set")] public List<string>? RuleSet { get; init; }
    [JsonPropertyName("domain")] public List<string>? Domain { get; init; }
    [JsonPropertyName("domain_suffix")] public List<string>? DomainSuffix { get; init; }
    [JsonPropertyName("query_type")] public List<string>? QueryType { get; init; }
    [JsonPropertyName("action")] public string? Action { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("rcode")] public string? Rcode { get; init; }
}