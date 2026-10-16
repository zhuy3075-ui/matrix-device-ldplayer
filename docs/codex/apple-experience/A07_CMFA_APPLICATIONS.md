# A07：CMFA应用实机验收

日期：2026-10-16。基线`01e5c10a7f14949b1124597479ea0b1a3611753d`。用户提供CMFA APK，继续AP-003与A13b；本页记录首轮单实例安装与启动请求，权限页阻断主页断言的原FAIL保持。后续已另立[本轮owned停止/卸载验证](A07_OWNED_CLEANUP.md)并通过，当前测试安装已移除；完整GUI与双实例仍未过。

## 范围与验收

1. 固定文件SHA、包名与版本，确认专用1/2的实际ABI、启动状态、PID/birth、ADB owner与transport；不从索引公式猜串号。
2. 先精确查同包，包含已卸载但可能保留数据的记录。已有同包不覆盖、不卸载；只有冻结目标下本轮新装的包可纳入清理台账。
3. 当前ApplicationsView实际选择目标、安装按钮/文件选择/确认、任务进度、精确列表版本、更多菜单启停及所选卸载路径都分别测试；直接VM/Backend结果只标对应层级，不换算为GUI通过。安装页的CompletedUnverified不能当精确版本验收。
4. 安装后须PackageManager版本211035/2.11.35.Meta及包路径回读；受控启动须实际PID/活动证据，停止后全部该包进程消失且安装保留，卸载后精确包消失。异常、取消和身份变化另记，不吞失败。
5. 双实例差异化与同步仍单独验，最终版本双实例30分钟在所有实现冻结后完整计时；不以本轮单实例预检替代。

## 已知事实与边界

文件`C:/Users/Administrator/Downloads/cmfa-2.11.35-meta-arm64-v8a-release.apk`为48780036字节，SHA256 `AF7F05D8801798A8E75DE682E5F67070138BECB8623B7A341C3E5CD042A3753D`。官方固定AAPT2两次静态dump均exit0/stderr0：包`com.github.metacubex.clash.meta`，版本2.11.35.Meta/211035，min21/target35/compile35，五个原生库仅arm64-v8a；MainActivityAlias含MAIN/LAUNCHER，但badging无launchable行，设备PM和实际启动仍需验证。META-INF存在不代表密码学验签成功。

这是VPN工具，TunService声明BIND_VPN_SERVICE；不得启用VPN、导入配置/账号、改代理或证书、授敏感权限。原实例0、用户现有应用数据和共享ADB服务不改，不调用外部ADB客户端启动/停止服务。只对重新证明的专用1/2操作；启动2另用现有协调器，并监测雷电自动注册ADB变化，不复制或修改实例。

主仓当前CLI已经fresh仅1绑定只读接入成功：PID75236/exit0/0.596秒，Pixel8/Android14/Ready/transport8，ADB owner62248/birth/path/SHA前后保持，原profile SHA保持。实例1播放器63000/VBox68124，2关闭。原始证据本机`artifacts/apk-cmfa-live/r00`，静态原件在隔离测试树`artifacts/apk-cmfa-inspect`，不上传。

## 推荐路径与失败模式

先给现有正式测试程序增加受限的应用预检模式，复用当前Host后端与身份冻结，不重写ADB或启动第二个完整runtime。新模式只接受专用1/2及MatrixAcceptance名称、固定包/文件摘要；从host-only种子固定实际ADB owner epoch，Create前后及每次读取前后复核manifest进程和连接证明，传入一致的admission防止换PID被新runtime接受。初始化既有getprop/wm/dumpsys读取如实记录，不虚称只发送后续数条shell。

独审明确一个现有SDK边界：Create的admission核Name/VBox PID/birth，但首次Discover不以manifest的transport作原子门禁；同VBox/串号在该窗口重连时，可能先有初始化只读，再被构造后校验拒绝。新模式无安装或写入，仅在后置transport严格一致后才开始显式探测；不宣称整个过程身份变化均零guest读取，也不为本轮测试扩大生产改动。若后续要求构造阶段也零读，须另建受控反例与独立修复。原实例0/其他实例不变也须外层目录快照证明，不由单个目标manifest代替。

更简单的旧LiveAcceptance改Package常量会污染历史抖音用例且缺Player/owner/admission保护，拒绝采用。直接命令行安装更容易验证后端，但不能代替当前GUI批次；仅作为明确标注的分层证据，不能缩小本页验收。

失败模式包括ARM翻译不可用、APK签名拒绝、已有同包/保留数据、目标换代、ADB重注册、别名无法启动、应用请求VPN/权限、命令成功但无实际效果，以及GUI输入源失败。遇风险停对应动作、保留原件，不自动重试或清数据凑通过。

## 当前任务

- P0：实现并独审受限只读预检模式，158旧基线与已有模式不变；用真实实例1获取ABI及同包证据，原始输出/源码与二进制SHA/退出码齐全。
- P0：补齐2的安全接入与两台包基线，再走当前应用页真实路径和独立效果回读；没有可用GUI输入证据时明确保留NOT-RUN，不用反射/合成事件冒称物理点击。
- P1：按实际缺陷最小修复、独立验证、同步报告/清单并单独push；AP-033与其他A07/A08-A16、最终30分钟均继续。

## 只读预检入口离线验证

新增正式测试入口`--application-preflight <absolute-target.json> <absolute-apk> <fresh-absolute-evidence-directory>`，不改变默认158项或历史入口。只接受命名为MatrixAcceptance-的专用实例1/2及固定CMFA摘要；读取实际ABI、启动状态、跨用户同包及保留记录。不执行安装、应用启停、卸载、API或实例启动，也不绕过窗口输入限制。

隔离实施与独立审查后，独立树单次`Build.ps1 -Verify`实际exit0：构建24.96秒、0警告0错误，原158项全部通过（52.21秒），名称及顺序不变。PDB精确核对新夹具、Program及原CatalogRepository源码，146项构建输入前后保持，主配置E4E33保持、执行结束自有进程0。原始输出与汇总在本机`.worktrees/app-preflight-verifier/artifacts/app-preflight-verifier/offline-o01`，summary SHA256 `7520A078D0588CC5BF00EC3D42CE868F6933402D967B043E68BFF4D517B0A61D`。

以上仅证明该o01入口可编译且旧回归保持，不等于APK可安装或当前ApplicationsView已通过。该o01当时主仓尚未整合测试源码，生命周期扩展仍在独立树准备；后续r3与l01另见下节，不与只读入口混淆。

## 实例1实际只读结果

独立`live-p01`正式预检PID1500正常exit0（进程2.829秒，模式内2750ms）；index0输入反例PID72556按预期exit1，未创建host/guest操作轨迹。实例1仍为冻结transport8、播放器63000/VBox68124。七条显式shell均exit0：SDK34、boot=1，ABI列表为`x86_64,arm64-v8a,x86,armeabi-v7a,armeabi`，64位列表为`x86_64,arm64-v8a`，native bridge为`libhoudini.so`；两条`--user all`查询（含`-u`）均为空。它是ARM64 ABI广告与翻译候选，不证明真正ARM CPU或实际安装兼容。

外层runner最终exit1，原因是StrictMode直接访问成功JSON中被默认序列化规则省略的`error`空字段；保留失败，不重跑设备或覆盖日志。正式预检成功与包装汇总失败分别报告。六个子进程均终结，无超时/kill；前后23条目录的采样业务字段一致（排除本地generation），ADB owner、注册目录及主配置保持，自有进程0。采样相同不代表所有实例数据绝对未变，也不证明没有中间变化。

该轮未安装、启动或卸载APK，也未运行GUI。原始模式输出、七条shell、身份轨迹和外层失败都保留在本机`.worktrees/app-preflight-verifier/artifacts/app-preflight-verifier/live-p01`，不上传。后续生命周期入口必须在实际安装调用处再次检查同包、保持APK只读lease，并仅对本轮精确版本/路径已证明的新装包启停或卸载；未验证所有权或身份变化时保留待人工审查，不自动清包。

## 共享批次生命周期入口

同一正式夹具新增独立入口`--application-lifecycle <absolute-target.json> <absolute-apk> <fresh-absolute-evidence-directory>`。只读入口固定`lifecycle:false`，输入不能开启写。生命周期复用当前应用页同一ApplicationBatch，不更改生产代码；薄转接仅允许固定目标的List/Install/Start/Stop/Uninstall，实际写调用前重新核身份、全用户同包/保留记录或本轮owned版本/路径。全程固定APK只读lease，安装只尝试一次；精确版本、非系统属性与PM路径回读后才登记owned。异常停后续操作并保存ledger，不在finally自动清理Android。

共享批次真实读取所有已安装/系统应用及逐包版本，不能称只读取固定CMFA。Install/Uninstall原后端成功stdout不暴露，保留Task完成及独立效果证据，不伪造原始Success文本。600秒为内部取消请求，不是严格墙钟：已在途写仍按原语义观察，每个底层命令60秒；外层900秒有界，超时只终结本轮子进程并留下状态不确定台账，不自动重发。PM前置观察不是能锁住其他安装器的原子包锁，执行期间假定专用实例没有并行同包修改；它也不是签名或来源信任证明。

r2独审发现前台正则的单词边界会把`evil.com.github.metacubex.clash.meta`误当本包，纯本地反例为真；r3只收紧左边界并将权限措辞限定为没有执行授权操作，未改生产、等待或原断言。独立5个本地正则用例符合预期，原r2与差异保留。最终源码SHA256：

- `ApplicationPreflightAcceptance.cs`：`CAD755F1055C90734392AFDD49B99BC35EF171AF031A649B107CBD86C06D1360`。
- `Program.cs`：`993914C9ADABC4C49BBF166AA409F5CD2E57768E4862689BEEA2BEAF08ED8D5D`，仅追加两个入口共12行，原158项与默认路径不变。

独立offline-o02实际exit0：构建3.88秒、0警告错误、158/158（51.16秒），summary SHA `86CEEB87E655585296C9447C7A8407A1C6CE1898B9E8287892C26A3336FE5046`。主仓机械整合同源后单次Build -Verify PID17524/exit0：构建20.63秒、0警告错误、158/158（51.43秒）；名称顺序逐项相同，物理142个src/tools/tests输入加4根配置等于146，无额外源，前后同。两边PDB均对应上述两源及原CatalogRepository；配置不变、自有进程0。主仓summary SHA `ACC78A0FF600A2928B321D4AC699DD002F6AB723E34F95D519487CCDFF0FAC9A`，本机`artifacts/apk-cmfa-live/main-offline-o01`。

## 单实例安装首验与真实阻断

独立lifecycle-l01只执行一次，正式PID72468/21.480秒、exit1，外层同exit1；没有超时、kill或重试。37步骤通过，主页效果断言FAIL：

| 用例 | 实际结果 | 判定 |
|---|---|---|
| 安装前保护 | 全用户及含保留记录均缺包，身份/摘要/lease成立 | PASS，非原子并发锁 |
| 真实共享安装 | Install一次，批次CompletedUnverified；之后独立核211035/2.11.35.Meta与唯一非系统base.apk路径，取得owned | PASS安装效果，不等于独立apksig验签或来源信任 |
| 共享应用列表 | 完整List共6次，精确包与版本回读 | PASS共享层，非GUI行/进度条证明 |
| 发起启动 | StartApp一次，Status: ok；精确主进程4564及background4590存在 | 请求/进程成立，不能外推主页成功 |
| 前台主页 | topResumedActivity与ResumedActivity都是PermissionController GrantPermissionsActivity；CMFA MainActivityAlias为PAUSED | FAIL，首次运行权限请求页实际挡住；不是正则假红，不归产品Start缺陷 |
| 截图、停止、卸载 | 原l01流程在主页断言后停止，均0次 | l01仍NOT-RUN，不倒填后续独立用例结果 |

没有点击授权、VPN、配置导入、代理修改或权限grant命令；不能从通用GrantPermissionsActivity推断具体请求了哪种权限，也不声称系统没有自动授予普通权限。应用自身联网未控制或验证。

l01结束时安装保留在专用实例1，原台账`owned=true`、`PENDING-MANUAL-REVIEW-NO-AUTOMATIC-CLEANUP`。该历史台账未覆盖；后续通过实时重核后的[独立c01停止/卸载](A07_OWNED_CLEANUP.md)，当前安装已移除，新ledger为UNINSTALL-VERIFIED/owned=false。不重跑完整安装模式、不倒填本次FAIL、不放宽主页断言或延长等待凑绿。当前Windows Computer Use抓取两次超时后已停止全部Windows输入，不用Win32/UIA/SendInput绕过；当前ApplicationsView实际选择、文件框、确认、进度和菜单仍未验。

五个子进程全终结、自有测试进程0；23实例目录采样字段前后相同（除本地generation）、ADB owner62248/birth/path/hash及transport8、146输入与主配置保持；不是所有实例数据或中间变化的绝对证明。原始步骤、PM、活动、批次进度和ledger在本机`.worktrees/app-preflight-verifier/artifacts/app-preflight-verifier/lifecycle-l01`，均不上传。正式summary SHA `C7F88DD881147EBC8AF58810E68A4D03E6D472584FB75645B4E472EDC74E5CA2`；ledger SHA `0D815ED2C25D0E2388B224EE1A49FB704D473E1982ACB9FACFB9B5B500B19AE6`；独立audit SHA `85BCAC09665F7692DC87E82AA02CBDBD10D0C5F10326C6099E8FFCFCA93007EF`。

AP-003仍开放，AP-036跟踪首次权限页前置；单实例共享停止/卸载只由后续独立c01证明，实例2、双实例同步、实际应用GUI及最终版本双实例30分钟不得由本轮关闭。提交仅正式测试及脱敏报告/清单，不包含APK、target/ledger原件、本机配置、依赖或原始证据，不合并发布。
