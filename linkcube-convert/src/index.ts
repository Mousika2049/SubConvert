import { parse } from 'yaml';

// ==================== 辅助函数 ====================
// 判断是否为 IP 地址
function isIpAddress(host: string): boolean {
  const ipv4 = /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/;
  const ipv6 = /^[a-fA-F0-9:]+$/;
  return ipv4.test(host) || ipv6.test(host);
}

// 快速生成远程规则集对象
function createRemoteRuleSet(tag: string, repoType: string, fileName: string, downloadDetour: string) {
  return {
    tag: tag,
    type: "remote",
    format: "binary",
    url: `https://raw.githubusercontent.com/SagerNet/sing-${repoType}/rule-set/${fileName}.srs`,
    download_detour: downloadDetour
  };
}

// ==================== 核心逻辑 ====================
export default {
  async fetch(request: Request): Promise<Response> {
    if (request.method !== 'POST') {
      return new Response("请使用 POST 方法发送 Clash YAML 配置", { status: 400 });
    }

    try {
      // 1. 读取并解析 YAML
      const yamlContent = await request.text();
      const clashConfig: any = parse(yamlContent);

      if (!clashConfig) {
        throw new Error("YAML 解析为空");
      }

      // 2. 初始化极简 Singbox 框架
      const sbConfig: any = {
        log: { level: "info", timestamp: true },
        dns: { servers: [], rules: [], final: "remote", strategy: "ipv4_only", independent_cache: true },
        inbounds: [],
        outbounds: [],
        route: { rule_set: [], rules: [], final: "🚀 PROXIES", auto_detect_interface: true, default_domain_resolver: "local" },
        experimental: {
          cache_file: { enabled: true, path: "cache.db" },
          clash_api: { external_controller: "127.0.0.1:9090", external_ui: "ui", secret: "" }
        }
      };

      // ========== Inbounds (入站) ==========
      sbConfig.inbounds.push({
        type: "tun", tag: "tun-in", interface_name: "tun0", address: ["172.19.0.1/30"],
        auto_route: true, strict_route: true, stack: "system", sniff: true, mtu: 1500, endpoint_independent_nat: true
      });
      
      if (clashConfig['mixed-port']) {
        sbConfig.inbounds.push({
          type: "mixed", tag: "mixed-in", listen: "127.0.0.1", listen_port: Number(clashConfig['mixed-port']), sniff: true
        });
      }

      // ========== Outbounds (基础节点) ==========
      sbConfig.outbounds.push({ type: "direct", tag: "DIRECT" });
      sbConfig.outbounds.push({ type: "block", tag: "BLOCK" });

      const proxyServerDomains = new Set<string>();
      const allNodeNames: string[] = [];

      if (clashConfig.proxies) {
        for (const p of clashConfig.proxies) {
          if (p.type === "trojan") {
            const name = String(p.name);
            const server = String(p.server);
            allNodeNames.push(name);

            if (!isIpAddress(server)) proxyServerDomains.add(server);
            
            sbConfig.outbounds.push({
              type: "trojan",
              tag: name,
              server: server,
              server_port: Number(p.port),
              password: String(p.password),
              tls: {
                enabled: true,
                server_name: p.sni ? String(p.sni) : server,
                insecure: p['skip-cert-verify'] === true || String(p['skip-cert-verify']).toLowerCase() === "true"
              }
            });
          }
        }
      }

      // ========== 提取机场原生【分地区】策略组并注入 Emoji ==========
      const finalRegionGroupNames: string[] = [];
      const regionOutbounds: any[] = [];

      const regionMappings = [
        { emoji: "🇭🇰", keywords: ["hk", "香港", "hongkong", "🇭🇰"] },
        { emoji: "🇸🇬", keywords: ["sg", "狮城", "新加坡", "singapore", "🇸🇬"] },
        { emoji: "🇯🇵", keywords: ["jp", "日本", "japan", "tokyo", "🇯🇵"] },
        { emoji: "🇺🇸", keywords: ["us", "美国", "america", "usa", "🇺🇸"] },
        { emoji: "🇹🇼", keywords: ["tw", "台湾", "taiwan", "taipei", "🇹🇼"] }
      ];

      if (clashConfig['proxy-groups']) {
        for (const g of clashConfig['proxy-groups']) {
          const lowerName = String(g.name).toLowerCase();
          let matchedEmoji: string | null = null;

          for (const mapping of regionMappings) {
            if (mapping.keywords.some(kw => lowerName.includes(kw))) {
              matchedEmoji = mapping.emoji;
              break;
            }
          }

          if (matchedEmoji) {
            let formattedGroupName = g.name;
            if (!formattedGroupName.includes(matchedEmoji)) {
              formattedGroupName = `${matchedEmoji} ${formattedGroupName}`;
            }

            finalRegionGroupNames.push(formattedGroupName);

            const outType = g.type === "select" ? "selector" : "urltest";
            const mappedOutbounds = (g.proxies || []).map((p: string) => p === "REJECT" ? "BLOCK" : p);

            regionOutbounds.push({
              type: outType,
              tag: formattedGroupName,
              outbounds: mappedOutbounds,
              default: outType === "selector" && mappedOutbounds.length > 0 ? mappedOutbounds[0] : undefined
            });
          }
        }
      }

      // ========== 准备所有策略组数据 (控制界面呈现顺序) ==========
      const mainProxyGroup = "🚀 PROXIES";
      const mainGroupOptions = [...finalRegionGroupNames, "DIRECT", ...allNodeNames];

      const mainProxyOutbound = {
        type: "selector",
        tag: mainProxyGroup,
        outbounds: mainGroupOptions,
        default: finalRegionGroupNames[0] || allNodeNames[0]
      };

      const customGroupOptions = [mainProxyGroup, ...mainGroupOptions];

      const usGroup = finalRegionGroupNames.find(n => n.includes("🇺🇸")) || mainProxyGroup;
      const sgGroup = finalRegionGroupNames.find(n => n.includes("🇸🇬")) || mainProxyGroup;

      const specialGroups = [
        { name: "🎬 YouTube", default: mainProxyGroup },
        { name: "🎵 Spotify", default: mainProxyGroup },
        { name: "🎮 Steam", default: mainProxyGroup },
        { name: "🎮 Epic", default: mainProxyGroup },
        { name: "🤖 OpenAI", default: usGroup },
        { name: "🪟 Microsoft", default: mainProxyGroup },
        { name: "✈️ Telegram", default: sgGroup }
      ];

      const serviceOutbounds: any[] = [];
      for (const group of specialGroups) {
        serviceOutbounds.push({
          type: "selector", tag: group.name, outbounds: customGroupOptions, default: group.default
        });
      }

      // [极其关键] 严格按照 UI 展示要求的顺序进行 Append
      sbConfig.outbounds.push(mainProxyOutbound);         // 1. 🚀 PROXIES
      for (const ro of regionOutbounds) sbConfig.outbounds.push(ro);  // 2. 地区分组
      for (const so of serviceOutbounds) sbConfig.outbounds.push(so); // 3. 业务分组

      // ========== DNS 策略 (FakeIP 支持) ==========
      sbConfig.dns.servers.push({ tag: "remote", type: "tls", server: "1.1.1.1", detour: mainProxyGroup });
      sbConfig.dns.servers.push({ tag: "local", type: "https", server: "1.12.12.12" });
      sbConfig.dns.servers.push({ tag: "fakeip", type: "fakeip", inet4_range: "198.18.0.0/15" });

      if (proxyServerDomains.size > 0) {
        sbConfig.dns.rules.push({ domain: Array.from(proxyServerDomains), server: "local" });
      }
      sbConfig.dns.rules.push({ rule_set: ["geosite-cn", "geosite-category-pt"], server: "local" });
      sbConfig.dns.rules.push({ query_type: ["A", "AAAA"], server: "fakeip" });

      // ========== Route 路由及规则集配置 ==========
      const airportDomainsRules = proxyServerDomains.size > 0 
        ? [{ domain: Array.from(proxyServerDomains) }] 
        : []; // 为空则在规则集中隐身

      sbConfig.route.rule_set.push({
        tag: "geoip-private", type: "inline", rules: [{ ip_cidr: ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8"] }]
      });
      
      // 如果 airportDomainsRules 为空，这里加进去也没有 rule 内容，符合要求
      sbConfig.route.rule_set.push({
        tag: "airport-domains", type: "inline", rules: airportDomainsRules
      });

      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-category-ads-all", "geosite", "geosite-category-ads-all", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-category-pt", "geosite", "geosite-category-pt", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-cn", "geosite", "geosite-cn", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geoip-cn", "geoip", "geoip-cn", mainProxyGroup));
      
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-youtube", "geosite", "geosite-youtube", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-spotify", "geosite", "geosite-spotify", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-steam", "geosite", "geosite-steam", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-epicgames", "geosite", "geosite-epicgames", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-openai", "geosite", "geosite-openai", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-microsoft", "geosite", "geosite-microsoft", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-telegram", "geosite", "geosite-telegram", mainProxyGroup));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geoip-telegram", "geoip", "geoip-tg", mainProxyGroup));

      sbConfig.route.rules.push({ port: [53], action: "hijack-dns" });
      sbConfig.route.rules.push({ protocol: ["dns"], action: "hijack-dns" });
      sbConfig.route.rules.push({ port: [443], network: ["udp"], outbound: "BLOCK" }); // 杀掉 QUIC 迫使 YouTube 回退 TCP
      sbConfig.route.rules.push({ ip_cidr: ["1.12.12.12/32", "1.1.1.1/32"], outbound: "DIRECT" });
      
      sbConfig.route.rules.push({ rule_set: ["geoip-private"], outbound: "DIRECT" });
      sbConfig.route.rules.push({ rule_set: ["airport-domains"], outbound: "DIRECT" });
      
      sbConfig.route.rules.push({ rule_set: ["geosite-category-ads-all"], outbound: "BLOCK" });
      sbConfig.route.rules.push({ rule_set: ["geosite-category-pt"], outbound: "DIRECT" });

      sbConfig.route.rules.push({ rule_set: ["geosite-youtube"], outbound: "🎬 YouTube" });
      sbConfig.route.rules.push({ rule_set: ["geosite-spotify"], outbound: "🎵 Spotify" });
      sbConfig.route.rules.push({ rule_set: ["geosite-steam"], outbound: "🎮 Steam" });
      sbConfig.route.rules.push({ rule_set: ["geosite-epicgames"], outbound: "🎮 Epic" });
      sbConfig.route.rules.push({ rule_set: ["geosite-openai"], outbound: "🤖 OpenAI" });
      sbConfig.route.rules.push({ rule_set: ["geosite-microsoft"], outbound: "🪟 Microsoft" });
      sbConfig.route.rules.push({ rule_set: ["geosite-telegram", "geoip-telegram"], outbound: "✈️ Telegram" });

      sbConfig.route.rules.push({ rule_set: ["geosite-cn", "geoip-cn"], outbound: "DIRECT" });

      // 返回结果并忽略 undefined 属性 (完美复刻 [JsonIgnore])
      return new Response(JSON.stringify(sbConfig, null, 2), {
        headers: {
          "Content-Type": "application/json; charset=utf-8",
          "Access-Control-Allow-Origin": "*"
        }
      });

    } catch (error: any) {
      return new Response(JSON.stringify({ error: `转换失败: ${error.message}` }), { 
        status: 500,
        headers: { "Content-Type": "application/json; charset=utf-8", "Access-Control-Allow-Origin": "*" }
      });
    }
  }
};