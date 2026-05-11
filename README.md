# SubConvert

将 Clash `proxies` YAML 批量转换为 sing-box `config.json` 的命令行工具，支持单文件本地输出和批量上传 GitHub。

## 当前能力

- 从 GitHub 仓库读取 `subconfigs/*.yaml`
- 仅转换 `trojan`、`vless` 节点（含 TLS / REALITY 字段映射）
- 自动生成地区分组（香港 / 狮城 / 日本 / 美国）与服务分组（Spotify / Steam / AI / Microsoft / Telegram）
- 生成 DNS、路由规则与远程 rule-set（MetaCubeX `geosite`/`geoip`）
- 按平台生成差异配置（Windows / Android / Linux）

## 运行环境

- .NET SDK `10.0`
- NuGet 依赖：`YamlDotNet`

## 运行方式

```bash
dotnet run
```

运行后会交互式选择：

1. 目标平台（Windows / Android / Linux）
2. 机场配置（单个或全部）

## 输入与输出

- **输入来源**：`{owner}/SubConfigHub/subconfigs/*.yaml`
- **单机场模式输出**：项目根目录 `config.json`
- **批量模式输出**：上传到 `{owner}/SubConfigHub/singboxConfigs/{机场名}/{平台}/config.json`

## 配置说明

- Android 平台不会写入 `experimental.clash_api`。
- Windows 的 `external_ui` 为 `ui`，Linux 为 `/etc/sing-box/ui`。
- 当前 `Program.cs` 默认写死了 `owner/token`；如需改为环境变量输入，可启用 `RequireInput("GITHUB_OWNER")` 与 `RequireInput("GITHUB_TOKEN")` 相关代码。
