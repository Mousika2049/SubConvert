# SubConvert

将 Clash YAML（当前默认读取 `1.yaml`）转换为 sing-box `config.json` 的命令行工具。

## 功能概览

- 解析 `proxies` 节点，生成 sing-box 出站配置（目前支持 `trojan`、`vless`）
- 按节点名称自动聚合地区分组（香港/狮城/日本/美国）
- 生成主代理组与常用服务分流组（YouTube、Spotify、Steam、AI、Microsoft、Telegram）
- 自动写入 DNS、路由规则集（geosite/geoip 远程 rule-set）
- 根据平台生成差异化配置（Windows / Android / Linux）

## 输入与输出

- **输入文件**：`1.yaml`（项目根目录）
- **输出文件**：`config.json`（项目根目录）

## 使用方式

1. 准备好 `1.yaml`（包含 Clash 格式 `proxies` 字段）。
2. 运行：

```bash
dotnet run
```

3. 按提示选择目标平台：
   - `1` Windows
   - `2` Android
   - `3` Linux

生成成功后会在根目录输出 `config.json`。

## 说明

- Android 配置默认不包含 Clash API 字段。
- 若找不到 `1.yaml` 或 YAML 解析失败，程序会直接输出错误并结束。
