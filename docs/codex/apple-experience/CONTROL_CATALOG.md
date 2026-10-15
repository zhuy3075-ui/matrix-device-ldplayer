# 逐控件验收目录

日期：2026-10-15。基线：8303cdc。此文件是测试索引，不是通过报告；全部本轮路径初始为NOT-RUN。

## 证据边界

原始扫描来自artifacts/list-console/verifier-final-1000/desktop.controls.json（忽略目录，仅本机）。1976条状态记录中，idle/可见/非空标签按page/type/label/binding去重为356项；移除重复全局导航、模板滚动条/列头后，得到下表103个页面控件键。扫描结果仅OBSERVED-NOT-EXERCISED，不代表已点击或真实效果成功。重复行模板、无标签输入、条件控件、菜单、浮层及主控另补充S项；扫描数量不是完整覆盖率分母。

## 通用检查

每个C/S项均验证N正常、E空或离线、B忙与重入、C取消或返回、F失败恢复、T目标/运行时代次变化；不适用时必须写出原因。关联功能用例见TEST_MATRIX.md，缺陷见DEFECTS.md。最终运行记录须关联caseId、buildSHA、目标证明和实际结果；只查CanExecute属于离线证据。

## 全局控件

| caseId | 控件 | 期望 |
|---|---|---|
| G-01 | 导航展开/收起与12页入口 | 保持Tag、当前输入和各类目标；键盘/触控可达；六尺寸无裁切 |
| G-02 | 设置入口 | 进入设置页，返回后不丢业务目标，不另设重复业务按钮 |
| G-03 | 设备详情入口 | 详情/改名职责；不再暗中改变群控选择和隐藏全局过滤 |
| G-04 | 顶部单设备目标选择（条件出现） | 明确只影响单设备操作，不误改主控或应用独立批次 |
| G-05 | 页脚、状态及错误反馈 | 数量语义一致；失败不伪装成功；不泄漏密钥和原始隐私内容 |
| G-06 | 表格滚动、列头排序、选择与菜单 | 横向滚动有意且主要动作可达；排序/筛选不偷换冻结目标 |

## 页面控件键

同一标签不同binding保留；同一行模板的多次实例以目标隔离用例覆盖。所有业务条目本轮初始NOT-RUN；A03共享名称/外观另有离线结果，不代替逐项业务执行。此处保留基线标签，最新12页名见UI_SPEC及A03报告，后续用binding/AutomationId定位而非旧标签。

| caseId | 页Tag | 类型 | 基线标签/提示 | binding | 原扫描caseId |
|---|---|---|---|---|---|
| C-001 | 脚本 | Button | 导入脚本包 |  | UI-04-脚本-25 |
| C-002 | 脚本 | Button | 停止所选 |  | UI-04-脚本-27 |
| C-003 | 脚本 | Button | 运行所选 |  | UI-04-脚本-26 |
| C-004 | 脚本 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-脚本-17 |
| C-005 | 节点 | Button | 导出 JSON |  | UI-04-节点-26 |
| C-006 | 节点 | Button | 刷新截图和节点 |  | UI-04-节点-25 |
| C-007 | 节点 | Button | 展开 |  | UI-04-节点-27 |
| C-008 | 节点 | Button | 折叠 |  | UI-04-节点-28 |
| C-009 | 节点 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-节点-17 |
| C-010 | 节点 | TextBox | NodeProperties | NodeProperties | UI-04-节点-33 |
| C-011 | 截图 / 取色 | Button | 1:1 |  | UI-04-截图 / 取色-36 |
| C-012 | 截图 / 取色 | Button | 保存 PNG |  | UI-04-截图 / 取色-26 |
| C-013 | 截图 / 取色 | Button | 保存裁剪 |  | UI-04-截图 / 取色-27 |
| C-014 | 截图 / 取色 | Button | 刷新截图 |  | UI-04-截图 / 取色-25 |
| C-015 | 截图 / 取色 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-截图 / 取色-17 |
| C-016 | 截图 / 取色 | Slider | 缩放 |  | UI-04-截图 / 取色-33 |
| C-017 | 扩展 | Button | 导出所选 |  | UI-04-扩展-46 |
| C-018 | 扩展 | Button | 导出无凭据 JSON |  | UI-04-扩展-28 |
| C-019 | 扩展 | Button | 核验 root |  | UI-04-扩展-32 |
| C-020 | 扩展 | Button | 解析配置 |  | UI-04-扩展-27 |
| C-021 | 扩展 | Button | 删除所选 |  | UI-04-扩展-47 |
| C-022 | 扩展 | Button | 验证并导入 |  | UI-04-扩展-45 |
| C-023 | 扩展 | Button | 应用并核验时区 |  | UI-04-扩展-36 |
| C-024 | 扩展 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-扩展-17 |
| C-025 | 设备墙 / 主控 | Button | 打开此设备的主控面板 |  | UI-04-设备墙 / 主控-59 |
| C-026 | 设备墙 / 主控 | Button | 调试 |  | UI-04-设备墙 / 主控-58 |
| C-027 | 设备墙 / 主控 | Button | 更多操作 |  | UI-04-设备墙 / 主控-28 |
| C-028 | 设备墙 / 主控 | Button | 连接 |  | UI-04-设备墙 / 主控-45 |
| C-029 | 设备墙 / 主控 | Button | 启动 |  | UI-04-设备墙 / 主控-49 |
| C-030 | 设备墙 / 主控 | Button | 启动所选，完成核验后自动连接并加入群控 |  | UI-04-设备墙 / 主控-25 |
| C-031 | 设备墙 / 主控 | Button | 新建空白实例 |  | UI-04-设备墙 / 主控-27 |
| C-032 | 设备墙 / 主控 | Button | 一键启动全部，完成核验后自动连接并加入群控 |  | UI-04-设备墙 / 主控-26 |
| C-033 | 设备墙 / 主控 | CheckBox | 勾选全部可操作设备；取消勾选清除所有选择，不删除设备或数据 |  | UI-04-设备墙 / 主控-36 |
| C-034 | 设备墙 / 主控 | CheckBox | IsSelected | IsSelected | UI-04-设备墙 / 主控-57 |
| C-035 | 设置 | Button | 保存设置 |  | UI-04-设置-25 |
| C-036 | 设置 | Button | 恢复遗留投屏窗口 |  | UI-04-设置-72 |
| C-037 | 设置 | Button | 浏览 |  | UI-04-设置-26 |
| C-038 | 设置 | CheckBox | 当前未接入硬件解码选择；值仅保留用于文档对照 | Settings.ForceHardwareDecode | UI-04-设置-91 |
| C-039 | 设置 | CheckBox | 启动时打开 API | Settings.AutoStartApi | UI-04-设置-65 |
| C-040 | 设置 | CheckBox | 启动时连接设备 | Settings.AutoConnect | UI-04-设置-64 |
| C-041 | 设置 | CheckBox | 显示 FPS | Settings.ShowFps | UI-04-设置-92 |
| C-042 | 设置 | TextBox | 例如：{"0":"实际发现的设备序列号"}；仅保存显式绑定配置，不表示已核验 | Settings.LdBindingsJson | UI-04-设置-49 |
| C-043 | 设置 | TextBox | Settings.AdbPath | Settings.AdbPath | UI-04-设置-27 |
| C-044 | 设置 | TextBox | Settings.AdbSerialsJson | Settings.AdbSerialsJson | UI-04-设置-30 |
| C-045 | 设置 | TextBox | Settings.AgentBaseUrl | Settings.AgentBaseUrl | UI-04-设置-52 |
| C-046 | 设置 | TextBox | Settings.AgentSerial | Settings.AgentSerial | UI-04-设置-55 |
| C-047 | 设置 | TextBox | Settings.AutomatorApiPort | Settings.AutomatorApiPort | UI-04-设置-61 |
| C-048 | 设置 | TextBox | Settings.BasicApiPort | Settings.BasicApiPort | UI-04-设置-58 |
| C-049 | 设置 | TextBox | Settings.CommandTimeoutSeconds | Settings.CommandTimeoutSeconds | UI-04-设置-69 |
| C-050 | 设置 | TextBox | Settings.FileRoot | Settings.FileRoot | UI-04-设置-66 |
| C-051 | 设置 | TextBox | Settings.LdConsolePath | Settings.LdConsolePath | UI-04-设置-46 |
| C-052 | 设置 | TextBox | Settings.MainBitRate | Settings.MainBitRate | UI-04-设置-85 |
| C-053 | 设置 | TextBox | Settings.MainMaxFps | Settings.MainMaxFps | UI-04-设置-79 |
| C-054 | 设置 | TextBox | Settings.MainMaxSize | Settings.MainMaxSize | UI-04-设置-73 |
| C-055 | 设置 | TextBox | Settings.MiniBitRate | Settings.MiniBitRate | UI-04-设置-88 |
| C-056 | 设置 | TextBox | Settings.MiniMaxFps | Settings.MiniMaxFps | UI-04-设置-82 |
| C-057 | 设置 | TextBox | Settings.MiniMaxSize | Settings.MiniMaxSize | UI-04-设置-76 |
| C-058 | 设置 | TextBox | Settings.MonitorFfmpegPath | Settings.MonitorFfmpegPath | UI-04-设置-42 |
| C-059 | 设置 | TextBox | Settings.MonitorServerPath | Settings.MonitorServerPath | UI-04-设置-38 |
| C-060 | 设置 | TextBox | Settings.ScrcpyPath | Settings.ScrcpyPath | UI-04-设置-34 |
| C-061 | 设置 | TextBox | Settings.WindowHeight | Settings.WindowHeight | UI-04-设置-96 |
| C-062 | 设置 | TextBox | Settings.WindowWidth | Settings.WindowWidth | UI-04-设置-93 |
| C-063 | 实时监控 | Button | 打开模拟器窗口 |  | UI-04-实时监控-35 |
| C-064 | 实时监控 | Button | 重试视频 |  | UI-04-实时监控-34 |
| C-065 | 实时监控 | ToggleButton | 画面墙 |  | UI-04-实时监控-26 |
| C-066 | 实时监控 | ToggleButton | 暂停监控 |  | UI-04-实时监控-25 |
| C-067 | 实时监控 | ToggleButton | 资源列表 |  | UI-04-实时监控-27 |
| C-068 | 文本 | Button | 发送文本 |  | UI-04-文本-25 |
| C-069 | 文本 | Button | 加载文本文件 |  | UI-04-文本-27 |
| C-070 | 文本 | Button | 粘贴 |  | UI-04-文本-26 |
| C-071 | 文本 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-文本-17 |
| C-072 | 文本 | TextBox | TextInput | TextInput | UI-04-文本-28 |
| C-073 | 文件 | Button | 查看目录 |  | UI-04-文件-28 |
| C-074 | 文件 | Button | 开始 |  | UI-04-文件-32 |
| C-075 | 文件 | Button | 添加上传 |  | UI-04-文件-30 |
| C-076 | 文件 | Button | 添加下载 |  | UI-04-文件-31 |
| C-077 | 文件 | Button | 停止 |  | UI-04-文件-33 |
| C-078 | 文件 | CheckBox | 上传 APK 后安装 |  | UI-04-文件-29 |
| C-079 | 文件 | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-文件-17 |
| C-080 | 文件 | ProgressBar | OverallProgress | OverallProgress | UI-04-文件-34 |
| C-081 | 文件 | TextBox | RemotePath | RemotePath | UI-04-文件-25 |
| C-082 | 应用 | Button | 安装 APK |  | UI-04-应用-39 |
| C-083 | 应用 | Button | 读取应用 |  | UI-04-应用-38 |
| C-084 | 应用 | Button | 启动、停止或清除所选应用数据 |  | UI-04-应用-41 |
| C-085 | 应用 | Button | 清空 |  | UI-04-应用-26 |
| C-086 | 应用 | Button | 全选可用 |  | UI-04-应用-25 |
| C-087 | 应用 | Button | 停止后续 |  | UI-04-应用-43 |
| C-088 | 应用 | Button | 卸载所选 |  | UI-04-应用-40 |
| C-089 | 应用 | CheckBox | 全选或清除可见应用 |  | UI-04-应用-63 |
| C-090 | 应用 | CheckBox | 系统应用 |  | UI-04-应用-42 |
| C-091 | 应用 | CheckBox | IsSelected | IsSelected | UI-04-应用-34 |
| C-092 | API | Button | 发送请求 |  | UI-04-API-42 |
| C-093 | API | Button | 启动两个网关 |  | UI-04-API-25 |
| C-094 | API | Button | 启用所选设备 |  | UI-04-API-27 |
| C-095 | API | Button | 停止所选设备 |  | UI-04-API-28 |
| C-096 | API | Button | 停止网关 |  | UI-04-API-26 |
| C-097 | API | CheckBox | IsSelected | IsSelected | UI-04-API-58 |
| C-098 | API | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-API-17 |
| C-099 | API | DataGrid | CurrentDevice | CurrentDevice | UI-04-API-49 |
| C-100 | Shell | Button | 执行 |  | UI-04-Shell-28 |
| C-101 | Shell | ComboBox | 节点、截图、命令等单设备操作目标 | CurrentDevice | UI-04-Shell-17 |
| C-102 | Shell | TextBox | Output | Output | UI-04-Shell-30 |
| C-103 | Shell | TextBox | ShellInput | ShellInput | UI-04-Shell-25 |

## 源码补充与条件控件

| caseId | 区域/控件 | 必测路径 |
|---|---|---|
| S-01 | 设备行启动/连接/调试/主控、复选框、表头三态全选 | 冷/运行未接/就绪/离线/受保护0；隐藏选择和空可见列表；唯一生命周期快照 |
| S-02 | 更多菜单：资源配置/关闭/重启；新建空白实例 | 单选/多选/非雷电，取消确认、代次变化、实际状态；不复制账号或删除实例 |
| S-03 | 主控面板：画面、点击/滑动、同步点击/按键、音量/主页/返回 | MainA/CurrentB、切主控/离线/关闭面板、拖动取消、实际目标回读 |
| S-04 | 主控外部投屏/嵌入/恢复窗口 | A/B隔离、身份变化、等待期间切目标、进程退出/复用、所有权清理；不自动接受外部ADB风险 |
| S-05 | 监控全局暂停、画面/资源模式、范围选择、卡片选择/双击/窗口/重试 | 实际动态帧、资源增量、滚出/切页/关闭清理、双实例差异画面；不虚构每路暂停或路数参数 |
| S-06 | 界面检查的树项、画布节点和属性动作 | 树与框双向定位、搜索移除后仍可浏览、目标切换拒绝旧节点 |
| S-07 | 截屏画布的取色/裁剪/缩放/保存对话框 | 像素/坐标一致、越界/取消/目标变化、导出文件可读 |
| S-08 | 应用目标/应用行复选框、计划/确认、进度/取消与结果 | 当前批次UI；两个目标自有唯一包安装/启停/卸载；部分失败/取消/重试 |
| S-09 | 文件本机选择/设备目录文本、上传/下载队列和上传后安装 | 仅本轮唯一路径；上传/下载哈希、取消和坏路径；当前没有设备目录新建/改名/删除入口，不虚构待实现需求 |
| S-10 | 终端历史快捷键、文本编辑区及焦点输入 | 实际回显/中文回读、离线/错误、系统文本复制语义；没有终端专用取消/复制/清空按钮 |
| S-11 | 脚本参数/包导入/执行/取消、步骤日志 | 真实步骤数据回传；参数坏值、取消2秒不继续、旧目标拒绝；没有步骤编辑/保存入口 |
| S-12 | 接口未命名方法/路径/请求体/端口字段、设备地址双击复制 | 普通/自动化拥有关系、请求副作用、停止端口不是停止实例；当前没有归档/密钥显示控件 |
| S-13 | 设置与工具所有未标记输入/组合框、验证提示、路径选择 | 标签与AutomationName补齐；保存/重载/坏值、依赖缺失和能力状态不假通过 |
| S-14 | 所有模态确认、原生打开/保存及错误窗口 | Esc/返回/取消、Owner与屏幕尺寸约束、默认安全动作、控件命中区域 |
| S-15 | PasswordBox、API地址/颜色双击、虚拟行编辑及页外窗口 | 脱敏/复制语义、焦点/键盘、滚入后实际行操作；对每个已打开Popup/Window单独盘点，不以Shell子树替代 |
| S-16..20 | 设置的5个“浏览”实例 | 按ADB、scrcpy、监控server、FFmpeg、雷电命令行各自绑定路径分别验证选择/取消/坏路径，不能因同标签仅测一次 |

本目录按实现阶段继续核对源码补漏和新增控件；未完成逐项实测前，不使用百分比或“全部控件正常”的结论。

## 字段和模态细化

源码基线7beea83；以下绑定/语义ID稳定，行号随重设计变化，执行以实际源码和当前控件为准。每个斜杠分隔的ID分别建立.N/.E/.B/.C/.F/.T记录，不合并成绩。表单只读值、无专用取消入口的项说明N/A；当前均NOT-RUN，不因A02守卫通过继承成功。

| 语义ID | 源码/绑定或实际控件 | 特有验证 |
|---|---|---|
| SET-ADB.value / SET-ADB.browse | SettingsView / Settings.AdbPath | 精确回填/取消留旧值/坏路径；只编辑，不执行外部adb |
| SET-SCRCPY.value / SET-SCRCPY.browse | Settings.ScrcpyPath | 文件有效性、取消；不自动肯定外部ADB风险 |
| SET-SERVER.value / SET-SERVER.browse | Settings.MonitorServerPath | 正确文件筛选，版本/摘要坏值；选择不等于视频通过 |
| SET-FFMPEG.value / SET-FFMPEG.browse | Settings.MonitorFfmpegPath | 精确回填/取消、坏路径、空值PATH回退与缺依赖分开 |
| SET-LDCLI.value / SET-LDCLI.browse | Settings.LdConsolePath | 回填/取消；保存后不得混用新路径和旧运行时 |
| SET-SERIALS | Settings.AdbSerialsJson | 数组/空/坏JSON/null/重复/空白；仅隔离配置，不改真实授权 |
| SET-BINDINGS | Settings.LdBindingsJson | 非零唯一映射/非法index/空串号/重复；配置不是身份凭证 |
| SET-AGENT-URL / SET-AGENT-SERIAL | Settings.AgentBaseUrl / AgentSerial | 配对、空值、合法地址、串号授权；只用自有loopback夹具 |
| SET-BASIC-PORT / SET-AUTO-PORT | Settings.BasicApiPort / AutomatorApiPort | 整数1024..65535、冲突/越界；保存不创建监听，不漂移运行端口 |
| SET-AUTO-CONNECT / SET-AUTO-API | Settings.AutoConnect / AutoStartApi | 保存回读；不后台新授权/启动；只在隔离数据和端口测试 |
| SET-LANGUAGE | SettingsView只读简体中文 | 显示且不可编辑；非输入，其他不适用说明原因 |
| SET-FILE-ROOT | Settings.FileRoot | 隔离绝对目录/坏路径/不可写/空值；不写用户旧目录 |
| SET-TIMEOUT | Settings.CommandTimeoutSeconds | 整数1..600和坏值；编辑校验与实际超时分开 |
| SET-MAIN-SIZE / SET-MAIN-FPS / SET-MAIN-BITRATE | Settings.MainMaxSize / MainMaxFps / MainBitRate | 各自范围/类型/保存回读；外部投屏参数，不是监控尺寸或实际码率/FPS证明 |
| SET-MINI-SIZE / SET-MINI-FPS / SET-MINI-BITRATE / SET-HW-DECODE | Settings.MiniMaxSize / MiniMaxFps / MiniBitRate / ForceHardwareDecode | 当前禁用及说明、旧值保留；不能统计编码/硬解业务成功 |
| SET-SHOW-FPS | Settings.ShowFps | 保存回读；仅scrcpy打印偏好，不是应用/游戏FPS |
| SET-WIDTH / SET-HEIGHT | Settings.WindowWidth / WindowHeight | 分别坏值/非数/NaN、Apply→Save→新Store.Load；四处尺寸契约一致 |
| SET-SAVE | SaveSettingsCommand | 忙/写失败/运行时冻结；无保存确认时取消为N/A |
| SET-RECOVER-NATIVE | SettingsView恢复遗留投屏 | 自有窗口夹具、PID/birth/路径拒绝、恢复失败保留记录 |
| APP-TARGET-CHECK[serial] | ApplicationsView / ApplicationTargetRow.IsSelected | Available、独立选择、代次变化不沿用旧目标 |
| APP-ROW-CHECK[serial,package] | InstalledApplicationRow.IsSelected | 相同包不同设备独立冻结，虚拟行滚入不串包/串台 |
| APP-VISIBLE-ALL / APP-SYSTEM-FILTER | 可见应用表头 / 系统应用 | 过滤与选择关系、表头状态、执行前重查系统属性；忙时计划不改变 |
| APP-MENU-START / APP-MENU-STOP / APP-MENU-CLEAR | ApplicationsView三个MenuItem | 零选/离线/忙；启停无确认不虚构取消窗，清数据必须危险确认 |
| APP-INSTALL-PICKER | 原生多选APK窗口 | 取消不改任务/零写；选择期间目标变化拒绝，不偷偷换新目标 |
| APP-CONFIRM-DETAIL | 只读可滚动任务计划 | 总数、串号/包/文件及风险准确，不算可编辑输入 |
| APP-CONFIRM-ACCEPT / APP-CONFIRM-CANCEL | 确认 / IsCancel | Enter、按钮、Esc、标题栏X分别测试；确认后重验冻结选择/身份 |
| APP-STOP-AFTER | 停止后续 | 当前项完成后取消后项，不回滚或打断已提交项，进度/终态准确 |
| SCAN-START-IP / SCAN-END-IP | NetworkScannerView独立输入 | 非法/反向/超大范围拒绝；仅自有loopback，不枚举未知网段 |
| SCAN-PORT / SCAN-TIMEOUT | 端口 / 超时 | 分别整数/范围/有界等待；发现TCP不等于ADB已连 |
| SCAN-RUN / SCAN-STOP / SCAN-WINDOW-CLOSE | 运行 / 停止 / 独立Window关闭 | 不自动开扫；运行四字段禁用；关闭Hide+cancel，重开无重入，不停共享ADB |
| LD-MENU-CONFIG / LD-MENU-QUIT / LD-MENU-REBOOT | LdPlayerView更多菜单 | 唯一勾选、运行/完全退出、0保护、旧代次拒绝 |
| LD-MENU-RESTORE-OLD-WINDOW | 恢复旧版隐藏窗口 | 仅明确记录、PID/birth/路径证明，失败保留记录 |
| LD-CREATE-NAME / LD-CONFIG-NAME | UnifiedDevicesView名称框 | 前者可编辑，后者只读；坏名零add，两种表单分别测 |
| LD-EDIT-CPU / LD-EDIT-RAM | CPU / RAM组合框 | 配置初始空，必须显式选择，不猜旧RAM |
| LD-EDIT-WIDTH / LD-EDIT-HEIGHT / LD-EDIT-DPI | 三个独立输入 | 分别整数/边界/空值；实例DPI不是WindowsDPI；CLI返回不算效果 |
| LD-CREATE-SUBMIT / LD-CONFIG-SUBMIT | 两种表单提交 | 坏值留窗；add成功modify失败保留新实例且不重复创建/删除 |
| LD-EDIT-WINDOW-CLOSE / LD-EDIT-CONFIRM | 标题栏X / 表单后第二确认 | X零CLI；无Cancel按钮/IsCancel，不预记Esc通过；确认后校验同一generation |
| API-PROTOCOL / API-METHOD / API-ROUTE / API-PAYLOAD / API-RESPONSE | ApiView的5个未绑定局部控件 | 普通/自动化/设备拥有关系、GET/POST、仅相对路径、坏JSON、实际HTTP结果；Response只读 |
| TOOL-PROXY / TOOL-PROXY-RESULT | ExtensionsView PasswordBox / 只读输出 | 解析与无凭据导出，不部署代理；密码遮蔽、日志无凭据 |
| TOOL-TIMEZONE / TOOL-PACKAGE / TOOL-NOTES | 时区 / 应用包名 / 备注 | 各字段坏值、取消、目标冻结/仅自有本地资料；不混淆设备恢复 |
| TEXT-EDITOR / TEXT-PASTE / TEXT-FILE | TextInput / 粘贴 / 原生文本选择 | 独立输入与两个导入路径；剪贴板/文件坏值、取消、中文回读 |
| CAP-MODE / MON-SCOPE | 截屏模式 / 监控范围 | 未绑定Combo各自选择，状态变化不改主控/应用授权 |

此细表把相同标签和Shell外控件展开为真实行为索引，仍需在各实施阶段运行并关联当前构建结果；不声明所有行均已执行。
