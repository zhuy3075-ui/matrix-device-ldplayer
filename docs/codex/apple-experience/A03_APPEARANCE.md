# A03：共享外观与中文命名

日期：2026-10-15。结论：通过共享外观/命名的独立离线阶段验收。没有将本阶段扩大为手机、真实DPI或全功能实机通过；后续A04至A11仍继续。

## 页面与组件

| 页面/组件 | 具体变化 |
|---|---|
| 全局导航、窗口标题、Tab Header | LocalizationService.PageDisplayName单一映射，保留全部12个Tag及页序；设备、监控、界面检查、截屏、终端、应用、文件、文本、自动化、接口、工具、设置 |
| 主窗口与公共控件 | 显式应用Window主题，修复派生MainWindow没有命中隐式样式的问题；Segoe UI/中文回退；Button/Check/Field及真实路径输入中文可访问名称 |
| 设备、应用、监控 | 原正文标题与统一显示名一致，原业务/选择不变；行内图标和暂停/恢复动作名称随状态更新 |
| 界面检查、截屏、自动化 | 原画面/树/日志保持；背景、边线、选中颜色收敛到共享主题；截屏“1:1”动作改“原始大小” |
| 接口 | 仅将启用/停止所选设备更名为“启用设备接口”“停止设备接口”，避免误解为启停模拟器；请求方法/路径/结果中文可访问名称 |
| 设置与全局工具 | 真实PathField内部TextBox命名；五个浏览仍各自绑定；全局设置/详情使用稳定Name和AutomationId，验收不再依赖旧中文tooltip字面量 |
| 主题与动效 | 中性浅底/白表面/低饱和蓝，青绿/琥珀/珊瑚状态；轻微选中渐变、浮层阴影、6至8圆角；120ms局部反馈，尊重系统动画偏好变化 |
| 其他原页面 | 保持布局、控件、Tag和业务；继承公共主题/字体与中文名称，不以此声称各自业务已经实测 |

仅19个Desktop源/验收文件，未修改Core/Host/Features、NativeView、窗口最小尺寸、栏宽、按钮命中尺寸或用户配置。没有引入外部UI框架/字体/图标包，不动画修改共享Brush。

具体文件：LocalizationService.cs、MainWindow.xaml.cs、MotionPreferences.cs、Themes/ModernTheme.xaml；Views下WorkspaceShell、WallView、ApplicationsView、MonitorView、DebugView、ScreenshotView、ScriptsView、SettingsView、ApiView、UnifiedDeviceTile；验收文件AppleAppearanceSmoke、UiControlAudit、ApplicationManagementSmoke、ShellSimplificationSmoke、UnifiedDeviceActionsSmoke。

## 失败与调整

1. 12名称红测实际失败，旧首页名称未统一；改为单一映射。
2. 真实样式断言发现主窗口字体未生效，以及“1:1”未有中文动作名；修复实际应用点，不只改资源。
3. 新动效焦点绑定两次失败，第三次以Self绑定真实模板控件后clock运行；保留各失败日志，没有删断言。
4. root目视拒绝最初final-*逐页证据：设备tab0只剩表头，最终另拍图却有4行。根因是离线状态恢复后未等绑定/布局，捕到了非idle帧。新增有界idle/render等待、实际DataGridRow视口/名称/动作断言和每页captures记录；旧轮次保留为被拒绝，不再作为通过证据。
5. 独立审查要求直接读取模板PART_Popup.PopupAnimation，而非只查偏好对象；另补Toggle实例clock开关。均已补齐并独立复跑。

## 最终验证

| 层级 | 结果 |
|---|---|
| 实现侧最终正式基线 | 149/149，50.66秒；0错误0警告 |
| 独立verifier | 149/149，52.32秒；1000×700、1440×900、1920×1080完整串行smoke均exit0 |
| 独立每轮 | 12页、195项外观断言、25主控守卫、error空；12个idle捕获，Busy/Transferring=false，恢复设备状态与预期一致 |
| 独立画面复核 | 每尺寸设备tab0与最终desktop的PNG SHA相同；逐一目视6图，4行/连接2和动作正常 |
| 主仓最终整合 | 149/149，51.52秒；19源文件与verifier逐SHA相同；最终1000×700重跑exit0，195/25/12 idle、4行/连接2，tab0与最终PNG SHA相同 |

主仓最终DLL SHA256：91ABD9705C30B0E31531A0E58850690CB0D1C174C4E25BBC38CFEF3E3993D525。独立worktree DLL摘要另记，不冒充同一二进制。

实际控件检查覆盖普通/主要/选中/焦点、WPF Space按下/释放路由、仅自有夹具的指针悬停、忙/禁用、必要边界、白色反馈层合成，以及实际Popup和Button/Toggle模板开关。正文最小对比4.65844:1，必要边界3.42862:1，渐变取最差端点。禁用控件不按正常可交互文字计入对比结论。此结果不代表所有页面每个像素/控件都已完成可访问性验收。

原始证据在实现worktree的artifacts/apple-theme/capture-*、独立verifier的artifacts/apple-theme-verifier及主仓artifacts/apple-experience/A03，不进Git。系统偏好仅模拟应用内分支并finally恢复系统读取，没有修改或实际切换操作系统设置；没有使用真实设备或外部ADB。

## 剩余范围

A04四处最低尺寸契约/导航抽屉/公共表单和弹窗；A05至A09各页面固定栏与真实交互；A10触控/DPI/键盘细节；A11最终实机与30分钟稳定性仍未完成。逐页idle截图不是业务成功，Demo节点自动首载被明确禁用，界面检查正常有节点与空状态需A06分别验证。自有APK工具链缺口AP-008仍保留。

用户配置摘要仍E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0，未修改授权/共享服务。每功能独立push原私有功能分支，不合并或发布。
