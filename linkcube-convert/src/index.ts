import { parse } from 'yaml';

// 辅助函数：判断是否为 IP 地址
function isIpAddress(host: string): boolean {
  const ipv4 = /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/;
  const ipv6 = /^[a-fA-F0-9:]+$/;
  return ipv4.test(host) || ipv6.test(host);
}

export default {
  async fetch(request: Request): Promise<Response> {
    // 限制只能通过 POST 方式提交配置
    if (request.method !== 'POST') {
      return new Response("请使用 POST 方法发送 Clash YAML 配置文本", { status: 400 });
    }

    try {
      // 1. 读取输入内容 (替代 C# 的 File.ReadAllText)
      const yamlContent = await request.text();
      
      // 2. 反序列化 YAML (替代 DeserializerBuilder)
      const clashConfig: any = parse(yamlContent);

      if (!clashConfig) {
        throw new Error("YAML 解析为空");
      }

      // 3. 初始化 Singbox 配置对象
      const sbConfig: any = {
        log: { level: "info", timestamp: true },
        dns: { servers: [], rules: [], final: "google", strategy: "ipv4_only" },
        inbounds: [],
        outbounds: [],
        route: { rule_set: [], rules: [], final: "", auto_detect_interface: true, default_domain_resolver: "local" }
      };

      // ========== Inbounds ==========
      sbConfig.inbounds.push({
        type: "tun",
        tag: "tun-in",
        interface_name: "tun0",
        address: ["172.19.0.1/30"],
        auto_route: true,
        strict_route: true,
        stack: "system",
        sniff: true,
        mtu: 1500,
        endpoint_independent_nat: true
      });
      
      if (clashConfig['mixed-port']) {
        sbConfig.inbounds.push({
          type: "mixed",
          tag: "mixed-in",
          listen: "127.0.0.1",
          listen_port: Number(clashConfig['mixed-port']),
          sniff: true
        });
      }

      // ========== Outbounds ==========
      sbConfig.outbounds.push({ type: "direct", tag: "DIRECT" });
      sbConfig.outbounds.push({ type: "block", tag: "BLOCK" });

      const proxyServerDomains = new Set<string>();
      let mainProxyGroup = "Proxies";

      if (clashConfig.proxies) {
        for (const p of clashConfig.proxies) {
          if (p.type === "trojan") {
            const server = p.server;
            if (!isIpAddress(server)) proxyServerDomains.add(server);
            
            sbConfig.outbounds.push({
              type: "trojan",
              tag: p.name,
              server: server,
              server_port: Number(p.port),
              password: p.password,
              tls: {
                enabled: true,
                server_name: p.sni ? String(p.sni) : server,
                insecure: p['skip-cert-verify'] === true || p['skip-cert-verify'] === "true"
              }
            });
          }
        }
      }

      if (clashConfig['proxy-groups']) {
        for (const g of clashConfig['proxy-groups']) {
          if (g.type === "select") {
            sbConfig.outbounds.push({
              type: "selector",
              tag: g.name,
              outbounds: g.proxies || [],
              default: g.proxies && g.proxies.length > 0 ? g.proxies[0] : undefined
            });
            if ((g.name === "Proxies" || g.name === "Proxy") && mainProxyGroup === "Proxies") {
              mainProxyGroup = g.name;
            }
          }
        }
      }

      // ========== DNS & Route 基础配置 ==========
      sbConfig.dns.servers.push({ tag: "google", type: "tcp", server: "8.8.8.8", detour: mainProxyGroup });
      sbConfig.dns.servers.push({ tag: "local", type: "udp", server: "223.5.5.5", server_port: 53 });
      sbConfig.route.final = mainProxyGroup;

      // ========== 关键拦截逻辑修复 (Patches) ==========
      sbConfig.route.rules.push({ port: [53], action: "hijack-dns" });
      sbConfig.route.rules.push({ protocol: ["dns"], action: "hijack-dns" });
      sbConfig.route.rules.push({ port: [443], network: ["udp"], outbound: "BLOCK" });
      sbConfig.route.rules.push({ ip_cidr: ["223.5.5.5/32"], outbound: "DIRECT" });

      // ========== Rule 收集与分类 ==========
      const collectedRules: Record<string, any> = {};
      const remoteGeoIpTags = new Set<string>();

      const privateIpTag = "geoip-private";
      sbConfig.route.rule_set.push({
        tag: privateIpTag,
        type: "inline",
        rules: [{ ip_cidr: ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "fd00::/8"] }]
      });
      sbConfig.route.rules.push({ rule_set: [privateIpTag], outbound: "DIRECT" });

      if (clashConfig.rules) {
        for (const line of clashConfig.rules) {
          if (!line || typeof line !== 'string') continue;
          const parts = line.split(',').map(s => s.trim());
          if (parts.length < 2) continue;

          const type = parts[0].toUpperCase();
          if (type === "MATCH") continue;

          if (parts.length < 3) continue;
          const value = parts[1];
          const target = parts[2];

          if (!collectedRules[target]) {
            collectedRules[target] = { Domain: [], DomainSuffix: [], DomainKeyword: [], IpCidr: [], ProcessName: [] };
          }

          if (type === "GEOIP") {
            const countryCode = value.toLowerCase();
            const ruleSetTag = `geoip-${countryCode}`;
            if (!remoteGeoIpTags.has(ruleSetTag)) {
              sbConfig.route.rule_set.push({
                tag: ruleSetTag,
                type: "remote",
                format: "binary",
                url: `https://raw.githubusercontent.com/SagerNet/sing-geoip/rule-set/geoip-${countryCode}.srs`,
                download_detour: mainProxyGroup
              });
              remoteGeoIpTags.add(ruleSetTag);
            }
            sbConfig.route.rules.push({ rule_set: [ruleSetTag], outbound: target });
            continue;
          }

          switch (type) {
            case "DOMAIN": collectedRules[target].Domain.push(value); break;
            case "DOMAIN-SUFFIX": collectedRules[target].DomainSuffix.push(value); break;
            case "DOMAIN-KEYWORD": collectedRules[target].DomainKeyword.push(value); break;
            case "IP-CIDR":
            case "IP-CIDR6": collectedRules[target].IpCidr.push(value); break;
            case "PROCESS-NAME": collectedRules[target].ProcessName.push(value); break;
          }
        }
      }

      for (const [target, collector] of Object.entries(collectedRules)) {
        // 去重 DomainSuffix
        collector.DomainSuffix = [...new Set(collector.DomainSuffix)]; 

        if (collector.Domain.length === 0 && collector.DomainSuffix.length === 0 &&
            collector.DomainKeyword.length === 0 && collector.IpCidr.length === 0 &&
            collector.ProcessName.length === 0) continue;

        const cleanTarget = target.replace(/ /g, "-").replace(/:/g, "");
        const ruleSetTag = `rs-local-${cleanTarget}`;

        const headlessRule: any = {};
        if (collector.Domain.length > 0) headlessRule.domain = collector.Domain;
        if (collector.DomainSuffix.length > 0) headlessRule.domain_suffix = collector.DomainSuffix;
        if (collector.DomainKeyword.length > 0) headlessRule.domain_keyword = collector.DomainKeyword;
        if (collector.IpCidr.length > 0) headlessRule.ip_cidr = collector.IpCidr;
        if (collector.ProcessName.length > 0) headlessRule.process_name = collector.ProcessName;

        sbConfig.route.rule_set.push({
          type: "inline",
          tag: ruleSetTag,
          rules: [headlessRule]
        });
        sbConfig.route.rules.push({ rule_set: [ruleSetTag], outbound: target });
      }

      // ========== DNS 补充规则 ==========
      if (proxyServerDomains.size > 0) {
        sbConfig.dns.rules.push({ domain: Array.from(proxyServerDomains), server: "local" });
      }

      const directTag = `rs-local-DIRECT`;
      if (sbConfig.route.rule_set.some((r: any) => r.tag === directTag)) {
        sbConfig.dns.rules.push({ rule_set: [directTag], server: "local" });
      }

      // 4. 返回输出 (自动忽略 undefined，格式化输出)
      const responseBody = JSON.stringify(sbConfig, null, 2);

      return new Response(responseBody, {
        headers: {
          "Content-Type": "application/json; charset=utf-8",
          // 允许任何应用跨域调用这个 API
          "Access-Control-Allow-Origin": "*" 
        }
      });

    } catch (error: any) {
      return new Response(JSON.stringify({ error: `转换失败: ${error.message}` }), { 
        status: 500,
        headers: { "Content-Type": "application/json; charset=utf-8" }
      });
    }
  }
};