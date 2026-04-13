import { parse } from 'yaml';

// ==================== 辅助函数 ====================
function isIpAddress(host: string): boolean {
  const ipv4 = /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/;
  const ipv6 = /^[a-fA-F0-9:]+$/;
  return ipv4.test(host) || ipv6.test(host);
}

// 使用 Web Crypto API 生成前 8 位 SHA256 哈希，完全复刻 C# GetContentHash
async function getHash(content: string): Promise<string> {
  const msgBuffer = new TextEncoder().encode(content);
  const hashBuffer = await crypto.subtle.digest('SHA-256', msgBuffer);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  return hashHex.slice(0, 8);
}

// 改为使用 Fastly CDN 加速
function createRemoteRuleSet(tag: string, repoType: string, fileName: string) {
  return {
    tag: tag,
    type: "remote",
    format: "binary",
    url: `https://fastly.jsdelivr.net/gh/SagerNet/sing-${repoType}@rule-set/${fileName}.srs`,
    download_detour: "DIRECT",
    update_interval: "1d"
  };
}

// ==================== 核心逻辑 ====================
export default {
  async fetch(request: Request): Promise<Response> {
    if (request.method === 'OPTIONS') {
      return new Response(null, {
        headers: {
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "POST, OPTIONS",
          "Access-Control-Allow-Headers": "Content-Type"
        }
      });
    }
    if (request.method !== 'POST') {
      return new Response("请使用 POST 方法发送 YAML 配置", { status: 400 });
    }

    // 1. 从 URL 获取平台参数
    const url = new URL(request.url);
    const platform = url.searchParams.get('platform') || 'windows';
    const isAndroid = platform === 'android';

    try {
      const yamlContent = await request.text();
      const clashConfig: any = parse(yamlContent);

      if (!clashConfig) throw new Error("YAML 解析为空");

      const configHashId = await getHash(yamlContent);

      // ========== 2. 初始化核心框架 ==========
      const sbConfig: any = {
        log: { level: "warn", timestamp: true },
        dns: { servers: [], rules: [], final: "remote", strategy: "ipv4_only", reverse_mapping: true },
        inbounds: [],
        outbounds: [],
        route: { rule_set: [], rules: [], final: "🚀 PROXIES", auto_detect_interface: true },
        // 根据平台动态写入 Experimental
        experimental: {
          cache_file: { enabled: true, path: "cache.db", cache_id: configHashId },
          ...(isAndroid ? {} : {
            clash_api: { external_controller: "127.0.0.1:9999", external_ui: "ui", secret: "127001" }
          })
        }
      };

      // ========== 3. Inbounds ==========
      sbConfig.inbounds.push({
        type: "tun", tag: "tun-in", address: ["172.19.0.1/30", "fd00::1/126"],
        auto_route: true, strict_route: true, stack: "system", mtu: 1350
      });
      sbConfig.inbounds.push({
        type: "mixed", tag: "mixed-in", listen: "127.0.0.1", listen_port: 8848
      });

      // ========== 4. 节点提取 (Outbounds) ==========
      const directOutbound = { type: "direct", tag: "DIRECT", domain_resolver: "local" };
      const proxyServerDomains = new Set<string>();
      const allNodeNames: string[] = [];
      const nodeOutbounds: any[] = [];

      if (clashConfig.proxies) {
        for (const p of clashConfig.proxies) {
          if (p.type === "trojan") {
            const name = String(p.name);
            const server = String(p.server);
            allNodeNames.push(name);

            if (!isIpAddress(server)) proxyServerDomains.add(server);
            
            nodeOutbounds.push({
              type: "trojan",
              tag: name,
              server: server,
              server_port: Number(p.port),
              password: String(p.password),
              domain_resolver: "node-resolver",
              tcp_fast_open: true,
              connect_timeout: "5s",
              tls: {
                enabled: true,
                server_name: p.sni ? String(p.sni) : server,
                insecure: p['skip-cert-verify'] === true || String(p['skip-cert-verify']).toLowerCase() === "true",
                utls: { enabled: true, fingerprint: "chrome" },
                alpn: ["h2", "http/1.1"],
                min_version: "1.3"
              }
            });
          }
        }
      }

      // ========== 5. 地区组 (Regex Mapping) ==========
      const regionMappings = [
        { name: "🇭🇰 香港", regex: /香港|hong\s?kong|深港|🇭🇰|(?<![a-zA-Z])hkg?\d*(?![a-zA-Z])/i },
        { name: "🇸🇬 狮城", regex: /狮城|新加坡|singapore|🇸🇬|(?<![a-zA-Z])sgp?\d*(?![a-zA-Z])/i },
        { name: "🇯🇵 日本", regex: /日本|japan|tokyo|东京|大阪|🇯🇵|(?<![a-zA-Z])jpn?\d*(?![a-zA-Z])/i },
        { name: "🇺🇸 美国", regex: /美国|america|洛杉矶|硅谷|🇺🇸|(?<![a-zA-Z])usa?\d*(?![a-zA-Z])/i },
        { name: "🇹🇼 台湾", regex: /台湾|taiwan|taipei|台北|🇹🇼|(?<![a-zA-Z])tw\d*(?![a-zA-Z])/i }
      ];

      const finalRegionGroupNames: string[] = [];
      const regionOutbounds: any[] = [];

      for (const region of regionMappings) {
        const matchedNodes = allNodeNames.filter(name => region.regex.test(name));
        if (matchedNodes.length >= 2) {
          finalRegionGroupNames.push(region.name);
          regionOutbounds.push({
            type: "selector",
            tag: region.name,
            outbounds: matchedNodes,
            default: matchedNodes[0],
            interrupt_exist_connections: true
          });
        }
      }

      // ========== 6. 业务与主代理组 ==========
      const mainProxyGroup = "🚀 PROXIES";
      const mainGroupOptions = [...finalRegionGroupNames, ...allNodeNames, "DIRECT"];
      
      const mainOutbounds = [{
        type: "selector", tag: mainProxyGroup, outbounds: mainGroupOptions,
        default: finalRegionGroupNames[0] || allNodeNames[0] || "DIRECT",
        interrupt_exist_connections: true
      }];

      const serviceGroupOptions = [mainProxyGroup, ...finalRegionGroupNames, ...allNodeNames, "DIRECT"];
      
      const usGroup = finalRegionGroupNames.find(n => n.includes("🇺🇸")) || mainProxyGroup;
      const sgGroup = finalRegionGroupNames.find(n => n.includes("🇸🇬")) || mainProxyGroup;
      const hkGroup = finalRegionGroupNames.find(n => n.includes("🇭🇰")) || mainProxyGroup;

      const specialGroups = [
        { name: "🎬 YouTube", default: mainProxyGroup },
        { name: "🎵 Spotify", default: mainProxyGroup },
        { name: "🎮 Steam", default: hkGroup },
        { name: "🤖 AI", default: usGroup },
        { name: "🪟 Microsoft", default: hkGroup },
        { name: "✈️ Telegram", default: sgGroup },
      ];

      const serviceOutbounds = specialGroups.map(g => ({
        type: "selector", tag: g.name, outbounds: serviceGroupOptions, default: g.default, interrupt_exist_connections: true
      }));

      // [核心：排序整合 Outbounds]
      sbConfig.outbounds = [
        ...mainOutbounds,
        ...regionOutbounds,
        ...serviceOutbounds,
        ...nodeOutbounds,
        directOutbound
      ];

      // ========== 7. DNS 树状结构 ==========
      sbConfig.dns.servers = [
        { tag: "bootstrap", type: "local" },
        { tag: "node-resolver", type: "https", server: "223.5.5.5" },
        { tag: "remote", type: "https", server: "1.1.1.1", detour: mainProxyGroup },
        { tag: "local", type: "https", server: "223.5.5.5" }
      ];

      sbConfig.dns.rules.push({ query_type: ["AAAA", "HTTPS", "SVCB"], action: "predefined", rcode: "NOERROR" });
      sbConfig.dns.rules.push({ rule_set: ["geosite-category-ads-all"], action: "predefined", rcode: "NOERROR" });
      
      if (proxyServerDomains.size > 0) {
        sbConfig.dns.rules.push({ domain: Array.from(proxyServerDomains), action: "route", server: "node-resolver" });
      }
      sbConfig.dns.rules.push({ rule_set: ["geosite-cn", "geosite-category-pt"], action: "route", server: "local" });

      // ========== 8. Route 与规则集 ==========
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-category-ads-all", "geosite", "geosite-category-ads-all"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-category-pt", "geosite", "geosite-category-pt"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-cn", "geosite", "geosite-cn"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geoip-cn", "geoip", "geoip-cn"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-youtube", "geosite", "geosite-youtube"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-spotify", "geosite", "geosite-spotify"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-steam", "geosite", "geosite-steam"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-category-ai-!cn", "geosite", "geosite-category-ai-!cn"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-microsoft", "geosite", "geosite-microsoft"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-telegram", "geosite", "geosite-telegram"));
      sbConfig.route.rule_set.push(createRemoteRuleSet("geoip-telegram", "geoip", "geoip-tg"));

      sbConfig.route.rules = [
        { ip_cidr: ["::/0"], action: "reject" },
        { inbound: ["tun-in", "mixed-in"], port: [53], action: "hijack-dns" },
        { network: ["icmp"], action: "route", outbound: "DIRECT" },
        { ip_is_private: true, action: "route", outbound: "DIRECT" },
        { ip_cidr: ["223.5.5.5/32", "1.1.1.1/32"], action: "route", outbound: "DIRECT" },
        { port: [3478, 3479, 19302, 19303], network: ["udp"], action: "reject" },
        { inbound: ["tun-in", "mixed-in"], port: [443], network: ["udp"], action: "reject" },
        { inbound: ["tun-in", "mixed-in"], action: "sniff", timeout: "300ms" },
        { protocol: ["ssh"], action: "route", outbound: "DIRECT" },
        
        { rule_set: ["geosite-category-ads-all"], action: "reject" },
        { rule_set: ["geosite-youtube"], action: "route", outbound: "🎬 YouTube" },
        { rule_set: ["geosite-spotify"], action: "route", outbound: "🎵 Spotify" },
        { rule_set: ["geosite-steam"], action: "route", outbound: "🎮 Steam" },
        { rule_set: ["geosite-category-ai-!cn"], action: "route", outbound: "🤖 AI" },
        { rule_set: ["geosite-microsoft"], action: "route", outbound: "🪟 Microsoft" },
        { rule_set: ["geosite-telegram"], action: "route", outbound: "✈️ Telegram" },
        
        { rule_set: ["geosite-cn", "geosite-category-pt"], action: "route", outbound: "DIRECT" },
        { inbound: ["mixed-in"], action: "resolve" },
        { rule_set: ["geoip-telegram"], action: "route", outbound: "✈️ Telegram" },
        { rule_set: ["geoip-cn"], action: "route", outbound: "DIRECT" }
      ];

      return new Response(JSON.stringify(sbConfig, null, 2), {
        headers: { "Content-Type": "application/json; charset=utf-8", "Access-Control-Allow-Origin": "*" }
      });

    } catch (error: any) {
      return new Response(JSON.stringify({ error: error.message }), { 
        status: 500, headers: { "Content-Type": "application/json; charset=utf-8", "Access-Control-Allow-Origin": "*" }
      });
    }
  }
};