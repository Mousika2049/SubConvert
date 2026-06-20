using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Helpers;

namespace SubConvert.Converters;

public class VlessConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "vless";

    public NodeConversionResult Convert(Dictionary<string, object> p)
    {
        string name = p.TryGetValue("name", out var nObj) && !string.IsNullOrWhiteSpace(nObj.ToString()) 
            ? nObj.ToString()!.Trim() : "Unknown-VLESS-Node";

        if (!p.TryGetValue("server", out var sObj) || string.IsNullOrWhiteSpace(sObj.ToString()))
            return NodeConversionResult.Fail($"VLESS 节点 '{name}' 缺失必填字段或为空: server");
        string server = sObj.ToString()!.Trim();

        if (!p.TryGetValue("port", out var pObj) || !int.TryParse(pObj.ToString(), out int port) || port <= 0 || port > 65535)
            return NodeConversionResult.Fail($"VLESS 节点 '{name}' 缺失必填字段或端口格式无效: port");

        if (!p.TryGetValue("uuid", out var uuidObj) || string.IsNullOrWhiteSpace(uuidObj.ToString()))
            return NodeConversionResult.Fail($"VLESS 节点 '{name}' 缺失必填字段或为空: uuid");

        OutboundTls? tlsConfig = TlsConfigHelper.Extract(p, server);

        return NodeConversionResult.Success(new VlessOutbound
        {
            Tag = name,
            Server = server,
            ServerPort = port,
            DomainResolver = "node-resolver",
            ConnectTimeout = "5s",
            Uuid = uuidObj.ToString()!.Trim(),
            Flow = p.TryGetValue("flow", out var f) ? f.ToString() : null,
            PacketEncoding = "xudp",
            Tls = tlsConfig
        });
    }
}