using System.Text.Json.Serialization;

namespace SubConvert.Models.Singbox;

public record Inbound
{
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("tag")] public required string Tag { get; init; }
    [JsonPropertyName("listen")] public required string Listen { get; init; }
    [JsonPropertyName("listen_port")] public required int ListenPort { get; init; }
    [JsonPropertyName("address")] public required List<string> Address { get; init; }
    [JsonPropertyName("auto_route")] public required bool AutoRoute { get; init; }
    [JsonPropertyName("strict_route")] public required bool StrictRoute { get; init; }
    [JsonPropertyName("stack")] public required string Stack { get; init; }
    [JsonPropertyName("mtu")] public required int Mtu { get; init; }
    [JsonPropertyName("auto_redirect")] public bool? AutoRedirect { get; init; }
}