# 当前目标交接

更新：2026-10-16。完整目标仍active，未达到全功能、全控件、真实设备、六尺寸和最终稳定性完成门槛；不要调用complete或把离线通过换算为实机通过。

## 最新执行：CMFA安装成功，权限页阻断后续

本节优先于下方历史。用户提供的固定CMFA包已在专用实例1真实安装一次，版本211035/2.11.35.Meta、非系统base.apk路径及本轮owned证明成立；共享Start一次、主进程4564与background4590存在。但Android PermissionController GrantPermissionsActivity占前台，CMFA MainActivityAlias为PAUSED，主页效果断言FAIL。没有截图、Stop、Uninstall、grant或VPN操作，不把启动请求成功当主页成功；具体权限种类未确定。详见[A07_CMFA_APPLICATIONS](A07_CMFA_APPLICATIONS.md)，AP-003仍开放、AP-036为测试前置阻断，不归产品缺陷。

安装仍保留，ledger owned=true、PENDING-MANUAL-REVIEW-NO-AUTOMATIC-CLEANUP。原件在`.worktrees/app-preflight-verifier/artifacts/app-preflight-verifier/lifecycle-l01/lifecycle/installation-ledger.json`，SHA `0D815ED2C25D0E2388B224EE1A49FB704D473E1982ACB9FACFB9B5B500B19AE6`。不得重跑完整安装模式覆盖它；下一步先重新证明冻结进程/owner/transport及本轮精确版本/路径，另立受限Stop/Uninstall用例，或恢复可靠观察后在真实GUI拒绝非必要权限。保留本次FAIL，不自动清包、不放宽主页断言、不代用户授权，也不借其实现VPN抓包。

两个正式测试源已主仓整合：fixture CAD755...D1360，Program993914...D8D5D，仅两个新模式分发，不改生产及原158。r3独立158/51.16秒、主仓158/51.43秒，构建均0警告错误、PDB三源一致、146输入/config前后保持；原158名称顺序与物理输入集合也核同。r2正则误匹配已在测试证据逻辑中最小收紧，不能当生产功能修复。r1只读17步骤通过，外层StrictMode空字段错误原样保留；l01正式exit1/21.480秒、37步骤通过后主页FAIL，5子进程全终结、无超时kill，自有测试进程0。23目录采样及ADB62248/birth/path/hash/transport8保持，不外推所有数据无变化。

当前无必需运行的exec子进程，执行锁归root。Windows Computer Use抓取两次超时后已停止Windows输入，不绕过Sky使用Win32/UIA/SendInput；当前ApplicationsView文件框/确认/进度、双实例同步与应用链仍未验，AP-033及最终双实例30分钟、A07余项/A08-A16继续active。正式测试、脱敏报告及源码清单限域核包/独审后推送原私有feature分支，不上传APK、配置、target/ledger原件或原始证据，不合并发布；实际提交以Git记录为准。

## 早期准备：用户提供CMFA测试包

AP-035已单独提交推送`01e5c10a7f14949b1124597479ea0b1a3611753d`，test备份不变；以下是推送后新开始的实机前置，不混入该修复验收。用户新提供`C:/Users/Administrator/Downloads/cmfa-2.11.35-meta-arm64-v8a-release.apk`，同意将其作为测试候选。官方固定AAPT2两次静态解析均exit0、无stderr；精确包名`com.github.metacubex.clash.meta`，版本`2.11.35.Meta`/211035，min21/target35/compile35，仅arm64-v8a原生库。文件48780036字节，SHA`AF7F05D8801798A8E75DE682E5F67070138BECB8623B7A341C3E5CD042A3753D`。有MAIN/LAUNCHER的MainActivityAlias，但badging无launchable行，须设备PM实证。它是VPN工具，TunService要求BIND_VPN_SERVICE；不授权VPN、不导入配置、不改代理或网络、不授敏感权限。静态声明不等于有效签名/安装兼容或已获权限。

静态原件在`.worktrees/sqlite-path-tests/artifacts/apk-cmfa-inspect`，root已读完整报告；不上传APK或原始输出。当前无需先等待自建APK即可开展该第三方包的安装链路预检，但独立可控画面/参数回读等自有测试需求没有因此取消。JDK只有官方便携候选的来源/散列调查，没有下载、安装或执行；旧“已有Temurin”仍无路径证据。

当前root `artifacts/apk-cmfa-live/r00`：仅1绑定的fresh配置，实际当前CLI PID75236正常exit0/0.596秒、stderr0，只读接入`emulator-5556`/transport8/Pixel8/Android14/Ready。实例1播放器63000、VBox68124，实例2关闭；原实例0未操作。前后host-only快照均ADB62248、同birth/path/SHA、协议0029，主profile仍E4E33；没有安装/卸载/启动APK或GUI操作。不能把这次CLI接入当当前ApplicationsView或双实例通过。

下一步先用现有身份冻结后端只读检查专用1/2的ABI及同包；已有同包则保护原数据，不覆盖卸载。之后走当前应用页实际安装/精确列表版本/受控启动停止/仅本轮新装包卸载消失；没有VPN或配置操作许可。仍需恢复AP-033可靠真实输入前置、双实例同步四开关及最终双实例30分钟；所有后续源码修复独审后单独push。该补充属于进行中状态，尚未重新核包或单独交付。

## 最新执行：AP-035已限域离线修复

本段优先于下方历史阶段。基于1b5bac0，唯一产品CatalogRepository修正Windows原生主库/备份路径，不搬移数据、不改事务/schema。新CatalogPathTests的8项追加在原150之后；原代码实际152/158、6条SQLite14。首修复f01运行旧DLL（PDB仍原源checksum），失败保留；只touch该自有源mtime后独立f02有效158/51.43秒，主仓一次158/51.76秒，0警告错误。两边PDB均核三冻结源码，145输入/配置前后保持、进程和自有临时目录清理。主仓四普通路径251/252/253/300的真实CLI也全exit0并独占再开；详见[A13报告](A13_SQLITE_PATH.md)，包/推送以实际交付记录为准。

未运行完整GUI或真实设备，不关闭AP-033或用户强调的双实例同步、当前ApplicationsView安装卸载、最终双实例30分钟。下一步先以原断言恢复可靠输入前置；lc16a三采样OS稳定而WPF ActiveSource仍null，不可继续归因指针漂移。AP-008四候选JAR重新核同SHA，常见JDK/JBR目录及PATH仍无可核工具；需可信便携JDK与自有唯一测试包，不拿用户抖音卸载凑验收。原实例0、共享ADB和原配置不改，不清理worktree。后续A07余项/A08-A16仍全部有效。

## 上一阶段：用户扩展目标与真实测试追踪

本段优先于下方旧阶段安排。用户新增四项已追加到PRD R11-R14、SPEC和TASKS A13-A16：P0全测、P1终控台/RPA闭环、P2群控MCP设计/最小接入、P3抓包调研/POC，四项均必交付。A13a基线先给新任务提供安全契约，A13b继续当前完整测试，A14/A15/A16受控链路再经A13c汇A11/A12，不形成循环；不新增第二个终控台导航。

四任务文档阶段的业务基线为21e3ac17，该历史阶段只更新文档和源码清单，不把计划标为实现。脚本为受限ZIP/动作，已有多目标逐台调度/参数保存，缺步骤参数替换及runId/taskId聚合作业；MCP需补实现。ProxyService Start/Stop为501，外部HTTP Toolkit MCP辅助客户端不是产品server/抓包。官方调研见`docs/codex/automation-expansion/CAPTURE_RESEARCH.md`，显式应用代理只是初选，POC未运行，不启用可能操作共享ADB/CA的外部自动接入。

最近实际运行见`docs/codex/list-console/LIVE_RECHECK_2026-10-16.md`：Build150/51.43秒/0警告错误；实例1真实冷启动82872 exit0/16.393秒，8项Live50432 exit0/13.630秒，中文精确回读、文件SHA一致、12页截图齐。实际ADB62248/birth/协议0029保持，原配置E4E33不变；实例1留运行，其他22仅业务字段一致，跨独立service generation不同，不称JSON逐字相同。

两次完整Demo均FAIL：深路径64608/105.613秒在发布第46例SQLite14（db252、journal260字符），45例通过后停；短路径27672/53.780秒旧coordinate-route前置失败，未到RaiseEvent，其后NOT-RUN。原失败和清理状态保留，不重试凑绿、不改坐标或归因操作者；AP-033/AP-035开放。全部自有测试子进程已结束，未关真实模拟器。

A13b又完成AP-035最小离线复现：当前CLI无授权目标、无雷电路径/无API，四个fresh目录db240/251字符exit0，252/253分别SQLite14非零；journal260字符边界稳定出现。root artifacts/p35a/summary.json及各stdout/stderr.bin保存原字节，四进程均正常终结、无timeout/kill、配置保持；只涉及自有本机数据库，不是产品修复完成。下一步先验证Windows长路径处理的最小方案，再补正式回归，不放宽原GUI断言。

用户再次强调双实例同步、新版应用页安装卸载、最终双实例30分钟和GUI失败都必须继续，已列A13b/A11必验。下一步先解决可重复的输入前置验证，再在仅1/2的独立配置走真实控件和自有测试包，不使用旧Live VM捷径冒充新版ApplicationsView。源码包不含本轮raw/data/依赖或两个用户PNG；保留旧worktree，不合并发布。文档阶段推送结果以实际Git/交付记录为准，不根据下方旧“下一步A07b2”另开不相关工作。

## 用户目标与权限

自主逐路径测试/修复全部原功能，消除真实重复职责，采用Apple HIG启发的浅色、简短中文和响应式体验；保持12页Tag/接口/数据与安全边界。每个功能独立验证后push原私有功能分支，不问重复问题，不合并/发布/部署。原实例0、用户账号/抖音数据、共享ADB、系统代理/证书/权限不自动修改；真实测试仅专用1/2重新核验后执行。CODEBRAIN不读写，全部状态在本项目。

## 已保存版本

- 8303cdc：列表精简基线已推送，完成删除全部分组入口/卡片和屏幕模式/实例目标下拉及重复连接入口，三态表头统一选择，同步操作归主控。
- c1be294：完整PRD/SPEC/UI_SPEC/TASKS、测试矩阵和控件目录计划已推送；两代理独立审查。
- 7beea8315eafad0118d5189a784afe8236111db6：主控A/当前B投屏目标混用修复已推送；完整证据见A02_MAIN_TARGET.md。
- 08c4f8c9b0057a8c1eda0abb43367f8fb93bb7b4：A03共享外观与命名19源文件已独立验收推送；见A03_APPEARANCE.md。
- 2b1fe6e：A04公共响应式已独立验收推送；15155a708802199d725d05523d02ceb8cb49f78a：A05a详情/筛选隔离已推送。
- test备份始终49e471081d2648a4d533cc98ec00be4f5a103c77。
- 76d0e43cb0badfa4b5ba6261a05d08b3f1e779dd：A05b1设备/主控正文已推送，7源+报告/README和清单共17文件；源码包287文件/290条目逐项SHA一致，配置与原始证据未上传。

A02主仓149/149（51.17秒）和最终1000×700界面通过；独立verifier149/149（51.60秒）及三尺寸12页、25守卫通过。主仓DLL摘要A58A27BD9437F07503A8F5637C28528DDB48BC930F9867A514E0B3381E5FBC29。没有调用真实外部确认、scrcpy启动或嵌入；不能声称PASS-LIVE。

## 当前状态与下一步

### 最新执行：完整目标恢复，继续A07b1原始待办路径

上一轮为progress：针对用户再次提出的列表精简请求，确认当前源码已经实现四项，未重复改产品；主仓原样Build/150离线（51.49秒，0警告错误）及既有公共壳135实际成功，独立源码复核通过。证据在`artifacts/lr16/summary.json`与device.png；139份src/tools/tests检查未变、实际配置E4E33未变，所有自有runner/GUI已终结，锁root空闲。该轮不是全GUI或实机通过，也没有新功能commit/push。

完整目标仍active。[A07_APPLICATION_PENDING](A07_APPLICATION_PENDING.md)实际baseline29660已终结：150/8警告0错误、12例5PASS7真产品红12清理，报告9F9A，五次144输入/原8/config同，无timeout/kill/残留；其他旧套NOT-RUN。生产r1仅ApplicationsView7+/3-（4260），测试r1a仅四处编译警告修整（DBDB），Main33C首接线、旧8不动。独立final45047已终结：150/51.27秒/0警告错误、新12/310/12清理和旧19/18/10/11/16/49/A07身份10/原应用/12页全部实际通过，Smoke0/126.368秒；112原任务全终结、144输入/配置同，owned0，独审限域批准并归锁。旧AP033本轮绿不解释间歇成因，nodes/colors0和stderr已捕获诊断如实保留。

root三源已机械导入4260/DBDB/33C；预导入143基线FD3C，144全文/canonical同独立版，其余141原raw/旧8未改。代表runner C832只修三处报告StrictMode解释，F213未执行。root38202已真正终结：Build44584 exit0/63.432秒，150/51.05秒/0警告错误；Smoke16536 exit0/118.134秒，同新12/310/12清理、旧19/18/10/11/16/49/身份10/原应用/12页全过。113任务记录终态（独立112之差为合法条件观察Task），原流及144前后不变，配置E4E33、自有0，无timeout/kill/清理错；root已目视报告并获独立限域交付批准。摘要94F972...ED6AE/DLL4BC9...32A62，所有session终结，Build/GUI锁root空闲。

当前是交付门禁：冻结三源与7文档→已审helper9CA按tracked快照加仅两显式新源核包/回写3清单→独审13暂存路径→单独commit/push原私有feature。实际Git/远端及`artifacts/app-pending-root/push-complete-r1.json`为准；若已含本功能且远端一致，不重复交付。两用户PNG/配置/依赖/current raw不stage，旧AP021材料保留；不合并发布或清worktree。下一项API释放通知真实复现，其后应用正文响应式、A08-A12、原生保存、真实1/2及30分钟必验，完整goal active；不要重跑45047/29660/38202或下方旧session。

### 最新执行：AP-033诊断采证完成，成因仍开放

上一轮已实际commit/push `ab11ec94fe6b7ab139a922ac346850ed7c292501`，feature远端相同、test仍49e4710；`artifacts/app-identity-root/push-complete-r1.json`记录16文件限域交付、304源/307ZIP/156Bundle、配置E4E33与owned0。主仓tracked/index干净，两用户PNG保留；上一目标轮为progress，不重做已完成交付。

当前轻规格及完整结果见[AP033_INPUT_DIAGNOSTICS](AP033_INPUT_DIAGNOSTICS.md)。两新树均ab11，唯一旧测试49+（70DF/patch0315）经双静审，原266行/定位/同步/loop/等待/断言恢复一致，无新PInvoke或生产变更。基线48E0/runner650524固定；独立p01 session61127已terminal，Build51736 exit0/77.536秒，150/54.33秒/0警告错误；Smoke16172 exit1/33.665秒。原RL19/CC18过，SL9PASS1前置FAIL/10清理，Main346先败，其后节点/自动/发布/A07与旧应用/12页NOT-RUN，无重试。raw/五次127源前后/旧fixture/config E4E33同，owned0/无kill/清理错，锁已归root空闲。

三快照仅zoom0.1，均ActiveSource/Captured/DirectlyOver=null、目标HwndSource有效且同window、焦点/可见/激活true，WPF回转屏幕(0,0)；after-set OS911573直接转Canvas800450/命中Image，after-loop OS325352、原loop采330355，两采样非同一时刻。观测确认本轮输入源缺失与OS后移，成因未知；不归因用户、不改坐标、不抢焦点/追加同步或盲重跑。输入源观测有时序开销，后续PASS也不证明根因已修。SL983927...682D7，acceptanceA8C976...69FC，原失败保留。

非实现者批准诊断增强限域交付条件：root核全编译输入并明确未重跑。root现仅机械copy同70DF单源，src/tools/tests diff49+；143输入=127+12tests含lock+4根配置，59raw同/84仅换行、normalized全同，proof430F1F...543C7。初142计数预检在Copy前拒绝，原记录保留；root本轮未Build/GUI，不冒称root新150。正式交付含测试源、README/五状态文档/AP033报告和三清单，实际commit/远端以Git及artifacts/pointer-root记录为准，原两PNG/配置/raw不stage，不合并发布。

下一步A07b优先实际复现AP-034：脚本/稳定Push跨真实timer tick可能清pending、API无释放事件、小屏固定栏目前均是未实测候选；自身running暂无普通UI反例，不强行造红。后续轻规格/12例矩阵见AP033末节及只读agent结果；旧套已经有输入前置缺口，不以盲全smoke替代专项。完整A07/API/布局/原生保存/A08-A12/实机与30分钟全goal仍开放。

### 最新执行：A07a已获限域独审，主仓GUI前置缺口保留

本段优先于下方全部旧即时状态。r1只改ApplicationsView/ApplicationRows，原合法改名三红后独立f02正式150/51.58秒、0警告错误，新10/114检查/10清理，旧六19/18/10/11/16/49、旧应用smoke及12页完整通过。f01截屏坐标首败不倒填；r0b只将新hook移回自然应用阶段，Main5864、新fixture03FD、生产9941/F57冻结，原七夹具不动。详见[A07报告](A07_APPLICATION_IDENTITY.md)。

主仓四源已机械导入且raw同；root127源与独立127源逐项规范化文本全同。root-r01参数预检在child启动前拒绝（ApplicationBatch LF/CRLF摘要差异，内容未改），保留后独审只纠正argv。root-r02 Build75.095秒exit0、150/54.18秒；完整GUI34.718秒exit1，旧重连19/缓存18通过，旧截屏9PASS1前置FAIL10清理。Main346先败，新hook381尚未到；新增10、其后四套/旧应用/12页NOT-RUN。请求OS911,573，实际1105,575，WPF screen推得0,0，不能归因用户或并同f01根因。原报告/原始流保留，所有session/child已终结，无kill/残留，127源/旧七/config E4E33均不变，Build/GUI锁空闲。

非实现者明确批准A07应用改名限域交付，拒绝“root完整回归通过”或“鼠标已修”表述。交付仅4源、README、六状态/复核文档、A06d/A07报告和三清单；A06d只有真实未验报告，不导入其代码。版本与远端以Git及本机artifacts/app-identity-root交付记录为准，原两用户PNG/原始证据/配置/二进制不stage，不合并发布。

下一步先对AP-033仅加有界只读输入源、捕获、真实前台与自有窗口生命周期证据，禁止盲重跑、抢焦点、弱化原断言。应用pending和正文响应式仍待，其后A08-A12、原生保存、AP019/020/023/028、真实双实例及30分钟全都开放；完整goal保持active，不回滚或清旧树。下面“A07a基线/等待r1”等为历史。

### 最新执行：A07a应用改名身份已取真实红

本段优先于下方历史。合同与结果见[A07_APPLICATION_IDENTITY](A07_APPLICATION_IDENTITY.md)。三新树apple-app-identity/-tests/-verifier基于1acbf23；测试r0保留，最终r0a新fixture03FD1C95...3A279/Main70055C6D...5F54D已双静审/机械导入。verifier唯一baseline-b01：150/51.54秒/0警告错误，Build73.223秒exit0；新10例3红7绿/10清理，1000smoke3.502秒exit1；旧六/旧应用smoke/12页均未到。原JSON52EEA416...7DBED、进程6CAD86E6...9E557，root实读有效前置、真实挂接命令/旧单元格名、错误和后端调用；不是helper前置红。127源/旧七套/实际配置E4E33未变，无kill，自有进程0，session62077实际终结、锁归root。

已授权新实现者仅在apple-app-identity首轮修改ApplicationsView/ApplicationRows，不BuildGUI，不改公共record/测试/pending/布局/原生保存；方案为严格连接identity比较+Name-only现有行展示更新，原冻结Target不变，计划/合法结果采用最新显示名。等待两生产源冻结→root与非实现者审→独立final单1000完整→主仓代表/交付，每个实际完成缺陷立即push原私有feature。当前主仓生产源仍未改，不把修复中写成通过。上一goal轮是progress（正式夹具/方案和独审修正），本轮已取得能改变下一动作的产品红；完整目标active。

### 最新执行：A06d原生交互前置失败已留证，继续A07

本段优先于下方历史流程。A06c2已经实际提交并推送 `1acbf23b149047dd46a132660d417f31f5613812`，远端feature相同、test仍49e4710；完整实际记录为根 `artifacts/reconnect-root/push-complete-r1.json`（SHA32892414...0A20）。其301源/304ZIP/155Bundle与精确13暂存已经独立audit6518AB5D...9314通过，不重做该交付；A06c2交付后tracked/index干净，当前仅阶段状态文档变更，原两用户PNG仍未跟踪，配置E4E33未变。

下一项轻量合同见[A06_NATIVE_IMAGE_SAVE](A06_NATIVE_IMAGE_SAVE.md)。已核SaveImageAsync在原生框后读实时crop，合法新DebugImage会清crop；VM写盘await前编码本身不是缺口。推荐开框前守卫核验并冻结PNG，确认仍保存原意图，不默认漂移后取消，不持全局Busy跨框；准备后写入不得被ExecuteAsync的Busy早退静默吞掉，并需局部写入/关闭约束。JSON原生框和未完成拖拽/失捕获另项。

三新隔离树apple-native-save/-tests/-verifier均基于1acbf23。正式r0仅App严格验收接线+新NativeImageSaveAcceptance；patch6E987...FD04/fixtureAB46...9531/App244C...9999已双静审，原六套不动。独立首次Build150/51.30秒/0警告错误，实际子进程73.422秒/exit0，DLL25F2...1F77；127源/旧六/配置同，exec12857已结束归锁。

root原生首启exec25625/PID27696，sky唯一返回自有Title/HWND，但首次窗口截图FrameArrived超时，刷新重选同窗/activate后单次重试仍window capture超时。遵守computer-use恢复规则停止，不用UIA/Win32输入。真实fixture在ready-for-crop90秒超时，整个child91.520秒正常exit1，无330s timeout/kill；原FAIL/verifiedfalse/cleanedtrue，Down/Up/SaveClicks0，只有Discover/Discover/F0Shot和原Task2完成，无F1/裁剪/保存框/信号/输出。这是前置失败，不是产品红，尚不批准保存实现。原报告4A5E741F...6276E、工具记录D124A613...FB3DD、root完整127/旧六/配置清理检查6EC31239...14E0D；PID0/所有exec实际终结、锁root空闲。窄runner仅102个src cs/xaml，另按Build127逐一检查，不混称。

源码与原始失败均保留在独立树和root artifacts/native-save-root，本阶段主仓只改状态文档，新CLI/夹具尚未合入或push。CLI补验首r0在missing-demo因CP936误解码为UTF8而首停（实际child拒绝正确），原FAIL1CB70...6285C保留；仅另存runner-r1严格UTF8/936与decoder记录、原10断言/argv不改，经root读增量后新轮一次10/10拒绝通过。摘要tests/artifacts/native-save-cli/run-20261016-040104-3a113fe2/summary.json SHA25A4E1EB...9E115，root重读十份原stderr和完整127/旧六前后证据。每child exit1/约0.189至0.216秒/无kill，目录无误建、配置同/owned0，不是原生保存绿；没有仍运行的session。

可在旧进程终结后独立做accessibility-only L1诊断，不能据此拖画布或填画面恢复；不要直接重跑同一个失败case或在无原生红时修保存。继续其他安全任务：A07当前ApplicationsView（不是旧VM LoadApps捷径）已只读发现Name参与record相等/自动读pending/固定栏三个候选，均未实测，优先做仅metadata改名的同身份应用行反例。具体为Features/Applications/ApplicationBatch.cs的ApplicationTarget positional Name自动参与record相等，而RequireSame不含Name；ApplicationsView.cs约193/231的Contains可能拒同代次/transport旧行。新A07三树尚未建立、未编码测试；下一轮先形成轻合同/受控红，不把静态推断当缺陷。完整目标active，原生保存/JSON/失捕获、AP019/020/023/028、真实1/2及30分钟仍开放。

### 最新执行：A06c2离线验收完成，交付门禁

本段优先于所有下方历史流程。A06c1已实际commit/push `90815e8cd05a6b2b4574dd7ca8213da0f8a41835`，远端一致/test49e4710不变；实际交付记录在根`artifacts/cache-root/push-complete-r1.json`，不重做旧交付。

新三树`apple-reconnect-loading`、`-tests`、`-verifier`均基于90815e8。轻量合同与全部本阶段证据索引见[A06_RECONNECT_LOADING](A06_RECONNECT_LOADING.md)。新19例r0 B0E4...63D1/Main仅测试接线0714...E017已双静审；原五套不改。首次独立Build进程exit0/73.494秒，正式150/150 51.11秒，实际2条新fixture CS4014、0错误；同EADD...ABB2 DLL首1000 childexit1/30.373秒，19=8PASS11FAIL19cleanup、208checks1826events、precondition0。原报告4FE2DE27...209A、摘要AC4D9312...37F6，root实读19及关键分量。无新Ready首载pending的产品缺口已复现，AP029开放。原五套未到NOT-RUN，不以第19基线前置绿代替新ack证明。

基线两exec已结束、自有0/126源前后同/旧五套及实际配置E4E33...BB05F0未变。生产首轮r1只VM/Main共30+/1-，累计F950B28D...50EB、VM C288EEE0...75F1/纯生产Main9C4F...3DDB，双静审通过；新fixture r1 F1AC3257...4228仅比r0多4ASCII显式discard，不await或改19合同。hig已机械导入并与root独立组合计算核最终Main F243E54C...F802/46879B，VM C288、fixtureF1AC/旧五套/实际配置同。同步严格source ack消费已履行pending，不改guards/revision/Core或默认Demo。

独立矩阵已完成并归锁：150/50.92秒/0警告错误，DLL8EED24D1...E83D；三完整窗口首次exit0/93.798、94.062、94.575秒，每19/289/19清理，事件实际2214/2214/2212；旧18/10/11/16/49全套、外观209/Native25/筛选73+matrix32/文案36/12idle及六shell135全过。第19三次externalpending true→strict g2/t12采纳false、inflight旧payload未发布/最大1、两段几何/实际Close实读；outer.colors仍0，不填外层取色成功。20份126源快照/旧五套/实际配置同，全部exec结束/owned0，摘要8609A9E0...6470E。不要重跑旧矩阵或误以为62667/6787等会话仍在运行。

根已机械导入DE0+F950，VM C288/Main F243/新fixture F1AC精确；new初17F847...CDB2仅CRLF差异全文证明后复制冻LF，旧5原LF和配置同，import记录在artifacts/reconnect-root。首次主仓150/51.05秒/实际0警告错误，Build进程71.580秒；一次1000完整exit0/93.348秒同新19/289/2214/19清理与旧五套/12idle；390shell exit0/135/2.528秒。第19true→false/时间序、旧payload拒绝、源和任务清理原分量已核，root目视1000及390。根摘要BE91B9A5...3E381，DLL FB4B1F8C...703D7；六份126源前后同、旧五套及实际配置同。根67844/97085及shell全部实际终结、自有0；Build/GUI锁root空闲，不凭下方历史状态重跑。

当前只剩本阶段交付：10份源码/正式文档最终独立审查→冻raw10→仅快照helperF36ABAB4...BA6C打包/全源ZIP核字节/回写3清单→精确stage13→独立audit60A9E46E...2C537→单独commit/push原私有功能分支。根代表执行器3C203BB1...F019、helper/audit均已root实读静审，打包与audit现在仍无执行批准。实际结果须读tests树artifacts/reconnect-loading-package及根artifacts/reconnect-root/push-complete-r1.json，并核Git/远端；不要凭本段流程推断已push，也不要Git已含且远端一致时重做交付。两用户PNG/实际配置/依赖/本轮原始证据排除，AP021旧tracked材料保留不称隐私净化，不合并发布或清worktree。

完整目标active，A06c2运行验收已过但当前交付未做。下一项先取证原生“保存裁剪”开框期间合法晚到帧导致crop清空、可能退化完整新图的静态候选；VM写盘await前已冻结bytes不是缺口。先读合同和真红，优先保留原保存意图，不因更小易绿就取消合法保存；JSON原生框与未完成拖拽/失捕另案。19自有窗口固定1000，不冒称逐例六尺寸；outer.colors/nodes实际0仍保留。AP019/020/023/028、A07..A12、真实专用1/2及最终30分钟继续开放，不启动普通程序、原实例0、ADB或改本机配置，不删除worktree/用户PNG。

### 以下为A06c1已结束的验收与交付过程

本段优先于下方所有历史执行记录。上一goal轮针对最新用户四项列表请求完成实际主仓150/1000完整/390设备889复验，是有效证据进展；没有重复改源码。新的goal继续完整目标，恢复A06c1；原c7b6d24 HEAD/test备份未动，本机配置仍E4E33A8...BB05F0，不导入未验修复。

生产r1b单VM raw41E4F7E6...A9CA/patch7177D2EB...CDF5不再改；正式夹具r1 EB029377...F516/MainA1878014...E34A已精确合入。原基线15产品红、首产品14PASS/4FAIL和仅七叶诊断均保留：四败只有Targets变化，其余六分量true，原离线安全门禁合法清有效目标，不偷换勾选。r1据此分开冻结行/统一行引用key/选择意图与有效目标保护，18场景/产物/等待/原四套不改，不倒填首轮为通过。

最终独立150/50.98秒；三桌面首次exit0，每18/295检查/163观察/18清理，旧10/11/16/49及原套、12idle/Error空；六shell135、33本地PNG/JSON逐项重开。正式汇总04A22C40...3971D、DLL05EFB7C7...160D7。根代表150/50.96秒、一次1000完整exit0/87.585秒同新18及旧四套/12idle，390shell135通过；根汇总57E17C7A...2F4BD、DLL9E381B9D...A249C。root实读原JSON并重开代表PNG/JSON。全部exec结束，Build/GUI锁root空闲、自有进程0；125源各阶段前后同，旧四套raw/canonical同，实际配置E4E33A8...BB05F0未变，未开普通程序或真实实例。

当前是完成运行验收后的交付门禁：10源码/正式文档raw冻结→已审helper562EDF08...C6B8E只在隔离快照打包并回写三清单→精确stage13→独立audit r1 68509E45...C3D89→单独commit/push原私有分支。实际结果须读本机tests树artifacts/completed-cache-package和主仓artifacts/cache-root/push-complete-r1.json，并核当前Git/远端；不要凭本段流程推断已经推送，也不要在Git已含本项且远端一致时重做交付。两个用户PNG/配置/依赖/原始证据不入提交，AP021旧tracked材料保留，不清worktree、不合并发布。详细边界见A06_COMPLETED_CACHE.md。

下一项先验同Current行新Ready后的自动首载：现通知只在Current换引用/导航建pending，c1清Nodes只投影，HasActiveOperations释放只恢复既有pending，尚为静态候选。建议从完整目录接纳后的新Ready身份建意图，防metadata重复、自己几何预处理重试、离页/关闭和在途新身份丢pending；先新红测，不先扩大产品事件。outer desktop.colors=0如实保留，是旧夹具伪离线后未重载，不冒称外层取色成功；新10及18颜色各有独立证据。AP023/028、原生保存/在途crop、DPI/实机、A07..A12和最终双实例30分钟未完成，goal active。

### 以下为先前阶段记录

本轮续跑以此为准：A06b2b已独立交付推送c7b6d24258699d975310b902f7a66f0380d8932c，远端一致/test备份未变；297源/300 ZIP/153 Bundle实际核验，交付audit6E71FBE5...35321、主仓artifacts/scp-root/push-complete-r1.json留证。上一目标轮为实际进展。当前[A06c1完成缓存身份](A06_COMPLETED_CACHE.md)三棵apple-completed-cache/-tests/-verifier基于c7b6d24，冻结r0 C79EE9D6...AD952/MainA1878014...E34A/18例已双审。独立首次150/51.67秒/0警告错误，DLL62D8A38C...13408；一次1000 childexit1/8.312秒，18=3通过15产品红/18清理、168检查137观察。原报告550B2249...38FC、汇总7E022FD9...FDC0A；Catalog新t12/g2而旧row t11/g1仍输出旧PNG/JSON、Error空/完成，root实读18例及文件并目视。125源/原四套raw/canonical/配置E4E33A8...BB05F0前后同，exec结束/owned0后归锁，锁现root空闲。旧10/11/16/49与原全套未到，NOT-RUN。main只获独立树单VM首轮编码/冻源、不Build；hig只准备expectedExit0最终runner；full仅准备已静审helper562EDF08...C6B8E，不执行。保留精确Offline历史、metadata与后台独立检查，不改revision/主动自动首载/旧测试或真实环境。最终产品、主仓代表和交付审查未做，不提前标绿；原生保存/在途crop/同row自动首载、实机和A07..A12继续开放，完整goal active。以下b2b及更早段落均为已结束的过程，不是当前执行授权。

最新阶段：[A06b2b截屏正文](A06_SCREENSHOT_RESPONSIVE.md)基于已推送29ab03feb858e87a875f4e41a82d3b79f35b4296，三新隔离树apple-screenshot-responsive、-tests、-verifier。基线六几何真实红、第一生产轮长坐标文本预算不足均保留，第二轮唯一坐标列70改100后通过。最终生产raw AB50FBBB53EB1C0641FDC9D2528F03C8387D0DC026B6F8170BB9423CF6DD361A、Main8356EF3694A3FCC403BFA31C330B66816196E84AAAB0E59C3EA1E9451F59C84D、新10例fixture59CAFC660A1C5427942EE453DE9BD6004DE13A48BA21479EB4CF4AFE84DE7762已精确合入主仓；仅BuildScreenshotPage与一行Demo接线，原11/16/49、VM/handlers/主题不变。

当前执行以此更新为准：独立最终150/51.05秒/0警告错误，三桌面1000/1440/1920各新10/630/257/10清理+旧11/16/49原全套/12idle、六shell每135，均首次exit0；汇总44476DB4601B9AE07467906341080918FD6D80FF412551F7A549744B358F5946。主仓150/51.43秒/0警告错误、1000完整87.66秒同10及旧全套、390shell135通过；root直接实读/目视390分段与1440双栏，汇总artifacts/scp-root/root-summary-r2.json SHA7193658ADB2F19EEF637B026F2584B46D66E2DA9AF933777FA82AFE7C898839E，DLL4D5DE1381A5DBB6B6DD22B9FE7B7970CD71773EC6DF0EB6763E7337831ADDFF2。首汇总标量matrixCases误记1已按真实32纠正保留，未改源码/测试/首次执行结果。各阶段124源前后同、旧fixture同、配置E4E33A8...BB05F0，自有进程0/exec结束，锁归root空闲；未启动普通程序或真实实例。10文件冻结后用2539EE49...F680 helper在隔离快照打包，只回写三清单；hig实际审ZIP/Bundle/Tree与精确13路径后才提交push。新打包审查与Git结果以ignored audit和实际记录为准，不把旧b2a包作为本阶段产物。两用户PNG、运行配置和本轮原始证据不提交，AP021历史材料仍保留。完整goal继续active；下一步缓存身份/原生保存/未完成裁剪，再A07..A12、真实1/2与最终30分钟，不扩大本轮为物理点击/DPI/Android通过。

上一阶段验收详情：A06b2a节点正文已完成独立和主仓离线验收及交付推送，完整goal继续active。最终三raw E95E45D5...7250C（DebugView）、41F0B95A...3F332（新fixture）、D85B0235...4C044（Main hook）已精确合入；原16/49及VM/Core/Host不变。独立150/50.80s、三桌面各11/905/319/11cleanup+旧16/49原全套、六shell135；主仓150/51.02s、一次1000完整新11与旧套/390shell135全部通过。独立汇总CB18E4F0077C9BF91F9CDFF2C36DB3A1A45BB4388945D1B0746F3852D6F9251B、主仓汇总B79985375CAAB5968D0110FA66410BEB350C01FE42799E7A7BC9CE7BA91A8342，root直接读核/目视；DLL B98851E884B13160B0D16710ADC08777A7E4BBCA06407245AF8562514DDE0B71。所有必需exec/GUI结束、锁归root空闲，用户配置仍E4E33A8...BB05F0，未打开普通程序或操作真实实例。

交付只包含本阶段10个源码/正式文档加3生成清单；使用已独立静审ignored packrunner-r3（SHA0F2C1F3A15E2F8E7E22BA5F79F8DA09E390B17A1C13BBEED641A3C6801BCC134）按tracked+2明确新增生成快照并逐源/ZIP核SHA，结果以独立audit及Git记录为准。首次r2打包在只读Git预检因PowerShell的VoidTaskResult污染字节返回而停止，尚未生成快照或回写清单；r3只加[void]，不改产品/测试，不算第四轮产品修复。原r2和失败记录保留。不纳入两用户PNG或本轮原始证据，AP021历史材料仍保留；独立交付审查通过后立即push原私有功能分支。下一步截屏正文/缓存/原生保存与未完成裁剪逐缺陷验证；A07..A12及真实1/2/30分钟保持，绝不以本轮布局绿标整个目标完成。

### 本阶段过程历史

以下“锁/待执行/尚未完成”等均已结束，仅保留失败与决策，不是续跑指令；当前状态仅以上两段为准。

即时最新：hig持独占Build/GUI锁，最终prod3b raw E95E45D5557542E30CE239DBB8B7DB77107A5251111FAF5C32BBD38BBFC7250C/15098与fixture4b raw41F0B95A62150279C4E432CFC89DEB0D088EA80601675D9287BA09FE6473F332/61441已root和hig实读静审；hookD85B不变。新11=原严格9+2个新用户覆盖case（wheel/原Thumb/PageDown/新选区/UIA新长46），S0.75/S1.27，仅合成路由和程序化新意图，不声称物理滚动或私有ticket。final-r3 runner SHA A99ADE6AE916C189E54E34A457CAC4308D0050CCCF593AC08BF1732AA6630B7D/Parser0。首次导入因增量旧路径指向artifacts拒绝，尚未BuildGUI，不算产品红或重试；root明确授权只reverse自有冻r2累计FC0B回HEAD Debug，再apply累计3b562794D...336BF及r3→r4→r4b夹具增量，核raw后继续首Build150→1000内11/旧套→其余矩阵，任何首错停。root/main/full不BuildGUI，主仓尚未导入源。旧状态段均为过程历史。

打包runner最新为tests树`artifacts/node-responsive-package/New-FrozenNodeSnapshot-r2.ps1` SHA63E6DB7F14498E1067EE3DAC469AA2E3071BD0F813E974481C4390E327AB8918，root已实读有界Git增量/Parser0，尚未执行或最终独立审。hig曾把JSON展示的双斜杠当源码而提出guard必拒绝，full/root按原BE6C字节核20/97均单U+005C，已纠正为静审误判；新DirectorySeparatorChar仅消歧，不称修了真实guard bug。新runner真实改动为Git10s退出/5s单次kill/两流10s drain及异常聚合，固定10交付+2明确新增/3清单回写门槛不变。待本阶段最终验证/十文件冻结后才放行打包。

当前锁归root空闲：hig final-r2首个1000已拒绝并结束，150/50.98s过，新9=5PASS/4FAIL/9cleanup。两个390完整过；四大尺寸属性纵offset回原尺寸后丢失，选区/caret/原对象/树锚点仍保持；JSON046D54EC0E2DA08B96468AC30944824C5C7A91B87329857E0A6B91D90FA5992D已root实读。其余桌面/shell/旧16/49未执行，不预称绿。root已授权main第三生产轮仅DebugView本地TextBox滚动意图+ticket，保留首原viewport/offset，中间clamp不消费，真实新输入/选区/文本取消旧恢复；先冻源审查、不BuildGUI。full仅设计补新用户覆盖反例，原9严格预期不变；hig待最终冻结再执行。下面即时段中的“hig正final”已结束，是历史，不再沿用其旧执行锁。主仓尚未导入新源码，完整目标active，所有真实设备/config边界不变。

第三轮生产候审已冻r3b：独立prod树DebugView raw `E95E45D5557542E30CE239DBB8B7DB77107A5251111FAF5C32BBD38BBFC7250C`/15098B，累计patch `562794D1B01729AF092E35796775584DEB8499C90A37A879144157400EA336BF`，位于`apple-node-responsive/artifacts/apple-node-responsive/r3/`。root已实读新增71行及将三输入统一Preview+handledToo的静态修正，hig在审其余异步/生命周期，尚未Build。full已获实施许可仅新增2case/5子步骤，总11，原9动作断言等待不动，长46仅new-content flag，pending Hooks不冒充私有ticket/物理输入。fixture新r4尚未冻结，不要用旧68B2或8A48冒充最终版；主仓尚无这3源。Build/GUI锁仍root空闲，所有代理此时只编辑/静审。ignored打包runner `apple-node-responsive-tests/artifacts/node-responsive-package/New-FrozenNodeSnapshot.ps1` SHA BE6C379B026069F654586B7ADFFF9F036767A3F80EFE370258AEC96928B21A71已root实读224行/Parser0，要求本阶段10文件最终冻结后显式放行，尚未执行。

即时续跑A06b2a节点正文：上一目标轮属于progress（60a7922已push），不重做列表。独立基线9例6布局FAIL/3状态PASS/9cleanup已root实读A755D091...AEC，原夹具CS0104编译失败另保留。生产仅DebugView两轮：同对象3/2/窄分段+局部44；首轮Header外padding与WPF滚入目标不一致，经实际45只显31及官方源码确认后改Header内容44，当前prod SHA5D93D9351020677E982B06DC459A2956D63A9E6AC5EB2327E8112E69D5C8451F。第二轮150/50.77s及首中末44/隐藏树末项重开已过，但夹具错要求父Tree.Focus返回true，实际子项持焦且Active/Within true；58170...398F原FAIL保留。三方核官方语义，只纠正前置为实际可见焦点归原树，r3 fixture8A489903B938958BCF1B79246D75A6DE641D810DB2D4E1BDA57A47F613BBB4F1，原后续断言/等待不动，hookD85B不变。root现已将独占Build/GUI锁交hig独立final Build150→1000内9及旧套→1440/1920完整→六shell，首次失败即停，尚无最终green；root/main/full不BuildGUI。主仓仍未导入新源码或推送本阶段。原0/账号/共享服务/config边界保持，完整goal仍active。

最新四项列表精简请求已再次复核并完成本轮两处提示修正，提交与远端功能分支均为60a7922843e24bc217d362a83e3e1325cb418a4c，test仍49e4710。不要再根据旧截图重复删除分组/模式/启动入口或重新跑本轮。完整证据见[列表报告追加段](../list-console/REPORT.md)：既有8303cdc功能先独立150/1000/390全过；新两测试源在旧生产上明确5条清晰度红，之后仅两生产源改“实例操作”/Name及启动目标范围和ShowOnDisabled，执行逻辑不变。最终独立150/两桌面完整/390正文889全过，summary F1D813E39E5804B4548735ABEAA5C9D33E8115CA1751A46E7CDB3327CC4C8BA7；root150/50.97s及一次1000完整exit0、报告704429694B8B2D6EB5CB3D3C0D0513A6733FF79D841791145B8C2D24F3DC5AE2、DLL0AC3D6B253294B96919B9856A4903407D54F1DBC4A9671A52F5072BCA06BB433已实读与目视。8文件精确scope获独立交付批准后push，未merge/release，所有exec/GUI已结束，锁归root空闲；未打开普通程序。主仓实际配置仍E4E33A8...BB05F0，无Android/物理hover通过声称。

本轮源码快照在artifacts/list-clarity-package/snapshot-20261015-220548-ad03a838，293源/296 ZIP entry及151源码片段独立逐项SHA通过，ZIP D9D07F19A487478AACB60F25F56FFF8C1138B134A29DBC76E7DA1A6CB051A0FD。打包对两未完doc采用HEAD-manifest原字节，不纳入本地TASKS/HANDOFF及未跟踪node规格草稿，全部untracked和两用户PNG均排除；这些本地草稿故意仍dirty，不是遗漏。AP021历史tracked材料仍保留，不能说隐私净化。不要重跑ExpectedHead ef的旧runner。完整goal继续active，下一步回到下面A06b2a，不把本轮用户请求当整个目标完成。

A06b2a节点正文暂存待续，不是goal暂停：优先读[A06_NODE_RESPONSIVE](A06_NODE_RESPONSIVE.md)，apple-node-responsive/-tests/-verifier均保留；仅tests树的新ResponsiveDebugSmoke+MainWindow hook草稿冻结，patch 5E9C7AA75AF8BB340090A8BAC9EAAA712A1BC5FAD26B39D2E6BC5ED9CC340993，新源A3C48F8AE4B13C785C4AB5A4EF4704AFC51997BC0C9E7EAC37A025C2676076ED、hook D85B0235829803F5C5FE7F2FC69AFCEB7082AF490C9C05C0878EFAB077A4C044，位于该树artifacts/fixture-contract；未编译、未导入主仓、未取节点red，不能称验收。原16/49/App/VM/DebugView未动。9case拟验证六尺寸及empty/Busy/error；body920/640起始三/二/窄画面节点属性分段仍需实测，复用控件不重建。焦点测试仅自有Demo子窗首次正常ShowActivated与可见控件Focus，不额外抢焦点。原生导出、物理指针、截图/crop/cache不由此替代。完成本轮当前请求后继续该草稿审查，不复跑已结束A06b1旧锁。

节点三树已安全对齐60a7922，不能拿旧菜单预期覆盖新名。先静审冻结草稿、独立旧DebugView实际几何取红，再授权生产，不预称9case已执行。后续A07只读准备发现：AP008四个JAR字节/SHA匹配，但JAVA_HOME/JDK_HOME/PATH及固定ProgramFiles路径没有可复核JDK，不能沿用“已有Temurin21”当当前证据；仓库无自有Manifest/Java/Kotlin/构建脚本，ApplicationBatchTests的APK是文本占位，旧实机包是抖音而非自有可破坏测试包。限定路径结论不代表全盘无JDK；未下载/执行Java/SDK/ADB或安装应用。A07应先定义唯一无权限测试包与可复现构建验收，再审官方便携JDK来源/工具执行/临时签名，真实安装清除卸载仍须身份边界。

当前 A06b1 自动首载调度已完成独立及主仓离线验收，提交以Git记录为准，详情[A06_AUTOMATIC_LOADING](A06_AUTOMATIC_LOADING.md)。基线150/16例8PASS8FAIL且16cleanup；首轮仅两生产文件修复后，独立150/三桌面每16+旧49与原完整套件/六公共shell135，9GUI首轮全exit0，无重试；汇总D3128E92ADF5EB47223454D650E817AA829792CF8C00B0373ABE696A477A38FD已root直接核。新tests/verifier/实施树均从e235657建立且全部保留，旧树不清理。

主仓三冻结源码精确一致：MainWindow 2E8DC93EA0CCD4E0F08A2FC467FBD703580AEA71E1EBEE4C78125C5C4DD98D49、VM 894985B3DAB1EF02856B8AEDEBEAB78386F9BD866928F3D0EF69C64C600E029B、新fixture 8733568495117D19953A6A261ACD25E0B80933E2A4A323F915884052FBD7A6AC，旧49不改。产品patch B454AFF58F326943FFFFC73F60C15F477A37EA0A878E4EA61858A33A31B32CEF。root150/51.27s、首次1000完整16/137/16cleanup+旧49/401/191/49cleanup/9正确磁盘源、390shell135通过；新报告1329C107AB6EAACB78005A4576D9AC499BEE74F5A47A663D3D831F42BED3792D，DLL E3A9FFE48553C980B857D3C5F05C498022E553510859CA2D57827AE0C58196E2。root已实读实际报告及目视两代表图，用户profile仍E4E33A8...BB05F0，所有必需exec/GUI结束，锁空闲归root；不得复跑旧锁。AP019/020/023历史首败不因此关闭，公共shell不代表正文全尺寸或真实Android通过。

A06a发布隔离本阶段已独立离线及主仓代表验收，提交以Git记录为准。四源准确冻结、完整红/夹具纠正/取消回归/独立证据见[A06_DEBUG_PUBLICATION](A06_DEBUG_PUBLICATION.md)，无需重做本阶段或执行下面旧锁。独立150/三桌面各49+原完整回归/六shell135/四设备监控代表，14进程首轮exit0；主仓150(51.65s)/1000完整49/390shell135通过，DLL55D2AC54BC89B7C9A5A8B2589A16B479215DDC20FC8EDAFAFD8B34E66C9A67B7。实际profile仍E4E33A8...BB05F0，必需exec/GUI均结束，锁空闲归root。原Hover首败AP023仍未知，辅助modal仅最后一轮截图，不称产品已修或六模态视觉齐全。

下一步A06b2：节点固定star+300+270、截屏固定300右栏仍待六尺寸正文复现；复用同一Image/Canvas/Tree/属性/色表/Scale对象进行重排，不整页缩放或clone控件，不在resize时重载。full只读盘点建议按正文宽分三栏/两栏/同区展开返回；实际断点要以44px动作和画面视口验证决定，不能把建议当已实现。树↔框/属性、长文滚读、缩放取色、完成crop跨页/resize、保存产物均须真实交互；独立处理已完成截图缓存身份、同row重连、原生保存快照与未完成crop捕获候选，不混进布局。A07..A12和真实双实例/30分钟全部保持，完整goal继续active。最新用户四项列表精简已在8303cdc且现版smoke再次确认，不重复实施或删分组数据。两英文PNG保留不提交，全部旧worktree保留，源包AP021分类仍A12。

## 近期阶段过程留痕

以下是已结束轮次的历史，内部“当前/即时/锁/待运行”均不再是执行指令；最新状态以上方为准。

当前即时锁回root空闲（下面旧锁均历史）：第三生产VM79137...03D4、fixtureE3EF...051B2、hook6F99...F540已main正式150/50.86s、49/49(401/191/49cleanup/9磁盘)通过；root直接核greenJSON AAE6C58255BDF63DDFECA3BB3017FDCB71894744D52C85BBD970721BA10196C4。此前C6+r3真红47/2也直接读核，只有取消两例失败，之后按批准四处SourceExpired最小修正，三生产轮已到不能无界再修。最终单1000完整旧Appearance.Hover(TextBox)首败，run exit1；旧labels36/11已过，其后Native/Filter/Monitor/12idle未到，不能说全绿。AP023已登记，失败原artifacts/apple-debug-context/r3-green保留，main所有exec/owned0已释锁，三冻结源不变。full只在tests树改第四文件AppleAppearanceSmoke.Hover加有限primitive时序与失败stderr，不改任何原条件/等待/输入调用/焦点，不编译；hig仅静审等新patch/raw和明确最终锁，不GUI。此是只读观测非第四生产修复，不把后续未复现倒填原首败已解。下一步root核第四源→hig静审→main机械导入只Build/代表预检或直接独立最终(需明确锁)→主仓整合/150/代表→脱敏文档清单与push；主仓此刻仍只有文档没有新源。完整goal仍active，A06b/A07..A12不缩小。

即时状态（优先于下方历史锁指令）：GUI/build锁已交main，root/hig/full不运行。独立r2基线已完整46：13PASS/33产品FAIL/46cleanup，JSON008BD7EFFCA469C5A5CD69CBF8C5ECC198E939466B3CBED205F48055536FA1BB，汇总69789BA90B9FC2BA314B70505CB9CB921D8BC83DA2839CF6A8B56B98A3C07783，root直接核过。main VM-r2 C6E3A641FAE2035E5756B6E1C41FB72C5BDC17369E1D55E7E8AF8CE9412E6C80/patchD3E86B864882D78D942E62D25F2EA4C5F5803BDA2E86061B5CCAB72CC3C90795被两人静审拒绝：宽OCE catch吞Auto后端取消、Wall丢per-row失败。该版保留未改，full r3只加3case(总49)且原46不变；patch89DC4F8560DA4CC3EF2E7F4E318978DCF5368A6CDB868A79AA42D71015B52ADC、fullrawE3EF3FD5362091CA2C1BC0AF41B12D5AD15FBBA8EB898DC42DD711A7DCD051B2/56959已root核SHA并读51行。main现获仅单次Build+C6/r3短profile完整49回归红，预计保留正常46与新取消差异，实际结果未回，不可先称通过或修改VM。全部进程结束回root后，读实际红再只授权私有SourceExpired类型/RequireDebugSource/Auto只skip本地过期其余OCE throw/Wall其他OCE回原generic固定A失败并继续B。不得在最终第三生产轮失败后无界扩case或循环取绿。hig只准备最终Build150+三桌面49/六shell/设备和监控各390640与1000700代表；所有profile须短，旧原始证据不覆盖。主仓仍仅A06文档未整合源/未commitpush。

最新执行锁归hig独立verifier：r1正式夹具已冻结46case，raw3B516F533F336FEC860CD727E6A2E7F456ED1D490C21D627FA38AE2FD46864DA、增量patch9E171B2B5D96AB21DBA6646ED9F49798A4C25B90B535163B30AF99D3E9811E82。独立clean28c仅fixture+同hook，VM仍0025BF...4E82；Build/Verify正式复取exit0/150(51.54s)，DLL266980993BF9320CC2CF286C0F084C70CFBD86ED40B7B3369F96C53348950FCD。首ignored启动器Task.WhenAll重载错误缺child退出码，保留后仅修runner复取，不算产品失败。首次GUI normal PASS/第二case SQLite初始化14于CatalogRepository39，db258/journal266；余44未执行，不算完整红。root直接读JSON/源码核构造会Dispose失败连接。原件保留，后仅缩短隔离profile到verifier/artifacts/r1-red/data，同DLL/源/断言不变已授权hig单次完整baseline聚合；最长db229/journal237。当前尚待结果，hig未正式释锁前root/main/full不BuildGUI。main已机械导入同r1并保持VM-r1冻结，之后需等锁并单次VM-r1差异预检，再依实际红授权异常修复。最新可见UI四项已在8303cdc实现且两人只读核现行仍满足，不重复改或删分组元数据。

A06a现已取得首类真实红，主仓HEAD28cdd4ded947a4d303ecf39744925faaabb3a314已push。轻规格A06_DEBUG_PUBLICATION.md与AP022限定四条VM链：几何前固定来源/owner/选择+显式revision，正常几何后冻payload身份，有效源cache保留/检查发布另门控/Automatic只快照。实施树r0 VM仍基线时normal6PASS，Discover TCS期间A→B，释放后实际Screenshot B/Dump B、B图/树改/NodeEvents1→3，root直接核JSON，两casecleanup true；Build0warn/0err，详见报告。r0只有两case，末尾PARTIAL不是全绿。hig规格与fixture静审通过，实际MainWindow三段artifacts/desktop-smoke接入已核，不破坏默认路径guard，但默认用户profile未实运行。当前GUI/build锁已明确交main：仅VM生产修复+Build150+r0两例预检（预期PARTIAL后停），full在-tests只扩r1新fixture不buildGUI，hig仅只读等独立树复验；root不Build/GUI。不得提前标修复/提交。下面A05b2是已完成阶段，不执行旧锁指令。

最新main第一版预检已结束并释放GUI/build锁回root（当前空闲，所有代理不自行BuildGUI）：VM SHA123932D470FB842196362DD50DD3488BA28866FE540E7975239570EE76690B1A，VM-r1.patch SHA9AB554FE90DAB5A6F329201F31269B4C2EA69FCB148723EB6B24B527C167C9C3，150/52.42s；同r0两case全PASS后PARTIAL/exit1为预期，B事件1→1、可恢复新B，root读过实际报告与基线log。完整r1仍full仅新fixture补约32+异常归属cases，未冻结/编译；main VM冻结不再改。hig/root静审发现A迟到异常落PresentError按CurrentB错记/污染全局Error候选，先加TCS错误+真实RollingLog flush反例取证再改，不能改全局PresentError。独立策略是新fixture+hook先baseline VM完整红，之后apply冻结VM同预期绿。root主仓还只有文档/规格未整合源，未提交此阶段。

A06后续只读候选仍未复现：Presentation.DeviceRow.Info换代不会给已完成Screenshot存/核身份戳；需判定最后已知画面与输入门禁语义，不能把此缓存复用也称本次迟到守卫已解决。EnsureDebugLoaded首条件HasActiveOperations含Busy会先return，后面的Busy→debugAutoPending分支可能不可达，ABA回原引用后也未必重发；Demo原早退使当前VM夹具无法代替自动首载证明。先后续受控红，不能为取绿删除合法缓存或声称自动功能已验。

A05b2 freeze4已获独立`PASS-OFFLINE-WPF-LAYOUT-OBSERVATION`，仅批准本阶段监控布局及观测；本阶段提交以Git记录为准。完整报告[A05_MONITOR_RESPONSIVE](A05_MONITOR_RESPONSIVE.md)保存所有旧红/夹具错误/首败/摘要失配，下面历史段落不是当前执行指令。禁止把AP019/020更新为已修复或把目标标complete。

- 最终4源patch BE261FA04A241BC3600EACD5993E0E667D30C8B8417314C8249B6628441285E1，manifest BD57272F6EDCD3DCCD429FCCB3466C0DAA9395C7E34BA6B56ECC00459B5EACFB，root/实施/独立raw全同，生产App/Monitor仍freeze2不变。
- 独立150/51.16s、六monitor952/308、六device889/140或847/139、三desktop209/36/73/32/22/25/12idle、六shell135和42CLI全过，64进程退出；汇总verification-freeze4.json SHA60D2646C9F95CACFFA6068943D0B3184C2FC37FA1659016AE3F258462320501E。DLL C7AF612F8019E940FCD6B34008887FFAA3D0CD977E7856EF051774B55045EF37。独立默认配置路径前后缺失，不是真用户配置SHA证明。
- root150/51.26s、fresh五代表(device390640、两390monitor、1000完整、390shell)首轮全过，DLL29EA3F8F7FFF131D5EF06DDACADD0C93365FB267672B4CA640E1624078A7FBAA；已目视390墙/长文/空目录及1000设备。明确运行profile前后SHA仍下方原值；全部exec/GUI已结束，锁空闲归root，代理均不得自行重启GUI。
- 下一小目标A06a：先定义并受控复现LoadNodes的A→B→同一A、目录Generation/Transport迟到；仅证实后小修VM发布守卫并单独push，之后A06b重排Debug/Screenshot。现有DemoBackend sealed且无TCS，复用ApplicationBatchTests的IDeviceBackend转发包装模式；新独立Desktop/STA夹具自建一致runtime（Catalog/Automation/Input/Scripts/API全用wrapper），artifacts数据且API不启动。不能仅换Backend复用旧服务、双重dispose或用Delay猜竞态。
- ABA可走原CurrentDevice setter；代次可由wrapper Discover改变Transport并调用既有RefreshDeviceSnapshotAsync，经真实Catalog增加Generation。此为受控目录，不是ADB wire或真实timer，原timer在Busy暂停。VM无合法Demo热换runtime入口，禁止反射伪造授权取红；runtime热换/真实接入忙守卫仍待独立证据。
- A06b必须保留已完成crop跨页旧合同；只取消未完成手势。窄屏复用原Canvas/Image/TreeView/属性/颜色Grid，不因resize重建/重载。Demo跳过自动节点首载，手调LoadNodes不算自动路径。原生保存/导出dialog期间global状态变化、裁剪失捕等均仅候选，尚未执行/修复。
- 新AP021：旧NewSourcePackage仍收录基线docs/verification历史PNG/JSON/txt，不能称当前ZIP纯源码或完全无原始证据；独立包审提出后已纠正口头说法。本轮不新增上传旧材料/本轮artifacts/ZIP，不删除用户文件或改写历史；A12另做正式材料分类和过滤。SHA/条目核验只证明完整性，后续打包重算摘要以最终输出为准。

## 历史阶段证据

A04最终已独立PASS-OFFLINE共享层、由本阶段提交推送，当前HEAD以Git记录为准：14源文件；A04.patch摘要EF7D087D56AA2CA5AA825483214C715E6679AE48E2BB4322A96952A771432054及仅夹具增量5BB4928E12B68EB4AE9505784DC6DA1AC560F156983D9966DD442A80582849E9。两个新增文件git apply仅CRLF转LF，14归一化内容逐字相同。主仓150/150（51.41秒），最终DLL79E3F633E6594DF811D5F6DB563A849CC534BA9569E470128A09B7CF515771BF；390shell135及1000完整209外观/25守卫/12idle通过，root目视。

独立apple-responsive-verifier最终150/150（51.90秒）、六shell每轮135、三桌面每轮209外观/25Native守卫/12idle，error空、恢复设备一致，目视390壳层/抽屉/实际模态及中大屏；三桌面tab0与最终图SHA相同。证据artifacts/apple-responsive-verifier/final-toggle。首轮1440旧Toggle断言FAIL+r2通过仍保留；仅补焦点前置/真实帧/分阶段诊断后重建重验，生产Theme未改，旧失败根因未确诊。旧modal透明黑底只Content取证已拒，最终取实际Window，系统标题栏像素未验。

所有必需exec/GUI已结束、GUI锁释放；未操作设备/profile/ADB，完整目标仍active。A04报告和P2 AP-011/012是当前范围边界，不可直接外推全部页面或实机通过。不重做A02/A03/A04或将历史绿色替代最终业务验收。

A05a已整合11个Desktop源/测试文件，最终补丁A12E4C2E3134B1D425E9EB04971C2A6CE3AA30EA705FBFB29521D861256C9ED4；与实现/独立归一化一致，仅新DeviceFilterSmoke行尾差异。正式主仓150/150（51.34秒）、独立150/150（51.40秒）；独立三桌面每73专项（含32分类/22实际监控协调）、209外观/25Native/12idle；六公共shell各135。主仓最终1000完整及390shell复验并目视通过，DLL DE075D83B35FA9DA5322AA0CD15731BC29E5F77F0E71D7FDD84E5FA76D4A8D48。详情控件/新数据库回读由实际断言验证，没有独立详情截图或tooltip像素证明；不宣称实机视频连续性、命令权限负例或小屏正文全过。红测4→0和green1/2夹具错误保留。最终报告A05_DEVICE_STATE，本阶段commit/push以Git为准。

A05b文案AP-013/014已独立验收：冻结patch298F8EBACF345A22BB028847B1B90C8B4D95E5A2E40757F5433984A3ED9E5A46，5源raw SHA全同，无分类/门禁/协调器改动。实现150/53.67s、独立150/52.14s及三桌面各36labels/11观察、原73/32/22、209外观/25Native/12idle、390shell135首次全过。主仓150/51.60s、最终1000完整同样通过并目视，最终DLL2EF968DDC1C51C2171246928A50D2ECC693AF497C6D052EDB3190F7B3145395D，用户配置未变，全部exec/GUI已结束。报告A05_DEVICE_LABELS，当前commit/push以Git为准。专项commands never invoked，不代替权限/实机；首次red-1000旧Appearance失败和red2文案19失败/17通过保留，AP-015原因未解。没有声称旧静态Tooltip必然陈旧。

## A05b2迭代历史

A05b2已开始独立worktree实施，轻规格A05_MONITOR_RESPONSIVE：仅监控头部/卡片/图表实际宽及正式Demo专项；main_target_fix持GUI锁，root与其它代理不同时GUI。先红测，冻结后由hig_ui_inventory另树独立重建验证；尚无新通过结论，不提前标完成。其它阶段与安全边界不变。

当前分工：main在apple-monitor-responsive仅改MonitorView/App生产合同，full在apple-monitor-responsive-tests只写新ResponsiveMonitorSmoke，禁止双写；full不GUI，编译时段与main约定。独立apple-monitor-responsive-verifier已从76d0e43准备，runner只Parser通过未执行，必须等冻结和GUI锁。红390实际10项失败，green1剩卡宽时序两项，日志均保留。AP-017仅为异步按钮finally状态候选，留A10单独复现，不混本轮。

当前进展：freshViewport修正后最小390和三桌面预检过，17新入口负例exit1/data未建。full早期r1可视守卫稿尚有后半pending，不算PASS；报告序列化Rect.Empty/Infinity遮住primary，先修日志取红。main为红测暂在自身树使用旧VisibleIn，新生产合同patch仍为SCP正面积相交；最终必须恢复新合同、合并完整fixture后同源重建全矩阵。GUI仍归main，root与hig不能启动GUI，最终7源A05b1不重做。

最新A05b2：r2旧VisibleIn已真实红/新6guard+BottomTop过，仍pending exit1非整绿。r3首个选卡前OSPosition失败未Down，保留未知；hig独立认可仅本条改真实Name/SCP InputHitTest+底层Mouse.MouseDownEvent、观察Surface一次Left/OriginalSource/ClickCount及实际选择/detail/Context，非physical，不动MainGesture/Appearance坐标。r4单卡Height315真实振荡：bar/viewport336↔346、Surface324×303↔330×306、extent315↔318反复并3秒超时。root已批准main只删随宽变video.Height一行，保留原180+Uniform，full补新路由专项；两人仍各自树/文件、不双写。最终同源矩阵和独立复验尚未做，详见A05_MONITOR_RESPONSIVE。

当前冻结已到freeze2：3源patch4F49F46BCC6160BB6792ABAE57B1A5308C61D57210403E83D84B9DD7B084AE54，同main artifacts目录freeze2-sources.json；产品App/Monitor与freeze1不变，fixture r7仅逐分量诊断。r6空目录已补齐，150/51.80s、首640过、844选卡后Context合取失败，原日志缺分量。r7单844一次952/307过，InputGen2→2/Active true，首败原因仍未知AP-019，不称修产品或环境确诊。main正最终Build150→六monitor/三desktop/六shell/六device/CLI，失败即停，仍持GUI锁；hig仅把自己的freeze1机械更新freeze2供静态审，不Build/GUI。full已冻结停止。root尚未apply监控源码，不重复问用户。

最终实现矩阵已经完成：150/51.03s、六monitor各952/307、三desktop原209/36/73/32/22/25/12、六shell135、六device两手机889其它847、17新CLI拒绝/data未建。全部进程退出后锁已交hig；独立150/51.41s通过，DLL35A9EC13D55A9256921BEF4F730DABDE9B36DF96D62BC6C54BDC9A3EF398CC77，其余完整矩阵进行中，未最终批准。main DLL0BADAE9D9A94147097E6B443A089EF921FF19A53FBE8539F0C6C56893418807E；汇总freeze2-verification.json摘要6AC299AD17EB96611ED2D61FD1919A8E46EAD21FB7E02E34B334EFF79B47AA77。root已机械apply3源且raw精确核同freeze2-sources（App A8522C...、Monitor1281ED...、新fixture0672BB...），尚未构建/GUI。root ignored artifacts/apple-monitor-responsive-root/Run-FinalGui.ps1已Parser过但未执行，拟两390monitor+1000完整+390shell；必须等hig全部exec结束并释锁再Build150/主仓复验。README/报告已补实施结果，独立/主仓状态仍待验，不预称实机或全目标完成。

当前实际阻断：hig freeze2首device390640 625checks/73obs于旧ResponsiveDevicesSmoke:188(workspace919后)五合取失败，未Position/Down，无分量，不能与AP019并根因；其余5device/42CLI未跑。独立REJECTED-INCOMPLETE、verification-partial-freeze2.json及raw保留，所有进程已退出，GUI锁已交root且root未使用。full现仅在tests树改旧ResponsiveDevicesSmoke只读有界事件/五分量+返回分量日志，不改条件/等待/基线/生产、不编译/GUI；main只读追五写入来源，不改3冻结源、不GUI/build，等full诊断patch。hig待新诊断与重新发锁。root不得提前push/complete；需机械合并第四诊断源、定位后新的冻结/完整验收。未重复问用户。

最新续验：full第四文件诊断r1经main/root/hig静态审查，原断言/等待/基线不变；main机械整合freeze3四源patch94F7CA28B49B94715E9B8AE66BCD9E5012173DC261BAE5C9465BBDC6FCA3ABE9，单次Build0warn/0err和旧device390640过889/140，54事件全保留，919/920五分量及返回三分量true、gen2→2、Active true，边界前无失活；先前首败仍未知AP019，不能归因。main全部结束并释锁后，root已机械apply第四源并核4raw同manifest（第四6AE200...），未Build/GUI。当前GUI/构建锁明确交hig：freeze3独立Build150→先device390640，若通过继续其余5device/42CLI/六monitor/三desktop/六shell，失败即停不重试。root/main/full不Build/GUI；root仅文档/静态准备，full只读A06验收设计。当前阶段未验收/未push，配置摘要复查仍原值。

freeze3最新独立已停且正式释锁回root：150/51.16s，DLL9EDFFF23C4891D30980C0CAB03FECF79E8BFA8998D51E641530E4F3857AD62F3，六device全过(两手机889/140、其余847/139)、42CLI全部拒绝且未建data。原390首调度因ignored runner tuple展开在Desktop启动前失败，processResults空；修runner后fresh实际单次，非产品重试。前三monitor各952/307过，1000×700 demo-coordinator在focusWindow三合取首败939/304，缺child/owner/MonitorActive与Activate返回，另登记AP020不并AP019。余两monitor/三desktop/六shell未跑，verification-partial-freeze3及所有原字节保留，独立REJECTED-INCOMPLETE。root仍未Build/GUI。full暂停A06只读，现仅给新MonitorSmoke加最小只读焦点分量/有界事件，原调用/等待/断言不变且不编译。main/hig无GUI；等patch静审后只做单1000诊断，不默认循环全矩阵取绿。

最新freeze4已主仓代表复验通过：最终增量6FE736...706B只加焦点日志；交付时同路径替换曾使旧摘要失配、main先build实际最终稿但未GUI，hig拒绝后root重新hash/审稿并获独立静审才放行；编排失误见正式报告，不声称旧摘要验证通过。最终四源patch BE261FA04A241BC3600EACD5993E0E667D30C8B8417314C8249B6628441285E1、manifest BD57272F6EDCD3DCCD429FCCB3466C0DAA9395C7E34BA6B56ECC00459B5EACFB，root/main/hig四raw全同（新MonitorSmoke797964...2848），生产App/Monitor仍freeze2不变。main单1000过952/308，Activate true/三leaf和四因子true，6事件无drop，合法失活gen2→3；AP020首败仍未知。root150/51.26s、五代表fresh首轮全过(device390640889/140、两390monitor952/308、1000完整、390shell135)，DLL29EA3F8F7FFF131D5EF06DDACADD0C93365FB267672B4CA640E1624078A7FBAA；已目视390墙/长文/空目录与1000设备，配置SHA不变，exec49106/39506均结束/GUI0。root已将独占GUI/build锁交hig最后freeze4全矩阵：先1000monitor再剩5monitor/六device/三desktop/六shell/42CLI，同binary无重复1000；失败停不循环。其它代理/root不buildGUI，未最终批准/未push，完整goal仍active。

A06只读准备：Debug固定star+300+270、截屏固定300侧栏；推荐同组件窄屏分段，resize不重建树/颜色/已完成裁剪。候选先红测：LoadNodes只核DeviceRow引用的A→B→A/代次/runtime迟到；保存原生对话框期间全局图/裁剪/root变化；RefreshScreens清cropStart未释放捕获以及离页/LostCapture。不能因导航取消已完成crop，旧smoke明确要求跨页保留；Demo跳过自动首载，手调LoadNodes不算该路径。仅源码候选，未执行/未修，不缩小NODE/CAP实机要求。

A04实现已统一四契约与默认值，完成导航699/700与1099/1100、Field自身460、More菜单、Owner-fit。六尺寸只证明公共层，三尺寸完整smoke业务断言保留；A05-A09固定多栏不得冒称适配通过。AP-011首次Fit失败事件订阅可能残留、AP-012页脚ellipsis/tooltip在触屏不完整，两项P2已登记A10；不把这些路径算已修。

A05a和文案已完成，A05b1本阶段提交：紧凑选择48/操作148冻结两列，44实际命中、原14信息横滚保留；workspace920切同一主控侧栏/同区域、固定返回与自然正文滚动；MainA/CurrentB/隐藏选择/原滚动意图保留。resize/LostCapture/离页等取消未完成手势，正常Up冻结局部快照后release。下一步A05b2监控330卡/308图表按实际宽，不能伪造资源采样或重建视频。AP-013/014/016不重做；AP-011/012/015及未复现Tab/指针前置候选仍A10。A06-A12保持，不将离线绿色替代实机与全页小屏验收。

A05b1最终7源patch摘要25CD4082425831E6803D6237C33088569ACF69DD49437986D451815E1F92C5E8，source清单070423D33AACE96F26EC4952DD1744C6AF48340C8F4CAF0898C884830BC21032，root/实现/独立7源raw全同。实现150/51.13s、独立150/51.98s；两方各六正文（两390各889/136，其余847/135）、三桌面36/73/32/22/209/25/12idle及六shell135全部通过。独立20入口负例拒绝；其采集器UTF8误读中文stderr，不证明console中文编码。root最终150/51.10s，首轮390640在normal-tap Position前置失败656checks、未Down；后验输入持续变化/当时采样全屏Clip，未确定首败原因。未改源/断言或禁用用户输入，等5秒会话idle后新目录一次复验：两390各889、1000完整及390shell135通过并目视；首败保留，不能称root首次全过。root DLL D4F9CD5A3218D19189E2CD0D3E31152AF52095FE2F29EA8EF4C3CBE554FF03F3，用户profile摘要未变，全部exec/GUI已结束且锁释放。完整报告A05_DEVICE_RESPONSIVE；本阶段commit/push以Git为准，不称实机、自动timer、ADB wire或DPI通过。

本阶段真实红另含header resize grip遮挡、默认非null TextColumn样式使tooltip未装、旧队列恢复盖新wheel、390844中间宽视口maxV5消费原V6快照；均保留红绿。保护行隐形RowHeader低层hit、Number类型、指针缓存/前置、无法构造Image内部gutter、构建与启动交叠均分开记录，不混作产品缺陷；默认外侧父Grid留白和capture拖出实测通过，内部gutter N/A。A05b2仍未实施，当前阶段完成并非全目标完成。

AP-015只读诊断线索：Appearance的probe经AsyncCommand在Space Up时同步CanExecute false→true；可能影响focus，但Task.CompletedTask无必然让出，不能据源码断言丢焦点。模板需要FocusWithin与AnimationsEnabled同时成立，旧恢复断言前没有当前Active/Focus证明。A10首轮仅加覆盖失败finally的有界事件时间线（Click/CanExecute/IsEnabled、Got/LostFocus、Activated、偏好与Rendering，记录实际FocusedElement、Opacity/clock/Popup及阶段）；不补Focus、不改主题/命令取绿。若仍focus有效且有帧却无动效，再查触发器；若失活先发生则分离前置失败。必要第二轮用同步/完成Task/延迟TCS控制组，不能换掉原probe冒称修复。

A01目录为103个页面可见键，加全局/源码补充和逐字段细表，全部未执行项维持NOT-RUN。已纠正不存在的终端取消、文件目录编辑、API归档、脚本编辑等推测，不新增这些未要求功能。当前工具页是代理解析、root核验、时区和本地资料包，不是设备备份恢复。

## 测试与环境

便携SDK：上级.tools/dotnet/dotnet.exe；pwsh7.4.20，不升级。Build.ps1 -Verify、Desktop --smoke使用artifacts内独立配置且GUI串行。原始图/日志/数据不上传，正式源码/测试/脱敏报告及三份生成清单提交。不要强制关闭用户进程。

只读ready检查（不是最终目标证明）：雷电23实例，仅5运行，专用1/2关闭；共享5037为E:/leidian/LDPlayer14/adb.exe，PID62248、出生2026-10-15T09:23:48.773808+08:00。原始日志artifacts/apple-experience/readiness。真实动作前重新冻结，不沿用该旧快照放行。

用户profile data/monitor-verified/AppConfig.json摘要E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0，autoStartApi=false，授权1/2/3/5；保留现有变化，不回滚成旧摘要/旧授权。

AP-008：仍无完整AndroidSDK或自有测试APK；已只读取得Central framework API16、Google Maven AAPT2 8.6.1、D8 8.6.27、apksig 8.6.1候选工件并核checksum/POM/归档，证据AP008_DEPENDENCIES。不要再称“无android.jar候选”，也不要把候选链称为可用构建。D8 manifest入口缺失应走官方class入口，apksig没有apksigner CLI；未执行、编译、签名、接受SDK条款或改环境。A07下一步仍需自有最小源码/签名/Android效果，不能拿抖音清数据/卸载凑覆盖。

A07额外只读准备：fixed apksig public member有setAlignmentPreserved、V1/V2及Verifier范围；官方当前源码的4字节对齐说明不等于8.6.1工件效果，必须最终ZIP LocalHeader offset与签名验证。无res原生Activity/匿名监听器/Prefs回显、min23/target34/exported/无权限是最小候选；framework不含java/javax，D8 JDK-provider实际成员存在，但精确Temurin根路径当前尚待恢复，不能假装已具备完整JDK。工具执行/生成测试密钥/构建APK/安装均未做。不要全盘扫路径或借用用户APP数据；候选研究不算AP008解除。

Temurin精确路径只读复查仍未找到：限定主仓dependency-inspect/旧artifacts文本及上级.tools，无可信JDK或安装器路径/SHA；旧“启动器签名有效”缺可复核路径，不能沿用为已有完整JDK，也不意味着全电脑不存在。没有全盘扫描、下载或执行工具。

## 保留工作

两个原有未跟踪文件docs/verification/desktop-english.png和desktop-english-settings.png，不提交、不删除。所有旧worktree不清理。不要回滚用户/代理变更。每次push前检查清单/ZIP排除data/artifacts/bin/obj、APK/SDK/二进制并核远端；分支codex/unified-device-access，私有zhuy3075-ui/matrix-device-ldplayer。
