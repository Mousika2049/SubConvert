using SubConvert.Models.Singbox;

namespace SubConvert.Helpers;

public static class TlsConfigHelper
{
    public static OutboundTls? Extract(Dictionary<string, object> p, string server, bool forceTls = false)
    {
        bool isTls = p.TryGetValue("tls", out var tlsObj) && bool.TryParse(tlsObj.ToString(), out var b) && b;
        bool isReality = p.ContainsKey("reality-opts");

        if (!forceTls && !isTls && !isReality) return null;

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

        return new OutboundTls
        {
            Enabled = true,
            ServerName = serverName,
            Insecure = p.TryGetValue("skip-cert-verify", out var skipCert) && bool.TryParse(skipCert.ToString(), out var insec) ? insec : null,
            Utls = new Utls { Enabled = true, Fingerprint = fp },
            Alpn = ["h2", "http/1.1"],
            MinVersion = "1.3",
            Reality = realityConfig
        };
    }
}