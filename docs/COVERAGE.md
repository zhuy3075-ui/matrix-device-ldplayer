# 文档与源码覆盖

| 输入文档 | 对应实现 | 验证边界 |
|---|---|---|
| PRD.md F01/F05 | Core/Devices、Core/Input、Desktop/设备目录和墙 | 身份/代次/保序夹具；单实例真实发现/输入通过，双实例同步待验 |
| PRD.md F02/F10 | PngImage、截图页、NativeWindowHost、ScrcpyProtocol | PNG裁剪/取色/包格式；实时scrcpy和私有链路未动态验 |
| PRD.md F03/F04 | Features/Debug、NodeSearch、Desktop节点/Shell | 引用联动/选择不tap/搜索算法及静止页实机通过；动态视频节点有限制 |
| PRD.md F06 | Host/Api、Features/Automation、PhoneAgentClient | 两种envelope、27分支、错误/端口、手机请求DTO验证 |
| PRD.md F07 | Features/Scripts、Desktop脚本 | ZIP/坏包/未知动作/重复/取消/500日志；专用应用脚本未知 |
| PRD.md F08/F09/F11 | Applications、TransferQueue、ClipboardService、Desktop应用/文件/文本 | 实际APK启停/双向文件哈希/全局中文通过；私有剪贴板与指定节点文本待agent |
| PRD.md F12/F15 | Host/Persistence、Diagnostics、Desktop资源与设置 | 合并/备份/SQLite/flush、WPF布局；不是46字段完整schema |
| PRD.md F13/F14 | Features/Extensions | 代理解析与归档检查可离线；部署/恢复无动态证据 |
| spec.md | 项目依赖方向、队列、网关、包、持久化 | 已知行为与新增默认分开，见ASSUMPTIONS |
| uispec.md | Desktop/Views、WorkspaceViewModel、Presentation | 原职责页面、真实渲染；不能恢复原像素 |
| API.md | AutomationService.Routes、ApiServiceManager、PhoneAgentClient | 不混淆网关/手机字段；未知ROM返回501 |
| CLOUD.md | 输入、脚本、SettingsStore、CatalogRepository、RollingLog | 逻辑重建；商业准入/旧数据库/第三方资源不复制 |
| DEBUG.md | DebugWorkspace、截图/树/取色/Shell/应用/传输页 | 页面数据与联动；未知远程业务不虚构 |
| SHARED.md | ScrcpyProtocol、PngImage、RollingLog、Extensions | 已定义编码/队列/日志；缺握手与路由细节 |
| DEBUG_SHARED.md | 独立后端/协议与明确日志实现 | 只采用已证布局；不混用同名DLL版本 |
| ARCHITECTURE.md | Core→Features→Host→Desktop/CLI | 层次重建；不把历史31k方法库存当源码 |
| PSEUDOCODE.md | 节点遍历、包校验、取消、MOVE/代次、合并保存等 | 原占位全部转为明确假设或不支持 |
| LEGACY_DEVICE_TASKS.md | 分层验证计划、原生外部窗口边界 | 单台→双台→5/10/20为现场门槛，非测试成绩 |

请用完整目录清单定位实际文件。离线 demo 数量不能证明真实设备容量；编译通过不能证明未知手机协议兼容。

当前逐用例与接口分类以testing/TEST_RESULTS.md、testing/API_MATRIX.md及testing/REPORT.md为准；不能把路由接线或拒绝路径扩张成业务全部通过。
