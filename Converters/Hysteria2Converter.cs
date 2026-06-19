using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class Hysteria2Converter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "hysteria2";

    public Outbound Convert(Dictionary<string, object> p)
    {
        string name = p.TryGetValue("name", out var n) ? n.ToString()! : "unknown-hy2";
        string server = p.TryGetValue("server", out var s) ? s.ToString()! : "";

        // 1. 解析端口：兼顾常规端口与 Hysteria2 的端口跳跃
        int? serverPort = null;
        List<string>? serverPorts = null;

        if (p.TryGetValue("port", out var pt) && int.TryParse(pt.ToString(), out int parsedPort))
        {
            serverPort = parsedPort;
        }
        else if (p.TryGetValue("ports", out var pts))
        {
            string portsStr = pts.ToString()!;
            // 如果包含连字符，说明是跳跃范围（Sing-box 支持 ["20000-50000"] 这样的字符串数组）
            if (portsStr.Contains('-') || portsStr.Contains(','))
            {
                serverPorts = [portsStr];
            }
            else if (int.TryParse(portsStr, out int singlePort))
            {
                serverPort = singlePort;
            }
        }

        // 2. 解析限速（提取 "100 Mbps" 中的数字部分 100）
        int? up = ParseSpeed(p, "up");
        int? down = ParseSpeed(p, "down");

        // 3. 解析混淆配置
        OutboundObfs? obfsConfig = null;
        if (p.TryGetValue("obfs", out var obfsType) && p.TryGetValue("obfs-password", out var obfsPwd))
        {
            obfsConfig = new OutboundObfs
            {
                Type = obfsType.ToString(),
                Password = obfsPwd.ToString()
            };
        }

        // 4. 解析 TLS (Hysteria2 原生必须有 TLS)
        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server, forceTls: true);

        // 5. 拼装为标准 Outbound，抛给上层的 Builder
        return new Outbound
        {
            Type = "hysteria2",
            Tag = name,
            Server = server,
            ServerPort = serverPort,
            ServerPorts = serverPorts,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Password = p.TryGetValue("password", out var pwd) ? pwd.ToString() : "",
            UpMbps = up,
            DownMbps = down,
            Obfs = obfsConfig,
            Tls = tlsConfig
        };
    }

    /// <summary>
    /// 从 "100 Mbps" 等字符串中安全提取出数值类型
    /// </summary>
    private int? ParseSpeed(Dictionary<string, object> p, string key)
    {
        if (p.TryGetValue(key, out var val))
        {
            string str = val.ToString()!;
            // 剔除字母和空格，仅保留数字进行转换
            string digits = new(str.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int speed))
            {
                return speed;
            }
        }
        return null;
    }
}