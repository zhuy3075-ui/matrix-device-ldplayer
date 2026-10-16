# AP-042 工作区正文无障碍子树

## 背景与目标

基线2ca79ce。官方Sky首次明确使用include_screenshot:false/include_text:true，成功读取独立Demo窗口：12个导航入口可见，但树止于47号TabControl，没有设备正文。原件SHA `626239CCC07A520F7B4BB2B1652D1E3EA6F6EC6210A8D025222812267DB027D4`。该探测窗口最终由精确PID/UTC ticks/路径/profile核验后的自有进程清理终止，不称自然关闭；runner的killAttempted=false不代表未发生外部清理。一次JSON日期类型比较预检误拒未采取动作。

原WorkspaceContentTemplate只有PART_SelectedContentHost，没有ItemsHost。版本化[ItemsControlAutomationPeer源码](https://github.com/dotnet/wpf/blob/v10.0.0/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Automation/Peers/ItemsControlAutomationPeer.cs)与[TabItemAutomationPeer源码](https://github.com/dotnet/wpf/blob/v10.0.0/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Automation/Peers/TabItemAutomationPeer.cs)支持这条断链候选；它们不是已安装10.0.13的逐行源码证明。实际红测再验证：正文及目标控件确实可视，从原实际tabs根peer却无法到达。

目标是恢复真实当前页控件的可访问性，不改变12页结构、外观、业务目标、输入或授权。AP-031截图捕获超时、AP-033输入源及本项正文子树缺失是不同证据，不合并根因。

## 方案与取舍

- 推荐最小WorkspaceTabControl子类；MainWindow仅替换创建类型一行，字段仍为TabControl，原Items、Selection、Template和事件保留。显式引用原typeof(TabControl)样式，避免派生类型丢失隐式Style；真实名称为“工作区正文”。
- 标准FrameworkElementAutomationPeer遍历原视觉树，仅把根语义明确为Pane。不覆盖GetChildrenCore、制造叶子或TabItem provider，也不手动Invalidate/Reset缓存。无头导航模型的UIA SelectionPattern/ItemContainer不再提供；实际12个侧栏入口仍是导航，不能宣称UIA接口完全兼容。
- 更简单的只给容器命名不能恢复断开的子树。隐藏ItemsHost可能造成不可见/重复TabItem与焦点副作用，重写内容宿主扩大变更，伪造provider或全局兼容开关掩盖根因，均拒绝。若标准peer不能正常更新切页，应推翻方案，不修改期望取绿。
- 正式源码只改MainWindow一行、新WorkspaceTabControl、新WorkspaceAccessibilitySmoke和App已有严格Demo入口一个hook。旧22个Smoke、原几何函数、Theme、API、VM、Wall与用户配置不改；不新增外部UIA/Win32/SendInput自动化替代工具。

## 验收与执行

1. 两独立工作树基于2ca79ce。先只加夹具和hook，用实际tabs.CreatePeerForElement一次生成根，真实正文可视前置成立后读取根后代，不能拿编译、环境、role或样式错误冒充产品红。
2. 设备→应用→设备使用同一个根peer。按实际owner、名称、ID、角色、IsEnabled检查正文，前页owner不得残留；原12导航只作结构检查，不假装已访问其余十页。保留选中页、输入、勾选、代次、模板与样式；没有Invoke、Focus、OS输入或后端方法调用。
3. 同一夹具不改期望，加最小生产修复后单次独立Build原158及既有安全Demo，再主仓代表。每child300秒、自有清理5秒、各原始流排空10秒；初错停，不自动重跑。App实际顺序是旧14报告契约、新无障碍契约、原几何诊断，不把先序汇总的NOT-RUN当最终到达结果。
4. 修复版用fresh Demo1000×700及官方Sky重新选择唯一窗口；只读false/true，随后逐次一个导航动作和立即重观测，最后正常关闭自有窗口。超时、空树、权限或目标不唯一就停止；无截图恢复或自制桌面控制绕过。像素、原生安装确认、真实模拟器及双实例不由本项继承通过。
5. 独立源码、实际结果、文档、完整包与精确暂存审核后单功能推原私有feature，不合并发布。CMFA APK仍保留；先前单实例共享层安装/启进程与owned卸载有证据，不等于当前GUI安装/卸载或主页已通过，不代授权或开启VPN。

## 实际验证

| 运行 | 原测试及报告 | 正文根契约 | 原安全几何 |
|---|---|---|---|
| 独立red-r01 | Build29992 exit0/95.165秒，158/53.17秒，0警告错误；旧14通过/8EXPECTED | mode25504 exit1/1.411秒；三阶段同root5，各仅1个Tab根，5个可视owner全缺失；纯PRODUCT-FAIL，非role/前置红 | NOT-RUN，新契约失败后未进入 |
| 独立green-g01 | Build41044 exit0/65.125秒，158/51.45秒，0警告错误；旧14通过/8EXPECTED | mode66672 exit0/2.051秒；同root1/Pane，节点146→71→146，5owner各唯一可达，真实名称/状态/样式正确，无前页残留 | 实际到达原3E报告 |
| 主仓root-r01 | Build23328 exit0/73.585秒，158/51.99秒，0警告错误；旧14通过/8EXPECTED | mode19844 exit0/2.281秒；同root1/Pane，146→71→146，5owner与真实metadata/state/style通过，0residue | 实际到达同一3E报告 |

红summary SHA `3C3278E6EB5AD6A163908B0944EDBDD40AB8D994168FA1318176B6EEF905DD68`，execution `32BFEF06FF97C80C50EB56193A103963BE99A0853B260B96F22790B201EAE119`。独立绿实际末快照SHA `A3DF5C7BA6199887C2EFE673D380FF04B0A78A21D5FC5DA0E548269B77073FEB`。原3E几何报告为 `3E0F0766BF6AF78A5DDBA9B469CA87CE4872B6F852AEC3FBBA1536C0D2D1DACE`，原六快照/22任务及清理均保持。新契约每轮5个自有Dispatcher任务终态、恢复与排空通过；旧报告的8条预期错误保留，不称stderr为空。

主仓execution SHA `6844B7C701160A26B111B25FB6F7549FCBE55D19C3B44CEBCCAC5767888BB592`，末after `401E2D61055453689F1BCD39221518CD9FCCFDDC3E1947A0AB87779414CFA27D`；主仓及独立绿summary均为 `0675A09D2DF4B0222345EC658BAD2EBB3B539E42DE57C3B9DA045F21F95116FC`。主仓152项canonical/规范化全文与独立已执行末快照相同，148原输入及22旧Smoke保持，实际PDB核30唯一路径；原配置与两用户PNG保持。主仓DLL为 `F18C0E9A1292F07074D3045130F2AC4C2047F8F947E87FD63D3817C073CF8387`，全部必需进程和原始流终态，无超时或运行期清理错误。独立绿后置本地回执草稿无效，已另作INVALID通知；只引用真实execution/summary/after/PDB/原流及文字独审，不引用该草稿为通过证据。

## 官方Sky观察与当前裁决

新fresh Demo1000×700、PID68696/唯一返回窗口198643342。现有调用transcript已核实际传入include_screenshot:false/include_text:true，工具envelope为isError:false；首个state却为accessibility:null。screenshots=[]在明确不请求截图时是预期结果，不能另算截图失败。原件SHA `54374E6627F596F082827E9622033607A432080DEF73E8E4929DAC2B4360197A`，现有请求记录摘录SHA `A7251572D53FE206AA139DF59A8D0F169015CBABAFF1E5776B8548C52858CB57`。当前文档/API没有暴露可信helper日志位置或日志读取接口，没有扫描实现或伪造内部日志，不能区分工具降级与产品提供者问题。

已按空树停止：没有activate、导航、点击、截图重试或替代UIA工具。随后精确核PID、UTC ticks、可执行路径、--demo及自有profile，只终止该自有进程；externalKill=true，child exit-1、父session61577 exit0，流完整，无超时。清理原件SHA `FBBF0D79B2C6BE8BAA998E224B8B0E1B1FA7AC242492426ED14767B9F90A7775`、terminal SHA `76F3AFD746DDDDE84CBE9FA901F4479FE677C4D095058F4E519A40017B6CCA5C`。runner.killAttempted=false只表示runner未自行终止，不是未发生清理或正常Close证明。

首轮独立reviewer裁决为暂拒交付：整树未返回，不能因组件绿就排除相关产品回归，也不能定性为环境故障。该轮及全部失败原件保留，不被后续可读覆盖。

后续AP043独审允许考虑**仅正文peer子树/初始设备正文只读可达的私有开发分支限域源码交付**：自有组件两页往返通过，受控外部首读已有改善证据。它不是AP042整体验收批准；官方应用导航、返回及正常Close仍未通过，稳定性未证。完整文档冻结、核包和精确index仍需独审，实际提交/推送以Git与本机回执为准；未实际发生前不写已交付。不合并或发布，不把这项源码改善称为官方控制/全GUI恢复。

## 后续诊断

受控A/B现已完成，详见[AP043实际记录](AP043_SKY_AB.md)。同一共同44及实际CLR/WPF两模块下，Old48总节点止空Tab，New200总节点含设备正文，支持初始正文暴露改善；不是恢复整个历史构建。随后官方应用入口点击返回明确几何错误，无后置state，停止且精确externalKill/流排空，不称导航成功或自然Close。原F18整树null仍未定因，不能由后续可读倒填原首败已解决；完整导航门保持开放，源码限域交付不关闭该门。

另可在自有WPF测试中从实际WindowAutomationPeer检查完整父子链、可见/Loaded及有界异常；它只能补窗口提供者链证据，不能替代Sky验收。不得新增外部UIA/Win32输入绕过，不重复原截图捕获路径。原S01/11项NOT-RUN、AP019/020/033、AP041后置定位、AP040反馈、全GUI、真实双实例同步/当前安装卸载、最终版本双实例30分钟、真实DPI与A08-A16仍开放，完整目标未完成。
