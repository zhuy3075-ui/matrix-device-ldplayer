# A14a1 脚本参数绑定

## 目标与边界

基线 `f2d171744c9296e7cbb4314f63649a0b4374408a`。现有自动化页保存参数的 Value，但 ScriptScheduler 只执行原步骤 Value/DurationMs。这个阶段使显式声明绑定的新包真正使用参数，仍经同一调度器执行；不是另建执行引擎或用格式校验替代 RPA 闭环。

本阶段属于 A14a/A08 的一个独立交付。完整 A14 的上传预览、逐目标作业 ID、身份冻结、分发、取消和结果回传，以及 A15 MCP/A16 抓包均保持必需，未因本阶段完成而取消。当前 GUI、真实双实例同步、应用 GUI 安装卸载和最终双实例 30 分钟仍未通过，不由 Demo 结果替代。

## 已知事实与约束

- 旧 ZIP 包、manifest.json/script.json、原归档限额和合法附带资源必须兼容。Manifest.Version 是脚本作者版本，不拿它隐式切换解释规则。
- 原 ScriptParameter.Value 为 JsonElement?，普通反序列化把 JSON null 和缺失都变为 C# null；现有页面的 JsonNode 往返也会丢失这个区别。仅增加 `?? DefaultValue` 不能满足新合同。
- 原脚本 Probability 只存储/验证有限数，执行语义缺失；不能擅自猜测随机概率含义。
- 原 RunAsync 的重复运行、serial 互斥、取消、日志、异常与旧“停止所选”的当前选择语义保持。本阶段不声称已修复“目录检查后 transport 换代”竞态；后续作业入口必须在既有串号队列/pin 边界传递和验证冻结的 expectedTransport，不能在队列内递归完整 Refresh。

## 格式与执行合同

- 新增可选 `executionContract` 对象，显式 `version: 1` 和 `bindings` 数组。缺失/null 契约沿旧路径；存在但未知/默认版本必须拒绝，不能回退成旧执行。
- 每个绑定为零起始 `stepIndex`、精确 `field` 与 `parameterKey`。字段仅 `value` 和 `durationMs`，只替换整个值；value 仅用于实际消费该字段的 launch/launchapp/click/swipe，durationMs 仅用于 wait/sleep，拒绝“绑定成功但执行器忽略”的组合。不支持模板、表达式、宿主代码或反射。重复的步骤/字段、空/不存在参数键、越界索引、空条目及未知字段均以 400/1007 明确拒绝。
- 基础步骤仍须是合法字面量，使旧导入校验顺序清晰；先不支持把占位符塞进旧坐标或包名字段。新契约中任何非空 Probability 都拒绝；旧契约继续原保存和执行语义。
- 参数优先级为本次显式覆盖（按键是否存在） > 参数 Value（按字段是否存在） > DefaultValue。显式 JSON null、undefined 或缺少可用来源，不回退；绑定前报错。未知覆盖键拒绝，键区分大小写。
- 原三参 RunAsync(serial, package, ct) 保持；新四参固定为 RunAsync(serial, package, ct, parameterOverrides)，不增加与旧第三参 default 有歧义的重载。无契约包加非空覆盖字典明确拒绝，不能无声忽略；旧入口及空覆盖保持旧语义。
- `value` 必须为 JSON 字符串，`durationMs` 必须为非负 Int32 整数；不把数字字符串、浮点数、布尔、对象或数组隐式转换。现有类型声明与数值范围校验保持，对覆盖值亦校验。替换完成后再次执行原动作校验，非法坐标格式/应用包名/滑动时长均在调度前拒绝。
- 编译和运行使用独立快照，不修改原包、参数、覆盖字典或绑定；编译结果移除 executionContract，成为固定字面量的普通执行包，不能被后一次编译用原 Value 重绑覆盖。它不是原包的参数化重导出格式。原调度器只执行解析后的普通步骤，调用方在提交后修改源对象不会改变已接纳的步骤。
- V1 使用由现有 JsonDefaults 派生的专属序列化选项和 nullable JsonElement 转换器，保留显式 null；不修改全局 JsonDefaults，不给旧参数类型加全局生效的属性转换器。读取 V1 可在同一受限 JSON bytes 上二次解析，但不能再读文件或放宽大小限制。保存/快照/重载须保留同一语义。
- 自动化页面沿原导入、参数表格和“运行所选”入口。V1 参数编辑采用保留 JsonElement 的副本，不经普通 JsonNode 反序列化丢失 null；旧页面分支保持原行为。运行前完整解析验证，失败不写运行元数据或启动任何目标；不新增重复操作入口。

## 最小方案与取舍

推荐显式契约加纯编译步骤，在现 RunAsync 提交前生成快照，继续原执行循环。较简单的“只保存参数”正是当前缺口，不采用；只返回编译结果而没有同一引擎的真实执行也不算本阶段通过。拒绝字符串替换/eval、复制第二执行器、根据文件后缀或附带文件自动切换格式。

V1 专属选项比全局模型转换多一条局部分支，但保护旧包解释，验证成本集中在新格式、往返和执行效果。若不能证明缺失/null、旧包语义或快照隔离，应拒绝交付，而不是弱化合同。此方案不解决输入权限、会话换代或新作业取消所有权，后续必须独立实现验证。

## 任务与验收

1. P0：冻结本文和正式测试，定义新 API；保持原 169 项名称、顺序与断言。先对旧生产行为执行红测；编译/夹具/环境错误不算产品红。
2. P1：实现局部格式/编译与现执行入口连接。测试同一合法 ZIP 经 Load、编辑/覆盖、Save、Load，再由原 Scheduler 在两个 Demo 目标产生各自实际动作，未指定第三目标零动作。
3. P1：验证 Value/Default/覆盖优先级、显式 null 不回退、Int32 边界/溢出、类型/范围、未知键/字段/版本、重复绑定、索引、Probability、替换后动作验证；拒绝时无后端动作、无新 Running/日志状态。保留 legacy Probability、附带资源、重复启动/取消回归。
4. P1：新增独立 VM/STA 专项入口 `--verify-script-parameters --demo --data-dir <fresh artifacts绝对目录>`，不创建 Window、不进行 UIA 导航，使用同一 Application 的 STA Dispatcher。四例使用各自自有 Runtime、原 ViewModel、只转发自有 Demo 的记录后端；走真实 ZIP Import、ParameterEditor 编辑、Start、原 Scheduler 和实际 Demo 结果。合法 V1 编辑、null/default 拒绝、非法编辑拒绝和 legacy 分支均验元数据、准备状态、日志/接纳及后端动作。混合模式、非 Demo、未知/重复参数、已有目录及截图/宽高参数在创建目录/Runtime 前拒绝；禁 API/Agent/自动连接/同步/窗口恢复，不读实际用户配置。它不是物理 GUI 点击或尺寸验收。每个接纳任务依据原事件有界观察终态，清理用原 ShutdownAsync，报告和清理完成后才显式关闭 Application；失败只写 stderr 并非零退出。
5. P1：独立 verifier 在另一冻结 worktree 实际执行全基线与新测试、审查 diff 和输入输出；然后主仓机械集成、复验、更新正式文档/源码清单并单功能推原私有 feature。实现者不得自批，不合并/发布。

每轮使用独立 ignored artifacts，保存原 stdout/stderr、命令、源码/二进制摘要、退出与超时状态。构建/测试有界，必要进程全部结束才交接，不重跑旧通过阶段凑证据。真实 guest、外部 ADB、Windows 输入/截图、账号/权限/VPN 和用户配置不在本轮执行范围。

上述“拒绝前零动作/状态”仅指本阶段格式、参数与原动作语法验证，不扩大成已知设备几何检查；坐标在目标上的实际范围仍由原后端执行时验证，其运行失败保持原错误和状态。

## 状态

- Core 固定 24 项正式测试已实际运行：唯一 red-r01 编译成功、0 警告/错误，原 169 项名称/顺序精确保持 PASS，合计 170/193；新增仅全局 JSON 隔离一项 PASS，其余 23 项包含真实参数效果/非法接纳/null 丢失和单列的新 API 尚未实现，不能统称 23 个同类产品错误。child77276 正常 exit1、76.987秒，双流完整，无 timeout/清理错误。exec session21107 已 terminal；本机原件 `artifacts/ap046/red-r01`，execution SHA35221F242D2CFC386A59E33ED1905E006B9DAE24BE1117E19E97D7217F9FEE0A。
- 唯一旧诊断 red-r01 在原 AP042 自身失败：三阶段146→146→146，应用三个 owner 均缺失、旧 owner 残留111；五任务终态/恢复/排空成立。原14类 PASS，但 AP040、新 VM 和几何均 NOT-RUN，不可拿它当新 VM 的红。child65260 exit1/1.187秒，双流完整、无 timeout/清理错误；原始 JSON/流和八源逐 SHA 的终态后副本保留，不重跑该失败链，也不推断环境或偶发成因。
- 原诊断链恢复未交付 hook 加入前的原14/AP042/AP040/几何顺序和断言。两个独立 reviewer 同意为不依赖 UIA 的 VM 业务建立上述独立入口，但该入口必须在原生产重新取得四例真实红、再同一夹具绿；不允许用它取代失败的旧门、真实 GUI/实机或最终全回归。四例产品断言不削弱，夹具壳差分和新入口另冻、静审后才执行。
- 独立新入口已完成代码/精确差分静审。App仅 early branch/catch 加旗标，其它原分支规范化全文不变；无窗夹具的原四 case 名称、Product 断言、VM/后端及清理逻辑保持。`red-r02` 只编译 child44564 exit0/15.335秒，无重跑 Core193；缺Demo/混合模式/请求截图三个边界 child74256/37876/48232 均exit1，profile/图片均未创建，原双流完整，二进制保持。
- 新 VM 业务红已实际建立：child59176 exit1/565ms，4例为2个纯 PRODUCT-FAIL、2个 PASS，无 NOT-RUN；合法编辑仍启动原 literal，显式null丢失后出现元数据/Running/日志/真实后端调用，非法JSON与legacy原行为通过。Application前后0窗口/null MainWindow，四清理、公共任务/转发任务终态和真实Backend.Dispose均成立，无清理错误；不声称访问私有Run.Task。summary SHA `C8566FA482D4D3888CA9A1F2F1FB824326749B6029DE54C03FEC83D0A4039C2A`，execution SHA `9DE2F1DF87CDA344050BC40DB21F46C260495B93D30379DA00DA9E0E7FDB55BE`。ignored `artifacts/ap046/red-r02-vm`另存逐SHA吻合的终态后八源/四DLL及PDB副本，保留原r01失败，不重写。
- 实现仅改Service/Scheduler/WorkspaceVM和新Compiler；Core24、VM4、Program与新入口在红绿间冻结不改。作者green-r01：Build67708 exit0/74.717秒，193/193（52.79秒）；VM42940 exit0/471ms，4PASS，summary `EEF7BB861A84CBAEF8274276E8B6AFBB7D441C8F30C8A2911DE51435FF02DFBB`。原169名序、旧24Smoke及旧诊断链保持；没有重跑AP042失败链。
- 独立隔离树唯一independent-v01：Build43700 exit0/74.776秒，193/193（51.57秒），实际PDB36源/156输入及8冻结源核同。三入口拒绝均exit1且目录/图片未创建；VM59560 exit0/535ms，4PASS、42公共任务和全部转发任务终态、4清理/Dispose、0窗口/null MainWindow。execution `733890447E661B497B6EC14A5355D0BEE231916D93513066FD6437955B5C5541`，VMsummary `36CB759A8CF009E3F19E534FC675EC0ED35E9731C0EC02484694B8F77C601632`。独审仅批准参数业务范围，不批准GUI/实机或完整RPA。
- 主仓精确机械集成八源后唯一green-r01：Build77400 exit0/73.440秒，193/193（51.24秒），原169精确保持；VM45824 exit0/506ms，4PASS，实际V1仅A启动settings/B仍demo，显式null和坏JSON拒绝前metadata/状态/日志/执行调用为零，legacy仍执行原literal但保存编辑值。两次双流完整、无超时/清理错，8源前后不变；27原守卫保持，App为唯一另审批准改变，不称28原件全不变。主仓execution `55EF318F027DC4FCCD08BD1CFBE342844E90B3341E6DB5F33AE989FBAB00A888`、VMexecution `144DFF29FE202740498DA9B581694D3D3F373105F9D1A1BE9E2429ED0C54B09B`、VMsummary `FA77BE5372B96ED3FCE8A0C701F7D78489825FC175F0F07EC1E6D798F8619AE1`；Desktop DLL `44B290489CF93E927A5D04732B13E4D736FFD0FCB91829123B1239DC7515AAB6`。不同树编译产物不要求同hash，以实际PDB与源码输入核对；报告含时间戳亦不要求raw同hash。
- 新增`examples/parameterized/manifest.json`和`script.json`作为打包源；实际生成的本地ZIP已核仅两个根entry、每entry原bytes同、JSON语法通过，未另执行该样包或声称实机效果。正式Core/VM验证的是对应绑定行为，不以样例格式校验充数。
- 本阶段源码交付仍须正式文档、完整核包/精确index独审与真实Git回执，不合并发布。旧AP042当前PRODUCT-FAIL和GUI/实机/最终双实例30分钟、完整作业身份/取消回传、MCP与抓包均开放，goal不关闭。
