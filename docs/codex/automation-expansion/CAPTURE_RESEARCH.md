# 指定模拟器抓包方案初步调研

日期：2026-10-16。对应R14/A16。状态：官方资料与源码盘点完成，方案为初步推荐；尚未运行POC、安装组件或启用任何真实流量采集。

## 当前事实与范围

产品ProxyService仅有配置解析/脱敏导出，Start/Stop返回501；tools/Invoke-HttpToolkitMcp.ps1是外部工具的辅助客户端，不是产品MCP server。其readOnlyHint检查不是完整的外部进程或数据授权证明，不能直接当安全抓包入口。现有脚本、截图、节点和HTTP调用也不等于捕获Android应用流量。

已重新阅读雷电公开的2026版ldconsole命令表，未见专用的抓包会话/分析接口；ADB接入和网络事件控制不等于抓包。该结论只覆盖所读公开文档，不能断言全部GUI版本或私有SDK均无能力；本机版本帮助/能力核查继续保留在A16a，禁止试运行未识别命令。[雷电官方命令行说明](https://help.ldmnq.com/docs/LDPlayer-ming-ling-hang-kai-fa-zhe-shi-yong-shuo-ming/n)

目标为Windows WPF控制台中的指定雷电实例/应用，要求采集与runId/目标关联、有限时长容量、可停止、证据可分析与导出。只研究自有或明确授权的应用；不默认抓取全主机、全部实例或真实账号内容。以下选型是工程推断，不是雷电兼容性或实际捕获通过证明。

## 方案比较

| 方案 | 优点 | 限制与前置 | 本轮选择 |
|---|---|---|---|
| 自有调试应用显式代理 + 独立mitmproxy进程 | regular proxy配置简单，HTTP记录可自动分析，容易用专有nonce验证来源；可只改变自有应用配置 | 应用必须遵循代理；绕过代理的请求不覆盖；HTTPS需要该自有调试包信任测试CA；代理地址必须对guest可达且不能全网暴露 | 推荐作为首个隔离POC，不更改系统代理或系统CA |
| HTTP Toolkit Android接入及现有外部MCP客户端 | 有成熟交互式检查、Android转发和外部工具基础 | 官方自动接入可能安装Android辅助应用、尝试系统CA/root及自行启动ADB；现安装/许可/工具Schema、进程/监听隔离仍需实核，不能直接满足本项目共享服务保护 | 保留用户指定工具的条件接入任务，未证明安全前不启用自动ADB interception |
| Android VPNService / mitmproxy WireGuard | 可覆盖不遵循HTTP代理的流量；WireGuard服务器为用户态 | 需要guest客户端/VPN授权和路由可达；Android同一时刻只有一个应用可获VPN权，可能挤掉现VPN；HTTPS信任问题仍独立存在 | 第二阶段备选，不在无人值守时自动接受权限或替换现VPN |
| guest tcpdump + Wireshark/TShark解析 | 标准原始包适合网络层诊断，不要求应用遵循代理 | 需要可信guest工具、可用抓包权限/接口；仅指定设备不等于按应用隔离；TLS/QUIC payload通常仍加密 | 条件备选，验证工具和权限后再纳入PCAP路径 |
| Windows主机原始包采集 | 可看主机网络/虚拟网卡诊断 | 依赖驱动/权限和正确接口；NAT后流量难可靠归属每个guest或App，容易采到其他软件内容 | 不作为默认指定实例/应用方案；不自动安装驱动或启用全主机采集 |

regular、WireGuard和local capture面向的场景不同，Windows主机进程捕获不能直接等同guest应用隔离。[mitmproxy模式说明](https://docs.mitmproxy.org/stable/concepts/modes/)

HTTP Toolkit官方说明其Android自动接入会尝试复用或启动ADB，并可能安装辅助应用、临时注入系统CA；这与本项目“不替换共享ADB、不隐式变更证书”的默认边界存在冲突，需要独立验证而非直接调用。[HTTP Toolkit Android接入](https://httptoolkit.com/docs/guides/android/)

VPN授权与可撤销行为由Android系统管理，同时只能授权一个应用；无root不等于无权限/无影响。[Android VpnService](https://developer.android.com/reference/android/net/VpnService)

自有debug构建可以通过network security config仅信任调试CA；不能从这一能力推断第三方App也接受用户CA或固定证书可以自动绕过。[Android网络安全配置](https://developer.android.com/privacy-and-security/security-config)

tcpdump提供原始包，Wireshark可读取其保存文件；抓到包与HTTP解密/应用来源识别属于不同验收。[Wireshark tcpdump说明](https://www.wireshark.org/docs/wsug_html_chunked/AppToolstcpdump.html)

## 推荐POC与验收门槛

1. 冻结专用实例1的真实身份、ADB owner和网络前快照，准备无账号自有调试包及测试服务；不安装CA、不改路由的只读预检先完成。实例2作为未选目标对照，不启动其他实例。
2. 以独立进程和私有配置启动代理，核实际监听地址/端口/拥有者；若必须放行网络或扩大监听范围，先记录必要性和边界，默认不改防火墙、不暴露到0.0.0.0。无法guest直达时研究专有ADB tunnel，但不得偷偷使用native adb客户端或修改共享转发。
3. 自有应用发送带runId/nonce的GET、POST及确定状态码响应；逐条核服务端记录、采集内容、数量和目标证据。未选实例发不同nonce作为串扰负例。HTTP成功只是第一门，不证明HTTPS或任意App成功。
4. HTTPS仅用自有调试包内测试信任配置验证，记录成功握手与明文请求/响应；第三方/固定证书/QUIC不支持时如实给受限状态，不尝试绕过。若只能元数据，将能力名称及导出类型标清楚。
5. 验证停止后无新记录、5分钟/50MiB限额、断线/身份变更/客户端退出、两会话隔离与进程清理。只有本次创建且所有权一致的代理/转发才可停止或撤销，外部后来修改的配置不得覆盖。
6. 导出真实HAR/后端流记录及摘要，使用匹配格式的解析器重新打开并核数量、nonce和SHA；PCAP仅对应原始包后端。凭据和请求体默认脱敏，原始包、证书私钥、流量/截图不进入Git或源码包，也不默认传给外部AI。

若应用显式代理不可行、来源隔离失败或清理无法保证，应推翻首选并比较VPN/原始包的权限与影响；不是降低验收为“存在网络连接”。没有安全可行方案时将A16保持未完成，并继续其他独立任务，不默默删除抓包要求。

## 开发文档接入与依赖

PRD R14定义业务范围与验收，SPEC“模拟器抓包”定义会话/配额/隐私和生命周期，TASKS A16a负责调查、A16b负责POC后实现；成功产物供A14c动态分析及A15工具契约使用。对比和设计可先行，真实采集必须在目标证明、工具来源与授权条件满足后执行。

“MCP连接”与抓包后端分开验收：工具能够握手/调用，不代表已采到流量；工具返回readOnlyHint也不授予数据权限。首版优先stdio，stdout只传协议消息，业务日志走stderr，按实际SDK协商协议版本。[MCP传输](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports)、[MCP工具](https://modelcontextprotocol.io/specification/2025-11-25/server/tools)

本调研没有运行现有HTTP Toolkit启动脚本，没有启动代理/监听、安装证书、提升权限、修改ADB或操作真实应用。后续POC结果、失败及恢复证据须追加，不将本页视作已实现报告。
