import { parse } from 'yaml';

// ==================== 辅助函数 ====================
function isIpAddress(host: string): boolean {
  const ipv4 = /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/;
  const ipv6 = /^[a-fA-F0-9:]+$/;
  return ipv4.test(host) || ipv6.test(host);
}

function createRemoteRuleSet(tag: string, repoType: string, fileName: string, downloadDetour: string) {
  return {
    tag: tag,
    type: "remote",
    format: "binary",
    url: `https://raw.githubusercontent.com/SagerNet/sing-${repoType}/rule-set/${fileName}.srs`,
    download_detour: downloadDetour,
    update_interval: "1d" // 新增：规则集 1天 更新一次
  };
}

// ==================== 核心逻辑 ====================
export default {
  async fetch(request: Request): Promise<Response> {
    if (request.method !== 'POST') {
      return new Response("请使用 POST 方法发送 Clash YAML 配置", { status: 400 });
    }

    try {
      const yamlContent = await request.text();
      const clashConfig: any = parse(yamlContent);

      if (!clashConfig) throw new Error("YAML 解析为空");

      // ========== 初始化核心框架 ==========
      const sbConfig: any = {
        log: { level: "warn", timestamp: true },
        dns: { servers: [], rules: [], final: "remote", strategy: "ipv4_only", independent_cache: true },
        inbounds: [],
        outbounds: [],
        route: { rule_set: [], rules: [], final: "🚀 PROXIES", auto_detect_interface: true, default_domain_resolver: "local" },
        experimental: {
          cache_file: { enabled: true, path: "cache.db", store_rdrc: true, rdrc_timeout: "7d" },
          clash_api: { external_controller: "127.0.0.1:9999", external_ui: "ui", secret: "314159" }
        }
      };

      // ========== Inbounds (入站) ==========
      sbConfig.inbounds.push({
        type: "tun", tag: "tun-in", interface_name: "tun0", 
        address: ["172.19.0.1/30", "fd00::1/126"],
        auto_route: true, strict_route: true, stack: "system", 
        endpoint_independent_nat: true, mtu: 1350 // 调优 MTU
      });
      
      if (clashConfig['mixed-port']) {
        sbConfig.inbounds.push({
          type: "mixed", tag: "mixed-in", listen: "127.0.0.1", listen_port: Number(clashConfig['mixed-port'])
        });
      }

      // ========== Outbounds (基础节点 & 代理节点) ==========
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
              
              // 连接层优化参数
              tcp_keep_alive: "15s",
              tcp_keep_alive_interval: "15s",
              tcp_fast_open: true,
              connect_timeout: "5s",

              tls: {
                enabled: true,
                server_name: p.sni ? String(p.sni) : server,
                insecure: p['skip-cert-verify'] === true || String(p['skip-cert-verify']).toLowerCase() === "true",
                utls: { enabled: true, fingerprint: "chrome" },
                alpn: ["h2", "http/1.1"],
                min_version: "1.3" // 强制 TLS 1.3
              }
            });
          }
        }
      }

      // ========== 地区策略组提取与 Emoji 注入 ==========
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
            if (mapping.keywords.some(kw => lowerName.includes(kw))) { matchedEmoji = mapping.emoji; break; }
          }

          if (matchedEmoji) {
            let formattedGroupName = g.name;
            if (!formattedGroupName.includes(matchedEmoji)) formattedGroupName = `${matchedEmoji} ${formattedGroupName}`;

            finalRegionGroupNames.push(formattedGroupName);

            const outType = g.type === "select" ? "selector" : "urltest";
            const mappedOutbounds = (g.proxies || []).map((p: string) => p === "REJECT" ? "BLOCK" : p);

            // 如果是 urltest，注入后台容灾测速机制
            regionOutbounds.push({
              type: outType,
              tag: formattedGroupName,
              outbounds: mappedOutbounds,
              default: outType === "selector" && mappedOutbounds.length > 0 ? mappedOutbounds[0] : undefined,
              url: outType === "urltest" ? "https://www.gstatic.com/generate_204" : undefined,
              interval: outType === "urltest" ? "3m" : undefined,
              tolerance: outType === "urltest" ? 50 : undefined,
              idle_timeout: outType === "urltest" ? "30m" : undefined,
              interrupt_exist_connections: true // 热切换核心
            });
          }
        }
      }

      // ========== 业务策略组与排序 ==========
      const mainProxyGroup = "🚀 PROXIES";
      const mainGroupOptions = [...finalRegionGroupNames, "DIRECT", ...allNodeNames];

      const mainProxyOutbound = {
        type: "selector", tag: mainProxyGroup, outbounds: mainGroupOptions,
        default: finalRegionGroupNames[0] || allNodeNames[0],
        interrupt_exist_connections: true
      };

      const customGroupOptions = [mainProxyGroup, ...mainGroupOptions];
      const usGroup = finalRegionGroupNames.find(n => n.includes("🇺🇸")) || mainProxyGroup;
      const sgGroup = finalRegionGroupNames.find(n => n.includes("🇸🇬")) || mainProxyGroup;

      const specialGroups = [
        { name: "🎬 YouTube", default: mainProxyGroup }, { name: "🎵 Spotify", default: mainProxyGroup },
        { name: "🎮 Steam", default: mainProxyGroup }, { name: "🎮 Epic", default: mainProxyGroup },
        { name: "🤖 OpenAI", default: usGroup }, { name: "🪟 Microsoft", default: mainProxyGroup },
        { name: "✈️ Telegram", default: sgGroup }, { name: "📚 Wikipedia", default: mainProxyGroup }
      ];

      const serviceOutbounds = specialGroups.map(g => ({
        type: "selector", tag: g.name, outbounds: customGroupOptions, default: g.default, interrupt_exist_connections: true
      }));

      // [顺序写入 Outbounds]
      sbConfig.outbounds.push(mainProxyOutbound);
      sbConfig.outbounds.push(...regionOutbounds);
      sbConfig.outbounds.push(...serviceOutbounds);

      // ========== DNS 策略 ==========
      sbConfig.dns.servers.push({ tag: "remote", type: "https", server: "1.1.1.1", detour: mainProxyGroup });
      sbConfig.dns.servers.push({ tag: "local", type: "https", server: "223.5.5.5" });

      if (proxyServerDomains.size > 0) {
        sbConfig.dns.rules.push({ domain: Array.from(proxyServerDomains), server: "local" });
      }
      
      // Windows 探测绿灯白名单
      sbConfig.dns.rules.push({ domain_suffix: ["msftconnecttest.com", "msftncsi.com", "wns.windows.com"], server: "local" });
      sbConfig.dns.rules.push({ rule_set: ["geosite-cn", "geosite-category-pt"], server: "local" });

      // ========== Route 规则集 ==========
      sbConfig.route.rule_set.push({
        tag: "geoip-private", type: "inline", 
        // 补充加入了多播和广播地址的阻断
        rules: [{ ip_cidr: ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8", "224.0.0.0/4", "255.255.255.255/32", "ff00::/8"] }]
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
      sbConfig.route.rule_set.push(createRemoteRuleSet("geosite-wikimedia", "geosite", "geosite-wikimedia", mainProxyGroup));

      // ========== Route 路由树 (核心：Fast Path 与 Slow Path 的分水岭) ==========
      sbConfig.route.rules = [
        // 1. 全局拦截与系统规则 (最高优先级)
        { action: "sniff", timeout: "300ms" },
        { port: [53], action: "hijack-dns" },
        { ip_cidr: ["::/0"], action: "reject" },

        // 2. 纯域名匹配阶段 (Fast Path) - 0延迟
        { rule_set: ["geosite-category-ads-all"], action: "reject" },
        { rule_set: ["geosite-youtube"], outbound: "🎬 YouTube" },
        { rule_set: ["geosite-spotify"], outbound: "🎵 Spotify" },
        { rule_set: ["geosite-steam"], outbound: "🎮 Steam" },
        { rule_set: ["geosite-epicgames"], outbound: "🎮 Epic" },
        { rule_set: ["geosite-openai"], outbound: "🤖 OpenAI" },
        { rule_set: ["geosite-microsoft"], outbound: "🪟 Microsoft" },
        { rule_set: ["geosite-telegram"], outbound: "✈️ Telegram" },
        { rule_set: ["geosite-wikimedia"], outbound: "📚 Wikipedia" },
        
        { rule_set: ["geosite-cn", "geosite-category-pt"], outbound: "DIRECT" },

        // 基于端口的物理封杀
        { port: [443], network: ["udp"], action: "reject" }, // 杀 QUIC
        { ip_cidr: ["223.5.5.5/32", "1.1.1.1/32"], outbound: "DIRECT" },

        // 3. 强制 DNS 解析！(分水岭)
        { action: "resolve", strategy: "ipv4_only" },

        // 4. 纯 IP 匹配阶段 (Slow Path) - 依赖解析出的真实 IP
        { rule_set: ["geoip-private", "geoip-cn"], outbound: "DIRECT" },
        { rule_set: ["geoip-telegram"], outbound: "✈️ Telegram" }
      ];

      // ========== 返回响应 ==========
      return new Response(JSON.stringify(sbConfig, null, 2), {
        headers: { "Content-Type": "application/json; charset=utf-8", "Access-Control-Allow-Origin": "*" }
      });

    } catch (error: any) {
      return new Response(JSON.stringify({ error: `转换失败: ${error.message}` }), { 
        status: 500, headers: { "Content-Type": "application/json; charset=utf-8", "Access-Control-Allow-Origin": "*" }
      });
    }
  }
};