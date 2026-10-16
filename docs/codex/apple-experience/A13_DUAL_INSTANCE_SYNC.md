# A13b 真实双实例同步补验

## 背景与目标

A08b已真实交付6cb127df2ca5e5b78a9c79616c7da11aacf0190a，原209、VM9与自有WPF十二组有限域通过。本阶段不重做该工作区功能，补验用户要求的真实双实例同步；完整GUI、应用安装卸载与最终冻结双实例30分钟仍为独立必验项。

根问题是缺少真实目标效果证据，而非四个开关状态尚无单元测试。现有InputCoordinator发送命令不等于目标页面真的变化，旧单实例LiveUiAcceptance和会quit/modify的MultiInstance.prepare也不能补齐双实例验收。

## 范围与约束

- 只允许专用实例1、2，实际名称分别为MatrixAcceptance-20261014-184112、MatrixAcceptance-20261014-205326。每次重新采样CLI摘要、目录、Player/VBox出生身份、现有5037连接owner、实际ADB serial/transport和VBox归属；旧PID和manifest不授信。
- 冷态测试只复用原协调器启动已授权实例2一次；已运行的两台续验只重新核定双方Ready身份，不关闭或重发启动。实例1不关闭/重启，原0及其他实例不操作、不新建/复制/删除，不改硬件配置。已有ADB owner变化即停，不主动启动、停止、重启共享服务或运行外部ADB客户端。
- 资源判断沿用现有LdLaunchMemory.Assess；明确不足则本轮停止，不替用户接受风险；未知或估算余量不足只记录，不伪造资源充分。启动命令只尝试一次，超时结果未知，不重发、不杀模拟器。
- 使用全新artifacts配置，API/Agent/视频与同步默认关闭，冻结本run实际身份。只在系统设置等安全页面操作点击、滑动、主页，不改变设置开关、账号、隐私/权限、代理、CA或VPN；不重复安装CMFA来规避已知首次权限页。
- 同步取原WorkspaceViewModel的Main/Current/Checked目标与原InputTargets、SendInputAsync、KeyAsync业务路径。程序内调用不是物理鼠标/键盘或完整GUI证明，不恢复已停止的AP045输入分支。
- 每个安全动作单独通过`am start -W -a android.settings.SETTINGS -f 0x10008000`重建专用实例的系统设置UI任务，避免上轮滚动/搜索位置成为假前置。这会改变设置页面返回栈，但不清应用数据、不改系统设置；不承诺恢复原UI任务。不能确认安全入口、列表或launcher就停止，不改用任意坐标。
- 仅Swipe准备阶段若未满足原四锚点判据，可对每台做至多一次独立SETUP-ONLY外层标题空白拖动，再各重新取一次截图/节点作新baseline。必须按新鲜exact Settings外层/内层/caption及层级计算，整段避开所有可点击/勾选控件，身份前后核等；不经VM同步、不计正式效果、不循环，仍不满足原四锚点即停。原Anchors会计入边缘裁剪节点，数量不等于四个完整可读标题。

## 方案取舍

推荐正式独立入口加专用夹具，复用当前运行时、身份准入、主控业务和输入协调器，以节点/画面/前台效果独立核验。更简单的直接ADB分别发两次命令只能证明后端，无法验开关选择与目标分发，拒绝作为同步结论。全GUI物理操作能提供更强证据，但官方Windows捕获本轮再次超时；保留该缺口，不通过Win32/UIA/SendInput或合成物理成功绕过。

主要失败模式为冷启动改变共享服务、目标重连换代、不同UI几何使归一坐标失配、节点生成失败、页面异步未稳定，以及命令成功但效果不符。失败先保存实际值再停止，不循环刷新取绿；若效果判据无法客观区分，标记前置未达而非通过。

## 验收标准

1. 入口严格限定本轮manifest及全新artifacts绝对目录，启动前拒绝原0、额外目标、名称/摘要/进程/owner冲突。冷态仅启动允许的关闭实例2一次，等待有界；两台Ready续验必须在guest前锁定双方出生身份及各自唯一实际transport，禁止任何launch。两种模式均保留两台运行。
2. 两台同时Ready且serial/transport唯一，通过实际VBox/TCP归属与原admission；原5037owner在所有写前后保持。与启动前全目录比较，除目标2合理启动外其他实例采样字段保持，不能扩大成所有数据无变化的绝对审计。
3. 主控A为1、当前设备B为2，仅勾选B。四组合(M,K)=00/10/01/11各自测试完成点击、完成滑动和主页键：A效果总成立；B仅对应同步位开启时变化，关闭时对照页面真实保持。每例有前置、步骤、预期、实际与独立效果证据，不只看Task完成。
4. 每次冻结输入来源、目录代次、主控代次和几何；目标身份变化立即拒绝，不按端口公式授信。真实节点和PNG、前台活动/列表位移等判据须在执行前静审，不能失败后改成更宽松标准。
5. 单次执行全步骤落盘，包含原错误和未执行项、实际程序集/输入摘要、原始流、运行时间及清理结果。只有owned宿主资源被排空，不在finally关闭真实模拟器、清数据或卸载应用。
6. 独立审查源与实际终态后，相关正式测试/报告/清单单独提交并推送原私有feature；不合并发布。即便本阶段通过，也不关闭应用GUI、原生选择器、物理输入、全12页/DPI、最终30分钟或完整RPA/MCP/抓包。

## 执行任务

| 顺序 | 优先级 | 内容 | 验证 | 状态 |
|---|---|---|---|---|
| 1 | P0 | 主仓/目标/共享服务与外部观察重新核验 | 当前快照、保护配置、窗口捕获实际结果 | 实例1正式只读17步及冷启2/双准入获独审；续验两台Ready须重新采样 |
| 2 | P0 | 最小入口与客观效果判据设计/独审 | exact1/2、身份/资源/副作用边界、四态反例 | 推荐设计已准；最终源码另审 |
| 3 | P0 | 实施并冻结正式测试入口 | 原209保持，CLI拒参及身份负例不调用guest | r05静审及唯一离线209/十一早拒获有限独审，源码和产物冻结 |
| 4 | P0 | 冷态仅启动2一次，再真实四态测试 | 实际效果、失败分流、owner与其他实例/清理 | r05 Ready续验14实际效果及身份/清理获独审有限通过；外层字段读取失败保留，不称outer通过 |
| 5 | P0 | 独立终态审、报告/清单和单项推送 | 不把功能层当物理GUI，实际Git回执 | 两源已精确集成，主仓209/十一早拒/VM9/WPF12及报告合同获独审有限通过；核包和单项交付以实际回执/Git为准 |

## 本轮初读

- 2026-10-16T13:06:19Z现有5037 PID62248、出生2026-10-15T01:23:48.7738080Z、路径E:/leidian/LDPlayer14/adb.exe、SHA705DDC21F33AC52105D1B075B019962AD0E44FB3D560BDE69CE8CB3A36BCA183，协议0029；host:devices-l仅实例1 emulator-5556/Ready/transport8。该工具guestCommands0，不启动服务；原件在artifacts/live-resume-6cb/host-before。
- 官方CLI list2初读23条，仅1运行Player63000/VBox68124，2及0/其他关闭；直接shell终端对中文错误解码，不作为中文姓名原始证据，后续须用原LdConsoleRunner的严格UTF8/GBK解码和保存原流。物理可用约9.30GiB仅是当时快照，不外推实际启动资源判断。
- 官方sky.list_windows成功，唯一实例1窗口140054044。首次只读capture报告minimized；按官方恢复步骤activate/get_window后再捕获得到FrameArrived timed out，停止所有后续Windows输入，不使用旧坐标或其他自动输入手段。没有点击、键入或guest操作；该激活只恢复了本来最小化的专用窗口。
- 托管worktree工具因调用目录不是Git根返回Not a git repository，未创建产物；回退在已确认主仓根以git worktree add --detach 6cb127d建立独立real-dual-sync，不删除或清理任何旧worktree。主仓仅此规格未跟踪，保护配置与两个用户PNG不改。

以上只构成真实环境初始证据，不是同步、安装卸载或稳定性测试通过。后续必须读取当时状态，不沿用此处历史数值。

## 实例1正式只读预检

- 原`preflight1-r01`包装器在任何host/产品子进程前由广泛命令行Contains运行保护拒绝，stages为空/163+3不变；原判定PID未记录，不能事后断言当时一定误把pwsh当产品。保留DE6EA023...BE3FE源码与失败回执，后仅将进程识别限定为已知dotnet/Matrix exe并保存匹配项，使用全新目录。
- `preflight1-r02`匹配运行时为空，但host初读后出生时间精确比较失败，尚未启动CLI或正式preflight。实际诊断：CIM CreationDate为639276242287738080 ticks，而原生Process.StartTime为639276242287738084，同PID/path/hash相差4个100ns tick。不是服务换代证据，不将较低精度CIM当精确身份判据；保留B24D9ECA...BD483和原失败，未重发guest动作。
- 根修`r03`先从唯一5037监听owner取得原生PID/birth/path/hash，再做host查询并重核同一原生epoch；正式manifest只使用原生ticks，与Core实际socket身份合同一致。CIM只记诊断，不容差、截断或重新冻结变化的epoch。固定runner439A5834...EF144经独审后唯一执行：directory-before65864/344ms、正式preflight34580/2557ms、directory-after39320/140ms均exit0；外层6.25秒/exit0、六原始流排空、无timeout/kill/清理错。
- 正式17步骤全部COMPLETED-READ-ONLY，124身份记录及38次仅list2，七条显式shell读均成功。Runtime初始化另有getprop/wm size/dumpsys input读取，其raw不由现接口暴露；不把七条显式读冒称全部wire。原生三快照及Formal实际socket owner全同62248/ticks639276242287738084；23目录业务采样字段前后保持、163输入/三保护和实际加载程序集摘要保持，结束owned Matrix进程0。
- 当前1为Ready、Android SDK34/boot1，ABI广告含arm64-v8a及libhoudini；普通及-u两条全用户PM查询均未见CMFA。只是兼容候选和PM观察，不证明安装/签名、无所有磁盘残留或GUI可用，未执行任何安装/卸载/启动2。
- 原件在`artifacts/live-resume-6cb/preflight1-r03`：execution `DC9B46950F706710E6077D9DFC18F48D7C14FCD19404703BD10E7E4D08ADBD36`，Formal summary `C20346E5FE5E5AB9D53DC88240F59D78C5804CA66F89D43E161D9ECA8E2C14CA`。独立验证者已逐原流/步骤/身份与保护核验，仅准进入双实例夹具冻结/独审门，不直接放行未审新入口或永久复用旧Ready身份。

## 新入口静审

独立树real-dual-sync的r01两源只完成静审，未构建/设备。App为B3F81ECA...860F92、新夹具95CD13A9...91920；独审拒绝三组：manifest未硬锁已授权专用名称及CLI、实例1当前transport核验晚于启动2、晚返回runtime可能错过唯一Dispose。原件已机械保留，不将静态问题称为真实产品失败。

r02夹具B96A464D...D2561B、App不变，已集中补上述范围和前置门、调整清理并补缺少Features命名空间及nullable读取，四态Exercise原文保持。当时尚待独审确认清理在Task完成与owner选择之间的竞态；该稿未获批准构建或实际启动2，后续未运行仍NOT-RUN，不为“已有测试源码”记同步通过。

r02后续独审确认owner选择竞态仍存在，拒绝进入执行。r03仅修改159-185清理段：对同一creation Task一次读取终态并冻结所有权，已成功则接管后唯一Dispose，当时pending则保持unresolved/FAIL并保留上游service/seed/deadline，不因稍后完成改称清理成功。App及Exercise和其余正文不变；r01/r02原件保留。最终夹具SHA为8D5D1B464F88AEB5F3DB887EBF871A0BD4CCF4F28F9D6B2413AEE5935A96897B，独审仅批准离线构建/拒参，不直接批准live。

## r03唯一离线验证

- 冻结包装器B405AE391C7C4404C944B725B5EC8816FE9E70D95010E9BC153AF3EE2F687928经root逐行核准后单次执行，session33405实际outer0。Build43772 exit0/76.192秒，原209名称和顺序精确相同，测试51.65秒。实际3条编译警告/0错误：新夹具280行CS8602在WPF临时/正式工程各一次，既有Tests CS9124一次；不写零警告。
- 八例分别为混入Demo、混入旧模式、重复模式、SHA不符、已有输出目录、错误专用名称、错误CLI身份、实例2非冷态。实际均exit1/173-228ms，原stderr含对应精确错误且堆栈落参数或manifest验证，未到Bootstrap；七个新输出未创建，已有目录仅原sentinel且摘要保持。
- 所有合成manifest固定假server PID1/birth1，不采样真实服务，也无有效基线manifest执行。八例stdout均0字节；完整九子进程/18原始流终态排空，无timeout/kill/清理错，owned0。两候选源实际PDB摘要对应，164输入/27原Smoke和主仓163输入/三保护/46编译文件保持。
- 原件位于独立树`artifacts/live-dual-sync/offline-r01`：execution SHA02E34E9ABF46271737315CF977CF93F7E5BE05CE7440FE1BC113A5B7F9F34540，after SHA135007871B65FE79B56AFAEC072DFBF57282F72240DF6388819E24D9F9D106B4。Root已实际核原始测试名序/编译告警和回执；执行者核验记录independent-audit.json SHA2315C5B6724402A3B36EFE98C485A652591FA1A166FAB4148B9AAF3A909BB1A0，另一未参与执行的独立reviewer亦实际核原流/PDB/保护后有限通过。27旧Smoke仅源码保真，不是本轮执行27种模式；没有批准live或证明未运行异常分支。
- 本轮没有启动2、host/guest操作或GUI输入；源码仍仅在隔离树，主仓生产代码未集成、未提交推送。不把离线拒参、编译成功或17步只读前置计作真实同步通过。下一门是重新采样当时目标与原生5037 epoch、冻结实际manifest并审有界owned宿主包装器后执行；不得Kill整棵后代树。

## 首次真实双实例执行

- 原driver r01（0B2A1703...2841C2）先完成host-only读取，但在directory-before/host-after子进程启动早期读取MainModule.FileName时遇null，双流尚未接管。外层exit1/6.39秒，未生成manifest、未运行fixture、未启动2。原terminal/drained=false及未知退出码不改；后续精确70340/57832 PID均missing只证明观察时不存在，诊断SHA AC8E1DA1...689A7，不恢复原流或编造退出码。
- driver r02（1A10864D...37F821）仅将双流接管移到Start后，保留同一个原始Process及SafeHandle至排空后，StartTime变辅助观测，去掉MainModule前置及按PID重新选择清理对象；仍只Kill(false)、5秒终止等待/每流10秒排空。独审后全新live-r02唯一执行，session51935已终态outer1；五子进程均终态，host-before47408/1240ms、directory-before33504/141ms、directory-after35904/156ms、host-after24140/1406ms退出0，live70324/25582ms退出1。十路原始流全排空，无timeout/kill/清理或辅助观测错误；首错误与原r01完整保留。
- 正式r03夹具确实只启动2一次（10.6775秒），实例2实际Player41564/VBox56028、Android已启动；同一5037原生epoch，1仍transport8、2新transport9，实际TCP/VBox准入及两目标runtime已完成。23目录仅2启动相关字段变化，原0、1及其余业务采样字段保持。末态owned任务20/pending0、唯一runtime Dispose完成，cleanup/report/target错误空，Matrix宿主0；164源、主仓163+三保护保持。所有数值仅本轮历史，不给下轮复用授信。
- 首个00-tap在A点击前后确有页面变化：前台由Settings转SettingsIntelligence，唯一android:id/search_src_text为android.widget.AutoCompleteTextView且focused/focusable/enabled/visible均true，原PNG亦显示搜索页；B前后整棵节点及PNG摘要完全一致。原SearchEditor硬限精确EditText类，因而记录Source A has no objective tap effect。独审确认是夹具oracle错误，不支持产品点击无效结论；[Android官方类定义](https://developer.android.com/reference/android/widget/AutoCompleteTextView)也明确其继承EditText。
- 原summary仍FAIL、started含00-tap而completed仅bootstrap/runtime，后13项NOT-RUN；不补记整例PASS，不用图片事后覆盖原断言。execution SHA2BCA54915E2EBA7766176A4DA888F8F0408CC138A59815A0D8EB4CC94C31AE9B，summary SHA5A109475311CA21DE048F895FD5ED0C526FABE79521796BEC5DAD3F701A2492A，原件在独立树artifacts/live-dual-sync/live-r02，另一未执行者已核节点/图片/原流/身份/清理。
- AP-050拟仅修精确搜索角色谓词：保留ID、两固定package、enabled/visible/正bounds，仅EditText或本次确证的AutoCompleteTextView且Focused。两台已运行，不采用先关闭2再冷启的更具干扰方案；新增同一入口的严格Ready续验，双方实际出生身份/TCP归属/唯一Ready transport均须在guest初始化前核定，Ready模式拒绝任何launch。原冷态路径、四态/映射/其他效果/清理不变；r04两源审、离线原209与Ready负例、单次真实续验后才能接受结果。

## r04离线与Ready首验

- r04只修改fixture为FA834CFCDEC328F3E26AFF80F593D3ABDAB1DA5DFE73130E07B70404D9E4619A，App仍B3F；Run/cleanup、Exercise/mapping和四态断言原文保持。精确AutoCompleteTextView且Focused修正及两Ready/noLaunch互斥模式获独审，所有Ready的目录/native birth/实际TCP/唯一transport在guest初始化前核定，Ready runner拒任何launch。
- 唯一offline-r02 session7528 outer0：Build53872 exit0/64.866秒，原209按冻结名序全过/51.26秒，原8及新缺birth/重复Ready tid/noLaunch却cold共11早拒全过，全部假server1/1，未进入host。12child/24raw终态排空，无timeout/kill/清理错，两个PDB/164源/27旧Smoke保真、main163+3/候选与主仓各46保持。增量实际2条fixture CS8602/0错误，未重发既有Tests警告，不称旧警告已修。execution3D66769B79965CA7F8B34FA0663EE4090D1EAE732AF7254750E27858DCAB3C51、afterF8157C1FD735CA91A9D93D4640E9CBC88A0F239B772D1928DB4CCCC8ADE165E9，未执行者已核全原件并有限通过。
- Ready driver59FC040C...DF013E经独审后唯一执行ready-r01/session72824，五child/十raw全终态，live38112 exit1/17.423秒，其余host/CLI均exit0；launches0。00-tap正式completed，A搜索页成立/B保持，AP-050搜索角色修正获此实际复验，不扩大到其余开关组合。
- 00-swipe开始后，在任何正式swipe发送前因Not enough concrete list anchors停止；两个ScrollView的截图/节点均显示外层settings_homepage_container为(0,36)-(1280,720)，内层main_content_scrollable_container只有(0,409)-(1280,720)。可见3个title，其中第三“应用”仅19px高被裁到720，原四锚点未满足。换内层候选不能解决高度；不是产品swipe失效，原summary FAIL及余12项NOT-RUN不改。
- 独审已核16步骤/五child十raw摘要、零launch、23行全业务字段/同epoch与两tid/native birth保持、164/main163+3/候选与主仓Desktop及CLI各46保持、runtime唯一Dispose/pending0。execution31B25BF007694DEE6D9B6FB7E37E36B440824469DDD604CD4B65F3BA9C3ACF0F、summary8CC69B5360BB5E9BC241007E857A67BE65BA81EF9EE361E1C21CB15295E6B838，原件仍在独立树artifacts/live-dual-sync/ready-r01。
- AP-051选择一次outer-header准备，而非降低四锚点或改显示：独立两方静读确认exact outer/inner/homepage_title均唯一，后者不可点击/勾选，按其本次bounds计算的(640,231)至(640,45)整段在外层内、内层之上，无交互控件覆盖。坐标仅历史证据，r05必须从新帧计算并验证层级/整段避让；是否真折叠只能由单次后读及原四锚点门证明，不预称成功。内层预滚动可能只换内容而不折叠标题，PageDown焦点路由未知，均不采用。作者仅获Prepare/独立setup helper范围，不修改正式效果和清理；r05静审/离线/新实际续验仍必需。

## r05离线与真实四态执行

- 冻结夹具6223A49168BAF9047F646EFE9B5FBF2B787CD1A262EAAEDF406C73FADEDB4F3E，AppB3保持；仅Swipe Prepare、独立安全header helper及对应调用修订，正式Scroll/effect/mapping、四态和cleanup保持。经静审后offline-r03唯一session21657 outer0，Build26296 exit0/64.791秒、原209精确名序/51.34秒、十一假server早拒均通过。12child24raw终态排空、无timeout/kill/清理错，实际增量2条fixture CS8602/0错，旧Tests CS9124未重发不称已修；164输入/27旧Smoke/两源PDB及主仓163+3/各46保护保持，独审有限通过。execution FD2CBBD6D2F9F946005704A33AF3D95DCEDAD530B6174BB5C455A2C054231799，after03835E846AFC8C24E320E82162C2AC36E6D9C7EA1AF3668C7F450BC49BC5AEDF；冻结files.json为DB89F1344A648632F67788F468295D4774E80B1DD148FED3CD27D68AE6870599。
- 仅更新dirname和冻结hash的Ready.r02 driver FEB715101A832BF486CB7A842422EFA0673DA05AE477EEB484CAA60104937C22经独审后唯一实际session8665，五child均exit0/十raw全排空、无timeout/kill/清理错。live25120实际190.016秒，manifest53508A1F6008F73DCC2F68D532A598CFF491F7F206CDA98BA5C5349BF5B94450，两台AllowLaunch=false/launches0。夹具报告PASS-LIVE-VM-SDK-NOT-GUI，bootstrap/runtime和四态各Tap/Swipe/Home、未勾选Tap/Home共16completed，14effect原件存在、NOT-RUN空；134owned/pending0、runtime唯一Dispose完成、Windows0，无模拟器stop。设置任务栈不恢复，不扩大为文字输入或物理GUI通过。
- 外层仍exit1/livePassed=false：actual-live-report在StrictMode下直接读成功summary中被序列化器省略的null error属性，触发PropertyNotFoundException。其余十条件（含23行全业务字段、现有owner及双transport/native、源码/编译物保护）PASS。原execution F368FD496AEFC94D67CE86A2E0C13A213297A9E66626B57E797DEFC67D3E8980，summary16E93F20FD5A65EB871585159E34AEC0B4D1D919A3D0FA00156FA9A89F152334均保留，不修改原回执或因此重发真实设备。
- 独立只读终态审已有限通过：129步骤SHA及raw PASS名序、五child十原流长度/SHA全相符；64帧PNG哈希/尺寸/目标与节点匹配并作目视核对，A全部有对应效果，B只随M的tap/swipe、K的Home变化，关闭位及未勾选保持前台/title/editor语义。八次header准备均单目标、每例每台一次；正式swipe使用准备后的新baseline，独审重算10-swipe A及11-swipe B均由四锚点变为另一组四锚点，不把准备折叠算同步。
- 独审另核505次完整目录快照、2030次epoch probe、三份native owner及前后双tid一致；23实例业务字段保持，508条LD返回均list2、launch0。134owned/pending0、单owner Dispose完成、after Matrix0、三组46编译文件及源保护通过，清理/目标/观察错误空。只认可本次VM-SDK实际14效果，原outer1不翻绿；物理GUI、文本输入、30分钟仍NOT-RUN。原r03/r04失败完整保留。
- 全部必需exec已终态。Root已按原main App A446与候选B3/6223先核摘要，再机械精确导入两源，主仓新摘要与实机冻结源相同；其他正式源未改，用户两PNG不动。待主仓唯一构建/十一fake拒参和既有VM/WPF代表复验、报告合同离线审、清单核包及单项推送；不因harness错误重复真实动作。

## 正式入口与复验

入口位于App早期分支和独立LiveDualSyncAcceptance.cs，正常桌面启动与既有专项链保持。仅显式模式读取manifest，不加载用户配置或启动API；不能与Demo或其他模式混用。默认Build -Verify不调用本实机入口。

```powershell
dotnet ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --verify-live-dual-sync --manifest ABSOLUTE_FRESH_MANIFEST_JSON --manifest-sha256 MEASURED_MANIFEST_SHA256 --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY
```

manifest的version为1，consolePath/consoleSha256和server的pid/startedUtcTicks/path/sha256均来自当前核定身份；targets只含已批准的精确1/2名称、serial、allowLaunch及Player/VBox原生出生ticks、实际transportId；directory含完整官方list2记录及0。不能复制报告中的历史PID/tid/manifest授信，不能修改硬编码范围以自行扩权。两个Ready目标必须各有完整出生身份和唯一transport，双方allowLaunch=false；冷态路径只允许明确关闭的2启动一次，1须已Ready。字段、重复JSON键、SHA、已存在输出或身份不符均应在设备操作前拒绝。

## 报告合同与主仓复验

- AP052仅对报告消费做离线验证。固定原execution/summary/trace及效果摘要，只读核原158文件、129step和十raw、14effect；新fresh目录运行8份JSON，省略error/显式null成功，非空/空串/对象error、缺status、bool与整数写成字符串均拒绝。唯一执行exit0/0.793秒，audit SHA4D9C9B70C06D5A19EE21500FC59E239B476B6F374773A5B28C960C2BE9BBA232，独立逐原件及8case复核通过。新审计明确originalWrapperOuterExit=1，不覆盖原回执，不替代独立Android效果审，更未重发设备。
- 主仓唯一MainOffline.r01包装器FFAC8E0EAB9337F4941FE8E74C646CD185CB4E85D6D6AEE4D923A29A8AF6E657经独立静审后执行，session84458 outer0。Build36156 exit0/73.956秒，原209精确名序/51.28秒，实际3条警告/0错误：本夹具CS8602两次编译及既有Tests CS9124；日志摘要重复行不重复计数。原十一fakePID/birth早拒全exit1/174-221ms、精确错误与fresh/sentinel保持，未到Bootstrap；VM56204 exit0/505ms原4参数+5任务，WPF80396 exit0/25.956秒十二own-WPF/Demo与清理全过。
- 14child/28raw均终态排空，无timeout/kill/清理错、NOT-STARTED空；单child预算300秒，仅原Process及SafeHandle持至双流排空，未杀后代。主仓164输入、原162未改raw、27旧Smoke原文、用户三保护及candidate164/46与主仓Build后46保持；两源PDB raw checksum对应已实测B3/6223。27仅源码保真，不是本轮运行27个模式。execution E8424CDEB331D15262F75DD3DE1041FE444DD3BDDD00C6F16FEA2605442E68B7，after7C58CBC5FA5B30ECCBAFDA96990A67C88254486FF2D0EDE440271C58843D0772，WPF报告2CAC7F499BF4D4E560CEE7FFA93715B114A0B94CAEB4E66835E3799D7A9215B5，原件在主仓artifacts/live-dual-sync-main/offline-r01。
- 独立主仓实际终态审已有限通过：VM9实际调用、零接纳、取消目标及结果元数据对应原oracle；WPF12实际已显示尺寸、编辑提交、A取消/B继续、日志来源和500滑窗成立，31个managed PNG尺寸有效、六尺寸presentation已目视。源码/PDB/原流与全部保护实核无新拒点，当前主仓DLL489DE35D0A50B874965D74763F6F96A02AACC597795CDCC7FFB998DEA56427D0、PDB8CEBAB65BA1EB64A1AFAE26A5664655A5260BBB585984A055931DE4375B954DE。交付只含相关正式源、README/报告及生成清单；核包/单项私有feature提交以实际Git与本机回执为准，不合并发布。
- 此轮主仓复验没有host采样、ADB/guest或物理GUI；十二WPF只是同一自动化页的六尺寸与交互代表，不是全12页面通过。完整A13/A11/A12及应用GUI、文本、最终30分钟和A14-A16仍开放。
