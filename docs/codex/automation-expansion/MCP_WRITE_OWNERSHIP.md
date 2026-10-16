# A15c 共同写入占用

日期：2026-10-17。基线13f3514，承接A15b有限Demo交付。本阶段是后续真实MCP的必要条件，不代替真实接入、授权、GUI或完整目标验收。

## 事实、范围与选择

当前Scheduler只互斥脚本，API和UI的busy是先查后写，后端队列只串单条命令。API已通过检查后，脚本仍可能进入同目标；上传与自动安装之间也不能靠逐命令排队保证完整操作不被抢占。

推荐同一个MatrixRuntime拥有一个DeviceWriteCoordinator，实际完整操作入口一次取得明确目标集合。锁内只验证和改占用表，不await、调用后端、取消或通知；忙则立即409/1003，不排队。组取得全部或全部拒绝，验证完整后才改变表；释放只处理本次lease，重复Dispose无害。IsBusy只用于提示，不能作为接纳依据。

无法确定serial的LD操作使用真正的AcquireAll全局屏障，已占用任一目标时拒绝；屏障存在时之后出现的新serial也拒绝。正常明确设备不使用全局锁。更简单的全局互斥会阻止不同设备并行，拒绝；仅加UiWriteBusy检查仍有竞态，拒绝；不在Backend或服务内部隐式重入，不使用AsyncLocal或另一套调度器。

范围为同Runtime现有设备写入口。跨进程唯一owner、Desktop承载MCP、IPC/grant、运行时换代排空与真实1/2另阶段实现，当前real继续拒绝。外部ADB和任意调用公共Backend的第三方不受此门控制，不能宣传为系统级沙箱。

## 入口职责

| 调用路径 | 唯一持有者和范围 |
|---|---|
| UI/MCP SubmitJob或legacy RunAsync | Scheduler，单目标完整执行、异常及自身同步清理；外部Completion覆盖释放，不等待另一设备占住的全局观察通知队列 |
| UI SendInput到InputCoordinator | 仅Input持有最终目标组，保原lane排队；Tap/Swipe/Key/Text或Up的实际ADB动作，Down/Move/Cancel只本地缓存 |
| UI文本、Shell及旧应用入口 | VM在冻结目标后取得一次完整组；确认前不持锁，确认后重新检查/取得 |
| 新应用批次 | View在确认后、原Batch执行前取得所有变更目标，覆盖写前/写后列表核验；纯List不取得写lease |
| 传输及自动安装 | VM冻结本批次队列和目标组，覆盖Push/Pull至AutoInstall结束；取消等待原操作返回才释放 |
| HTTP basic/automator写请求 | API整条业务动作，包括等待节点后点击；GET与只读动作不占写入 |
| HTTP/UI LD实例命令 | Runtime映射serial取得单目标，否则全局屏障；保留原0保护和generation/PID/发送前identity检查 |
| Unicode Text内部进入LD service | 外层原lease继续覆盖，内部不再次Acquire |
| 冷启动、接入、新建和资源配置 | workspace入口一次全局屏障；下层ApplyLdConnections及old Runtime Dispose不再Acquire或等待自身lease |
| 工具页时区setprop及回读 | 对冻结的当前目标单次占用；本地备份导入/导出/删除不访问guest，不新增占用 |

UI StartScript不在外层取得AB，保留原逐JobId接纳和部分失败回传。Input只保证一次实际ADB动作组的占用，不声称从物理Down到Up全程独占；忙拒绝Up须清除本次匹配手势，后续重复Up不得发送陈旧动作。监控、截图、节点等只读仪器路径仍可用，不把其内部临时文件等同应用控制写入。

目标明确的UI操作不再通过UiWriteBusy全局阻止无关B；显式外部busy、未明确目标的工作区操作和旧公开UiWriteBusy=true仍保留全局拒绝合同。应用只读自动读取的既有排空门继续保留，不借本项改写刷新行为。独立TransferQueue当前未被Runtime/Desktop实例化，登记为库组件，未来接入必须传递同门，不能从本项推断任意库调用受控。

## 失败模式

- 同栈二次Acquire造成自拒；按上表唯一owner，不增加隐式重入。
- 取消或终态事件过早释放；停止请求不释放，实际操作/原清理结束后释放。
- AB部分取得、验证半途异常或旧lease清除新lease；先完整验证，再锁内一次接纳，释放核原持有者。
- Input各lane先后取锁造成部分写或组互等；按原单Worker顺序完整入队，用同Work组准入保留FIFO，所有原任务排空后释放。
- 外层LD操作持old全局屏障又等待old Dispose的全部lease；本阶段不增加这种自等待，跨Runtime seal/drain留后继独立规格。
- 资源身份改变或目标别名变化；本门不替代原generation/transport及发送前核验。
- 将全局观察通知的排空绑定到每台设备Completion会新增跨设备循环等待；保留原发布语义，不把任意观察者回调当设备后端动作。

## 任务与验收

| 优先级 | 任务 | 验证 |
|---|---|---|
| P0 | 原13f基线实际竞态红测及新门合同 | 现有HTTP进入自有TCS后，另一脚本同A不应进入后端；原行为实际记录，缺新API单列合同未实现，不用编译失败冒充业务红 |
| P1 | Core/Features/Host共同门 | A竞争恰一方接纳、B继续；AB拒绝零残留；global与未来key双向排斥；旧lease重复释放不影响新持有者；legacy与JobId共享 |
| P1 | Desktop完整入口覆盖 | 文本/Shell/应用/传输自动安装/LD/时区按上表取得，Input/StartScript不二次取；保原交互及信息结构 |
| P1 | 取消、异常和原队列 | backend忽略取消仍未返回时A仍忙；完成后可新取得；抛错释放；Input忙Up重复发送无旧动作；读取仍可用 |
| P1 | 独立集成与主仓回归 | 原209名序不变、新focused用例另计、原MCP14及13NOTRUN保持；实际编译源/PDB与原流终态，不执行真实设备 |
| P1 | 独立审查与单功能交付 | 只相关源、正式测试、README/报告/完整源码清单，排除本机配置/二进制/raw，推原私有feature，不合并发布 |

## 执行记录

上一阶段已实际推13f3514885ac036bea8bb7200be96598748bd03d，远端同值/test49e保持；本地回执artifacts/mcp-stdio-main/push-complete.json SHA93AD60E632F5692172EABF321029C1FD031ED59D7560E91738A00194C49565DA。上一goal轮为progress，不重复既有209/14或核包。

已建立四个独立worktree：write-core仅Core/Features/Host实现，write-desktop仅Desktop实现，write-tests仅正式测试，write-verifier供独立集成；均从13f起。管理工具因当前聊天父目录不是Git仓库而失败，实际用非破坏性git worktree add，不复制用户配置。

2026-10-17续轮：上一条只回答剩余数量，按no-progress处理；本轮重新核实文件和已终态handle后继续实现，不重跑原红以累计数字。write-tests/frozen-r01保留实际基线红：原209均PASS；新13中3PASS、6业务FAIL、4合同未实现，总212/222；API已进入后仍接纳同A脚本、HTTP取消但后端未返回时仍接纳、忙B时Input却写A等均有真实Demo调用记录。13例cleanup pending均0，无强杀，构建与测试自然终态；r01a只加固未触发的清理失败路径，原断言不变，不能称该异常路径已测。

Core首候选仅compile-only通过（28.223秒，3既有警告、0错），独审随后发现新增notificationDrained会使B完成等待被A观察者占用的全局队列，要求移除该新增耦合，并新增独立W14回归。该候选未获行为批准，旧冻结与编译原流保留。Desktop新增无窗own-Demo VM验收，覆盖文本、Push到自动Install、取消未返回、组拒绝、Shell异常及忙Up清缓存；它不创建Window或操作真实设备，不能代替ApplicationsView选择器/确认或完整GUI。当前尚未完成集成绿测与主仓集成。

第二次独审确认Input候选另有FIFO自拒：SerialCommandQueue先释放已完成A的lane，而AB的lease仍等B及外层WhenAll；后续只A操作已经入lane，却撞自己前组保留的A。r03在同Work内增加结束屏障，所有实际callback与同步错误通知结束后，最后参与者先释放组lease，再放行各lane；未进入callback的取消/失败只计一次并唤醒其余参与者。不改全局Worker为串行。W15以A已实际完成/B仍held的AB到A反例验证，W16验证已入lane组取消不早释前组；新增观测只用于自有Demo后端。

执行前置失败分别保留：core-red-r01在FilePins空集合时读取目录Length失败，无子进程、构建或业务执行；仅存工具结果摘要，未冒充原始raw。窄修空输入早返回后，独审独立离线核null/empty/single三个合同通过。core-red-r02真实Build PID1996自然exit1/19.127秒，App新入口把StringComparer传给Equals而报CS0176；仅改为显式string.Equals及StringComparison，业务225和VM5仍NOT-RUN，不能计FIFO红。该轮双流final/drained、无kill/pending/retained，main186/148及用户3保护均保持；旧runner/源冻结/构建失败原流不覆盖。

core-red-r03实际完成：使用冻结Core r02、旧五个Desktop业务文件及新入口/测试；Build6776自然0/14.340秒，Unit84648自然1/52.142秒，原209名序全PASS、新16仅W15 FIFO FAIL，总224/225（51.82秒）。VM54240自然1，五例均业务FAIL，包括AB忙B仍真实Text写A/B、脚本结束后重复Up实际Tap；并非夹具初始化失败。16份单位清理与五份VM原任务均终态、无kill/timeout/retained；九个实际PDB源码SHA与冻结相符，pdb-nine.json为5E13D95A13C8D1881D59FC9B79CFE5822F13F0FB1839A6F428845FE612CEA112。外层0只表示如实识别预期红，不能当产品通过。

最终Core r03、Desktop r02及原16测试共14源逐冻结SHA机械集成到独立verifier，原红保留。复制保留旧时间戳可能误用增量产物，因此只更新目标14文件修改时间、不改字节，并核实际PDB。green-r01原session59782已终态0：Build81944自然0/114.556秒、225/225（56.67秒）、MCP Demo14/13NOTRUN、VM5与三早拒通过；无kill/timeout/retained/error，156实际产物已先冻结。两独审实读原流/业务门及5个实际Portable PDB的14源，批准限域集成。execution F05BFFE295967F6BF9E3BD5FC306F277A24BFE893C554AF8753C4E5A07F9907C；frozen-products A7E5EBE11DAED21B814AA892E296D1F204C4A9AB13A03A2398F278357542AE99。

证据边界：红轮九PDB文档由root在终态实际读取并核SHA，但未另存旧PDB二进制；后继绿构建已替换current bin，独立reviewer只能核原PDB报告SHA，不能再复读该轮原二进制。因此不称红轮PDB已独立二进制复核，也不以其他worktree产物补作原件。红轮原始双流、前后source guard、具体调用与清理仍可独立审计。绿轮终态后先冻结实际DLL/PDB，再进行独立复核和主仓整合。

主仓14源已精确纳入，原175其他输入及用户3保持，完整189输入冻结于approved-main-r01.json（15F5FE803293FFE0C29868659EC36465695A2834D9EA5B481C0A1AFB4666E222）。Main runner FD758A280051B3F841403DF3341CF126767806810EDBB4DBB14895838E97CC8F获独审单次准入，原session81092/Build57120已启动；当前主仓最终结果待终态，尚未提交或推送本阶段。MCP十四组不代表十四个正常EOF：独立审计实际13会话exit0、重复RPC id负例session04预期exit1，227调用与原wire逐id对应、13终态job及78协议raw实核；旧13类NOTRUN/real/GUI不变。

主仓实际终态更新：原session81092已outer0，Build57120自然0/114.452秒，225/225（56.51秒）、MCP Demo14/13NOTRUN、VM5与三早拒通过，3项既有编译警告/0错误；五child无kill/timeout、双流final/drained，error/postflightnull/retained0。execution760F684E33543ECEC60039A523F2D5E349D6A59AACF7B131DFD507061CE092B5，MCPsummaryAF4950511DD66F583B689461B3C6E5EEB24D1ACF3A845960AFC1CF0FAEB9E881；实际156产品已先冻，frozen-products C4F362FD01D889C7F749E9CAB89AFB022DAEBCA6CF1D367B38FF3F4F286052D0，14源实际PDB及各原流/业务门均经两独立终态审通过。当前Desktop DLL1F010BF47D9C76481F3693612731D4223C49D29E7FEF8194F3779E43467341D5/PDB B124F7027E6AEABE162CFE6DCE4685925C76FBDAE4A847C7F521347EBD2F9A47。不重Build，下一门仅文档/核包/精确index与单功能private feature交付；是否已推必须读取实际Git/回执。

主仓独审另实核27个staging ZIP精确对应27次成功导入。当前合同退出只关闭已提交FrozenHandle、不删除这些冻结ZIP；cleanup0表示任务/SDK/流及handle排空，不能写为staging已清空或文件已全部删除。文件仍在自有测试data/artifacts下保留供取证，不进入本次提交。
