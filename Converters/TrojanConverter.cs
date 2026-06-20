using SubConvert.Models;
using SubConvert.Models.Singbox;
using SubConvert.Helpers;
using SubConvert.Extensions;
using SubConvert.Exceptions;

namespace SubConvert.Converters;

public class TrojanConverter : IProxyConverter
{
    public bool CanHandle(string proxyType) => proxyType == "trojan";

    public NodeConversionResult Convert(Dictionary<string, object> p)
    {
        string name = p.GetString("name") ?? "Unknown-Trojan-Node";

        try
        {
            string server = p.GetRequiredString("server");

            return NodeConversionResult.Success(new TrojanOutbound
            {
                Tag = name,
                Server = server,
                ServerPort = p.GetRequiredInt("port"),
                Password = p.GetRequiredString("password"),
                Tls = TlsConfigHelper.Extract(p, server, forceTls: true),
                DomainResolver = "node-resolver",
                ConnectTimeout = "5s"
            });
        }
        catch (NodeParseException ex)
        {
            return NodeConversionResult.Fail($"Trojan 节点 '{name}' 解析失败 -> {ex.Message}");
        }
    }
}