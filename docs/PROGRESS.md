# 当前目标阶段记录

## 2026-10-16 / A14a1版本化脚本参数绑定

- Goal/Decision：修复“编辑参数已保存、步骤仍执行原literal”，显式executionContract.version=1及whole-field绑定，局部JSON保留null；原Scheduler前编译私有快照，VM在metadata/准备前完整验证，legacy保留。范围见[规格与原件](codex/apple-experience/A14_SCRIPT_PARAMETER_BINDINGS.md)。
- Red：Core原169保持、新24为1绿23红（含新API缺失）；旧诊断14过但AP042真实146→146→146/owner缺3/残留111，后继NOT-RUN，未重跑。另立严格无窗VM业务原4例2PRODUCT红2PASS，同Product断言不削弱。
- Verification：作者/独立/主仓均193/193及VM4通过，独立三入口拒绝/PDB36/156输入核同。主仓77400 exit0/73.440秒、193/51.24秒，VM45824 exit0/506ms；27原守卫保持/App为批准变更，原24Smoke不变，全部进程/双流终态、无timeout/清理错。
- Evidence：独立execution733890447E661B497B6EC14A5355D0BEE231916D93513066FD6437955B5C5541；主仓execution55EF318F027DC4FCCD08BD1CFBE342844E90B3341E6DB5F33AE989FBAB00A888，VMsummaryFA77BE5372B96ED3FCE8A0C701F7D78489825FC175F0F07EC1E6D798F8619AE1，DLL44B290489CF93E927A5D04732B13E4D736FFD0FCB91829123B1239DC7515AAB6。样例两JSON只另验打包entry/字节/语法，不另称guest样例通过。
- Next/Risks：核包/精确index独审后单功能推原私有feature，实际Git为准；不合并发布。AP042当前PRODUCT-FAIL、AP045官方输入分支停止、全部GUI/真实双实例/当前应用页安装卸载/最终30分钟及完整RPA作业身份/MCP/抓包仍开放，goal active。以下AP040/AP042历史通过不覆盖当前失败。

## 2026-10-16 / AP040应用反馈与当前实机前置

- Goal/Decision：不把“读取后安装完成0”误判为APK安装失败；仅在摘要存在非零CompletedUnverified时保留原安装段。App唯一同步hook/new纯合同夹具直接核原函数，旧业务不动；[规格](codex/apple-experience/AP040_APPLICATION_FEEDBACK.md)。
- Verification：真实红4PRODUCT/2PASS，原169/14/AP042过；实现绿、独立及主仓新6均通过，原3E/23旧Smoke保持。root Build17600 exit0/77.126秒、169/51.69秒，mode35036 exit0/2.651秒；context/input保持，原8条预期报告错误stderr9842B保留，全部进程/流终态、无timeout。
- Evidence：root execution EF2EADDA047A9DCDE0BC8289D3E47D4326A97DFB340B8636A5912F3B059CD080，6例summary C1BFA006D13AC462EE7C8E837B9065F99CBF8C124D4FD659A741887704F2140B；DLL61679572DF6C3D3242AE5FEA744B6EEE827ED580EC6645AB193D21CCD6404AA9。仅限摘要修复，待正式文档/核包/index独审再单独推私有feature，不预写成功。
- Live/GUI：已交付9117版本的[当前只读17步](codex/apple-experience/A07_CMFA_APPLICATIONS.md)通过，1 Ready/transport8、2关闭，CMFA未观察到安装/保留，无写动作；不替代安装或双实例。AP045显式激活+200正文后唯一click仍几何错误，效果未知；精确owned清理、停GUI分支，见[记录](codex/apple-experience/AP043_SKY_AB.md)。
- Next/Risks：AP044已实际推9117fd2，不重做；原全部GUI/安装卸载/双实例/最终30分钟及A08-A16未完成。下一阶段优先A14a明确版本化参数绑定与共享批次任务，保留旧ZIP，不猜Probability或执行任意脚本，最终GUI阻断仍单列，goal active。

## 2026-10-16 / A08a脚本包格式边界

- Goal/Context：AP042仅正文源码改善已实际推送cf79fe3，远端同值、test49e4710不变；AP043导航缺口仍开放。本阶段从2ca独立树修复自动化picker宣称JSON而实际只接受ZIP的矛盾，不扩展脚本语言。
- Decision/Files：仅ScriptPackageService的ZIP构造InvalidDataException转中文400/1007、ScriptsView移除支持格式中的.json、Program新增11项及[规格](codex/apple-experience/A08_SCRIPT_FORMAT.md)；I/O、Entries/解压、内层JSON、验证和限额不改。
- Verification：r01夹具编译9错保留，不作产品红；仅3处显式构造修正后r02为163/169，恰六非法格式契约失败。实现169/72.044秒，独立169/77.523秒；主仓Build63972 exit0/76.764秒、169项51.86秒，0警告错误，三源码raw前后同，七guards无差，全部进程/双流终态且无timeout。
- Evidence/Next：root execution C53F38CDDBEEB5284B5E90CCF873478C2AA35C0573874DA28399E8C9A4182173，DLL D123D120C8235431A2330EB43984B319EA839D8B0471879F24A6141489CE00A5；原件ignored artifacts/script-format-main/main-r01。待文档/完整核包/精确index独审后单功能推私有feature，实际Git为准；GUI选择器、A08其它页、全RPA/MCP/抓包及原真实双实例/应用GUI/最终30分钟保持，goal active。

## 2026-10-16 / AP-043受控对照与导航失败

- Goal/Decision：按独审计划在同一冻结共同目录顺序替换Old/New两DLL/PDB，各一次官方纯文本首读；不Build、不改生产、不触guest。详见[受控对照](codex/apple-experience/AP043_SKY_AB.md)。
- Verification：Old48个总节点止空Tab，New200个总节点包含真实设备正文；实际CLR/WPF两模块一致，原Release46/共同44/七guards全同。仅证初始读取改善，不证明全历史环境或完整GUI。
- Failure/Cleanup：New应用导航官方click明确isError，无法取得输入坐标，无freshstate；停止且不重试。两个精确自有进程externalKill，session86146/11102父0/child-1、流完整、无timeout/清理错，不称正常Close；原整树null仍保留。
- Next/Risks：AP042整体验收未获批准；独审允许考虑仅正文可达的开发分支源码交付，仍须文档/核包/index审核与实际Git回执，不合并发布。转独立A08脚本格式小目标实际红绿；原全GUI/双实例同步/当前GUI安装卸载/最终30分钟与A08-A16均保持，goal active。

## 2026-10-16 / AP-042正文无障碍候选，外部验收未通过

- Goal/Context：旧2ca79ce已交付；官方Sky纯文本曾只读到无头Tab根，先用真实owner红测确认正文断链，不与AP033或截图超时合并根因。
- Decision/Files：Main仅替换创建类型，新WorkspaceTabControl复用原Style、标准视觉树peer+Pane，不造节点或手动清缓存；新WorkspaceAccessibilitySmoke及App安全mode单hook，旧22与原几何/业务保持。规格和原件索引见[AP-042](codex/apple-experience/AP042_WORKSPACE_ACCESSIBILITY.md)。
- Verification：独立红三阶段同根仅1节点、5可视owner缺失；独立绿与主仓各158及旧14/3E通过，同根146→71→146、5owner唯一可达/状态样式正确、无旧页残留。主仓Build23328/73.585秒、mode19844/2.281秒，全流终态，152/148/旧22/PDB30守卫保持。
- External/Decision：fresh官方Sky首读accessibility:null，已核false/true请求且无工具显式错误；未导航或重试，精确owned清理后child-1/父session61577 exit0，不冒称正常Close。独审暂拒按已修复交付，不核包/暂存/commit/push，保留四源候选；已交付仍2ca79ce。
- Next/Risks：先新规格的旧E729/新F18受控单次A/B，旧也null就停；共同依赖和比较范围需冻结，不以旧失败反复重跑凑绿。全GUI、真实双实例/应用GUI/最终30分钟、AP041/S01/11NOTRUN及A08-A16继续，goal active。

## 2026-10-16 / AP-041验收报告出口稳健性

- Goal/Context：诊断5263587已推原私有feature；原s01报告二次失败仍待修，先修同一真实writer，不改原指针或生产输入。
- Decision/Files：原路径/payload factory、stderr及异常归并保留；仅报告本地clone选项加double写转换器，有限原JSON不变，非有限明确null/finitefalse/kind；新增14类纯契约和App安全mode唯一首hook，全局仍严格。参见[规格及实测](codex/apple-experience/AP041_REPORT_RELIABILITY.md)。
- Verification：独立红14=6PASS8FAIL，8红实际严格double错误；独立绿及主仓均原158和原14全过、0警告错误。主仓Build82636 exit0/72.112秒、mode41336 exit0/2.082秒；8EXPECTED stderr9842B完整、0UNEXPECTED。原3E几何/6快照/22Task/cleanup保持，全部child/raw流终态/owned0。
- Evidence：主仓summary4E7EC02F...7E7FA、execution01DDF9A1...A856B、末afterFF153C17...CA2C8、DLL E7293521...CF0A9；150输入/147原件/旧20/PDB28与独立已执行canonical全文同，配置和两PNG保持。准备稿的解析/行尾假设错误未运行产品，原件记录保留。
- Next/Risks：仅报告子项通过，独立终态已过，待文档/完整核包/精确index审核后单独推私有feature；不合并发布，实际交付以Git/回执为准。继续原S01/11NOTRUN、AP019/020/033、真实GUI/双实例同步和当前安装卸载/最终30分钟、A08-A16；goal仍active，不把新增日志测试当全功能验收。

## 2026-10-16 / AP-041受控离线几何诊断

- Goal/Context：A07c/d已实际推送a23348b，原s01首设备指针前置及报告保存失败保留；先证明无图/几何链，不改缓存、原负例或生产输入。
- Decision/Files：新增独立严格Demo/fresh目录诊断入口和OfflineGeometryDiagnostics；新mode禁截图、激活/指针/guest，仅原合法发布MainA/CurrentB与row-only同代/增代Offline对照。r1仅静态准备，r2补实际窗口尺寸并删截图能力，无新增等待/重试；21旧Smoke及147其它源保持。
- Verification：独立d01原158/51.42秒及三参数拒绝、新390诊断过；root-r01 Build22880 exit0/72.003秒（实际20.09/158的51.12），diag40764 exit0/1.955秒，0警告错误。两报告同3E0F0766...D1DACE，各实际390×640/27记录检查/6同步快照/22Task全完成/cleanuptrue/errors0；同代撤图Visible且旧公式NaN、增代保图有限映射的受控假设成立，不推旧s01具体字段。
- Evidence：独立审7A903AF9...C30B10、root独审80C101EF...B5FD3/executionACF6A83E...5AE14/末9F49C71A...70561、DLL95F0FF99...CFC21；实际PDB27、5次root149/canonical全文同已执行d01、147原raw/21旧/config/两PNG同。所有child/raw流终态，无timeoutkill/owned残留；三拒绝的预期stderr完整保留。见[诊断规格及记录](codex/apple-experience/AP041_OFFLINE_DIAGNOSTICS.md)。
- Next/Risks：终态独审仅准限域诊断源码交付，待完整核包/精确暂存独审再推原私有feature；不合并发布，实际提交读Git/回执。下一小修评估原offline负例不对无图重新定位及原报告局部非有限编码，保留全部原Down/Up/零副作用断言；source-generation撤图仅静态候选。不关闭AP019/020/033、旧S01/11NOTRUN、实机双实例/当前GUI安装卸载/最终30分钟及A08-A16，goal active。

## 2026-10-16 / 应用正文有限域验收与独立入口缺口

- Goal/Context：解AP037/038/039并保留所有旧失败；不继续猜测修改生产，不把编译或源文件数量当运行覆盖。AppView局部响应式及共享字符串Tooltip Wrap保留，业务tail/API/350ms/身份门禁不变。
- Decision/Files：d01证实七点Ideal/Display差异，d02证实历史633与当前505域不同且1当前字号前提未满足；只修测试测量mode、首次本地字形注入与当前完整域判据。冻结四源App2C9853/Theme567766/MainA752/fixture7248精确导入主仓。
- Verification：独立c02及主仓r01各158/0警告错误、新12/2890/12清理/111后端Task终态，原17源Smoke调用链及12页过。root Build15420 exit0/71.867秒（实际19.92秒+158的51.12秒），Smoke41364 exit0/164.976秒；当前505字体成员含344隐藏全1.5，diag均无错/不变，133图核，root直接实读原JSON及目视390图，非真实DPI/字体绑定验收。
- Evidence：root报告5F864A7E...AF3B7F、独审3EA80FE0...3EE1BE、末快照D2451427...6793FA，DLL4058AD1A...AB1757；5次148/144原raw/20旧源/API/Main/PDB24与配置E4E33保持，所有流与进程终态/owned0。stderr15945B及外层nodes/colors0保留，不宣称零诊断或全功能通过。
- Next/Risks：20是冻结Smoke源数，另三独立入口由s01补验；6壳层格各135过，但首设备offline指针前置和报告序列化双失败，后11格NOT-RUN，AP041独立跟踪，不能归咎环境或与AP019/020/033合并。独审只批准原私有开发分支有限域交付，核包/暂存/提交以实际记录为准，不合并发布。下一步安全诊断Source/几何/非有限值和原失败报告，AP040反馈文案、实机双实例/安装卸载/最终30分钟及A08-A16继续，goal active。

## 2026-10-16 / A07d提示字形测量诊断及限域修正

- Goal/Context：上一turn的A07c三轮有实证进展，继续保持完整目标；先解AP-038而非对产品做第四次猜测修补。新app-tooltip-verifier保留原树及f03所有证据，原147基线、20旧源与原158名序冻结。
- Decision/Files：只增只读诊断，d01保留原断言；143诊断/0error、全程状态保持，7个版本图完整可见，实际Display高17/原Ideal高30.48。两独审和root判A仅七点成立。随后只在隔离fix树改ReadTooltip测量为实际mode的9参构造及两条诊断描述，Culture/宽/font/DPI/原高度predicate/+1保持，生产未再改。
- Verification：d01原12=5PASS7红/旧20NOT-RUN不回填。corrected-c01 Build83584 exit0/64.301秒、158/51.46秒/0警告错误；Smoke4872 exit1/23.187秒，新12=11PASS、12清理、2887检查/111后端List Task终态。常规六尺寸与三表首中末全数据列阅读过，232诊断/error0/状态保持、133图核；唯一glyph最终Fonts合取红未归因，其后/旧20/12页NOT-RUN。
- Evidence：d01报告6447387D...1C5B3/独审006892AE...061B67；c01报告90A45356...0EDBE/独审6DA67566...405E9/末快照DB6B5887...D602B，148+其余144/20旧/PDB24/主配置与原流/进程保持，原件均冻结。详见[A07_TOOLTIP_DIAGNOSTICS](codex/apple-experience/A07_TOOLTIP_DIAGNOSTICS.md)。
- Next/Risks：AP-039只读观测Count/不匹配Key/当前Page归属/字体值来源和倍率，先判断再改；不从36个18号文本样本认全部150%，不先过滤Fonts或施加字体取绿。当前独审仍拒绝交付，主仓仍1de5136、未导入/Build/核包/暂存/push候选。真实GUI、双实例同步/安装卸载、最终30分钟和A08-A16仍全部必交付，goal active，无待用户问题。

## 2026-10-16 / A07c应用正文候选三轮验收未过

- Goal/Context：承接已推1de5136，仅隔离验证应用正文六尺寸、原表阅读/选择与批次反馈；保留20旧Smoke、API/350ms和全部业务门禁。原b01为11例10记录红/1绿，截图证实390宽裁剪，不能把未知Tooltip组合断言都归因产品。
- Files/Decision：fix树ApplicationsView重排同三表、44命中/焦点/长值，第三轮仅增共享ToolTip字符串数据模板以修实证NoWrap裁剪；tests树新ResponsiveApplicationsSmoke及Main唯一首hook。r3纠正无Content选择列的隐藏glyph误读，原hit/绑定及实际字段断言保留。未导入主仓生产。
- Verification：f01启动MultiBinding强转异常已保留，单行改原BindingBase后f02有4PASS/8报红；f03原158/0警告错误，新12例5PASS7报红、2033检查/111后端List Task终态/12清理。六尺寸geometry/targets/tasks通过，真实取消、失败恢复、状态/焦点及84/96表头选择通过；应用版本Tip高度测量仍红，其后/完整150%/旧20和全12页NOT-RUN。
- Evidence：f03报告1EE72CC5...A806、独审C54D18C6...9B8C、末快照08839E65...2F1D；148输入/其余144/20旧/API/Main全文/PDB24/配置E4E33保持，进程和四流终态。源码/PDB/DLL及三轮失败保留，细节见[A07_APPLICATION_RESPONSIVE](codex/apple-experience/A07_APPLICATION_RESPONSIVE.md)。
- Next/Risks：独审拒绝，不做本阶段第四轮；不替换主程序/不核包/不提交push未验候选。主仓HEAD仍1de5136。下一独立诊断保留原断言，记录实际排版mode/测量/clip及自有Tooltip图像，先区分测量差异与真实裁剪。真实GUI、双实例同步/安装卸载、最终30分钟和A08-A16不取消，完整goal active，无待用户问题。

## 2026-10-16 / A07b2接口排空后恢复应用读取

- Goal/Context：承接ebda58d及A07b1保留的API释放缺口；原Tick保留pending但API计数归零无通知。先在三个隔离树定点静审/冻结，原生产真实loopback HTTP与原350ms勾选路径取红，不触真实设备或共享ADB。
- Baseline：r1a原12例8 PRODUCT-FAIL/4 PASS、12清理，真实计数归零5秒仍零List；原158通过。List失败例尚未进入首List及后续新意图，旧套/12页NOT-RUN。原件保持；本地审计首次空属性错误仅补守卫后审同证据，未重跑产品。
- Files/Decision：生产仅ApiServiceManager/ApplicationsView 32+/2-，归零提示逐observer隔离，页面排原350ms且执行时重核source/runtime/关闭；全部门禁/全局活动定义不变。新fixture及Main唯一首hook保留19旧原件；r2原12路径加两个新事件合同，不能倒填原基线14例。详见[A07_API_DRAIN](codex/apple-experience/A07_API_DRAIN.md)。
- Verification：独立Build总69.458秒，实际构建17.84秒/158项50.85秒；完整1000 Smoke148.325秒。主仓Build总72.155秒，实际构建20.24秒/158项51.08秒；完整Smoke144.498秒。均0警告错误，各14 PASS/496检查/100任务记录终态/16真实HTTP门/14清理，全部旧套及12页到达通过，原AP-033成因不因本次绿关闭。
- Evidence：主仓报告352AB302...875B、独审1916917D...3C6A、DLL6947B826...A509；五次147输入、原143及19旧raw、PDB23、与独立147规范化全文/canonical、配置E4E33保持。异常observer后正常observer/落盘marker各1；真实唯一Pending操作在close请求后执行Completed且零List。无timeout/kill/清理错/owned残留；stderr15948字节旧已捕获诊断保留，不称空流。
- Next/Risks：本功能独审后限域核包并单独push原私有feature，排除APK/配置/依赖/本轮raw及两PNG，不合并发布；具体commit/包摘要以Git及本机receipt为准。日志observer自身抛错未测，公开Demo不支持runtime热换故仅静审其guard。当前应用GUI/正文六尺寸、真实双实例同步、最终双实例30分钟与A08-A16全部继续，完整goal active。

## 2026-10-16 / CMFA本轮owned停止卸载实测

- Goal/Context：承接已push的e5a44bd首验，原l01主页权限阻断FAIL保持；接受权限不是验证停止/卸载的前提。另立受限模式，原台账SHA与目标全部字段先验，现场版本/非系统路径与身份再证明后仅共享Stop/Uninstall，无重装、授权或GUI动作。
- Files/Decision：只扩展ApplicationPreflightAcceptance及Program五参入口，不改生产/原lifecycle断言/默认158；冻结两源AE8185...64B与E607C3...585F。源ledger保持原字节，新ledger写前记录请求可能性并关联源runId/SHA，失败不自动补偿。详见[A07_OWNED_CLEANUP](codex/apple-experience/A07_OWNED_CLEANUP.md)。
- Verification：独立构建22.64秒/158项51.49秒；主仓20.79秒/158项51.33秒，均0警告错误。158名称顺序、实际PDB三源、146输入/config/源ledger前后保持。错误SHA及目标不符反例均只在inputs拒绝、无host/guest轨迹；c01正式PID8408正常exit0/21.943秒、51步通过，Stop前4564/4590存在→之后0且安装保留→Uninstall后普通/-u全用户查询空、进程0、列表缺包。
- Evidence：formal summary59D458...840FC，独审B51435...53EA；本机工作区独立树cleanup-c01完整raw/7child终态，无超时kill/重试、自有进程0，23目录采样及ADB62248/native birth/transport8保持。新ledger UNINSTALL-VERIFIED/owned=false，原0D815...9AE6及原l01 FAIL不改；当前安装已移除，下载APK保留，专用1运行、2未启动。
- Next/Risks：当前ApplicationsView实际文件框/确认/进度/菜单、主页、双实例同步、最终双实例30分钟及A08-A16仍待；AP-003/AP-033/AP-036不关闭。原台账历史owned不能当当前状态，PM不是并发安装原子锁。当前Windows抓取失败不绕过；正式测试/报告/README和源码清单独审后单独push原私有feature，排除APK/配置/原始证据，不合并发布，实际提交以Git记录为准。

## 2026-10-16 / CMFA单实例共享安装首验

- Goal：使用用户新提供APK继续真实应用链，固定SHA/包/专用1目标；原实例0、共享ADB与账号数据不改，不授权VPN或敏感权限。新增正式只读预检与显式生命周期入口，默认158及生产代码不变。
- Context/Decision：现有ApplicationBatch安装为覆盖参数，先all-users含保留记录检查，再在实际Install调用处复核并持APK只读lease；只有精确版本/路径回读才owned，异常停止并留ledger，无自动清理。r2前台包边界误匹配经本地反例后仅两行收紧，r2原件保留。
- Verification：r3独立158/51.16秒、主仓158/51.43秒，0警告错误，PDB三源与146输入/config前后相同。r1实读17步骤通过，但外层空error字段StrictMode失败保留未重跑；l01一次安装/版本211035/2.11.35.Meta/路径核对及启动进程成立，首次权限页占前台，主页断言FAIL；Stop/Uninstall/截图/GUI均NOT-RUN。
- Evidence：详见[A07_CMFA_APPLICATIONS](codex/apple-experience/A07_CMFA_APPLICATIONS.md)。本机原件在独立树lifecycle-l01，5child终结、无超时kill、自有进程0，23目录采样/ADB/config保持。ledger owned=true、待人工复核，安装保留；不覆盖重装或自动补清理。
- Next/Risks：AP-003与AP-036开放，先基于ledger重核后另立受限停止/卸载或恢复可靠GUI观察后拒绝非必要权限；当前Windows抓取失败不绕过。双实例同步、当前应用GUI、最终双实例30分钟及A08-A16不取消。仅正式测试/报告/README与清单限域提交，排除APK、本机配置、依赖和原始证据，无merge/release；实际push以Git记录为准。

## 2026-10-16 / 用户提供CMFA包与当前版本只读接入

- AP-035已实际push`01e5c10a7f14949b1124597479ea0b1a3611753d`，独审13路径/311源/314ZIP/158节Bundle通过，未合并发布，test备份原值。
- 用户提供CMFA APK后独立官方AAPT2静态预检两次exit0：包com.github.metacubex.clash.meta、2.11.35.Meta/211035、min21/target35、仅ARM64，含VPN服务声明；未验签/安装/启动，不授权VPN或导入配置。它提供真实安装测试候选，不取消受控自有应用的后续需要。
- 当前版本CLI在fresh仅实例1配置实际只读接入成功，PID75236/exit0/0.596秒，Android14/Ready/transport8，stderr0；前后ADB owner62248/birth/协议及主配置保持。原始证据artifacts/apk-cmfa-live/r00，不上传。尚未查同包/ABI、未安装，不冒充新版应用页/双实例或最终30分钟验收。
- 下一步按HANDOFF继续同包保护与兼容性预检、当前应用页实测、AP-033输入源恢复；本补充为进行中日志，不属于已交付AP-035包或新的功能修复提交。

## 2026-10-16 / A13b数据库长路径修复

- Goal：继续全测，先解除可稳定复现的AP-035，不用短目录掩盖原数据路径故障。三隔离树分别实现、编写正式回归、独立验证；只改CatalogRepository原生主库与备份路径，事务/schema/原位置不变。
- Baseline：原150不变，新8冻结后原代码152/158、6条真实SQLite14。首f01仍失败，PDB证明运行旧编译输入；保留失败，仅更新自有源mtime触发现有构建，补丁/断言原字节不变。
- Verification：有效独立f02为158/158、51.43秒，主仓单次158/158、51.76秒，均0警告错误；PDB精确对应3冻结源，145输入前后同。主仓实际CLI四普通路径251/252/253/300均exit0、库102400字节且可独占重开，配置和二进制保持。详见[A13报告](codex/apple-experience/A13_SQLITE_PATH.md)。
- Next/Risks：本功能限域核包/独审后单独push原私有分支，不合并发布。AP-033原FAIL不因本项通过关闭；继续可靠输入前置、仅1/2真实同步及当前应用页自有包安装卸载，最终版本双实例30分钟仍必验。UNC/非Windows/完整GUI未由本轮覆盖，完整目标仍active。

## 2026-10-16 / 追加全测与RPA、MCP、抓包任务

- 用户更新完整目标，明确未验的双实例同步、安装卸载、长期稳定性及GUI失败必须继续。沿用R01-R10/A00-A12，追加R11-R14/A13-A16，P0至P3按用户顺序排列且全部必交付，不关闭原余项。
- 实读PRD/SPEC/TASKS/plan、脚本包与调度、代理/外部HTTP Toolkit辅助客户端；真实子代理完成能力盘点、结构及风险审查。当前ZIP脚本、HTTP和代理配置分别不等于任意RPA、产品MCP或抓包会话。
- 使用ai-coding-workflow与write-codex-plan的文档门禁，当前无superpowers；用可见subagents交叉审查。追加任务范围、依赖、效果验收、权限/取消清理和无环子阶段；终控台复用12页职责，不新增重复导航。
- 官方抓包/Android/MCP资料已调查，初步推荐自有调试应用显式代理的独立POC；HTTP Toolkit自动ADB/证书行为与默认边界冲突保留为条件备选。POC未运行，不安装CA/VPN/代理、不启动外部服务。
- 追加最近实机报告：当前21e构建150/51.43秒、0警告错误；实例1冷启动16.393秒及8项Live13.630秒通过。深路径Demo SQLite14及短路径AP-033均FAIL，后续未执行保留；ADB/原配置保持，实例1留运行、其余22业务状态未变，不能称完整GUI/双实例/30分钟通过。
- 同时继续A13b：当前CLI默认空授权、无雷电路径/API的离线初始化四例实际执行，db路径240/251字符均exit0，252/253均SQLite14且非零；journal恰到260字符触发边界。仅自有文件，原始stdout/stderr字节及PID/退出码保存在artifacts/p35a，原配置不变；不是该缺陷已修或GUI已通过。
- 本阶段仅文档及离线初始化复现，无业务源码修改。隔离树goal-expansion-20261016基于21e，官方managed worktree工具因父目录非仓库失败后采用精确git worktree add；原树和用户PNG未清理。独审/链接编号检查后限域提交推送，实际版本以Git记录为准。
- 下一步A13b优先恢复可靠GUI输入前置并验证真实双实例同步与自有测试包安装卸载；最终二进制双实例30分钟仍必验。新功能实施先冻结对应契约、先红后绿、每修复独审后单独push；完整目标继续。

## 2026-10-16 / A07b1应用自动读取恢复

- 原生产首次12例5PASS/7真实产品红/12清理：可用勾选建立待办，原350ms Tick在脚本/稳定Push内清掉pending，真实释放与VM通知后仍无List；原失败保留，其他旧套当轮NOT-RUN。
- 生产仅ApplicationsView7+/3-：阻塞保留意图、空选择撤销、关闭不调度；350ms及全部原门禁不变。正式新fixture与Main唯一首接线不改旧八套；基线4处警告仅显式discard/nullability修整，12路径/断言/等待不变。
- 独立150/51.27秒/0警告错误，新12/310/12清理及旧19/18/10/11/16/49、身份10、原应用和12页全过；完整Smoke126.368秒exit0。不是AP-033根因已修或实机通过。
- 主仓三冻源4260/DBDB/33C机械整合，144输入全文与独立版逐项同，其余141保留root原raw/旧八套不动；代表Build63.432秒exit0、150/51.05秒，完整Smoke118.134秒exit0，同新12/310/12清理与旧七套/原应用/12页通过。摘要94F9724C...ED6AE，DLL4BC9D06A...32A62。
- 两实际会话29660/45047和root38202均终结，配置E4E33不变、无timeout/kill/残留。报告/README/矩阵/任务与源码清单按独立限域交付后单功能push原私有feature，实际版本以Git记录为准。
- API释放唤醒另项A07b2、应用小屏正文、原生保存、后续页、真实1/2及30分钟继续，完整goal active。详见[A07报告](codex/apple-experience/A07_APPLICATION_PENDING.md)。

## 2026-10-16 / AP-033输入诊断

- 上一轮A07a已实际push ab11ec9，feature一致、test49e4710不变；本轮不重复旧交付。
- 新两树基于ab11，单旧测试49行只读新增，剥离后原266行完整恢复；源70DF/patch0315双静审，原定位/同步/循环/等待/断言未改，无生产或权限变更。
- 独立p01实际150/54.33秒、0警告错误；完整GUI33.665秒exit1，旧19/18过、截屏9PASS1前置FAIL/10清理，其后及A07/旧应用/12页NOT-RUN。无timeout/kill/残留，127源及配置不变。
- 三个zoom0.1快照均ActiveSource/Captured/DirectlyOver=null、目标源/HWND有效、WPF屏幕零点；after-set OS精确命中目标Image且直接转换正确，之后OS移动。确认本轮输入前置状态，仍不知成因，不修改产品坐标或盲重试，AP-033开放。
- 根单源机械导入同70DF，143编译输入=127+12tests含lock+4根配置，59raw同/84仅换行；首次142计数预检拒绝在Copy前，原件保留。root本轮未Build/GUI，不把独立150写成主仓新执行。限域诊断增强经独审后交付，版本以Git为准。
- 下一步A07b优先受控复现脚本/稳定传输跨真实timer后丢pending，API无释放通知与应用正文另项；这些仍是未运行候选。原生保存、全部后续页、实机和30分钟仍开放，完整目标active。

## 2026-10-16 / A07a限域修复与交付

- 原生产三条同身份改名路径真实失败；仅ApplicationsView/ApplicationRows区分展示名与连接身份，保留原行/勾选及在线、运行时、保护和活动门禁。
- 独立f02正式150/51.58秒、0警告错误，新应用10/114检查/10清理，旧19/18/10/11/16/49、原应用smoke与12页完整通过。f01旧截屏坐标前置失败仍保留；测试r0b仅移动新增hook回自然应用阶段，未改旧七套或新10断言，不称鼠标根因已修。
- 主仓四源与冻结版raw相同，127源与独立版规范化文本逐项相同；root-r02正式150/54.18秒通过，但完整GUI在Main346旧截屏前置失败，新hook381尚未执行，后续NOT-RUN。无timeout/kill/残留，实际配置E4E33不变。
- 非实现者批准应用改名修复的离线限域交付，AP-033输入前置可靠性另项开放；不把主仓FAIL或外层colors/nodes为0改写为通过。报告、README和源码清单随本功能提交原私有feature，实际版本以Git及本机交付记录为准。
- 原生保存、应用pending/小屏正文、A08-A12、真实1/2和最终30分钟仍待；完整目标active。下一步先按AP-033只读诊断补证，不盲重跑或弱化旧断言。

## 2026-10-16 / A07a基线

- 读取当前计划/任务、真实应用页批次和旧验收，不使用旧VM应用快捷入口；三新隔离树均1acbf23。
- 正式10例/两源r0a双审，保留旧六及原应用smoke。修正确认前置异常可能被生产错误呈现吞掉的误绿风险，非允许写入口统一留证并拒绝。
- 首次独立150/51.54秒/0警告错误；1000新专项3红7绿/10清理，已复现同身份仅改名的名称陈旧、启停/确认和在途读取误拒绝。原失败和分阶段NOT-RUN保留，旧套未到不算通过。
- root实读实际10例、后端/单元格/确认同步/原Task、原始Build输出和进程记录；无timeout/kill，127源/旧七套/配置不变，owned0。
- 首轮生产仅两文件隔离实现进行中，下一步非实现者绿测、主仓代表和单独push。原生保存、应用pending/正文响应式、真实1/2与最终30分钟仍开放；完整目标active。

## 2026-10-15 / A00-A01

- 目标：保持用户完整范围，先形成可执行计划并从现有真实状态出发，不把上轮149项和Demo当全功能完成。
- 读取上下文：README、根旧计划、list-console报告及最终构建/三尺寸JSON、参考PRD、旧测试/缺陷、WPF主题/页面。未访问CODEBRAIN。
- 决策：保留12页Tag和接口；显示名短中文；WPF按390/768等逻辑窗口尺寸适配，不虚称原生手机客户端；只在独立测试实例执行已界定安全操作。
- 独立发现：主窗口MinWidth=1000；节点/截屏/应用/设置/监控固定栏；历史实机应用测试没走当前批次UI；主控A/当前B投屏为P0候选需先复现。
- 验证：读取主仓149/149（51.38秒）及三尺寸12页、zh-CN、error空；Desktop摘要与最终独立证据一致。这里只证明上轮列表精简基线。
- 下一步：计划独立复核；保存该基线；先修P0目标候选，再按任务表推进全页设计与真实路径。每个功能验证后push至既有私有功能分支，不合并发布。
- 风险：ROM/私有agent、原生外部ADB投屏等受限能力必须保留缺口；不会自动接受第三方协议或重启共享服务。

前一目标轮判定：已完成列表精简、正式/界面独立验证和源码清单，属于具体进展；本目标首轮重新审计产生新缺口和下一行动，不是状态重复。当前完整目标尚未完成。

## 2026-10-15 / 计划审查与保存

- 已提交并push列表精简基线8303cdca8f463d995b206815883aba72fbe5f75e至既有私有功能分支；远端HEAD一致，test备份49e4710不变。
- 两名只读代理完成计划独立审查。已补四处最小尺寸契约、详情隐藏过滤/重复选择、对比度4.5:1/3:1、44逻辑触控命中、字体与真实DPI分开，以及自有APK来源/签名/安装前不存在证明。
- 当前逐控件目录：1976条原始记录→356个去重可见键→103个页面控件键，另6类全局和15类源码补充；仅观察记录，不当执行。功能矩阵覆盖12页、业务路径、异常、隔离与最终稳定性；原始证据不上传。
- A02已在独立apple-main-target工作区开始零外部进程红测；目标是修复MainDevice/CurrentDevice误用，不启动外部ADB或操作模拟器。独立UI审查列出单源与同步目标解耦、快照与进程证明等负例。
- 当前计划已具备实施入口；完整功能、视觉和六尺寸验收仍未完成。用户现有配置和正在使用的窗口不被强制修改或关闭。

## 2026-10-15 / A02主控目标修复

- 目标/根因：主控A但当前B时，旧NativeView误用B；零外部进程红测实际失败。
- 改动：仅NativeView、MainWindow和新NativeTargetSmoke；投屏明确MainDevice并冻结runtime/代次/transport/输入代次，确认与等待后重验，进程固定PID/birth/path；不与群控同步耦合。
- 验证：主仓149/149（51.17秒），独立verifier149/149（51.60秒）+三尺寸12页/25守卫；主仓最终1000×700亦通过。源文件逐SHA相同；实际确认/外部启动/嵌入未调用，见A02报告边界。
- A01补审：纠正矩阵中不存在的终端/文件/接口/脚本入口，当前工具页不是设备备份；5个浏览绑定分别验收。未发现Android平台/构建工具或自有APK，登记AP-008，不操作抖音代替。
- 下一步：本修复独立push；A03共享主题/短中文名称与可访问标识，之后A04四尺寸契约和逐页重排。目标整体仍未完成。
- 风险/保护：正常配置SHA仍E4E33A8...BB05F0，未改授权或共享ADB、未操作模拟器；保留历史失败日志和原有英文截图。

## 2026-10-15 / A03共享外观与中文命名

- 目标/决策：沿原WPF资源键统一12页名、浅色主题和可访问标识；保留Tag/页序/功能/尺寸；不引入新框架或字体。
- 改动：19个Desktop源/验收文件，具体表见A03_APPEARANCE；显式应用Window主题修复字体未生效；接口按钮不再误称停止设备；120ms局部反馈尊重系统偏好。
- 失败与调整：名称红测、未中文“1:1”、派生窗口样式和新动效绑定错误均保留日志；root目视拒绝旧final-*空表分页截图，新增有界idle/render等待、实际行视口断言和每页captures；独立要求补实际Popup/Toggle实例检查。
- 验证：主仓149/149（51.52秒）；独立149/149（52.32秒）和三尺寸每轮195外观/25守卫/12 idle页，目视设备4行/连接2；主仓最终1000亦同，tab0与最终图SHA一致。正文/边界最小对比4.65844/3.42862，OS偏好仅应用分支模拟，未改系统。
- A01：逐字段/菜单/模态细表已补，五个浏览独立；不把目录和属性可见当业务通过。
- 只读环境检查：23实例，仅5运行，专用1/2关闭；5037雷电ADB身份记录于本机readiness，真实动作前需重新核验。未启动或操作任何实例。
- 下一步：本功能单独push后开始A04公共响应式：最低外窗390×640、四契约统一、单一导航抽屉、Field重排/Owner-fit。A05至A12仍未完成，节点Demo空树不代表实机通过。

## 2026-10-15 / A04实施中

- 基线：08c4f8c已推送；实现与verifier各建独立worktree，不改用户配置或设备。四处尺寸契约红测真实失败149/150，390×640被Normalize放大；契约修复后新用例通过，阶段总验收尚未完成。
- 范围：本轮六尺寸probe只证明共享壳层/表单/自有弹窗；三尺寸完整smoke保留业务断言，不能通过削弱四行可见性等旧检查，把固定页面冒称为手机适配完成。
- 审查：Owner-fit的主屏工作区误用、统一降低MinHeight和非有限几何处理正在修正；原生文件选择器、MessageBox和真实DPI不属于此helper的完成声明。
- A05只读确认：详情筛选确实写全局StatusFilter并使设备列表、监控目录丢项/停流，关闭详情不会清除。该缺陷独立排入下一阶段，不与本次尺寸修改混修。

## 2026-10-15 / A04独立验收与保存

- 14源文件整合；尺寸四契约、导航/Field/Toolbar/指定弹窗完成本阶段公共适配。独立150/150（51.90秒）、六尺寸各135、三桌面各209外观/25守卫/12idle通过；主仓150/150（51.41秒）、390shell和1000完整通过，DLL79E3F633E6594DF811D5F6DB563A849CC534BA9569E470128A09B7CF515771BF。
- 不抹失败：独立首轮1440旧Toggle动效断言FAIL，单次同二进制r2通过仍被拒作全绿；仅补测试焦点前置/真实帧/阶段JSON后重建并完整独立复验，生产主题与动效条件未改，原失败根因未确诊。modal透明取证旧图同样保留，不再当视觉通过证据。
- 目视：390壳层/抽屉及真实模态背景正文可读；三桌面四行/操作可见、分页首图与最终图SHA相同。主仓14源内容归一化相同，两新文件行尾差异单独核验。
- 边界：AP-011首次Fit失败订阅、AP-012页脚触屏完整读取仍开放A10；固定页面正文、真实DPI/副屏、实机和30分钟仍未完成。未改用户配置或任何模拟器/ADB。
- 下一步：本功能独立push后，A05优先修已确认的详情隐藏筛选/重复选择与监控隔离，再处理设备/监控/主控窄屏正文。完整目标继续active，不能调用complete。

## 2026-10-15 / A05a开始

- 前一目标轮属于实际进展：A04提交2b1fe6e已push，远端HEAD匹配、test备份不变；不是状态重复。当前干净源码，原两张未跟踪英文图保留。
- 事实：AP-004是默认视图、状态属性及监控枚举耦合，不是仅文字重复；状态约束与验收已保存在A05_DEVICE_STATE。更小的关窗重置不能解决根因，升级为原计划内跨边界修复，不套用5文件以内minimal-fix。
- 并行边界：main_target_fix在独立apple-device-filter实现；hig_ui_inventory只读审查并准备独立verifier；full_test_inventory只读准备后续A05b尺寸/可视区风险，不并行改同文件。
- 当前尚无本阶段通过结论；先实际红测，再迁移原断言，最后独立150/三桌面/六公共shell及专项验证。未操作真实设备、用户配置或共享服务。

## 2026-10-15 / A05a独立验收

- 11个Desktop源/测试文件：删除详情第二套群控与隐藏全局筛选；设备局部状态视图和监控完整目录隔离，分类与显示共源，刷新不偷换高亮A为B；改名通过新数据库连接回读保留原GroupId。
- 先红4项目录变0项、green1中文夹具错误及green2赋值破坏绑定均保留日志；最终原断言迁移到真实列表控件，未改命令门禁/授权/真实设备。
- 实现150/150（52.04秒）；独立150/150（51.40秒）及三桌面每73专项（32分类/22实际协调）、209外观/25Native/12idle、六公共shell每135通过。主仓150/150（51.34秒），最终1000完整与390shell同样通过，root目视四行/状态及壳层。
- 主仓DLL DE075D83B35FA9DA5322AA0CD15731BC29E5F77F0E71D7FDD84E5FA76D4A8D48；11源归一化与实现/独立一致，新文件行尾差异明确记录。用户配置摘要未变，所有测试会话结束。
- 只读AP008依赖调查取得4个固定工件并核发布checksum/POM/归档；记录候选链，不执行/安装或接受SDK条款，不标记APK构建或Android效果通过。
- 下一步：A05a单独push，A05b继续设备/主控/监控正文响应式，补AP-013表头范围和AP-014过渡标签；其余A06-A12与真实双路/30分钟范围不缩小，完整目标仍active。

## 2026-10-15 / A05b操作含义开始

- 前一goal轮为实际进展：A05a提交15155a708802199d725d05523d02ceb8cb49f78a已push，远端匹配、test备份不变；非等待或无进展。当前源码干净，用户两张英文图保留。
- 本轮在原A05范围内先处理AP-013/014一个连贯文案修复，再分别处理设备/主控和监控正文响应式，每项独立验收push；未缩小完整目标。
- 读取PRD/SPEC/UI_SPEC/TASKS/HANDOFF及实际协调器/列表代码。推荐仅改显示和动态提示，保留同一命令/分类/授权/启动语义；先红测，最多5源文件。
- main_target_fix独立apple-device-labels实现并持Demo GUI锁；hig_ui_inventory只读审协调器语义；full_test_inventory只读补后续列表/主控实际边界验收，不并行写同文件。不接触用户profile、真实设备或共享服务。

## 2026-10-15 / A05b操作含义验收

- 5个Desktop源文件，仅动作标签、动态条件说明、表头范围及测试调用；状态分类/门禁/协调器/后台均未改。Connected+Device仍先调试，未知不承诺自动修复，提示准确描述条件启动/等待/可能失败。
- 红测保留：首次被旧Appearance恢复动效断言拦住，不记为本缺陷红测；前置专项后red2为19失败/17通过/11阶段。旧动效原因未知，登记AP-015，不借此改主题或循环重试取绿。
- 实现150/150（53.67秒），独立150/150（52.14秒）+三桌面各36新专项/11观察、原73隔离/32分类/22协调、209外观/25Native/12idle；390shell135。独立三轮首次全过，无重跑。主仓150/150（51.60秒），最终1000完整同样通过并目视。
- 主仓DLL2EF968DDC1C51C2171246928A50D2ECC693AF497C6D052EDB3190F7B3145395D，5源原始SHA与独立/实现一致；用户配置摘要未变，全部测试会话结束。未调用这些实际实例命令，不称真实授权或设备效果通过。
- 下一步：本项单独push后A05b1列表关键动作/44命中/主控窄屏展开返回/resize手势，再A05b2监控正文。AP-011/012/015和全部后续验收仍属完整目标，不调用complete。

## 2026-10-15 / A05b1开始

- AP-013/014提交6c4adce8f975fa9e0cfe1d71bb5fea9c63d1459e已push，远端一致、test备份不变；当前进入下一连贯功能，不把文案通过当布局完成。
- 已知边界：原操作列前397宽、cell padding20、32按钮/48行高与Wrap、主控300侧栏/固定正文Height、resize坐标失配。推荐同一组件重排和集中手势取消，非另建页面。
- 轻量spec/逐尺寸/真实完整bounds/滚动/目标与手势验收保存在A05_DEVICE_RESPONSIVE。专用六尺寸设备probe与三桌面旧完整smoke并存，不放宽其它正文的未完成状态；不操作真实设备或用户配置。

## 2026-10-15 / A05b1设备与主控验收

- 本轮有实际进展：7源冻结patch25CD4082425831E6803D6237C33088569ACF69DD49437986D451815E1F92C5E8；root/实现/独立原始源SHA一致。只改相关Desktop布局/手势与正式夹具，不改Core/Host/API、权限、用户profile或真实实例。
- 已修窄列表关键动作不可达、44命中与第三动作裁切、主控固定侧栏/正文末端、正常Up释放捕获丢目标、旧手势跨布局等问题；实际红另定位header resize grip、默认ElementStyle、旧恢复覆盖wheel及中间宽屏clamp消费V6，原条件不放宽。
- 实现最终150/150（51.13秒）；独立150/150（51.98秒）及六正文（两390各889、其它各847）、三完整桌面36/73/209/25/12idle、六shell135全部首轮通过。20入口负例拒绝，但collector误用UTF8不证明console中文编码。详细分类/观察与所有旧失败见A05_DEVICE_RESPONSIVE。
- 主仓最终150/150（51.10秒），首轮390640指针前置失败656checks/未Down；保留原证据，后验只读输入/clip检查不确定首败具体原因。等待会话idle5秒后以新目录一次复验，两390各889、1000完整、390shell135通过并目视；不改源/断言、不锁定用户输入，不称root全部首次成功。
- 最终主仓DLL D4F9CD5A3218D19189E2CD0D3E31152AF52095FE2F29EA8EF4C3CBE554FF03F3，profile仍E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0；全部必需exec/WPF已结束。正式报告/矩阵/README/清单随本次功能提交，原始图/日志、环境配置和依赖不上传。
- 下一步：独立push本阶段后A05b2监控正文，再A06-A12。默认Image内部gutter不适用，实际父留白/捕获拖出已验；非物理点击/Android/自动timer/wire/DPI/全部页面适配，完整目标仍active。A10保留AP011/012/015及未复现Tab/指针前置候选，不把候选当产品缺陷。

## 2026-10-15 / A05b2监控正文验收

- 范围：仅Monitor/App生产两源与新监控夹具、原设备夹具只读诊断。头部换行、真实视口卡宽、44动作、长文滚读、图表纯重绘；保留180视频高、原采样/会话/输入/授权规则，不改Core或共享服务。
- 实测红：固定宽/28高动作、仅滚动条条带被误判可见、随宽改高产生Auto滚条振荡；原始轮次均保留。freeze2/3独立另遇无分量上下文与焦点合取失败，只补有界观测，原条件/等待不变，AP019/020仍开放而非冒称修复；诊断交付摘要失配和runner未启动事故亦单独留痕。
- 最终四源patch BE261FA04A241BC3600EACD5993E0E667D30C8B8417314C8249B6628441285E1；root/实施/独立raw一致。独立freeze4正式150/150（51.16秒）、六监控各952/308、六设备两手机889/140其它847/139、三桌面各209/36/73/32/22/25/12idle、六shell135、42CLI全部首轮通过，64自有进程结束；批准仅离线布局/观测。最终汇总SHA60D2646C9F95CACFFA6068943D0B3184C2FC37FA1659016AE3F258462320501E。
- 主仓150/150（51.26秒）及fresh五代表首轮全过，DLL29EA3F8F7FFF131D5EF06DDACADD0C93365FB267672B4CA640E1624078A7FBAA；目视390墙/长文/空目录及1000设备。明确运行profile前后SHA仍E4E33A8...BB05F0；独立默认路径不存在的null比较不作用户配置证明，报告已纠正。
- 下一步：本功能单独push后A06a先受控TCS验证截图/节点ABA与目录代次迟到，再A06b窄屏界面检查/截屏。所有这些候选尚未执行，不拿Demo手载当自动首载；A07-A12及真实双路/30分钟范围不缩小，完整目标继续active。
- 打包审计边界：旧打包器仍含docs/verification历史PNG/JSON/txt；完整性校验不等于纯源码/无原始证据。口头绝对说法已纠正，AP-021留A12单独分类，不删除用户文件；本轮不新增这些历史材料、不上传本轮artifacts或ZIP。

## 2026-10-15 / A06a先红验证开始

- 上轮为实际进展：28cdd4ded947a4d303ecf39744925faaabb3a314已push，远端feature匹配、test备份49e4710不变；本轮从该基线独立展开，不重做监控布局。
- 已读PRD/SPEC/TASKS/交接及四条DebugImage发布链；两名只读审查同意区分源行缓存与检查发布、正常几何预处理与后续身份冻结。轻规格A06_DEBUG_PUBLICATION，AP-022目前仅源码候选，暂无运行红或修复通过结论。
- 三个新独立CLI worktree为apple-debug-context、-tests、-verifier；应用工作树工具因聊天根目录不是Git仓库不可用，已在实际核验的项目Git根用普通worktree，无重试创建同目录或清理旧工作。
- main仅加现Demo smoke调用一行，full只拥有新正式Desktop/STA夹具，VM生产尚未改；hig独立审规格。GUI/构建锁仍root空闲，必须明确转交才可跑。所有数据在artifacts，不触真实配置/模拟器/共享ADB；A06布局和实机仍未开始。
- r0已有实际红而非计划：实施树VM未改，正常6项A截图/节点/导出通过；Discover TCS期间切B，释放后真实Screenshot B/Dump B并改B检查，事件1→3，两case均清理完成。root直接读JSON确认；hig静审夹具有效。接入改为三段artifacts/desktop-smoke保留默认路径语义，未实际运行用户默认profile。现main已获仅VM四链修复+Build150+r0预检独占锁，full只补新fixture r1（不buildGUI），独立最终验收仍未做。
- 第一版已预检150/52.42s，r0两case全PASS后按原合同PARTIAL停止，B事件保持1→1，新B可恢复；root已读实际JSON/log，不能计全面绿。main释锁且VM-r1暂冻结，full仍只补完整r1；独立静审另发现A迟到异常可能按CurrentB记日志/错误，已授权同链错误反例，不扩大到无关全局错误处理。下一步完整baseline红与修复绿、独立矩阵仍未执行。
- 最新截图需求再次核对：8303cdc已移除全部分组、设备视图切换和重复启动/连接，表头统一选择、同步开关归主控、主控截图自动调度；root和独立只读审查均确认28c现行源码仍保持这些约束，不重复实施或删除原分组数据。此处是本轮源码复核，未冒称重新执行三尺寸旧验收。
- 新r1正式夹具已冻结46独立case：增量patch SHA9E171B2B5D96AB21DBA6646ED9F49798A4C25B90B535163B30AF99D3E9811E82，完整源SHA3B516F533F336FEC860CD727E6A2E7F456ED1D490C21D627FA38AE2FD46864DA；root实核，r0原件不变。独立verifier获独占锁，先在28c未改VM上运行完整聚合红；main仅机械导入同fixture并等待，不构建GUI，第一版VM仍冻结。46case静态完成不算执行覆盖。
- 独立完整r1先被长SQLite journal路径拦截，保留失败后仅改隔离短profile，同DLL完成46(10PASS/36FAIL)且全部清理。root实读后区分31产品首红+2混合+3纯夹具；两处安全UI期望纠正为原Describe且不泄rawmarker，r2同基线真实46(13PASS/33FAIL)全部清理，8磁盘日志错误来源仍实证。完整证据/摘要见A06报告，不将36项都算成产品缺陷。
- 新VM-r2异常归属实现已冻结但被root和独立静审拒绝：宽OCE catch改变Auto后端取消传播与Wall单设备失败反馈，未BuildGUI/未push。full只加三例取消防回归、不改原46条件；main保留VM-r2等实际红，再最小私有SourceExpired区分。GUI锁已回root空闲，所有代理不自行运行；完整目标仍active。

## 2026-10-15 / A06a发布隔离离线验收

- 实际中间版49=47PASS/2取消回归FAIL、全部清理，root直接读核；之后仅SourceExpired私有类型四处最小第三生产修正。49/150先过但旧Hover首败，保留AP023；只读诊断不改原断言/等待/焦点，不把后续绿称原原因已解。
- 最终四源VM79137...03D4、fixtureE3EF...051B2、hook6F99...F540、Appearance304D...8C90，root导入前实核每patch及canonical内容，fixture仅机械CRLF→准确LF，其余raw直接匹配。
- 独立PASS-OFFLINE-A06A：150/51.09秒、三桌面每49/401/191/49清理/9正确源日志及原209/36/73/25/12idle、六shell135、设备889/140与847/139、监控各952/308，14进程首轮退出0、无最终矩阵重试；汇总SHA E9C1300886B8E55C1F3413DB4CC08974B6E304F66044E82BD907F31F3EF457D3已root读核。辅助modal图片固定名被覆盖只余最后一轮，六份主/抽屉图和JSON/原日志各独立，不称六modal视觉齐全。
- 主仓150/51.65秒、fresh1000完整和390shell首次exit0；49/401/191/49清理/9来源及旧回归、135壳层通过并目视单一列表。最终DLL55D2AC54BC89B7C9A5A8B2589A16B479215DDC20FC8EDAFAFD8B34E66C9A67B7；实际profile前后SHA仍E4E33A8...BB05F0，所有exec/Desktop结束，锁空闲。不操作模拟器、账号或共享ADB。
- 本阶段只提交相关四源码/正式测试、README/脱敏报告/清单，不上传本轮artifacts/本机配置/依赖/ZIP；归档旧材料范围AP021不变。下一步A06b先复现自动首载Busy调度/旧缓存身份及保存裁剪边界，再窄屏树/截图布局，A07..A12和实机全目标不缩小，不标goal完成。

## 2026-10-15 / A06b1自动首载调度

- 上一目标轮为实际进展：A06a独立及主仓验证后已提交并push e235657；本轮继续完整目标，未将其缩为既有通过项。
- 三方只读定位Busy早退/pending不可达、传输/脚本恢复通知缺口、A到B到同一A漏待办；先写[A06b1轻规格及证据](codex/apple-experience/A06_AUTOMATIC_LOADING.md)，只允许两生产文件。Demo默认保护保留，新正式夹具仅自有artifacts/一致服务/STA临时许可，通过真实导航/Current/活动事件，不手调Ensure/LoadNodes。
- 独立基线150/51.89s；首次16例8PASS/8FAIL、112检查/481观测/16cleanup，七阻塞释放无新增读，DumpABA仅旧A读后0节点，原始2F1755报告已root直接核。原49及后续套件本轮未到，不计通过。
- 首轮产品只改窗口pending/单Dispatcher合并、执行前重验和VM明确HasActiveOperations通知；不改A06a、活动定义或安全门禁。不缓存脚本布尔去重，不以自身Busyfalse创建重试。一次机械patch导出参数错误保留后仅纠正命令，未改源码，不算产品第二轮。
- 实施150/51.60s、首次完整1000 exit0，新16/137/729/16cleanup及旧49/401/191/49cleanup/9正确日志源、原回归全过；root直接复读与目视最终PNG。冻结三源Main2E8DC93E...98D49/VM894985B3...E029B/new87335684...7A6AC，生产patch B454AFF5...B32CEF。
- 独立最终150/51.28s、三桌面各16/137/16cleanup及旧49/全部原套、六公共shell各135，9GUI首次exit0/0retry/owned0，18前后SHA一致；汇总D3128E92ADF5EB47223454D650E817AA829792CF8C00B0373ABE696A477A38FD已root直接核。六尺寸辅助modal本轮各自留存，不将文件留存称逐图目视全过。1920耗89.04s而另两约51s，无计时根因结论。
- 主仓机械导入前核基线/patch/canonical后精确三raw，旧49内容及原raw不变；150/51.27s、首次1000完整及390shell exit0。新16/137/729/16cleanup报告1329C107AB6EAACB78005A4576D9AC499BEE74F5A47A663D3D831F42BED3792D，旧49/401/191/49cleanup/9正确磁盘源报告47563A50BCDAB691CF582FE478DD823C1764400D89B73C501E3BD249B057F53F，DLL E3A9FFE48553C980B857D3C5F05C498022E553510859CA2D57827AE0C58196E2。root直接读实际JSON、目视两代表图，实际用户profile仍E4E33A8...BB05F0，必需exec/GUI结束。
- 本阶段仅三源码/正式测试、README/脱敏报告及清单，原始证据/配置/依赖/ZIP/两英文图不提交。AP019/020/023首败和AP021历史归档范围不关闭；不是生产timer、Android、同row离线重连或全部节点/截图功能通过。下一独立阶段为同控件正文响应式及缓存/保存/裁剪逐缺陷验证；A07..A12、真实双实例和30分钟稳定性范围不缩小，完整目标仍active。

## 2026-10-15 / A06b2a节点正文适配

- Goal/Context：延续完整目标，上一轮60a7922列表精简已push不重做；读当前源码、规格、交接及官方HIG/WPF合同，三方独立工作区。
- Decision：仅DebugView同对象三栏/中左右分区/窄分段、原工具和树头44；实际红后修Header滚入目标与属性未消费滚动意图，不在resize重建或请求设备。
- Evidence：独立原布局9例6FAIL/3PASS；夹具Path编译、Focus返回值前置分别纠正并保留原失败，不称产品红。首轮Header45仅显31、后续四大尺寸offset丢失均真实取证，第三生产轮局部ticket及新用户取消完成严格11例。
- Files：DebugView、MainWindow一行Demo接线、新ResponsiveDebugSmoke，README/报告与任务/交接/问题/矩阵及三源码清单；原16/49、VM/Core/Host和目标边界不改。
- Verification：独立150/50.80s、三桌面每11/905/319/11清理+旧16/49与原全套、六shell135；主仓150/51.02s、一次1000完整11及旧套/390shell135通过，root实读并目视。独立汇总CB18E4F0...9251B、主仓B7998537...A8342，DLL B98851E8...E0B71，配置原SHA/owned0。详见[A06b2a报告](codex/apple-experience/A06_NODE_RESPONSIVE.md)。
- Next/Risks：本功能独立审查后push原私有分支，不合并发布；再做截屏正文/缓存/保存裁剪，A07..A12、真实双实例及30分钟稳定性仍须完成。不是物理输入/DPI/Android/全部触控，历史首败与AP021保持。

## 2026-10-16 / A06b2b截屏正文离线验收

- Goal/Context：节点正文29ab03f已独立交付推送后，三方只读审截屏布局、原像素/缩放/保存合同，保留完整目标和CODEBRAIN不读写边界。
- Decision/Files：仅ScreenshotView.BuildScreenshotPage同对象640双栏/窄画面色值、局部44及色表文本预算；新增独立10例夹具和Main一行Demo hook，原11/16/49及VM/handlers/主题不改。
- Evidence：独立150/51.37秒、首次10例6几何红/4通过/10清理，两390图26宽，其余四在44命中首败。第一生产轮150/51.23秒、原几何/工具均过但坐标需求51.1767而文本区48，10例仍6红；不把布局预算不足冒充像素缺字证明，原日志保留。
- Verification：第二轮唯一坐标列70改100，预检150/50.81秒及完整套通过。独立最终150/51.05秒、三桌面各10/630/257/10清理+旧11/16/49原套/12idle、六shell各135；汇总44476DB4...F5946。精确合入主仓后150/51.43秒、1000完整exit0/87.66秒、同10和旧套、390shell135通过；root实读JSON/目视390画面色值及1440双栏，汇总r2 7193658A...8839E，DLL4D5DE138...DDFF2。124源码各阶段前后相同、用户配置原SHA、自有进程0。只读汇总首稿将标量matrixCases32计成数组长度1，r2按实际JSON纠正并保留首稿；未改断言或重跑产品。
- Next/Risks：冻结10源码/文档后独立核包与13路径提交，单独push原私有功能分支，不合并发布；包只回写三清单，排除两用户PNG/本机配置/本轮原始证据，历史AP021材料不清理。原生保存/剪贴板、未完成裁剪、缓存身份、DPI、实机、A07..A12及最终30分钟仍另验。未启动/修改真实实例、API、共享ADB或用户配置，完整goal仍active。

## 2026-10-16 / A06c1完成缓存取证准备

- Goal/Context：核当前c7b6d24及上一轮实际推送结果，重读PRD/SPEC/TASKS、当前VM/行/Catalog和旧49离线历史合同；上一目标轮是实际进展。
- Decision：本项仅已完成检查的来源身份；明确Offline且旧字段/代次+1的历史查看导出保留，新Ready换代严格核两侧身份。当前图/节点不能用后台最新bitmap引用决定失效；不推进revision或主动自动读，保存模态/在途crop/同row自动首载分开。
- Files/Verification：先存[A06c1轻规格](codex/apple-experience/A06_COMPLETED_CACHE.md)，三隔离树从c7b6d24建立；full仅新正式夹具/Main hook，hig仅有界runner，main只读。尚无Build/GUI、运行红、产品修复或通过率。
- Next/Risks：冻结夹具独立取原红，再授权单VM修复、独立/主仓回归及单独push；真实环境/配置/CODEBRAIN不碰，两用户PNG与历史材料保留，完整目标active。

- Baseline：冻r0 C79EE9D6...AD952/MainA1878014...E34A，独立首次150/51.67秒/0警告错误；一次1000 childexit1/8.312秒，18例3通过/15产品失败/18清理、168检查137观察。Catalog新t12/g2但旧row t11/g1仍导出旧PNG/节点且Error空/完成；root直接读原JSON/产物并目视，原报告550B2249...38FC、摘要7E022FD9...FDC0A。125源/旧四套/实际配置前后同，exec结束/owned0后归锁。旧10/11/16/49未到，NOT-RUN；首轮只授权单VM编码/静审，不提前称绿。

## 2026-10-16 / A06c1完成缓存身份离线验收

- Goal/Context：上一goal轮已实际复验用户四项列表精简（150/1000/390设备正文），不是状态复述；继续完整目标和c7b6d24缓存缺口，独立生产/tests/verifier树，不读CODEBRAIN。
- Decision/Files：单VM分别保存行截图/当前图/节点根来源戳，双row/Catalog核消费；精确Offline只读历史保留，失效不推进revision/自动加载、不抹Native状态。Main只一行Demo hook，新正式18例；原四套/Core/Host/输入门禁不改。
- Evidence：基线15产品红保留；首产品14/4，经保持原合取的七叶诊断证四例仅有效Targets按原离线门禁变空，选择和其他六分量不变。仅纠正新夹具合同并加强引用/意图与禁写目标断言，全部18case/产物/等待精确不动；生产41E4...A9CA未再改。
- Verification：最终独立150/50.98秒、三桌面各18/295/163/18清理及旧10/11/16/49全套/12idle、六shell135；33实际本地文件重开核像素/根/哈希。汇总04A22C40...3971D。主仓150/50.96秒、一次1000完整87.585秒同18及旧套、390shell135；汇总57E17C7A...2F4BD/DLL9E381B9D...A249C。125源各阶段前后同、旧四套/配置同，全部exec结束/owned0；root实读并目视代表历史PNG及JSON。
- Next/Risks：10文件冻结后核包/三清单/独审13路径，按授权单独push原私有分支，实际Git结果见本机交付记录及远端。外层colors=0不算取色成功；同row自动补载下一项，AP028未同步中间态观察、原生保存/在途crop、DPI/Android及A07..A12/30分钟仍开放。没有实际实例/ADB/代理/账号操作，完整goal仍active。

## 2026-10-16 / A06c2同行恢复首载离线验收

- Goal/Context：上一轮90815e8已实际交付推送并核远端/test备份，属于进展；重新读PRD/SPEC/任务、窗口调度/VM/Catalog，三新隔离树保持CODEBRAIN不读写及真实环境边界。
- Decision/Files：只VM/Main生产30+/1-，完整目录严格新Ready观察/通知，当前引用建基线，实际采用source同步strict ack履行已覆盖pending；不用Busy全局忽略、NodesChanged轮询或放宽guards。新正式19例/Main两测试接线；原五套/Core/Host不动。
- Evidence：首次独立150/150 51.11秒，原新fixture有2条CS4014；同DLL首次1000 exit1/30.373秒，19=8PASS11FAIL19清理/208检查1826观察，Ready行与Catalog新身份、图空节点0/pendingfalse实证。第19真实时序前置成立但基线无pending不称ack通过。原日志保留；新fixture r1仅显式discard多4ASCII，不改19动作/断言/等待；原五套该次未到NOT-RUN。
- Verification：唯一生产r1 C288/MainF243及新fixtureF1AC，独立150/50.92秒/0警告错误；三桌面首次exit0/93.798、94.062、94.575秒，新19每289检查/19清理，事件实际2214/2214/2212；原18/10/11/16/49全套/12idle、六shell135，摘要8609A9E0...6470E。主仓精确机械导入、只新fixture全文CRLF证明后复制冻LF；150/51.05秒/0警告错误、一次1000完整93.348秒同19/289/2214与旧五套、390shell135通过。根摘要BE91B9A5...3E381，DLL FB4B1F8C...703D7；root实读并目视，原Task/并发/晚到payload及第19true→false有原始分量。全部exec终结、自有0、各阶段126源/旧五套/实际配置同。
- Next/Risks：10文件冻源/独审后仅快照核包和三清单回写、精确13路径独审再单独push原私有功能分支，以本机交付记录/Git远端为准。19自有窗口固定1000，不冒称六尺寸每控件或Android/生产timer；outer.colors/nodes实际0保留。下一项先取证原生裁剪保存开框期间换帧的静态候选，保持合法保存意图，不误修已在写盘await前冻结的VM。未完成拖拽、JSON原生保存、AP019/020/023/028、A07..A12/真实双实例30分钟继续，完整goal active。

## 2026-10-16 / A06c2交付完成与A06d实际前置失败

- 前轮判定：实际commit/push `1acbf23b149047dd46a132660d417f31f5613812`、远端一致/test49e4710不变；301源/304ZIP/155Bundle与精确13路径经独审6518AB5D通过，属于进展。实际记录root artifacts/reconnect-root/push-complete-r1.json，不重做旧交付。
- 本轮范围：新[A06d合同](codex/apple-experience/A06_NATIVE_IMAGE_SAVE.md)、三隔离树和独立r0（仅App严格Demo验收入口/新正式夹具）；保存生产逻辑不改，原六套不动。先实际原生红再修，推荐保留开框时原图/选区，不默认漂移后取消。
- 验证：独立首次Build150/51.30秒，0警告错误/子进程73.422秒exit0，DLL25F2...1F77。root真实首启自有Demo后，sky首次FrameArrived超时、按技能重选/activate单次恢复后window capture仍超时，停止交互，无UIA/Win32输入旁路。
- 结果：同一child91.520秒正常exit1，无外层超时/kill；原fixture FAIL/verifiedfalse/cleanedtrue，裁剪/保存Click0，只有Discover/Discover/F0Shot，原Task2完成，无F1/原生框/信号/输出。只能判交互前置失败，不能当保存产品红或通过。
- 完整性：root实读原报告4A5E741F...6276E、工具记录D124A613...FB3DD，逐项重核完整127源、旧六canonical和配置E4E，实际PID0/两个exec终结，记录6EC31239...14E0D。窄GUI runner另为102个src cs/xaml，不冒称127。
- CLI补验：首r0仅missing-demo即因runner错把实际CP936按replacement UTF8解码而停，实际child拒绝正确；原FAIL1CB70...6285C/raw保留。新runner-r1仅严格解码/decoder记录，10断言/argv不动，root实读后批准另轮一次10/10拒绝PASS、各child exit1/无kill、路径无误建；摘要25A4E1EB...9E115。root逐例重读原字节核中文异常，127源/旧六/DLL/配置同、owned0。此处不是原生保存通过或产品修复，所有session实际结束。
- 下一步/风险：不发布未验保存改动；A07当前应用页只读审查发现Name相等/pending/固定栏候选，优先metadata同身份改名反例。可独立helper只读诊断，不据无截图坐标拖画布；原生保存合同与A07..A12/实机/30分钟仍全部保留，完整goal active。下一轮无需重复本轮Build/CLI/原生失败，应从A07轻合同与真实受控反例推进。
