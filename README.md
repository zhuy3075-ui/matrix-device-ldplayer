# 矩阵设备文档还原工程

依据 `docs/reference/` 的 11 份文档独立编写的 C# 工程。界面统一简体中文，增加雷电实例管理及官方命令行适配。覆盖设备目录、截图墙与群控、节点调试、Shell、应用与文件、脚本、两类 HTTP API、配置、SQLite 元数据、诊断。原始源码、私有手机 JAR、完整视频握手、商业登录与 ROM 功能不在文档中，不能逐字节恢复。

新版桌面采用侧栏导航和蓝白主题，当前共12页，包含“实时监控”。所有页面已移除搜索栏和“当前设备”摘要行；需要单设备命令的页面保留可交互的“单设备目标”选择。设备支持卡片/屏幕/列表视图。最新布局、组件与验证边界见[控制台调整报告](docs/codex/unified-console-ui/REPORT.md)。构建后可运行 `./tools/Start-Desktop.ps1 -Demo` 查看演示界面；真实模式不加Demo，雷电路径仍需显式配置。

设备管理使用一个侧栏入口和一份统一目录，关闭、启动中、待接入和已连接实例同屏显示，也可切换卡片或屏幕视图。不再有“全部设备”“高级管理”切换或独立雷电页面；启动、接入、新建、资源配置、关闭、重启均在同页，“实例快捷操作”折叠区保留应用、文字与按键操作。模拟器数量不等同于可控设备数量。官方能力、限制和删除建议见[统一管理调研](docs/testing/unified-devices/OFFICIAL-RESEARCH.md)，一键启动及窗口行为变更见[一键启动报告](docs/testing/one-click-start/REPORT.md)。

完整文件清单见 `PROJECT_TREE.md`；全部源码文本见 `SOURCE_BUNDLE.md`；对应文件摘要见 `SOURCE_MANIFEST.json`。这三个文件由 `tools/New-SourcePackage.ps1` 生成，源码 ZIP 位于 `artifacts/`。

## 构建与运行

需要 Windows 10/11、[.NET SDK 10.0.402](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)。依赖固定于项目文件与 `packages.lock.json`，NuGet 源位于 `NuGet.Config`。本工作区已有通过官方 SHA-512 校验的便携 SDK，路径为上级 `.tools/dotnet/dotnet.exe`。

从工程目录运行：

```powershell
./tools/Build.ps1 -Verify
../.tools/dotnet/dotnet.exe ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --demo
```

在其他电脑上可把第二行的 SDK 路径换成 `dotnet`。真实模式去掉 `--demo`，配置雷电命令行路径并重启，在“设备管理”点击行内“启动”，或勾选后点击“启动所选”；“一键启动全部”处理全部未连接的独立实例。一次点击会启动关闭实例、核验实际ADB连接与安卓启动/截图，成功才保存接入并加入群控，不再弹普通启动确认框、不强制跳页；阶段进度、失败与取消留在原页面。已经运行的实例只核验连接，不重复启动。原实例0受保护，不参加自动接入、批量启动或资源修改；本轮不替用户操作原0。关闭、重启、新建与资源配置仍保留必要确认。

已移除“后台启动”开关及启动时自动隐藏窗口的逻辑。“启动”、 “启动所选”和“一键启动全部”均按雷电默认方式显示窗口，不主动隐藏或最小化，也不强制抢占前台。为兼容旧版，保留带PID、进程出生时刻、安装目录和窗口标记核验的隐藏窗口恢复记录处理；恢复失败仍保留记录并提示，不关闭模拟器。实时监控及显式打开既有窗口不受影响。

连接变更前需停止本程序的API、脚本、文件传输及网络扫描，等待请求排空；雷电/ADB路径等手工配置保存后仍需重启，不能混用新路径与旧运行时。高级串号JSON和索引绑定保留给明确知道真实串号的用户，不按端口公式授信。本程序直接连接现有5037服务，不执行adb.exe、不创建/kill服务；服务进程变化或协议不兼容会拒绝继续。无需原产品DLL、账号或数据库，未授权时不查询guest、不伪造在线设备。

实例目录每5秒自动更新，已授权设备状态最多每15秒查询一次；自动截图默认开启，仅前台设备群控页每5秒轮转最多4台，单轮有20秒预算。旧刷新/扫描按钮已移除，网络连接与应急目录更新收在设置齿轮的右键菜单。

“一键启动全部”逐台等待，每次冷启动按该实例配置内存加512 MiB进程开销、1024 MiB系统预留估算物理内存建议值，不再强制要求11 GiB或负载低于80%。可用物理内存低于建议值、可用提交额度低于实例内存加512 MiB、负载达到90%，或无法可靠读取配置/系统内存时，提示风险，由用户选择继续或取消；这些均是本程序的经验性提醒，并非雷电官方启动限制或启动成功保证。资源充足时无确认弹窗；已运行实例只连接，不做冷启动资源检查。超时或取消即停止后续，不关闭已启动实例。详见[启动内存策略与验证](docs/codex/launch-memory-policy/REPORT.md)。

仅本次用户点击的目标可经过真实身份检查后保存授权，后台自动发现不启动或授权。勾选中含未连接实例时，批量应用、传输、脚本与同步输入拒绝执行，避免落到旧的当前设备或悄悄只操作部分设备。

“新建空白实例”和“资源配置”使用官方`add/modify`，只开放CPU、内存、分辨率和DPI；配置要求实例所有进程/窗口完全退出，命令返回不代表实际资源已核验。复制涉及账号/应用数据，本轮仅调研；删除不可逆，不开放一键删除。Android版本、小磁盘、节能等高级GUI选项没有稳定公开CLI保证，不伪造支持。

```powershell
dotnet ./src/MatrixDevice.Cli/bin/Release/net10.0/MatrixDevice.Cli.dll --demo --serve --basic-port 18080 --automator-port 17172
```

CLI默认真实模式；`--demo`显式模拟，`--serial`授权唯一设备，`--data`独立目录。旧`--adb`字段兼容保留，不运行该二进制。`--serve`启动网关、在线设备端口和发现轮询；Ctrl+C清理本程序资源。

雷电无需配置 ADB 即可查询和控制实例。桌面启动可加 `--ldconsole 'E:\leidian\LDPlayer14\ldconsole.exe'`，或者在设置中配置路径并重启。独立只读查询不会构造 ADB 后端：

```powershell
../.tools/dotnet/dotnet.exe ./src/MatrixDevice.Cli/bin/Release/net10.0/MatrixDevice.Cli.dll --ldconsole 'E:\leidian\LDPlayer14\ldconsole.exe' --ldplayer-list
```

官方提供本机 CLI 和 ADB，不是官方 REST 服务；本工程将允许的 CLI 动作封装成本机 HTTP。接口字段、功能矩阵和当前本机限制见 [雷电接入说明](docs/LDPLAYER_INTEGRATION.md)。不得自动替换现有共享 ADB 服务，也不要把同一实例的两条串号同时纳入群控。

当前 Windows 缺少完整 CET 支持，SDK 编译器 apphost 会启动失败。`Build.ps1` 通过未修改的 `dotnet exec csc.dll` 托管入口编译；本工程 apphost 设置 `CETCompat=false`，只影响本工程程序，不修改系统设置或 SDK 二进制。更新环境后可按需恢复默认属性。[微软 CET 说明](https://learn.microsoft.com/en-us/dotnet/core/compatibility/interop/9.0/cet-support)

## 模块

| 项目 | 职责 |
|---|---|
| MatrixDevice.Core | 设备/节点契约、雷电 CLI/列表兼容/实例代次与命令校验、ADB 后端、逐设备队列、输入协调、PNG、视频/触摸编码、手机 HTTP 客户端、网络发现 |
| MatrixDevice.Features | 脚本包与调度、27 条自动化动作、节点搜索/等待、调试上下文、应用/传输、剪贴板边界、代理配置、应用备份格式与操作 |
| MatrixDevice.Host | 双网关/逐设备监听、配置合并恢复、SQLite、日志、运行时资源整合 |
| MatrixDevice.Desktop | WPF 页面、ViewModel、截图/取色/裁剪、脚本参数、独立子系统状态、原生 scrcpy 窗口 |
| MatrixDevice.Cli | 无界面枚举与本机 API 服务 |
| MatrixDevice.Tests | 默认离线/协议/文件/竞争/loopback验证；显式--live-acceptance读取冻结目标回执才进入独立实例测试 |

## 应用管理

左侧“应用管理”使用独立的模拟器勾选列表，不自动继承主控或群控目标。选择已接入在线实例后自动读取应用，右侧按“模拟器串号 + 包名”展示独立行，可切换是否显示系统应用。名称或版本未取得时如实显示，不承诺APK图标和标签解析。

安装支持多选APK，确认全部目标与文件后逐项执行；卸载仅针对实际勾选的应用行，确认串号、包名和不可撤销的数据损失，执行前重新检查系统应用，执行后刷新列表核验对应包已移除。底部显示逐项状态、实际任务进度与失败原因；“停止后续”不回滚或中断已经提交的操作。更多菜单保留启动、停止和清数据，清数据仍需确认。确认后目标、运行时、连接代次或transport变化会拒绝执行，不回退到旧设备。

安装暂未解析本地APK包名，因此显示“安装命令完成、列表已刷新、包名未核验”，不冒充精确安装验收；启动、停止和清数据的命令完成也不等于其运行效果已核验。本轮仅离线验证批次及界面，未对真实应用执行安装、卸载或清数据。

## API 与脚本

普通网关默认 `http://localhost:8080/`，GET `/health`、`/devices`。每设备 8001..8999 由 API 面板显式启动；GET `/device` 和 POST `/shell`、`/tap`、`/swipe`、`/text`、`/app/start`、`/app/stop`。成功为 `{ok:true,data:...}`，失败 `{ok:false,message:...}`。普通 swipe 使用 `DurationMs`，非正值归一为 300。

Automator 默认 `http://127.0.0.1:17171/`，路径允许 `/api/` 前缀。所有 27 条 POST 都包含 `serial`，响应 `{code:0,message:"ok",data:...}`。具体路由与字段以 `docs/reference/API.md` 第4节和源码 `AutomationService.Routes` 为准，swipe 使用 `steps`/`stepMs`。错误映射和本次差异见 `docs/ASSUMPTIONS.md`。

`.jxscript` 是包含 `manifest.json` 和 `script.json` 的 ZIP。`examples/basic-example.jxscript` 可直接导入，对应可编辑文件在 `examples/basic/`；支持 `launchapp/launch`、`wait/sleep`、`click`、`swipe`。在专用设备上确认坐标后运行；取消表示结束，不能续跑。默认拒绝未知动作。参数与 Probability 的证据边界见假设表。

## 数据与设备边界

默认数据位于 `%LOCALAPPDATA%/MatrixDevice.Reconstructed/`；桌面 demo 与真实模式使用分离子目录。配置主文件/备份、SQLite 和日志均为本工程所有，不读取 CODEBRAIN 或旧产品数据。SQLite 使用新的 schema version=1；没有原产品密码或迁移兼容承诺。

截图墙使用实际截图，ADB 群控将完成的手势转换为 tap/swipe。原有投屏入口通过用户提供的独立 `scrcpy.exe`，只附着本工程启动且核验 PID/开始时间的窗口；它与新监控视频链路分开。原私有视频流只还原已定义包编码，未实现未知握手。不得以截图刷新验收原产品实时视频、音频或 20 台解码性能。

## 实时监控

接入并核验独立雷电实例后，点击左侧“实时监控”：默认只显示已连接设备，每个可见卡片自动播放视频，不需要选中或放大。画面最大边320像素，源端上限10帧/秒、1 Mbit/s，UI最多8次/秒呈现并丢弃未显示的旧帧。“资源列表”暂停视频、保留资源采样；暂停、切页、滚出视口及退出都会释放对应视频会话，不关闭模拟器。静止页面可能不产生新帧，时间戳与等待状态独立显示；视频到帧率不是应用或游戏FPS。

在“环境设置”指定`监控视频组件`和`监控 FFmpeg`后重启。视频组件固定为[官方scrcpy-server v3.3.4](https://github.com/Genymobile/scrcpy/releases/tag/v3.3.4)，SHA256为`8588238c9a5a00aa542906b6ec7e6d5541d9ffb9b5d0f6e1bc0e365e2303079e`；从[官方发布附件](https://github.com/Genymobile/scrcpy/releases/download/v3.3.4/scrcpy-server-v3.3.4)单独取得，不能用任意版本替换。FFmpeg路径留空时只查找PATH中的已安装`ffmpeg.exe`。两项二进制不随源码包分发。

新链路只使用已有、经身份核验的ADB服务，经独立socket推送并启动唯一临时官方server，读取H.264后交自有FFmpeg解码；不运行外部ADB客户端、不建立共享forward、不启用音频、控制、剪贴板或亮屏。仅清理本次会话的临时文件/经nonce、PID出生证明核验的自身进程。若清理不能确认，同一后端拒绝该设备再次启动视频，需先人工核查，不能靠重开程序绕过；该阻断不是跨进程持久锁。

主机CPU/物理内存与每实例CPU/工作集约2秒采样一次，缓存当前会话最多15分钟趋势。实例CPU按全机逻辑核归一，工作集为Player+VBox进程求和、包含共享页，不是安卓可用内存；GPU、网络、磁盘和应用FPS尚未接入。首个CPU样本/权限不足/进程更换显示未知，不伪造0。多处理器组机器的整机采样保守报告未知。

单击卡片仅改变监看详情，不改变主控、操作目标或节点。双击卡片或点击右上角窗口图标，核验现有进程/窗口后请求显示对应雷电窗口，可在那里用鼠标键盘操作；不启动关闭实例，不操作原实例0。节点调试首次进入或切设备时自动读取定位用的截图和节点，视频帧不会覆盖节点坐标。专项验证与限制见[监控报告](docs/testing/realtime-monitoring/REPORT.md)。

2026-10-15限定验收：专用实例1/2的R5实机11项通过，包括双路连续192.401秒、全部目录45.503秒、隐藏窗口后收新帧、暂停恢复及离页清理；根离线137/137。此结论不代表4/23路或长时稳定性已通过。可用`./tools/Start-Desktop.ps1 -DataDirectory '已明确配置授权的绝对目录' -Monitor`直接打开监控页；不自动启动模拟器或授权其它实例。

手机 agent 可通过 `AgentSerial` + `AgentBaseUrl` 显式绑定一台设备，用于已定义节点文本/服务请求。不配置时不会部署未知 JAR，也不会把一个 URL 用于所有 serial。私有 ROM SIM、代理部署规则和剪贴板协议会明确返回不支持。

中文全局输入：仅对同时列入授权串号及唯一`LdSerialBindings`的运行实例使用雷电call.input，并先验证现有ADB代次。普通ASCII走ADB。LD绑定的所有ADB操作及SDK应用/输入入口都核验冻结的实例身份，变化后需在设备管理重新接入；ADB服务进程变化必须重启本程序，不自动接受新服务。节点指定set-text仍需兼容agent，不冒充节点输入已实现。动态视频页uiautomator可能不生成快照，界面明确报错并清空旧节点。

## 验证与交付

`tools/Build.ps1 -Verify` 构建六个项目并运行全部验证。`--smoke --screenshot ABSOLUTE_PATH` 自动使用 demo 渲染各页面后退出；真实设备验收步骤见 `docs/VERIFICATION.md`。文档逐项覆盖见 `docs/COVERAGE.md`，缺失/矛盾处理见 `docs/ASSUMPTIONS.md`。

本轮计划与实机日志见[测试计划](plan.md)、[阶段记录](docs/testing/TEST_LOG.md)、[缺陷账本](docs/testing/DEFECTS.md)。原始截图/节点/响应位于私有artifacts/acceptance，不进入公开源码包；离线、实机、拒绝路径和未实现能力分别标记。

历史单实例验收结论见[独立实例报告](docs/testing/REPORT.md)，本轮多实例结论以[专项报告](docs/testing/multi-instance/REPORT.md)为准。本机可双击`tools/Start-VerifiedDesktop.cmd`或在PowerShell 7运行对应`.ps1`，使用保留的测试实例1及本机独立配置；不调用原实例0。源码包不分发该授权配置，换电脑须显式重新确认。

多实例任务另见[多实例计划](docs/testing/multi-instance/PLAN.md)、[测试记录](docs/testing/multi-instance/TEST_LOG.md)和[报告](docs/testing/multi-instance/REPORT.md)。显式测试入口为Tests.dll `--multi-acceptance spec.json prepare|workload|diagnose 绝对证据目录`，只允许非零独立目标、唯一串号、实际进程绑定及资源预留；diagnose仅收集授权guest的活动/窗口/进程及节点截图，不操作原0。型号测试只走冷态官方CLI，不开放通用身份修改HTTP。HTTP Toolkit安全/许可未确认时直连轮不计抓包。

本机多实例桌面入口为`tools/Start-MultiVerifiedDesktop.cmd`，只读取私有`data/multi-verified`内已明确授权的非零独立实例配置；它不启动/关闭模拟器，程序会在启动时重新核验实际绑定。默认API/输入同步关闭。359MB测试APK使用300秒命令超时；专项驱动另有45秒UI预算，不能把这个预算误认为桌面所有控件已获得同样的期限。私有配置不进源码ZIP。

依赖来源：[AdvancedSharpAdbClient](https://www.nuget.org/packages/AdvancedSharpAdbClient/3.6.16)、[Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.12)、[SkiaSharp](https://www.nuget.org/packages/SkiaSharp/4.153.1)。不复制厂商二进制。
