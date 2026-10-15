# A06c1 已完成检查缓存身份

日期：2026-10-16。基线：c7b6d24258699d975310b902f7a66f0380d8932c。

状态：生产r1b和正式夹具r1已合入主仓，独立最终矩阵与主仓代表均已通过。交付须继续经过末节冻结、核包和独立审查；实际提交/推送以Git及本机交付记录为准。以下设计/基线/诊断均保留原阶段事实，最终结论只以末节实际结果为准。

## 目标和事实

承接完整12页验收，不把已推送的截屏正文适配当作缓存或实机通过。上一目标轮已完成真实源码变更、独立验收和推送，是有效进展；本轮开始先重读当前Git、任务、规格及源码，不重跑旧失败来制造新进展。

基线源码在进行中的截图/节点请求上保留Owner、Row及DeviceInfo身份，完成后Screenshot、DebugImage和nodeRoot没有随结果保存身份戳。目录按Serial复用DeviceRow，Info通知只更新上下文/命令/设备墙；CurrentDevice同引用早退，PNG/JSON本地导出只检查内容非空。它们在设计时是待验证候选，后续独立基线原红见下文，不以静态推断代替运行结果。

旧49专项明确保留离线设备已有检查，以及合法后台缓存返回。离线本地查看和导出不等于设备写权限，不能以全部清缓存或禁止本地保存取代该能力。颜色采样列表仍按原历史列表保留，不趁此重做UI或数据格式。

## 范围与方案

本项仅处理已完成内容遇到真实身份换代后继续作为当前检查使用的问题，先取红再决定修复。范围包括Ready到Ready传输/几何换代、同Serial离线后重连、新目录已变化但VM行尚旧的消费检查、正常恢复及有效内容负控。

推荐在VM私有记录完成缓存的来源身份及Bitmap引用，当前检查与行缓存分别记录，复用原Owner/Row/目录/身份校验；VM同步目录时撤下新身份不适用的内容，返回缓存及PNG/节点导出消费前再次核验。正常同身份刷新、metadata-only修改、合法后台源缓存和离线历史必须保持。

更简单的Info每变必清会误伤改名与离线历史，也漏掉Catalog-only变化，拒绝。只用Serial或Generation不够，不能区分传输、几何与runtime-owner。历史图库或整套新状态机超出此缺陷，不新增。不得推进显式请求revision而取消正常RefreshGeometry的新旋转基线，也不在失效清理中主动调用LoadNodes；同row自动首载另项验收。

离线历史许可的最小合同：来源为当时在线且已接纳的内容，当前同一Owner/Row、Serial/Transport/几何不变，状态明确为`Offline`且代次恰为来源Generation+1；重复离线发现不增代。row和Catalog双方必须各自满足原在线身份或该精确离线历史资格，不能一方离线便忽略另一方的新身份。`Unauthorized`、`Unknown`等非在线状态不自动获得历史许可；保存历史后也不把来源戳改成Offline。回到在线后必须严格匹配原身份，重连的新代次不能恢复旧检查。该合同已独立静审，仍须冻结红测、实现和旧49运行验证。

生产候选限定WorkspaceViewModel；正式测试新增独立完成缓存夹具，MainWindow只接Demo验收一行。原16/49/11/10、Core/Host/API、输入代次和主题不改。真实runtime热换没有合法Demo入口时不反射伪造，保留实机缺口。

## 验收与顺序

1. P0：冻结独立夹具，实际完成A截图/节点，随后真实Catalog推进传输/几何或离线再重连，证明同一row、旧引用/标记与新身份；基线如失败保留首次证据。
2. P0：目录同步后无新设备读也不能把旧图/根误归新身份；Catalog-only新在线身份但旧row前置成立时，返回缓存、本地PNG和JSON消费必须识别失效，不创建错误产物。不能声称Catalog-only时UI即时推送失效。
3. P1：稳定刷新/metadata-only、合法后台缓存A返回、MainA/CurrentB隔离、离线历史图/根及真实本地导出保持。新显式请求可恢复正确内容；Busy/错误路径按原合同，不无限重试。
4. P1：保留原进行中49、首载16、节点正文11、截屏正文10与完整桌面原套；每个自有case有断言、实际产物/像素或节点标记、清理与原始日志。夹具仅一致Demo/STA、独立短artifacts，不调用真实ADB/API/实例、剪贴板或原生保存框。
5. P1：独立verifier先红后绿；重要变更最多三轮产品review-fix。实现、测试、验证分别独立worktree，Build/GUI一次只一个执行者。
6. P1：主仓代表复验、README/正式报告与清单、实际源码包逐项校验和独立交付审查后，单独commit/push原私有功能分支，不合并发布；随后继续其余缺口。

## 非目标与风险

原生SaveFileDialog期间快照、未完成裁剪/失捕获、同row自动首载、真实DPI/物理输入及Android分别另验。本轮不读写CODEBRAIN，不修改原实例0、账号数据、共享ADB、代理、证书、权限或实际配置；不启动普通程序。两用户PNG及所有旧worktree/失败材料保留，不自动清理。完整目标继续active。

其他失败模式：合法后台新帧与当前较早图/节点可属于同一身份，不能以DebugImage必须等于row最新Screenshot作为有效性条件；失效截图不能覆盖NativeView写入的scrcpy/嵌入状态；缓存条目移除不留已删除行。metadata-only重复Info通知不得重复清节点或选择。

## 冻结验收输入

正式r0夹具`Views/CompletedDebugCacheSmoke.cs`329行/41187字节，raw及LF规范化SHA256 `C79EE9D6694B2BE1902BEFAC72D8FB66682BB055C2F8560346134C55040AD952`；Main仅Demo guard后一行接线，raw `A18780148E131FFC2E95E23C6158E8CB78CA32A73AAF2016A15C2EA6EF04E34A`。累计补丁`apple-completed-cache-tests/artifacts/completed-cache-contract/completed-cache-r0.patch`42464字节，SHA256 `E4029A0DE1569807C233826FFE1942C7CBB4C3452F7EF099576931972E1A1B97`，原件及freeze-r0.json保留；root与独立verifier均实读，未改原16/49/11/10或生产VM。

固定18例：Ready传输/尺寸/旋转3，离线历史及重复离线导出后重连1，非当前隔离1，真实metadata稳定1，Catalog-only返回/同row/PNG/JSON4，当前检查与较新行缓存1，合法后台返回1，Unknown/Unauthorized2，NativeView状态文本种子2，重连再离线不得复活1，离线几何变化1。NativeView文本种子不证明启动或嵌入；旧产物按实际文件SHA/PNG完整像素/JSON根标记观察，不依赖任务成功返回。新wrapper调用记录仅证明Discover/Screenshot/Dump读边界，不声称全部guest写已计数；源码无新增真实环境操作。

所有case使用真实Catalog→原VM→已显示自有WPF投影和短artifacts目录，没有新增Demo自动许可、物理输入、原生保存框、剪贴板或runtime热换。普通产品断言失败且清理成功才继续独立case；初始化或清理未完成即记录并停止，已接纳Task/TCS有界排空，原异常和清理异常都保留。

独立基线runner`artifacts/cache-verifier/Invoke-Baseline-r1.ps1` SHA256 `A3BC4D7FFE01DEC63544BA06F1CF42CEB545971526BC2CA0D2BBCD43FDC1B4C8`，203行/Parser0，root实读相对已通过Screenshot Final-r1的完整差异。保留240秒/单次自有终止/10秒drain/源码及配置前后核验，Smoke真实预期exit1并不等于产品红。新journal按实际短路径117字符，旧49保守最坏247字符，小于260；未知结果不预填。

## 独立基线实际结果

首次Build150/150（51.67秒）、0警告0错误，阶段75.87秒；独立DLL `62D8A38CC89A3B8D01C094495108BDD3FC4C2B160E224A7B48AD2E5066413408`。同DLL一次1000×700 Smoke真实child exit1/8.312秒，18例完整聚合为3通过/15产品失败/18清理，168检查/137观察。没有编译、夹具前置或清理失败，没有重试。

三个完整正控为稳定metadata、后台缓存与当前检查分离、合法后台缓存返回。离线及重复离线历史正控先实际导出PNG/JSON并核像素/根标记，随后重连仍保留旧内容才失败；不能把整例失败倒写成离线能力不工作，也不能把首败后的恢复等步骤填绿。其余15失败在对应换代、缓存返回或消费断言发生，均已保持真实目录/同行引用证据。

Catalog-only两消费例中，VM行仍为Ready/Generation1/Transport11，而实际Catalog已为Ready/Generation2/Transport12。旧版本仍显示Error空和“保存截图完成 / 导出节点完成”，实际写出`c09/stale.png`（160×240，1216字节，SHA256 `1A6CEE71D51222D620583A84AE1BDA25FB9A4E1C4F1A4AA03791C09F491BAA6B`，完整像素SHA `88C8DF875080B9C4120F042E85450F0334608CD166204040D1A3576A6AA1B795`）及`c10/stale.json`（1193字节，SHA256 `A9527D48C6ECA3DEDE40D61544CD8375ED47B6549289F725D18556CE8719960F`，根仍为`nodes:DEMO-001:3:t11:160x240:r0`）。这是本地检查结果来源失效，不冒称Android越权写入。verifier实际重开9个产物核验，root直接读取18例原JSON、两个错误文件及反馈并目视旧PNG。

原报告`apple-completed-cache-verifier/artifacts/u/s01/artifacts/cc/7a301fde/completed-cache.json` SHA256 `550B22496CC142391D1AED878513F1152D142D420AA74BB881F14FBBC0BE38FC`；摘要`artifacts/cache-verifier/baseline-r1-summary.json` SHA256 `7E022FD990EACF14A3EDDB25842F0861F99E5D5B5155F260E70426FC38AFDC0A`。四份前后快照125源完全相同，旧16/49/11/10 raw/canonical不变，实际配置仍`E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0`，自有exec全部结束、Desktop/dotnet进程0后归锁。新fixture gitapply产生CRLF时先核全文规范化相等，再机械复制已冻LF原件，import-r0.json记录，不改旧测试文件。

新专项汇总throw之后的旧10/11/16/49和完整桌面套未到达，均NOT-RUN；正式夹具原条件冻结不改。现只授权main在独立树首轮单VM最小修复并冻结，不Build/GUI；verifier仅准备expectedExit0的最终runner。最终同18例、旧套及主仓代表复验还未执行，不能提前称修复通过。

打包helper已仅作阶段路径/源清单/术语替换准备，`artifacts/completed-cache-package/New-FrozenCompletedCacheSnapshot-r0.ps1` SHA256 `562EDF081BD51CE35E80E78A9A80CF08A89EF2B9B537D91F59FF762C970C6B8E`，18259字节/Parser0；root实读相对旧2539helper的4个hunk真实差异。尚未执行、生成快照或回写清单，必须等待最终产品/根验收和10文件冻结。

## 首轮生产候选与观察边界

首轮候审r1b只改VM，63行新增/9行删除，raw SHA256 `41E4F7E6E82689E708F2577929BE5F5FF3E685A255C6A383A4AD79FABFDEA9CA`；累计补丁 `7177D2EB2072FC06E26C91DADDD883324F7B1007DD23BA7E75D24B466EC8CDF5`。r1为首次构建前的检查点，r1b增加非当前失效源的精确“节点可用”状态复位；两原件保留，不记作两次已执行的产品修复。root及独立verifier已静读，正式18例及原四套未改；已授权独立首Build/1000首错停，再继续其他矩阵。运行结果未出前不标绿。

完成源分别用于行截图、当前检查图和当前节点根；返回同row、PNG/JSON保存和取色再次核验。失效不推进请求revision、不主动首载、不清颜色历史；只复位仍属于截图/节点缓存的状态文字，NativeView或后续其他诊断不覆盖。本地写入开始前冻结已核验像素或JSON，之后换代不删除已经接纳的本地文件。

本项历史资格是消费时的Owner/Row/Catalog快照字段合同，不是完整状态时间线。独立挑刺指出：仅Catalog曾从Ready经Unknown/Unauthorized到Offline，而中间VM既未同步也未消费时，现有Catalog可能仍是原代次+1；单VM无法从终态重建该中间状态，未执行的这个串联不称已复现，也不宣称能证明全程仅发生一次Offline。消费时可见的Unknown/Unauthorized会拒绝，已同步失效并清除戳的内容不得在后续Offline复活；离线历史只允许本地只读，不放宽新设备读取、输入或身份授权。需要中间态不可逆撤销时，须另行验收Catalog状态事件/版本合同，仍列为完整目标的开放边界。

上一用户请求的列表精简已另行复验而未改源码：当前c7b6d24重新构建150/150、1000完整和390正文889检查通过，源码124项及真实配置前后相同；证据在主仓`artifacts/list-recheck-current/`。一次GUI调用的占位哈希参数在脚本入场前被拒，保留记录，随后首次实际Smoke使用真实DLL执行；不是产品失败或重试。它不替代本项缓存修复验收。

## 首轮实际执行与待诊断

独立r1b首次Build150/150（52.17秒）、0警告错误，DLL `EE6E38F98C95F31E8A9EEFCF2AAD9459C5FADACF34865AFD4A0A3E53638CADAD`；首次1000实际exit1/6.251秒，18例14PASS/4FAIL/18清理、274检查154观察。四败为Unknown、Unauthorized、重连后再离线、离线变几何，均先通过行缓存/当前检查撤下及真实投影断言，随后在原夹具`ContextSame`合取失败。缺七分量诊断，不能直接称4产品缺陷，也不能称本轮验收通过。其余桌面、六shell及新专项后的旧10/11/16/49和原全套未到，NOT-RUN。

Catalog-only两消费已拒绝旧文件，实际Error分别为“保存截图失败：截图来源已变化，请重新刷新截图。”及“导出节点失败：节点来源已变化，请重新刷新节点。”，Status为“操作失败”；随后显式刷新恢复且Error清空。独立重开11个正向PNG/JSON核完整像素和节点标记，未以函数成功代替产物。14通过包含稳定metadata、离线/重复离线导出及重连恢复、非当前B隔离、当前旧检查与后台新帧分离、合法缓存返回及Native状态文本种子。

实际原报告`artifacts/v/s01/artifacts/cc/7549f7e4/completed-cache.json` SHA256 `546B981ABB214D273A0619288005047411EC688017CE69452C54722452726C03`，独立摘要`final-r1-first-summary.json` SHA256 `EC329D3A041D4F14D7871BCC2E47E028171901F2DA4C9A2C97A3DCB7A55649E6`。125源/原四套/配置前后相同，全部会话结束且自有进程0，无超时、终止或重试。root已实读原报告、四败堆栈及上下文相关源码，暂只授权夹具加入七分量诊断并保留原合取和18场景，不修改业务或目标门禁。

待诊断假设是夹具将`Targets()`有效命令集合误当稳定选择意图：原VM在存在已勾选但未连接实例时按安全合同返回空集合，四例结尾均非在线，不能为保持测试旧集合而放宽该保护。未取得分量证据前不确诊该假设；原r0及首轮所有证据保留。

### 诊断原断言与分类

r0d只增加只读七分量观察，原`ContextSame`合取、错误文本及18场景逐字保留。fixture raw `276E4555A49221D9C727C1B548A7D783D69DAC05F46BB40EC1610494DC1F84E6`，增量 `DF7336FDD1AA08646D079750ECA73C6D7F13336C768667DBD671123FFA197DBF`；诊断runner `69E945E7ABEE61AB5BD4CFF870D5C5A4AE97D3778A249A72878964B47855135A`仅改固定fixture、独立证据前缀和Smoke预期聚合exit1，不把退出码当产品结论。

诊断首次150/150（50.92秒）、0警告错误，DLL `89385E741A481F74B08A244C80716BFA5B3012057F3A3A080D67CC444CE639CF`；一次1000真实exit1/5.991秒，18例14PASS/4FAIL/18清理，274检查167观察。四例七分量完全相同：`currentSame/mainSame/selectionsSame/colorsSame/inputGenerationSame/busyReleased`全部true，只有`targetsSame`为false；原目标A/B，实际有效目标为空，两行仍勾选，A分别Unknown/Unauthorized/Offline/Offline，B仍Ready，输入代次均2到2。root直接读四例原分量与门禁源码，独立verifier确认原统一目录会把已选未连接计数传入`SetPendingLaunchSelection`，因此原`Targets()`安全地返回空，而不是偷换或清空勾选。

诊断报告`artifacts/x/s01/artifacts/cc/18a80e7d/completed-cache.json` SHA256 `B4CFC673A9B76A3F5866ABC2A344258573EB65FA5706A6078EDF7AC55A7853EB`。125源/配置前后相同、全部会话结束/自有进程0。原合取仍失败；据此四败分类为新夹具将动态有效目标误当不变意图，不改写原首轮为通过。仅授权r1夹具将不变行/勾选与有效目标分开，并增加离线目标为空及写操作禁用断言；生产r1b仍保持同一SHA。最终完整绿验收尚未执行。

### 正式r1纠正合同

最终候审fixture raw `EB02937763123BC25CE118515091BB9B0074825F028050846E446219D56EF516`，45156字节；r0d到r1增量 `449ABFDA7E5B7B2A18367442BD9BE2EED324EA6BE8E158945936AB8162E401CC`，累计含Main一行接线补丁 `D6569097C7594FBB3C30B8B5971F9B60C9F2A47A2F446DCEC3D2E762B220A9D3`。root及hig分别全文审差异，另实际比较证明Snapshot之前全部18case正文及RowCacheSame之后全部helper、产物、Factory/Backend与等待字节不变。

r1增加原设备/统一目录行引用、顺序、key、布尔勾选冻结，不再用动态有效目标代替意图；保留原七叶的`targetsSame`原值诊断。根据冻结勾选及原行当前Online独立计算预期：存在非在线时必须空有效目标并且`CanWriteTargets=false`，否则仍与原Targets序列精确一致，明确限制为本夹具无LD绑定的设备行，不泛化真实雷电授信。所有缓存撤下、离线正控、本地PNG/JSON、恢复、调用边界和清理断言保持，生产r1b没有再改。

Final-r2 runner `BAC41EE0285D8AC2F2441EC3959ADA85892E087ABAEC3855B7F1D600520A1234`相对Final-r1只改固定fixture SHA和独立证据/profile目录，所有真实预期exit0；根代表runner `764E5D7C1A00DDFA6DC12A546E095CFAC23409AE793BF4EFEB32AC23274B7811`仅根路径/原四套root raw及同样fixture/证据适配。两个runner都已独立静审，先独立完整矩阵，再主仓代表；尚无最终运行结果时不预填通过。

## 最终独立验收

Final-r2在相同VM/Main及已纠正合同的r1上首次完成全矩阵，没有再次修改生产、正式场景或原四套。结果为PASS-OFFLINE-COMPLETED-CACHE，不是实机或全目标通过。

| 范围 | 实际结果 |
|---|---|
| 正式构建/协议测试 | 150/150，50.98秒，0警告0错误 |
| 1000×700完整窗口 | 首次exit0，88.618秒；18例/295检查/163观察/18清理 |
| 1440×900完整窗口 | 首次exit0，88.516秒；相同18/295/163/18清理 |
| 1920×1080完整窗口 | 首次exit0，111.207秒；相同18/295/163/18清理 |
| 原四套及完整路径 | 三窗口每次原截屏10/630、节点11/905、首载16/137、发布49/401全部通过并完成各例清理；外观209、目标25、筛选73/32、文案36及12页idle，Error空 |
| 六公共布局 | 390×640、390×844、768×1024、1000×700、1440×900、1920×1080各135检查，真实HWND及实际逻辑尺寸准确 |
| 本地产物 | 33个实际PNG/JSON逐一重开，核完整PNG像素、节点根标记和文件SHA；无旧stale产物，来源失效Error/Status明确 |
| 不变性与清理 | 20份前后快照125源字节一致，旧四套raw和canonical未变，配置E4E33A8...BB05F0；全部exec结束、自有进程0，无超时/终止/重试 |

独立DLL `05EFB7C76CA34925DEFF96F0E4C19DBA054B19983E8CDD3A3B59CE014B1160D7`。正式摘要`apple-completed-cache-verifier/artifacts/cache-verifier/final-r2-summary.json` SHA256 `04A22C400BB7D9605D309357CEBC5D2F89050041D1388A6E76AF1E4BABE3971D`；root已读三桌面原专项/反馈及完整摘要，未只看退出码。1000新专项原JSON `37A9CC150E48E1FFC5F140A2BD3E8D93BF2A20D92D7CAB82A17A74688AAC8949`，其余路径和文件摘要在正式本机汇总。

外层`desktop-smoke.json.colors`三次实际均为0，不能填成旧值1或称外层取色动作成功：旧OperationGates和Canonical夹具直接把Info置为原代次Offline、之后只还原Info，没有更新Catalog或重新读图；本修复按严格身份撤下该缓存，默认Demo定时器/自动首载关闭，后续外层SamplePixel遇空图返回。没有手工补Capture或改旧测试取绿；新10专项的已加载源取色及本项18历史颜色各自通过，不代替这个外层未执行效果。同row换代自动补载仍是下一独立任务，原生保存/在途crop、真实runtime热换、DPI/Android、A07..A12及最终双实例30分钟仍开放。

## 主仓代表与交付

root按两个累计补丁机械合入三份冻源。新fixture在gitapply后raw为`BACF3D336D92EFF2830ED26C385D19695F23CCB59F95FAC5B0B13C754E0A9AD4`；先全文CRLF归一比较与冻LF原件精确一致，再仅该新文件机械复制回EB029...F516。VM/Main分别与41E4...A9CA/A187...E34A精确相同，旧四套未改换行。导入事实见`artifacts/cache-root/import-r1.json`。

主仓首次150/150（50.96秒），实际构建日志为“0 个警告 / 0 个错误”；一次1000完整exit0/87.585秒，新18/295检查/163观察/18清理，原10/11/16/49及完整流程、12页idle/Error空；一次390×640公共布局exit0/135检查。根DLL `9E381B9DAFF994B54CA6CFA6F19F94D272A7806F33C9427FC63C4B2A74CA249C`，全部必需exec结束，自有进程0，六份前后125源相同，实际配置仍原SHA。外层colors同为0，保留同样边界。

root已直接读原18例、旧四套及公共布局报告，实际重开代表离线PNG与JSON；历史PNG完整像素哈希匹配源`88C8DF875080B9C4120F042E85450F0334608CD166204040D1A3576A6AA1B795`，重连新源匹配`9D534137D17B47C3B234C53BE1C24415D1FB81901FA65E5E9703ABCF7DA1F827`，JSON根为当时合法源标记，不能把保留本地历史错写成仍可向离线设备发送输入。根原专项JSON SHA256 `114779FAD06731A087AF459FD216BEEC330AAC629BC2913DCB5F532CCFA6A1DC`；汇总`artifacts/cache-root/root-summary-r1.json` SHA256 `57E17C7A91A60A0A7310FFBC5E64B4B77B55F358A2CA42A2DE0453C69962F4BD`。

根只读汇总过程曾误用PowerShell不存在的Select-Object参数、误读旧报告cases而非reports及英文警告行；均在生成汇总前纠正为已读实际schema/中文构建日志，不修改源码/断言，不重跑产品，也不计为产品失败。正式失败、诊断失败和最后实际通过记录分别保留。

交付限定VM、Main一行Demo接线、新正式fixture、README、本报告、任务/交接/进展/问题/矩阵共10个完成文件，加3份生成清单。冻结这10份raw后，使用已审562EDF...C6B8E helper只在隔离快照打包并逐源/ZIP核字节，仅回写三清单；独立审查脚本r1 `68509E45C80ED33E7534AF56DF2CF963BE98F6FE9AF123090E8B42A98ACC3D89`核10冻件、13 staged、旧四套canonical和实际ZIP/Bundle/Tree后才允许提交推送。来源是tracked加明确新fixture/本报告，排除两用户PNG及其他untracked、data/bin/obj/APK/SDK和本轮原始证据；AP-021旧tracked材料仍保留，不称纯源码或历史隐私净化。

本机最终核包/审查和实际Git记录分别在tests树`artifacts/completed-cache-package/`及主仓`artifacts/cache-root/push-complete-r1.json`。应读取实际记录并核Git/远端，不凭本节交付流程推断已经推送。只向原私有功能分支单独push，不合并、发布、部署或清理worktree。整个12页目标仍active；下一阶段先同row身份变化自动首载，再原生保存/在途裁剪与A07..A12。
