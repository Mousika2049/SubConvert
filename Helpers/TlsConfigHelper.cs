using SubConvert.Models.Singbox;

namespace SubConvert.Helpers;

public static class TlsConfigHelper
{
    public static OutboundTls Extract(Dictionary<string, object> p, string server, bool forceTls = false)
    {
        bool isTls = p.TryGetValue("tls", out var tlsObj) && bool.TryParse(tlsObj.ToString(), out var b) && b;
        bool isReality = p.ContainsKey("reality-opts");

        if (!forceTls && !isTls && !isReality) return null;

        // 提取 Reality 专属
        OutboundReality? realityConfig = null;
        if (isReality && p["reality-opts"] is Dictionary<object, object> realityOpts)
        {
            realityConfig = new OutboundReality
            {
                Enabled = true,
                PublicKey = realityOpts.TryGetValue("public-key", out var pk) ? pk.ToString() : null,
                ShortId = realityOpts.TryGetValue("short-id", out var sid) ? sid.ToString() : null
            };
        }

        string fp = p.TryGetValue("client-fingerprint", out var fpObj) ? fpObj.ToString()! : "firefox";

        string serverName = server;
        if (p.TryGetValue("sni", out var sniObj))
            serverName = sniObj.ToString()!;
        else if (p.TryGetValue("servername", out var snObj))
            serverName = snObj.ToString()!;

        // 新增：动态解析 ALPN，如果不存在则使用默认值
        List<string> alpnList = ["h2", "http/1.1"];
        if (p.TryGetValue("alpn", out var alpnObj))
        {
            // YamlDotNet 通常将 YAML 数组反序列化为 List<object>
            if (alpnObj is List<object> list)
            {
                alpnList = [.. list.Select(x => x.ToString()!)];
            }
            else if (alpnObj is string alpnStr)
            {
                alpnList = [alpnStr];
            }
        }

        return new OutboundTls
        {
            Enabled = true,
            ServerName = serverName,
            Insecure = p.TryGetValue("skip-cert-verify", out var skipCert) && bool.TryParse(skipCert.ToString(), out var insec) ? insec : null,
            Utls = new Utls { Enabled = true, Fingerprint = fp },
            Alpn = alpnList, // 注入动态读取的 ALPN
            MinVersion = "1.3",
            Reality = realityConfig
        };
    }
}