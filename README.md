# 矩阵设备文档还原工程

依据 `docs/reference/` 的 11 份文档独立编写的 C# 工程。界面统一简体中文，增加雷电实例管理及官方命令行适配。覆盖设备目录、截图墙与群控、节点调试、Shell、应用与文件、脚本、两类 HTTP API、配置、SQLite 元数据、诊断。原始源码、私有手机 JAR、完整视频握手、商业登录与 ROM 功能不在文档中，不能逐字节恢复。

新版桌面采用侧栏导航和浅色中性主题，12页显示名为设备、监控、界面检查、截屏、终端、应用、文件、文本、自动化、接口、工具、设置，内部Tag和功能保持原值。共享字体、状态/焦点、中文可访问名称与120ms局部反馈已通过[独立阶段验收](docs/codex/apple-experience/A03_APPEARANCE.md)。[公共响应式阶段](docs/codex/apple-experience/A04_RESPONSIVE_SHELL.md)已统一最低外窗390×640，六尺寸验证导航抽屉、公共字段和指定自有弹窗；固定页面正文、触控完整信息、真实DPI与实机仍待后续，不宣称全页小屏已完成。所有页面已移除搜索栏、“当前设备”摘要及分组筛选；需要单设备命令的页面保留可交互的“单设备目标”选择。设备管理固定列表，实时多画面使用独立“监控”，不会删除已有分组元数据。最新布局与验证边界见[列表控制台精简](docs/codex/list-console/REPORT.md)，历史四项布局见[控制台调整报告](docs/codex/unified-console-ui/REPORT.md)。构建后可运行 `./tools/Start-Desktop.ps1 -Demo` 查看演示界面；真实模式不加Demo，雷电路径仍需显式配置。

设备管理使用一个侧栏入口和一份统一列表，关闭、启动中、待连接和已连接实例同屏显示。已移除“全部设备/高级管理”、视图切换、实例目标下拉及重复的“启动并连接/接入已运行实例”。表头复选框统一全选与取消选择，不删除设备；启动所选和行内启动自动判断是否需要冷启动，已运行实例只连接。“实例操作”的资源配置、关闭、重启仅操作列表中唯一勾选的有效实例，多选或空选不回退到高亮行/旧目标。新建空白实例保留直达，旧隐藏窗口恢复放在实例菜单，取消启动只在启动任务进行时显示。

“启动所选”包含被状态筛选隐藏的已勾选实例；“一键启动全部”包含全部尚未连接的可接入实例，不受当前状态筛选限制。两者排除受保护实例，已运行的仅连接；按钮禁用时仍可查看范围提示。顶部“更多”只放网络连接和立即更新目录，与“实例操作”职责不同。

状态筛选仅影响设备列表显示，不改主控、群控勾选或监控范围。详情仅保留独立设备选择、状态和改名，不再有第二套群控/隐藏全局筛选；改名原样保留旧分组元数据。表头选中当前列表可勾选行，取消会清除全部选择（含被筛选隐藏的项）；“启动所选”包括隐藏勾选，“一键启动全部”面向完整目录。状态列显示实际字段分类，原始状态和操作阶段在提示中保留；启动中动作称“等待连接”，不确定状态称“接入”，提示说明重新检查、条件启动、等待和可能失败，不表示自动修复或必然连接。详见[A05a隔离报告](docs/codex/apple-experience/A05_DEVICE_STATE.md)与[操作文案修复](docs/codex/apple-experience/A05_DEVICE_LABELS.md)，不等于真实视频连续性或启动效果已验。

设备页窄窗口固定勾选和操作两列，原14项信息仍可横向滚动，其它信息列保留调宽；行内动作、主控按键和同步开关使用44逻辑命中区。主控按实际工作区在侧栏与同区域展开间切换，返回入口固定，正文和投屏末端可自然滚到；缩放、离页、失活或拖出画面取消未完成手势，不重设主控、当前设备或勾选。返回保留可实现的滚动位置，新的滚轮操作不被旧恢复任务覆盖。六尺寸独立Demo验收和实测边界见[设备与主控适配报告](docs/codex/apple-experience/A05_DEVICE_RESPONSIVE.md)；不代表其它页面手机正文、物理输入、自动定时调度、真实DPI或实机均已通过。

已删除重复的实例快捷表单：应用安装、卸载、启停统一到“应用”，文字发送使用“文本”。已连接行的“控制”打开列表旁主控面板，保留主页、返回、任务、音量和投屏，关闭面板归还列表宽度。“同步点击滑动”只复制主控嵌入画面的手势；“同步按键”只复制程序内主页/返回/任务/音量键，二者目标都是列表明确勾选的其他设备，不监听雷电原生窗口，也不等于物理键盘镜像或文字广播。含未连接目标时拒绝同步。各独立页面选择不暗中互相授权。历史入口去重见[上轮报告](docs/codex/device-action-dedup/REPORT.md)。模拟器数量不等同于可控设备数量；官方能力、限制和删除建议见[统一管理调研](docs/testing/unified-devices/OFFICIAL-RESEARCH.md)，窗口行为变更见[一键启动报告](docs/testing/one-click-start/REPORT.md)。

完整文件清单见 `PROJECT_TREE.md`；全部源码文本见 `SOURCE_BUNDLE.md`；对应文件摘要见 `SOURCE_MANIFEST.json`。这三个文件由 `tools/New-SourcePackage.ps1` 生成，源码 ZIP 位于 `artifacts/`。

全面验收与界面重设计正在按[当前计划](plan.md)实施；目标、技术边界、视觉/命名及依赖任务分别见[PRD](docs/PRD.md)、[SPEC](docs/SPEC.md)、[UI规格](docs/UI_SPEC.md)、[任务表](docs/TASKS.md)。当前测试范围见[功能矩阵](docs/codex/apple-experience/TEST_MATRIX.md)、[逐控件目录](docs/codex/apple-experience/CONTROL_CATALOG.md)和[问题清单](docs/codex/apple-experience/DEFECTS.md)，进展见[阶段记录](docs/PROGRESS.md)。已修复主控A/当前B时投屏错目标的代码路径，增加主控身份与进程所有权守卫，见[A02独立离线验收](docs/codex/apple-experience/A02_MAIN_TARGET.md)；真实外部投屏仍待验。这是进行中的工作，149项和三尺寸Demo不代表本轮全功能/实机/窄屏已通过。

2026-10-16新增任务R11-R14/A13-A16已纳入上述文档：继续全功能/双实例/安装卸载/最终30分钟验收，并规划终控台RPA闭环、群控MCP接入及指定模拟器抓包。最近[真实实例复核](docs/codex/list-console/LIVE_RECHECK_2026-10-16.md)为实例1冷启动及8项限域Live通过，不是全控件通过。现有脚本为受限ZIP格式，产品尚无群控MCP server或完整抓包会话；[抓包初步调研](docs/codex/automation-expansion/CAPTURE_RESEARCH.md)已比较方案，POC未运行，不把规划当实现。

自动化导入选择器现在只列出`.jxscript/.zip`。加载器对ZIP构造阶段识别出的非归档格式给出中文400/1007错误，不执行顶层JSON、Python或JavaScript；合法ZIP按内容读取，即使后缀为`.json`或包含合法附带资源也保持兼容。I/O、归档内JSON、动作验证及原限额保持原边界，不能把这项修改称为所有损坏ZIP的统一处理。实现方、独立方和主仓各169项通过，包含合法包实际Demo点击和非法内容零点击；[格式边界报告](docs/codex/apple-experience/A08_SCRIPT_FORMAT.md)保留原红测。GUI文件对话框实开、真实设备和完整RPA闭环仍待验收。

Windows数据库长路径初始化已按原位置修复，不搬移数据或关闭事务。独立及主仓正式回归均158/158，实际CLI的251/252/253/300字符普通路径均通过；迁移备份、未来版本拒绝与文件释放也有回归，见[长路径报告](docs/codex/apple-experience/A13_SQLITE_PATH.md)。完整GUI的输入源问题、真实双实例同步、新版应用页安装卸载和最终双实例30分钟仍待，不由本项离线结果替代。

## 构建与运行

需要 Windows 10/11、[.NET SDK 10.0.402](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)。依赖固定于项目文件与 `packages.lock.json`，NuGet 源位于 `NuGet.Config`。本工作区已有通过官方 SHA-512 校验的便携 SDK，路径为上级 `.tools/dotnet/dotnet.exe`。

从工程目录运行：

```powershell
./tools/Build.ps1 -Verify
../.tools/dotnet/dotnet.exe ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --demo
```

在其他电脑上可把第二行的 SDK 路径换成 `dotnet`。真实模式去掉 `--demo`，配置雷电命令行路径并重启，在“设备”点击行内“启动”，或勾选后点击“启动所选”；“一键启动全部”处理全部未连接的独立实例。一次点击会启动关闭实例、核验实际ADB连接与安卓启动/截图，成功才保存接入并加入群控，不再弹普通启动确认框、不强制跳页；阶段进度、失败与取消留在原页面。已经运行的实例只核验连接，不重复启动。原实例0受保护，不参加自动接入、批量启动或资源修改；本轮不替用户操作原0。关闭、重启、新建与资源配置仍保留必要确认。

已移除“后台启动”开关及启动时自动隐藏窗口的逻辑。“启动”、 “启动所选”和“一键启动全部”均按雷电默认方式显示窗口，不主动隐藏或最小化，也不强制抢占前台。为兼容旧版，保留带PID、进程出生时刻、安装目录和窗口标记核验的隐藏窗口恢复记录处理；恢复失败仍保留记录并提示，不关闭模拟器。实时监控及显式打开既有窗口不受影响。

连接变更前需停止本程序的API、脚本、文件传输及网络扫描，等待请求排空；雷电/ADB路径等手工配置保存后仍需重启，不能混用新路径与旧运行时。高级串号JSON和索引绑定保留给明确知道真实串号的用户，不按端口公式授信。本程序直接连接现有5037服务，不执行adb.exe、不创建/kill服务；服务进程变化或协议不兼容会拒绝继续。无需原产品DLL、账号或数据库，未授权时不查询guest、不伪造在线设备。

实例目录每5秒自动更新，已授权设备状态最多每15秒查询一次。列表没有自动截图开关，也不后台轮转截取全部设备；只有设备管理页激活且主控面板可见时，每5秒自动刷新该主控，单轮有20秒预算，打开面板也尝试取首帧。多设备实时视频仍在“监控”，不把定时截图当视频。旧刷新/扫描按钮已移除，网络连接与应急目录更新收在顶部可点击的“更多”菜单；设置齿轮仅进入设置，不再依赖右键。

界面检查、截屏和后台截图已分开校验请求来源、设备身份与当前检查修订号：切换设备或重新连接后，迟到结果不能覆盖新设备的截图/节点；身份仍有效的后台源缓存可以保留。旧任务的错误按原设备写入本机日志，当前目标已变更时不挂到新设备上；后端取消与内部身份过期分别处理，保留正常取消和失败恢复。49项受控Demo/STA专项及验证边界见[A06a报告](docs/codex/apple-experience/A06_DEBUG_PUBLICATION.md)，不代表真实ADB、自动首载、生产定时器或全部实机路径已验收；完整目标仍在进行。

已完成的截图和节点也分别保留来源身份：新Ready传输、几何或重连代次不再沿用旧检查，返回缓存、保存PNG/JSON和取色前重新核验VM行与实际目录。当前检查与后台较新行截图独立，不因合法后台新帧清掉同身份旧节点；稳定改名不反复清理。明确Offline且传输/几何不变、仅较来源增加一代的内容可作为本地只读历史查看和导出，不放宽新读取或输入；新身份或已经失效清除的内容不得复活。此资格只核消费时最新快照，不追溯VM未同步的完整中间状态。基线红、诊断纠正及独立/主仓验收范围见[A06c1报告](docs/codex/apple-experience/A06_COMPLETED_CACHE.md)；同行换代补载见下文，原生保存模态、未完成裁剪和真实runtime热换仍分项验收。

界面检查的自动首载现会保留暂时忙碌期间的加载意图：普通操作、传输或脚本结束后自动读取最终选择的设备，连续切换也不会丢失待办；已有节点不重复加载，离页/关闭不新增读取，失败不无限重试。16项真实窗口事件专项、独立三桌面/六公共布局及主仓复验见[A06b1报告](docs/codex/apple-experience/A06_AUTOMATIC_LOADING.md)。这是隔离Demo后端验证，不是Android实机或生产定时器通过；默认Demo仍不自动读取，只有正式验收自有窗口具有临时许可。

同一行设备重新Ready、传输或几何换代后，完整VM目录接纳也会自动补载当前图和节点；metadata稳定刷新不重读，明确离线历史不主动读取。正在运行的操作结束后只补最终身份，在途旧结果仍拒绝；已经被本次实际读取覆盖的待办同步消费，失败或取消后不因同身份轮询无限重试。新19项独立窗口场景、三桌面/六公共布局和主仓执行状态见[A06c2报告](docs/codex/apple-experience/A06_RECONNECT_LOADING.md)。不扩大为Catalog-only任意变化、Android、全部控件或完整目标已验收。

界面检查按实际正文宽度重排：宽屏三栏，中等窗口为左画面、右侧节点与属性上下排列，窄屏通过等宽的“画面 / 节点 / 属性”切换原内容。缩放与切换不重新截图、重建树或改变设备目标；属性阅读位置保留，新滚动、选区或节点文本会取消旧恢复。四个工具和树头选择区采用44逻辑命中，树展开箭头仍待后续专项，不宣称全部控件均已触控优化。六尺寸及11项受控Demo场景见[节点正文适配报告](docs/codex/apple-experience/A06_NODE_RESPONSIVE.md)；物理输入、真实DPI、Android及原生导出对话框仍另验。

截屏正文也按可用宽度重排：宽屏画面与色值并排，窄屏使用“画面 / 色值”切换，保留同一图像、缩放、已完成裁剪、颜色列表和选择状态，不因重排重新读取设备。原四工具、操作模式及缩放整体采用44逻辑高度；长坐标、十六进制色值和RGB可通过表内横向滚动完整阅读，不承诺三列同时可见。六尺寸10项专项、独立三桌面/六公共布局和主仓复验见[截屏正文适配报告](docs/codex/apple-experience/A06_SCREENSHOT_RESPONSIVE.md)；实际指针前置后的合成取色路由不等于物理点击，真实DPI、Android、原生保存、剪贴板和未完成裁剪仍另验。

“一键启动全部”逐台等待，仅在实例RAM已知且当前可用物理内存或可用提交额度严格低于该RAM时，才弹出资源不足确认。配置缺失/冲突、读取或采样失败、高负载，以及仅低于512 MiB进程开销/1024 MiB系统预留等估算余量，都只在页面提示，不弹窗，也不冒充资源充足。RAM优先读取实例设置，缺少该字段时只读同编号虚拟机当前配置；两个有效来源冲突则显示未知，不猜默认值。每次冷启动重新采样，检查不是雷电官方最低要求，也不保证启动成功。已运行实例只连接，不额外检查冷启动内存；取消仍停止后续，不关闭已启动实例。最新实现与验证见[启动资源确认修复](docs/codex/launch-resource-confirmation/REPORT.md)，旧策略记录保留在[历史内存报告](docs/codex/launch-memory-policy/REPORT.md)。

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

左侧“应用”使用独立的模拟器勾选列表，不自动继承主控或群控目标。选择已接入在线实例后自动读取应用，右侧按“模拟器串号 + 包名”展示独立行，可切换是否显示系统应用。名称或版本未取得时如实显示，不承诺APK图标和标签解析。

模拟器名称仅作为展示信息：同一串号、连接代次和transport下改名，已有应用行与勾选保持，名称即时更新，合法启停、确认和在途结果不再被误拒绝；换连接、离线或明确目标变化仍拒绝。此项已获独立离线验收；早先A07a主仓完整GUI在旧截屏坐标前置失败，该轮新应用专项未执行，原记录保留于[应用名称与身份报告](docs/codex/apple-experience/A07_APPLICATION_IDENTITY.md)。后续A07b1复验曾实际通过该10例及完整演示流程，但不解释间歇输入源问题的成因。

应用目标勾选后的自动读取会保留脚本、文件传输或普通忙碌期间的待办，结束后只读取最终明确选择；清空目标即取消，离页不读取、返回再恢复，关闭后不排新读取，读取失败不无限重试。脚本只阻塞它所在的所选设备，不把另一设备的脚本当全局锁。原350ms调度、连接身份和活动门禁保持。A07b1阶段独立及主仓各12例/310检查、150项离线测试和完整1000演示通过，见[应用自动读取报告](docs/codex/apple-experience/A07_APPLICATION_PENDING.md)。

API请求全部结束也会唤醒已有应用读取待办，不再依赖手动刷新；并发请求只释放一个时仍等待，实际读取前重核全部门禁，窗口关闭或目标清空不再读取。A07b2保留原版12例中8条真实失败，修复版独立及主仓各14例/496检查、原158项及完整12页演示通过，见[接口排空报告](docs/codex/apple-experience/A07_API_DRAIN.md)。这是自有Demo/真实loopback HTTP验证，不代表真实双实例、原生安装卸载界面、小屏正文或长期稳定性通过。

应用正文现按实际可用宽度排列：宽窗保留目标、应用及任务表，窄窗使用同区域的“目标 / 应用 / 任务”切换，进度与“停止后续”持续可见。原表、勾选、排序和批次对象不因重排而重建；全部列保留水平滚读，长字段可通过完整提示阅读。局部动作采用44逻辑命中区，不整页缩放。

本阶段独立及主仓应用正文各12项通过，含六尺寸三表首中末/全部列和单窄窗150%字形压力；后者使用自有控件本地字号覆盖，不是系统DPI或字体绑定保真验收。原Smoke调用链及12页、共享壳层六尺寸已通过；另立补验在设备指针前置和报告保存处失败，余11格未执行，不能宣称全GUI通过。原失败、诊断修正及主仓核验结果见[应用正文报告](docs/codex/apple-experience/A07_APPLICATION_RESPONSIVE.md)和[测量诊断记录](docs/codex/apple-experience/A07_TOOLTIP_DIAGNOSTICS.md)。真实双实例、当前安装卸载界面及最终30分钟仍待。

离线画面诊断有独立 `--diagnose-offline-geometry --demo --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY --width 390 --height 640` 入口，必须使用尚不存在的artifacts目录，不接受`--screenshot`，不请求激活或操作指针/真实模拟器。它依次执行14类[报告出口契约](docs/codex/apple-experience/AP041_REPORT_RELIABILITY.md)、[正文无障碍](docs/codex/apple-experience/AP042_WORKSPACE_ACCESSIBILITY.md)、[应用摘要](docs/codex/apple-experience/AP040_APPLICATION_FEEDBACK.md)和[原几何诊断](docs/codex/apple-experience/AP041_OFFLINE_DIAGNOSTICS.md)。`device-layout.json`仅局部编码非有限值为`{value:null,finite:false,kind:...}`，有限JSON、FAIL、原异常与全局严格JSON保持。历史阶段曾通过原158项、报告契约和几何诊断；最新复核止于AP042产品失败，后续未执行。报告契约自身故意注入的8条预期stderr与这次AP042失败是不同结果，不得混作无故障。完整GUI、双实例和最终长期稳定性缺口保持。

工作区容器使用保留原视觉样式的标准Pane无障碍peer；原无头Tab的UIA SelectionPattern/ItemContainer不再提供，侧栏导航保持。历史阶段的自有WPF设备/应用往返曾通过；最新[A14a1旧链复核](docs/codex/apple-experience/A14_SCRIPT_PARAMETER_BINDINGS.md)却在AP042再次真实失败：根节点146→146→146，三个应用owner缺失、旧owner残留111，其后摘要与几何未执行，当前不承诺正文往返已恢复。[受控官方观察](docs/codex/apple-experience/AP043_SKY_AB.md)曾证旧48个总节点止空Tab、新200个总节点含设备正文，但导航点击仍报输入几何错误、无后置状态；原null与根因保持开放。真实GUI安装卸载、双实例同步和最终30分钟仍未验收。

同一离线诊断入口现增加[原位子树记录](docs/codex/apple-experience/AP042_PEER_CACHE_DIAGNOSTICS.md)，不改变原查询、断言或等待。独立与主仓各193项、无窗参数VM4通过，但原页面诊断仍PRODUCT-FAIL；日志确认实际视觉切页后公开子树未更新，不证明唯一根因或产品已修复。该记录不替代真实GUI或模拟器验收。

新增受限的CMFA实机测试入口，仅供固定摘要、已验证专用实例与新证据目录使用，不限制正式应用页的通用APK选择。独立及主仓158项回归通过；实例1真实共享安装、精确2.11.35.Meta/211035及启动进程成立，但首次权限页挡住主页，原流程如实FAIL。后续另立[本轮owned停止/卸载验证](docs/codex/apple-experience/A07_OWNED_CLEANUP.md)，实际两个包进程停止、安装保留后再卸载，普通及含保留记录查询和列表均缺包，51步骤通过。当前测试安装已移除、原APK保留，未再安装/启动或授权VPN/权限，也未改原FAIL；详见[首轮记录](docs/codex/apple-experience/A07_CMFA_APPLICATIONS.md)。这些是共享后端证据，当前GUI、双实例同步及最终30分钟仍待，默认离线测试不触发实机入口。

安装支持多选APK，确认全部目标与文件后逐项执行；卸载仅针对实际勾选的应用行，确认串号、包名和不可撤销的数据损失，执行前重新检查系统应用，执行后刷新列表核验对应包已移除。底部显示逐项状态、实际任务进度与失败原因；“停止后续”不回滚或中断已经提交的操作。更多菜单保留启动、停止和清数据，清数据仍需确认。确认后目标、运行时、连接代次或transport变化会拒绝执行，不回退到旧设备。

安装暂未解析本地APK包名，因此显示“安装命令完成、列表已刷新、包名未核验”，不冒充精确安装验收；启动、停止和清数据的命令完成也不等于其运行效果已核验。批次摘要现在仅在确有非零未核验安装项时显示安装专属计数，读取、等待、取消或空集合不再出现误导性的“安装命令完成 0”；进度、完成、失败和取消数量保持，见[摘要红绿验证](docs/codex/apple-experience/AP040_APPLICATION_FEEDBACK.md)。此前A07离线阶段仅验证批次及界面，未对真实应用执行安装、卸载或清数据。

## API 与脚本

普通网关默认 `http://localhost:8080/`，GET `/health`、`/devices`。每设备 8001..8999 由 API 面板显式启动；GET `/device` 和 POST `/shell`、`/tap`、`/swipe`、`/text`、`/app/start`、`/app/stop`。成功为 `{ok:true,data:...}`，失败 `{ok:false,message:...}`。普通 swipe 使用 `DurationMs`，非正值归一为 300。

Automator 默认 `http://127.0.0.1:17171/`，路径允许 `/api/` 前缀。所有 27 条 POST 都包含 `serial`，响应 `{code:0,message:"ok",data:...}`。具体路由与字段以 `docs/reference/API.md` 第4节和源码 `AutomationService.Routes` 为准，swipe 使用 `steps`/`stepMs`。错误映射和本次差异见 `docs/ASSUMPTIONS.md`。

`.jxscript` 是包含 `manifest.json` 和 `script.json` 的 ZIP。`examples/basic-example.jxscript` 可直接导入，对应可编辑文件在 `examples/basic/`；支持 `launchapp/launch`、`wait/sleep`、`click`、`swipe`。在专用设备上确认坐标后运行；取消表示结束，不能续跑。默认拒绝未知动作。参数与 Probability 的证据边界见假设表。

新脚本可显式声明 `executionContract: {"version":1,"bindings":[{"stepIndex":0,"field":"value","parameterKey":"targetApp"}]}`，绑定步骤的完整 `value` 或等待动作的 `durationMs`。取值优先级为显式覆盖、参数Value、DefaultValue；显式null不回退，类型、范围或绑定无效时在运行前拒绝。新契约不接受Probability；未声明契约的旧包保持原字面量执行。格式、兼容与实际验证范围见[参数绑定](docs/codex/apple-experience/A14_SCRIPT_PARAMETER_BINDINGS.md)。

`examples/parameterized/`是参数化包的两个JSON源文件，不是可直接导入的普通JSON。可运行 `Compress-Archive -Path ./examples/parameterized/*.json -DestinationPath ./artifacts/parameterized-example.zip` 生成ZIP，在自动化页导入，参数JSON字符串应保留引号，例如 `"com.android.settings"`。`--verify-script-parameters --demo --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY`仅运行无窗VM业务专项，不读用户配置；它不代表GUI点击、真实模拟器、完整作业/MCP或抓包验收。

## 数据与设备边界

默认数据位于 `%LOCALAPPDATA%/MatrixDevice.Reconstructed/`；桌面 demo 与真实模式使用分离子目录。配置主文件/备份、SQLite 和日志均为本工程所有，不读取 CODEBRAIN 或旧产品数据。SQLite 使用新的 schema version=1；没有原产品密码或迁移兼容承诺。

主控面板使用实际截图，ADB群控将完成的手势转换为tap/swipe。原有投屏入口通过用户提供的独立 `scrcpy.exe`，只附着本工程启动且核验PID/开始时间的窗口；它与新监控视频链路分开。原私有视频流只还原已定义包编码，未实现未知握手。不得以截图刷新验收原产品实时视频、音频或20台解码性能。

## 实时监控

接入并核验独立雷电实例后，点击左侧“监控”：默认只显示已连接设备，每个可见卡片自动播放视频，不需要选中或放大。画面最大边320像素，源端上限10帧/秒、1 Mbit/s，UI最多8次/秒呈现并丢弃未显示的旧帧。“资源列表”暂停视频、保留资源采样；暂停、切页、滚出视口及退出都会释放对应视频会话，不关闭模拟器。静止页面可能不产生新帧，时间戳与等待状态独立显示；视频到帧率不是应用或游戏FPS。

在“设置”指定`监控视频组件`和`监控 FFmpeg`后重启。视频组件固定为[官方scrcpy-server v3.3.4](https://github.com/Genymobile/scrcpy/releases/tag/v3.3.4)，SHA256为`8588238c9a5a00aa542906b6ec7e6d5541d9ffb9b5d0f6e1bc0e365e2303079e`；从[官方发布附件](https://github.com/Genymobile/scrcpy/releases/download/v3.3.4/scrcpy-server-v3.3.4)单独取得，不能用任意版本替换。FFmpeg路径留空时只查找PATH中的已安装`ffmpeg.exe`。两项二进制不随源码包分发。

新链路只使用已有、经身份核验的ADB服务，经独立socket推送并启动唯一临时官方server，读取H.264后交自有FFmpeg解码；不运行外部ADB客户端、不建立共享forward、不启用音频、控制、剪贴板或亮屏。仅清理本次会话的临时文件/经nonce、PID出生证明核验的自身进程。若清理不能确认，同一后端拒绝该设备再次启动视频，需先人工核查，不能靠重开程序绕过；该阻断不是跨进程持久锁。

主机CPU/物理内存与每实例CPU/工作集约2秒采样一次，缓存当前会话最多15分钟趋势。实例CPU按全机逻辑核归一，工作集为Player+VBox进程求和、包含共享页，不是安卓可用内存；GPU、网络、磁盘和应用FPS尚未接入。首个CPU样本/权限不足/进程更换显示未知，不伪造0。多处理器组机器的整机采样保守报告未知。

单击卡片仅改变监看详情，不改变主控、操作目标或节点。双击卡片或点击右上角窗口图标，核验现有进程/窗口后请求显示对应雷电窗口，可在那里用鼠标键盘操作；不启动关闭实例，不操作原实例0。节点调试首次进入或切设备时自动读取定位用的截图和节点，视频帧不会覆盖节点坐标。专项验证与限制见[监控报告](docs/testing/realtime-monitoring/REPORT.md)。

监控页头自然换行，卡片和资源图按真实内容视口宽度适配，打开窗口/重试动作采用44逻辑命中区；长主机信息、摘要和选卡详情可在有界区域滚动读取。视频保留180高和等比显示，避免自动滚动条与卡高相互影响；图表重排只使用已有历史，不追加采样或重建视频。六尺寸Demo专项已获独立离线布局验收，原测试首败及边界见[监控正文适配](docs/codex/apple-experience/A05_MONITOR_RESPONSIVE.md)，不替代当前冻结版本真实视频、物理输入或DPI验收。

2026-10-15限定验收：专用实例1/2的R5实机11项通过，包括双路连续192.401秒、全部目录45.503秒、隐藏窗口后收新帧、暂停恢复及离页清理；根离线137/137。此结论不代表4/23路或长时稳定性已通过。可用`./tools/Start-Desktop.ps1 -DataDirectory '已明确配置授权的绝对目录' -Monitor`直接打开监控页；不自动启动模拟器或授权其它实例。

手机 agent 可通过 `AgentSerial` + `AgentBaseUrl` 显式绑定一台设备，用于已定义节点文本/服务请求。不配置时不会部署未知 JAR，也不会把一个 URL 用于所有 serial。私有 ROM SIM、代理部署规则和剪贴板协议会明确返回不支持。

中文全局输入：仅对同时列入授权串号及唯一`LdSerialBindings`的运行实例使用雷电call.input，并先验证现有ADB代次。普通ASCII走ADB。LD绑定的所有ADB操作及SDK应用/输入入口都核验冻结的实例身份，变化后需在设备管理重新接入；ADB服务进程变化必须重启本程序，不自动接受新服务。节点指定set-text仍需兼容agent，不冒充节点输入已实现。动态视频页uiautomator可能不生成快照，界面明确报错并清空旧节点。

## 验证与交付

`tools/Build.ps1 -Verify` 构建六个项目并运行全部验证。`--smoke --screenshot ABSOLUTE_PATH` 自动使用 demo 渲染各页面后退出；真实设备验收步骤见 `docs/VERIFICATION.md`。文档逐项覆盖见 `docs/COVERAGE.md`，缺失/矛盾处理见 `docs/ASSUMPTIONS.md`。

截屏输入验收保留原定位与命中断言，新增有限OS/WPF坐标和输入源诊断。本轮独立正式150项通过，但完整GUI在输入前置失败；三个快照已确认活动鼠标源缺失，成因未定，不是坐标算法或实机修复通过。原始失败、采样范围及未执行项见[输入诊断报告](docs/codex/apple-experience/AP033_INPUT_DIAGNOSTICS.md)。

本轮计划与实机日志见[测试计划](plan.md)、[阶段记录](docs/testing/TEST_LOG.md)、[缺陷账本](docs/testing/DEFECTS.md)。原始截图/节点/响应位于私有artifacts/acceptance，不进入公开源码包；离线、实机、拒绝路径和未实现能力分别标记。

历史单实例验收结论见[独立实例报告](docs/testing/REPORT.md)，本轮多实例结论以[专项报告](docs/testing/multi-instance/REPORT.md)为准。本机可双击`tools/Start-VerifiedDesktop.cmd`或在PowerShell 7运行对应`.ps1`，使用保留的测试实例1及本机独立配置；不调用原实例0。源码包不分发该授权配置，换电脑须显式重新确认。

多实例任务另见[多实例计划](docs/testing/multi-instance/PLAN.md)、[测试记录](docs/testing/multi-instance/TEST_LOG.md)和[报告](docs/testing/multi-instance/REPORT.md)。显式测试入口为Tests.dll `--multi-acceptance spec.json prepare|workload|diagnose 绝对证据目录`，只允许非零独立目标、唯一串号、实际进程绑定及资源预留；diagnose仅收集授权guest的活动/窗口/进程及节点截图，不操作原0。型号测试只走冷态官方CLI，不开放通用身份修改HTTP。HTTP Toolkit安全/许可未确认时直连轮不计抓包。

本机多实例桌面入口为`tools/Start-MultiVerifiedDesktop.cmd`，只读取私有`data/multi-verified`内已明确授权的非零独立实例配置；它不启动/关闭模拟器，程序会在启动时重新核验实际绑定。默认API/输入同步关闭。359MB测试APK使用300秒命令超时；专项驱动另有45秒UI预算，不能把这个预算误认为桌面所有控件已获得同样的期限。私有配置不进源码ZIP。

依赖来源：[AdvancedSharpAdbClient](https://www.nuget.org/packages/AdvancedSharpAdbClient/3.6.16)、[Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.12)、[SkiaSharp](https://www.nuget.org/packages/SkiaSharp/4.153.1)。不复制厂商二进制。
