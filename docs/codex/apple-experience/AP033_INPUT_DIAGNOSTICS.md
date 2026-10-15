# 截屏输入前置的可观测性

日期：2026-10-16；基线`ab11ec94fe6b7ab139a922ac346850ed7c292501`。上一目标轮属于progress：A07a两文件修复、独立验收、主仓构建与失败分类、16文件独审及提交推送已完成；远端feature同ab11ec9，test备份49e4710未改。完整目标保持active。

## 事实与最小目标

- 旧截屏`coordinate-route`在系统定位后、合成鼠标事件之前核验OS/WPF/原Image命中；该前置不成立就停止，不允许弱化。
- 独立f01实际OS等于请求但WPF坐标失败；主仓r02实际OS也偏离请求，且失败早于新增A07a夹具。f02单次通过不能证明hook位置或应用代码是根因。
- 原日志缺输入源、捕获与同一OS点的直接坐标转换，无法确定偏差来源。`IsActive=true`本身不是输入源有效证明。

目标是补足能区分失败模式的原始观测，不凭推断改生产代码，不把诊断通过称为鼠标或全程序已修。

## 范围与方案

P0：仅在旧`ResponsiveScreenshotSmoke.cs`的原定位前、后与原循环结束，每档倍率增加三次有界只读快照，四档最多12次。记录系统位置前后、同一OS点经PointFromScreen的坐标/命中、WPF位置与屏幕转换、输入源和目标PresentationSource、捕获/悬停/焦点及自有窗口状态；只序列化primitive，不保留活动对象。诊断异常独立记录，不能吞掉原异常或修改原断言。

推荐复用现有只读helper；只改该测试文件，保留原SetCursorPos、Mouse.Synchronize、循环、等待、事件、断言和清理原样。更简单的重跑只会改变结果，不能解释首次失败；增加重试、抢焦点、额外同步、反射或把OS点直接用于取色会改变合同，因此拒绝。若需要生产修改，另按实际证据形成小范围修复，不借本项扩大。

非目标：A07b自动读取、应用正文响应式、真实设备、原生保存、DPI与全目标验收。禁止触碰CODEBRAIN、本机授权、ADB、账号、代理/证书、原实例0或用户实例；只运行自有Demo和artifacts内配置。

## 验证顺序

1. 两个新独立worktree `apple-pointer-diagnostics`、`apple-pointer-verifier`基于ab11ec9。桌面工具因工作区根不是Git目录拒绝创建后，使用真实子仓的精确`git worktree add --detach`建立，不删除或清理旧树。
2. 实现者只增加诊断，冻结源/增量SHA；root和独立verifier核对原行为字节及所有生产文件未变。
3. 独立树机械导入该单文件，仅一次Build -Verify及1000×700完整smoke；各child300秒和有界自有清理，保存原始双流和源/配置前后摘要。观察yield不重启进程。
4. 无论PASS或FAIL都逐项读三快照和旧检查是否到达；后续未到明确NOT-RUN。失败先分类，不凭结果重复运行。诊断只能证明本轮所观察状态。
5. 若观测改变下一行动，更新AP-033与交接；有可验证修复才按独立验收后单项push。全部实际执行结束再回交锁。

## 实施与运行前检查

主仓ab11ec9、两用户PNG和配置已重新核实。诊断仅49行新增，源SHA256 `70DF27ED182D12B298D648C015DFBDE792C3EAED1F2FC543F11BFB6052D72B4F`，补丁`03151146313F789F60EFC01C91A5F63CAD63288F76C9289D33FBA6524EDFF79C`。root与独立verifier逐行静审通过；剥离新增后恢复原266行，原同步循环及断言不变。未新增P/Invoke，未读取其它窗口标题；无法复用另一静态类的private GetForegroundWindow，所以不提供该字段，不伪称已采集。

导入前127源raw/canonical/HEAD基线为verifier树`artifacts/pointer-verifier/baseline-ab11.json`，SHA256 `48E0D8EA325656C58E272205C691BA62D173B53D0C1BED828FC7DA7A993AB702`，root逐127复核无差异。专用runner SHA256 `6505241965405FD49E506A0E91F1CD2C1F791110D8D35BBE5F4789D423696BA8`，15962字节、Parse0、双静审；只允许本诊断源diff，其余126原raw/HEAD固定，新树A07原CRLF A872也固定，不为对齐摘要改源码。

root只机械导入冻结单源并核70DF、实际配置E4E33；明确将唯一Build/GUI锁交独立verifier，固定fresh p01的单Build/单1000smoke已开始。没有本阶段测试或根因修复结论；所有进程实际终结后更新结果，原f01/r02失败不倒填。

## p01实际结果

独立Build child51736 exit0，执行77.536秒；正式150/150用时54.33秒、0警告错误。Smoke child16172 exit1/33.665秒，旧重连19/19、完成缓存18/18通过并清理；旧截屏9PASS/1FAIL、10清理。Main346原坐标前置抛错，尚未RaiseEvent，之后节点/自动加载/发布隔离、A07/Main381、原应用smoke/Main382和完整12页均NOT-RUN。本轮完整GUI仍FAIL，不以已到用例通过代替后续。

原报告`artifacts/p/p01/artifacts/sl/ecb11105/screenshot-layout.json`，SHA256 `98392702489C430419B8B60D9C9248C85E14B5A265BF967B832FA0EB063682D7`；进程摘要`artifacts/pointer-verifier/p01/acceptance.json`，SHA256 `A8C976E29675F9622CC81215A3BB676C85D463EB7585F1EFB22423708F8069FC`。DLL SHA256 `49AAD61CC7D3546A420C725065A9101F5A198D221826A53C3CE154FBF4B5187B`。四份raw流字节/SHA完整核对，stderr严格936可读；没有重试、timeout、kill或清理错误，两个child实际terminal/ownedAfter0。五个源快照与live127逐项raw/canonical同，另外126原raw/HEAD固定，A07原A872与实际配置E4E33保持。

### 三快照读数

本轮只到zoom0.1，共3个快照，所有字段无诊断错误。三次Mouse活动源、Captured和DirectlyOver均null，Mouse屏幕回转均(0,0)，Canvas均(-8310,-5280)。目标HwndSource存在，HWND54924962等于本窗口，IsActive/Loaded/Visible/KeyboardWithin均true；因此不能再用IsActive单独替代输入源资格。

- before-set：OS前后均(468,398)。请求屏幕(911,573)直接转Canvas(800,450)，命中Image。
- after-set：OS前后均精确(911,573)，OS直接转换也为Canvas(800,450)，命中同一Image；WPF鼠标位置仍为屏幕零点对应的局部坐标，活动源仍null。
- after-loop：OS前后均(325,352)，直接转换Canvas(-5060,-1760)命中TextBlock；WPF仍原点/无活动源。原循环最后采样另为(330,355)、624毫秒、owned=false；两处不是同一时刻，不能混作原子快照。

这些证据确认了本轮三个采样点的WPF输入源缺失，以及定位后至循环结束发生OS指针变化；小倍率请求点的直接坐标映射正确，SetCursorPos也并非完全无效。它们不能证明是谁移动指针、为何活动源未建立、全624毫秒是否持续同状态、其它倍率或真实手机输入结果。没有依据修改产品坐标算法、暂停用户输入、追加抢焦点/同步/反射或弱化原断言。AP-033仍开放，不把本轮诊断或后续偶然通过当根因修复。

## 主仓导入与限域交付

非实现者实读上述记录并批准仅诊断增强的限域交付；不要求额外重复GUI或用Build-only补绿。root机械导入单文件70DF，源diff只有49行新增，无产品代码变化；未在root再次Build/GUI，不挪用独立150为主仓新执行。

编译输入不能只核src/tools。第一次预检错误按127+11+4计142，检查在File.Copy之前拒绝；实际两树均为143，包含tests的packages.lock.json，即127 src/tools+12 tests+4根配置。原拒绝保留，不当产品失败。纠正计数后完整路径集合相等，143项中59原raw同、84仅LF/CRLF，规范化文本全同；四根配置是Directory.Build.props、global.json、MatrixDevice.slnx、NuGet.Config，无遗漏其它根构建配置。新诊断源raw精确同编译版。该文本核对不等于在主仓重新执行测试或证明环境相同。

主仓`artifacts/pointer-root/import-equivalence-r1.json` SHA256 `430F1F3B5A2BB80DB91A300B2634B2193542C27326DDAD9B35DF23D3389543C7`。末尾首次显示OrderedDictionary摘要时Select-Object输出null，实际JSON已正确序列化；只读重读确认真实值，并未重跑导入。实际配置仍E4E33，原两用户PNG保留。正式交付只包括该测试源、README/阶段报告和三清单，不上传原始证据或修改本机配置，不合并发布；最终版本以Git为准。

## 后续应用页候选

独立L1定点审查应用页第158行在CanManage检查之前清pending：脚本/稳定无进度传输期间可操作目标区并跨过真实350ms tick，待办可能被清掉；终态虽有VM通知，也无法恢复不存在的意图。API请求计数没有释放事件，仅延后清pending也不能保证唤醒。以上未实际复现，不称已修或真实缺陷；自身running因目标区禁用，尚无普通用户反例，不靠直接改禁用行制造产品红。

推荐下一单先以自有一致Demo实际CheckBox、真实timer tick、ScriptScheduler与受控Push TCS复现脚本/传输路径，再按事件驱动合并待办最小修复。保留在线、选择、Runtime、保护及连接身份门禁，不轮询或自动重试失败List。API通知生命周期和小屏固定228+12+最小200正文分别验收，不把首单范围替代完整A07。矩阵包含正常、阻塞/取消/失败、双阻塞、最终选择/清空、离页/关闭、错误不循环和Current/Main隔离；旧A07a改名与原七套保持。
