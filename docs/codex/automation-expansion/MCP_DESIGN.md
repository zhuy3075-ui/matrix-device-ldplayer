# 群控 MCP 最小接入设计

日期：2026-10-16。对应 PRD R12/R13、TASKS A15a/A15b，后续关联 A14c/A16。基线：`77366305c34c8c7246c2d10f637ef9498b73b366`。

状态：仅设计及官方资料核对，尚未添加 SDK、实现服务端、运行协议客户端或操作真实模拟器。本页不是 MCP 已交付报告，也不改变原完整 GUI、双实例同步、应用安装卸载、最终双实例 30 分钟等未完成验收。

## 1. 背景、目标与事实

目标是让真实 MCP 客户端在明确授权范围内发现目标、导入受限脚本包、提交逐目标任务、读取结果和按 JobId 取消。白话验收是：客户端能实际连接，列出工具，在指定两台设备执行不同任务，读回各自结果；未选目标不被操作，停止某个任务不会停止另一台或同串号的后继任务。

已只读核对 `src/MatrixDevice.Cli/Program.cs`、`src/MatrixDevice.Host/MatrixRuntime.cs`、`Api/ApiServiceManager.cs`、`src/MatrixDevice.Features/Scripts/ScriptPackageService.cs`、`ScriptScheduler.cs`、`ScriptJobs.cs`：

| 现有事实 | 可复用或缺口 |
|---|---|
| CLI 启动会向 stdout 输出设备 JSON，`--serve` 启动原 HTTP API | 不是 MCP；不可原样接到 stdio，否则污染协议流 |
| Runtime 创建目录、后端和 `ScriptScheduler(backend, catalog, demo)`，拥有 Dispose 生命周期 | 复用这些业务对象，不另建执行引擎；新入口默认关闭所有自动 API/接入/同步行为 |
| importer 读取含 `manifest.json`、`script.json` 的 ZIP，保留旧 `.jxscript` 与合法附件兼容 | 可作唯一包解析器；还缺 MCP 授权导入根、冻结文件、哈希与引用存储 |
| `SubmitJob` 同步接纳或拒绝，返回 JobId/Completion；已有逐目标身份和实际队列/发送前 pin | 可作为唯一作业入口，不绕过为直接自由后端调用 |
| Job 查询、日志、真实启动输出、取消和有界历史已存在 | 复用状态，不把普通 job handle 包装成协议持久 Tasks |
| busy、JobId、目录代次及队列是 Runtime/进程内状态 | 另起 MCP Runtime 不会自动与 Desktop 共享互斥或任务列表，这是实机写入前置阻断 |
| 产品抓包尚未接入，外部 HTTP Toolkit 辅助脚本不是产品 MCP server | 首版不注册假的 capture 工具；抓包继续按 [CAPTURE_RESEARCH](CAPTURE_RESEARCH.md) 独立推进 |

根问题不是把 HTTP 路由换个名称，而是在新协议进程边界保住已有目标身份、任务取消和副作用约束，并为真实客户端提供可验证的契约。最小方案是窄 stdio 适配层，而不是通用主机执行器或另一个控制台。

术语：stdio 指客户端子进程的标准输入/输出；scope 指启动时固定的授权目标和动作集合；epoch 指本次服务进程的随机身份；operationId 是业务防重复键，不是 JSON-RPC 请求 id；JobId 是原 Scheduler 产生的逐目标任务身份；artifactRef 是服务端管控的产物引用，不是任意主机路径。

## 2. 范围与方案比较

首版六工具见第 5 节。没有 `all` 或自动唤醒：多目标由客户端对冻结且明确授权的在线集合逐台提交，分别记录接纳/拒绝；单台失败不被伪装成全批事务回滚。“全部授权目标已逐一测试”和“23 路同时容量已通过”是两个不同结论。

不在首版：远程 HTTP/SSE、账户/OAuth 改造、任意 shell/eval/URL 抓取、Python/JavaScript/二进制执行、APK 管理、实例新建/删除/重启、系统代理/CA/VPN/root、自动接受协议或敏感权限、协议 Tasks 扩展、跨进程/重启持久任务、截图/节点/流量原文自动外传。原应用功能不会因此删除。

| 方案 | 复杂度、风险与成本 | 裁决 |
|---|---|---|
| 独立 `MatrixDevice.Mcp` console 引用 Host，官方 SDK stdio，六个显式工具 | 小规模新增适配和策略层；独立客户端可测；实机写入必须先解决第 4 节的单一所有者问题 | 推荐；先 Demo 真协议闭环，实机写能力默认关闭 |
| 仅 capabilities/devices.list/scripts.import | 更简单，几乎无设备写风险，可先核协议/Schema/导入 | 可作 P0 小阶段，不算 A15b 的执行/结果/取消验收完成 |
| CLI 增加严格早分支 | 少一个项目，但必须在任何 Console.WriteLine、用户配置自动行为之前分流，并维护两种输出约束 | 可行备选；当前独立入口更易审查，不为少一个 csproj 混用 stdout |
| MCP 适配层接统一常驻 Host/双方参与的目标 lease | 解决 Desktop 与 MCP 共同写所有权，能进一步显示同一批任务 | 后续条件方案，需独立 spec；不能暗称现有架构已具备 |
| 现 HTTP API 改名 MCP、手写 JSON-RPC、自由 shell 工具、默认所有实例 | 无真实协议证明，扩大副作用和授权面，维护及验证成本高 | 拒绝 |
| 首版即远程 HTTP/OAuth/协议 Tasks/抓包全家桶 | 引入网络、身份、持久生命周期和敏感数据边界，偏离最小闭环 | 拒绝首版实施；需求保留在后续阶段 |

## 3. SDK、协议与兼容矩阵

官方 C# SDK `v2.2.0` release（2026-08-13）已发布，其 tag 项目直接包含 `net10.0;net9.0;net8.0;netstandard2.0`，适合本仓 `net10.0`。这是源码/资料适配结论，尚未完成本仓 restore/build 或 Windows 运行证明。拟精确固定 `ModelContextProtocol` `2.2.0` 及实际解析的依赖 lock，不使用浮动版本，也不引入 `AspNetCore` 或 Tasks 扩展包。[官方 release](https://github.com/modelcontextprotocol/csharp-sdk/releases/tag/v2.2.0)、[精确 tag csproj](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol/ModelContextProtocol.csproj)（已读 2026-10-16）。

使用官方 `AddMcpServer().WithStdioServerTransport()`，显式注册六工具，不用扫描程序集意外暴露未来方法。stdout 只传 MCP 消息，启动失败和日志均写 stderr；未知参数/重复参数/缺必要配置立即失败。现 CLI 普通启动、`--serve` 和旧 stdout 不改。[官方 stdio Hosting 示例](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/docs/concepts/getting-started.md)、[官方 stdio 消息与 EOF 规则](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/stdio)（已读 2026-10-16）。

| 场景 | 固定配置与预期 | 验收状态 |
|---|---|---|
| 首版基准 | 服务端及独立客户端均显式 `ProtocolVersion = "2025-11-25"`；initialize、initialized、tools/list、tools/call | NOT-RUN；作为 A15b 首门 |
| 当前 SDK 默认 | `ProtocolVersion = null` 优先 `2026-07-28`，stdio 先 server/discover；旧版回退行为由 SDK 处理 | 已读官方说明，未运行；不混记为固定旧版初始化通过 |
| 原生新版本 | 双方固定 `2026-07-28`；新逐请求元数据和 server/discover，不期望 initialize | 独立兼容阶段 NOT-RUN；没有通过前不宣称支持 |
| 不匹配/未知版本 | 明确拒绝或 SDK 文档规定的协商，不静默伪造版本 | 必测负例 NOT-RUN |
| HTTP/SSE/Tasks 扩展 | 不启用、不注册、不宣传 | NOT-IMPLEMENTED |

`v2.2.0` 的客户端及服务端 options 均有明确 pin 语义；客户端固定旧版会强制旧握手，不同版本拒绝，服务端固定旧版会拒绝新逐请求协议。默认新旧兼容不等于业务工具、权限和取消已验证。[客户端 tag options](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Client/McpClientOptions.cs)、[服务端 tag options](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Server/McpServerOptions.cs)、[2025-11-25 生命周期](https://modelcontextprotocol.io/specification/2025-11-25/basic/lifecycle)（已读 2026-10-16；最后链接为现 SPEC 采用的旧协议基准）。

## 4. 架构、单一所有者与授权前置

推荐调用链：官方 stdio transport -> 六工具显式 handler -> 固定 scope/epoch/输入/幂等校验 -> 原 importer/compiler 或原 `SubmitJob` -> 原 Runtime 目录与身份队列 -> 原 Demo/ADB 后端。不得从 handler 直接执行 Shell、读取任意文件或构造另一个无目录校验的 Scheduler。

拟新增职责（均尚未实施）：`MatrixDevice.Mcp/Program.cs` 负责严格参数/Host/退出；窄 `McpAccessPolicy` 负责可信启动策略；`McpScriptTools` 负责六工具映射；`ImportedScriptStore` 负责有界冻结包和引用；`OperationLedger` 负责原子防重复。只在出现真实复杂度时拆文件，不为未来插件增加工厂/策略体系。不改 Desktop 导航、旧 HTTP 路由、全局 JSON options 或原 Scheduler 停止所选语义。

启动必须显式选择 `demo` 或 `real`、独立绝对 data-dir、可信策略文件及导入根；不默认加载用户 Desktop 配置，`AutoStartApi=false`，不自动接入未授权目标、不启动实例或同步输入。真实授权还必须将 canonical serial 与受保护索引/雷电进程出生身份绑定；没有可靠映射的 real 目标不可因“看不到 index0”就获准。原实例0及保留目标固定拒绝，即使策略文件误列入也拒绝。可信 scope 不能由任意 tools/call 扩大。

scope 最少区分 `devices.read`、`scripts.import`、`jobs.start`、`jobs.read`、`jobs.cancel`；导入不授予执行，读任务不授予写，句柄不替代每次权限检查。首版固定策略，不提供远程修改授权工具；撤权通过有界退出/重新启动生效，不能先撤权再让旧 handler 继续写。模式来自受信 Runtime/后端能力，绝不接受客户端提交 `isDemo=true` 绕过 real transport。

**真实写入阻断必须显式解决：**独立 MCP、Desktop 或另一个 MCP 实例各自有 busy、队列及代次。独立 data-dir 只隔离文件，并不隔离同一模拟器。仅提醒“请不要同时点 GUI”不足以声称产品互斥。A15b 可先用自有 Demo；real `jobs.start` 保持禁止，直到以下一种受审前置完成并通过竞争负例：

1. 最小独占运行模式：所有可写产品入口（至少 Desktop、CLI/API、MCP）参与同一机器/目标 lease，MCP 拿不到独占所有权即拒绝 real 写；owner 有进程出生身份，不能靠易复用 PID 或覆盖旧 lock 文件抢占。进程退出释放自己的 lease，不杀外部进程。额外一端不参与时不宣称已防住它。
2. 后续统一 Host：只有一个 Runtime 调度真实目标，Desktop 与 MCP 都使用这个可信调度边界。IPC 身份、授权和断线行为另立窄 spec，不把“localhost/命名管道存在”当自动授权。

这些前置均未实现，不纳入本设计文档的实现证明。外部雷电/其他第三方操作者仍不受本产品 lease 控制；真实副作用无法宣称跨所有软件恰好一次。发现共享 ADB owner/会话异常、目录代次/transport/雷电 PID 出生身份变化立即拒绝对应新写，不重建共享 ADB、不盲目重试已发写入。

stdio 不采用 HTTP OAuth 流，但仍需要受信进程启动、最小 OS 文件访问和显式业务 scope；同用户恶意进程不是因此被沙箱隔离。客户端应使用绝对受审可执行文件，禁 shell 拼接，环境只继承必要项，不转发 GitHub/OpenAI 等无关凭据。[官方授权的 stdio/HTTP 区别](https://modelcontextprotocol.io/specification/2026-07-28/basic/authorization)、[SDK 环境继承与 stdio 参数](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/docs/concepts/transports/transports.md)（已读 2026-10-16）。

## 5. 六工具与完整 Schema

以下是设计版本 `matrix-mcp/1` 的业务 Schema；不是已注册工具。工具列表按下列顺序稳定返回，仅宣告实际注册的 tools 能力，不宣告 prompts/resources/subscriptions/Tasks。只读 annotation 是提示，不是授权。所有字符串均为数据，日志、包和节点中的文字不能变成新指令。每个输出都有 `ok/serverEpoch`；标准 MCP 外层仍由 SDK 产生。

| 工具 | inputSchema 定义 | outputSchema 定义 | scope 与行为 |
|---|---|---|---|
| `matrix.capabilities` | CapabilitiesInput | CapabilitiesOutput | 返回本进程模式、epoch、实际授权边界和限制；不连接额外目标 |
| `matrix.devices.list` | DevicesListInput | DevicesListOutput | devices.read；返回授权且已在当前目录内的目标，含离线状态；不扫描网段或启动实例 |
| `matrix.scripts.import` | ScriptImportInput | ScriptImportOutput | scripts.import；冻结并校验 root 内文件，预览计数；无目标写入 |
| `matrix.jobs.start` | JobStartInput | JobStartOutput | jobs.start；一个明确目标，冻结 expectedIdentity，经原 SubmitJob；不产生批次原子性的承诺 |
| `matrix.jobs.get` | JobGetInput | JobGetOutput | jobs.read；仅本进程/授权 scope 拥有的任务，状态/真实结果/有界日志快照 |
| `matrix.jobs.cancel` | JobCancelInput | JobCancelOutput | jobs.cancel；只请求原 JobId 取消，之后 get 到终态才算停止 |

下列单一 JSON Schema bundle 包含全部共享类型和十二个 input/output 定义。每个工具发布的 inputSchema/outputSchema 根必须显式为 `{"type":"object","$ref":"#/$defs/<表中名称>","$defs":<本 bundle 的 $defs>,"$schema":"https://json-schema.org/draft/2020-12/schema"}`，或保留根 `type:"object"` 的等价局部展开；不能只发布悬空的 `$ref`，也不允许在校验时从网络拉取 `$ref`。官方 SDK 2.2.0 的 [Tool.cs](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Protocol/Tool.cs) 的 InputSchema setter 拒绝缺少根 type:object 的 Schema；OutputSchema setter 较宽松，但固定 2025-11-25 的 MCP wire schema 仍要求输出 Schema 根为 object。实现测试须先验证实际 tools/list 符合 MCP wire schema，再用其返回的十二个根 Schema 验证业务实例；仅本文 bundle 校验通过不足以证明可注册或可发现。

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "urn:matrix-device:mcp:1",
  "$defs": {
    "Uuid": { "type": "string", "format": "uuid" },
    "Sha256": { "type": "string", "pattern": "^[0-9a-f]{64}$" },
    "Serial": { "type": "string", "minLength": 1, "maxLength": 256 },
    "Generation": { "type": "string", "pattern": "^[1-9][0-9]{0,18}$" },
    "Transport": { "type": ["integer", "null"], "minimum": 1, "maximum": 4294967295 },
    "ExpectedIdentity": {
      "type": "object", "additionalProperties": false,
      "required": ["generation", "transportId"],
      "properties": {
        "generation": { "$ref": "#/$defs/Generation" },
        "transportId": { "$ref": "#/$defs/Transport" }
      }
    },
    "Target": {
      "type": "object", "additionalProperties": false,
      "required": ["serial", "generation", "transportId"],
      "properties": {
        "serial": { "$ref": "#/$defs/Serial" },
        "generation": { "$ref": "#/$defs/Generation" },
        "transportId": { "$ref": "#/$defs/Transport" }
      }
    },
    "Error": {
      "type": "object", "additionalProperties": false,
      "required": ["code", "message", "errorTruncated", "projectCode", "retryable"],
      "properties": {
        "code": { "enum": ["INVALID_ARGUMENT", "UNAUTHORIZED", "TARGET_OFFLINE", "IDENTITY_CHANGED", "BUSY", "UNKNOWN_REFERENCE", "CAPACITY", "DEADLINE_EXCEEDED", "CANCELLED", "NOT_SUPPORTED", "INTERNAL"] },
        "message": { "type": "string", "maxLength": 2048 }, "errorTruncated": { "type": "boolean" },
        "projectCode": { "type": ["integer", "null"] },
        "retryable": { "const": false },
        "jobId": { "$ref": "#/$defs/Uuid" },
        "operationId": { "$ref": "#/$defs/Uuid" },
        "serial": { "$ref": "#/$defs/Serial" }
      }
    },
    "ErrorOutput": {
      "type": "object", "additionalProperties": false,
      "required": ["ok", "serverEpoch", "error"],
      "properties": {
        "ok": { "const": false }, "serverEpoch": { "$ref": "#/$defs/Uuid" },
        "error": { "$ref": "#/$defs/Error" }
      }
    },
    "Limits": {
      "type": "object", "additionalProperties": false,
      "required": ["requestBytes", "responseBytes", "bodyBytes", "manifestMetadataBytes", "toolConcurrency", "requestsPerSecond", "requestBurst", "activeJobs", "scriptRefs", "stagingBytes", "operationIds", "terminalJobs", "logsPerJob", "logsPerResponse", "resultBytes", "defaultJobTimeoutMs", "maximumJobTimeoutMs", "archiveBytes", "declaredExtractedBytes", "archiveEntries", "jsonEntryBytes", "steps", "parameters"],
      "properties": {
        "requestBytes": { "const": 1048576 }, "responseBytes": { "const": 1048576 },
        "bodyBytes": { "const": 65536 }, "manifestMetadataBytes": { "const": 8192 }, "toolConcurrency": { "const": 4 },
        "requestsPerSecond": { "const": 10 }, "requestBurst": { "const": 20 },
        "activeJobs": { "const": 2 }, "scriptRefs": { "const": 16 },
        "stagingBytes": { "const": 67108864 }, "operationIds": { "const": 100 },
        "terminalJobs": { "const": 100 }, "logsPerJob": { "const": 500 },
        "logsPerResponse": { "const": 100 }, "resultBytes": { "const": 16384 },
        "defaultJobTimeoutMs": { "const": 300000 }, "maximumJobTimeoutMs": { "const": 300000 },
        "archiveBytes": { "const": 16777216 }, "declaredExtractedBytes": { "const": 33554432 },
        "archiveEntries": { "const": 128 }, "jsonEntryBytes": { "const": 4194304 },
        "steps": { "const": 10000 }, "parameters": { "const": 1000 }
      }
    },
    "CapabilitiesInput": { "type": "object", "additionalProperties": false },
    "CapabilitiesOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "contractVersion", "protocolVersion", "mode", "scopes", "authorizedSerials", "realWritesEnabled", "supportedTools", "supportedActions", "unsupportedCapabilities", "limits", "retention"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" },
            "contractVersion": { "const": "matrix-mcp/1" },
            "protocolVersion": { "const": "2025-11-25" }, "mode": { "enum": ["demo", "real"] },
            "scopes": { "type": "array", "uniqueItems": true, "items": { "enum": ["devices.read", "scripts.import", "jobs.start", "jobs.read", "jobs.cancel"] } },
            "authorizedSerials": { "type": "array", "uniqueItems": true, "maxItems": 2, "items": { "$ref": "#/$defs/Serial" } },
            "realWritesEnabled": { "type": "boolean" },
            "supportedTools": { "type": "array", "uniqueItems": true, "maxItems": 6, "items": { "enum": ["matrix.capabilities", "matrix.devices.list", "matrix.scripts.import", "matrix.jobs.start", "matrix.jobs.get", "matrix.jobs.cancel"] } },
            "supportedActions": { "type": "array", "uniqueItems": true, "items": { "enum": ["launch", "launchapp", "wait", "sleep", "click", "swipe"] } },
            "unsupportedCapabilities": { "type": "array", "items": { "type": "string", "maxLength": 256 } },
            "limits": { "$ref": "#/$defs/Limits" },
            "retention": { "const": "process-lifetime; terminal-jobs-oldest-completion-100; no-restart-resume" }
          }
        }
      ]
    },
    "DevicesListInput": { "type": "object", "additionalProperties": false },
    "DevicesListOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "mode", "snapshotUtc", "devices"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" }, "mode": { "enum": ["demo", "real"] },
            "snapshotUtc": { "type": "string", "format": "date-time" },
            "devices": {
              "type": "array", "maxItems": 2,
              "items": {
                "type": "object", "additionalProperties": false,
                "required": ["target", "instanceIndex", "displayName", "online", "writeAllowed"],
                "properties": {
                  "target": { "$ref": "#/$defs/Target" },
                  "instanceIndex": { "type": ["integer", "null"], "minimum": 1 },
                  "displayName": { "type": "string", "maxLength": 256 },
                  "online": { "type": "boolean" }, "writeAllowed": { "type": "boolean" }
                }
              }
            }
          }
        }
      ]
    },
    "ScriptImportInput": {
      "type": "object", "additionalProperties": false,
      "required": ["serverEpoch", "relativePath", "sha256"],
      "properties": {
        "serverEpoch": { "$ref": "#/$defs/Uuid" },
        "relativePath": { "type": "string", "minLength": 1, "maxLength": 4096 },
        "sha256": { "$ref": "#/$defs/Sha256" }
      }
    },
    "ScriptImportOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "scriptRef", "sha256", "packageId", "packageVersion", "displayName", "stepCount", "parameterCount", "bindingVersion", "parameterKeys", "retention"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" },
            "scriptRef": { "$ref": "#/$defs/Uuid" }, "sha256": { "$ref": "#/$defs/Sha256" },
            "packageId": { "type": "string" }, "packageVersion": { "type": "string" }, "displayName": { "type": "string" },
            "stepCount": { "type": "integer", "minimum": 0, "maximum": 10000 },
            "parameterCount": { "type": "integer", "minimum": 0, "maximum": 1000 },
            "bindingVersion": { "enum": [null, 1] },
            "parameterKeys": { "type": "array", "maxItems": 1000, "uniqueItems": true, "items": { "type": "string" } },
            "retention": { "const": "process-lifetime; immutable; no-execution-authority" }
          }
        }
      ]
    },
    "JobStartInput": {
      "type": "object", "additionalProperties": false,
      "required": ["serverEpoch", "operationId", "scriptRef", "serial", "expectedIdentity"],
      "properties": {
        "serverEpoch": { "$ref": "#/$defs/Uuid" }, "operationId": { "$ref": "#/$defs/Uuid" },
        "scriptRef": { "$ref": "#/$defs/Uuid" }, "serial": { "$ref": "#/$defs/Serial" },
        "expectedIdentity": { "$ref": "#/$defs/ExpectedIdentity" },
        "parameterOverrides": { "type": "object", "maxProperties": 1000, "additionalProperties": true },
        "timeoutMs": { "type": "integer", "minimum": 1000, "maximum": 300000, "default": 300000 }
      }
    },
    "JobStartOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "operationId", "jobId", "target", "scriptRef", "sha256", "accepted", "replayed", "timeoutMs"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" },
            "operationId": { "$ref": "#/$defs/Uuid" }, "jobId": { "$ref": "#/$defs/Uuid" },
            "target": { "$ref": "#/$defs/Target" }, "scriptRef": { "$ref": "#/$defs/Uuid" }, "sha256": { "$ref": "#/$defs/Sha256" },
            "accepted": { "const": true }, "replayed": { "type": "boolean" },
            "timeoutMs": { "type": "integer", "minimum": 1000, "maximum": 300000 }
          }
        }
      ]
    },
    "JobState": { "enum": ["Running", "Cancelling", "Completed", "Cancelled", "Failed"] },
    "JobStatus": {
      "type": "object", "additionalProperties": false,
      "required": ["jobId", "operationId", "target", "scriptRef", "sha256", "packageId", "packageVersion", "state", "stepIndex", "stepCount", "startedUtc", "endedUtc", "error", "result", "errorTruncated", "resultTruncatedForResponse", "terminalReason"],
      "properties": {
        "jobId": { "$ref": "#/$defs/Uuid" }, "operationId": { "$ref": "#/$defs/Uuid" },
        "target": { "$ref": "#/$defs/Target" }, "scriptRef": { "$ref": "#/$defs/Uuid" }, "sha256": { "$ref": "#/$defs/Sha256" },
        "packageId": { "type": "string" }, "packageVersion": { "type": "string" }, "state": { "$ref": "#/$defs/JobState" },
        "stepIndex": { "type": "integer", "minimum": -1, "maximum": 9999 },
        "stepCount": { "type": "integer", "minimum": 0, "maximum": 10000 },
        "startedUtc": { "type": "string", "format": "date-time" }, "endedUtc": { "type": ["string", "null"], "format": "date-time" },
        "error": { "type": ["string", "null"], "maxLength": 2048 }, "errorTruncated": { "type": "boolean" },
        "result": { "type": ["string", "null"], "maxLength": 16384 }, "resultTruncatedForResponse": { "type": "boolean" },
        "terminalReason": { "enum": [null, "completed", "failed", "cancelled", "deadline", "shutdown"] }
      }
    },
    "JobGetInput": {
      "type": "object", "additionalProperties": false,
      "required": ["serverEpoch", "jobId"],
      "properties": {
        "serverEpoch": { "$ref": "#/$defs/Uuid" }, "jobId": { "$ref": "#/$defs/Uuid" },
        "logLimit": { "type": "integer", "minimum": 0, "maximum": 100, "default": 50 }
      }
    },
    "JobGetOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "snapshotUtc", "job", "availableLogs", "logWindowLimit", "omittedLogCount", "logs"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" }, "snapshotUtc": { "type": "string", "format": "date-time" },
            "job": { "$ref": "#/$defs/JobStatus" }, "availableLogs": { "type": "integer", "minimum": 0, "maximum": 500 },
            "logWindowLimit": { "const": 500 }, "omittedLogCount": { "type": "integer", "minimum": 0, "maximum": 500 },
            "logs": {
              "type": "array", "maxItems": 100,
              "items": {
                "type": "object", "additionalProperties": false,
                "required": ["timestampUtc", "message", "truncated"],
                "properties": {
                  "timestampUtc": { "type": "string", "format": "date-time" },
                  "message": { "type": "string", "maxLength": 2048 }, "truncated": { "type": "boolean" }
                }
              }
            }
          }
        }
      ]
    },
    "JobCancelInput": {
      "type": "object", "additionalProperties": false,
      "required": ["serverEpoch", "jobId"],
      "properties": { "serverEpoch": { "$ref": "#/$defs/Uuid" }, "jobId": { "$ref": "#/$defs/Uuid" } }
    },
    "JobCancelOutput": {
      "oneOf": [
        { "$ref": "#/$defs/ErrorOutput" },
        {
          "type": "object", "additionalProperties": false,
          "required": ["ok", "serverEpoch", "jobId", "cancelRequested", "state", "terminal"],
          "properties": {
            "ok": { "const": true }, "serverEpoch": { "$ref": "#/$defs/Uuid" }, "jobId": { "$ref": "#/$defs/Uuid" },
            "cancelRequested": { "type": "boolean" }, "state": { "$ref": "#/$defs/JobState" }, "terminal": { "type": "boolean" }
          }
        }
      ]
    }
  }
}
```

Schema 之外仍必须执行的语义校验：UUID 使用规范格式；generation 以 invariant 正整数解析且不超过 `Int64.MaxValue`；real transport 必须正且非 null，Demo 必须 null；serial 按原 canonical 字面身份匹配，不随意 trim/lower；未知或重复参数键、重复 JSON 属性、不可表示数值、过深 JSON 均拒绝。建议 MCP arguments JSON 深度上限 32，数值不经过 double 转换。`parameterOverrides` 缺省与 `{}`等效，但某键显式 null 仍传递为 JsonElement.Null，由原编译器拒绝，不回退默认值。无参数绑定契约且非空 overrides 仍按原行为拒绝。

manifest 不被当作主机代码；支持动作恰为原 launch/launchapp、wait/sleep、click、swipe。`stepIndex` 沿用原零基及开始前 -1，运行状态沿用原五态；当前原引擎不具有 Queued/Preparing，不伪造已实现 SPEC 后续批次状态。单目标的 JobId 可作为后续 A14 批次中的 taskId；跨目标 runId 仍需后续批次层，首版不偷偷重命名成已完成批次引擎。

outputSchema 必须覆盖成功和业务错误；正常返回 `isError=false`，ErrorOutput 返回 `isError=true`。协议格式/未知工具由 SDK 标准 JSON-RPC 错误处理；输入值或业务问题给可理解的工具错误，不靠异常堆栈。官方工具规范要求校验、访问控制和限流；结构化结果应同时有序列化文本便于兼容。[2025-11-25 工具](https://modelcontextprotocol.io/specification/2025-11-25/server/tools)、[2026-07-28 工具及错误/句柄说明](https://modelcontextprotocol.io/specification/2026-07-28/server/tools)（已读 2026-10-16）。

## 6. 错误、幂等与不明结果

| code | 情况 | 处理 |
|---|---|---|
| INVALID_ARGUMENT | 错误 epoch 格式、参数、ZIP/hash、Int64、重复属性等 | 无接纳、无目标写；包含安全的 key/field/非法值摘要，敏感值脱敏 |
| UNAUTHORIZED | 缺 scope、目标未授权、原0/保留目标、真实写前置未满足 | 不隐式授权或启动目标 |
| TARGET_OFFLINE / IDENTITY_CHANGED | 当前离线或 generation/transport/实例证明变化 | 保留期望和实际身份摘要；不将同串号新连接当旧目标 |
| BUSY | 原 Scheduler 同目标占用、写入协调冲突 | 不产生新任务、不清旧日志；不排入隐藏的等待重试 |
| UNKNOWN_REFERENCE | 未知/旧 epoch/已淘汰 scriptRef 或 jobId | 不按 serial fallback，也不创建代替任务 |
| CAPACITY | 消息/导入/并发/幂等/产物/输出限额 | 显式拒绝，不删活跃任务或丢防重记录腾位置 |
| DEADLINE_EXCEEDED / CANCELLED | 接纳前已超时/取消 | 不接纳；接纳后以 job 状态和 terminalReason 回传，不伪称外部写未生效 |
| NOT_SUPPORTED / INTERNAL | 未具备能力或非预期错误 | 受限原因或脱敏关联 id；完整本机诊断不送 stdout |

projectCode 仅在确有原 `DeviceException` 时保留原值，例如 1007 参数、1003 身份/busy、2001 不支持；纯 MCP 策略错误为 null，不发明原工程错误码。`retryable:false` 表示禁止客户端自动重放写；用户纠正输入后可显式新操作。

operationId 在 `(serverEpoch, trustedScopeOwner, operationId)` 下原子登记。scopeOwner 来源于进程启动策略，不接受工具自报。规范请求摘要包含 scriptRef 对应冻结 ZIP SHA256、serial/expectedIdentity、参数 JSON、实际 timeout 和契约版本；object 属性排序，拒绝重复键，保留 null/缺省区别和整数精度，不借 double 归一化。

1. 先做输入、请求者当前 scope/epoch、冻结导入引用校验并生成规范摘要，再进入短的防重/接纳串行临界区；先查既有键并核对原冻结身份及摘要，早于仅适用于新键的编译、当前在线/强 pin 和活动 job/100 键等接纳校验。已接纳旧键通过请求者 scope/epoch、原冻结身份及摘要校验即可返回原 JobId/接纳结果；目标当前离线或身份变化不阻止恢复，恢复不得新增设备写。只有新键提交 SubmitJob 才须通过当前在线强 pin；不在 ledger 锁中等待 Completion 或异步设备操作。
2. 相同 id/相同摘要并发到达，只允许一次 SubmitJob；其他请求读同一接纳结果，即使活动 job 或 100 键已满也返回原接纳身份。相同 id/不同摘要拒绝 INVALID_ARGUMENT（冲突），不执行第二份。
3. 只有新键在同一临界区原子预留 ledger 名额、检查接纳容量并调用原 SubmitJob；接纳结果原子记入 JobId/冻结目标，相同请求返回原 JobId，`replayed=true`。接纳前原 Scheduler 或活动 job 容量拒绝也存该操作的拒绝结果，不能对同键忙状态自动重试；100 键已满时拒绝新键且不再登记。下一次显式尝试使用新 id。
4. 若请求在临界接纳过程中取消，已接纳结果仍留 ledger；客户端只能在同 epoch 用同 operationId 取原结果，不凭网络响应丢失判断未执行。JSON-RPC request id 变化不改变业务幂等键。
5. 首版最多 100 个 operationId，包含接纳和已登记拒绝；不在进程存活期间淘汰防重记录。满额只拒绝新 operationId，旧键重放须先返回已保存结果，jobs.get/cancel 仍可用。规范化请求摘要只保存 hash，避免累积任意输入正文。
6. 每键最多接纳一个 job，故本入口每进程最多 100 个 job，不可能产生第 101 个终态而触发原 Scheduler 的历史淘汰。该淘汰及淘汰后引用分支在本契约下为 stdio 不可达，只能由原 Scheduler 独立测试证明；不以 stdio 的 100 键满额拒绝冒充引擎淘汰验证。
7. 重启产生新 UUIDv4 epoch，旧 epoch 写请求一律拒绝。没有跨重启持久防重或执行恢复；丢响应后跨重启自动创建新 id/重新执行不安全，必须先人工/受控核验实际副作用。

幂等只保证本适配层不重复接纳同键，不保证设备/应用外部业务恰好一次；动作已发出后超时、断线或进程崩溃可形成结果不明，不允许据通用协议“可重试”建议盲重发。

## 7. 请求取消、作业取消与退出

协议请求取消与原 job 生命周期分开。handler 收到请求 CancellationToken 后在接纳前检查；SubmitJob 使用服务器拥有的作业令牌（服务寿命 + 指定 job deadline），不把 tools/call 的请求令牌链接到已接纳 job。start 返回只是接纳，不 await 整个脚本，也不假称协议 Tasks 或后台持久任务。

取消请求、接纳和 ledger 写入之间需要同一有序门：未接纳的取消不产生任务；已接纳但请求响应被取消时，JobId 仍可按原键恢复。测试必须覆盖门两侧，而不是仅检查 token 参数被传递。应用 `jobs.cancel` 调用原 CancelJob，不跟随当前勾选设备，也不取消整个服务器。重复取消运行中的 Cancelling 返回当前状态；已终态任务可返回 `cancelRequested=false/terminal=true`；未知或淘汰 id 返回 UNKNOWN_REFERENCE，不能用 false 掩盖未知。

deadline 是服务器发出的取消原因，不保证正在发送的外部写入可撤销；原任务最终仍用 Cancelled/Failed/Completed，额外 terminalReason 记录 deadline/shutdown/cancelled 等本层原因。若完成与 deadline 竞争，必须以实际原终态与令牌生效次序记录，不把完成强改为失败或反之。取消停止后续未提交步骤，不回滚已生效操作。

实际 EOF 生命周期必须在后续实现证明：官方 transport 读到 stdin EOF -> 停止新接纳 -> 对本服务器拥有的活动 JobId 请求取消 -> 等待其 Completion 和 handler/导入工作 -> Dispose Runtime -> 关闭自有日志及 staging handle -> 退出。建议正常清理预算 10 秒；客户端再等待 5 秒，仅终止自己的 child，随后 bounded drain stdout/stderr 各流共同 10 秒并记录未排空/强杀为失败。不得用 kill ADB/所有 dotnet/所有模拟器达成退出。

若 SDK Hosting 的 EOF 行为没有触发业务 drain，必须补窄 owned-lifetime wiring 并复验，不能仅援引文档算通过。任何 timeout wrapper 都须跟踪原 task，不能后台遗留仍有可能接纳或写入的工作。退出遇到未知外部写结果或 drain 失败保留失败证据，不自动恢复输入、删除实例或重放脚本。官方 stdio 的 EOF 退出与取消规则仅是协议依据，不是本项目生命周期已满足证明。[官方 stdio](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/stdio)（已读 2026-10-16）。

## 8. 导入冻结、路径与数据边界

MCP 不上传脚本到手机；本版 `scripts.import` 是将客户端预先放入受信本机导入根的包读入本机 Scheduler。没有任意 path、URL fetch、环境变量展开或 base64 巨型参数。相对路径语义必须拒绝绝对/UNC/设备路径、驱动器/ADS、`..`/`.`、空或混淆段；不做 URL decode 后再打开另一个路径。

实现不能仅 `GetFullPath().StartsWith(root)`：必须有目录边界比较、拒绝根和路径任一层 reparse/junction/symlink，采用受审单次打开方式和可信导入根 ACL 防止竞态。根目录由操作者建立，不能由工具参数创建或扩大；若无法可靠证明路径/文件 handle 最终身份，拒绝该导入而非退回任意 File.ReadAllBytes。此为待实现/待负测安全前置，不声称现 importer 已解决 NTFS TOCTOU。

单次打开源并在共享策略下复制到本进程自有 staging，复制实际字节限 16MiB，同时算 SHA256，与输入 hash 比较；原路径后续变化不影响已冻结包。staging 路径由服务端产生，不来自 manifest；原 importer 只读冻结副本。引用在 ZIP 与 manifest/script 校验、输出预算检查全部成功后才登记，失败清理只针对本进程创建且所有权一致的临时文件。复用原 parser，不手工解压附件到目标目录，不把合法附件当脚本执行；清理不触碰源包。

原 importer 限额保持：压缩文件 16MiB；声明解压总量 32MiB；128 entries；单 JSON 实际读取 4MiB；10000 steps；1000 parameters；重复和越界 entry 名规则不放宽。声明限额不等于全面解压沙箱证明；后续如真要解压附件，需另验实际解压计数/链接/嵌套，而非沿用这页宣称安全。合法顶层 JSON/Python/JS 仍不支持，扩展名不是 ZIP 内容证明。

脚本参数值按原 V1 编译语义处理，禁止模板 eval、反射或任意代码；覆盖之前和之后校验都不能省。导入可保存完整不变包，启动时再冻结参数后的执行快照；SHA 指源 ZIP 字节，不声称等于参数后步骤 hash。operation 摘要另外绑定参数与 timeout。包/日志/结果可能含敏感数据，默认只读本地授权范围；不自动发送外部 AI 或远程服务，接入具体 AI 客户端前须明确其数据去向。截图/流量原文不进入初版结果。

## 9. 容量、状态和结果真实性

第 5 节 Limits 是拟固定首版限额，不是现有产品全局限额；不得为了 MCP 改坏旧 CLI/GUI 合法包行为。首轮最多两个明确授权目标是验证/暴露范围上限，不是23台容量结论；以后扩大须新版本策略与测试，不隐式放宽。

请求和响应整条 UTF-8 MCP 消息各 1MiB；业务 JSON body 64KiB，structuredContent 和兼容 text 的双份开销需计入整条响应。并发 handler 总数仍为4：普通工具最多3，另1仅保留给 matrix.jobs.cancel，普通工具不得借用；各通道满时立即返回有界 BUSY，不排无限等待队列。令牌桶总额度仍为平均10请求/秒、突发20，须在此额度内保留取消准入；SDK/transport 调度层也不得先被普通工具占满并发门、等待队列或令牌预算而堵住取消通道。start 接纳门串行，活动 job 最多2且每目标沿用原互斥；新 start 的容量检查不阻挡旧键恢复和必要查询/取消。消息大小检查需覆盖 transport 反序列化前，若 SDK 没有现成设置应加窄有界 stream/line 边界并测，不能读入无限行后才数长度。

scriptRef 最多16，冻结 staging 总实际字节64MiB；引用活到本进程退出且不可变。原包在该预算内共享/复用可按 SHA 精确去重，但不因引用重用而自动执行。operationId 最多100，不在寿命内淘汰；已满仍允许既有 id 重放、jobs.get/cancel 和能力查询。不能删除 active job 腾空间。

原 Scheduler 最多保留100个**终态**，按完成次序淘汰，活动任务不淘汰；第101终态淘汰由原 Scheduler 独立测试验证，本 stdio 入口因100键寿命上限不可达。每 job 最近500条日志，Result 最大16KiB UTF-8，并有原显式截断标记。已有 Result 是真实 StartApp 返回文本按步骤累积；无输出任务可为“已完成”，失败后此前输出仍保留。启动成功输出不是 App 业务成功证明，不能只返回“已完成”替代真实输出。

jobs.get 默认为50条、最多100条最新日志，availableLogs 仅表示这次原500窗口的可用数；omittedLogCount 是此次因请求/响应预算省略的窗口内条数，不谎称累计总数或提供不存在的稳定序号/cursor。每条安全呈现文本最多2048字符并标 truncated；总输出超限时先减日志数量，保留原日志在本机。不按时间或文本去重。Result 保持原16KiB及原截断标记，不把 Error 覆盖 Result，两者独立呈现。

UTF-8原文长度不等于JSON转义后的长度，例如16KiB控制字符转义可超过64KiB。故导入还须限制 manifest 的 packageId/packageVersion/displayName 合计序列化元信息不超过8KiB，预留固定状态字段预算；输出按实际JSON序列化字节计数，业务 body 不超过64KiB，含 structuredContent/兼容 text 双份、JSON转义及协议外层的整条响应不超过1MiB。先减少日志，仍超额时只对本次Result呈现按Unicode边界收短，追加“[MCP响应预算截断]”，并设置 resultTruncatedForResponse=true；不改Scheduler中原Result。此标志不假称能反推Scheduler内部是否曾截断，原有输出截断标记仍保留。Error.message、JobStatus.error 和每条日志的2048字符上限指 Unicode 码点，包含截断标记，且仍受上述实际序列化 UTF-8 总预算约束；收短不能切开码点。通用错误必须返回 ErrorOutput.error.errorTruncated，作业错误必须返回 JobStatus.errorTruncated；任何因字符或响应预算收短均为 true，未收短为 false，不能在 additionalProperties:false 的对象外临时添加标志。上述长错误、转义/元信息/双份结果膨胀均为必测容量反例。

导入元信息超过8KiB或完整参数 key 列表无法放进64KiB返回，明确 CAPACITY 且不登记引用；这是新 MCP 输出预算拒绝，不表示原本机合法 ZIP 已变非法。默认 job deadline300000ms，客户端可在1000..300000ms内指定；不改原 wait 的 Int32 兼容范围，只在本 MCP 生命周期发有原因的取消。其他工具建议30秒预算；原同步 importer 不支持即时取消，工作必须有界、被跟踪且在取消/过期后不能登记引用，退出需等待或记录未清理失败，不能声称可瞬间中断所有 I/O。

## 10. 分阶段实施、迁移与验收矩阵

当前仅 A15a 设计；以下阶段需要单独授权/隔离 worktree/TDD/独立 verifier，不能把本文存在标为 MCP 功能完成。A08 自动化页响应式与原 GUI 修复继续独立，不混入同一功能提交。

| 优先级/阶段 | 工作与依赖 | 可观察通过条件 | 当前状态 |
|---|---|---|---|
| P0 / A15a 设计准入 | 本契约、版本、权限、Schema、单一所有者阻断独立审查 | 每工具边界/错误/取消/重启/容量有反例，引用可追溯，无未实现冒领 | 待独立审查 |
| P0 / A15b-1 协议外壳 | SDK精确锁、独立入口、Demo、read-only工具 | 真子进程旧版初始化、tools/list、空/坏参数、未知工具、stdout纯协议、EOF退出；不启动HTTP或实例 | NOT-IMPLEMENTED / NOT-RUN |
| P1 / A15b-2 六工具 Demo 闭环 | 原 importer/Compiler/SubmitJob + 冻结store/ledger | 实际文件导入、A/B不同动作与参数、真实Demo回读、结果、JobId取消、第三目标0调用 | NOT-IMPLEMENTED / NOT-RUN |
| P1 / A15b-3 边界与独审 | 并发/取消门/额度/目录/重启/生命周期 | 下表 stdio 可达负例真实经过stdio；明确不可达的引擎分支另留原 Scheduler 独立测试证据，原Core209及VM语义不回退，独立重建与日志原件齐全 | NOT-RUN |
| P1 / A15b-4 real 写准入 | 第4节单一所有者方案另实施并审；当前目标证明/用户范围仍有效 | 两产品进程竞争明确拒绝，授权实例1/2身份和ADB owner记录有效，无0/其余副作用 | BLOCKED：所有者前置未实现 |
| P1 / A15b-5 两实例实机 | 当前单一所有者与受信包，非账号画面，明确允许的实例1/2 | 逐实例真实执行/回读/取消、差异化并行不串扰；目标断线/身份变化拒绝且共享ADB不变 | NOT-RUN；不继承旧Live通过 |
| P2 / 兼容及扩大目标 | 精确2026-07-28客户端/服务端；额外目标显式授权 | 新启动流程独立原件，所有授权目标逐台覆盖；N路容量另测 | NOT-RUN |
| P3 / A16 + A14c | 抓包POC、产物读取及隐私契约先过 | 同job/目标/包关联的真实采集、停止、分析、脱敏与证据回读 | NOT-IMPLEMENTED；见独立调研 |

最小独立客户端验收必须由另外的测试程序/进程，使用官方 `StdioClientTransport` 启动自有受审 server。不得以同进程直接 handler 调用或内存 pipe 替代 stdio 外部进程证据；这些只能作补充单元测试。精确可执行路径与分开的参数、最小环境、fresh data/staging/导入目录、明确 Demo A/B/未选C，服务无 Window、API端口或后台实例唤醒。官方客户端具备列工具和调用接口，但本项目尚未执行它们。[官方客户端示例](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/docs/concepts/getting-started.md)（已读 2026-10-16）。

| 类别 | 独立客户端用例与断言 | 必留证据 |
|---|---|---|
| N / 协议与正常 | pin旧版连接；六工具名称/顺序；实际 tools/list 先过 MCP wire schema，再以十二个根显式 type:object 的 input/output Schema 验证业务实例；合法ZIP import；A启动自有测试应用、B执行不同点击/等待；get真实结果 | SDK/依赖版本、协商版本、原双流、实际根 Schema/业务实例校验与Demo后端转发回读；C零调用 |
| E / 输入与包 | 未知工具、缺字段/额外字段、重复属性、坏UUID、Int64溢出/精度边界、real空/零transport、wire null覆盖、无契约覆盖、坏ZIP/路径/哈希 | 协议错误与业务isError分层，原日志/任务计数无新增，目标零写 |
| B / 权限与文件 | 缺scope、未授权串号、原0/未知映射、绝对/UNC/ADS/穿越/链接/换文件竞态；源复制后修改 | 拒绝明确；不能逃出root，原文件不删；已冻结引用不变；无法稳定制造竞态不得记为已过 |
| C / 并发与防重 | 同id同请求并发仅一个JobId；异请求冲突；活动job及100键满额时已接纳同键仍恢复原JobId且无新增job；忙拒绝后旧id不自动重试；A/B并行与旧A JobId不取消新A | 接纳/拒绝/后端动作精确计数，真实当前身份；第三目标无调用 |
| F / 故障与取消 | 接纳前取消零job；已接纳丢响应后目标离线或身份变化，同键仍恢复原JobId且零新增设备写；已接纳协议取消不暗停job；3个 import 占满普通通道时 jobs.cancel 仍经保留通道到达且只取消目标job；额外普通调用立即有界 BUSY；deadline、断线、代次/transport变化 | 从 SDK/transport 调度到 handler 的到达/时限证据、原Completion/每任务终态与末步动作计数、恢复前后JobId/设备写计数；不以取消返回当已清理 |
| T / 配额与退出 | 大消息、深JSON、每种配额、100键满后拒新键但旧键恢复、长错误及截断Schema/实际UTF-8总预算、500日志/16KiB结果、stdin EOF/客户端崩溃/不读输出/强制终止 | 第101终态淘汰属stdio不可达，另留原 Scheduler 独立测试原件；其余留有界时间/实际内存与双流排空/所有owned任务终态；无法drain保FAIL；不杀外部进程 |
| R / 重启与实机 | 新epoch拒绝旧操作；丢写响应重启不重放；real竞争准入及授权1/2实际差异动作 | 重启前后epoch/动作计数、当前ADB owner/目标身份证据、零0操作；Demo不替代Live |

每例记录 caseId、前置、步骤、预期、实际、PASS-OFFLINE/PASS-LIVE/FAIL/BLOCKED/NOT-IMPLEMENTED/NOT-RUN、源码/DLL/PDB/配置非敏感摘要/原始双流SHA、own child exit/清理与未选目标计数。编译成功不是协议成功，协议成功不是设备效果或抓包成功。保留原失败及旧GUI/AP042/AP045边界，不以本专项掩盖完整GUI验收。

## 11. 迁移、风险与推翻条件

初版新入口不改变旧 CLI/API/GUI 使用方式，不迁移原用户数据；新私有 profile 及 staging 路径单独保存，默认不读取授权配置或账号内容。升级 SDK/协议/权限/真实所有者方案必须重新冻结 Schema 和兼容矩阵，不让浮动SDK改变启动流程。未启用 MCP 时产品行为应保持原样。关闭/禁用新入口即可停止新增接纳，但取消不回滚已经生效的设备动作；不自动执行数据回滚、删除分支/worktree或卸载组件。

已知假设与剩余风险：官方 SDK net10.0/stdio 可适配只由资料证明，本仓运行尚未核；原 importer 是受限格式校验器不是执行任意代码的沙箱；scope 文件只在可信本机进程边界内有效，不能对抗同用户任意恶意程序；进程内历史和幂等不跨重启；外部设备写入结果不明时可能需要人工核验；原16KiB输出不保证应用业务已成功；没有真实采集后端就没有MCP抓包能力。

出现以下反例应推翻或收缩当前推荐，而不是删验收取绿：

- SDK实际无法在本仓锁定依赖/stdio正确EOF清理，或固定协议断言与真实wire不一致：停止该版本准入，回到官方稳定版本/精确协议选型，不自写替代协议。
- 同一真实目标存在两个未协调可写Runtime：保持real写关闭，先完成单一所有者；独立profile或口头不并发不足以放行。
- 不能可靠证明根内文件和冻结副本身份：缩为操作者预登记的固定包清单/只读预览，暂停自由relativePath导入，而非允许任意路径。
- 幂等门下出现双Submit、旧id重放新job或请求取消引发无记录写：视为接纳契约失败，修复后完整重测，不靠重试掩盖。
- 不能清理owned任务/导入/子进程，或写结果不明：停止对应动作并保留失败，不强杀共享ADB/模拟器。
- 实际用户必须同时使用Desktop与AI、跨进程/重启找回任务，或希望远程多人授权：当前独立进程最小版已不足，另立统一Host/持久存储/远程身份设计，不暗中扩大本版承诺。

本轮只新增本设计文件。后续实现需由 root 单独组织测试先行、独立审查/重建及分阶段版本提交；设计审查通过也不等于 A15b/A14c/A16 已完成。
