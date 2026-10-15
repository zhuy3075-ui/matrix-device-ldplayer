# A06a 调试结果发布隔离

日期：2026-10-15。基线28cdd4ded947a4d303ecf39744925faaabb3a314。状态：发布隔离专项、独立离线阶段及主仓代表复验已通过；本阶段提交以Git记录为准，不代表全项目、实机或A06b布局已完成。下方早期轮次保留为历史记录，不按其旧锁指令执行。完整目标继续active。

## 事实与目标

R03要求旧结果不能覆盖新目标。CurrentDevice可以在Busy时改变；Capture/LoadNodes在首次RefreshGeometry之后才读取Current，后续只比较DeviceRow引用。Catalog会复用行并推进Generation，现有发布没有核验这些身份变化。Wall/Automatic结尾按完成时Current无条件ClearNodes/DebugImage。以上是可构造的源码候选，不当成运行红证据。

根问题是把请求来源、源行缓存和当前检查内容当作同一上下文。最小解决是明确三种独立有效性：固定请求来源；每个返回结果核验源身份；当前检查发布另核选择/检查修订号。拒绝用禁用设备选择、Input输入代次或清除全部缓存掩盖竞态。

## 选择与边界

- 推荐：只在相关Desktop ViewModel加入小范围快照/校验。源行身份有效时保留原后台截图缓存；检查内容按单调修订号接纳，不让旧任务清掉新节点。请求内固定runtime，正常几何预处理完成后建立后续截图/节点身份基线。
- 更简单的只加ReferenceEquals或在节点返回时比较serial不足：ABA仍是同一行，同串号可换Transport/Generation，后台末尾也会清除其他检查内容。
- 拒绝全局UI忙锁、删除缓存、取消全部后台批次、重写Core/ADB队列或新增运行时热替换入口。它们会丢原能力或扩大权限/维护面。
- 若原正常旋转/尺寸刷新被新规则取消，或批量墙不再完成其他合法行缓存，应推翻该实现。几何预处理本来就用于取得新几何，不能把自己的正常刷新一概当迟到身份失败。

本次只处理CaptureAsync、LoadNodesAsync、RefreshWallAsync、RefreshAutomaticScreensAsync和必要的Current/RefreshGeometry发布边界。Debug/Screenshot布局、原生保存对话框快照、未完成裁剪捕获、自动节点首载调度另属A06后续；应用/终端/API/Core协议、权限、用户配置与真实设备不改。

## 发布合同

1. 单设备Capture/LoadNodes在首次await前固定源行、runtime和当前检查修订号，不在返回后改读新Current作为请求目标。选择改变或有更新检查请求时，旧任务不能再向新目标发截图/节点请求。
2. 几何预处理使用固定runtime；返回后先核同runtime、同一源行仍存在和检查上下文，再从刷新后的Info建立后续设备调用的身份基线。新尺寸/旋转正常刷新可继续；一旦后续截图或Dump发出，Generation、Transport、几何、在线状态及目录身份必须保持有效。
3. 每次源行缓存/时间/截图状态写入前核身份。身份失效连源缓存也不得修改。只改变Current而身份仍有效时，源行缓存可按原意完成，不要求删除合法缓存。
4. DebugImage、Nodes、SelectedNode、导出根及ClearNodes属于当前检查发布。Current真实改变及新的显式Capture/LoadNodes/Wall检查请求推进单调修订号；后台Automatic只快照，不抢占新的显式检查。
5. A→B→同一A的setter恢复A既有缓存是合法动作，必须先记账；之后放回旧payload不得替新选择发布截图/节点。Dump未被当前检查接纳时，不先写没有对应节点缓存的“节点可用”。保持原截图阶段可先发布的行为，不改成截图与节点全成败原子事务。
6. Wall是在线设备批量截图；Automatic按显式serial集合。不能因Current切换就取消全部身份有效的后台缓存；其末尾仅在本批确实成功更新同一当前源行、检查修订号未变化时投影，不能清未拍摄B或期间新建立的节点。
7. 检查修订号只能在ExecuteAsync实际接纳action后推进，保持Busy重入no-op及finally恢复；不能让被忽略的重复调用废弃正在跑的请求。Wall/Automatic在Current=null或不属本批时仍完成合法源缓存，不套用单设备Current必需门禁。
8. LoadNodes截图身份仍有效但检查修订号失效时，可以接纳源缓存，但不能继续发过期Dump。显式Capture/LoadNodes失效沿现取消反馈显示“已停止”、Error空，不冒称刷新完成；真实后端失败仍Error可见。Automatic只因检查修订失效时不改其他显式请求的Status，批量源失效仍可继续合法缓存。异常不得当Task正常返回即成功，需检查Error/Status/Busy和后续真实请求可恢复。正常日志与清理仍有界。

## 任务与验证

| 优先级 | 任务 | 验证 |
|---|---|---|
| P1 | 正式受控红测 | 一致自建Demo runtime/VM与IDeviceBackend转发包装；TCS明确进入Discover/Screenshot/Dump后再变状态，不用Delay猜竞态；保留至少一个真实失败及正常对照 |
| P1 | 首次等待目标固定 | Discover挂起期间A→B及ABA，实际backend调用日志不能把A请求改为B；自己正常几何刷新后仍能读取 |
| P1 | 截图/节点迟到 | Capture截图、LoadNodes截图与Dump各测ABA；先记setter合法缓存/清空，再放payload检查引用、标记和事件，失效截图不继续Dump |
| P1 | 身份失效 | 通过真实Catalog刷新改变Transport/Generation/在线/几何，再释放旧结果；另至少一例只runtime.RefreshAsync推进Catalog，保留row.Info旧值，证明不能只查VM行；缓存/时间/状态与检查内容均不误写 |
| P1 | 后台与显式检查 | Automatic([A])挂起，CurrentB完成LoadNodes并选节点后放A；A合法缓存可完成，B图/节点/选择/状态/导出根保持。另测Current未变B、Automatic([A,B])旧批不得清更新显式B检查 |
| P1 | 原能力与失败恢复 | 正常单图/节点、Wall全在线与Automatic显式串号、缓存返回、Busy重复请求、单次后端异常、finally清理与真实新请求恢复 |
| P1 | 冻结与独立复验 | 同源四边界摘要；正式150、三完整桌面原检查与新专项、六shell及相关设备/监控代表回归；非实现者独立运行，主仓代表复验后单独commit/push |

## 夹具与证据边界

新增单一Desktop/STA正式夹具，沿用现有Demo --smoke入口，仅增加调用，不造新CLI。调用方传现Demo DataDirectory下独立artifacts子目录，夹具仍要求绝对路径/STA/artifacts；不能因此让旧不带--data-dir的默认Demo smoke突然失败，也不把夹具写入普通Demo配置文件。新runtime的Catalog/Automation/Input/Scripts/API全部使用同一wrapper，数据/SQLite/log在artifacts子目录，API不启动；不能只替换Backend而复用旧服务或双重dispose。每case的TCS在finally释放，待飞行任务排空，由该VM单一Shutdown处置。

记录阶段、源serial/行/Generation/Transport、实际请求序列、payload标记、当前图/树/选择/导出及节点事件、Busy/Error、清理结果；合取失败写各分量。原始日志不上传。不会操作用户profile、原实例0、模拟器或共享ADB。

此专项证明真实VM入口的受控发布隔离，不是按钮/Android/ADB wire/生产timer或自动首载证明。Demo没有同VM合法runtime热换入口，不能反射写runtime或伪造授权来取红；运行时冻结的生产代码审查与真实接入忙守卫另列边界。已完成crop跨页旧合同保持，不能为本次修复删除。

## r0实际红证据

仅在独立实施树加smoke调用和正式夹具，VM基线原始SHA`0025BF907708E6A9339209F3D9A314E71DB5518F6EAF950A8C4E96320ECB4E82`未改。r0补丁SHA`AB51C2DDB51FB0AC857BD87392B69F7FA87F2D5AFFF2A05A0DE767A4919747BA`，Build0警告/0错误（29.22秒），测试DLL`1BB813203982DF58EB4BC89DFE2A48A3723A840CAF1D85A15A77F953542FFCC7`。

正常对照6项通过：真实A截图、节点、选择与导出根均有效。下一case由TCS证明首Discover已挂起，此时A→B，先保存setter对B合法清空基线，再释放：实际日志为Discover→Screenshot(DEMO-002)→Dump(DEMO-002)，B图和树被改、NodesChanged1→3，Busy已恢复且Error空。这是产品错来源红，不是末尾PARTIAL-FIXTURE或指针/焦点前置失败。两个独立case均cleanupCompleted=true；root已直接读取JSON核验，不只采信实施者结论。

原始报告位于实施树`artifacts/apple-debug-context/r0-red-1000x700/data/artifacts/desktop-smoke/debug-publication/20261015-110131-e677310811da4001974c4715d98fd90b/debug-publication.json`，原轮次保留不上传。该夹具r0只有正常及Discover→B，其余合同明确未实现；修此首例后若到达PARTIAL仍不计全面通过。所有r0进程已退出，后续VM修复与r1扩展各在独立树，不能覆盖已交付的冻结补丁。

调用兼容性：实际接入行使用`Path.Combine(ViewModel.DataDirectory,"artifacts","desktop-smoke")`，避免仅以artifacts结尾而不满足严格路径guard；旧默认Demo路径未实际运行，不把源码路径修正当该用户配置验证。新夹具不改App/CLI合同或普通Demo配置。

## 第一版修复预检

实施侧VM第一版SHA`123932D470FB842196362DD50DD3488BA28866FE540E7975239570EE76690B1A`，VM-only补丁SHA`9AB554FE90DAB5A6F329201F31269B4C2EA69FCB148723EB6B24B527C167C9C3`。范围为四条链与私有快照校验，不改Core/权限/设置。正式150/150（52.42秒）；同一r0夹具的两case均PASS/cleanup完成，旧Discover阶段无后续设备请求、B图/树/节点事件保持1→1，新B请求可恢复。root已直接读取预检JSON及基线结果核验。随后按原r0合同PARTIAL-FIXTURE/exit1停止，不能把它算完整绿；DLL`58391AA379FBDC1684FD08D998FB83B65979DAC272D92A5903EF9C296BB1048C`。

非实现者静审同一SHA发现错误返回仍有候选：A等待期间切B、A后端抛异常，单目标可能落入ExecuteAsync的PresentError按CurrentB写日志/全局Error，Wall/Auto亦调用PresentError后Auto再记正确A，形成错归属。成功结果守卫不能替代异常完成路径验证。VM第一版暂冻结，r1夹具增加TCS故障和真实RollingLog flush反例后再决定修复；不全局改所有PresentError或删日志取绿。实际日志为UTF8 `.log`括号字段文本，不是JSONL。

r1将按每case独立runtime聚合失败，清理成功才继续下一case，末尾总体FAIL不会吞错取绿；尚未冻结/编译/执行。下一独立verifier先在28cdd4d基线VM取完整红，再机械导入修复以同测试/预期取绿。真实按钮、自动首载、生产timer与runtime热换仍未验证。

后续r1已冻结46个独立case，增量patch SHA`9E171B2B5D96AB21DBA6646ED9F49798A4C25B90B535163B30AF99D3E9811E82`，完整夹具源SHA`3B516F533F336FEC860CD727E6A2E7F456ED1D490C21D627FA38AE2FD46864DA`。root直接核过字节及静态合同，r0未改；状态已从编写转为独立baseline执行，不把上段历史未冻结状态当当前指令。覆盖含12种payload/身份组合、四种过期单读异常与两类批次日志来源，实际通过项待后续报告。

## 完整基线与夹具纠正

独立首次Build的stdout已到150/150，但ignored启动器`Task.WhenAll`重载出错丢失子退出码，不能作完整成功；保留后只改runner并正式复取exit0、150/150（51.54秒）。此时VM仍基线SHA不变，DLL`266980993BF9320CC2CF286C0F084C70CFBD86ED40B7B3369F96C53348950FCD`。首次GUI正常case通过，第二case在SQLite初始化事务失败，数据库路径258字符、journal候选266，余44未执行；不是目标发布产品红。构造失败会Dispose连接，进程全部结束。

只把隔离profile改为`artifacts/r1-red/data`后，同一DLL、同一fixture、未改VM，单次25秒完整执行46case，10通过、36失败，全部cleanup完成。实际最长数据库230/journal238；这证明短路径避开该初始化障碍，不据此宣称已完整确定平台路径上限。完整JSON SHA`AF1E1757FD3BC3BB00AC1C5A9D93DA8B7B5D3A30DEE0C5FE3A424FB6F03515F2`，分类摘要SHA`3440028B721FAC9342D0A5FF2156802679413BADADF03E154D5322ADC5E16CE7`，均保留不上传。

36失败中31项为产品首断言红，2项混合错误预期与真实日志错归，3项仅夹具错误，不能报告成36个产品缺陷。真实红包含Discover后偷换来源、ABA、12身份组合、Automatic清新B检查；8个已flush日志case中，4个迟到单读和2个正常批源故障被记到B，Current=null的批故障被记到host，Automatic还多记一条正确A。root已直接读取全部case、调用序列、分量和磁盘日志字段核验。

夹具两处共5case错误地要求安全UI含英文内部marker；既有`UiErrorPresentation`会将DeviceException映射为中文错误码，行为正确，不为测试改成泄露raw消息。r2仅纠正为与同一注入异常对应的安全中文精确文本，并核UI不含marker；Busy/Status/无节点/恢复及8磁盘原始marker归属检查均不变。r2增量SHA`9E39A116D30CD9011C3899945F2DAA68039CB963F0477EA23E7702851D2972F3`，完整源SHA`5CF52687D612C292A361FB75359D7F68347977D851F87602FC4C23F9B654FFD0`，原r0/r1和全部失败保留。

当前编排：独立verifier以基线VM重建r2夹具、短profile取同46case完整红；实现者并行只编码同四链局部故障归属，不BuildGUI。因为独立r1已实证8条故障日志且VM-r1这些异常分支未改，不再重复已知错误的VM-r1差异预检；最终同r2绿与独立复验不能省略。错误合同是固定owner/源serial留原始故障、身份无效不写旧行、单读检查已过期走停止而非污染新目标，同目标有效故障仍显示安全错误并恢复。仅允许四链私有失败类型在最终错误发布点重验，不改其他页面通用PresentError或削弱正常取消语义。

后续r2基线已实际完成：46case、13通过、33产品失败、46清理成功，26.09秒/exit1；只有原三项夹具假失败转为通过，其余43状态不变。5个安全UI观察均未泄露marker，8磁盘marker仍全部存在错来源。报告SHA`008BD7EFFCA469C5A5CD69CBF8C5ECC198E939466B3CBED205F48055536FA1BB`，汇总SHA`69789BA90B9FC2BA314B70505CB9CB921D8BC83DA2839CF6A8B56B98A3C07783`，root直接读核；此次只改fixture而按授权不重复150，Build0警告/0错误，VM仍原基线。

实现VM-r2源SHA`C6E3A641FAE2035E5756B6E1C41FB72C5BDC17369E1D55E7E8AF8CE9412E6C80`、patch SHA`D3E86B864882D78D942E62D25F2EA4C5F5803BDA2E86061B5CCAB72CC3C90795`已冻结但被root/独立静审拒绝，未构建或宣称通过：宽泛OCE catch会吞掉Automatic在外部token未取消、源仍有效时的后端取消；Wall同根因会静默丢原有per-row失败反馈。保留原件，先在同版补三个确定性用例（Automatic后端OCE、Automatic外部取消、Wall后端OCE），然后仅以私有SourceExpired子类型区分本地身份过期与后端取消。原46条件不改，不以已有成功发布测试代替取消证明；第三生产修复轮后若仍有阻断须明确升级，不无限循环取绿。

## 第三版专项与完整回归边界

r3正式夹具只增加三项取消合同，49case，raw`E3EF3FD5362091CA2C1BC0AF41B12D5AD15FBBA8EB898DC42DD711A7DCD051B2`、增量SHA`89DC4F8560DA4CC3EF2E7F4E318978DCF5368A6CDB868A79AA42D71015B52ADC`。先以未修取消的VM-r2实际运行：49=47通过/2失败，原46全部通过，全部清理。Automatic实际RanToCompletion、未观察到OCE且继续请求B；Wall没有源失败反馈且marker匹配0。原JSON SHA`3795B74DFDF7556AAEBA82DB74B164013A9469861BFA835459317BF26ADAEDE5`，root直接读核，未只靠静推断。

第三生产版仅四处区分内部身份过期与后端取消：私有SourceExpiredException只由RequireDebugSource抛；Wall仅skip该本地类型，其余OCE仍按原per-row故障记录并继续；Automatic只skip本地类型且先检查外部取消，其他OCE原样传播。VM raw`79137E2E6D0DF2FF28FFA82471A1E9FA5E093D64471F73F0A1084E34883A03D4`，累计patch`594CF2D747E4BB7CAC0061478DAAD7F4D5375E8785DE9050ED7DF804D6D513A8`，两人静审后才执行。

主实现150/150（50.86秒）、0警告/0错误；DLL`F00A5EA6198344A72737874663024EDD839D76422C8AC13D3E7E96D474E7AEB7`。单1000×700中A06专项49/49、401检查/191观察、49清理完成、9个已flush磁盘来源均通过，JSON SHA`AAE6C58255BDF63DDFECA3BB3017FDCB71894744D52C85BBD970721BA10196C4`，root直接读核两种Auto原Task均Canceled/onlyA以及Wall安全源失败和B恢复。旧文案36/11随后通过。

但该次完整smoke最终exit1：旧Appearance在Hover(TextBox)真实IsMouseOver首败，尚未到Native/Filter/Monitor/12页idle等后续，不能称完整绿或整体验收。原首败无指针实际位置/active/hit分量，登记AP-023待诊断，不归因操作者。三生产源保持冻结；仅为旧Hover添加有界只读时序，必须保留原SetCursorPos/Synchronize/Render/Delay30/Check的调用顺序、等待与条件，失败路径同样输出诊断。此非第四生产修复，不以日志代替产品修复；独立最终矩阵和主仓代表复验仍待执行，当前不提交推送。

## 最终独立验收

第四文件只加Hover只读诊断，patch SHA`0486AA9D7FB4160CFFFE0FF5AA28526C3AE6525E907ED638579EAF7F29ADA742`、源SHA`304DF55D478E644834608975A51CCA7B3FE0FD2086DFC2BA1AFC3D4101FD8C90`，原断言和显式等待不变；常规读取/序列化仍有开销，不宣称绝对零时序影响。root与非实现者静审后才机械整合，未修改生产主题、布局或焦点行为，不将其称为AP-023修复。

独立结论`PASS-OFFLINE-A06A-DEBUG-PUBLICATION-ISOLATION`。最终汇总`artifacts/debug-publication-verifier/final-verification-f1.json` SHA`E9C1300886B8E55C1F3413DB4CC08974B6E304F66044E82BD907F31F3EF457D3`，root已直接读取、核摘要及14进程全部退出0；同DLL`943254BF4BF5CF58D5DB9C9D22C57BF3F6115B512B9670399A78EE65E3093DE6`，最终矩阵无重试。

| 独立运行 | 实际结果 |
|---|---|
| Build/Verify | 0警告/0错误，150/150，51.09秒 |
| 1000×700、1440×900、1920×1080完整Demo | 每轮49case/401检查/191观察/49清理/9正确来源日志；209外观、36文案/11观察、73筛选含32分类/22观察、25Native守卫、12页idle和4设备行，error空 |
| 六公共shell尺寸 | 每轮135项，实际尺寸精确；主图/抽屉独立保留 |
| 设备390×640、1000×700代表 | 889/140、847/139，WPF路由/几何与原上下文保护通过 |
| 监控390×640、1000×700代表 | 各952/308，合成帧与几何/协调观测通过，非真实视频 |

独立目视三桌面设备行、390抽屉、设备末列/主控尾、监控合成帧和1000资源。Hover三轮各9快照，afterWait均ownerActive/实际指针匹配/IsMouseOver true，仍不能倒推原首败原因。辅助模态截图的固定文件名在六shell共享目录被后续覆盖，只保留最后一轮；六份JSON/断言、主图/抽屉与原日志仍独立完整，不声称六尺寸辅助模态视觉证据齐全。后续runner应按case分目录，当前不补跑取绿。

最终冻结仅四个Desktop文件：WorkspaceViewModel（固定来源/身份/检查发布及局部故障归属）；MainWindow（Demo smoke中同一artifacts路径调用）；新DebugPublicationSmoke（49独立一致runtime/TCS用例）；原AppleAppearanceSmoke（只读Hover诊断）。root主仓机械导入前逐patch核SHA/apply-check，fixture因gitapply行尾为CRLF而raw不同，先核canonical blob`631827d62a83a4cc35de6e48976d423f4434df35`内容一致，再复制冻结LF字节。四raw最终准确匹配独立清单，没有Core/Host/API/App、用户数据或权限改动。

主仓最终Build/Verify为0警告/0错误、150/150（51.65秒）。同一最终DLL`55D2AC54BC89B7C9A5A8B2589A16B479215DDC20FC8EDAFAFD8B34E66C9A67B7`，fresh1000完整与390shell代表首次均exit0，49/401/191/49清理/9正确日志来源、旧完整回归及135壳层断言通过。主仓49报告SHA`D1558009EF5B046E51B47872B36C34F197CF112E86977CCC46C30E7EFC13C50F`；`desktop.json`确认listOnly/groupControlsRemoved/selectionTargetUnified/zh-CN/error空，root已直接读核并目视当前单一列表。所有必需exec与Desktop进程结束、锁空闲；实际运行profile前后SHA仍`E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0`，没有恢复旧配置。

未验证范围始终包括真实按钮到Android效果、共享ADB/真实重连旋转、生产timer/自动首载、无合法Demo入口的同VM runtime热换、A06b固定栏/保存对话框/裁剪捕获、真实多路视频与全项目稳定性。AP-023及AP-015/019/020原失败仍开放。新原始图/日志/数据和ZIP不推送，源码归档仍含基线历史验收材料，AP-021保持；逐SHA完整性不代表隐私净化或纯源码认证。
