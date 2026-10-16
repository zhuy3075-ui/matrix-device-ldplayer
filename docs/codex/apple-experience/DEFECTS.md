# 问题与修复登记

日期：2026-10-16。优先级按串台/数据风险、功能阻断、可用性排序。未实现和未验不称为已修复。

| ID | 级别/状态 | 事实与根因 | 处理/验证 |
|---|---|---|---|
| AP-001 | P0/代码修复已独立离线验收；真实投屏待验 | 主控面板标示MainDevice A；原NativeView读取CurrentDevice B，红测确认错目标 | [A02报告](A02_MAIN_TARGET.md)：25守卫/149项/独立三尺寸及主仓最终1000通过；主控、运行时/代次/transport及进程PID/birth/path冻结；真实确认/启动/嵌入仍NOT-RUN，纳入A11 |
| AP-002 | P1/已独立离线修复 | 最小尺寸四处不一致：XAML1000×700、smoke Clamp1000×700、设置Apply拒绝小尺寸、Normalize放大到1100×720 | A04四契约统一390×640，150项、六尺寸实际HWND/大小及390/768 Apply/Save/新Load通过；不扩张为全部固定页面或真实DPI通过 |
| AP-003 | P1/单实例共享层安装/停止/卸载已验，完整GUI/双实例仍缺 | 当前应用批次UI未有真实双实例安装/卸载完整证据，旧测试走过时ViewModel入口 | [CMFA首验](A07_CMFA_APPLICATIONS.md)真实安装及211035版本/路径已核，原权限页主页FAIL不改；[独立owned清理](A07_OWNED_CLEANUP.md)c01实际停止两个进程、保留安装后卸载及列表/PM缺包。非当前GUI或双实例证明，不关闭原验收 |
| AP-004 | P1/已独立离线修复 | 设备详情全选/清空/行勾选直接改群控；全局StatusFilter经统一目录影响监控卡片，关闭详情只Hide，筛选继续生效 | [A05a](A05_DEVICE_STATE.md)：实际红测4→0，删除VM全局筛选和详情第二套群控；列表局部筛选、监控独立。150项、三桌面每73专项/209外观/25守卫、六公共shell通过；真实视频连续性仍NOT-RUN |
| AP-005 | P1/开放 | 节点/截屏/应用/工具/设置/监控固定宽栏与固定弹窗，窄屏不可达 | A04..A10逐页重排与六尺寸真实路径；不整页缩放或藏功能 |
| AP-006 | P1/证据缺口 | 原控件审计仅OBSERVED-NOT-EXERCISED，遗漏菜单/模态/画布/树/虚拟行等 | CONTROL_CATALOG补充S项，逐控件效果测试，不用控件数代表覆盖率 |
| AP-007 | P1/依赖与风险待审 | 私有agent/ROM/代理/硬解等能力及HTTP Toolkit抓包尚无当前可用/安全证明 | 保留单项NOT-IMPLEMENTED/BLOCKED，记录真实失败和替代；不自动改证书/代理/共享服务 |
| AP-008 | P1/候选依赖已取证，实机链未验 | 无完整SDK或自有测试APK；已只读取得Maven framework与Google Maven AAPT2/D8/apksig固定工件并核发布SHA1、计算SHA256、检查归档/POM许可，未执行 | [依赖取证](AP008_DEPENDENCIES.md)；A07仍需真实构建/签名验证与Android效果。apksig不是现成CLI，不接受SDK条款、不改用抖音清数据/卸载凑覆盖 |
| AP-009 | P1/已独立离线修复 | MainWindow派生类型未自动命中Window隐式主题，原字体/布局取整设置未真正应用 | A03显式引用现有样式，真实实例字体及三尺寸回归通过；不代表真实DPI切换已测 |
| AP-010 | P1/取证缺陷已独立修复 | 逐页截图在offline恢复后未等待布局，设备页只有表头，最终另图有行；旧通过记录的分页视觉证据不足 | A03新增idle/render有界等待、恢复状态记录/实际行视口断言；三尺寸tab0与最终PNG SHA一致，逐一目视；旧final-*保留并拒绝作通过证据 |
| AP-011 | P2/开放 | A04 Owner-fit在首次Apply前创建对象并订阅Owner事件；首次适配拒绝且局部窗口未Close时可能被Owner委托保留 | 独立源码审查发现；A10补失败路径订阅生命周期与回归，不能把几何负例当清理已验 |
| AP-012 | P2/开放 | A04长页脚仅分行布局、ellipsis与tooltip；边界通过不代表触屏可读全部被省略文本 | A10提供可点击/可键盘读取的完整状态路径并实测；当前不宣称手机触控信息可达已通过 |
| AP-013 | P2/已独立离线修复 | 表头“全部可操作设备”提示未说明只勾选当前筛选的可选择行，可能包含关闭/离线；取消却清除全部隐藏选择 | [A05b文案](A05_DEVICE_LABELS.md)：实际表头精确名/提示、原三态/隐藏清除均保留；独立150/三桌面36专项+原回归、390shell通过；不改授权门禁 |
| AP-014 | P2/已独立离线修复 | 行内PrimaryLabel只看AndroidStarted，未知/启动中快照有时仍显示“启动”，而核心协调器实际等待而非重复launch | 同报告：真实绑定按钮11阶段观察，36项通过；Connected+Device先调试，其余条件标签及可能失败说明；无门禁/分类/协调器改动，不称实机效果已验 |
| AP-015 | P2/开放，测试可靠性待诊断 | A05b初次红测在新专项前被旧Appearance恢复动效断言拦截；A04亦留有另一旧Toggle偶发失败，不能因后续通过称根因已解 | A10保留首轮日志并补焦点/Active/真实帧/偏好及Popup实际状态诊断；不改生产动效或削弱断言取绿。A05b后续完整轮次单独记录，不将此失败计为文案红测 |
| AP-016 | P1/已独立离线修复 | 390设备动作初始X408越出374客户区，主控固定侧栏与旧映射不适配；迭代实际复现表头resize grip遮挡、默认非null ElementStyle使tooltip未装、旧恢复覆盖新滚轮，以及390×844中间宽屏clamp消费原V6意图 | [A05b1](A05_DEVICE_RESPONSIVE.md)：独立150、六正文889或847/三桌面/六shell135；主仓150/两390正文/1000完整/390shell通过。保留全部红和夹具错误、根首轮指针前置失败；不混称实机/自动timer/物理输入已验 |
| AP-017 | P2/源码候选，未复现 | MonitorIcon异步finally无条件重新启用按钮，可能覆盖await期间协调器的身份禁用；实际Open入口及后续仍有资格/冻结配置/身份校验，当前无越权证据 | A10独立复现：真实卡片按钮+TCS等待，资格变更/禁用后放行，观察完成与下一次Reconcile；Demo替身不能冒充真实ADB竞态。不得混入A05b2布局或把源码推断写成已发生故障 |
| AP-018 | P1/已独立离线修复 | 监控330/308固定宽与28高动作不适配小屏；旧VisibleIn把仅在外框滚动条条带的卡片判可见。适配迭代还复现随宽改视频高导致单卡Auto滚条反馈振荡 | [A05b2](A05_MONITOR_RESPONSIVE.md)：实际视口重排、44动作、长文滚读、图表纯重绘与原180高切断反馈。freeze4独立150/六监控各952/308、六设备、三桌面、六shell135、42CLI及主仓五代表通过；AP-019/020仍开放，非真实视频或全目标通过 |
| AP-019 | P2/测试可靠性候选，首败未确诊 | A05b2 r6首640过，844选卡成功后上下文合取失败；另一次独立freeze2旧设备390×640在workspace919五合取失败，尚未发指针事件。两次旧日志均无分量，不能合并根因 | 原失败均保留；分别只加只读分量/事件诊断，原条件和等待未改。监控r7一次过；设备freeze3单次889/140过、54事件无丢弃且边界前无失活，均不是生产修复证明。A10继续诊断；不归咎操作者或把后续绿改写为首次全过 |
| AP-020 | P2/新监控夹具焦点合取失败，待诊断 | freeze3独立监控1000×700在child.IsActive && !owner.IsActive && MonitorActive失败，939检查后停；原日志缺三分量和Activate返回，不能据此判产品或环境 | freeze3当轮独立暂拒绝且余项未执行；只补只读诊断后freeze4布局/观测完整通过，根因仍开放。原调用/等待/断言不变，不抢前台或跳过case，不与AP-019合并根因 |
| AP-021 | P2/源码归档范围待分类 | 旧New-SourcePackage排除artifacts/data/bin/obj与英文截图，但仍收录docs/verification历史PNG/JSON/txt验收材料；逐SHA通过不等于归档完全不含原始证据 | A12区分正式文档与原始证据并单独处理打包范围，不删除历史用户文件或改写Git历史。本轮提交只新增监控源码/正式测试/脱敏文档与清单，不新增上传这些旧材料；不得称当前ZIP为完全纯源码包 |
| AP-022 | P1/已独立离线修复迟到发布与故障归属 | 原LoadNodes首次等待后偷换来源；ABA/行及Catalog-only代次失效仍写旧缓存，后台清B新节点，A故障错记B或host均有受控红。中间版取消回归亦实测47/2后最小修正 | [A06a报告](A06_DEBUG_PUBLICATION.md)：独立150/三桌面各49及旧完整回归/六shell135/四代表，主仓150/49/390shell通过，9磁盘来源正确；保留合法cache/原CLI/跨页crop。仅直接Demo/STA VM隔离，不代表自动首载、旧已完成缓存换代或实机已验；AP023仍开放 |
| AP-023 | P2/测试可靠性待诊断，首次失败未确诊 | A06a主实现VM第三版150/49业务全过后，旧AppleAppearanceSmoke.Hover(TextBox)的真实IsMouseOver断言失败；原日志无实际指针/ownerActive/命中分量，不能归因用户输入或宣称产品悬停已修 | 保留首次日志且完整smoke当轮FAIL，后续旧检查未执行；仅授权Hover增加有界只读primitive时序和失败stderr，原指针调用/断言/等待不变。不得用49业务绿或后续未复现倒填该次全绿，不与AP-015/019/020擅自并根因 |
| AP-024 | P1/已独立离线修复活动阻塞导致的漏首载 | 自动首载在HasActiveOperations返回前未登记pending，Busy/传输/脚本释放后丢请求；在途A到B到同一A旧结果被A06a拒绝后亦未补载 | [A06b1报告](A06_AUTOMATIC_LOADING.md)：基线16例8PASS/8FAIL且16cleanup，首轮只修两生产文件；独立150/三桌面每16+旧49及原完整回归/六公共shell135，主仓150/16/49/390shell通过。默认Demo/活动定义/授权与发布门禁未改，真实Android/定时器/同row重连不由此替代 |
| AP-025 | P1/已独立及主仓离线修复节点正文 | 固定star+300+270令390图0宽、768图82宽，原工具32；适配中复现Header外padding与滚入目标不一致、四大尺寸属性回原窗口丢offset | [A06b2a报告](A06_NODE_RESPONSIVE.md)：同对象三/二/窄分段、局部44和属性未消费滚动意图；独立150/三桌面每11/905/11清理及旧16/49原套/六shell135，主仓150/完整11与原套/390shell135均通过。新5种用户覆盖保护保留，Focus返回值夹具前置误设另行纠正，不冒充产品修复；非Android/物理输入/全部触控或全目标通过 |
| AP-026 | P1/截屏正文离线修复并独立通过 | 固定300右表令两390图像真实仅26宽，其余四尺寸原刷新按钮完整44命中检查失败；第一轮长坐标文本预算51.1767大于48 | [A06b2b报告](A06_SCREENSHOT_RESPONSIVE.md)：同对象宽双栏/窄画面色值、局部44及坐标列100最小预算；独立150/三桌面每10/630/257/10清理与旧11/16/49全套、六shell135，主仓150/1000全套/390shell135通过。原两轮红保留；非物理点击/DPI/Android、缓存/原生保存/在途裁剪认证 |
| AP-027 | P1/已独立及主仓离线修复完成缓存 | 已完成图/节点没有身份戳，同row新Ready或重连仍保留旧检查；Catalog已t12/g2但旧row t11/g1仍真实导出旧PNG/JSON并显示完成 | [A06c1](A06_COMPLETED_CACHE.md)：原15红保留；首14/4经原断言七叶诊断确认为新夹具误预期，仅修意图/有效门禁合同，生产41E4不再改。最终独立150/三桌面各18/295/18清理及旧四套/六shell135，主仓150/完整18及旧套/390shell135通过；离线历史及PNG/JSON产物真实核验。AP028、同row自动首载和实机仍开放，不称Android越权或全历史追踪已验 |
| AP-028 | P2/状态历史观察边界，待取证 | Catalog只存最新快照；VM未同步/未消费的Ready经Unknown/Unauthorized到Offline中间态可能无法从最终g+1区分，尚未实际运行该串联 | A06c1只承诺消费时Owner/Row/Catalog字段资格，不宣称全历史追踪；可见非Offline拒绝且清戳后不复活。完整目标后续验证中间态撤销需求和Catalog事件/版本合同，不扩大本地只读历史为新读取或输入许可 |
| AP-029 | P1/已独立及主仓离线修复 | 同Current行新Ready后旧缓存正确撤下，但完整目录接纳不创建自动首载意图；有空闲也无Shot，真实传输/脚本阻塞时pending仍false | [A06c2](A06_RECONNECT_LOADING.md)原19=8PASS/11FAIL/19清理及r0两CS4014保留；首生产仅VM/Main30+/1-，新fixture仅显式discard+4ASCII，19动作/断言/等待不改。最终独立150/三桌面各19/289/19清理与旧五套/六shell135，主仓150/同19与原五套/390shell135通过；真实externalpending true→strict采纳false、旧在途payload拒绝及Close均核。仅完整VM接纳的自有Demo路径，不代表任意Info/Catalog-only、生产timer、Android/native保存/runtime热换或全目标通过 |

## 新候选与测试环境

| ID | 级别/状态 | 事实与边界 | 后续验证 |
|---|---|---|---|
| AP-030 | P1/原生保存源码候选，尚未复现 | SaveImageAsync在原生框返回后才取crop/图；合法新帧会清crop，存在裁剪退化新全图候选，但首次实际输入未到 | [A06d](A06_NATIVE_IMAGE_SAVE.md)保留r0；不能把工具捕获超时当产品红。恢复实际交互后取证原图/裁剪保存意图，再实施，不默认取消合法保存 |
| AP-031 | P1/测试工具前置故障，非产品缺陷 | sky两次自有WPF窗口capture timeout，已按一次恢复规则停止；真实crop/保存Click0，fixture90秒前置超时后正常清理 | 原FAIL/清理/127源与配置证据见同报告。单独helper只读诊断不能代替原生验收；不改生产渲染、共享服务或权限来凑通过。其他页面安全工作继续 |
| AP-032 | P1/已独立离线修复，主仓完整GUI仍有限制 | 应用页把显示名称纳入连接身份；metadata仅改名后旧应用名称不更新、启停/确认/在途List误拒绝 | [A07a](A07_APPLICATION_IDENTITY.md)原10例3红保留；r1两生产文件后独立新10/114检查及完整旧套/12页通过，四源raw同主仓、127源逻辑同，主仓150通过。root-r02在新hook前旧截屏前置失败，新10当轮NOT-RUN；限域交付已获独审，AP-033不冒称已修。在线、Runtime、明确选择和活动门禁未放宽 |
| AP-033 | P2/输入源缺失已取证，间歇成因仍未知 | 旧coordinate-route在RaiseEvent前失败；[诊断p01](AP033_INPUT_DIAGNOSTICS.md)三快照均ActiveSource/Captured/DirectlyOver=null、目标源有效；after-set OS和直接映射命中Image，之后OS移动、WPF仍屏幕零点 | p01独立150过、GUI FAIL，旧19/18过、截屏9PASS1FAIL/10清理，其后NOT-RUN；该轮root未重跑。49+仅日志不变断言。后续A07b1独立及主仓完整smoke实际通过，仍不证明输入源成因已修。不归因操作者、不做坐标生产修复、不并同AP-028，不把一次绿当关闭依据 |
| AP-034 | P1/脚本、传输与API排空恢复已独立及主仓离线修复 | 原12例7红证明脚本/稳定Push丢pending；另有API最后请求归零无通知，A07b2原12例8红，真实HTTP排空后仍无List | [A07b1](A07_APPLICATION_PENDING.md)历史12/310保持；[A07b2](A07_API_DRAIN.md)仅Host通知与页面补排原350ms，独立/主仓各14/496/14清理及158/旧套/12页通过，147输入/19旧源/PDB核同。保留全部门禁与失败不循环，日志双故障未测；正文、真实runtime热换、Android/物理GUI及双实例仍开放，不称所有刷新已完成 |
| AP-035 | P1/已独立及主仓离线修复 | 原db252/journal260初始化SQLite14已复现；新正式原代码152/158有6真红。原生路径未扩展；首修复验证还遭旧产物，PDB核原源后保留该失败 | [A13报告](A13_SQLITE_PATH.md)：唯一产品主库/备份Windows扩展路径，原位置/事务不变。有效独立158/51.43秒、主仓158/51.76秒且PDB三源一致，主仓四普通长路径CLI通过；迁移/拒绝/文件释放覆盖。完整GUI/AP-033、UNC、非Windows及真实双实例不由本项关闭 |
| AP-036 | P1/实机验收前置阻断，非产品缺陷定论 | 新装CMFA已取得精确包进程，但Android权限请求页占前台，主页效果断言真实FAIL | [CMFA首验](A07_CMFA_APPLICATIONS.md)保留l01 FAIL及原ledger；后续[独立c01](A07_OWNED_CLEANUP.md)停止/卸载成功不解决或关闭主页前置。当前测试安装已移除，未授权限/VPN，未重跑启动；恢复可靠GUI观察后另测，不把权限页当主页 |
| AP-037 | P1/应用布局有限域修复验过，关联补验保持 | 原应用固定双栏在390宽挤出操作，命中不足44；长字段/Tooltip阅读不完整 | [A07c](A07_APPLICATION_RESPONSIVE.md)/[A07d](A07_TOOLTIP_DIAGNOSTICS.md)原红保留；c02及主仓r01各158、新12/2890/12清理，六应用尺寸三表首中末/全列、取消/恢复/选择/焦点过，原17源Smoke链/12页过。限域独审批准开发分支交付，s01/AP041及真实双实例/GUI仍开放；不把共享Theme间接风险或全GUI缺口关闭 |
| AP-038 | P1/七点模式误判已实证，最小测试修正验过 | 旧测量用Ideal、实际版本Tooltip为Display，143宽下30.48与17高度不一致 | [A07d](A07_TOOLTIP_DIAGNOSTICS.md)d01原5PASS7红保持，7图完整尾-long支持判据A。只改测量用实际mode，宽/font/DPI/高度predicate/+1不变；独立及主仓新12全过、各232诊断/error0/133图核。仅解释这七点，不扩大到全部Tooltip/DPI，旧失败不回填 |
| AP-039 | P1/字形试验范围及输入已修正，主仓有限域验过 | c01在原Fonts.Count>20与全部历史Key当前字号=baseline×1.5合取处失败 | [A07d](A07_TOOLTIP_DIAGNOSTICS.md)原117失配保留；7248夹具首次setter本地覆盖+当前完整域判据后，c02与root-r01各12/2890/12清理过，当前505含344隐藏全登记/1.5/0缺失失配、历史128保留，末transform/SameCalls/glyph图过。非DPI/字体绑定保真，不推重置时点；s01/AP041与真实GUI仍开放 |
| AP-040 | P2/摘要修复离线复验通过 | 读取应用后的摘要原无条件包含“安装命令完成（包名未核验）0”，易被误认为安装发生；实际6case中的4个纯产品红成立 | [专项](AP040_APPLICATION_FEEDBACK.md)仅非零CompletedUnverified保留原段；实现/独立/主仓各169及6case/14/AP042/3E通过，进度/失败/取消及未核验1保持。仅摘要修复，不解释列表空的原因或当GUI/实机安装验收 |
| AP-041 | P1/报告子项已修，原指针负例未复验 | s01首device390×640的offline-reject未达OS/WPF指针一致前置，随后非有限Double使原JSON报告再次失败；受控诊断证明同代row Offline撤图但Visible时旧公式可非有限 | [报告出口](AP041_REPORT_RELIABILITY.md)同一实际writer红6PASS/8FAIL，局部double编码后原14类全PASS；独立与主仓各158及原3E几何/22Task/cleanup保持，8条预期stderr完整，FAIL/原异常/有限JSON不变，全局仍拒绝NaN。仅报告子项通过；原S01/11NOTRUN、AP019/020/033及实机仍开放，不推原具体坏字段或OS同因。下一独立阶段评估有效图像/有限坐标的后置定位合同，保留每个原Down/Up及零副作用断言，不整批跳负例 |

## 工作区无障碍补验

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-042 | P1/原离线PRODUCT失败未解释，开放；父链方案已证不足 | [原位日志](AP042_PEER_CACHE_DIAGNOSTICS.md)146×3/缺3/残留111保持；[官方对照](AP042_CLIENT_OBSERVATION.md)双绿不能证明因果。新[父链负实验](AP042_PARENT_CHAIN_RESULT.md)实际标准Window→同一tabs成立，但两臂仍各4条原Product红；原任务/清理及守卫通过 | 实验C#不合入生产，不为夹具绿追加Reset/Invalidate；225及MCP14不替代GUI，outer0仅实验完成。转A09安全页面业务，外部导航/几何/捕获与全GUI、实机仍开放，不重跑到绿 |
| AP-043 | P1/初始正文改善已证，外部导航缺口开放 | 首轮F18整树null保留；[对照及AP045](AP043_SKY_AB.md)旧48/新200总节点含正文；另已交付D123显式激活后仍唯一click几何错误、无后置state，效果未知 | 精确owned清理/流终态，无截图/替代/重试；停止该GUI输入分支，待受支持诊断或外部条件变化，不将工具/环境/peer定因。应用导航/返回/正常Close和全GUI未过，继续其它安全实现而非缩小目标 |
| AP-044 | P1/格式边界已离线复验，GUI未验 | 自动化picker原列.json，但Load只接受ZIP；实际红六非法格式均抛原InvalidDataException/提示不明确，其余163项过。r01编译错误另存不算产品红 | [格式边界](A08_SCRIPT_FORMAT.md)仅ZIP构造格式异常转中文400/1007、picker去.json；实现/独立/主仓各169通过，合法ZIP资源及Demo Tap1、拒绝Tap0过。未扩大到Entries/解压错误或支持通用脚本；GUI/真实设备和完整A08/A14不由此通过 |
| AP-046 | P1/参数绑定业务独立及主仓有限域通过 | 原参数编辑仅保存、不影响实际步骤，JSON null丢失后错误回退默认并接纳运行；Core24原1绿23红含新API缺失，独立VM4原2PRODUCT红2绿 | [A14a1](A14_SCRIPT_PARAMETER_BINDINGS.md)显式V1全字段绑定、私有JSON选项保留null、同原Scheduler执行快照，VM在metadata前完整编译。作者/独立/主仓各193与同VM4过，旧169/24Smoke保持；非GUI、真实双实例或完整作业身份/MCP/抓包验收，AP042最新FAIL不覆盖 |

## 脚本任务能力

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-047 | P1/逐任务能力缺口，离线独立及主仓通过 | 原调度器只有serial当前运行，状态/日志被下次覆盖；动作调用时才捕获transport，未冻结整个任务；这是新能力缺口，不伪造缺接口编译为产品红 | [A14a2](A14_SCRIPT_JOBS.md)共用旧引擎与占用表，新增JobId、目标身份、按任务取消/状态/输出/历史；实际队列与shell发送前复核，独立及主仓各209、原4/新5 VM过。限域交付以Git实际为准；真实设备、完整GUI及全RPA继续开放 |

## 自动化工作区

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-048 | P1/自有WPF/Demo独立及主仓有限域通过 | 原七例六尺寸有裁剪/44命中/长字段阅读红，390×640长摘要及目标、真实100高错误栏使正文被挤出；记录列另有State MultiBinding测试强转与disabled enabledHit误oracle，已分别归类不当产品 | [A08b](A08_SCRIPT_WORKSPACE.md)复用原四视图及全部5/8列，有限摘要与结果全文阅读；原取消按钮迁到记录标题释放一行。独立/主仓各原209、VM9及十二组实际控件全过；390参数/记录98、日志88、结果118。原生picker/物理输入/实机/DPI及完整GUI仍未验，不扩大为全页验收 |
| AP-049 | P2/同任务日志选中项丢失已有限域复验 | 实际Scheduler在第二TCS门38→39日志，同JobId，原ListBox选中12→-1、Reset1/Add39；滚动offset8及锚点y1保持，不扩大成全部滚动损坏 | [A08b](A08_SCRIPT_WORKSPACE.md)同源entry引用尾窗增量、切来源重载，保留500/重复文案。独立/主仓十二例含原实证与真实575步500尾窗隔离全过，选中12→12/Reset0，首日志77→78、选B不混A；不以此关闭其它GUI/真实设备验收 |
| AP-050 | P2/夹具角色修正已获实机有限复验 | r03真实双实例首个00-tap原判Source A无效果，但独审图/节点确认A已进搜索页，exact search_src_text为AutoCompleteTextView且聚焦，B节点/PNG保持；夹具只认精确EditText类 | [A13实机原件](A13_DUAL_INSTANCE_SYNC.md)保留live-r02 FAIL/13NOTRUN。r04精确角色+Focused及两Ready模式获独审；r05四态及未勾选tap真实语义均获独审，修正的是测试oracle，不是产品点击缺陷。原outer1属AP052，不扩大到GUI/文字或30分钟 |
| AP-051 | P2/滑动准备前置已获实机有限复验 | r04真实Ready首轮00-swipe发前原四锚点门未过：横屏Settings大header占至409，inner仅311高，3title含裁剪第三；正式swipe未发送，不能归为产品手势失败 | [A13实机记录](A13_DUAL_INSTANCE_SYNC.md)保留ready-r01 FAIL/12NOTRUN。r05实际八次单目标header准备，各例每台至多一次，重取baseline后原四锚点/映射/效果判据保持；独审四态正式swipe真实变化，未把setup折叠算同步，不改显示/阈值或循环取绿。原outer1另列AP052，完整GUI仍未验 |
| AP-052 | P2/成功报告字段消费已获独立离线复验 | ready-r02五child均0、夹具16completed含14真实效果且独审通过；外层StrictMode读取不存在的summary.error报PropertyNotFoundException。JsonDefaults省略null是既有合同，非产品动作失败 | [A13记录](A13_DUAL_INSTANCE_SYNC.md)保留原outer1/livePassed=false。另立只读合同审计实际2正例/6严格拒例及158原件长度/SHA保持，audit4D9C9B70...BA232获独审；仅允许省略/null，真实错误/错误类型/缺必要字段仍拒，不改原件或重跑设备，不认证GUI |

## MCP实际接入

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-053 | P2/日志消费问题已独立核定，原失败保留 | 独立r04两restore及Build60528均0，原209实际通过；外层以严格UTF8读取含C8字节的混合日志而失败，MCP未启动 | [A15b](../automation-expansion/MCP_STDIO.md)保留execution321461...BD2C及六raw。两独审以Latin1一一保字节映射仅提取并断言ASCII PASS，209名序/result同；新续跑已实际越过该门，不重Build，不将非ASCII全文冒称已解码或改原outer1 |
| AP-054 | P1/Schema夹具修复已独立及主仓有限复验 | 实际wire ListToolsResult只有tools，旧夹具用默认STJ重新序列化SDK DTO，添入线上缺失的_meta/title/icons等null，与严格Schema冲突 | [A15b](../automation-expansion/MCP_STDIO.md)原PRECONDITION-FAIL保留。专用官方序列化并按实际raw请求id验原Schema；领域JSON与explicit-null负例不动。独立与主仓均实际14顶层/227调用通过获独审，不能扩大为13类未验或真实设备通过 |
| AP-055 | P1/受控Stream路径自然清理已独立及主仓有限复验，旧Stdio兼容失败开放 | 旧官方Stdio等待进程退出未关闭stdin，与relay形成EOF环；原SDK45512 exit-1/缺报告保持。新路径是官方StreamClientTransport接原Process真实管道，不是原Stdio问题已修 | [A15b](../automation-expansion/MCP_STDIO.md)独立及主仓各14真实会话获独审：13自然0、复用RPC id负例自然1，原输入关闭/流与SDK终态、无kill/pending/保留/清理错；12raw拒绝也通过。real、崩溃/背压等13类NOTRUN保持；不用stop-file、反射或假transport |
| AP-056 | P1/标准同Runtime入口独立及主仓离线修复通过，实机并发待验 | 原13f先查busy后写与单命令队列不能保护完整动作；API已进入仍接纳同A脚本，HTTP取消但原后端未返回仍可抢占，忙B时Input组却先写A | [A15c](../automation-expansion/MCP_WRITE_OWNERSHIP.md)原209通过、新13的6业务红/4合同缺失保持，后继FIFO和旧VM5红保留；最终共同门/完整组结束屏障后独立与主仓各225、MCP14/13NOTRUN、VM5及三早拒获独审。原任务/流终态、189输入/156产物/实际14PDB/用户3实核，3既有警告0错；跨进程、Runtime换代、真实MCP/实机并发和完整GUI不由本项替代 |

## 设置工作区

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-057 | P1/自有WPF独立及主仓有限域通过 | 实际数值输入abc/空值有绑定错误，旧保存仍使用上次整数并改文件、显示成功 | [A09a](A09_SETTINGS_WORKSPACE.md)原绑定逐项更新/检错，修正后恢复；同原命令和Editor，独立及主仓16组过。不以直接给VM赋整数代替真实TextBox绑定 |
| AP-058 | P1/自有WPF独立及主仓有限域通过 | Editor允许Store会夹取的投屏大小/FPS/超时；FTP/相对Agent地址亦能保存而新Runtime不接受 | 同报告对齐既有Store有效范围并验证HTTP/HTTPS，最终几何覆盖后复核宽高；合法上下边界/非法输入/清空实测。没有扩大后端范围或连接Agent网络，额外实际超宽前置仍NOT-RUN |
| AP-059 | P1/自有WPF独立及主仓有限域通过 | Store合并外部FileRoot/窗体值后Editor显示及original仍旧，连续保存回写旧值 | 同一Editor完整回填与重基线，外writer连续Save及原绑定/身份过；活动VM同步/列数仍按原控件权威，不扩称全部AppSettings并发意图已修 |
| AP-060 | P1/自有WPF独立及主仓有限域通过 | 窗体字段保存后窗口未应用，原WindowClosing以旧实际尺寸覆盖；无关保存与单轴编辑需要区分意图 | 明确输入轴与自上次保存后的实际变化分开，失败不消费，成功不把未编辑轴缩回；实际原Close唯一owner及新Store重载过，最大化/RestoreBounds仍未验 |
| AP-061 | P2/自有WPF独立及主仓有限域通过 | 旧六尺寸控件不足44；候选390×640授权串号JSON撑高，ScrollToEnd仍读不到末字，green2保留真实单项红 | 原面板随实际宽度单/双列、局部44及原JSON120高内部滚动；同夹具最终16/3066检查过。非外部物理GUI/DPI/全部页面或实机验收，原生浏览和遗留恢复仍未执行 |

## 接口启动一致性

| ID | 级别/状态 | 事实与边界 | 处理/后续 |
|---|---|---|---|
| AP-062 | P1/同Runtime公开API候选及主仓限域回归通过 | [A09b1](A09_API_STARTUP.md)原225通过，新增10为3PASS/7PRODUCT失败；补启失败、配置不匹配和预取消会停止既有网关/device，并发启停还取得不可串行的部分网关状态 | 同生命周期锁协调并仅回滚本次具体新监听；同夹具独立及主仓各235通过，后置实际HTTP/端口/重试、原错误和预取消token过。原红及77Task保持，绿104Task全终态；不扩称中途取消/所有交错/释放故障/GUI/实机或完整A09通过 |
| AP-063 | P1/自有WPF及本机接口独立/主仓限域通过 | 只有设备接口且无选择时全停禁用，全部目标已有接口仍能重复启用；批次A成功/B1003失败后A地址陈旧 | 原VM谓词与四操作finally刷新实际端点/数量，保留本地化错误和A成功，不新增重复入口或回滚A；[同夹具对照](A09_API_WORKSPACE.md)及真实HTTP/目标/重试过，非实机验收 |
| AP-064 | P1/自有WPF及本机接口独立/主仓限域通过 | 切设备协议目标仍保留旧结果；五种实际在途请求变化后旧nonce回填，响应缺少来源 | 冻结Runtime/协议/所属端口/设备serial及输入revision，变化立即失效、完成前再核；同端点刷新及全局网关无关设备选择保合法结果。只证明已测五交错，不扩为设备目标受控在途或外部同端口重启全部情况 |
| AP-065 | P2/自有WPF六尺寸独立/主仓限域通过 | 接口页原固定区导致窄屏表格不足、动作小于44、长文本和字段无法完整读取，名称混淆全停/设备地址职责 | 保原五按钮/两下拉/三输入/六列，双区重排、局部44、有限内滚和全文提示；六尺寸/原绑定身份及1385检查通过。图仅自有Shell，物理GUI/DPI/触控及原生复制继续未验 |
| AP-066 | P2/接口验收夹具修正，非产品缺陷 | r01把下拉隐藏模板框计入三自有输入；r02未建立长错误前置且强要求本地化错误串号；首候选检查文字前未带入外层 | 分别盘点模板与原输入身份、先Require真实长文/保原错误码合同、补原框BringIntoView；原字符/裁剪/业务断言不删。同最终r04旧红/候选/主仓完成，原全部前置/错误判据/失败报告保留，不追认旧轮通过 |

## 修复记录规则

每个问题记录红测命令/错误、最小diff、绿测/独立复验、未验证范围和commit。原失败轮次保留本机artifacts。最多3轮review-fix；未解决需明确风险，不绕过断言或缩小用户目标。不得仅将控件禁用后称功能正常。
