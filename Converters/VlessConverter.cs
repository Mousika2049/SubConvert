using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Helpers;
using SubConvert.Extensions;
using SubConvert.Exceptions;

namespace SubConvert.Converters;

public class VlessConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "vless";

    public NodeConversionResult Convert(Dictionary<string, object> p)
    {
        string name = p.GetString("name") ?? "Unknown-VLESS-Node";

        try
        {
            string server = p.GetRequiredString("server");

            return NodeConversionResult.Success(new VlessOutbound
            {
                Tag = name,
                Server = server,
                ServerPort = p.GetRequiredInt("port"),
                Uuid = p.GetRequiredString("uuid"),
                Flow = p.GetString("flow"), // 取不到就是 null，自动不生成字段
                Tls = TlsConfigHelper.Extract(p, server),
                PacketEncoding = "xudp",
                DomainResolver = "node-resolver",
                ConnectTimeout = "5s"       
            });
        }
        catch (NodeParseException ex)
        {
            return NodeConversionResult.Fail($"VLESS 节点 '{name}' 解析失败 -> {ex.Message}");
        }
    }
}