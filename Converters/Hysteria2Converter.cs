using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class Hysteria2Converter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "hysteria2";

    public NodeConversionResult Convert(Dictionary<string, object> p)
    {
        string name = p.TryGetValue("name", out var nObj) && !string.IsNullOrWhiteSpace(nObj.ToString()) 
            ? nObj.ToString()!.Trim() : "Unknown-HY2-Node";

        if (!p.TryGetValue("server", out var sObj) || string.IsNullOrWhiteSpace(sObj.ToString()))
            return NodeConversionResult.Fail($"Hysteria2 节点 '{name}' 缺失必填字段或为空: server");
        string server = sObj.ToString()!.Trim();

        // 优先解析 ports（端口跳跃特性优先级最高）
        int? serverPort = null;
        List<string>? serverPorts = null;

        if (p.TryGetValue("ports", out var ptsObj) && !string.IsNullOrWhiteSpace(ptsObj.ToString()))
        {
            string portsStr = ptsObj.ToString()!.Trim();
            
            // 如果包含连字符或逗号，说明是真正的跳跃端口，赋值给 serverPorts
            if (portsStr.Contains('-') || portsStr.Contains(','))
            {
                serverPorts = [portsStr];
            }
            // 边缘情况兜底：如果机场在 ports 字段里只写了一个纯数字，降级解析为 serverPort
            else if (int.TryParse(portsStr, out int singlePort) && singlePort > 0 && singlePort <= 65535)
            {
                serverPort = singlePort;
            }
        }
        // 只有在 ports 完全不存在或无内容时，才去读取旧版的 port 字段
        else if (p.TryGetValue("port", out var ptObj) && int.TryParse(ptObj.ToString(), out int parsedPort) && parsedPort > 0 && parsedPort <= 65535)
        {
            serverPort = parsedPort;
        }

        // 如果两个都没解析成功，抛出明确的错误
        if (serverPort == null && serverPorts == null)
        {
            return NodeConversionResult.Fail($"Hysteria2 节点 '{name}' 缺失必填字段或端口格式无效 (需 port 或 ports)");
        }

        if (!p.TryGetValue("password", out var pwdObj) || string.IsNullOrWhiteSpace(pwdObj.ToString()))
            return NodeConversionResult.Fail($"Hysteria2 节点 '{name}' 缺失必填字段或为空: password");

        int? up = ParseSpeed(p, "up");
        int? down = ParseSpeed(p, "down");

        OutboundObfs? obfsConfig = null;
        if (p.TryGetValue("obfs", out var obfsType) && p.TryGetValue("obfs-password", out var obfsPwd))
        {
            obfsConfig = new OutboundObfs
            {
                Type = obfsType.ToString(),
                Password = obfsPwd.ToString()
            };
        }

        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server, forceTls: true);

        return NodeConversionResult.Success(new Hysteria2Outbound
        {
            Tag = name,
            Server = server,
            ServerPort = serverPort ?? 0,
            ServerPorts = serverPorts,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Password = pwdObj.ToString()!.Trim(),
            UpMbps = up,
            DownMbps = down,
            Obfs = obfsConfig,
            Tls = tlsConfig
        });
    }

    private int? ParseSpeed(Dictionary<string, object> p, string key)
    {
        if (p.TryGetValue(key, out var val))
        {
            string str = val.ToString()!;
            string digits = new string(str.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int speed)) return speed;
        }
        return null;
    }
}