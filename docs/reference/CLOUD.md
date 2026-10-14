# JiXingCloudControl 主程序静态逆向报告

> 开发参考资料，来自 2026-10-14 静态分析；不是已批准的首版建设规格，也不是当前运行结果。原始日志、CIL/IDA 导出、样本和资源中间文件已按用户清理要求移除。正文中的 token/RVA、evidence 路径仅保留为历史定位，不能当作现存文件；动态修正见 [已知问题](../KNOWN_ISSUES.md)，来源与可复核边界见 [来源说明](../SOURCES.md)。


分析日期：2026-10-14。范围：JiXingCloudControl.exe、SQLCore.dll、LanguageCore.dll。方法：REA 托管元数据/CIL、IDA CLI 全量反汇编、交叉引用、只读原始 IL 与嵌入字符串恢复。未启动目标、未接入手机、未读取数据库行、登录状态或用户凭据。

## 1. 样本和证据覆盖

|样本|SHA-256|规模与形态|
|---|---|---|
|JiXingCloudControl.exe|48af9c992023d3b8765764138066a269cb68c77d88797f5a9a27fdb3339a9576|5,183,488 字节；PE32+ x64；.NET Framework 4.8 WPF；2,221 类型；22,120 方法|
|SQLCore.dll|024c256372d68f0d6a26812da90d70c7ff58193414f490e3a17fdb59ba51a3f3|18,944 字节；AnyCPU 托管库；24 类型、91 方法|
|LanguageCore.dll|41cf83cf18a9b8ff513fa04cc917382801873bc6733c9d5beb8714966d9baf73|13,824 字节；托管本地化库；14 类型、67 方法|

主 EXE 的 REA 结果包含 21,990 个有 CIL 方法体、130 个无方法体声明，共 425,738 条解码指令，方法体截断数 0、调用边 94,448。IDA 对全部 22,120 个方法导出 assembly 数组和引用；SQLCore、LanguageCore 分别导出全部 91、67 个方法。**这表示全量 CIL 导出完成，不表示逐个函数语义、动态调用、所有外部 DLL 和所有运行时分支均已人工证明。** REA 有 2,186 个复杂/泛型方法签名不能完整解释；IDA 的 CLI 泛型签名显示也有不准确项，因此以调用对象、CIL 栈和真实 IL 校验。

地址规范：下文 token 是当前模块的 MethodDef/FieldDef；RVA 为 PE 中原始方法体位置；IDA EA 是 CLI 加载器合成地址，不能解释成运行时 native VA。

主要证据：
- cloud-rea.json：六次 REA artifact/members 记录；主程序成员 evidence ID 为 ev_f06c2e1bac5b5b9e082f035275c9fd2f147bf2879ec0492d99e39f018b2e8309。
- cloud-ida-disassembly.json：完整 CIL 与引用；SQLCore.dll-ida.json、LanguageCore.dll-ida.json 对应小库。
- cloud-selected-annotated.json：1,210 个关键方法附字符串注释；cloud-method-index.json 可按 token/RVA 找全量方法。
- cloud-schema.json：11 个实体、46 个 AppConfig 字段及脚本结构，仅模式，无数据库记录。
- cloud-static-string-map.json：5,530 个静态字符串访问器映射；疑似内嵌数据库凭据已脱敏，未尝试使用。

## 2. 程序概述与架构

这是 Windows WPF 桌面群控宿主。设备墙、小屏/主控屏、输入同步、应用/文件管理、脚本调度、普通设备 API 和 Automator API 在同一托管进程组合；ADB、投屏解码/控制、自动化、代理和数据库由相邻 DLL 提供。界面采用 View/ViewModel/Service/Repository/Entity 分层，CommunityToolkit.Mvvm 命令和 WeakReferenceMessenger 连接视图状态与后台事件。

层次调用方向：WPF View → ViewModel/命令 → 生命周期/批处理/调度服务 → CommonCore / JiXingAdb / JXScrcpyCore / JXAutomatorService / JXAutomatorCore → ADB/设备侧服务。持久化方向：AppGlobal→AppConfig JSON；Repository→SQLCore→SQLite-net/SQLitePCLRaw；语言方向：View/LocExtension→LanguageCore→ResourceManager。

程序不是一个单纯 scrcpy 包装窗口：可见独立的设备准入、发现、故障恢复、主控借用、迷你会话、渲染/解码容量、分组目标快照、输入投递服务。但仅凭类名不能确认每项的实际性能或部署容量。

### 入口与初始化链

1. App.Main（0x060002bc，RVA 0x19eec，IDA 0x17550）创建 App，InitializeComponent，Application.Run。
2. App.OnStartup（0x0600019c，RVA 0xff1c，IDA 0xe0e0）启用文件日志、最低级别设为 1；单实例 mutex 不成功则退出；初始化崩溃报告；接入 ADB/Scrcpy/Core trace；注册设备恢复/断线与全局异常事件；加载 AppConfig；同步主控/小屏参数；初始化语言；展示 LoginStartWindow；调度延迟启动。
3. RunDeferredStartupWork（0x060001bb，RVA 0x11130）设置打包 ADB 路径，初始化窗口/投屏基础设施，观察异步启动任务，并在后台尝试 ADB 路径配置。
4. InitializeStartupWindowInfrastructure（0x060001c0，RVA 0x112e8）用 Interlocked 防重复，配置 D3D11 解码运行参数，使用 WPF Dispatcher 初始化 ScrcpySessionManager，再构造 MiniSessionConnectCoordinator、StreamLifecycleCoordinator、MainControlReplicaCoordinator 及回调依赖。
5. RunDeviceStartupPipelineAsync（0x060001bc，RVA 0x1118c；状态机 IDA 0x263f0）等待 StartDeviceInfrastructureForShellAsync（0x060001b7，RVA 0x10e5c）；基础设施未就绪则跳过补连；没有有效登录/设备额度则跳过启动网段扫描；就绪且授权有效则扫描已配置网段，再 EnsureMiniDevicesConnectedAfterStartupAsync。此处只描述存在的准入判断，不涉及绕过。

构造器（0x0600023c，RVA 0x15494）初始主控参数 MaxSize=1280、MaxFps=45、BitRateKbps 属性所赋数值 6000000、音频/控制开启；小屏 MaxSize=240、MaxFps=45、该属性数值 100000、音频关闭、控制开启、连接超时 15 秒。随后会被配置同步覆盖；属性命名与数值单位需结合 JXScrcpyCore 确认，不能直接宣称 6,000,000 kbps。

### 进程/线程模型

- WPF 主线程 + Dispatcher 负责界面更新；Task/async 处理 ADB、HTTP、扫描、脚本等异步工作。
- 定时器分离 token 定期检查、session health、UI hang watchdog：0x060001a1/RVA 0x10344，0x060001a3/0x103c4，0x060001a5/0x1044c。
- 群控输入有专用后台线程 JX MultiControl Input；目标 fanout 另有后台 mailbox 工作者，见第 5 节。
- SemaphoreSlim、Interlocked、锁、ConcurrentDictionary 用于启动单次执行、并发门限与缓存。没有动态测量，不能给出准确同时活跃线程数。

## 3. 功能清单及证据等级

|功能|静态可见行为|主要锚点|
|---|---|---|
|设备发现/连接与恢复|ADB 基础设施、网段扫描、启动补连、授权设备数门限、断线处理|App.RunDeviceStartupPipelineAsync 0x060001bc|
|设备墙/主控|ScreenListViewModel、DeviceControlViewModel、虚拟化网格、小屏/主屏会话协调|App.InitializeStartupWindowInfrastructure 0x060001c0；ScreenListViewModel.CreateWallConnectionDependencies 0x06000fed/RVA 0x5b9a0|
|分组与同步输入|组/备注实体、选择目标快照、鼠标和键盘开关、坐标映射与保序投递|DevicesLayoutViewModel.ToggleSyncMouse 0x06001433/RVA 0x6a927；ToggleSyncKeyboard 0x06001434/RVA 0x6a938|
|应用管理|读取应用列表、搜索/过滤、启动/停止、清理/卸载确认、安装入口|AppManageViewModel.LoadDeviceAppList 0x06001a05/RVA 0x82adc；StartApp 0x06001a12/0x830b8；StopApp 0x06001a11/0x83054；AppClear 0x06001a13/0x83124；AppDelete 0x06001a14/0x831d8|
|文件管理|电脑与手机双向浏览、路径段导航、文件选择、同步到手机/电脑、进度/忙碌状态|FileSyncViewModel.SyncToPhone 0x06001f7f/0x96db0；SyncToComputer 0x06001f80/0x96e7c；SyncFileViewModel.StartSync 0x06002075/0x9a06c|
|剪贴板/文本|安装 JiXingKeyboard、选择文本文件、剪贴板推送/拉取及超时|InputTextViewModel.InstallJXKeyboard 0x06000f44/0x5a1d4；ClipboardSyncLayoutViewModel.PushClipboardAsync 0x06001315/0x65490；PullClipboardAsync 0x06001316/0x654d4|
|普通设备 API|本地 HTTP 网关、每设备端口、health/device/shell/tap/swipe/text/app|ApiDeviceRequestHandler 0x06002fa8..0x06002fb8|
|Automator API|独立服务管理、默认 17171、脚本获得 JXDevice、连接事件联动|AutomatorApiServiceManager.Initialize 0x0600302f/0xce10c；AcquireScriptDeviceAsync 0x06003035/0xce2f8|
|脚本包和调度|ZIP 脚本包、校验、参数、单设备防重复、停止/断线取消、每设备日志|ScriptPackageService.Load 0x06003edb/0x10c18c；ScriptTaskSchedulerService.StartAsync 0x06003f3f/0x10f594|
|代理/环境/备份|专门 ViewModel、Entity、DLL 依赖、配置表单存在；详细底层在其他模块报告|ProxyEntity、EnvironmentConfigEntity、AppCkEntity；ProxyCore/JXAPPCK 引用|
|配置/数据库/本地化/日志|原子 JSON 替换、SQLite 迁移与 repository、资源翻译、崩溃与文件日志|第 6、7 节|

注意：Shell.Layouts.AppManageLayoutViewModel 中有 DemoFill；真正的应用操作实现位于 Popups.AppManageViewModel。不能把任何名称相似的 shell 占位布局都当成已投产界面；对未确认导航可达的布局保留这一限制。

## 4. 网络与普通设备 API（与 Automator 分开）

ApiHttpServer（构造 0x06002f91/RVA 0xca270；Start 0x06002f92/0xca2f4）使用 System.Net.HttpListener，注册 prefix、Start 后 Task.Run(ProcessLoopAsync)，异步接受 HttpListenerContext 并交给处理委托。BuildBaseUrl（0x06002fac/0xcaf2b）明确格式为 http://localhost:{port}/。

普通网关默认 8080：ApiDeviceServiceManager.NormalizePort（0x06002f80/0xca1a1）。每设备从 8001..8999 分配：ApiDevicePortAllocator.Reserve（0x06003003/0xcd56c）在锁中清理过期冷却项，排除已用/保留/冷却端口并预留首个可用端口；耗尽抛出错误。普通服务 StartDeviceService/StopDeviceService：0x06002f63/0xc9444、0x06002f64/0xc9504。

**Automator 是另一条链**：AutomatorApiServiceManager.NormalizePort（0x06003043/0xce8f1）默认 17171；Initialize（0x0600302f/0xce10c）接入显示信息提供者和 scrcpy 连接/断线消息，并 StartGatewayOnApplicationStartup（0x0600303e/0xce830）。不能把 8080 的下表自动套在 17171 服务上。

|监听范围|方法/路径|输入|输出或后端|
|---|---|---|---|
|普通网关|GET / 或 /health|无|{ok:true,data:{port,runningCount,status:"running"}}|
|普通网关|GET /devices|无|{ok:true,data:[设备快照]}|
|单设备|GET / 或 /health|无|{ok:true,data:{serial,port,status:"running"}}|
|单设备|GET /device|无|{ok:true,data:{serial,name,brand,model,androidVersion,connectionType,groupId,port,baseUrl}}|
|单设备|POST /shell|Command:string，UseRoot:bool|调用 ExecuteRemoteCommand/ExecuteRemoteRootCommand；{ok:true,data:{output}}|
|单设备|POST /tap|X:int，Y:int|input tap X Y；{ok:true}|
|单设备|POST /swipe|StartX,StartY,EndX,EndY,DurationMs:int|DurationMs<=0 时用 300；input swipe；{ok:true}|
|单设备|POST /text|Text:string|AdbService.SendText；{ok:true}|
|单设备|POST /app/start|PackageName:string，Activity:string 可选|有 Activity 时 am start -n 包/Activity，否则 monkey -p 包 -c android.intent.category.LAUNCHER 1；返回 output|
|单设备|POST /app/stop|PackageName:string|am force-stop 包；返回 output|

请求由 StreamReader（请求 encoding 或 UTF-8）ReadToEndAsync，再 Newtonsoft.Json 反序列化；响应 SerializeObject→UTF-8→Content-Type application/json; charset=utf-8→写入并关闭 response stream。错误统一 {ok:false,message}，可见 400/404/410 分支。这里只确认静态分支，不声称在本机已监听这些端口，也未验证所有异常的状态码。

函数定位：HandleGateway 0x06002fa8/RVA 0xcad68（状态机 IDA 0xd5890）；HandleDevice 0x06002fa9/0xcadb4（0xd5540）；CreateDeviceSnapshot 0x06002faa/0xcae10；WriteJson 0x06002fab/0xcaed8（0xd72e0）；HandleShell 0x06002fb0/0xcb058（0xd5b00）；Tap 0x06002fb1/0xcb0a4；Swipe 0x06002fb2/0xcb0f0；Text 0x06002fb3/0xcb13c；StartApp 0x06002fb4/0xcb188；StopApp 0x06002fb5/0xcb1d4；ReadRequest 0x06002fb6/0xcb220。DTO 字段 token：0x040023dc..0x040023e8。

## 5. 群控和脚本核心算法

### 群控输入链

Router.HandleMouse（0x0600325d/RVA 0xd7ad8）/HandleAndroidKey（0x06003261/0xd7f04）→ Coordinator.Dispatch（0x0600328b/0xd87cc）→ MultiControlInputService.Enqueue（0x060031dd/0xd3f48）→专用输入 worker→Dispatch（0x060031f7/0xd4880）→源设备先投递→QueueTargetFanout→每目标 mailbox→JXScrcpyCore 控制消息。

输入结构携带源设备、目标快照、鼠标/按键、画布/视频尺寸、generation、是否完成手势等。Enqueue 只在 generation、gestureId、排除的源 session 与序列号一致时将 MOVE 合并为最新位置，遇到 DOWN/UP 等非 MOVE 边界时先冲刷待发 MOVE，再把边界加入 FIFO。Dispatch 用 MainControlInputHandoffPolicy 比较 generation/取消状态，防止主控切换后的旧请求错误落到新会话。真实取消手势和完成手势的例外条件由该 policy 决定，不简化为丢弃所有旧 generation。

StartWorkerIfNeeded（0x060031e1/0xd4114）以 Interlocked CAS 启动后台线程 JX MultiControl Input；RunWorkerLoop（0x060031e3/0xd4208）等待 SemaphoreSlim 后 drain。FanoutScheduler.Enqueue（0x06003223/0xd618c）按设备 key 找 mailbox，移除过时 MOVE，加入工作项，把未排程 mailbox 放入 ready queue；ProcessMailbox（0x06003227/0xd6600）逐项执行目标委托，某目标异常记录后继续。多个 mailbox 可并行；同 mailbox 保持顺序。

ScaleControlPoint（0x0600321b/0xd5ef4，IDA 0xe07a0）对每个目标：x=clamp(trunc(point.x*videoWidth/max(actualWidth,1)),0,videoWidth-1)，y 同理。DispatchMouseTarget（0x0600320e/0xd578c）按尺寸缓存消息；SendMouse（0x06003210/0xd586c）创建触摸或滚轮 snapshot，再入控制队列。这支持混合分辨率屏幕，但旋转/黑边等更上游转换应结合投屏层验证。

### 脚本包与运行链

ScriptPackageService.Load（0x06003edb/RVA 0x10c18c，IDA 0x119dd0）打开 .jxscript ZIP，读取 manifest.json 和 script.json，用 Newtonsoft.Json 解析；Manifest.Id 必填；缺少 Steps/Parameters 时创建空集合；NormalizeAndValidate（0x06003edd/0x10c364）进一步检查。保存反向写两个 JSON entry。安装目录为 %APPDATA%/JiXingCloudControl/Scripts/Imported（cctor 0x06003ee2/0x10ca64）。

结构：Manifest {Id,Name,Version,Description,Entry,MinimumAppVersion,AppName,Category,AppDownloadUrl}；ScriptPackage {Manifest,Steps,Parameters}；ScriptStep {Action,DurationMs,Probability,Value}；ScriptParameter {Key,Type,LabelZh,LabelEn,Unit,Min,Max,Step,DefaultValue,Value}。字段详见 cloud-schema.json，存在 Probability 字段并不等于已证明其概率应用语义。

ScriptTaskSchedulerService.StartAsync（0x06003f3f/0x10f594）校验设备/包/序列号，在锁中查 dictionary<serial,CancellationTokenSource>，同设备已有运行时仅记日志并返回 completed task；否则创建 CTS、清空日志、记开始并 RunAndReleaseAsync（0x06003f42/0x10f788）。Stop（0x06003f40/0x10f69c）执行 CTS.Cancel；OnDeviceDisconnected（0x06003f43/0x10f7ec）调用 Stop。UI 文案虽有 Pause，实际是取消；未发现通用断点恢复承诺。

ScriptExecutionService.RunAsync（0x06003ef0/0x10cc94，状态机 IDA 0x11af90）逐 Step 检查 cancellation、IsExecutableStep、记录步骤，再 ExecuteStepAsync（0x06003ef1/0x10ccf0，状态机 0x11ac20）。已验证通用动作：launchapp/launch→monkey 启动；wait/sleep→Task.Delay；click→解析坐标并 input tap；swipe→解析参数并 input swipe。另有特定应用脚本分支 TikTokWarmupScriptService，调用 AutomatorApiServiceManager.AcquireScriptDeviceAsync 获取 JXDevice；其行为不能代表所有包都支持通用任意脚本语言。

RunAndReleaseAsync 的 finally 仅在字典中 CTS 仍为本次实例时移除，并 Dispose、记结束；非取消错误记录 Script_LogError 后重新抛出；取消异常被处理并最终释放。ScriptRuntimeLogService.Append（0x06003f59/0x10fcf8）按 serial 保存 Timestamp/Message，单设备最多 500 条，超出移除最旧；锁外发布 EntryAdded。

## 6. 配置、数据库、语言与日志

### 配置

StaticConstant 的构造器（IDA 0x50f20）以 BaseDirectory 定位 AppConfig.json、Resources/Adb/adb.exe、Resources/JiXingKeyboard.apk、Resources/Static/aapt、CK、Screencap 等。

AppGlobal.LoadAppConfig（0x060004d6/RVA 0x2b668，IDA 0x28fa0）：读主 JSON并验证；失败尝试 .bak；再失败使用默认；归一化数值和集合（设备墙列数夹在 5..20）；必要时保存。SaveAppConfig（0x060004dc/0x2bda8）：锁内重读磁盘版本，根据 last-saved baseline/current/disk 合并本地修改，避免直接覆盖磁盘上的并行更改；序列化后交给 FileUtil.SaveTextFile（0x0600281a/0xad0e4）。后者创建目录，写 .tmp（UTF-8 无 BOM），已有文件用 File.Replace(tmp,path,bak,true)，首次用 Move；失败清理临时文件返回 false。

46 个字段包含：主屏/小屏分辨率、帧率、码率；墙列数和缩放；ForceHardwareDecode；重启恢复时序；AutoConnect、SyncMouse、SyncKeyboard；API 网关端口/自动启动；语言、显示 FPS、设备列表模式；窗口坐标尺寸和菜单折叠；文件根目录；用户可见应用包集合；每设备小屏分辨率 profile。详见 cloud-schema.json，字段 token 0x04001b3f..0x04001b6c。

### 数据库

LocalDatabaseService 默认 BaseDirectory/db.sqlite3（IDA 0x2f5c0）；EnsureInitializedAsync 0x060006b2/0x31148；ResolveDatabasePathAsync 0x060006c0/0x31240；RunSchemaMigrations 0x060006c2；RunDataCompatibility 0x060006c3；有迁移前备份、完整性检查、无效库隔离和待迁移恢复方法。CreateAsyncConnection（0x060006d0/0x31818）把路径、Password 和 StoreDateTimeAsTicks=true 传 SQLCore。这里只确认样本代码设置 password；未打开数据库，也不以此单独证明实际磁盘的加密状态。

模式层观察到 11 个实体：Group、DeviceGroupRelation、DeviceRemarkRelation、EthernetScan、CustomDeviceCommand、ScriptDeviceConfiguration、EnvironmentConfig、Proxy、Server、AppCk、User。关系由 GroupId、RoSerialNo/DeviceKey、ScriptId 等字段表达；未读取实际行，未验证数据库约束、索引或完整建表 SQL。EnvironmentConfig 具有国家/语言/时区/SIM/位置与 KeyboxXml/PifJson 字段，仅说明配置模型存在，不能作为宣传中检测通过的证据。含凭据的字段只保留字段名。

SQLCore.SQLiteConnectionFactory.CreateConnectionString（0x0600003c/RVA 0x2554；IDA 0x6f0）校验 settings 和数据库路径；EnsureInitialized（0x0600003d/0x25ab；0x740）Interlocked.Exchange 单次调用 SQLitePCL.Batteries_V2.Init；创建 SQLite-net ConnectionString 时传入路径、ticks 设置和 password。MigrationRunner 用 PRAGMA user_version 读写版本；RepositoryBase<T> 包装 Find/GetAll/Where/Insert/InsertOrReplace/Update/Delete/Execute；UnitOfWork 提供事务封装。依赖 SQLite-net 1.9.172.0、SQLitePCLRaw.batteries_v2 2.1.2.1721。

### 本地化和日志

LanguageCore.LocalizationService.Init（0x06000020/RVA 0x238a；IDA 0x430）注册 assembly/base-prefix/lang-file/支持语言，LoadLanguageConfig（0x06000021/0x23b0）读 Language.Default.AppCulture 或选择默认语言；ChangeCulture（0x06000024/0x2514）设置当前/默认线程 culture、发 LanguageChanged 并保存 Settings。GetString（0x06000026/0x2594）按 (Assembly,baseName) 缓存 ResourceManager，找不到资源显示 !!key!!；LocExtension/LocalizationProxy 接入 WPF binding，支持动态语言切换。

App.OnStartup 直接将 CommonCore.LogUtil.EnableFileLogging=true、MinimumLevel=1；因此不能将 CommonCore 库静态构造默认值 false/3 当作云控运行配置。脚本每设备环形日志见上；普通日志/崩溃记录使用 CommonCore.LogUtil、CrashReportService 和 trace 转接。日志保留/轮转由 CommonCore 实现，详见对应模块报告。

## 7. 反推界面规格依据

静态证据支持的导航/界面组成：登录启动窗口；主窗口壳和可折叠菜单；设备墙/列表与分组；主控/悬浮控制窗；应用管理弹窗；电脑/手机双面板文件浏览；文本/剪贴板；API 服务；脚本管理；代理/环境/备份设置。

API 面板：设备选择、搜索、分组/状态过滤、全选、范围选择、批量启动/停止、运行数/摘要、网关端口/自动启动配置。依据 ApiServiceLayoutViewModel 的 Search 0x0600127a/0x635b0、ToggleAllSelection 0x0600127b、SelectRangeFromAnchor 0x0600128a、BuildGroupOptions 0x0600128d、BuildStatusOptions 0x0600128e、BatchStartAsync 0x06001280/0x636b0、BatchStopAsync 0x06001281/0x636f4。ItemViewModel 提供 ConnectionType 等显示属性。

应用面板：设备/分组选择、列表/过滤，启动/停止操作；清除数据和卸载分别弹 AppClearConfirmDialog/AppUninstallConfirmDialog 并考虑其他已选设备。文件面板：电脑盘符和路径面包屑、手机目录、刷新/返回、双向同步按钮、进度与 busy 状态；未选手机或文件会 Growl.Warning。

**界面规格仅能反推信息架构、交互和状态，不能从这些证据精确还原每个像素、颜色、间距或确认安装样本与用户截图版本一致。** 未执行 WPF/BAML 可视化，默认布局/隐藏菜单的可达性需运行验证。

## 8. 已知限制与后续验证

- 本报告的已观察项是静态 IL/元数据，推断架构与复刻方案必须与已观察行为区分。
- 未验证 20 台设备的实时帧率、延迟、线程峰值、重连成功率，未发送 API 请求，未测试 ADB 命令。
- HTTP 8080/8001..8999 和 Automator 17171 是样本默认/分配逻辑，不是当前系统端口实测。
- 样本引用的外部库、设备端 ROM、内核、SIM/网络环境、检测兼容性不由此 EXE 全部实现；不能由 UI 字段推导检测通过。
- 新实现建议优先复刻设备目录、分组、屏幕墙、保序输入、可取消脚本、公开 API 契约；使用独立配置与数据目录，避免复制原产品账号/凭据或迁移用户数据库。该建议是新产品取舍，不是目标原始行为。

核心可复刻控制流见 cloud-pseudocode.md。所有代码均为依据静态控制流人工整理的伪代码，未声称是 Hex-Rays 输出或可直接编译源码。

## 9. 字符串恢复方法与交叉校验

样本把 5,530 个字符串访问器放入 PrivateImplementationDetails 的 70E861AA-7B68-4192-B585-38250EA333D5 类。其静态构造器 0x06005668（IDA 0x1609a0）加载 172,224 字节 FieldRVA blob；按 decoded[i]=encoded[i] XOR (i & 255) XOR 0xAA 还原。访问器 helper 0x060040cd（IDA 0x123a30）以 UTF-8 offset/length 解码并缓存。原始字段 0x0400328c 的 RVA=3134376、文件偏移=3126696。恢复仅计算只读样本数据，未加载程序集执行初始化。

为避免 IDA 将整数展示为符号名导致解析错误，访问器四个常量（缓存索引两次、offset、length）最终均从原始 IL opcode 解码，核验长度与边界；映射文件保存 token/RVA/IDA EA。HTTP 路由、配置路径、脚本 entry 名称等结论来自这一校正后的映射与调用点交叉确认。数据库 credential 对应项已脱敏，不在报告输出其值。
