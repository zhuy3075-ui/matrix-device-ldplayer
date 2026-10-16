# A15b 群控 MCP 实际接入

日期：2026-10-17。当前主仓基线ca6c04e；承接R12/R13、A15及已独立审查的MCP_DESIGN，不改变完整GUI、实机、RPA与抓包验收范围。

## 目标、事实与边界

目标仍为真实MCP客户端调度明确授权目标，导入受限包、提交逐目标作业、读取结果并按JobId取消，最终验证专用实例1/2的真实效果。已有HTTP没有脚本作业路由及可信共享授权，不能直接作为MCP桥；独立Runtime的busy/目录代次不与Desktop共享，真实写入门禁尚需实现。

本阶段先实施官方SDK stdio与六工具完整Demo闭环，明确不计实机验收。独立入口要求显式Demo、绝对fresh数据目录、绝对可信策略和导入根；不加载Desktop配置、不开放HTTP/Agent/ADB，不注册shell、实例生命周期、安装卸载或采集工具。real模式在创建目录或Runtime之前拒绝，后续只有共享所有权竞争负测通过才开启；此为阶段前置，不是删除real验收。

## 第一性原理与方案

已知原Importer、Compiler、SubmitJob、JobId及身份队列可复用。根问题是跨协议保持业务身份、防重、取消及生命周期，不是重写执行引擎。推荐新增MatrixDevice.Mcp独立console，用官方ModelContextProtocol 2.2.0精确版本及lock，协议固定2025-11-25，显式注册六工具，stdout只传协议、诊断stderr。

实施机制勘误：精确2.2.0默认WithStdio入口内部直接读取Console且ReadLineAsync无配置上限，不能据此承诺解析前1MiB边界。采用同官方SDK的WithStreamServerTransport，输入输出仍为真实Console标准流，外层仅增加有界字节/JSON检查；不是内存假transport或手写JSON-RPC。这是对设计中入口方法的必要偏差，协议、十二份Schema与安全验收不变，须通过实际EOF与背压验证。

更简单的只读协议外壳只可作中间红绿检查，不算本阶段六工具完成。拒绝手写JSON-RPC、任意shell、把现HTTP改称MCP或默认授权全部23台。统一常驻Host/双方参与的lease为真实阶段候选，须另审IPC/进程出生身份/授权与竞争，不声称当前具备。SDK不能可靠实现有界输入/EOF清理时停止准入，不能删除这些验收取绿。

官方依据已于本日复核：[SDK v2.2.0](https://github.com/modelcontextprotocol/csharp-sdk/releases/tag/v2.2.0)、[stdio与客户端用法](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/docs/concepts/getting-started.md)、[协议pin选项](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Server/McpServerOptions.cs)。文档支持不等于本仓restore、构建或运行通过。

## 任务与验证

| 优先级 | 实施范围 | 必需验证 |
|---|---|---|
| P0 | 严格启动参数、策略、官方SDK、输入输出边界、六工具wire Schema | 无/坏/重复参数在副作用前拒绝，官方独立客户端真实子进程握手、tools/list/未知工具/无stdout污染，实际wire Schema有效 |
| P1 | 原Importer加根内稳定文件冻结与有界引用 | 正常ZIP/hash/参数预览；绝对/穿越/ADS/reparse/换文件/坏hash/额度拒绝，源不删除、冻结后不受源变化影响 |
| P1 | 原SubmitJob及防重ledger、get/cancel、deadline | A/B不同动作与结果、同键并发只一job、冲突/忙/身份/旧epoch/拒绝重放、按JobId取消不影响另一目标或后继任务 |
| P1 | 取消保留通道、响应/速率/日志/引用/键容量及生命周期 | 满普通通道仍可取消，大小在反序列化前有界，Unicode/转义后实际预算、EOF停止接纳与owned任务排空/Dispose，原子记录结果不明 |
| P1 | 独立复建与主仓回归、README/清单及私有feature交付 | 原209及既有任务语义不回退，新专项真实stdio证据与原流/终态，精确diff独审后单功能push；不合并发布 |
| 后继必需 | 统一所有权门禁、真实1/2与兼容/采集链 | 产品双写竞争拒绝、当前ADB/目标证明、真实差异任务/取消/回读；Demo或两个实例不得代替23路容量，采集另依A16 |

六工具为matrix.capabilities、matrix.devices.list、matrix.scripts.import、matrix.jobs.start、matrix.jobs.get、matrix.jobs.cancel。输入输出及限额沿用MCP_DESIGN中的完整契约，不从设计静检推断已运行。参数覆盖保留JSON null与整数精度，未知/重复字段拒绝；作用域来自可信启动策略，工具调用不能扩大。作业请求取消不暗停已接纳job，取消不撤销已发送动作；已接纳旧operationId重放先恢复原结果，不因当前离线/满额再次提交。

独立测试用官方StdioClientTransport/McpClient；其Windows实现会经cmd包装并在退出预算后结束自己拥有的子树，不能宣传无shell或从未tree kill。为保留server原始字节与原Process所有权，正式测试可使用透明relay子模式，仅在官方客户端和真实server之间原样tee三条标准流，不解析或生成协议。relay只持有本次Demo server及fresh路径；EOF后业务清理10秒、退出5秒、原流有界排空，必要只Kill(false)原owned server；SDK退出预算必须覆盖这些时间，外层再留余量。不能据SDK行日志重序列化冒称原始字节。

静审补充的transport资源约束：固定SDK会在响应发送之后才移除内部请求记录，仅在flush时允许RPC id复用仍有ABA竞争。当前连接所有见过的RPC id在首次接受或拒绝前登记带类型的精确值SHA256摘要，最多65536项；string id的UTF-8值最多1024字节。复用或满额即停止接纳、关闭该连接并清理owned任务，不创建无界拒绝响应队列。此为明确额外传输边界，不是operationId业务幂等，也不修改additionalProperties:false的业务Limits或十二Schema；README/ServerInstructions必须告知，达到边界不自动重放任务。65536仅是本进程容量，不等于已压测；30分钟按10次/秒是18000次，也不能用这项算术替代实际稳定性测试。

业务arguments在任何语义接纳、ledger或job之前按实际JSON编码后UTF-8字节限65536，超额返回CAPACITY；拒绝后同operationId可用合法短值首次接纳，不能误登记旧拒绝。IO终态须覆盖原native线程结束、timer/handle清理和Join完成，不仅action已返回的TCS。cleanExit同时要求owned任务全终态、protocol credits为零、原IO终态及无清理失败；HostOptions10秒不保证任意OS驱动调用绝对结束，relay有界强杀必须FAIL，不以丢弃pending Task伪造清理。

现有Demo后端只有A/B两台：同scope验证A/B差异并行，另一fresh scope仅授权A，将确实存在的B作未选目标；未知C拒绝另测，不称已有第三台真实目标。如果无额外观测入口无法独立回读未选B效果，则保留该效果NOT-RUN，不用无job等价替换。任何难以从公开接口触发的身份换代/精确接纳竞争仍保留原契约未验，不增加测试业务工具或削减矩阵。

## 执行与证据

以下初始记录按阶段保留；当前结果以末尾“测试机制修复后的独立实测”和“主仓集成门”为准，不将历史尚未运行解释为当前状态。

实现使用独立worktree；测试实现另独立，作者不能给自己最终批准。主仓当前三份GUI观察记录WIP及用户PNG/配置保留，不归为MCP已验源码。正式测试以外的临时包装器、raw、APK、依赖二进制和本机策略不提交。

上一GUI轮属于progress：一次官方默认抓屏实际FrameArrived超时，parent终态/护栏获独审，GUI仍FAIL，应用流程NOT-RUN；不重复该分支。当前MCP服务端13文件r03和独立测试9文件r02均已编译；服务端获仅限fresh Demo的运行前静审，尚未执行server/client或真实stdio握手，不能关闭A15。main尚未集成新源、构建、提交或推送；约13剩余工作块/7类不因此扣减。

测试r02静审拒绝运行，原冻结保留为NOT-RUN；已一次交由测试作者窄修六项：Windows relay原stdin取消与终态、活跃raw日志共享读取、未知工具30秒预算与原Task追踪、超大/半帧负例的精确stderr路径、初始化失败终态、raw清理失败后的全局熔断。正常业务断言与13类未覆盖项不减少。独立verifier已准备有界runner但未执行，仍pin旧测试；须先核新freeze及静审，再允许独立复建、原209和真实MCP专项。每阶段更新实际结果、失败原件、剩余范围；完整goal保持active。

### 首轮实际运行与失败

tests r03九源已冻结，作者树frozen-r03/manifest.json为63B62EC6C66E5BBEC2F3232865AFA563E78A4558B05C3BF63B783A8C01DD2FEE；机械复制后，实际runner固定的verifier/freeze-r04/tests-inputs.json为08E7D1B2DD796710D9F15300C9FB4D1690349390FB87A955AC8EB45B01B8345A，两清单格式不同但对应九源逐文件核同。六项差异及outer runner均获独立运行前静审。root唯一执行，所有子进程均使用隔离Demo，未操作GUI、真实实例或ADB。

1. independent-r04原exec session64747已终态outer1。两locked restore及Build子进程均exit0，Build78.265秒、原209项51.59秒；外层整段UTF-8解码在原日志C8字节处失败。原execution SHA321461CC8EC6B77241AF691D68315C39F35E26C97842873A2C6A7F73B4AFBD2C和六raw保留，不重跑构建。两独审按Latin1一一保字节映射，仅提取并逐条断言ASCII PASS名称，实际209名称顺序及RESULT全匹配；不声称非ASCII全文已解码。
2. 新continuation-r01只续实际PDB14、102编译文件冻结和首次stdio，脚本97A74596C64B0F5129FC128FDAB59AE98127C82F3CD090EE832FAB541835C6CD获独审。session89697终态outer1，client68420 exit1/47.393秒，外层双raw排空、无root强杀或超时、postflight null、retained0。execution B7B40405FC5A7AE015C2735152BF5824719007AD0F92810A6CBEBF780FD395BF，summary 0BDB8D14235A06590812A78A977CE79D1300BE8E2759949D4ABC419D7525F00A。
3. P00的十二个真实子进程负例均自然exit1、原流排空；九启动拒绝在Runtime之前，三个畸形输入的生命周期记录owned job为零。解析前超过1MiB、半帧EOF及超过1024字节RPC id的精确诊断成立。仅此范围PASS-OFFLINE，不能外推正常EOF或业务工具。
4. 唯一正常会话实际收到2025-11-25初始化及六工具列表，但ListToolsResult的测试Schema门失败。原stdin只有initialize、initialized和tools/list，未调用capabilities或作业；后续十二正常项自动NOT-RUN，加原十三类，共二十五NOT-RUN。不能把列出六工具称为六工具业务通过。
5. 清理同时失败：SDK outer45512的completion exit-1，relay.json和server lifecycle缺失；owned server64172事后missing。原Task完成/外层无kill不等于自然清理，也不能宣称无SDK树杀。保留完整FAIL，不重跑相同前置取绿。

两项根因分开处理。Schema门使用默认STJ重新序列化SDK DTO，写出线上缺失的_meta/title/icons等null字段，另有NextCursor默认命名差异；只允许新增专用官方McpJsonUtilities序列化及实际raw Schema门，不修改领域Evidence.Json或删除explicit-null负例。退出方面，SDK2.2.0 StdioClientSessionTransport先等待进程，基类也不关闭serverInput流，不能把Client.Dispose等同stdin EOF。候选为官方StreamClientTransport直接连接本次Process的真实stdin/stdout，并由唯一owner显式关闭输入、等待原进程/流终态；不是内存假transport、控制文件侧信道或手写协议。该机制偏差已获有限独立设计准入，只允许测试作者编译/冻结，尚未运行或通过；原StdioClientTransport清理失败不被后继成功覆盖。源码依据：[官方SDK序列化](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/McpJsonUtilities.cs)、[stdio退出](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Client/StdioClientSessionTransport.cs)、[公共stream入口](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.2.0/src/ModelContextProtocol.Core/Client/StreamClientTransport.cs)。

### 测试机制修复后的独立实测

测试r04九源（OwnedStdioProcess替代StdioRelay）及runner EE39BEB5849FE0D858F75228A8B8204DD4CBE191B1D1C451F1D71A8DCE8698F0经独立静审后，root唯一执行fixture-fix-r01。只重建发生修复的客户端，不再重跑原209；server及其余100个编译文件、186输入与主仓164+46+3均逐文件保持。原r03失败及r04首次编译CS0844保留，编译修正仅明确字段this.owned。

- 实际session63970 terminal outer0；build17140 exit0/2.298秒、client46168 exit0/32.185秒。14个顶层用例全部PASS-OFFLINE，仍汇总PASS-LIMITED；13类未实现/不可稳定触达覆盖保持NOT-RUN，不能合并到通过数。
- 14个官方SDK正常业务会话均为外部server真实OS管道；13自然exit0、RPC-id复用负例自然exit1。owned-process记录原stdin关闭、stdout EOF、stderr/SDK/原Task终态与资源释放，无强杀、超时、pending、retained或清理错。另12个严格启动/畸形输入子例自然exit1；其中畸形输入实际接受1048577/33/1085字节与精确诊断成立。
- 实际227条调用逐请求id对应原wire；13个JobId全部终态。三次并发同operationId只首次接纳一次，其余重放；71969字节业务体拒绝后，同键短值首次接纳。引用16/第17拒绝、ledger100/满额重放、500日志尾窗与Unicode结果16382字节有实际记录；deadline与EOF活动任务取消分别核验。
- 独审实际核82条raw清单、3089条IO终态、28个owned PID事后missing、PDB14/完整102编译物与输入冻结；原两失败JSON及八raw未变。execution SHA38033BD70AE3BF287DD4DFDC547CA74B8A76B3E8AD52B70877A0B1C5A11DB636，summary SHAB12E0DD10E13C063C9A2E4EBB76A5673DBA87C601212196C35B807754937B9CF，after SHA E1DE0EFC6F7369A36DF73E730F586233F0E97BFD5535D5D0862C02D1D1DD985B；原件在独立verifier artifacts/mcp-stdio/fixture-fix-r01。

官方StreamClientTransport.Name固定为in-memory-stream，但实参是外部进程OS管道，不据名称误称同进程测试。此结果只批准主仓精确集成与代表复验，不关闭旧StdioClientTransport兼容失败，不认证实机/GUI或整个A15。

### 主仓集成门

13个server文件与9个test文件从实际获审freeze-r05复制；唯一测试代码差异是summary把旧relay.json更正为owned-process.json，不改断言或用例。solution加两工程，Build在原209成功后调用固定client/server DLL，启动前打印ASCII相对证据路径，exit0后另打印成功标记，不预建输出叶目录。无Verify仍只编译。更简单的独立runner独留方案会让标准Verify漏测新工程；拒绝普通Build自动运行、手写协议或借Demo开启real。

root核复制前main原164输入全一致、无运行Matrix程序后，受审outer8C75AA26E7EA44F52982B693FD8BA4EC500F4A7E9F6EA66103DF60F29931E6D3已唯一执行主仓Build -Verify。exec73951 terminal outer0，owned Build85808自然exit0/113.515秒，原209精确名序/51.92秒，MCP14顶层/14会话、12拒绝子例及13类NOT-RUN分开保持。双raw23381B/0B终态、无timeout/kill/retained、error/postflight null，实际PDB14/148编译物与186输入/用户3门通过；这是本次主仓实际结果，不沿用独立树binary身份。授权重建后原Desktop46中9项变化已单独记录，生产162源未变。编译保留既有CS8602/CS9124警告，不称零警告。

主仓execution SHA11321B64C8045D267C8F441592BBAA4779483EC41A6EA55E2DD32015B84399AC，summary SHA7C68EC0199A47FFF5F4D8695A71EE563423689315A046E1D4DC8372FDAFA7A2E，after SHA44FF4D0382B57248EF9D2AAA3543AE0B77E8F811CC71F98E19882DA694226447；原件为main artifacts/mcp-stdio-main/main-r01与verify-9d25e1db03b343dba574b2332955d74f。独立终态审已有限通过：逐227调用核wire，13JobId对应lifecycle全终态，80条原流与3089IO状态实核，源/产物/用户保护及原失败保持；只批准文档/核包门，不代表整个A15完成。当前尚未核包/提交推送，须核完整源码包与精确index后单功能push。标准Build入口只是编排，不新增独立硬退出预算或强制清理保证，不重跑已通过阶段凑数。

剩余13类：目标换代/丢响应；三import阻塞下取消保留；精确cancel接纳边界；reparse/复制换源；归档/解压/条目/staging完整额度；深JSON/转义/长错误；崩溃/不读输出/强杀；100终态上限下不可触达的第101淘汰；tap及未选目标后端回读；real共同所有权/1与2；协议不匹配；metadata/参数列表/非BMP预算；65536 lifetime id容量。它们以及真实多实例容量、全RPA、抓包、完整GUI/DPI与最终双实例30分钟均仍必需，不拿当前部分通过替代完整用户目标。
