# A07：提示文字高度的独立诊断

日期：2026-10-16；主仓基线1de5136，承接[A07c第三轮拒绝](A07_APPLICATION_RESPONSIVE.md)，归属AP-038。上一goal turn取得原版裁剪、三个候选和真实字形失败证据，属于实际进展，不是连续阻塞。

## 事实与问题

f03的12例5PASS、7记录PRODUCT-FAIL；共同失败位于应用首行版本串`2026.10.16-preview-long`的提示高度断言。弹层163×31、TextBlock143×17且Wrap，实际模式和原FormattedText高度没有记录。长失败文本已从924宽NoWrap修为334×68/Wrap并通过，因此不能把剩余7红等同于换行仍失效或7个真实裁剪。

本机SDK及[官方10.0.13源码](https://raw.githubusercontent.com/dotnet/wpf/v10.0.13/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/FormattedText.cs)证明原7参FormattedText构造选择Ideal，默认段落WrapWithOverflow；生产TextBlock为Wrap。Window主题Display/rounding只是线索，不能推定独立Popup实际模式。根问题是尚缺能区分测量语义与实际裁剪的证据，不是已知需要更高行高。

## 方案与范围

推荐在原高度断言之前增加同步只读观测，保留原FormattedText对象、MaxTextWidth、全部断言/控件动作/等待/清理逐字不变。记录原测量宽高、有效字体/文化/Language/模式/DPI、Actual/Desired/Render、rounding/snapping，以及同参数Ideal/Display/有效mode对照；对照只是诊断，不择绿替换断言。保存同一自有Tooltip内部位图及正文/容器/clip位置，人工核对末尾`-long`。

更简单的新建TextBlock实验可作为辅助，但不能证明当前字段没有被祖先裁剪；不单独作为本阶段结论。拒绝改宽/字体/行高、增加误差容忍、禁用原断言或只根据某个对照为绿判假红。只读观测与本地图片写入若失败，必须记显式FIXTURE-FAIL；不能吞掉后将诊断称为通过。

作者仍使用独立app-layout-tests树，先冻结原r3；verifier使用新app-tooltip-verifier树，保留原app-layout-verifier全部f03文件。生产AppView2C9853、Theme567766和MainA752保持，旧20Smoke/API不动。新的d01是诊断产物验收，不是本阶段偷偷重跑f04：预期原红保持，旧专项仍未进入，不能发布产品通过结论。

## 验收与计划

1. P0：冻结原r3、诊断精确补丁及新SHA；独立审查仅增观测，没有改变原断言、等待或生产。建立新树原147输入基线及148冻结，真实PDB核24源码，主配置E4E33和owned0保持。
2. P0：只运行一次新d01的原Build -Verify和完整原1000×700 Demo Smoke入口，原158名序不变。每child300秒、终结等待5秒、双流各10秒；只续同handle，首失败/原字节保留。预期exit1本身不是诊断成功，须读实际12例、诊断错误、图片和终态。
3. P1：至少原版本失败点取得原高度值、实际有效模式、同宽度对照及完整自有渲染；诊断error=0、关键图片存在且无捕获限额遗漏。原全部断言结果另存，未达到阶段记NOT-RUN，不回填f03。
4. P1：独立审查判据A：原红数值明确、实际模式与对照能解释差异、全文和首尾布局及图像没有裁剪，才可判测量语义问题。判据B：实际容器/clip或位图缺尾可证真实裁剪；仅FormattedText更高不足。否则C：仍未归因，不改测试或产品取绿。
5. P1：诊断结果先更新AP-038和交接，再定义独立最小修复及同源验收；只有真正通过后才整合、核包、单功能push。三轮原失败保持，不合并发布，不上传APK/本机配置/依赖或原始证据。

## 风险与非目标

只操作一致Demo的自有WPF控件/内部渲染，不新增Windows原生输入、系统DPI变更、guest/ADB/HTTP/权限/VPN。内部位图不是桌面截图、触摸输入或真实系统DPI证明。真实双实例同步、当前GUI安装卸载、完整旧GUI、最终30分钟与A08-A16仍全部必交付，不因本诊断完成而缩小目标。当前为执行前规格，尚未运行d01或确认根因。

## 执行前冻结

原r3 `A8031C03B2A28993920C8C88FD6D81AAD7F4289F26C59BD16A82ED6D03BCA7B0`保留。首诊断4488未运行，独审发现其前后对照仅覆盖图像附近，已保存原件并补最早Entry/最末Exit，覆盖所有读取与对照，另记录Open/原文/owner.ToolTip引用一致；不是改原断言。最终夹具687行77178字节，SHA `DF373D77F9D43ECAAFDC9AFCEC9C43416F1FEB5E517D9F82703C098C4E17307D`；相对4488补丁SHA `AFE2A32ECB8E640C638D14EA5154073CFC228863B4F0292A8DF6E2132BD913C3`、总r3增量SHA `6AFF0298C4D541EED33CFE30D6F2BD92719B4DA799CFF4A73D3908720EC29915`。剥离仅新增using/字段/Record调用/方法可精确还原原r3；MainA752及旧20不变。

生产沿用f03冻结AppView2C9853/Theme567766，不修改测量模式或布局。Tooltip位图每case每不同原值最多24，关键版本预留槽、单图最多2048×2048；捕获限额与诊断异常明确记录，不能默默缺关键图再放行。只读TextPointer在有效layout下作为辅助caret边界，不冒充逐字ink证明。新的独立诊断树和runner准备已审，尚未运行或形成结论。

## d01结果：诊断判据A成立

单次d01已终结并归锁。Build67132 exit0/76.564秒，实际构建24.28秒、原158项51.47秒、0警告错误；Smoke3636 exit1/17.816秒，外层exit0只表示保存了预期子退出，不是产品通过。原12仍5PASS/7红、与f03逐case/stage一致，旧20/完整12页仍NOT-RUN。

实际143条诊断、0诊断错误、143/143全程Entry/Exit和捕获前后保持；89张不同值图片及54次复用，无BOUND遗漏。七处版本点实际TextBlock为Microsoft YaHei UI/12/zh-CN/Display/96DPI，143×17；原Ideal测量高30.48，而同143宽、同字体/文化/DPI的有效Display对照高17。全文TextRange精确匹配，首尾caret同一行y0..17、尾x143，正文bounds等于可见交集。七张版本图同SHA `CE1CACB763C8384E256B40525C23D526B4A66AF14C1C8FC4ABF26743EC4DC2A9`，root与两个独审实际目视完整`2026.10.16-preview-long`，尾`-long`没有裁剪；长失败样本first/last-marker亦完整。

因此判据A仅在这七处成立：高度红来自Ideal测试测量与Display实际排版不一致，而不是已证产品裁字。原d01/f03报红不回填；也不由七张图推断全部Tooltip、真实DPI或后续字段已通过。

独立证据在工作区`.worktrees/app-tooltip-verifier`：`artifacts/td/d01/artifacts/al/86222440/application-layout.json` SHA `6447387D39E3F57B8A3D187E582FF3A204A9169A5B740DD002D6A2C95A01C5B3`；`artifacts/app-tooltip-verifier/diagnostic-d01/independent-audit.json` SHA `006892AE662002D274F14A9E5C631F0BFC1E22E227318015D451260B1D061B67`；diagnostic-execution SHA `57D5ADEA68497B34BDD06F098DB97206A8BE59675043625B5023E3B97CC97928`，末快照SHA `45BF26077C0B9D776FEF43D723CD7499842FBCC55FB912C5FF1E4C634CDBA098`。五次148输入/其余144/20旧/API/Main/PDB24/配置E4E33、111后端List Task终态/12清理/禁止操作0及双流均核，无timeout/kill/owned残留；四源码、3PDB/DLL及原日志冻结。

## 最小测量修正及新验收门

只在隔离fix树将ReadTooltip测量改为9参构造、读取该TextBlock的实际TextFormattingMode，不设置实际控件模式、不改Culture/Flow/字体/字号/brush/DPI/MaxTextWidth。原`ActualHeight + 1 >= Height && ActualWidth > 0`及容差完全保持。另两行只更新诊断constructor及heightPredicate描述，避免假称当前仍是7参旧测量。当前fixture `12DAA90CA08246A7EC2E68C789A5BB7AA4823B5BC4A01BEC78128E0B4D251944`，三行差异获独审；MainA752、App2C9853、Theme567766、API及旧20保持。

新corrected-c01必须原完整入口复验全部12例、所有旧专项和12页流程，不跳过AP-033或改其它测量取绿。判据是当前原断言、实际滚读/焦点/批次/清理及旧套到达，而不是d01对照显示17。FT的WrapWithOverflow与实际Wrap更广差异仍是边界；若出现新失败，保留后据实际结果判断，不据七点外推或增加容差。诊断图像限额可能在更完整阅读后达到，非关键项须明确记录未采集，关键版本图和全部metric/原字段断言仍必须存在。

当前仅修正测试测量，尚未执行corrected-c01或导入主仓；产品候选未交付。全目标与原所有未验项保持。

## corrected-c01结果：11项通过，新增字形前置未过

上述“尚未执行”为该步骤准备快照。corrected-c01单次已终结并归锁，不重跑：Build83584 exit0/64.301秒，实际构建12.08秒、原158项51.46秒、0警告错误；Smoke4872 exit1/23.187秒，四流排空、无timeout/kill、自有进程0。原12例中11 PASS、12清理、2887检查通过、111条后端List Task终态。

六常规尺寸的geometry、三表首/中/末行及全部数据列滚读均通过；ReadTooltip的有效mode校正消除了前一版本高度断言的阻挡。空、取消/恢复、状态/焦点、表头选择均过。232条诊断/0错误、全程及capture状态保持，133图全部bytes/hash/IHDR核对，99复用、无限额遗漏。这不是原f03/d01回填绿，也不是全部系统功能通过。

唯一失败发生在glyph-150末尾原合取`Fonts.Count > 20 && Fonts.All(current ≈ baseline * 1.5)`。该场景三表已读完，36个数据文本样本FontSize均18，但未记录Count、具体不匹配Key、其是否仍属Page或字体来源；不能由样本推断所有控件150%，也不能把这个红定为真实产品裁剪或夹具假红。后续原no-transform、SameCalls及glyph最终截图未到，保留failure图而不改名当通过图；旧20专项和完整12页仍NOT-RUN。

新报告`artifacts/ta/c01/artifacts/al/6d46da85/application-layout.json` SHA `90A453567CDCE009FA82199247CE8588E7B27A741AF20E7CD77BE2ED6D90EDBE`；独审`artifacts/app-tooltip-verifier/corrected-c01/independent-audit.json` SHA `6DA67566E298DD7E714AF28952EBBB3D6AA5112F56D0B30E83BF54D0F5A405E9`；acceptance SHA `635907698F96D82257DFD31655F9A3940F54C2C228F3E167CC6AC4E6FDA7C3BC`，末快照SHA `DB6B58871B42CBDC1E6707B809848511F15F779FCC535BBBE72950DC2DCD602B`。五次148/其余144/20旧/API/Main全文/PDB24/配置E4E33及全部终态核同，四源、3PDB/DLL留存。stderr4346字节SHA `571CCDB9719DB04159A4927B9DC596D179C0A0F85B893F24C81638938E915478`保留。

独审仍拒绝交付，主仓1de5136源码未变，不导入、主仓Build、核包、暂存、提交或推送未验候选。测量修正只在fix及verifier树，源SHA12DA保持；不是系统已发布修复。

## 下一步：保持原字形断言的只读审计

新字形红与AP-038模式原因分开跟踪为AP-039，后续独立观测而非无证修补。Fonts为累计历史字典、ApplyGlyphScale只处理新Key，虚拟化移出/复用后样式重算或baseline记录时点均只是静态候选。不能先过滤历史Key、重新施加字体或放宽Count/All取绿。

最小观测放在原字形Product之前：Count/匹配数量、当前Page视觉成员数量；不匹配Key及匹配对照的对象id/类型/纯文本、baseline/current/expected/差值、IsLoaded/Visible、是否Page后代及同PresentationSource，字体DP的值来源和只读有限父链。记录入口/出口Count/Key引用/font一致；无SetCurrentValue/Apply/Settle/Dispatcher新增，异常明确单记FIXTURE且原assert继续。

若不匹配全是移出的历史控件，仅能支持断言范围疑点，还须证明当前Page成员完整覆盖/倍率；若仍在Page，则按字体来源判断测试驱动遗漏、复用重置或真实布局问题。证据不足继续未归因。真实Windows输入/DPI、当前GUI安装卸载/双实例同步/最终30分钟及A08-A16不取消。

## 字形诊断执行前冻结

作者在独立tests树保存DF373及12DA原件后，仅添加原字形Product前同步调用和诊断方法；剥除两处可精确恢复12DA原始文本/SHA。新夹具SHA `B23647AC13C116378CB4307C67CF923F7B1B3258FD232DCFC34410663A18E2AB`，增量SHA `AA87E7ABB12C1BC78F7A78D353C5AC7F59E2B6525F93BE52492AB060476AA62B`。所有失配、6个匹配对照及最多16个当前未登记对象采样均纯字段，有限8层父链；记录当前Page可见、隐藏与历史脱离对象，前后Key引用/基线/字体/Page成员一致。诊断异常明确记录FIXTURE，原合取仍执行，不加Apply/Settle/新await或修改字体。

verifier新准备`font-diagnostic-d02`与独立`artifacts/fa/d02`，runner SHA `CF8F1EF822A0B835BA26C3EB7DAD69785C71253551E0A3028F8E131B81C0971E`，原158/148/144/20/API/Main/PDB24、300/5/10及配置守卫不变。预期子exit1只保存原红，不代表诊断成功。此为准备快照，尚未执行；必须先独审夹具及机械导入静态证据，再明确授唯一Build/GUI锁。

## d02结果：历史范围与当前施加条件均需修正

上述准备已完成独审及单次运行。Build56824 exit0/63.701秒（实际12.07秒、原158项50.94秒、0警告错误）；Smoke47428 exit1/21.942秒，原12仍11PASS/1字形合取红，12清理/111后端Task终态，旧20及完整12页NOT-RUN。四流排空、无timeout/kill、自有进程0，锁已归root，不重跑d02。

字形诊断1条/error0，入口出口Key引用、基线、字号、Page成员及字号数组逐项相同。累计Fonts633个：516匹配、117不匹配；当前Page有505个字体对象、全已登记且0缺失，其中504匹配。117失配中116已离Page/未Loaded/不可见/无PresentationSource，字体Default12、预期18；它们包含31个TextBlock及行、单元格等已退出对象。另1个仍在Page且Visible/Loaded/同Source的ScrollViewer，baseline13/current13/expected19.5，来源Inherited、无local/IsCurrent=false，8层父链均13。当前字号条件确实未全满足，不能只过滤历史取绿，也不能由容器字号推断其内容文字已变小。

两类事实分别成立：累计历史对象不是当前渲染覆盖域，夹具的实际施加环境也未完全达到自身150%前提。末态快照不能证明字体何时/为何恢复继承，不写成已证reparent或产品字体缺陷。修正应限定自有试验的字号施加与当前完整成员检查，不给产品ScrollViewer硬编码字号、不忽略当前失配、不重记已缩放baseline。原c01/d02红保持，新的合约变更须独审后另一次真实验收。

报告`artifacts/fa/d02/artifacts/al/7784ac5c/application-layout.json` SHA `70E1F82650EC73B6280CF4A093B529D06183E9A033F83123920BB6CE78B502CF`；独审SHA `F7A793D1B779A093214629827CBB195125D3B17CFF23F8E8F9E8078AB6263AE0`，execution SHA `78B12692CAEA6A9AAD6D1E1E724D2CC3CD26D1E43475A92483DE695DA7190B9B`，末快照SHA `6E23EEA28E7D739EC00B877FAF36EBBCA38C287B60F44408B08C88E845226113`。Tooltip232诊断/error0/状态保持、133图hash/IHDR核同；5快照148输入/其余144/旧20/API/Main全文/PDB24/原158名序/配置E4E33保持，四源/3PDB/DLL冻结。独审只批准诊断证据有效，不批准产品交付；主仓源码未变。

## 最小字形环境修正

推荐方案限定夹具：仍只在原Apply调用点首次登记新Key并整批冻结原baseline，原循环两个FontSize setter改为SetValue，本地覆盖建立自有试验输入；不重复放大当前值、不在最终断言前补施，不新增等待或重试。最终检查当前Page全部Control/TextBlock数量>20、每个已登记且当前字号与首次baseline×1.5差<.01，包含隐藏成员和ScrollViewer；历史Fonts保留完整诊断，不再充当当前渲染域。字号FontSize的表达式/资源引用被有意覆盖，所以这里只称Typography压力注入，不声称原字体绑定语义保持或真实系统DPI。

更简单的单纯过滤历史方案不能解决当前1个失配，拒绝采用；全当前反复SetValue虽可建立更强输入，但比首批两个setter修改更广，当前先不做。也拒绝产品硬编码字号、排除容器/隐藏成员、重记已缩放baseline或放宽.01。若实际新验收仍有当前失配，应保留结果重新判断，不自动追加重施或重跑。

fix树新fixture SHA `7248CA7AD5ADE4485A1D0FCC01090E06597C5D2C22895A21CB8EC18CC7AD51EA`，B236原件保存，精确四处差异为当前域断言、诊断描述、环境注释和两个setter；生产App2C9853/Theme567766/MainA752/API及旧20不变。下一font-corrected-c02必须独审差异/守卫后单次实际验收新12、当前覆盖/倍率/诊断前后不变、最后transform/SameCalls/glyph图以及原旧20/完整12页。当前未运行、主仓未整合，不预称150%或交付通过。

## c02结果及独立入口补验

font-corrected-c02已单次终结：Build17992 exit0/64.132秒，原158项50.99秒、0警告错误；Smoke37664 exit0/169.933秒，四流排空、无timeout/kill、自有进程0，锁归root。新12例全部PASS、2890检查、12清理及111后端Task终态；六常规尺寸三表首中末/全列、取消/恢复/焦点/选择保持。glyph当前Page505字体对象全部登记并达到固定1.5（含344隐藏/161可见），历史128保留且本轮全部633记录匹配；字形诊断1/error0/EntryExit保持，Tooltip232/error0/133图核对。原末尾Transform、SameCalls和glyph最终图均实际到达，root已重开图像目视，不把该图当Windows桌面截图或系统DPI证明。

报告`artifacts/fc/c02/artifacts/al/95f774df/application-layout.json` SHA `73462946B7FD73532D70D210392B1E0A549A35CC57540FEAA78F109ACC9B50CB`；独审SHA `D78AD6F901DC4ECBF3D7876AD2B5A4AA8E8C9241A0E297249DA985E8D0702709`，acceptance SHA `6B518C57747A7E609663F8F7DE1E9188CA0A13746AD17891F654541AACDF740C`，末快照SHA `5651B09D5272B24634F12EF8B47E30E0DA6E8A306E1F624E4218EAC662FF4D23`，实际DLL SHA `0A51912AAC17DCBC4BFA0F81350468257FB44CE3E551C211FA36D5D545B9F2C4`。5快照148输入/其余144/20旧源/API/Main全文/PDB24/158名序/配置E4E33保持。stderr16009字节原诊断留存，末desktop nodes/colors=0不外推对应外层功能通过。

必须纠正计数口径：本阶段“20旧Smoke”是冻结源文件数量，不是普通`--smoke`会执行20个独立入口。实读App分支与原await链：c02走完17源相关原Smoke调用链及12页，15份实际旧/末报告通过（含API14、pending12、改名10、同行19、缓存18、节点11、截屏10、首载16、发布49等）；共享shell/设备正文/监控正文另由三个互斥`--verify-*`模式进入，本次未调用，明确NOT-RUN。PDB编译成功不补齐运行覆盖。

下一步仅复用上述冻结DLL，对这三个原有独立模式在原六尺寸各运行一次，共18格，新的fresh profile/evidence、原300/5/10及全部输入/配置守卫；不重Build、不再跑原Smoke、不改旧断言。补矩阵仍待静审与明确授锁。候选当前12专项与原Smoke链限域通过，主仓仍未导入/执行/核包/push；旧c01/d02原红不回填，全部实机和后续目标保持。

## s01结果：6格通过，1格前置及报告失败

独立s01复用上述DLL/3PDB，未Build或重跑Smoke。准备稿4DE未执行，自审先修StrictMode读取被省略null error字段的边界；新runner SHA `1690D2BFA778C70FF0CB6E882FF0264EC3513A96239F16590E307EC0C7FB5156`独审后才执行，缺字段/null/真实错误三个本地反例通过。这不是产品修复或原测试断言放宽。

单次session86322已终结并归锁root：共享壳层六尺寸各135断言PASS；第7格device-390x640/PID40436正常exit1，原offline-reject负例在Position的600ms OS/WPF指针一致前置失败。其后原ResponsiveDevicesSmoke第527行把非有限Double观测序列化为JSON再次失败，`device-layout.json`不存在。保留两种独立失败，不把前置误判为已证产品回归，也不因缺报告推断此前全部检查或动作数0。

首错按计划停止，后5设备与6监控共11格NOT-RUN，18格整体验收INCOMPLETE；不得把c02/本次壳层绿换算为另外11格通过，也不重跑取绿。7子进程全部终态、14原始流完整，无timeout/kill/清理错误，自有进程0。实际只有15次输入快照（不是计划上限37），148原始/规范化/其余144/20原源/API/Main全文/PDB24/配置E4E33及冻结编译物全部保持；27图签名/IHDR/bytes核，窄、宽及设备恢复图目视。

独审SHA `9DC99D481E436419A5F263BA18069A830BF03CC0D0FCF22AFC8EE122A02C7BA5`，acceptance SHA `7DFACB860C3D605CB171493ECFD81A24986B62F4760BD6FDF8160F7A38E84C98`，末快照SHA `2B35A911ACB28C65B49B1690A5BBC68D8C365230C14C3A4639C830126EB12ACE`。原始stderr为10534字节，CP936只读解码确认首因，原bytes不转写；与既有AP-019/020及AP-033一起保持开放，不由相似报错声称同根因。root尚未导入/执行/核包/push，正独立审查有限域整合与后续安全诊断边界，未宣布整体验收通过。

## 主仓r01：有限域核验通过

独审明确只批准精确导入后的一次主仓代表核验，不批准s01或发布。Root按原147输入/配置/索引/进程前置核对后，机械复制冻结App2C9853/Theme567766/MainA752/fixture7248，未再改生产或旧20源。已审root runner SHA `D85C8D3CFF45A114E271123CEED56E129B8C47A4AB53B95DABFDE00DEF3AF181`固定c02末快照，规范化全文/原raw与PDB双校验，原300/5/10及原全Smoke不变。

单次r01已终结：Build15420 exit0/71.867秒，实际构建19.92秒、原158项51.12秒、0警告错误；Smoke41364 exit0/164.976秒。新12/2890检查/12清理/111后端List Task终态，六应用尺寸三表全阅读、停止/恢复、焦点/选择及末glyph图通过；当前505字体成员含344隐藏均1.5、历史128保留，原Transform/SameCalls通过。Tooltip232/error0/133图与字形1/error0、全部EntryExit状态/数组由独审重算，root实际读取报告并目视390应用表与glyph图。

普通Smoke原17源await链到末12页，15份实际旧/末报告通过；不是s01三独立入口重跑或20模式通过。外层nodes/colors0及stderr15945字节保留，后者含旧已捕获诊断，不能称日志为空。5次148输入规范化全文同候选、144原rootraw/20旧源/API/Main原文/实际PDB24/配置E4E33保持，四原流终态、无timeout/kill或owned残留。四源/三PDB/DLL八份原件由独审冻结。

主仓报告`artifacts/al/r01/artifacts/al/4b22dc7b/application-layout.json` SHA `5F864A7ECFC93DEBB49C2152A81C7BD6985E86D2DDED9D6080B3A4BF30AF3B7F`；`artifacts/app-layout-main/root-r01/independent-audit.json` SHA `3EA80FE09ABF4D2E9D358F56B024497C3407313394642E970F8AF953FE3EE1BE`，acceptance SHA `543C16112206135EBD1C1C5B00F779ED08D243FE75E191AC5C3AF4F7141C10CC`，末快照SHA `D24514270AC1BD4FE850B27D46EE204B2E8F4D17B1EDAC7DC6ED7795266793FA`，DLL SHA `4058AD1A39B8B6F179755FA3112CF7B92F73D92342D262E5A550E1CA3FAB1757`。纯本地审计脚本曾有命名/字段处理问题，原稿与更正留在ignored annotations中，不是产品或r01重跑。

最终仅获原私有`codex/unified-device-access`开发分支的有限域源码交付批准，仍须文档、完整源码清单/包与精确暂存独审；Git结果以实际记录为准。s01/AP041与11格NOT-RUN、AP019/020/033、原生应用GUI、真实双实例及最终30分钟、A08-A16均不关闭。不合并、不发布、不上传APK/配置/依赖或本轮原始证据；全目标继续active。
