# A06d 原生保存框与截图保存意图

## 背景和范围

基线 `1acbf23b149047dd46a132660d417f31f5613812` 已实际推送，A06c2交付记录在本机 `artifacts/reconnect-root/push-complete-r1.json`。上一目标轮是有效进展，不重新打包或重跑已结束的旧交付。

本阶段仅验证并修复截屏页“保存 PNG / 保存裁剪”的原生保存路径。保留12页结构、完成缓存身份守卫、精确Offline历史、本机配置和全部设备权限边界。不操作真实模拟器、账号、共享ADB、系统代理/证书，不删除用户文件或旧worktree。JSON导出、未完成拖拽/失捕获和A07至A12继续开放，不把本阶段替代完整目标。

## 已知事实与根因候选

- `ScreenshotView.SaveImageAsync` 开框前只检查crop非空，原生框返回后才读取实时crop并调用VM。
- `MainWindow.RefreshScreens` 在合法DebugImage变化时清空crop/cropStart；原生保存框有嵌套消息循环，之前接纳的截图可能在框内完成。
- `WorkspaceViewModel.SaveScreenshotAsync` 本身已在文件写入await前完成PNG编码，因此“写盘await后才读图”不是现有缺口。
- VM的通用ExecuteAsync遇Busy直接返回。已合法接纳的保存不能在确认后因另一设备任务Busy而静默消失，也不能在整个原生框期间锁住工作区。

这些是源码事实与候选路径；首次实际红测之前不称已复现。

机制参考：[WPF嵌套消息处理](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/threading-model#nested-pumping)、[CommonDialog.ShowDialog的Owner与返回值](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.commondialog.showdialog?view=windowsdesktop-10.0)、[IsWindowEnabled的只读输入资格检查](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-iswindowenabled)。官方机制不替代本机实际按钮、原生框和PNG证据。

## 最小合同

1. 点击保存时，先按现有完成缓存资格核验Owner、当前行与Catalog，固定本次图像和裁剪区域并编码PNG；不能只固定矩形而重新读取晚到图像。
2. 原生保存框打开后，合法新帧、目标或身份变化可以正常更新界面；确认仍保存开框时已合法接纳的原图/原区域，不漂到新图或退化整图，不默认取消合法保存。
3. 取消不创建输出。开框前无截图、无裁剪或身份已失效，明确拒绝而不提供保存旧内容的旁路。
4. 现有精确同源Offline历史仍允许本地保存，不扩大在线读、输入或应用操作权限。
5. 已接纳PNG的本地写入不重新经过会Busy静默返回的通用入口；写盘异常必须反馈，不能记录假成功，也不能覆盖另一正在运行任务的状态。
6. 不持全局Busy跨越保存框；本次异步写入要有单次按钮执行与关闭/清理约束，不能退出后遗留未排空任务。
7. 公共SaveScreenshotAsync原签名及消费校验保持；内部准备能力只接受裁剪参数、不接受外来Bitmap/bytes，只有VM的合法完成缓存能生成封闭保存动作。
8. 保存不新增设备读取，不修改Main、勾选意图、授权或原缓存内容；不能把已保存旧快照重新放回当前检查结果。

## 方案与取舍

推荐提取已有校验和PNG编码为私有函数，由内部准备方法生成只接受文件路径的封闭动作；界面在ShowDialog之前取得动作，确认后仅写该内容。公共直接保存复用同一校验编码，不改变旧调用者语义。关闭约束和局部反馈需在编码前静审，不为本项重构通用Busy或全部命令。

更简单方案是检测漂移后取消保存，但它会减少已合法接纳的操作能力，仅作备选，不作为默认实现。拒绝冻结整个工作区、暂停所有后台任务、降低c1身份守卫、允许任意旧图导出或用假保存框替代主验收。若实际证据证明没有框内合法换帧，应修正候选而不是编造红测。

风险包括原生框的嵌套调度、同尺寸不同像素、Busy静默丢保存、文件错误后假成功、关闭时本地任务未排空，以及测试侧错误地用直接VM保存代替实际按钮。最多三轮生产review-fix；保留每轮失败与环境前置，不倒填首次通过。

## 测试与执行顺序

| 优先级 | 路径 | 足够证据 |
|---|---|---|
| P0 | 裁剪正控与框内合法晚到帧 | 自有Demo真实按钮/原生框、原裁剪与新帧/清crop时间序，磁盘PNG尺寸和全部像素等于原裁剪而非新整图 |
| P0 | 保存整图与框内换目标/身份 | 实际原生框确认后仍输出已接纳原内容；当前UI正常更新，旧源不回灌 |
| P0 | 取消 | 实际取消按钮，指定独立输出不存在，任务/框清理完成 |
| P0 | 开框前Catalog-only过期 | 实际消费被拒绝，没有新保存框或错误产物，不改原六套守卫测试 |
| P1 | 精确Offline历史 | 实际导出合法原内容，无新增截图/节点设备读 |
| P1 | Busy期间确认与写盘错误 | 真实受控设备任务仍在运行，保存不静默丢失；错误明确、没有假成功；无任务状态误覆盖 |
| P1 | 回归与交付 | 正式150项、原19/18/10/11/16/49、相关尺寸与清理；独立审查、README/报告/清单后单独push原私有功能分支 |

使用三个新隔离worktree `apple-native-save`、`-tests`、`-verifier`，都基于1acbf23。Windows实际交互只使用已安装computer-use的sky JS；已读取完整技能、安全策略与API，并实际初始化/列出窗口。不得编写PowerShell/UIA/Win32点击驱动。仅自有Demo一致Backend/Catalog/Runtime及artifacts产物，阶段信号不得变为普通程序的权限或runtime替换入口。原六套正式夹具保持字节不变。

## 准备状态（首次运行前）

2026-10-16：完成当前状态核对、源码事实分析、两个只读设计交叉审查与三个隔离树建立。r0在独立测试树仅新增正式单例夹具和App严格验收模式，普通smoke不等待交互；尚未修改生产保存逻辑、构建、打开原生保存框或取得本阶段红/绿证据。Build/GUI执行锁root空闲。真实专用1/2、全部后续页面及30分钟稳定性仍未完成，完整目标active。

r0累计补丁SHA256 `6E9872973A06E21775138818B0039607C8C4B1BEAC53454292F370E26BF7FD04`；新夹具30837字节/224行、SHA256 `AB46FE0983D02B27188ED47BE9AA1A5FCEAC353809E6F009DC0CC9E437C39531`；App验收接线原CRLF为16271字节、SHA256 `244C6BDF1A7E3E1DD25634468E42F553990CF650CFDAD71212F052CFEB599999`。原六套raw/canonical不变。root已读完整夹具、App增量、冻结清单与协议，独立审查仍待结果。

预定首例仅crop-late-frame：由root通过sky真实选择裁剪、拖动、点击保存、确认原生框；nonce信号只释放已接纳F1，不接受任意指令/路径。信号由根写同目录独立临时文件后原子发布，避免File.Exists早于内容完成的伪失败。三个交互阶段各90秒、自有排空5秒、残留模态取消30秒；观察超时不重启进程。r0即使单例通过也明确PARTIAL/exit1，其余合同NOT-RUN。后续局部保存互斥须同时覆盖两按钮及closing/closeApproved，反馈优先页面内独立结果，不侵占其他设备任务Status/Error；尚未编码实现。

## 首次实际执行：原生交互前置失败

独立verifier机械导入r0，两源raw核同；新夹具gitapply产生的CRLF版本先全文归一比较，再仅该新文件对齐冻结LF，不修改旧六套。首次Build子进程73.4221064秒、exit0，0警告/0错误；正式150/150用时51.30秒。DLL SHA256 `25F2D6F08311C99A68938A5A36602EB30331F4A4804F347FBE19AA172E3C1F77`，Build process记录SHA256 `10AEDDAD6E5A5457A8789D908900472F42E5FD729FAA37E9EBB4F3C61BB515C9`。实际127源、旧六套raw/canonical与配置E4E33前后不变；Build exec12857实际结束后归锁，GUI此前未启动。

root单次启动自有r0（exec25625、PID27696、artifacts/n01），sky实际列出唯一测试窗口。首次get_window_state报`FrameArrived timed out: timed out waiting on channel`；依技能刷新窗口列表、重选同一窗口、get_window/activate后仅重试一次，再报`window capture timed out: timed out waiting on channel`。没有可用画面，按恢复规则停止该case的所有交互，没有编写UIA/Win32输入旁路或重启进程。

同一子进程91.5200158秒后正常exit1，无外层330秒超时、无kill，原90秒ready-for-crop等待真实超时。原报告保留FAIL/verified=false/cleaned=true：Down、Up、保存Click均0；仅初始化Discover、Capture的Discover/F0 Screenshot，原Task2均RanToCompletion；F1、裁剪、原生框、信号与保存产物均未到。该结果归类`PRECONDITION-FAILED-NATIVE-INPUT-NOT-RUN`，不是保存产品红，更不能填原生保存通过。

原报告SHA256 `4A5E741F571FAB2F6DAE4141ECD1E54EA35226938D8D6858019F83ECD936276E`；root工具前置记录`artifacts/native-save-root/baseline-r0/computer-use-precondition.json` SHA256 `D124A6132595458B0DBE5D03F02BBDA279886914750ECB98995798E3E1BFB3DD`；清理/源检查记录`post-native-integrity.json` SHA256 `6EC3123992114EF89AE672C5DF1DA97908D48535EA9323408F232D4121414E0D`。root重读原JSON并核Build的完整127源集合/旧六canonical/实际配置仍相同、PID27696剩余0。supervisor窄集合实际102个src cs/xaml，不冒称它独自覆盖127；完整集合另按Build postflight逐项重核。

本阶段未修改生产保存逻辑，未整合新验收入口到主仓、未提交未验修复。完整目标active：当前仅原生交互路径受工具前置阻塞；可单独做accessibility-only只读诊断（不据其坐标拖画布、不冒称画面恢复），并继续应用管理等不依赖原生文件框的受控验证。A07只读审查已发现名称进入目标record相等、活动阻塞可能丢自动读取pending和小屏固定栏三个候选，均未红测，不称产品缺陷。下一项优先取证同一身份仅改名后的应用行操作；不以这些替代未完成的原生保存合同。

补充日志边界：root实读原生supervisor的`stderr.txt`发现142个U+FFFD替换字符；这是默认UTF8解码后的文本，不是原始stderr字节，不用它还原中文错误。上述原始失败以完整`native-save.json`为准，旧文本保留不覆盖；以后同类runner应直接保存原始流字节，再严格识别编码。

## CLI拒绝边界补验

同一冻结DLL另测严格入口拒绝，不执行有效GUI用例。首r0 runner `1E9FCF46...38B2B` 在missing-demo一例即停止：child正常exit1/0.204秒、目录未创建，实际ArgumentException正确；但runner把CP936原字节按replacement UTF8解码，中文匹配false，原FAIL摘要`1CB70AFB...6285C`保留，余9例当轮NOT-RUN。root实读434字节stderr（SHA256 `2A584263DE01E904D15CCE662A9D58B64A378437DBC2161C5994C16DA4A9BDC2`），严格UTF8抛DecoderFallback、严格936得到原预期中文。root首条只读诊断命令曾因catch缺空格而ParserError，未执行状态操作；纠正后只读验证，不重跑该child或倒填首轮。

独立-r1 runner SHA256 `16123CDA2328293B23764071AD65784D49781426516688C6327B44FF61035261`，真实增量仅严格UTF8、仅DecoderFallback时严格936与解码器记录；10条argv、断言、等待、源门禁和失败即停逐字不改。root读原完整脚本及增量后批准新轮一次执行；旧runner/raw/FAIL不动。

新轮实际10/10 `PASS-CLI-REJECTION`：缺demo、缺data、重复demo、重复mode、未知参数、mixed-smoke、相对目录、artifacts外目录、已有本轮profile、data缺值。每个child均exit1/ArgumentException/指定中文片段匹配、严格936（gb2312）、无kill；各约0.189至0.216秒，整轮3.79秒、runner exit0。所有应不存在的目录仍absent，n01仅作为拒绝argv与目录存在判断，不读取其内容；127源/旧六raw及canonical/DLL/配置前后同、残留0。

实际摘要在tests树`artifacts/native-save-cli/run-20261016-040104-3a113fe2/summary.json`，SHA256 `25A4E1EB1F6F44B07F71E12D76E8CEC81A65C4238BD4EC36C33E9EA945F9E115`。root已逐例重读真实stderr原字节并严格解码、核异常和路径无副作用，并比较前后完整127源与旧六。该结论只属于新测试入口拒绝边界，不能换算为原生保存/裁剪、物理输入或产品修复通过；所有执行进程已实际结束。
