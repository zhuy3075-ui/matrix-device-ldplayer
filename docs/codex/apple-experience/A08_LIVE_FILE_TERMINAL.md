# A08f 真实终端与文件队列

日期：2026-10-17。起点`226c1e4d4963d32709b418c8ea6b5bd8ce4a3691`已实际普通推送并获提交后独审；上一goal轮为progress，文件页自有WPF局部交付不重做。完整目标仍active，剩余约13工作包，不把新增case数换算成完成率。

## 背景与目标

原终端、文件页和后端均已有局部证据，但当前版本缺少原ShellCommand、QueuePush/Pull与StartTransfersCommand在真实双实例上的字节回读闭环。本阶段直接补该证据，不再增加独立Demo来代替实机。

已知事实：Shell命令以Current为目标，上传按Targets，下载按Current；原队列固定pending逐行执行，不声称它会并发传输。AsyncCommand可因CanExecute为false而无动作返回，VM会捕获业务错误写入Error/行状态；Task返回和Progress100均不足以证明成功。Shell和原传输命令不接受外部Ct，WaitAsync超时不等于原任务终态，Shutdown也不负责等待原传输Task。

## 范围与约束

- 只操作现有专用实例1、2，名称与原A13授权保持；双方必须已经Ready，严格拒绝任意AllowLaunch=true。每次fresh核完整目录、现有5037原生PID/birth/path/SHA、双方Player/VBox出生及实际唯一Ready transport，沿用原TCP/VBox admission与epoch继承。旧manifest、transport8/9或CIM低精度时间均不永久授信。
- 不实例0、不启动/关闭/重启任何模拟器，不运行外部adb.exe，不主动创建/重启/停止共享ADB；不调用旧同步Prepare/Exercise或am start，不改变当前页面、UI任务栈、账号、权限、代理、VPN、CA、APK或硬件配置。Runtime既有getprop/wm size/dumpsys input初始化读取单独如实说明。
- API、Agent、AutoConnect及同步均关闭；固定同一Runtime且禁止重连。队列没有generation/tid字段，本阶段不扩大为重连接管合同或任意I/O交错证明。
- 使用本轮随机ASCII目录`/sdcard/Download/matrix-io-<nonce>`，双方先证不存在及非symlink，再不用-p的mkdir独占创建，并写不同于目录名的随机nonce marker。既有目录或父目录不可用即停，不覆盖。只允许本轮marker和payload.bin；本地源位于fresh artifacts不同子目录且用CreateNew，同basename、不同长度/bytes/SHA。
- 成功及失败清理均先保存首错，保原Command Task并请求原CancelTransfers，再等待真正终态。身份、marker、entry集合或所有权不明就保留残留并FAIL，不清VM Error、不递归删除、不用glob、不删父目录，不借新transport清理。排空失败保留Runtime/上游owner并如实报告，不能提前Dispose后称成功。

## 方案与取舍

推荐在现有DualSyncRun内加默认关闭的窄模式及partial文件，仅复用manifest校验、Bootstrap、CreateRuntime、Identity/epoch/目录与唯一清理骨架；旧同步动作计划保持原文，新模式绝不进入它。更简单的两次Backend直接调用无法验证原VM目标/队列，旧LiveUi整套还混有抖音前置与直接Transfers.Add，不采用。只用随机文件加test不存在存在创建竞争且部分失败难证归属；独占目录及nonce是最小可靠的清理边界。全GUI证明更强，但不能借已停止的官方捕获/错误几何输入分支凑通过，也不使用Win32/UIA/SendInput绕过。

主要失败模式：host或目标换代、注册未Ready、mkdir/marker部分完成、命令实际被禁用、VM吞错、文件部分写入、原Task迟到、清理时未知entry或身份变化。每种情形保原错误和未执行项，不重发一次已可能写入的动作或放宽效果判据。

## 任务与验收

| 阶段 | 优先级 | 工作与验证 | 状态 |
|---|---|---|---|
| 1 | P0 | fresh主仓保护与只读host目录/注册，禁止guest前核定身份 | host样本已独审；不是当前guest准入，live前仍须fresh目录/native/TCP与admission |
| 2 | P0 | 独立树最小入口、严格参数/manifest早拒、任务/清理及实际效果判据静审 | 三测试源r02已静审并冻结，仅准离线；主仓运行代码未改 |
| 3 | P0 | 原235/MCP基线、两模式严格离线早拒、精确源/PDB与compiled冻结 | 唯一Offline-r01自然0且整轮独审通过；235及23早拒通过、MCP14PASS/13NOT-RUN，仅离线门 |
| 4 | P0 | 原ShellCommand分别Current1/2，原历史新增serial/command/输出及Error/Busy核实 | Live-r02四条原终端历史及反向勾选/Current已实测并独审；仅VM-SDK，不是物理GUI |
| 5 | P0 | 同basename不同源；Current与勾选交错，通过原QueuePush两目标、原Start、原QueuePull及回读 | Live-r02两上传/两下载及六份字节原件已独审，各自长度/SHA匹配且两组不同；原队列顺序执行 |
| 6 | P0 | 原任务终态、Writes释放、nonce/type/精确entries与连接身份核实后精确清理 | Live-r02六原命令/44任务终态、14精确清理动作及两目录最终ABSENT已独审，pending/残留/窗口0 |
| 7 | P0 | 独立实际终态审、正式文档/源包/index审及单功能普通push | Live/r03及主仓离线实际终审已过；核包/index审与push以实际Git和artifacts回执为准，不合并发布 |

原ShellCommand使用固定只读命令，分别报告各实例标记、实际文件长度和SHA，必须与本地独立预期匹配且原历史增量恰一。队列严格检查CanExecute、入队增量与完整目标/path、原Command Task、行State、Error和最终字节，不以后台调用冒充本页入口。上传时Current设另一台，下载时勾选设另一台，明确检验两种目标合同。

## 证据与未验项

外层自有进程stdout/stderr原流、显式ShellHistory返回文本、传输文件bytes/SHA分别记录；SDK内部成功shell原始wire未由接口暴露，不称全部guest原始流已采集，不为补raw运行外部ADB。每动作/清理前后核同epoch/birth/tid/generation及完整非目标目录采样。

结果最多为PASS-LIVE-VM-SDK-NOT-GUI。物理GUI/DPI、原生picker、目录owned窗口、中文/控制字符输入、APK安装卸载、实际API在途冲突、任意断连/文件I/O交错、最终双实例30分钟、完整RPA/MCP/抓包及FILE-01/02剩余要求继续开放。

## 首轮静审与主机前置

- 新detached树`live-file-terminal`从226c1e4建立；基线`artifacts/live-file-terminal/baseline.json` SHA256 `0FB7021004F1D9E2A4ED2131D2306B3CA2E4A577D91780537A2B767113849BE5`保存主仓195输入/255产物、用户3、21历史proof，以及fresh树195 raw/HEAD clean blob与原33 Smoke。main仅本规格WIP，未改主仓代码/构建。
- r01仅App/原DualSync扩展及新partial测试入口，静审发现最终host-audit等待超时后仍可能Dispose其service/seed及CTS。原三源已保`source-r01`，未Build/guest，不计产品红；r02只补新模式每步后冻结pending所有权及有条件释放audit CTS，旧默认模式与同步动作不变。r02已获独立静审，准固定离线构建/早拒，不准live；源码另冻`source-r02`，未放宽业务效果断言。
- 唯一host-r01主机观察因新增wrapper“VBox也在LDPlayer14目录内”的错误假设而停止，不是产品失败。唯一CLI child36172自然0/160ms，23行完整JSON及双raw排空；共享服务同native身份，guest/launch0，host注册步骤未启动，retained0，保护保持。原execution SHA256 `D452630319C48CC83A8F40B7B5806944D821EBCF22D9C4C4A0464F9878234143`，目录stdout `F6DE8891720D7BBC2B7894C3D63901D009654A22516CB81371D2C6161E6CBFA6`保留，不重发这次CLI。
- 只读诊断与旧已审ready-r01/r02 native原件共同确认：Player在`E:/leidian/LDPlayer14/dnplayer.exe`，SHA1724211B260DB2017D01EDAD5FFBD97587623ED2F40ABAB82A995D532DA08673；VBox在`C:/Program Files/ldplayer9box/Ld9BoxHeadless.exe`，SHA C6F7F1E765FB9BE9002610196594B7F9FB7C53F147609CAEE3909FA0B9A440C4。当前两文件字节与历史相同，Authenticode均Valid、同JUST OKAY LIMITED证书CB889AD259D7B829547479857BED8BD359902817；签名不替代目标授权。后继只改角色精确路径/摘要门，不放宽任意目录或进程。
- 允许准备窄续验：pin旧成功CLI raw/JSON，fresh采双方native与owner后补一次host注册；新native不倒称r01已有出生记录。该manifest只能是旧目录样本加新host观察，不是live准入，实际执行前仍fresh核完整目录/TCP/admission。当前尚无正式guest操作或结果通过结论。
- 窄续验固定脚本`19A09FDE0AACEB439FA903C54CB233DDF6884AF205081B391361755E89E28B12`经root全文及独立静审后唯一执行，外层自然0；只新增原host工具child8380/1210ms自然0、stdout536B/stderr0、双流排空，无timeout/kill/清理错误或retained。实际注册1/2为唯一Ready transport8/9，native5037仍62248/ticks639276242287738084及原SHA，四角色进程前后同。未重发CLI、guest/launch0。execution `2E166F2564CFFCEDF6F88BE2CF1BB9146559AC427E1E8CD9A397C503382B5C94`，after `222111CA7F22C5ADD87E7D68461F011F545F8CE363E32C73F1F43EE6ED54CFFE`，样本manifest `36AC9A0BC8AE847815172AE0F621D0F3F7E56E9A1982FCED3D73C370F206672D`；独立实际审查待核，仍不是live准入。

## 离线实际终态

- 上述host续验已获独立实际审查，只证明当次主机样本；guest准入仍需重新观察，不复用旧目录授信。
- fixed Offline脚本`ECDFFAFA060D59C473366DF12C716AF8503F6BD98258C66BE5CA3447B1CB7D59`和合同`7AF308A1136B34A1D9B53829B02C94229E241B7D9FCA14A57CD0A1315220975F`经root全文读取，唯一原session54742已续至自然0（终态工具872eb5），不重建或重跑。原235名称与顺序保持，235/235用时57.28秒；MCP实际14PASS/13NOT-RUN。旧11与新12合成负例全部在runtime前精确拒绝，输入与输出边界保持，guest0。
- execution SHA256 `F91E508AD4161D7BDBA219547F02D15844BB28FB01031082AFFC18F0EC1AF005`，after `E56F8C2C018BD57BD285F3EBB4DC10A9FEB5B0D2ABD5C70746B94FB79A3A8ACD`；error/postflight均null，retainedOwnerCount0。当前196输入、255产物及冻结、三源PDB和原235/MCP前缀已获独立预审；23负例及整轮终态独审待核，不提前批准live。存在既有CS4014/CS8602/CS9124编译警告，不称零警告。
- 后继live草稿`8449C187BC1FFF09DCA501359B93C8FAC276BF85425F8BE7217D7A55934B46ED`仅准备，首行硬throw及四个PENDING pin保留，Parser0，没有创建live-r01或执行guest。本次仍不计真实终端/文件通过，约13工作包与完整目标保持开放。
- 后续完整Offline实际独审已通过：24个直接子进程/48份原流逐长度与摘要匹配，23负例均精确异常且runtime前拒绝、输入及fresh输出或sentinel边界保持；两自有Junction的目标与内容未变。196输入/255当前及冻结、三源PDB、main195/255/user3/21proof和旧33 Smoke实核保持，无kill/timeout/retained。原CS4014/CS8602/CS9124警告保持，不重复运行。
- 据实际离线原件另建live执行候选`1B950D6AE1516FECE4E76508841E9F461C688478798F009E6D859EE7684FB6DE`，仅删草稿首throw、填四实际pin及一行说明；草稿不覆盖。root全文审过原草稿和精确差异，执行候选仍待独立静审；当前无guest，也没有创建live-r01。
- Live-r01随后获独立执行准入，由root唯一session63653运行并自然1（5077ee/27adbc）。实际只产生execution `6AC612EF5BA962DEF1BA8C35F91E77B239B1AE6629B6CD81A5B3FEC5B763046B`，stages为空、liveStarted=false、manifestSha256=null、retained0；在任何host或guest前被PDB JSON整串比较拦截。root与独立verifier各用实际Portable PDB复现：三源checksum和DLL/PDB GUID完全相同，255产物保持，但Hashtable属性枚举顺序不同于已保存JSON，导致文本比较假红。归类为HARNESS/PRECONDITION，不改C#、不重Build、不计设备失败；原件保留。后继只在新r02改为必需字段及有序三源/两GUID的结构比较，仍须独审后执行，没有重发写入。

## 双实例实际结果

- 固定Live-r02脚本`4D7EC98F552A57C15BA9D85EFA6D20FF70FD9E6B473B62481C2713BFADB3485B`只修上述结构比对并换fresh输出、pin旧失败。独审准入后，root唯一原session30391已终态1（07d156/9e19ec）；五个原子进程都自然0，十份stdout/stderr原流排空，无timeout/kill/retained/postflight错误。实际live child65256用时9595毫秒、stdout577字节/stderr0。
- 程序内结果`PASS-LIVE-VM-SDK-NOT-GUI`，11阶段完成、14步骤文件、六个原命令Task全部RanToCompletion；四条原终端历史、四条原队列逐设备/path/方向核对。上传Current为另一台、下载勾选为另一台，各自按原合同执行，没有把SDK直接调用冒充原传输队列。
- 实例1三份source/returned/cleanup-readback均32769字节，SHA256 `352514851436BD13B959C9562672D3B2B0717EB7B318B4C2AFC367CFB79390BE`；实例2三份均65537字节，SHA256 `834AA452810D7E28587478D5892FE9BD75EE1CC1970FED2AB0BBE93242A310F2`。独立verifier逐字节重算既定模式，不只比较报告内摘要；两组内容不同。原队列是顺序执行，不称并发传输或压力测试。
- 两独占claim实际CLAIMED；14条精确清理动作逐次核nonce/type、NUL分隔项目集合、回读后删除固定payload及marker/rmdir，最终两个目录ABSENT，无残留。44 owned任务/pending0、audit token已释放、VM唯一关闭/RuntimeDispose完成，窗口0。113条完整目录身份记录、462次native probe及116次内部LD list2无偏差；非目标21实例状态与原5037/native角色身份保持，没有启动/重启共享服务或模拟器。
- 原outer1来自只读报告校验的第二个schema假设错误：`TransferRow.Size`本来返回F1 KiB显示字符串，wrapper却按原始字节整数比较。实际四行是32.0 KiB/64.0 KiB，符合原`Presentation.cs:107-112`，不是文件大小错误。原outer失败不改写、不重发guest；只准备纯文件续验修显示类型判据，其它六文件字节与任务/清理断言不改。
- 原件SHA256：execution `22B2D76C282B19B99C4C7CE2E27AFEE95C9575C2A842B16A506566469E4F5220`；after `2FAB66B3FF78C0CA007A0FE3B2CD5E6A05AD1129CFD5D89FAA9AA44A96E6632B`；程序summary `D233AE590650C22AA331FAE1C92B333903EC5D2BDB1C5CE2768A6FC137FF4601`；原results `2E6E878F2D405EB97FD6DE199F3D1A67B451454CB058BDFF4D64D833A828A42E`；cleanup `E13E48B5B5A6DBE0D5DAF09562391B8056B9E4A264771077BFFFEFF482FFCCB9`。实际全轮已获独立有限审查，无产品/cleanup拒点；不是原outer0，也不是物理GUI/完整目标通过。
- 所有必需exec已终态。下一步只读报告续验、精确三测试源集成主仓及离线代表验证、核包与单功能push；不重复真实操作。约13工作包、完整GUI/文本/APK/RPA/MCP/抓包及最终双30分钟继续开放。

| 用例 | 实际步骤 | 预期 | 实际/状态 |
|---|---|---|---|
| LIVE-IO-01 | 分别设Current为1/2、勾选另一台，经原ShellCommand读取本次标记及机型 | 每次只新增一条归属于Current的历史，标记精确、机型非空、Busy/Error恢复 | 两台均符合；后续文件校验命令后总历史4条，PASS-VM-SDK |
| LIVE-IO-02 | 两个本地payload.bin内容和长度不同；各自勾选目标而Current设另一台，原QueuePush入两行并原Start执行一次 | 上传只按勾选，路径/serial不落到Current，无安装副作用 | 两行完成/100%，路径与目标准确；后续实际回读证明异源隔离，PASS-VM-SDK |
| LIVE-IO-03 | 分别将Current设1/2、勾选另一台，原QueuePull入两行并原Start执行一次 | 下载只按Current，返回各自字节、不混用同basename | 两返回文件逐字节等于各自源且两组不同，四原队列均完成，PASS-VM-SDK |
| LIVE-IO-04 | 对各Current通过原终端执行wc与sha256sum | 远端长度32769/65537和SHA各自匹配，原历史/命令/输出可关联 | 两条原命令、两组真实远端输出均匹配，PASS-VM-SDK |
| LIVE-IO-05 | 原命令排空后逐台核nonce/type/NUL精确条目，清理回读核字节，再删除固定payload/marker并rmdir | 不递归、不碰未知文件；两目录最终不存在、身份未换代、无待定任务 | 14条精确清理动作、两份额外字节回读及最终ABSENT已独审，pending0/无残留，PASS-VM-SDK |

这五组用例不包含物理GUI点击、传输取消重试、错误/断连交错、APK、中文文本或长时间运行；这些项目未被本表的PASS覆盖。

## 报告续验与主仓集成

- 纯文件续验脚本`6F2E9AE8AD890B3E1A04BBD3C41BFD9F0F982C43495AFC8C58507EDE7331B25E`和输入清单`FEC6856DA1D65D857E33DC7FA874AB5F4CB17563A3681345FB97D6493F018DEE`冻结全部56份Live-r02原件。仅将Size判据改为string/原F1 KiB/固定32.0或64.0的Ordinal比较，六份文件的实际bytes/SHA及其它任务/历史/清理断言不改，不加载产品/host执行函数。
- root唯一续验session8468自然0（43e1ba/3fe4cf）且独立实际终审通过：`PASS-READ-ONLY-REPORT-CONTINUATION`，error/postflight为null，新增product/host/live为0，原outer1保持。execution `737E3A988E170F40024856098ECD6B87F4AA3FC4D0B2108EC70FE0BE2981BC10`；after `3B0D0765946233D66A00A509C5B71F8EBF4AB03F4FBE3A12B4FBD3C0029062F6`；report `97CCDB068429FFCCEFE42B7549CE7E656C3BA701B7B3015BDE93A3B7CE847E26`。仅启动这个PowerShell审计宿主，不产生产品或设备操作；56原件和全部保护保持。
- 获独立准入后，root机械集成App/原DualSync扩展/新partial三份冻结源（4234/CEBA/A022），主仓旧两源另备份并CreateNew记录`artifacts/live-file-terminal/import-r01/copy.json`。实际196输入，另外193原raw、旧Main255、用户3及21历史proof均前后逐SHA保持，未改变业务VM/backend或默认同步模式。
- 固定主仓离线脚本`E7102F18524AB0A3E364CDA27B27E8C563775C67EB5BD9DF91DF97EB0E554F8A`全文与依赖差异经root和独立静审后，唯一main-offline-r01/session79914（初6f0424）正在执行。只有原235/MCP14+13、原23合成早拒和PDB三源/实际255冻结，不发guest或新的host观察；必须续原handle至终态，不重复Build。合法主仓新产物不能再要求等于旧Main255。

| 问题编号 | 范围/严重性 | 原因与处理 | 状态 |
|---|---|---|---|
| HARNESS-A08f-01 | 测试基础设施/P2，非产品 | PDB对象字段枚举顺序被误当身份；r02改严格结构比较，三源行序/字段/两GUID不放宽 | 已修并独审；r01原失败保留，未触发host/guest |
| HARNESS-A08f-02 | 测试报告/P2，非产品 | 原Size显示字符串被错按字节整数比较；纯只读续验匹配getter，实际六文件字节仍精确校验 | 已修并独审；原Live-r02 outer1保留，无设备重发 |

## 主仓终态与交付边界

- 上述唯一Main session79914已自然0（终态7b019d），并获非实现者实际整轮终审，无拒点。Build child29896自然0/116685毫秒，原235名称和顺序保持、235/235用时56.94秒；MCP实际14PASS/13NOT-RUN、14会话/42份原流与原27类结果核实。存在原CS4014/CS8602/CS9124警告，不称零警告。
- 主仓旧11与新12合成负例全部精确拒绝，严格唯一CP936解码、含正确主仓路径和Run/OnStartup且未进入Bootstrap；manifest均为server1/birth1和fake serial。24直接子进程/48份原流自然终态，无timeout/kill/retained，error/postflight显式null，known Matrix进程0，guest0。两自有Junction/sentinel及输出边界保持，没有为了合成负例连接设备。
- Main execution SHA256 `FBA40DD5E318FFA85475FD252876FD3F4C8BCE0DD77696481BD05F20DE6BCACD`；after `907372FE706CBF23F4213EE307AAB28A2C14AA463429878084F2E02978F6C75F`；PDB回执 `364E5B1F5774A565BBE28D7045108EDBB7886245F57B4027F18014287ED00873`；新255产物清单 `5E07152636CF80E2D83B4619A9C908614045BA9E116D7C974B04D826514BEEF5`。三源checksum与原冻结raw相同，实际Main PDB和唯一DLL CodeView均为`501ee4b8-057a-4a5a-907f-e6cea9eba64b`；这是合法重建的新身份，不要求等于隔离树旧GUID。
- 当前196主仓输入/其它193原raw、主仓新255及冻结、用户3/21旧proof/33旧Smoke、隔离196/255及冻结、原Live56份文件、r03五份回执和r01失败原件均经实际终审保持。全部必需exec已终态，不再Build、重跑或发设备动作。
- 本轮仅三测试入口源、README及本阶段正式记录/任务矩阵和三生成清单。包/精确12路径index及正常private feature推送另行审查，以实际Git与`artifacts/live-file-terminal/push-complete.json`为准；不提前写提交SHA，不合并/发布/清理worktree。原始证据、实际授权配置、APK、SDK与bin/obj不进入本轮提交；继承82份tracked历史验收材料的AP021分类继续开放，不声称完成隐私净化。
- 后续继续真实文本、当前应用页安装卸载、完整GUI/DPI与原生选择器、RPA/MCP/抓包和最终版本双实例30分钟。此阶段约9.6秒的设备操作不是长期稳定性或并发压力测试，不减少整个约13工作包为局部用例数，完整目标仍active。
