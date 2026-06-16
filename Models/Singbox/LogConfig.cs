using System.Text.Json.Serialization;

namespace SubConvert.Models.Singbox;

public record LogConfig
{
    [JsonPropertyName("level")] public string Level { get; init; } = "warn";
    [JsonPropertyName("timestamp")] public bool Timestamp { get; init; } = true;
}