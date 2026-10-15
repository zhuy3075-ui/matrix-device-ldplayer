# A06c2 同一设备恢复后的自动首载

## 目标与事实

基线为已推送的 `90815e8cd05a6b2b4574dd7ca8213da0f8a41835`。A06c1已经拒绝旧身份截图和节点继续充当当前内容；本阶段补齐当前行引用不变、设备恢复或Ready身份变化后的自动首载。现有导航/Current变化产生pending，HasActiveOperations释放只恢复已有pending；完整目录同步并未产生同row新身份意图。这是源码候选，首次受控红测前不称已复现。

保留原12页、Windows WPF、列表模式、权限/身份守卫、接口和真实设备安全边界。不得清空账号数据、操作原实例0、调整共享ADB或本机实际配置。仅使用自有Demo窗口与一致Backend/Catalog/Runtime测试，不能把这些结果称为真实Android验收。

## 最小合同

1. 真实导航进入界面检查后，完整VM目录同步接纳当前同一行的新Ready身份时，自动产生一次当前检查首载意图；身份按runtime/row引用、serial/generation/transport/宽高/rotation判定，不按名称或反馈文本。
2. 离线保留的合法只读历史不主动读取；Unknown、Unauthorized、无当前设备和重复同身份不产生新设备读。名称等metadata-only刷新不重载，不丢节点选择或取色历史。
3. Busy、ExternalBusy、真实传输和ScriptScheduler阻塞期间合并最终有效身份；全部阻塞释放后再读取，不绕开安全门禁。
4. 在途Screenshot/Dump遇外部新Ready身份，原守卫继续拒绝旧发布，随后最多单个加载器接续最终身份；不得并发两个截图/节点首载。
5. LoadNodes自身的几何预处理允许更新已观察身份，但不能作为失败后新重试意图；后端失败/取消后重复同身份目录同步不循环。若外部目录在本次几何预处理完成前已经接纳g2并登记pending，而本次随后也采用g2，则这份已被本次覆盖的pending不能在失败后再读同g2；不能误清Screenshot/Dump在途到达的后续新身份意图。用户离页返回或外部另一新身份可以再次尝试。
6. 离页或窗口关闭时不接纳新自动读；已经合法接纳的在途任务按原清理约定排空，不伪称已取消。默认Demo仍关闭自动设备读，测试许可仅限一致自有artifacts/STA窗口。
7. 图像、节点标记、Tree/Canvas投影、实际读取计数和顺序、活动状态、pending/清理都要有原始证据；不直接调用私有Ensure或LoadNodes冒充自动触发。

## 方案与取舍

推荐在完整VM目录接纳边界观察当前Ready身份，并复用原Ensure/Yield/Queue与活动释放机制。自身几何预处理需单独更新观察基线，不能把每次NodesChanged/Busy结束当新意图。先通过独立红测和静审确定最小事件边界，再编码。

更简单方案是目录结束无条件Ensure，但这会在失败后每次轮询重新尝试；仅监听NodesChanged还会把自身清空节点当成新用户意图，拒绝。禁止新增定时器无限重试、放宽身份守卫、修改原16/49/11/10/18套用例取绿，或为了消除空白保留过期截图。

风险集中在同步重入、自身几何和外部目录更新交错、已覆盖pending重复尝试或未覆盖pending被误清、runtime/row替换和窗口关闭。不得用Busy或debugAutoLoading来判断通知来源，因为外部目录也可以在截图/节点等候期间完成。若三轮生产review-fix仍不能满足合同，停止扩大该修复并保留失败证据；继续其他安全工作。真实runtime替换的设备接入路径仍须后续实机证明，不以Demo推断通过。

真实Catalog用SemaphoreSlim串行Discover与快照写入；自身backend Discover仍被hold时，另一完整VM刷新只能等待，不能声称已经接纳。交错用例必须先证明释放Discover后，外部完整VM接纳确实先于本次几何采用/Shot入口；可在测试侧使用明确的Dispatcher优先级控制继续顺序，不增加产品hook或伪造Info/pending。不满足时归类夹具前置失败，不能当产品红或跳过为绿。

## 任务与验证

| 优先级 | 顺序与状态 | 验证 |
|---|---|---|
| P0 | 已建立三个基于90815e8的独立实现/测试/验证worktree；源码分析中 | 不复用旧交付树修改，保留所有历史证据 |
| P0 | 新正式夹具与最小Demo接线，先静审冻结再首次红测 | 新fixture先执行；完整目录同步、真实导航、TCS和Backend调用证明，无私有runtime替换或Info伪造 |
| P0 | 仅相关VM/窗口调度最小修复 | 新专项红转绿，正式150项及原五套保持不变 |
| P1 | 独立三桌面/六公共窗口与主仓代表复验 | 新专项逐场景、旧套及清理/源字节/配置一致，不只看退出码 |
| P1 | 报告、README和交付清单，经独审后单独push | 原私有功能分支；不合并发布，不提交本机配置/依赖/新原始证据/两用户PNG |

## 当前执行

2026-10-16：上一轮是实际进展，A06c1提交90815e8已核远端一致/test备份49e4710不变。本阶段开始时根tracked/index干净，仅原两用户PNG未跟踪；本阶段尚未运行红测或修改产品。具体结果在后续执行时记录，不预填通过。A07..A12、原生保存/在途裁剪、AP023/028、真实专用实例和最终30分钟验收继续开放，完整目标active。

正式新夹具r0已在独立测试树冻结19例，包含仅宽高/仅旋转分段、B真实阻塞/A身份变化、旧payload冻结、同身份失败不循环及第19真实串行发现后的Dispatcher交错前置。`ReconnectedDebugLoadingSmoke.cs`为53745字节，SHA256 `B0E4EFB665F9E6B4A3EB2140486F2B6C35EBD08F9EC8829D2099AFA7DB1563D1`；Main仅原Demo gate加入自有许可和RunSmoke最前接线，raw `0714E4F7E404A39AB5C8B4CB95FE3662B3A8B178E2CE9D0C39518790E33BE017`；累计补丁 `6C306DCC7DD5B5CA6F0225EF0361C7A95CCA6A71689FE5D1BD1D34C62C77F3A7`。原五套raw/canonical均未改，VM仍41E4...A9CA。root已读完整夹具/接线，独立verifier尚在冻结静审，未Build/GUI。

夹具冻结清单的最大journal长度曾把190+23误写212；不可变原件保留，另`journal-bound-correction.json`明确正确上限213。这是冻结算术元数据修正，不是产品修复或测试通过；执行器仍按实际绝对路径检查260字符边界。独立baseline runner `0369184699EFD92C53C7E2155EB0CC95B066F2C20AC0D70F59956CD15B83955A`已全读，只有明确冻源及独占锁放行后才执行首次150/1000新专项；预期exit1只代表应检查原失败，不自动证明产品红。

## 首次受控红测

独立verifier机械导入r0，Main精确0714；新fixture的gitapply CRLF原件先全文归一比较与冻结LF相等，再仅该新文件复制回B0E4，旧五套不改换行。首次Build进程exit0/73.494秒，其中正式测试150/150用时51.11秒；实际有2条CS4014警告、0错误，均为新fixture第278行返回Task的登记调用（WPF临时及正式工程各一次），不能写成0警告。DLL `EADD4E50317789392CD3AE9FDC5021DD6614EB75FEBAA943C641EE4CFD96ABB2`。

同一冻结DLL第一次1000完整入口child exit1/30.373秒：新19例为8PASS、11FAIL、19cleanup，208检查/1826观察、preconditionFailure=0；无超时、kill或重试。原五套和其后完整smoke因新专项聚合抛错未到，均NOT-RUN。四份前后126源快照相同，VM41E4/原五套raw及canonical/实际配置E4E33不变；两exec会话已结束，自有进程0。

11个失败是：同row Offline→Ready、Ready transport、仅宽高变化，以及两个在途场景尚未进入首个新读门；Busy/ExternalBusy/Transfer/Script/Dual/Close六例在pending=false处失败。原JSON证明Current A/Main B未偷换，行与Catalog已是Ready g2/t12或g3新几何，旧图null/节点0；无活动时也无pending/queued/loading或新Shot。阻塞场景B仍实际Transferring/Running，HasActiveOperations=true但pending=false。这与源码缺触发一致，不以5秒等待门超时本身充当根因。

8个通过包含默认Demo、metadata稳定、自身几何正常/失败/取消不循环、非Ready与无Current、离页返回、第19前置交错。第19实际外部同步完成timestamp8258120973031，早于本次Shot8258120979288，采用g2/t12；但基线外部pending=false，不能将它称为新ack已验证。Close在PendingOnly提前失败，真实Close尚未到；两个在途场景也尚未到旧payload拒绝的深层断言，不计为已验。仅宽高失败后旋转子阶段未到，同样不倒填。

原报告`apple-reconnect-loading-verifier/artifacts/q/s01/artifacts/rl/6dafd836/reconnected-loading.json` SHA256 `4FE2DE2718086A7BAD67EEC1678B199BB17E1384F14F225A9CB86006CA77209A`；独立摘要`artifacts/reconnect-verifier/baseline-r1-summary.json` SHA256 `AC4D9312A157B4B8FF18A082BA8E2E94374B1B325A4D6DDFF5B8DB68CB1837F6`。root已实际读19分项、关键行/Catalog/阻塞分量、第19先后及process/stdout警告。源码失败轮与r0证据保留，不因下一轮通过而改写。

接下来只批准首轮两生产文件修复，以及新fixture末尾`Track(task);`改为`_ = Track(task);`显式丢弃已登记Task返回值；不得await而提前解除本来要维持的活动，不改原19动作/断言/等待或旧五套。最终产品、回归和交付尚未通过，不能预填已完成或已push。

## 首轮候选冻结

生产r1累计补丁 `F950B28D410094BA48E16E0EC002BE8FA15E27A982E31DF65129A7D6D9DC50EB`只涉及VM/Main共30+/1-；VM raw `C288EEE0A34F00EF0EF3BB7B0C7E881094026BDCEFEF43DE90D28A4C4D0475F1`，纯生产Main raw `9C4F4F7CC0A643B40F34562E520657B2301F549287A668EFE38199020A123DDB`。完整目录接纳末尾比较严格Ready身份并先记后通知；Current换引用建观察基线；只在LoadNodes严格Capture及RequirePublication后同步记录采用source并ack，Main复用原严格谓词确认当前来源后消费pending。没有延迟ack、全局Busy抑制、revision或Core/Host守卫改动。

新fixture r1为53749字节，raw `F1AC325753784B59E33F2B34A79B3D94E18FE648ECD3EF7D4842C54FE80B4228`；root全文证明与r0仅相差显式discard的4个ASCII字节，19动作/等待/断言不动，Main测试接线仍0714，旧五套不动。r0警告和失败证据保留。两种改动组合后的Main必须另冻，不能误用纯生产9C4F或仅测试0714：root独立计算并与verifier实际机械导入核对为 `F243E54C6261DFBBA14A07D64AFA922C84199533A89C8FD237CE527A55A3F802`，46879字节。

独立Final-r1执行器 `352A62291A28E8C3947C115324EADB4899ACAB756CCC774BFCF1D5F15D9728F3`已全读、Parser0，仍为240秒上限、单次自有进程清理、冻结126源/旧五套canonical/实际配置门槛及首次失败停止。已放行首Build与首1000，结果未出；其余桌面和六公共布局需首1000全部原始报告核过后再执行。根尚未导入候选，不用源码静审代替运行绿。

## 独立最终验收

首轮生产r1未再改动。独立Build进程exit0/63.495秒，正式150/150用时50.92秒，实际0警告/0错误；DLL `8EED24D187CB1D8671FE2B2010AF8CB67A291C2C9DEC92BACA3ADD29CB14E83D`。r0两条CS4014仍保留原日志，不倒填为首次无警告。

| 首次执行 | 实际结果 |
|---|---|
| 1000×700完整窗口 | exit0/93.798秒；新19/289检查/2214观察/19清理 |
| 1440×900完整窗口 | exit0/94.062秒；新19/289/2214/19清理 |
| 1920×1080完整窗口 | exit0/94.575秒；新19/289/2212/19清理，事件数不伪填常量 |
| 原五套，每个完整窗口 | 完成缓存18/295、截屏10/630、节点11/905、原首载16/137、发布49/401全部通过及各例清理 |
| 其它原完整流程 | 每窗外观209、Native守卫25、筛选73与标量matrix32、文案36、12页idle；outer Error为空 |
| 六公共布局 | 390×640、390×844、768×1024、1000×700、1440×900、1920×1080各135检查；真实HWND/Loaded/Visible及实际尺寸准确 |
| 不变性 | 10个stage全部结束，20份前后126源字节相同，旧五套raw/canonical与实际配置E4E33不变，自有进程0；无超时/kill/重试 |

三轮都真实读完宽高与旋转两个阶段、在途g2旧图/节点标记从未发布且最大Shot/Dump并发1、最终g4图树对应、Busy/External/传输/脚本/双阻塞仅补最终源、实际Close/Closing完整路径。第19三轮外部接纳pending=true，随后实际g2/t12采纳时false，失败后同身份同步仍不重复；首1000的外部tick8262211756957小于Shot8262211766416。不能把基线第19的pending=false正控与此机制证据混用。

新19的自有测试窗口固定1000×700，三轮是在不同外层桌面入口运行同一功能契约，不冒充每例已经操作六种尺寸。六公共布局及原节点/截屏正文专项分别提供其实际尺寸证据；物理移动设备、DPI和全部控件仍依总计划验收。

独立摘要`artifacts/reconnect-verifier/final-r1-summary.json` SHA256 `8609A9E081C3288FBEEEA083DFDD198D81A0F96DDB206D22C8FFBDD960F6470E`；三份新原JSON分别为 `6A929C425F6A8362BDFE0EFC05C78EADF9A789260DBD5919390B724DD168E865`、`2C16A78C2BF04A7C8B10136F058B31392A531845161E8C6F79FB8CABE3B123F1`、`AD156EF11CF23E9FCC8E4D77A7A76A62D519EAB4283F5131B14824AFBA50A3AA`。root已读首19逐例实际读序/身份/原Task完成/旧payload和ack分量，并读其它矩阵摘要与首完整outer/process；独立verifier另读原五套及目视三桌面和390抽屉，root目视1000设备正文。

三窗外层`desktop-smoke.json.colors`仍为0，nodes亦为0：原夹具直接改Info制造离线后未重载，而默认Demo不自动读。本项只验证完整VM目录接纳的自动路径，不宣称任意Info赋值/Catalog-only或外层取色动作已修。保留A06c1所述边界；新19图/树投影有各自实际证据。不代表生产timer、Android、物理输入、原生保存或真实runtime热换通过。

## 主仓整合入口

独立矩阵结束归锁后，root按测试累计DE0C80...AD4C及生产F950...50EB机械导入。新fixture gitapply raw为`17F8477A91BE82CF8D4797A87BC5EEBB91F766E322294262D05AB253F343CDB2`，先全文CRLF归一与冻LF相等，再仅新文件复制为F1AC；Main/VM精确F243/C288，根原五套LF字节及实际配置不变，记录在`artifacts/reconnect-root/import-r1.json`。根代表执行器`3C203BB193963FC0F2C597CC2868FE916FE1031A982130472426830F622EF019`只相对已审Final修正根路径层数、原五套raw和独立证据前缀，不改断言/超时/期望。

主仓首次Build进程exit0/71.580秒，其中正式150/150用时51.05秒、实际0警告/0错误；DLL `FB4B1F8C6E77539F2C66F31C8254699FCA408674AF8EF64A4C1A9563013703D7`。一次1000完整child exit0/93.348秒，新19/289检查/2214观察/19清理及原18/10/11/16/49全套通过、12页实际1000×700 idle；一次390×640 shell child exit0/2.528秒，135检查及实际HWND/尺寸成立。全部exec结束、自有0，六份126源前后相同，旧五套/实际配置同。外层colors/nodes实际仍0，如前述不冒充其动作成功。

root按实际schema分别读取旧完成缓存的`cases/passed/cleanup`和其余套`reports/status/cleaned或cleanupCompleted`，完整原19及各套/进程/帧源分量已核，目视独立1000设备正文和主仓390抽屉。原新专项`artifacts/u/s11/artifacts/rl/3f1392b5/reconnected-loading.json` SHA256 `604687265341011BC1B77178BA98F6D15B103D07569142E732496CC683C3073A`；根摘要`artifacts/reconnect-root/root-summary-r1.json` SHA256 `BE91B9A5E827DEB1B6A277346C0EAE2AFD30F85EB9A18CB70C12A7ACABB3E381`。根第19externalpending=true，8270280321872先于采纳8270280333404，采纳后false且只一次Screenshot、无Dump或同身份重试；两个在途旧payload未发布。

## 交付门禁与未完成项

本阶段限定VM、Main、新正式19例、README、本报告、任务/进展/交接/问题/矩阵共10源码与正式文档，加3生成清单。10份raw冻结并经独立文档/范围审查后，只在隔离快照使用已审helper `F36ABAB4809AAF5CB454A70354C1C8A25CF013730F24A5CEEF5FFE8EF39BBA6C`打包并逐源/ZIP核字节，只有三清单可回写。随后精确stage13，独立只读audit `60A9E46E0FCB23B09E6F1BE4041288A805BF0AE810A83FBCD591321598E2C537`重建Tree/Bundle、核全部ZIP、旧五套canonical与staged/working一致后才可单独commit/push。

包来源为当前tracked加明确新fixture/本报告，其余untracked包括两用户PNG排除；不新增本机配置、依赖、APK、SDK、bin/obj或本轮原始证据。AP021旧tracked材料仍保留，不称包已隐私净化或完全纯源码。实际核包/交付审查结果在tests树`artifacts/reconnect-loading-package/`，根`artifacts/reconnect-root/push-complete-r1.json`与当前Git/远端为推送事实依据；流程文字不证明已push，若实际Git已含此项且远端一致则不要重做交付。只推原私有功能分支，不合并发布、部署或清理worktree。

完整目标active。下一项优先取证原生“保存裁剪”开框期间合法晚到帧清crop、返回后可能退化完整新PNG的静态候选，不能把VM已在写盘await前冻结bytes误判为缺口；先定义保存意图合同并实际红测，不为更容易通过而默认取消合法保存。未完成拖拽/失捕、节点JSON原生保存、AP019/020/023/028、DPI、A07..A12、真实专用实例和最终30分钟继续开放，不拿本阶段离线结果替代。
