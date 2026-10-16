# A07：应用正文的尺寸适应

日期：2026-10-16；基线1de5136b52515dc1196341f70d74994182513810。承接API排空修复后的应用页，范围来自R05/R07/R09及A07；不替代真实应用安装卸载GUI、双实例或最终30分钟。

当前是有限域主仓核验阶段，不是下文第三轮拒绝时的执行状态。[A07d诊断与修正](A07_TOOLTIP_DIAGNOSTICS.md)已分别证明七点测量模式差异，以及字形试验的历史范围/施加前提不完整；原f03、d01、c01与d02报红均保留，不回填。

冻结候选独立c02与主仓r01均已实际通过原158项、新12项/2890检查/12清理和普通Smoke原17源调用链/12页。六应用尺寸三表首中末及全部列、停止后续/恢复、焦点/状态/勾选保持；单390×844字形压力覆盖当前505字体成员（含344隐藏），非系统DPI或原字体绑定保真。root只精确导入App2C9853、Theme567766、MainA752及新夹具7248，未再改生产。

必须保留验收缺口：20是冻结旧Smoke源数，另三个独立入口不由普通Smoke调用；s01共享壳层六格各135通过，但设备首格指针前置及报告保存失败，余11格未执行，AP-041另追踪、不与旧AP019/020/033合并归因。主仓r01不是s01重跑，也不替代这些缺口或真实双实例/安装卸载/最终30分钟。原始结果独审3EA80FE0...3EE1BE仅批准原私有开发分支有限域交付，核包、暂存及Git结果以实际记录为准，不合并发布；不得宣称全GUI或最终交付完成。下文为完整准备和失败历史。

## 事实与目标

已读PRD、SPEC、UI_SPEC、TASKS、最新交接及ApplicationsView、共享Page/Toolbar和截屏正文响应式。原应用正文固定228+12+star(min200)双栏；任务区Height182，工具栏本来就是WrapPanel。表格列最小预算允许局部横滚，不能仅凭列宽大于窗口认定缺陷；需要测实际viewport、命中、反馈与动作可达。当前只有源码候选，尚未取得本阶段运行红。

根问题候选是同时展示的固定区域超出窄正文的有限空间，而非字体太大或功能太多。目标是在六种请求尺寸的真实自有WPF窗口中保留目标、应用、任务全部信息及操作，明确当前独立目标和批次状态，不整页缩放、删列或复制第二套表。

## 推荐与取舍

推荐宽屏保持原双栏工作区；仅实际正文不足时，将同一目标表、应用表和任务表按可返回的分区重排。目标入口、主要动作、批次进度与停止后续须可达，长反馈局部有界滚读；宽窄往返保持对象、选择、排序、过滤、在途任务和全局上下文。断点由实测正文宽度确定，不按数据内容或外窗宽度循环重排。复用共享主题、Toolbar及已有分段控件，命名简短明确。

更简单的三块纵排可作为备选，但390×640中多层滚动可能把主要内容挤出首屏；须有实际可达证据才采用。拒绝缩字体/整页ScaleTransform、删除唯一功能、复制表/集合、resize重新读设备、将应用独立目标回落到群控勾选。若原布局实际通过，推翻静态候选，不为设计偏好制造产品红。

## 范围与约束

- 生产限定ApplicationsView的BuildAppsPage及其局部纯布局/命中辅助；不改API通知、350ms调度、pending消费、批次计划、确认、身份/授权/Busy/保护门禁、数据格式或全局Tag。
- 保持12页和应用独立目标，不新增左侧导航或第二套安装/卸载入口；原读取、APK选择、卸载、更多、系统过滤、全选和停止后续仍有明确入口。
- 新专项独立文件和Main唯一首hook；20份旧Smoke原件及调用相对次序保持，包括刚验14项API、12项pending与旧输入前置。新专项失败时旧套尚未到记NOT-RUN，不跳过旧AP-033取绿。
- 仅自有一致Demo/STA、非转发stub、逻辑控件与内部截图/命中检查；不使用Win32/UIA/SendInput新增Windows输入，不操作真实guest、共享ADB、账号、权限、VPN或原生文件框。真实DPI、物理输入与Android效果单独留验。

## 验收与计划

1. P0：冻结正式新夹具后，原生产单次Build -Verify及完整1000启动smoke。新夹具逐一建立390×640、390×844、768×1024、1000×700、1440×900、1920×1080的真实自有HWND，断言ActualWidth/Height，不允许Clamp取绿。先测原有控件几何和裁剪/可见命中，不能以不存在的新分区控件作为基线红条件。
2. P0：经原目标CheckBox/TwoWay及原List路径加载可辨长名称/包名、多行应用/任务；主要按钮、目标选择和停止后续可达。表格首/中/末行及水平内容可真实滚读，长结果不遮挡关键动作；空、真实TCS在途、单次失败及恢复各自留证。
3. P1：修复后同原断言复验；窄宽往返及断点正负1保持原控件/行/集合引用、应用与目标勾选、过滤/任务状态/全局Current/Main/Targets，纯重排不新增backend调用。紧凑目标选择与主要动作命中至少44逻辑单位；焦点/返回路径可见，不靠隐藏活动区域继续操作。
4. P1：150%字形布局与上述普通尺寸分开记录，截图目视六尺寸；真实DPI不冒充已测。全部原158、20旧Smoke及12页真实到达后报告其实际状态；每child300秒、owned kill5秒/双流各drain10秒，全部Task/自有进程终态才归锁，不因观察yield重启。
5. P1：测试、生产和独立verifier分别隔离树；原生产取红后root只改范围内布局，再独立复验、主仓冻结整合、README/状态/清单和核包独审，单功能push原私有feature，无合并发布。最多三轮review-fix，原失败保留；不上传本轮raw、APK、配置、依赖或两用户PNG，不清历史材料/worktree。

当前为执行前轻规格。原版实际红、修复、主仓及交付尚未发生，不从既有共享壳层或API专项通过推断应用正文已经适配。

## 原生产基线

上述“尚未发生”为准备时快照。r1原稿8013CB...B954未运行，保存后r1a仅Case报告分类净加7行，避免产品红后又发生前置/夹具异常时被混记纯产品；动作、断言、等待和清理均不变。r1a正式SHA `F636F27E340D4534C0C8E70EF17F70C256F058809D71973AAD4EF1DAF881199F`，Main唯一首hook SHA `A752C6BF763751EDD837D33007689583F5E55D14C5F9BE03847E2297D5F5D1C7`，20旧Smoke与其余原输入保持。

独立baseline-b01仅一次，session91870已终态：Build37928正常exit0/79.843秒，其中实际构建27.61秒、158项51.40秒；新fixture原418行CS8602经WPF两轮产生两条警告、0错误，不能改写为零警告。实际PDB精确核24源。Smoke8888 exit1/11.236秒，11窗口case全部到达和清理，记录10 PRODUCT-FAIL、resize-state PASS，无MIXED/前置/夹具/清理类；新专项聚合后原20专项和12页全部NOT-RUN。

六尺寸均先以实际HWND核请求值，再原控件勾选/350ms/12次List生成96行（默认84用户应用）与12项任务。390×640实际正文346×470，原固定双栏把右侧动作与表格挤出可见区，已目视本机c01/loaded.png；多处原32/24高控件与选择列的实际44命中失败，应用与任务原长字段缺少完整阅读路径。桌面几何阶段通过不能掩盖字段阅读失败，也不能仅以总列宽判红。

失败恢复的后续原读取按钮阶段实际PASS。in-flight先证明真实List在途，但在停止按钮44断言处停止，未点击取消，11项后续跳过效果当轮NOT-RUN；清理放行只读剩余批次，最终12任务不能当取消成功。字形case在44阶段失败，其余150%全表滚读未到。宽屏目标的已有Tooltip在测试默认自有提示框组合断言失败，但未留全部分项渲染值，归因仍不明，不据此改共享主题或把全部10 case根因都称已证明。

五次148输入、20旧原件/API/Main剩余全文、原三PDB副本与配置E4E33保持。110条backend List Task记录均终态，VM初始化/关闭另有有界await与UI终态检查，不宣称全部VM Task逐条登记。双流均排空，无timeout/kill，自有进程0后显式归锁。原报告在工作区`.worktrees/app-layout-verifier/artifacts/l/b01/artifacts/al/cfe31939/application-layout.json` SHA `61A826CD76B3C0C8296440CEA6D9FBF54522132BAA7E5EFCE04BED9E84064CF5`；独审SHA `A2E16F66E413ECBA0C5EE4742709F445FFFB13CEC4581D487902B127718F09C7`，原失败保留。

## 首轮修复准备

root仅在app-layout-fix修改ApplicationsView的正文、选择列及局部表格尺寸；API订阅方法及ApplicationCommand起全部业务尾部规范化全文保持。共有操作、进度/停止与有界反馈，正文640以下同三表分区，宽屏仍228目标栏；局部44、完整值Tooltip及固定选择列，不更改共享主题。独审指出新44列头勾选框仍会受旧38列头裁剪，已将本应用表头设48；退宽时目标禁用的焦点回到可用停止/更多。当前生产候选SHA `F1AC4399155CB66A4E4B750B2E186B9EAA31D74719A5472898007CE369B8F1B2`，尚未构建或执行。

r1b在已保留r1a基础上只加Tooltip原断言前的渲染诊断与唯一Vm!警告修正，原断言和动作不变；SHA `559D072D71A0351FE1AABF93E764723C8B787F2AFAC10FCD11BA31F779A76FE7`，未运行。r2已冻结为581行62618B，SHA `3FD08887486017C041581202D41CF9AA9F781285266D6406DB921F2984BE62D7`；原11合同保留，追加第12条原表头peer全选84用户/系统过滤后96行及清除，以及两条窄段焦点退宽合同。表头按Page与原表交集验证，不误用仅包含行区的ScrollContentPresenter。精确增量SHA `9DF82DBB78F2F01965ED3AE00DECB412210B0C6B0A6218C8F38502371A4AC605`，独审通过、未运行，不倒填原11例。Main仍A752；最终静态快照、独立f01、主仓和交付仍待。原两PNG、真实GUI、双实例及最终30分钟缺口保持。

## 修复候选首验失败

f01已执行并终结，不覆盖或重跑：Build41276 exit0/63.645秒，实际构建11.95秒、原158项50.99秒、0警告错误，实际PDB24源正确。Smoke15476 exit1/0.485秒，窗口构建即在ApplicationsView106抛InvalidCastException，12条新case及全部旧专项/12页均NOT-RUN。原stderr828字节SHA `0872656F9BA7B2E36AC35965316678ADDF364B72A9B93CDE60F778F16EE7034F`，双流排空、无超时kill、owned0。

根因是新增Tooltip样式将所有column.Binding强转单值Binding，而原TextColumn对State使用带语言converter的MultiBinding。只改为复用原BindingBase，沿用WallView96既有写法，不改共享TextColumn、状态翻译或测试断言；此时仍是待修复的候选启动回归，不称12项布局已验收。后续另用新f02保存结果，f01全量保留。

## 第二轮实证与第三轮范围

f02使用单行修复后的ApplicationsView `2C9853CCFA0458AEDA41E06C670B0E69B5739068A76B457DD7F2FA067333B735`和原r2夹具。Build57944 exit0/63.688秒、158/50.95秒、0警告错误；Smoke49460 exit1/13.072秒，12例全部到达/清理，4 PASS、8记录PRODUCT-FAIL，960检查、111后端List Task终态、禁止操作0，原20专项/12页仍NOT-RUN。原报告SHA `477EC4F3061F4B9365B5130F829B5F74D301A62C9B03CA6C6EF28F19F2D0FFF7`、独审SHA `52D9E28BF8CE27C1D21671BF2983ED226A9064558F164128BF734CA921E86E02`保留。

六几何阶段都PASS；空、真实在途取消、宽窄状态/焦点、表头84/96选择整例PASS。取消实证只有1次List，12任务中11项后续取消；失败后原读取恢复PASS。这些不能补齐尚未达到的全部字段/字形滚读。

两类失败必须分开：原ReadTable误将两个选择列复选框模板的隐藏CheckMark当数据文本，此项是夹具问题，不能靠给图标加Tooltip修产品；只排除此装饰项，保留原44命中并核其没有Content、保持其它全部字段阅读断言。真实长结果提示框已实现且样式MaxWidth360，但正文实际924×17/NoWrap，原first/last marker超出弹层；不是弹层没有打开。

因此第三轮最小范围增加ModernTheme中仅ToolTip字符串数据模板：显式换行，保留非字符串及自定义UIElement默认模板；不改变其它主题或业务。与应用页另建第二套提示框相比可修复同一共享根因，但影响所有默认字符串Tooltip，须跑原全Smoke/12页，不能仅凭新专项放行。采用按类型的数据模板方式，参考[Microsoft数据模板概述](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/data-templating-overview)，实际效果以本机新渲染为准。拒绝取消换行断言、裁短原文或只拉宽弹层。

此处是已测根因后的规格增补，不代表第三轮已执行或通过；原b01/f01/f02及二进制/源码冻结保留，不重复取绿。最终第三轮、主仓、交付及真实GUI/双实例/30分钟均待。

## 第三轮结果：不予交付

f03完成并明确归锁，不进行本阶段第四轮。Build40136正常exit0/63.808秒，实际构建12.15秒、原158项50.99秒、0警告错误；Smoke67528正常exit1/16.953秒，无timeout/kill，四流完整排空。12例中5 PASS、7记录PRODUCT-FAIL，2033检查通过、111条后端List Task终态、12清理、禁止操作0。旧20专项和完整12页仍NOT-RUN；notRun空数组只表示新12例均到达。

六尺寸的geometry、targets-scroll、tasks-scroll和no-extra-backend阶段全部通过；空、真实在途取消、失败恢复、resize-state及表头选择整例通过。长失败Tooltip现在实际Wrap，弹层354×82、正文334×68，原完整字形断言通过；不称尚未达到的所有字段或150%正文已通过。

剩余7例均在应用首行列5版本串`2026.10.16-preview-long`提示的高度断言失败；列1目标、2序号、3长包名、4长名称已读，列6类型和应用中/末行及完整150%路径没有达到。版本Tip已实际打开，弹层163×31、正文143×17、Wrap、字号12。此处既不能把红抹去，也不能称7个真实裁剪缺陷：当前没有记录FormattedText.Height及实际TextFormattingMode，Display/Ideal或取整差异只是候选解释。

独审结论拒绝，root未导入生产/新夹具，没有运行主仓r01、核包、暂存、提交或push本阶段候选；主仓仍为已推1de5136。修改源仅留隔离树：AppView `2C9853CCFA0458AEDA41E06C670B0E69B5739068A76B457DD7F2FA067333B735`、Theme `567766D0AD9D2C2FF122DBBE1F64299044D2109CBA8304D89268E1281936CA06`；新r3夹具 `A8031C03B2A28993920C8C88FD6D81AAD7F4289F26C59BD16A82ED6D03BCA7B0`、Main A752保持。r2选择列误读已按原件/精确补丁保存，不能倒填旧b01/f02为绿。

实际证据位于工作区`.worktrees/app-layout-verifier`：报告`artifacts/al/f03/artifacts/al/86abb81d/application-layout.json` SHA `1EE72CC53826C148F4BD792B3E2048C1374030E1D4EF4BFC0A2B1FDB0BCDA806`；独审`artifacts/app-layout-verifier/final-f03/independent-audit.json` SHA `C54D18C67C68C0E2E51A3810BDC3F049651F7167EAA6DD97715BB1969E529B8C`；acceptance SHA `D7B243BED440D0D7AE3FB977C36CB9281148A0DD301EA4FE6D92101538E0D28C`，末快照SHA `08839E65F6EE396FD1BFDC52B55CC8ADB01B50B860AB0118F5C71E7137FF2F1D`。5次148输入/其余144/20旧/API/Main全文及实际PDB24、主配置E4E33/owned0均核，四源码、3PDB/DLL和原日志留存；stderr29631字节保留，不称空。

下一独立诊断阶段先保持生产和原断言，补真实FormattedText宽/高/MaxTextWidth、TextBlock/Tip/owner有效TextFormattingMode、Desired/Render/Actual尺寸、rounding/snapping、字体/文化/FlowDirection与DPI；显式有效mode和Ideal/Display仅作对照，不择绿替换断言。保存自有Tooltip内部图像及正文相对容器的bounds/clip，目视末尾`-long`。先区分测量语义差异与真实裁剪，再决定修夹具或产品；不是本阶段偷偷重跑f04。真实Windows输入/DPI/原生安装框、仅1/2双实例同步与最终30分钟、A08-A16仍全部待验。
