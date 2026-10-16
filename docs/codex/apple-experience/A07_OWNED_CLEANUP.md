# A07：本轮新装应用停止与卸载验证

日期：2026-10-16；基线e5a44bd6b6885013e2f6bac609362e167c1bb58d。承接[CMFA首验](A07_CMFA_APPLICATIONS.md)的owned待复核台账，不覆盖或重试l01，不关闭其主页FAIL。

## 背景与目标

已知首轮共享安装、精确版本/路径及启动进程成立，但Android权限请求页占前台，后续Stop/Uninstall未执行。接受权限不是验证这两项的必要条件。推荐新建显式、受限的本轮owned停止/卸载用例，保留原失败，测试各项真实效果，而非重新跑完整安装循环。

更简单的直接pm force-stop/uninstall缺少现有身份和批次链证明，拒绝采用。放宽主页断言或忽略权限页继续把全流程记绿同样拒绝。无条件finally清包可能误删后来被更新或替换的应用，继续禁止。

## 范围与约束

- 仅专用1/2且当前实际授权范围为1；固定CMFA APK与包名，原实例0及其他应用不动。不授权限、启用VPN、导入配置或改代理，不安装、不启动应用，不改变任何实例配置或共享ADB。
- 一次有界读入原ledger同一byte[]，先验显式期望SHA再反序列化，不分开hash/reopen；输入在新输出目录外，原件不改写。要求runId、固定包名、owned=true、PENDING-MANUAL-REVIEW状态、源ledger的installAttempted=true及目标/版本/路径证明完整。当前源SHA为0D815ED2C25D0E2388B224EE1A49FB704D473E1982ACB9FACFB9B5B500B19AE6，runId为544a6150088b42d8a13d3c2215ac7355。
- source target全部字段与当前manifest一致，串号及transport一致；不从编号推串号、不以名称代连接身份。旧ledger的device/LD generation属于旧runtime，仅保留来源，不跨run比较；新runtime验证后冻结本run的generation并在本run内保持。复用既有实际owner、PID/birth、admission和runtime后置transport门禁；保留初始化可能先读新transport再拒绝的已知边界，不外推零读。
- 新模式通过新PM all-users、含-u、精确versionCode/name、codePath和base.apk路径重新证明原owned。相同版本不单独构成所有权；manifest中Player/VBox/ADB的PID或birth、安装路径变化均停止。源ledger没有保存Android PID，不把旧4564/4590当恒定身份或直接按旧PID kill；Android进程现场按精确包名及冒号子进程识别。PM观察仍不是对外部并行安装器的原子锁，专用目标需独占操作。
- 仅允许共享ApplicationBatch的List/Stop/Uninstall；实际mutation入口再次验证身份与owned证据，分别在真正发送前持久化REQUEST-MAY-BE-SENT及source SHA/runId，其他操作拒绝。命令完成后回读失败仍待复核，不因Task返回就登记完成。失败不自动补偿或重试，不改原l01台账。
- 仅测试后端和共享批次，不算当前ApplicationsView真实控件/文件框/确认/进度证明。Windows抓取失败后不绕过输入限制；Android backend只读截图如有使用，也不代表Windows GUI已验。

## 验收标准

1. 输入反例（错误ledger SHA、目标不符等）拒绝在任何host/guest动作之前；原158及已有两个入口不受影响，原主页断言保持。
2. 停止前所有权及当前进程表重新证明，Stop之后精确包及其冒号子进程全部消失，安装与精确版本/路径仍存在。写前已0只能记ALREADY-STOPPED/幂等结果，不冒称实际终止效果，仍需后续活动进程用例。
3. Uninstall之后all-users普通及-u均无固定包，精确进程为0，共享应用列表不再包含该包。
4. 只有该新用例全部效果成立才登记本轮清理完成；不倒填l01、GUI、双实例或30分钟通过。源ledger原字节和输入APK保持，原配置及ADB owner/目录采样保持。
5. 独立审查/构建/PDB与实际输入核对、逐步原始输出/批次进度/退出码/新ledger齐全，所有自有测试进程终结。内部取消600秒不等于墙钟；单次外层900秒，owned终结及流读取有界。状态不确定则保留，不重发。

## 执行计划

- P0：隔离树准备显式owned-cleanup入口，独审安全边界及范围；冻结两源码，不改生产代码。
- P0：独立与主仓158回归、实际PDB/源输入核对；受控输入反例保持无设备动作。
- P0：单次实例1真实Stop/Uninstall，新效果与原失败分开保存；独审后更新报告/清单并单独推送同一私有feature，不合并发布。

## 实施与离线验证

新增正式入口`--application-owned-cleanup <absolute-target.json> <absolute-apk> <absolute-source-ledger> <expected-ledger-sha256> <fresh-absolute-output>`。只增加测试代码，不改生产、默认158项或原lifecycle主页断言。新模式先按固定SHA验证同一份有界ledger字节及全部目标字段，再创建host/runtime；实际写前重核owned，Stop/Uninstall分别记录发送可能性，Install/Start在该模式拒绝。

冻结源SHA256：`ApplicationPreflightAcceptance.cs`为AE8185DC921952864F2D3C7EBCDC8243BE84725E3752677599CF3A456CAD264B，`Program.cs`为E607C367F2D4D713B7703409C839943538CF842C5BED0EA5A34DA9092134585F；Program相对基线仅增加6行模式分发。独立树构建22.64秒/158项51.49秒，主仓构建20.79秒/158项51.33秒，均exit0/0警告错误。原158名称和顺序逐项保持，两边实际PDB三源checksum与冻结源匹配，146唯一构建输入前后保持；主仓物理142源加4根配置无额外输入。原配置、源ledger不变，自有进程0。

独立离线summary SHA `31A5BD5206CC113B4F2CC9CCBF415377A96601B81190994FE56605C168E3E1F9`；主仓summary SHA `7CC6F3C19DC63B1C2E16DD0D6BDFB5BC971CA6EE19C24C9B398E4E501BDC3111`，本机`artifacts/app-owned-cleanup-main/main-offline-o01`。新checkout换行不同于旧r3 raw的情况已区分，不拿旧源码hash证明新DLL。

## 单次实例1实测

独立审查后仅执行cleanup-c01一次；两个输入反例与正式结果分开保存：

| 用例 | 真实效果 | 判定 |
|---|---|---|
| 错误源ledger SHA | 子进程按预期exit1，仅inputs失败，无host/LD/backend/runtime轨迹 | PASS保护边界 |
| 目标名称不符 | 子进程按预期exit1，在目标全部字段一致性门禁拒绝，同样无设备动作 | PASS保护边界 |
| 重新证明安装 | 全用户普通及-u精确包存在，211035/2.11.35.Meta和非系统路径与旧owned一致 | PASS，不是原子并发安装锁 |
| 共享停止一次 | 之前主进程4564和background4590确实存在，之后精确包及子进程均为0，安装/版本/路径仍保留 | PASS实际停止效果，非ALREADY-STOPPED |
| 共享卸载一次 | 全用户普通及-u均空、精确进程0、共享应用列表缺包 | PASS卸载效果 |
| 当前GUI/主页/双实例 | 未操作Windows控件、未再启动应用或实例2，原l01主页FAIL保持 | NOT-RUN/原FAIL，不计本轮通过 |

正式PID8408正常exit0，进程21.943秒/模式21848ms，51步骤全部通过；共享List六次、StopApp一次、Uninstall一次，Install/Start/权限grant/VPN/GUI均0次。7个子进程全部终结，无超时kill或重试，所有原始stdout/stderr与51项步骤证据摘要均独立核对，自有测试进程0。没有为了清理而重跑安装或修改原l01。

新run为6f786a487eff4f4bb88eeeaaf96b45d5，新ledger状态UNINSTALL-VERIFIED/owned=false；源runId/SHA关联保留，源ledger字节及原主页FAIL完全不改。**当前测试安装已移除，下载目录原APK保留，实例1继续运行**。源ledger历史owned=true/待复核不能再解释为当前安装状态。

23条目录采样业务字段前后相同（除本地generation）；ADB实际owner62248/native birth、Player63000/VBox68124 native birth及transport8保持，146输入与主配置E4E33保持。仅证明这些时点采样，不宣称全部数据或中间状态未变。没有授权限或启用VPN，也未控制/验证应用自行联网。

## 证据与后续

原始证据仅存本机工作区`.worktrees/app-owned-cleanup-verifier/artifacts/app-owned-cleanup-verifier/cleanup-c01`，不上传APK、target/ledger原件、本机配置、依赖或raw：

- formal summary：`59D458FAFC639047D85FA00B30100085EC965A25D70A7FD0BFA2C27674B840FC`。
- outer summary：`4A86931DB4A5284B22128EDE91B05525F29D84975A094C73E02480619FA344E0`。
- 新ledger：`4961F9AD80687F2B87CEB1150080E5EB2578E42247CE0FC92D02C98466C43299`。
- independent audit：`B514352738BA021480283D75E07259B3E55D62CA163252A8CA97192932E153EA`。
- 原l01 summary仍为`C7F88DD881147EBC8AF58810E68A4D03E6D472584FB75645B4E472EDC74E5CA2`，源ledger仍为前述0D815...9AE6。

AP-003与AP-036不关闭。当前ApplicationsView文件选择/确认/进度/菜单、双实例同步及应用差异化、完整GUI失败与最终版本双实例30分钟仍需继续；本轮只验证单实例共享停止/卸载，不补签主页、GUI或全系统验收。正式源码/报告与清单独审后单独推送原私有feature，不合并发布。
