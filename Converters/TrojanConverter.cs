using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class TrojanConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "trojan";

    public NodeConversionResult Convert(Dictionary<string, object> p)
    {
        // 1. 提取名称 (提供 Fallback，防止报错时连名字都打不出来)
        string name = p.TryGetValue("name", out var nObj) && !string.IsNullOrWhiteSpace(nObj.ToString()) 
            ? nObj.ToString()!.Trim() 
            : "Unknown-Trojan-Node";

        // 2. 必填项：Server
        if (!p.TryGetValue("server", out var sObj) || string.IsNullOrWhiteSpace(sObj.ToString()))
            return NodeConversionResult.Fail($"Trojan 节点 '{name}' 缺失必填字段或为空: server");
        string server = sObj.ToString()!.Trim();

        // 3. 必填项：Port (必须存在且能转为合法端口)
        if (!p.TryGetValue("port", out var pObj) || !int.TryParse(pObj.ToString(), out int port) || port <= 0 || port > 65535)
            return NodeConversionResult.Fail($"Trojan 节点 '{name}' 缺失必填字段或端口格式无效: port");

        // 4. 必填项：Password
        if (!p.TryGetValue("password", out var pwdObj) || string.IsNullOrWhiteSpace(pwdObj.ToString()))
            return NodeConversionResult.Fail($"Trojan 节点 '{name}' 缺失必填字段或为空: password");
        string password = pwdObj.ToString()!.Trim();

        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server, forceTls: true);

        return NodeConversionResult.Success(new TrojanOutbound
        {
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Password = password,
            Tls = tlsConfig
        });
    }
}