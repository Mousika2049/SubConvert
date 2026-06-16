using System.Text.Json.Serialization;

namespace SubConvert.Models.Singbox;

public record Outbound
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("tag")] public string? Tag { get; init; }
    [JsonPropertyName("server")] public string? Server { get; init; }
    [JsonPropertyName("server_port")] public int? ServerPort { get; init; }
    // VLESS 核心字段
    [JsonPropertyName("uuid")] public string? Uuid { get; init; }
    [JsonPropertyName("flow")] public string? Flow { get; init; }
    [JsonPropertyName("packet_encoding")] public string? PacketEncoding { get; init; }
    [JsonPropertyName("password")] public string? Password { get; init; }
    [JsonPropertyName("outbounds")] public List<string>? Outbounds { get; init; }
    [JsonPropertyName("default")] public string? Default { get; init; }
    [JsonPropertyName("tls")] public OutboundTls? Tls { get; init; }
    [JsonPropertyName("domain_resolver")] public string? DomainResolver { get; init; }
    [JsonPropertyName("connect_timeout")] public string? ConnectTimeout { get; init; }
    [JsonPropertyName("interrupt_exist_connections")] public bool? InterruptExistConnections { get; init; }
}

public record Utls
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    [JsonPropertyName("fingerprint")] public string? Fingerprint { get; init; }
}

public record OutboundTls
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; } = true;
    [JsonPropertyName("server_name")] public string? ServerName { get; init; }
    [JsonPropertyName("insecure")] public bool? Insecure { get; init; }
    [JsonPropertyName("utls")] public Utls? Utls { get; init; }
    [JsonPropertyName("alpn")] public List<string>? Alpn { get; init; }
    [JsonPropertyName("min_version")] public string? MinVersion { get; init; }
    [JsonPropertyName("reality")] public OutboundReality? Reality { get; init; }
}

public record OutboundReality
{
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    [JsonPropertyName("public_key")] public string? PublicKey { get; init; }
    [JsonPropertyName("short_id")] public string? ShortId { get; init; }
}