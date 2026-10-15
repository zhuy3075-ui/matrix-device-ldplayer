# 应用自动读取的待办恢复

日期：2026-10-16。基线`cdfe674305a9ec8133ce1b97ba34a080c9beda2d`已实际推送；前序AP-033阶段补齐观测并保留完整GUI失败，不重做该交付。完整目标仍active。

## 事实、根问题与目标

应用页独立勾选通过350ms DispatcherTimer读取列表。基线先清`applicationReadPending`，再由CanManage检查脚本、传输及API活动。脚本终态和传输释放已有VM通知，但通知只在pending仍存在时补排。空选择、离页、关闭和读失败也必须有明确意图语义，不能靠偶发刷新或手工重选恢复。

这些是当前源码事实与AP-034候选，尚无本阶段产品红。目标是暂时阻塞只延后一次只读List：活动结束后加载最新仍明确勾选的目标；不自动安装、启停、清数据或卸载。不把应用选择与Current/Main/群控混同，不放宽原在线、保护、脚本、运行时和连接身份检查。

## 范围与推荐方案

本单A07b1优先处理已有真实结束通知的脚本/传输，以及Busy、空选、离页和关闭边界。API释放通知生命周期保留为A07b2；应用正文响应式、外部安装后的列表新鲜度、原生保存、实机和30分钟仍是后续必验，不缩小完整目标。

推荐先用真实UI事件复现，再仅在ApplicationsView中把“不需要读”与“暂时不能读”分开。pending只在实际获准发起批次时消费；空选明确取消，关闭不新增读取。现有VM活动通知唤醒已有待办；读失败消费当前意图，不循环重试。批次开始仍重新冻结当时的明确选择/连接身份，而非阻塞时旧目标。

更简单的仅移动清pending语句可覆盖主路径，但不能单独证明空选、关闭等边界。拒绝移除CanManage、修改全局HasActiveOperations、持续polling、自动重试失败List、重建页面/清合法选择或改不可操作控件制造红。若必须改变公共活动语义或超过最小页面范围，应另以证据说明，不借本项扩大。

## 验收与轻量矩阵

| 优先级 | 用户路径 | 必需证据 |
|---|---|---|
| P0 | 正常应用目标勾选 | 实际可见可用CheckBox/Binding产生意图，原350ms Tick已触发，恰好一次List并显示实际包 |
| P0 | 所选目标脚本Running/Cancelling | 真实ScriptScheduler状态与后端/等待门、tick在阻塞内；阻塞期间零List，真实终态后只读一次 |
| P0 | 脚本Completed/Cancelled/Failed恢复 | 真实终态/原Task处置，错误任务如实观察；不把脚本未开始或先结束当产品红 |
| P0 | 稳定无进度Push | 真实StartTransfersAsync和Backend Push TCS进入，tick跨过稳定阻塞；释放后Transferring=false并补读 |
| P0 | Busy与双阻塞 | Busy前后真实通知；先释放一项仍不读，全部结束后一次，不放宽门禁 |
| P0 | 阻塞期间改选择/清空 | 仅最终明确目标；脚本只阻塞当前所选设备，旧A脚本不应阻塞已改选B；需共同阻塞时使用全局Push。全清后零List，无旧行/旧Current回退 |
| P1 | 离页/返回、真实Close | 隐藏不读，返回恢复已有意图；关闭后不新增，处理原Task和timer，不冒称实际模态确认已测 |
| P1 | List错误、重复通知 | 一次错误后不循环；新用户意图可恢复，不伪反馈成功、不并发List |
| P1 | Current/Main/群控B，应用A | 只有应用A的List；全局对象引用、勾选和有效目标保持 |
| P1 | 安全回归 | 原CanManage/RunPlan/RequireContext保持，旧A07a与八份正式夹具不弱化；受控Demo不替代真实保护/身份/设备效果 |

每例记录真实Tick、pending/running、活动状态、List目标/次数/最大并发、后端调用与原Task/TCS清理。只跟踪实际可取得的后端、脚本、传输与VM任务；生产timer回调是async void，没有可await的原Task，以真实Tick、后端活动数、UI状态和Dispatcher idle作有界排空证据。复合用例分阶段标STARTED/PASS/FAIL/NOT-RUN，初始化或门未进入的失败单列前置，不混算产品红；不要求所有基线用例均失败。350ms是原行为，不靠缩短/加长timer取绿；不可直接调用Schedule/读取helper或停timer清pending来冒充自动路径。

测试只用一致自有Demo Backend/Catalog/Runtime和STA窗口，API关闭、artifact内目录，不触碰真实模拟器/账号/共享ADB或CODEBRAIN。必要脚本调用只在自有后端留证，不转发guest；应用自身只允许只读List，其它写入口记录并拒绝。实例running时目标区禁用，尚无普通UI新意图反例，不直接修改禁用行制造故障。

## 顺序与约束

1. 新三树apple-app-pending、-tests、-verifier均基于cdfe674。测试者只新增ApplicationPendingSmoke及Main单hook；新专项放旧首套之前，原全部旧调用相对次序和八旧夹具字节不变。这样原生产可取新专项红，不靠已知旧指针前置阻断；没有新增App/CLI许可模式。
2. 正式新夹具冻结、双静审，独立树原生产单Build -Verify和单1000完整smoke，先读取真实红和明确NOT-RUN。原AP-033失败不改、不跳过、不把后来未到的套件计通过。
3. 授权实现者在另一树最小生产修复，最多三轮review-fix；冻结源后独立相同新用例与正式回归。只有测试证据改变下一行动时才新轮，不盲重跑取绿。
4. 主仓按冻结字节整合，核全仓内构建输入（含tests lock和四根配置）、独立交付检查、README/状态/源码包；一个已验证功能单独push既有私有feature，不合并发布。

所有Build/GUI串行，锁由root明确发放；每child300秒另有有界自有清理，raw双流保存，观察yield只续同session。实际进程终结后归锁，不依据文档旧状态假设已结束。

## 当前状态

主仓cdfe674/两用户PNG与实际配置已核，三个新树建立。正式12例static-r1已冻结：新fixture D14C76FE5E7AE882F0C25F04B180F9CD3508CA9DB33793FF7C96A2569F3CCB3D、Main唯一首hook 33C7597D4ED6688CF0CC015EAEF7BE85DB2A0246242BD057E1A5B0A6710023BD、patch372AC1D994E93350C81783245A44C35A45C9CB2E271F0814B7E9A0017384DA01。原八夹具逐字和原调用相对次序不变；成功后850ms观察重复读取，失败路径亦记录禁写与无关读取企图，GateExited不混称原返回Task完成。

独立verifier导入前143输入基线C840C9AE3A31C4D6A66C6CFE471B1ADA8159839555C24A6CD74D5217A746B06D保留，两新源已机械导入。root已读runner5640778E59B0F592469A97FC0CFCAC7404A34400ECC448E22CA2B630D4350242，最终独审通过并核配置/自有进程后，授权独占锁仅跑一次baseline b01；执行状态以实际session/acceptance为准，不能从本段推断已运行。生产仍9941原字节，本候选尚未取红；API、布局和完整目标保持未验。

## 原生产首次实际结果

上述“尚未取红”是执行前状态。独立baseline-b01的session29660已真正终结：Build36796 exit0/74.218秒，正式150/150（51.68秒）；新夹具4处编译警告经WPF两轮共8条、0错误，原始流保留，不改写为0警告。Smoke37660 exit1/51.022秒，12例5PASS、7PRODUCT-FAIL、12清理，无前置/夹具/清理失败。

7条红为脚本Completed/Failed/Cancelled、稳定Push、双阻塞、阻塞内改选B及离页返回。日志实际记录可用CheckBox/TwoWay选择先建立pending，原350ms Tick在真实阻塞内清为false且零List；脚本终态或Transferring释放及VM通知已发生，但5秒内没有恢复。最终选择例的两个Tick均在Push内，Push结束后A脚本仍Running、当前目标是B，仍无List；不是把A当全局锁。正常B脚本/应用A、Busy恢复、清空、真实Close和List失败后新意图恢复5例通过，不能要求它们基线失败。

原报告`artifacts/a/b01/artifacts/ap/41aa351b/application-pending.json` SHA256 `9F9A62F1C21A983D54A9C48B193198059402784284E3828EC2411FA466B3209E`，Smoke原stderr `FF57F1128BB041602859D758F2CCAB6ED25C4E0DC1709EF8F4B299EECFC36567`；均在独立verifier树。后续全部旧专项、旧应用与12页因新增聚合失败尚未执行，明确NOT-RUN。原始144输入/八旧夹具/真实配置前后同，无timeout/kill/残留，两个child都terminal，自有进程0。

据此授权另一树仅ApplicationsView首轮生产修复，以及测试作者仅4处显式discard/nullability修整；D14C原夹具和首次红记录保留。修复版、主仓与交付尚未验证，不标已完成。

## 修复与独立验收

生产r1仅`ApplicationsView.cs` 7+/3-：空选择清待办并停止debounce，Schedule及Tick尊重关闭状态和pending，CanManage通过后才消费意图。原350ms、CanManage/RunPlan/RequireContext、身份/保护及失败不重试不变；源码`4260EB20604B4C340A52A9E35CD522C2932B4A46D870707F346AF1D672D3F56A`，补丁`F4E8DAB8A7D397EA42F47CC9FECEF466C96F967F83713AEF957616D65D793DC2`。

测试r1a仅在原D14C基础改一处`_ = Track`与三处`Vm!`，后者表达Window已构造必先建立Vm的现有不变量；12路径、断言、等待与八旧夹具不变，逐字四替换证明保留。最终fixture `DBDBAB64D1639838D59CFBB88FE877F9BD6888385C5CF6EBD52DB63216B3DA9C`，四行delta `FAD8798928DED711F1E4A4F45B1949367EC9FC715651780F5BA096161C1062C7`，Main仍33C。这不是重写基线红为绿。

独立final-f01 session45047已真正终结：Build77936 exit0/64.859秒，150/150（51.27秒）、0警告错误；完整1000演示Smoke85300 exit0/126.368秒。新12 PASS/310检查/12清理、112个可取得任务终结，门全终结且无禁写/无关读取；阻塞Tick保留pending，全部必要门释放后单次读取，B读取时A仍Running，不改变全局B。失败窗口不循环且新意图恢复，清空/Close零List。

旧19重连、18缓存、10截屏、11节点、16自动首载、49发布、10应用改名与原应用smoke、本轮12页流程都实际通过；desktop.error为空、apps1，nodes0/colors0仍原样记录，不换算为外层节点或取色效果通过。旧AP-033本轮通过也不证明其间歇输入源成因已修。stderr非空16090字节，其中是旧节点观察器已捕获Visual祖先/外观诊断，不称零输出。DLL `CF690B61EBBCA2AA9E8ED5FEDDBADADCC03C0BD7E005B79B55FA52FCF4ABC8AD`，新报告`artifacts/a/f01/artifacts/ap/1f5b48c9/application-pending.json` SHA `4FE70E6AA8843A459F511B67AFE5B9C4A6BCD38AC6B199F4FA9E20F6DE3C6350`。

独立五次144输入快照及运行后实读完全同，其余141、原8、实际配置E4E33、runner5640不变，无timeout/kill/清理错误，自有进程0后归锁。非实现者批准A07b1限域整合。主仓已机械导入4260/DBDB/33C：预导入143基线FD3C、导入后144规范化全文/canonical与独立版逐项同，其他141保留主仓原raw，原8未复制/改写；主仓运行复验及交付仍待。

## 主仓复验与交付边界

上述“主仓复验仍待”为执行前快照。代表runner C832在执行前独立审查修正F213三处报告解析：缺失failure字段先守卫、0/1/多项数组在if外捕获；15纯表达式/Parser0通过。原F213未执行、不改产品或断言，其原稿和三行delta保留。root-r01同session38202已终结：Build44584 exit0/63.432秒、150/150（51.05秒）、0警告错误；Smoke16536 exit0/118.134秒，完整1000演示实际通过。

主仓新12 PASS/310检查/12清理，113条可取得任务/TCS记录均终态；较独立112仅script-failed按条件多登记一个已完成read-ui-settled观察Task，不是额外List或重跑。原19/18/10/11/16/49/身份10、原应用smoke及12页实际通过；error为空，外层nodes/colors0照旧不冒充效果。stderr15944字节为旧节点观察器已捕获诊断和外观观察，非空流原样保留。五次144输入/全文与独立版完全一致，其余141原raw/八旧夹具/真实配置E4E33保持，无timeout/kill/清理错，双树自有进程0。

根摘要`artifacts/app-pending-root/root-r01/summary.json` SHA `94F9724CA65D6CEB4704EA3B7B14A803C41E5465E447E1CBA9A2A4B4B26ED6AE`；新报告`artifacts/a/r01/artifacts/ap/a8c8b7ca/application-pending.json` SHA `36B19FE2634761386251C6D9BC396DAAE5C04306F5C7C5B8A64140909B3F125E`，DLL `4BC9D06AFA3573F62C1B0D733FE51E2884AD6E203F693AB8EEBF5D19CE232A62`。非实现者实读所有原始报告、四流和输入后批准限域交付。

交付范围为三源、README及六份正式状态/报告文档，另由未修改的官方packager从tracked快照加显式新fixture/report生成三清单；执行前后核所有源、Bundle全文、ZIP集合及字节，独审暂存范围后单功能push原私有feature。实际commit/远端与包摘要以Git及`artifacts/app-pending-root/push-complete-r1.json`为准，不从此流程段推断已push。两用户PNG、本机配置、依赖、二进制及本轮raw不提交，旧tracked AP-021材料保留、不称隐私净化。

本项不是API请求结束唤醒、应用正文六尺寸、真实安装/启停/卸载/文件效果、物理输入、原生保存或30分钟验收；旧AP-033成因仍开放。完整目标继续，下一项先用真实受控API活动复现释放通知缺口，再处理应用正文响应式。
