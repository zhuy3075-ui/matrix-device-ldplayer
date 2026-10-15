# A06b2b 截屏正文响应式

日期：2026-10-16。

## 目标和范围

基线为已推送的`29ab03feb858e87a875f4e41a82d3b79f35b4296`。继续原12页完整目标，不把上阶段节点正文通过作为截屏页通过。本阶段只调整原截屏画面与色值表的布局和局部可达性，保留缩放、滚动、取色、裁剪、原始大小及保存入口。

生产范围先限定`Views/ScreenshotView.cs`的BuildScreenshotPage；不改VM、RefreshScreens、共享MapImage或节点页。正式验收新增`Views/ResponsiveScreenshotSmoke.cs`，只在MainWindow的Demo smoke入口接一行。原16/49/11和完整基线检查保留。

## 事实与方案

原正文为DockPanel，右侧色值表固定300宽及10间距，画布处在剩余宽度；原按钮32高，模式115宽、缩放Slider150宽。原Canvas/Image/ScaleTransform采用原图像素坐标，GetPosition(Canvas)经原LayoutTransform取色及裁剪。以上是立项时的源码事实；当时尚未运行六尺寸，不能提前写为已复现故障，实际结果见后文独立基线。

推荐同一个正文Grid：稳定正文宽起始640及以上保留画面和右侧色值两栏，窄屏使用等宽44高的“画面 / 色值”分段，默认画面。原“取色 / 裁剪”是画布操作模式，与正文分段职责不同，保留并给予明确可访问名称。不新增独立页面或重复控件；原对象、ItemsSource、绑定、图源和ScaleTransform持续存在。焦点在色值表时缩窄仍显示色值，切段不能遗留隐藏焦点。

更简单的纵排方案可减少入口，但390×640中工具换行加色表可能挤掉首屏图像，故先用分段。拒绝整页缩放、自动fit、新Viewbox、resize重新截图/调用RefreshScreens及复制第二套图或表；这些会改变原像素或状态合同。阈值可根据原红测实际几何调整，必须记录理由。

参考Apple官方[布局](https://developer.apple.com/design/human-interface-guidelines/layout)与[分段控件](https://developer.apple.com/design/human-interface-guidelines/segmented-controls)指南，按实际可用空间适配而不删功能，以简短等宽名词表达相关内容选择。本项目是Windows检查工具的局部正文切换，不复制macOS平台样式，也不改变原12页导航；HIG参考不替代像素坐标、可见范围和真实交互测试。

四个原按钮、模式和Slider整体局部44可达；按钮/模式/分段检查完整区域命中，Slider分开检查整体44布局、实际轨道/Thumb中心及原RangeValue作用，不声称四角都改变值。不以Slider外框44冒充原小Thumb、共享箭头和滚动条均已44。长坐标、HEX/RGB和100条颜色的阅读路径须实际验证，不能用字符串存在代替可见文本。

## 验收与分工

1. 六个实际Demo HWND外窗：390×640、390×844、768×1024、1000×700、1440×900、1920×1080，另验正文640±1。原图与实际ScrollContentPresenter及祖先viewport相交，在默认1倍时至少100×100，图框至少160高且不越界；不要求1:1同时显示整张大图。
2. 基线先验原画面/32工具实际几何，之后才检查未来分段，不能因新入口尚不存在伪造红。原四按钮、模式和Slider完整可达，真实命中与可访问名称有证据。
3. 宽双栏及窄分段原对象/Source/Colors集合和所选行、完成crop/overlay、缩放/模式、Main/Current/Targets/Nodes不变。纯resize和切段不调用任何后端。可见时原有效滚动offset按新Scrollable范围合法clamp；不能用隐藏时零viewport把阅读意图无条件清零。
4. 色表100行首/中/末和三列可滚读，选中行/焦点可返回。只用合法样本或明确标记的布局种子填表，不把直接SamplePixel当鼠标路径。
5. 10例专项：六几何、空图/实际TCS Busy/单次错误恢复三状态、一个宽窗跨缩放/双向滚动坐标路由。独立wrapper实现IDeviceBackend，只有一致Demo runtime及临时路径，不依赖另一个fixture的private backend。
6. 坐标路由仅在自有已激活窗口一次定位系统指针、有界观察实际OS/WPF位置和Image命中后，向实际命中元素发送合成WPF事件。记录事件真实GetPosition、调用数、实际样本及已知gradient像素/HEX/RGB对照；不重试定位或Activate抢焦点取绿，finally恢复指针和解绑。覆盖原缩放范围端点与代表中间值；低倍率的物理像素舍入记录为实际路由坐标，不伪称任意源像素都能精确点中。
7. 已完成crop受控种子只验证布局状态保持，不称拖拽通过；原生保存框、剪贴板副作用、在途裁剪失捕/换图/离页、缓存身份和真实DPI分别另验。本阶段不自动读写用户剪贴板，不操作真实实例、ADB/API、代理或配置。
8. 实现前先冻结正式夹具并独立取原红；最多三轮产品修复。最终独立六正文/三桌面完整原套/六shell与主仓代表复验，文档/清单审查后单独push原私有功能分支；保留所有首次失败，不标全目标完成。

## 当前状态

三名代理已完成只读调查和反例审查；三棵新隔离树从29ab03f创建，旧树/证据保留。正式r3夹具266行/10例已由主代理和独立verifier静审，累计补丁SHA256 `2D001FCD5A9AB2534A2E0C1ABA132EFED1888A61193020B461B1822FA99E55B0`，新夹具raw `59CAFC660A1C5427942EE453DE9BD6004DE13A48BA21479EB4CF4AFE84DE7762`，Main一行hook raw `8356EF3694A3FCC403BFA31C330B66816196E84AAAB0E59C3EA1E9451F59C84D`。原几何红及首实施拒绝已保留，第二轮实现预检、独立最终矩阵与主仓代表复验全部通过。本阶段离线产品验收完成，冻结交付按末节规则独立审核后单独push；整个目标仍在进行。以下按实际过程保留证据。

运行前r0/r1/r2均保留：先补真实大图末点白像素和100合法样本以覆盖四位坐标及完整RGB，再将后端计数起点提到Refresh结束并覆盖原始大小，最后补实际轨道两按钮中心命中。均为第一次执行前的验收缺口修正，未增加case或删除原条件，不是产品红/绿。轨道零面积项只能记录不适用；不宣称轨道点击效果或子按钮44。此时固定右栏挤图等仅为源码候选，随后原几何得到以下运行证实，长坐标布局预算不足见首轮证据；像素级缺字、在途裁剪和保存来源仍未获本阶段运行验证，不能登记为已修复。

## 独立基线实际结果

首次Build为150/150（51.37秒）、0警告0错误，阶段74.76秒；同DLL `5D8B2BB57767DF621844F4E6C51A462B802A67052299E73CFEB07601B615477B`的一次fresh 1000入口执行完整10例，exit1/20.48秒，6个几何FAIL、4个PASS、10个自有窗口全部清理，110检查/47观察。不是编译或指针前置失败，没有重试。

390×640及390×844原图与实际SCP交集仅26×308、26×512，外ScrollViewer宽36；其余四尺寸的实际图像交集为332×783、564×491、864×691、1344×871，均在原刷新按钮完整44命中检查处首败。JSON没有单独记录该按钮的数值高度，不从源码32倒填运行观测。主代理直接读取10例原报告并目视390和768失败图。

无图、真实TCS Busy、安全单次错误恢复及宽窗坐标路由四例PASS：.1/.5/1/3各只有一次系统定位，真实OS/WPF/Image命中前置均成立，一次合成原Image至Canvas路由的实际事件坐标与已知像素/RGB/HEX一致，3倍双向滚动生效。它不是物理点击或Android证据。

原报告`apple-screenshot-responsive-verifier/artifacts/q/s01/artifacts/sl/ce8893a3/screenshot-layout.json` SHA256 `CDB7FC5871E4889EBF38B13D05808C16606124B7DBC8A8CBF0DB010B6B4F06AB`；汇总`artifacts/scp-verifier/baseline-r1-summary.json` SHA256 `4D4C8DB3D7CEBA690BEC0B974D76FF7A40FE2A9AC9377EC99335E0A5AB16AB0E`。两阶段124源码前后相同，原16/49/11 canonical及实际配置原SHA保持，自有进程归零后返锁。

六个几何例的后续分段、焦点、滚动、长色值和Track尚未到达，不能当作通过或不适用；旧11/16/49及其余完整套件NOT-RUN。已授权第一轮仅BuildScreenshotPage适配与局部命中，先冻结生产源码再验证，不改坐标逻辑或为未复现滚动预造恢复器。整体目标继续active。

## 首轮实施及拒绝点

首稿只改BuildScreenshotPage：原Grid在640切双栏/窄分段，四按钮、模式与Slider整体44，Checked/Unchecked同步保护及焦点可达；原图/Canvas/Scale/表格与命令持续存在。色表SizeToCells前保留原70/80/140最小宽度，避免空表预算被自动尺寸压缩；这是构建前静审的保守修正，不当作已运行复现。首轮生产r1b raw `CBD232E1088A3FA6B37AADE1FBE17CF77DEE329EE43954276542A799D34AE134`，所有像素、裁剪、保存和VM逻辑未改。

首轮Build150/150（51.23秒）、0警告0错误；一次1000完整入口exit1/20.79秒，10例6FAIL/4PASS、10清理。六几何全部越过真实画面/原44工具、Slider Thumb及两个实际可见Track中心检查，随后在首白样本坐标列失败：`899, 1599`或`1599, 899`的FormattedText宽51.1767，TextBlock实际只有48，文本在viewport内但布局预算不足。此断言不是像素级缺字证明，也不据此擅定WPF自动尺寸的更深根因；主代理与独立verifier均实读JSON，主代理目视390图。

原JSON `apple-screenshot-responsive/artifacts/t/s01/artifacts/sl/9380f58f/screenshot-layout.json` SHA256 `DA7E439AB7E7473266010618B3E6780BF07DC65AFD5B95EB83409E6E6CE85569`；DLL `6DE2B83D414A53B7D2CD31F947045B0C5CC6C4D2617FAEC90B532A50CB37CE6A`。状态三例及四倍率坐标路由仍PASS，六例之后的其它长值/焦点/滚动、旧11/16/49和完整套件未到，不能倒填绿。124源码前后相同，用户配置原SHA、自有进程0、必需exec结束后归锁。

第二轮只补坐标列初始宽70至100，经现有MinWidth保留后给当前合法长坐标约78的文本预算，HEX80/RGB140、横滚、主题及正式夹具原条件不变。这是沿用原页面固定列预算的更小修复，不修改全局Padding或容差，不宣称任意字体/DPI和任意整数坐标均已验证。raw `AB50FBBB53EB1C0641FDC9D2528F03C8387D0DC026B6F8170BB9423CF6DD361A`已冻结，原r1/r1b与首败保留；第二轮首次执行得到下述结果。

## 第二轮实现预检

第二轮150/150（50.81秒）、0警告0错误；同一DLL `FC9C4C7A48F27159DFC0FA238C8C3A7DAB114652215BB6B9EA78A006D04A4623`的一次1000完整入口exit0/81.27秒。新10例全部PASS、630检查/257观察/10清理；旧节点11/905/319/11清理、首载16/137/16清理、发布49/401/191/49清理以及原完整桌面检查和12页idle均通过。

新报告`apple-screenshot-responsive/artifacts/t/s02/artifacts/sl/8c67e2e0/screenshot-layout.json` SHA256 `DAB5DD9BA1BF655CB096F5C14259FC75731E7D866249D9AE21618A0A99D72AFE`已主代理实读；两390默认1倍实际可见图为336×220、336×424。主代理目视390画面和色值，截图中的1.5倍局部图不是整张手机画面同时可见的证明。124源前后相同、用户配置原SHA、自有进程0、必需exec均结束后归锁。

## 独立最终验收

冻结Final-r1 runner SHA256 `0297C00FF36EC8EC8DDF770FD4A762A77C0F28E1EE651B0B8F738C31C22018A9`在独立树运行Build、三个完整桌面、六公共shell，均首次exit0，没有GUI重试。Build150/150（51.05秒）、0警告0错误；DLL `BC1C28019ACBA67F84B58078C7799B967ED5170915A88F57BB24273D1D649B15`。

1000×700、1440×900、1920×1080分别83.18、85.69、87.04秒。每轮新10例/630检查/257观察/10清理；旧节点11/905/319/11清理、首载16/137/16清理、发布49/401/191/49 cleanupCompleted、外观209、Native25、筛选73/32/22、动作名36/11以及12页idle/恢复/空错误均通过。六公共shell每135断言，实际HWND尺寸与六项要求一致，另有12张自有模态窗口图分目录留存。

三份新报告SHA依次`3B6337F76AF651F508C6DEEB1C1EB489CBD7FA5688644C378E858E8F2B139EA7`、`F3A417AD9E14DBC095144AE0FB0C287D14856E908844A7245B14CF4130AE8C42`、`9E6A4186A4A929D451B000416F4AE464440D7125BB01C7A4528BE8BE22022150`，完整路径在`apple-screenshot-responsive-verifier/artifacts/scp-verifier/final-r1-summary.json`，汇总SHA256 `44476DB4601B9AE07467906341080918FD6D80FF412551F7A549744B358F5946`。54个长值单元格分别滚入实际viewport可读；12个Track中心全部适用且tested/ownsHit，不是点击效果或子按钮44证明。verifier目视390分段、768/1440双栏、空忙故障/坐标与390实际抽屉/模态。

十阶段124份源前后相同，原16/49/11 raw和canonical未变，用户配置仍`E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0`，所有自有执行结束，进程归零后返锁。独立结论为PASS-OFFLINE-A06b2b，不扩大为CAP-01实机、物理点击、DPI、轨道点击、原生保存、剪贴板、缓存身份、在途裁剪或整个目标完成。

## 页面与组件

| 页面/文件 | 改动及保留 |
| --- | --- |
| 截屏正文 / ScreenshotView.BuildScreenshotPage | 固定右栏改为同对象响应式Grid；正文640起双栏，窄屏画面/色值等宽分段；不重新加载设备或图像 |
| 原工具栏 | 原四按钮、操作模式和缩放整体44；保留原命令，模式标注“截图操作模式”，缩放有明确可访问名称 |
| 原色值表 | 坐标/HEX/RGB最小预算100/80/140，SizeToCells及局部横滚；长值分别完整滚读，不承诺三列同时可见 |
| 原画布与焦点 | Image/Canvas/ScaleTransform及色表对象不重建；完成裁剪、所选行、有效滚动位置和可见焦点保持 |
| Demo验收入口 | MainWindow仅加一行新10例接线；新ResponsiveScreenshotSmoke，不改原11/16/49 |

## 主仓复验

最终累计生产补丁`31A76AAD4A450019A4A72677D75DF089E5F2E91E879FA79452595234CA245572`与正式fixture补丁精确导入。新增fixture先核canonical及全文规范化相等再复制冻结LF原件；最终三源raw与独立验收完全一致。旧11/16/49 diff空，用户配置不变。root runner SHA256 `5066A366CE583055228D79727D229B32784C2BD75FC2F5DC5D0D2F4A201DE3EB`。

主仓Build150/150（51.43秒）、0警告0错误；DLL `4D5DE1381A5DBB6B6DD22B9FE7B7970CD71773EC6DF0EB6763E7337831ADDFF2`。首次1000×700完整入口exit0/87.66秒，新10/630/257/10清理、旧11/905/319/11、16/137/16、49/401/191/49清理及209/25/73-32-22/36-11和12页idle/恢复/空错误全部通过；390×640公共shell首次exit0/2.49秒、135断言及实际HWND尺寸通过。三个阶段124源前后相同、自有进程0、全部必需执行结束。

新报告`artifacts/t/s01/artifacts/sl/431bc6c1/screenshot-layout.json` SHA256 `249CCE22A2EBE3199CD396FF42AD77E188DBF49EB8DFE9F29C5B2D376DBE13B7`；root实读四专项及原桌面报告，目视390画面/色值、1440双栏。汇总`artifacts/scp-root/root-summary-r2.json` SHA256 `7193658ADB2F19EEF637B026F2584B46D66E2DA9AF933777FA82AFE7C898839E`。只读汇总首稿把标量matrixCases32计成数组长度1，r2按实际JSON纠正，缺省auto observations记null并保留实际727条事件；旧稿保留，不改正式断言、不重跑或覆盖首次产品结果。汇总读取过程的schema/文件共享错误不算产品失败或GUI重试。

## 冻结交付与剩余项

本项仅10份相关源码/README/正式文档及3生成清单，范围在本文组件表、任务表与交接中明确。使用已静审ignored打包helper `New-FrozenScreenshotSnapshot-r0.ps1`（SHA256 `2539EE493E73DCAEF0443A62E08DC08A47902FB4A090978888D036F068C4F680`），绑定29ab基线和完成后的10文件raw冻结；独立快照仅Git已跟踪源加新fixture/本报告两个明确新增，逐项核清单、ZIP及源字节后才回写三生成文件。独立verifier另核真实ZIP全部条目、Bundle/Tree重构、10文件冻结及13暂存canonical，审查结果和最终包SHA保存在本机ignored audit并由实际Git提交/远端记录确认，避免把包自身SHA写入被打包文档形成循环。

两用户英文PNG、本机配置、依赖、二进制、临时runner及本轮原始证据不上传。AP021历史已跟踪材料保留，不宣称这是隐私清理或纯源码认证。失败基线、第一轮长坐标拒绝及旧AP019/020/023首败全部保留；本阶段只关闭AP026离线正文范围。单独push原私有功能分支，不合并、发布或清理旧worktree。

下一步仍须分别验证缓存身份、同row重连、原生保存快照、在途裁剪/失捕获、物理输入/真实DPI/Android，再完成A07..A12、真实专用1/2和最终30分钟稳定性。本轮完成crop是布局种子、坐标是实际指针前置后的合成WPF事件，不能扩大为这些路径或完整目标通过。
