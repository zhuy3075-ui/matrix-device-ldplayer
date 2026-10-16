# AP-042 官方客户端读取前后对照

## 背景与目标

前一阶段已实际推送`91beac619dd09847263be4a66b1926e7f033e2c5`，属于progress：原位诊断的独立/主仓各193及VM4通过，原AP042仍PRODUCT-FAIL。回执`artifacts/workspace-cache-main/push-complete.json` SHA `402B94752E6538783EC7ECB23375589680B2C112A8A92D3740ACA51D8824130F`。不重复核包或运行该阶段。

已知实际visual正文切换而原peer返回旧树；14个只读快照中的Window既有peer为null、listener为false。这不证明唯一根因。官方WPF存在局部peer与窗口根提供者不同初始化路径，官方Sky允许纯文本读取，但未公开内部注册位。本轮只检验整个官方观察流程条件是否影响原三阶段，不先改生产peer或把外部工具初始化作为产品依赖。

硬约束：原AP042源码、Product/Require、五任务、5秒ApplicationIdle Flush和原遍历保持；AP045输入分支仍停止。不能通过Reset/Invalidate、私有反射、原生UIA/Win32、额外GetChildren、激活或截图绕过。

## 范围与方案

推荐独立测试入口`--probe-workspace-client`，只接受显式`--demo`、fresh绝对artifacts `--data-dir`、32位十六进制`--nonce`、`--arm client|control`及有效`--width/--height`。未知、重复、缺值、混用旧模式、截图、真实配置或已存在目录全部在创建目录/Runtime/窗口前拒绝。仅App入口两处路由及新增测试文件可变，旧生产链和全部既有Smoke不改。

1. 构造自有Demo Runtime，不加载用户配置；强制API/自动接入/Agent/输入同步关闭。创建同一真实MainWindow，ShowActivated=false；原RefreshDevices/UpdateUnifiedDirectory完成后，设置带nonce的唯一测试标题。
2. 窗口Loaded/Visible且业务调度门满足后，写独立ready记录。记录nonce、arm、PID/创建时间、实际程序集SHA、标题、尺寸、UTC与单调计时起点；只读现有Window/tabs peer、listener和窗口状态，不创建Window peer或查询子树。
3. 异步轮询同目录release.json，不阻塞STA。门总限180秒；文件必须一次完整、长度有界、nonce/arm/PID/标题与ready一致，成功或abort意图明确。错误、超时、用户关闭、未知字段或非匹配文件立即失败，不重新尝试到绿。门前后记录现有peer/listener和实际单调elapsed。
4. client由根代理使用已安装官方`@oai/sky`。只调用`list_windows()`选择唯一实际返回窗口，再一次`get_window_state({window, include_screenshot:false, include_text:true})`。不用get_window/activate/输入/截图，opaque窗口ID不当作PID/HWND。读取成功、文本非空、目标一致且无意外截图后才写成功release；否则abort并记录外部失败，不运行原三阶段凑证据。
5. client正常释放后调用原`VerifyWorkspaceAccessibilityAsync`，无几何或后续诊断。原失败保持exit1，报告与清理仍写出；新门失败与Product失败分别分类，不覆盖彼此。
6. 若client外部条件成功，另建fresh control。本arm不调用Sky，沿用相同门；以client实际ready→release单调时长作为目标，应用内部实际时长偏差须不超过500ms。误差超限则比较条件无效，不重跑。系统客户端/helper是否观察新窗口无法排除，初始peer/listener不等价时不作初始化因果结论。

更简单方案只重复旧fixture无法区分条件，不采用；额外selection gate只能区分枚举与读取，而本轮研究整个观察流程，故不增加。拒绝强制清缓存、外部激活、标准容器偷换或放宽Product。即使client绿/control红，也只支持官方观察条件影响该行为，不能关闭旧失败或据此把产品依赖外部客户端。

## 生命周期与证据

协议文件都在该次data-dir根。ready包含进程启动UTC字符串与程序集SHA；其原字节SHA由release和probe-summary的`readySha256`记录。release严格恰9字段：`nonce`、`arm`、`pid`、`processStartUtc`、`assemblySha256`、`title`、`readySha256`、`intent`、`reason`。intent只接受`success`或`abort`，后者reason非空；外部须完整写同目录临时文件后无覆盖原子发布。`probe-summary.json`中的`elapsedMeaning=ready-to-validated-release`及实际elapsed才可用于时长对照，不能用发布脚本的时刻代替。

- 两臂使用独立fresh目录、严格nonce和自有进程。测试采用显式VM/Runtime清理及AbortStartup，不写用户配置，报告为受控中止关闭而非normalClose；Runtime只由一个所有者释放一次，不再进入原App catch重复清理。清理限15秒并记录底层任务状态；超时不称已排空，交外层兜底。清理异常另记，不吞原失败。
- 外部runner每child300秒，仅终止自己持有的进程句柄，kill后5秒退出、双流各10秒排空；一切原始流与实际exit保留。窗口归属不唯一、桌面锁定、空树、显式错误、截图意外返回或清理失败即停相应路径，不转原AP045输入。
- 不触实例0、模拟器/ADB/账号/权限/VPN/CA；不下载组件，不读取helper内部实现，不运行未知APK/脚本。本轮不替代真实GUI安装卸载、双实例同步或最终30分钟。

## 任务与验收

1. P0：只读设计审查后在独立worktree实现；App严格入口与新测试类之外零源码diff，冻结旧193名序、156输入及25Smoke（App为批准例外）。
2. P0：独立审精确diff与清理；Build193通过，并一次实际验证无Demo/混mode/截图/既存目录的入口拒绝，证明无新目录/窗口/runtime副作用。
3. P0：冻结二进制与源/PDB，独占执行一次client官方纯文本条件；满足条件再执行一次匹配control。原Product、外部调用和门条件分别报告，不重复直到绿。
4. P1：独立核原始流、调用结果、源码/PDB、门与清理。若产生可交付测试能力，则文档/清单完整核包后单独推原私有feature；实际Git决定状态。产品修复另定规格，不混入本轮。

## 当前状态

新入口已在独立实现/验证工作树完成：App只增加最早分支及专用catch共15行，剥离后原链全文不变；一个新测试文件210行，原25个Smoke与155个其它编译输入保持。App SHA `74AA6AD185E066BF6BD8BA197D24BA7D3AA64B5118DCC2562306A2ACCA1A8487`，新测试源 `5975F73A89D999AA7E2D5B244D8082FB120FE5B51953258982741708D8E930C7`；原AP042仍 `66EFC759A95218E8701B336DF67ED664A58D8EF632A0ECB1BB24FFFEB5AF0560`。

独立预检Build61608 exit0/77.188秒，原193项名序全过，实际PDB39唯一源校验；四拒绝child66048/79112/23272/33992均exit1且各不足200ms，原ArgumentException/Parse→App栈明确未进Run。三个新profile与截图未产生，已有目录sentinel保持。五child/十原始流全部终态、无timeout/kill/清理失败。execution SHA `17B44B2078570E21CAEB442D440AC13AB39379551C3134B5E3FA625FC4613C7A`；批准清单 `E4F339C45AFACD35A9E140689370CCAEAF41B1CE4B21391E0A19F2B7F540D744` 包含17顶层及46递归文件，不将17称全覆盖。实际Desktop DLL `3021DB406742C46968946AE9F4F5536BE4179F03519EE14A7C792C8356529217`。

## 两臂实际结果

| 条件 | 实际过程 | 原AP042 | 收尾 |
|---|---|---|---|
| client | PID43316，ready→validated release为33800.9152ms；官方一次枚举得到唯一nonce标题返回Window，再一次纯文本state，116个索引节点含真实设备正文、screenshots0 | PASS，146→71→146 | child0，受控AbortStartup，VM清理1次并完成，双raw均0字节 |
| control | PID18724，本arm无Sky调用，实际33902.6336ms | 同PASS，146→71→146 | child0，同单次清理，双raw均0字节 |

两臂初始公开条件相同、均Loaded/Visible/390×640/非活动/无焦点，已有Window/tabs peer和两类listener均false。释放前两臂却都出现Window/tabs既有peer、StructureChanged listener为true，PropertyChanged仍false；业务上下文保持。实际时差101.7184ms，小于预先冻结500ms门。

这不能证明显式读取是通过原因，也不能证明单纯等待足够；全局helper/系统客户端是否观察control无法排除。Node既有Sky已加载，本轮未重启或清理helper。没有get_window、activate、点击/按键、截图、guest/ADB动作，也没有在control上补读窗口。原位诊断阶段的Product红保持，当前生产peer未改；不把两次绿倒填成原失败已修复、完整GUI或实机通过。

client summary SHA `A9C06543167B6DF7907B789E148F297E514737F57DE3647A954E21B4792532E4`，execution `1E8AEDBD0FFAEFFDADD8DC20F587871E163362F6F10B4109AB0F11F408BCAF5B`；control summary `507CEC3DC618D4E23D3BF3F73080F47AE340CB59F8E8BA4C18997D6588AB6B00`，execution `3FDE153B219F1084A307846230538FBC5917A1B09DF3E8F5A71C3FE13D230113`。实际官方观察原件 `859BED96D1F159CDEE9347DD6F30880BB3336437C39CE338751323F94E2ED1C2`；强制事后比较 `8AAF9F0A28DFB808FDC3D47B71035FD72641F5E112EE9390124A5E796542258C`。原件仅保留本机 `artifacts/workspace-client-main`，不上传。

外层RunProbe在真正启动之前另有一次清单数组顺序比较拒绝：46行集合及每项字节/hash均同，未创建profile或进程。保存原helper及prestart-failure后仅将两端按同一relativePath排序；新冻结helper才真正启动上述两臂。不算产品红、不重新编译、不重跑原失败凑绿。参数发布/实际500ms门的脚本也保留静审修订记录，未改C#或原Product。

## 当前裁决

独立预检、完整两臂实证与主仓编译输入审查均仅批准严格测试能力有限交付。根已机械集成精确双源；一次Build子进程44200 exit0/73.093秒、0警告错误、原193名序全过/51.03秒，实际157输入全文同独立实跑、155原输入与25Smoke保持、PDB39核对通过。根Desktop DLL `F889C1126CD7FA8446EC4A19C8AEB267733BA987834CC1879C3E05264BC00861`，execution `903A0CF645489E3EBF9A9BFBAA45479D35818C97E32D70E35D33BF75E0CCC969`。

但主仓wrapper外层实际exit1，不能称整轮全绿：根在构建期间修正文档中的ready SHA措辞及阶段状态，导致唯一非编译spec护栏从E28452F7...DA9B5F变为93D47E9B...912703；十个编译guard不变。后置检查正确报`HARNESS-DOCUMENT-GUARD-FAIL`，原始execution与护栏记录SHA `FC4EC4D43C2047A32E13B4F1A27391B96B9D8206E5746EB3BF78DA6EA91D0ACC`保留，不改写原PENDING字段或重跑凑绿。另保存按已知两段修改逆向重建、SHA确实等E284的旧doc，明确标为hash验证重建而非原始运行前快照。独审将实际编译/193通过与外层文档冻结失败分别裁决，只接纳源码/测试有限证据。

全部必需进程和流已终态，source/测试断言未为通过而放宽，配置与两用户PNG保持。本入口尚待完整核包/index审核及实际单独推送；实际Git决定交付。两次组件通过不足以解释原失败，保持生产peer、AP042旧失败及AP045输入分支；不追加另一轮寻找绿测。下一安全业务项为A14a2逐目标作业身份/状态/取消，原全部GUI、双实例同步、应用GUI安装卸载、最终30分钟和A08-A16继续必交付，完整目标active。
