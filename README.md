# 矩阵设备文档还原工程

依据 `docs/reference/` 的 11 份文档独立编写的 C# 工程。界面统一简体中文，增加雷电实例管理及官方命令行适配。覆盖设备目录、截图墙与群控、节点调试、Shell、应用与文件、脚本、两类 HTTP API、配置、SQLite 元数据、诊断。原始源码、私有手机 JAR、完整视频握手、商业登录与 ROM 功能不在文档中，不能逐字节恢复。

新版桌面采用侧栏导航、统一搜索和蓝白主题，设备支持卡片/屏幕/列表视图，全部12页保留。布局与验收记录见 `docs/UI_REDESIGN.md`。构建后可运行 `./tools/Start-Desktop.ps1 -Demo` 查看演示界面；真实模式不加Demo，雷电路径仍需显式配置。

设备管理使用一个侧栏入口，默认“全部设备”的统一列表，关闭、启动中、待接入和已连接实例同屏显示，也可切换卡片或屏幕视图。“高级管理”保留实例配置，不再显示独立的底部待接入卡片区；模拟器数量不等同于可控设备数量。官方能力、限制和删除建议见[统一管理调研](docs/testing/unified-devices/OFFICIAL-RESEARCH.md)，最新界面与后台启动验证见[一键启动报告](docs/testing/one-click-start/REPORT.md)。

完整文件清单见 `PROJECT_TREE.md`；全部源码文本见 `SOURCE_BUNDLE.md`；对应文件摘要见 `SOURCE_MANIFEST.json`。这三个文件由 `tools/New-SourcePackage.ps1` 生成，源码 ZIP 位于 `artifacts/`。

## 构建与运行

需要 Windows 10/11、[.NET SDK 10.0.402](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)。依赖固定于项目文件与 `packages.lock.json`，NuGet 源位于 `NuGet.Config`。本工作区已有通过官方 SHA-512 校验的便携 SDK，路径为上级 `.tools/dotnet/dotnet.exe`。

从工程目录运行：

```powershell
./tools/Build.ps1 -Verify
../.tools/dotnet/dotnet.exe ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --demo
```

在其他电脑上可把第二行的 SDK 路径换成 `dotnet`。真实模式去掉 `--demo`，配置雷电命令行路径并重启，在“设备管理 → 全部设备”点击行内“启动”，或勾选后点击“启动所选”；“一键启动全部”处理全部未连接的独立实例。一次点击会启动关闭实例、核验实际ADB连接与安卓启动/截图，成功才保存接入并加入群控，不再弹普通启动确认框、不强制跳页；阶段进度、失败与取消留在原页面。已经运行的实例只核验连接，不重复启动。原实例0受保护，不参加自动接入、批量启动或资源修改；本轮不替用户操作原0。关闭、重启、新建与资源配置仍保留必要确认。

“后台启动”默认开启，只隐藏本次用户启动的独立雷电窗口，不隐藏早已运行的窗口或原实例0。可在列表行内或勾选后用“显示窗口”恢复；操作失败、取消或正常退出矩阵时尝试恢复本程序持有的隐藏窗口，不关闭模拟器。恢复记录核验PID、进程出生时刻、安装目录和窗口所有者，异常退出后下次启动尝试恢复。雷电14.0.29.0公开CLI没有真正headless参数，启动初期仍可能短暂闪现；隐藏不等于降低CPU/GPU占用，也不承诺任意应用持续渲染。

连接变更前需停止本程序的API、脚本、文件传输及网络扫描，等待请求排空；雷电/ADB路径等手工配置保存后仍需重启，不能混用新路径与旧运行时。高级串号JSON和索引绑定保留给明确知道真实串号的用户，不按端口公式授信。本程序直接连接现有5037服务，不执行adb.exe、不创建/kill服务；服务进程变化或协议不兼容会拒绝继续。无需原产品DLL、账号或数据库，未授权时不查询guest、不伪造在线设备。

实例目录每5秒自动更新，已授权设备状态最多每15秒查询一次；自动截图默认开启，仅前台设备群控页每5秒轮转最多4台，单轮有20秒预算。旧刷新/扫描按钮已移除，网络连接与应急目录更新收在设置齿轮的右键菜单。“一键启动全部”逐台等待，冷启动前保守要求11 GiB可用内存且系统内存负载低于80%；不足、超时或取消即停止后续，不关闭已启动实例。仅本次用户点击的目标可经过真实身份检查后保存授权，后台自动发现不启动或授权。勾选中含未连接实例时，批量应用、传输、脚本与同步输入拒绝执行，避免落到旧的当前设备或悄悄只操作部分设备。

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

## API 与脚本

普通网关默认 `http://localhost:8080/`，GET `/health`、`/devices`。每设备 8001..8999 由 API 面板显式启动；GET `/device` 和 POST `/shell`、`/tap`、`/swipe`、`/text`、`/app/start`、`/app/stop`。成功为 `{ok:true,data:...}`，失败 `{ok:false,message:...}`。普通 swipe 使用 `DurationMs`，非正值归一为 300。

Automator 默认 `http://127.0.0.1:17171/`，路径允许 `/api/` 前缀。所有 27 条 POST 都包含 `serial`，响应 `{code:0,message:"ok",data:...}`。具体路由与字段以 `docs/reference/API.md` 第4节和源码 `AutomationService.Routes` 为准，swipe 使用 `steps`/`stepMs`。错误映射和本次差异见 `docs/ASSUMPTIONS.md`。

`.jxscript` 是包含 `manifest.json` 和 `script.json` 的 ZIP。`examples/basic-example.jxscript` 可直接导入，对应可编辑文件在 `examples/basic/`；支持 `launchapp/launch`、`wait/sleep`、`click`、`swipe`。在专用设备上确认坐标后运行；取消表示结束，不能续跑。默认拒绝未知动作。参数与 Probability 的证据边界见假设表。

## 数据与设备边界

默认数据位于 `%LOCALAPPDATA%/MatrixDevice.Reconstructed/`；桌面 demo 与真实模式使用分离子目录。配置主文件/备份、SQLite 和日志均为本工程所有，不读取 CODEBRAIN 或旧产品数据。SQLite 使用新的 schema version=1；没有原产品密码或迁移兼容承诺。

截图墙使用实际截图，ADB 群控将完成的手势转换为 tap/swipe。实时视频通过用户提供的独立 `scrcpy.exe`；原生宿主只附着本工程启动且核验 PID/开始时间的窗口。原私有视频流只还原已定义包编码，未实现未知握手。不得以截图刷新验收原产品实时视频、音频或 20 台解码性能。

手机 agent 可通过 `AgentSerial` + `AgentBaseUrl` 显式绑定一台设备，用于已定义节点文本/服务请求。不配置时不会部署未知 JAR，也不会把一个 URL 用于所有 serial。私有 ROM SIM、代理部署规则和剪贴板协议会明确返回不支持。

中文全局输入：仅对同时列入授权串号及唯一`LdSerialBindings`的运行实例使用雷电call.input，并先验证现有ADB代次。普通ASCII走ADB。LD绑定的所有ADB操作及SDK应用/输入入口都核验冻结的实例身份，变化后需在设备管理重新接入；ADB服务进程变化必须重启本程序，不自动接受新服务。节点指定set-text仍需兼容agent，不冒充节点输入已实现。动态视频页uiautomator可能不生成快照，界面明确报错并清空旧节点。

## 验证与交付

`tools/Build.ps1 -Verify` 构建六个项目并运行全部验证。`--smoke --screenshot ABSOLUTE_PATH` 自动使用 demo 渲染各页面后退出；真实设备验收步骤见 `docs/VERIFICATION.md`。文档逐项覆盖见 `docs/COVERAGE.md`，缺失/矛盾处理见 `docs/ASSUMPTIONS.md`。

本轮计划与实机日志见[测试计划](plan.md)、[阶段记录](docs/testing/TEST_LOG.md)、[缺陷账本](docs/testing/DEFECTS.md)。原始截图/节点/响应位于私有artifacts/acceptance，不进入公开源码包；离线、实机、拒绝路径和未实现能力分别标记。

历史单实例验收结论见[独立实例报告](docs/testing/REPORT.md)，本轮多实例结论以[专项报告](docs/testing/multi-instance/REPORT.md)为准。本机可双击`tools/Start-VerifiedDesktop.cmd`或在PowerShell 7运行对应`.ps1`，使用保留的测试实例1及本机独立配置；不调用原实例0。源码包不分发该授权配置，换电脑须显式重新确认。

多实例任务另见[多实例计划](docs/testing/multi-instance/PLAN.md)、[测试记录](docs/testing/multi-instance/TEST_LOG.md)和[报告](docs/testing/multi-instance/REPORT.md)。显式测试入口为Tests.dll `--multi-acceptance spec.json prepare|workload|diagnose 绝对证据目录`，只允许非零独立目标、唯一串号、实际进程绑定及资源预留；diagnose仅收集授权guest的活动/窗口/进程及节点截图，不操作原0。型号测试只走冷态官方CLI，不开放通用身份修改HTTP。HTTP Toolkit安全/许可未确认时直连轮不计抓包。

本机多实例桌面入口为`tools/Start-MultiVerifiedDesktop.cmd`，只读取私有`data/multi-verified`内已明确授权的非零独立实例配置；它不启动/关闭模拟器，程序会在启动时重新核验实际绑定。默认API/输入同步关闭。359MB测试APK使用300秒命令超时；专项驱动另有45秒UI预算，不能把这个预算误认为桌面所有控件已获得同样的期限。私有配置不进源码ZIP。

依赖来源：[AdvancedSharpAdbClient](https://www.nuget.org/packages/AdvancedSharpAdbClient/3.6.16)、[Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.12)、[SkiaSharp](https://www.nuget.org/packages/SkiaSharp/4.153.1)。不复制厂商二进制。
