# A08b 自动化工作区控件与响应式验收

## 背景与目标

A14a2 已实际交付 77366305c34c8c7246c2d10f637ef9498b73b366，独立及主仓209/VM9通过。本阶段不重做其核包或协议测试；补验自动化页实际 WPF 控件与六尺寸正文。当前静态风险为无界脚本说明/目标文字挤占正文，参数/记录与日志固定分高，结果展开后任务行空间不足，以及长字段全文读取和编辑状态。

## 范围与约束

- 只优化自动化页的布局与阅读/编辑交互。复用原参数、任务、日志集合、选中任务、脚本导入及运行/取消命令，不改执行引擎、HTTP、真实设备授权或共享主题。
- 推荐最小方案：沿现有内部视图扩为“参数 / 运行记录 / 日志 / 结果”，迁移原日志和结果，不建立第二套控件或重复操作入口。各视图占满有限正文，长元数据有界但全文可读。
- 保留全部参数/任务列和绑定，优先让参数值和任务状态易于扫描，长字段经真实滚动或全文提示阅读。主要控件具备44逻辑命中区，不整体缩放页面。
- 若基线实证活动日志发布会清空列表并丢失阅读状态，只修同一上下文的增量发布；切换任务、运行时和已淘汰历史仍正确更新，不保留错误来源。
- 只用自有 Demo、实际已显示的测试窗口和程序内 WPF 控件操作；不操作真实模拟器、共享ADB、账号、代理、CA或VPN，不通过OS输入/原生UIA绕过停止的AP045。
- 导入业务以实际自有ZIP和原ImportScriptAsync作前置。原生OpenFileDialog不在本阶段自动打开或冒称通过；全GUI上传链、实机与DPI仍必需，保留在原任务中。

## 方案取舍

原理上，正文必须得到有限、足够的可见空间；仅让工具条换行不能解决无界摘要和多个最小高度争用。推荐复用一个四视图容器，复杂度与维护成本最低，也不引入迁移。更复杂的宽屏三栏/窄屏分段可保留同时查看，但增加断点、焦点与滚动恢复成本，本轮不采用。拒绝整页无限ScrollViewer、删列或重建两套参数/任务表，这些方案会破坏虚拟化、信息保真或编辑状态。

## 验收标准

1. 实际外窗为390×640、390×844、768×1024、1000×700、1440×900、1920×1080，误差小于0.6 DIP；内容在长脚本说明/目标、错误提示下仍可达。导入/运行/停止所选/取消此任务、四视图选择及参数编辑/任务行的交互高度至少44 DIP，实际裁剪/命中一致。
2. 参数和任务视口至少96 DIP高，日志至少48 DIP高，独立结果视口至少100×48 DIP；结果全文可读，不挤掉任务行。全部列、首/中/末行实际滚动可读，不以“有滚动条”代替验证。布局压力可用明确标注的100历史/500日志/16KiB结果样本，只证明呈现，不冒充真实运行。
3. 原DataGrid实际BeginEdit→TextBox更新→CommitEdit→原运行按钮控件路径，编辑值进入原参数模型、metadata和实际Demo执行；非法JSON拒绝且零新增任务/metadata/动作。
4. 实际任务行选择双向绑定同一JobId，取消按钮按该任务取消；变更设备勾选不误停另一任务。使用真实Scheduler/Demo与受控TCS，不靠固定sleep造序，所有任务及窗口清理排空。
5. 同窗在六尺寸与四视图往返保持页面、表、集合、参数编辑值、选中JobId、目标及日志来源；纯布局无新设备调用或重复脚本。若活动日志案例实际失败，先保留红，再最小修复并复验。
6. 保存实际WPF渲染PNG，尺寸及非空像素成立，独立验证者查看代表图并实际运行；原209及VM9保持。程序内控件/渲染不外推到物理键鼠、原生选择器或完整GUI验收。

## 执行任务

| 顺序 | 优先级 | 内容 | 验证 | 状态 |
|---|---|---|---|---|
| 1 | P0 | 冻结本页预期及独立设计审查 | 目标、边界、六尺寸与原对象/命令保真 | 独立静审有限认可 |
| 2 | P0 | 独立入口和基线控件测试 | 原页长内容/滚读/编辑/取消，保留真实红与前置分类 | 原七例基线保留；正式十二例已执行 |
| 3 | P1 | 最小本页布局及已实证发布问题修复 | 同一组案例转绿，不弱化原断言 | 自有WPF/Demo有限域独立及主仓通过 |
| 4 | P0 | 独立及主仓复验、文档、源码清单和单功能推送 | 实际日志、渲染、精确范围审 | 主仓终态通过；核包与推送以实际Git为准 |

## 验证记录

### 原版基线

- 测试树`script-workspace-tests`从7736630建立，正式两源为App严格入口和新ResponsiveScriptsSmoke；新入口不进入旧AP042链。`baseline-r01`仅两处测试代码编译失败（Error私有setter、嵌套ToolTip名称遮蔽），页面NOT-RUN，不称产品红；原件保留。
- `baseline-r02`构建实际209/209、1条既有CS9124/0错；Build66.35秒、工作区6.72秒/exit1，session74158终态。七个已显示自有窗口均cleaned，后端单Dispose/Active0，无残窗或共享ADB动作。
- 真任务38→39条日志，同JobId，第二TCS门仍Active1：选中项index12→-1、Reset1/Add39，是PRODUCT红。滚动offset8、原首可见行及y1保持，不能扩称滚动锚点也丢失。
- 六尺寸布局均有独立几何红；390×640在长说明、长目标及真实100高错误栏下，参数/记录/日志/结果视口被挤出。记录全部列读取同时遇到测试代码把既有State MultiBinding强转Binding的FIXTURE错误，不能算记录字段产品失败。停止设备按钮在无真实任务时正确禁用，原InputHitTest要求其成为enabledHit亦属HARNESS/ORACLE误判，不掩盖其余44和裁剪红。
- 原报告`artifacts/script-workspace/baseline-r02/profile/script-workspace.json` SHA256 `EB80A268C9766632683793F831CB748D17D3525305C3B9CE6DCED2D0312C4041`，保留原源和原流。R2.2仅修上述列路径/禁用输入语义；旧几何、96/48、选中项及锚点标准不变，稳定副本由独立作者提供。

### 实现树候选

- 根实现独立树`script-workspace-layout`只改ScriptsView和WorkspaceViewModel，导入冻结的App/新夹具两源；其最终四源已精确集成主仓。四视图仍使用原集合、绑定及命令，日志同源按原始entry引用对齐尾窗，只追加/淘汰，不按文本去重；切任务仍换源。
- `candidate-r01`Build0/13.66秒、mode1/14.112秒：真实日志例PASS；新结果TextBox默认TwoWay绑定只读Result导致异常，源码堆栈确认是本候选PRODUCT错误，虽原夹具阶段泛类为FIXTURE，不能照抄该类别或将未加载完成的压力算通过。已显式OneWay，原件不覆盖。
- `candidate-r02`同原七门，真实日志及五尺寸PASS，390×640参数48/记录4/日志38/结果68高，纯PRODUCT纵向预算红。`candidate-r03`导入44图标（原tooltip/AutomationName/命令不变），短页摘要/目标各单行20真实可滚动；同七门仅390×640记录54<96，参数98/日志88/结果118，其余五尺寸与日志例PASS。
- 三轮候选所有required子进程终态、163输入前后稳定、原始双流排空、七例均完成清理；结果仅用于调试，不冒充全209/VM9、最终新增十二例或实机。原件位于实现树ignored`artifacts/script-workspace-layout/candidate-r01`至`candidate-r03`。最终原CancelSelectedScriptJob按钮移到运行记录页签头，先清空原Header再重新挂载caption，避免WPF逻辑父冲突；caption有独立44高命中区，不把取消按钮当页签选择。结果绑定显式OneWay，全部5/8列与原命令保持。

### 独立正式复验与失败分流

- `script-workspace-verifier/artifacts/script-workspace-verifier/independent-r01`首次Build29892 exit0/83.151秒，原209/209精确名序/51.74秒、1条既有测试CS9124/0错；VM30228 exit0/580ms、原4新5过。首个no-Demo拒绝17068实际exit1、未建profile，原中文stderr为CP936；runner误按UTF8解码而outer1，归HARNESS-ENCODING-FAIL，不是产品红。原execution `9CEA99A0BF1D6BE639884AB991855F6B008D6B6388E219EC4C2B221241FEB97F`及480字节stderr保留。
- 窄continuation-r01不重跑Build/VM/首拒绝，仅原已编译46文件冻结后跑其余三严格拒绝和完整WPF十二例。三拒绝正确；首次WPF60396 exit1/25.109秒为9PASS、3FIXTURE-INPUT-NOT-REACHED：新测试包错用绑定字段`Value`，原契约只接受`value`，三编辑/取消用例未到实际动作。报告 `9B4A6795F4A90DA501C0B20266A71B096BC96880909A6C0DF9B689E46B2FCEA7`完整保留，不冒称产品失败或十二全绿。
- 只将该夹具数据行改为`value`，其余三源/断言不变。必要重建47212 exit0/13.553秒，不重复运行209；原Tests/Core/Features/Host八DLL/PDB和44非Desktop文件摘要未变，故仅复用首次209证据，不称本轮又跑209。最终VM37984 exit0/506ms、9PASS，WPF78044 exit0/24.990秒、12PASS/12cleaned；所有backendDisposed/Active0、Windows0/MainWindowNull、errors0、双流排空、无timeout/kill/cleanup错误。修正轮execution `38370BBA356E9E48A53AD021E768A3C47757DAE7122E9B3FBD1FB65985B41ED7`，report `1C60FC7B6E5A054E9595848AC1CFF96E5A805B37749E592C8C4E66D000F75441`，终态独审 `33811821F14B260000C79F812228596158DF010F3A92E68E0E60392BC3C360A2`。
- 独立实际核31张PNG签名、尺寸/非空，目视390记录/结果、768参数及1920日志。390×640参数/记录视口各98高、日志88、结果118，记录caption为76×44且中心命中TextBlock，不误取消。真实日志选中12→12、anchor y1→1/offset2→2、Reset0；实际575步的500日志尾窗首77→78，选中B不混入A新日志。

### 主仓最终代表

- 四源分别为ScriptsView `AA2E5383AB822E605F9D4529EBBF182CC576C4613A286C2C78E8657DB5CC0ED6`、WorkspaceViewModel `74A234ADC42F53C5937AE4C9D3AA3F0CC2EB7540294267684B71D7846DF9D19A`、新ResponsiveScriptsSmoke `F2DBACA68F5FB17EAFECE03097CEC4466119D590B5334966221670A724EE941A`、App `A446E8183EE5695669A14A2AE71FEF4651D46545FB751E504C5784FE0B36DDBE`。实际PDB核四项同值，159个其余原始输入/26旧Smoke原样，原App旧链保留。
- 唯一主仓session14716 outer0，Build47936 exit0/73.330秒，原209/209/51.36秒、1条既有测试警告/0错；VM65336 exit0/505ms、原4新5全过。四严格前运行拒绝实际exit1、原中文按CP936核验、无目录/截图副作用；WPF13736 exit0/25.241秒、十二例全部PASS及cleaned。十二例含六尺寸、实际编辑/Run、非法JSON零接纳、原表选择与取消A/B继续、同窗resize保真、真实500尾窗隔离，不只是属性存在检查。
- 主仓原件在`artifacts/script-workspace-main/main-r01`：execution `254611E94BD1AC6AF6ED61BA4E867896377923DF83C2113EA96811A57D97DD46`，workspace report `8C9FBEC2F9B4E2299D8A4FC654AA49FDCF72C16C25D2F914874654DC3D652DB3`，after `3D85CFEF0585A9EE357ABE679CBBA9244D98461A95FF2E04C811903A9FDD3166`。七个required子进程均终态、十四原始流排空、无timeout/kill/清理错、owned0；163输入前后不变，用户配置及两PNG未动。主代理目视主仓390记录和768参数实际渲染；独审与核包结论以最终回执为准。

正式入口为`--demo --verify-script-workspace --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY`。在创建配置/runtime/窗口前拒绝无Demo、混测试模式、尺寸/截图参数与已有目录；原测试模式和AP042链未被替换。此入口是程序内自有窗口/控件与Demo的有限验收，不是外部UIA、原生文件选择器或物理键鼠。源码包与提交只包含本功能四源、正式文档及生成清单，不包含本机配置、当前原始测试证据或二进制；仓库原有历史verification材料保持原样，不宣称清除了历史证据。

原AP042 PRODUCT-FAIL、AP045停止、真实双实例同步、GUI安装卸载、最终30分钟及完整RPA/MCP/抓包继续开放。本轮自有WPF控件与渲染不证明物理GUI、原生picker、DPI或真实手机执行。
