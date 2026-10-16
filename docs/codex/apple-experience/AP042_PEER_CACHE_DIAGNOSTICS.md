# AP-042 切页子树只读诊断

## 背景与目标

基线`84e0b5842945db2db79a22a5c2ca4b7027ebe94c`。上一阶段A14a1已实际推送该提交，回执`artifacts/script-bindings-main/push-complete.json`，SHA `1C07BF5697D4A144F4EA5B07E0CCF5DC41DD7B7447B97739E15E7AB7EF67F555`；193项和无窗VM4只证明参数业务，不解决本项GUI门。

旧AP042原件summary `28342CF8F2D737B74EA7625C5FB706890ACBBE3C11084333F2499657AA8631BA`实际三阶段146→146→146：同一真实宿主的正文从设备63变为应用266，应用正文可见且346×470，但原根返回相同首层peer/owner，三个应用控件缺失、旧owner残留111。五个原Dispatcher任务、恢复和排空成立。该PRODUCT-FAIL保留，不重写为环境或夹具错误。

根问题是实际可视正文与公开根peer返回子树不一致。现有Pane peer只定义角色，标准GetChildren有缓存，不能假定每次会重读视觉树；但尚未证明具体缓存位、布局失效链或外部provider条件。此阶段只补能区分这些候选的实际观测，不先改生产或放宽验收。

## 范围与约束

- 仅在基线隔离工作树修改`Views/WorkspaceAccessibilitySmoke.cs`的诊断，原App入口、WorkspaceTabControl、布局、业务、授权与其它24个Smoke源不动；原三阶段、全部Product/Require、5秒ApplicationIdle Flush和恢复/排空条件不变。
- 不增加GetChildren、GetParent、CreatePeerForElement调用，不手动Invalidate/Reset，不更换根或制造叶子，不调用UIA客户端事件订阅。原GetChildren每次仍只调用一次，以原返回集合继续原遍历，原Node getter次序保持。
- 不请求激活、焦点、指针、截图、原生输入、guest/ADB/API或真实模拟器；不加载用户实际配置，不重启共享服务。官方AP045外部鼠标输入分支仍停止，本诊断不是重试该分支。

## 只读观测

1. 原Navigate之前、原Flush之后和原ReadTree前后同步记录SelectedItem/SelectedContent/presenter.Content及原模板、宿主引用；真实host直接视觉子链、当前/旧正文Loaded/Visible/measure/arrange/尺寸与PresentationSource。每条诊断使用独立身份表，不改变原报告ID分配。
2. 记录预期控件、旧wallSelectAll及最多八个残留owner的有界Visual/Logical/TemplatedParent链与当前页归属。只读取现有对象，不创建peer或调用布局。
3. 对原遍历中每一次GetChildren记录其原返回集合和peer/owner身份、阶段/序号，保留null与空集合的区别，不重复查询。每个原位快照必记`FromElement(this)`返回的Window既有peer存在/类型/独立ID、`FromElement(tabs)`与原根引用是否一致，以及StructureChanged/PropertyChanged的`ListenerExists`布尔；不查询新peer的Name/Children/Parent/provider。peer存在不等于已接入provider/AutomationEvents，listener快照也不能单独证明因果；不得触发失效或建立客户端监听。
4. 临时观察WPF选择、SelectedContent/presenter.Content通知及两正文Loaded/Unloaded/IsVisibleChanged。回调只读引用与记序号，不导航、布局或访问peer；finally逐项移除。日志有界且溢出明确，诊断异常另列，不覆盖原Product失败。

## 方案与取舍

推荐先补上述只读原位记录。更简单的重跑旧链只能再次得到通过或失败，不能区分原因，故不采用。拒绝通过延长等待、轮询到绿、手动重建peer缓存或新造provider改变被测状态；也不把listener不存在直接等同夹具错误。若日志本身改变调用次数、原断言或调度，则退回修改诊断，不据此改生产。

若视觉host已为应用、旧owner已脱离且原GetChildren仍返回旧身份向量，可确认“本次公开根返回陈旧子树”，但具体内部缓存位仍需证据。若新一轮意外通过，只记本轮观察，不宣称修复。下一步仅在证据足以区分生产与测试条件后，另定最小修复或受控标准容器对照，原失败不关闭。

## 官方来源与尚未证明的条件

独立调研定位[WPF v10.0.13发布提交4726a1a](https://github.com/dotnet/wpf/releases/tag/v10.0.13)，不以10.0.0代指当前版本；尚未用本机框架DLL/PDB的SourceLink证明逐方法对应。

- [AutomationPeer缓存实现](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Automation/Peers/AutomationPeer.cs#L1343)：GetChildren可以复用有效缓存，InvalidatePeer异步更新不等同同步重建子缓存。故本阶段不把加一次InvalidatePeer当保证修复。
- [UIElement布局链](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/UIElement.cs#L502)和[HwndTarget注册根](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/HwndTarget.cs#L1285)：局部创建peer与正常窗口provider初始化不是同一件事，布局自动化根登记是待区分条件。
- [EventMap首次监听](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/MS/internal/Automation/EventMap.cs#L193)也能登记根；反之listener为false不证明从未登记，Window peer存在亦不证明provider已连接。过去通过与本次失败可能与这些条件有关，但只是待验证推断，不能改判旧结果。

## 任务与验收

1. P0：两只读reviewer审方案；准备独立实现/验证工作树，冻结当前156编译输入、193测试名序、原Product/Flush/清理和其它24个Smoke。
2. P0：仅增加诊断，静审精确diff和调用次数；诊断通过编译不等于产品通过。
3. P0：明确独占执行锁后，唯一Build193及一个fresh390×640 Demo诊断；每child300秒、只杀自有child且5秒有界、原双流各10秒排空。首错停止，不自动重试；先序失败后继如实NOT-RUN。新日志应完整/error0/无drop，业务守卫和原五任务/清理保持。
4. P1：独立核实际PDB/源码、原始流、诊断及原结果；若需交付诊断，仅按已验证观察范围更新README/问题表/任务/阶段记录，完整核包/index后单独推原私有feature，不合并发布，不当GUI/实机或全目标完成。

## 当前状态

参数绑定阶段已实际推送84e0b58，不重做。实现与验证树均从该基线隔离；本阶段只改唯一正式fixture，106行增加、1行替换。原GetChildren仍各调用一次，全部原Require/Product断言、五任务、等待和恢复路径保持。源码SHA `66EFC759A95218E8701B336DF67ED664A58D8EF632A0ECB1BB24FFFEB5AF0560`。主仓原baseline SHA `931A016428FA4E994AA2601BD636DDF469DC7947934A3BE3466ED185EB78FE2D`。

| 实际执行 | 编译与业务回归 | 原390×640诊断结果 |
|---|---|---|
| 独立验证树 | Build84320 exit0/88.736秒，193/193；VM20012 exit0，4PASS | child51972 exit1；原14类PASS/8EXPECTED，AP042原4条PRODUCT失败 |
| 主仓 | Build308 exit0/73.978秒，193/193；VM38456 exit0，4PASS | child45716 exit1；同原PRODUCT失败，后续AP040与几何NOT-RUN |

两轮各有466条完整记录：438次原GetChildren、14个快照、14个WPF事件；9项观察全部解绑，诊断error0/drop0，原五Dispatcher任务终态、恢复和排空成立。两轮都不是产品通过，也不是新的实机或外部GUI验收。

## 实际观察与限制

- 同一宿主18的SelectedContent/Content实际为4→27→4；应用正文Loaded/Visible与measure/arrange有效、346×470且接入HwndSource。旧设备页4在应用阶段已Unloaded/不可见且visual脱离，但logical仍经TabItem保留，不能称所有关系断开。
- 原根31三次返回同一List50及相同六个首层身份；全146个peer的返回集合与子身份向量均三阶段相同。实际仍146→146→146、应用owner缺3、旧owner残留111，确认本轮公开返回子树陈旧；未证明某个私有缓存位或唯一根因。
- 14个快照中现有Window peer均null、两类listener均false、EventsSource均null；这些条件不是provider从未注册、环境故障或夹具错误的证明。同步观测前后原状态相同，也不承诺日志无分配/时序影响。
- 独审核156编译输入全文与独立实跑候选相同，155其它原始输入、24其它Smoke及26原守卫不变；fixture是明确批准的唯一例外。实际五PDB覆盖38唯一源checksum正确。原配置与两用户PNG保持；不要求重建DLL字节等同旧Git提交。
- 主仓六条原始流全部终态且完整，无timeout、kill或清理失败。stderr11057字节保留，逐项核对为原8个预期报告错误、4个Product Aggregate及原throw栈，不称空stderr。全部自有child已结束。

独立execution SHA `F17641B3653D5D69E19B7C356D3599C60D6AB819ABDDD50116672F905FC1BCBA`，summary `FF78AB63658932972B4D3EDF4100103A3B377CC0164E6E93D01C9EE2CAE2BDB7`；主仓诊断execution `62A5102BEDE6675492B235221E0E2EFC2F943D9D82C8CE4304A20EBEEAD10F7D`，summary `900656D5DC61BF5A5D6EDFC96EF6F56DD08F0208C3887B0999AA410CB83F4C1D`。主仓终态原件`artifacts/workspace-cache-main/main-terminal.json` SHA `8CA8EF2E1EEA29DDFCED34C70607B6774610BA0192F7299F54A4F6517638C06C`。原始证据留本机，不随本阶段源码上传。

## 裁决与下一步

独立verifier仅批准诊断源码有限交付，不批准AP042修复或整体验收。正式文档与完整核包/index审过后单独推原私有feature，不合并发布；实际提交以Git记录为准。下一阶段另定“官方客户端读取前后对照”轻规格，先用受支持的纯文本读取区分条件，不预称完整provider初始化或因果成立；不得恢复AP045输入、使用私有反射/原生UIA或强制Reset凑绿。真实双实例、应用GUI安装卸载、最终30分钟及A08-A16仍必交付，完整目标active。
