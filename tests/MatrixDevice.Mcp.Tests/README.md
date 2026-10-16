# 独立 MCP stdio 验收

本工程不引用 MCP server 源码，只引用原 Host 以用正式 `ScriptPackageService.Save` 创建测试 ZIP。官方 `ModelContextProtocol` 精确固定 `[2.2.0]`，协议固定 `2025-11-25`；`JsonSchema.Net` 精确固定 `[9.4.1]`，依赖记录在 `packages.lock.json`。

入口：`dotnet MatrixDevice.Mcp.Tests.dll --dotnet ABS_DOTNET_EXE --server ABS_SERVER_DLL --output ABS_FRESH_DIRECTORY`。三个参数必须分别出现一次。编译不会执行 server；此程序只有获得独立运行批准后才能执行。正式旧测试、solution 与生产代码均不修改。

## 证据边界

- 正例使用官方 `StreamClientTransport` / `McpClient` 接本次唯一 owned `Process` 的真实 OS stdin/stdout，真实启动外部 server，不在同进程调用 handler。这是测试 transport 机制偏差，不是产品 MCP 协议或执行引擎变更。
- `StreamClientTransport.Name` 官方硬编码为 `in-memory-stream`，本测试实际传入外部子进程管道，不能按该名称宣称同进程验收。当前没有 shell/relay 子进程；仅可能 `Kill(false)` 本次 owned Demo server，不能用于任何真实 ADB、模拟器或第三方进程。
- stdout 只有 SDK 一个消费者；其 Stream wrapper 原字节 tee 后再返回给 SDK，stdin 在真实 OS 写入成功后 tee，stderr 独立原字节 pump。`server-start.json` / `owned-process.json` 分别记录实际 PID、native birth、exit、真实 stdin 关闭、stdout EOF、kill、原 I/O Task 与每流 SHA。活跃原流只读固定长度快照中的完整行。
- 每个会话使用 fresh data/import/policy；环境不继承任意凭据，仅复制必要 Windows 路径与临时目录变量，并显式设置 DOTNET_ROOT。
- 正常请求间隔至少 120ms；同键并发仅 3 请求，不占用超过合同允许的 3 个普通通道。此用例不是保留取消通道或速率容量证明。
- SDK DTO 只用官方 `McpJsonUtilities.DefaultOptions` 的专用 `ProtocolJson<T>` 验 Schema；领域 `Evidence.Json` 保持默认行为，explicit-null 负例不删 null。原 stdin 请求 id 关联的实际 `tools/list` result/逐 Tool 立即核同 Schema，所有实际 `tools/call` result 在原进程/流终态后逐 id 核同 Schema，不按最近相同 arguments 猜并发 response。批准的独立业务 Schema、server 实际 Schema、structured/text 一致、UTF-8 body 预算及不完整/额外字段拒绝断言仍保留。
- 参数化 A/B 正例核原 Demo StartApp 返回的不同包名，不把日志、任务状态或返回文本称为节点、截图、Tap 计数回读，也不造不存在的第三设备。另以 A-only policy 拒绝确实存在的 Demo B。
- `mcp-lifecycle.json` 必须给出实际接纳 JobId 的完整集合、所有终态、handler drain、runtime dispose、EOF 和零协议余量；这只补充进程和原流证据，不以布尔字段替代任一失败原件。

## 当前矩阵

14 个顶层 case，包括 14 个官方会话及 12 个独立 raw 拒绝子进程。覆盖九类严格启动拒绝、>1MiB 无换行、半帧 EOF、>1024 UTF-8 字节的 string RPC id；以及六工具/Schema/参数实际 launch、同键并发与冲突、忙拒绝记账、取消与后继隔离、输入/身份/null/重复字段、路径/坏包/坏 hash/冻结源、A-only、缺启动 scope、deadline/活动 job EOF、16 refs、100 operation 与旧键恢复、500 日志/16KiB 中文结果、新进程 epoch。新增 70KiB 已声明但未绑定的 V1 string override 仍须通过 Schema，检查实际 wire 小于 1MiB、业务 arguments 大于 64KiB；拒绝后同 operationId 的合法短值必须为新接纳，最终生命周期集合不能隐藏 job。

RPC-id 复用负例先用官方客户端正常初始化，再用公共 RequestId 显式完成一次只读调用，之后复用同一个 id。必须从原 stdin 确认两次均已发出、stdout 只有首响应，实际 server 自然非零退出并排空，不能把 SDK 本地拒发记为产品通过；公共 API 未发出第二次时列 NOT-RUN。raw 负例不构造正常握手，不能代替官方客户端正例。首个正常会话的握手/tools-list/Schema/capabilities/devices 共同前置失败后，其余正常会话停止创建并标 NOT-RUN，不对同一前置反复启动；独立 raw 启动负例不依赖该前置。

初始化失败保留原 `CreateAsync` Task 状态、完整异常、SDK 公开 completion details 和实际 owned Process/原流证据；SDK 仍是失败创建的唯一 SDK 清理 owner，测试单一 owner 负责真实管道/进程，不重复 SDK dispose。关闭先拒新调用并排空原 tracked calls，再明确关闭真实 stdin 写端；原进程退出、SDK 实际 stdout 读取与 stderr 终态后才 SDK Dispose/Completion。任何未终态均 FAIL、保留资源并熔断后例。raw 子进程只要被 kill、未退出、流 pending 或清理异常，也停止后续所有 raw/正常进程并记 NOT-RUN；自然预期非零退出本身不熔断。oversize 与半帧必须命中各自精确 stderr guard，实际接受字节与写失败独立保留，不把预存 payload 当成全部已发送。

13 类未实现/不可稳定触达覆盖明确写入 `NOT-RUN`，包括目标换代和丢响应、精确接纳取消边界、三 import 的真实饱和取消通道、reparse/复制中换文件、各归档/staging 限额、深 JSON/转义与长错误、崩溃/不读输出、stdio 不可达的第 101 终态、独立 backend 回读、实机所有权竞争、协议不匹配、metadata/参数列表/非 BMP 输出预算、65536 个 lifetime RPC-id hash 的压力边界。大消息实际覆盖仅限独立无换行子例，不能泛称所有输入边界通过。不能把本项目编译或现有 209 项通过补写成这些验收通过。

`cases.jsonl` 与最终 `summary.json` 区分产品、夹具、清理和混合失败；任何失败保留并返回非零。每会话有界：初始化 30s、单调用 30s、终态查询 12s、原 calls drain 10s、关闭 stdin 后原进程退出 15s、必要 owned child kill 后等 5s、stdout EOF/stderr/I/O 各 10s、SDK 关闭 55s + completion 5s。超时不能当原任务终态，未终态资源不释放。外层批准 runner 仍须设置整轮期限并保留所有 owned 终态。没有后台执行授权、GUI/guest/ADB 或产品服务启动。

旧 `r03` 官方 `StdioClientTransport` + relay 的真实 `continuation-r01` 失败原件保留：12 raw 子例通过，首 DTO Schema 夹具拒绝后 SDK 等待约 45s，外层进程 exit -1，relay/lifecycle 缺失，后续业务 NOT-RUN。SDK Windows cmd/tree-kill 兼容问题未被新编译关闭；本修订仅定义改用官方流 transport 的独立所有权路径，尚未运行，不能称自然 EOF 已通过。

## Schema 来源

`ContractSchema.json` 是已审 `MCP_DESIGN.md` JSON bundle 的机械摘录。`ProtocolSchema.json` 是官方 `2025-11-25` Schema 中 Tool、ListToolsResult、CallToolResult 及其 `$defs` 引用闭包的机械摘录，无网络 `$ref` 解析。

- [官方 C# SDK 2.2.0](https://github.com/modelcontextprotocol/csharp-sdk/tree/v2.2.0)
- [官方 stdio transport 源码](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Client/StdioClientTransport.cs)
- [官方协议 Schema](https://github.com/modelcontextprotocol/specification/blob/2025-11-25/schema/2025-11-25/schema.json)
- [JsonSchema.Net 9.4.1](https://www.nuget.org/packages/JsonSchema.Net/9.4.1)

以上是测试实现合同，不是运行通过报告。native picker、物理 GUI、AP042/AP045、实机、完整 RPA、抓包和最终双实例 30 分钟均不由本专项关闭。
