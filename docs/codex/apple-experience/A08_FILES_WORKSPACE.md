# A08e 文件工作区

日期：2026-10-17。起点为已实际推送并获提交后独审的`1f397cfcfb8cb640bb3b450014744384b4c3dfb8`。本节继续原A08/R04，不关闭真实模拟器、完整GUI或最终双实例30分钟。

## 目标与边界

保留文件页原路径编辑、目录查看、上传后安装开关、添加上传/下载、开始/停止、进度条及原七列；不新增重复入口或删除有效信息。先复现AP073/074，再检查原控件六尺寸、实际队列正常/异常及目标语义。所有未执行项保持未验，源码疑点不先算产品红。

本轮自有WPF正式夹具使用真实Demo文件读写及原VM方法/命令。真实1/2需后继严格入口和当时身份核定，不能直接运行含旧抖音前置的LiveUiAcceptance或CLI整套。原生picker、目录查看的owned非模态窗口、物理输入、DPI、Android、任意重连交错另验；不恢复已停止的官方捕获/错误几何分支，不触碰实例0、账号、权限、VPN/代理/证书或共享ADB。

## 第一性原理与方案

已知`OverallProgress`是行进度平均值，QueuePush/Pull只刷新命令；`TransferRow.Size`根据本地文件读取，下载后只发布Progress/State。根风险是依赖值改变后没有通知原绑定，不是后台没传完或需要用户手动刷新。必须先实证原绑定与实际getter/文件是否分离。

推荐在原数据改变位置发布现有依赖属性，保留队列、平均算法、文件大小格式及完整错误/取消语义。更简单的切页重绑/刷新表格会掩盖状态问题并破坏选中与滚动；不采用。更大的传输框架、轮询本地文件或新建冗余进度模型没有必要。现有Toolbar已按实际宽度收紧子控件，不能仅凭路径Width440判定布局溢出；只按六尺寸实际证据局部调整。

失败模式包括异步Progress回调尚未排空、UI尚未实现Size单元格、虚拟化回收/重绑导致假绿，以及取消请求被误记为任务已结束。测试需确认同一原控件身份和Dispatcher正常回流，保持任务/写入拥有权直到实际终态。

QueuePull方法本身只取Current，原UI下载按钮另有CanWriteDevice门禁；不能把两层混淆。Start先固定pending并整组Acquire，再逐行ScriptBusy及失败继续，不臆造原本不存在的全批脚本/离线原子拒绝。查看目录允许在Transferring时只读，不要求所有按钮一起禁用；停止按钮必须通过真实在途任务变为可用。

## 任务与验收

| 优先级/阶段 | 工作 | 验证与完成标志 |
|---|---|---|
| P0 / 正式旧红 | 隔离UI/测试worktree；原235断言与旧32 Smoke不改，严格Demo fresh入口 | 原生产经实际传输完成100后再用原方法新增0行，分别比较同一ProgressBar与getter；不用手设属性或强制UpdateTarget |
| P0 / 下载显示 | 不存在和已有不同大小的本地目标；固定原已实现Size cell | 原QueuePull/Start命令后实际bytes/SHA一致，Size getter及同一cell即时一致；禁止Items.Refresh/切页/回收重绑 |
| P1 / 六尺寸 | 390×640、390×844、768×1024、1000×700、1440×900、1920×1080 | 实际尺寸、路径首尾/长文本、原动作44、七列横滚可达、队列视口和图像；非物理触控/DPI证明 |
| P0 / 原业务 | QueuePush按Targets，QueuePull按Current；入队快照与开始时pending保持 | 实际Demo文件字节回读，目标差异/切换、空/失败/重试/取消、脚本与共同Writes门禁、错误可见及释放后恢复 |
| P0 / 复验与交付 | 最小生产改动，同夹具旧红/候选绿，独立审查和主仓复验 | 源/PDB/本轮冻结产物一致、唯一清理无残留；README/记录/清单及单功能普通push原私有feature，不合并发布 |

队列行没有generation/transport字段；本阶段不得声称验证了排队后重连的身份接管合同。固定同一Runtime且禁止重连，变化场景仍须单独定义/验证，不能沿用旧transport授信。

## 执行前状态（历史）

- 上一轮确有进展：文本功能14文件已正常推`1f397cf`，远端feature同值/test49e471保持；回执`artifacts/text-workspace/push-complete.json`摘要085CAF5F…ED47及源包已独审。原文本各轮测试不重跑。
- 新建`files-workspace-ui`与`files-workspace-tests`两个detached worktree，均从1f397cf；未清理旧worktree或覆盖用户文件。主仓开始时tracked/index干净，仅原两英文PNG未跟踪。
- AP073/074仍是源码风险，尚未运行复现。正式夹具与执行前独审先行，不先改生产、写通过或启动任何模拟器。
- 只读主机复核已实际完成：批准的官方console仅`list2`一次，单child自然0/双原流排空/retained0；23行中专用1/2的AndroidStarted为true，原0与其它行关闭，共享5037原生身份前后相同。首工具展示误用UTF8解码名字，原raw未变，另按CP936解析完整目录，未重跑命令。此观察不含ADB协议/guest读取、实际Ready transport或页面验收；没有启动/关闭模拟器。
- 主机证据在tests树`artifacts/files-workspace/host-readonly-r01`，execution SHA256 `ECD0689D9D106E671C435EB0C6270F1B20956CE945791E4C20809D3618DDCDC5`。主仓194输入/255产物/用户3及17份历史proof前后核对保持；独立审查仍需实际核原件，不由本段自述代替。
- host原件/CP936派生及保护已获独立只读核对。另发现fresh Git checkout与main raw行尾不同，原baseline不能借给tests；保留原baseline，另从同HEAD的pristine UI树冻结194原raw并逐项验证clean-filter blob等于HEAD，tests除授权App外193原raw均同。侧基线`tests-base-r01.json`摘要3B7D4EA7…2C05A；不格式化源码、改Git设置或用normalized文本替代实际PDB/raw证明。
- 随后原tracked `Get-AdbHostState.ps1`仅执行一次host version/devices-l；原生owner前后复核相同，未使用CIM的截断时间授信。协议0029，按现有明确授权绑定观察专用1/2注册为device且transport唯一；guest命令0。原件`host-registered-r01/execution.json`摘要D1AF952B…5D9EC6待独审；这不包含VBox/TCP准入或实际guest文件操作，更不是当前页面通过。下一次实机操作仍需重新采样，不能永久复用本次注册。

## 正式夹具冻结

上述主机注册原件已获独立实际审查，仍只认可host层。正式测试作者仅在tests树修改App和新FilesWorkspaceSmoke，未改生产、原235断言或32旧Smoke，未运行任何业务case/严格拒参/单元或MCP。

- 最终15组：六尺寸、上传/下载新增行进度各一、缺失/不同旧大小下载各一、Current/Targets与pending快照、取消真实在途任务并重试、真实missing文件失败恢复、空/脚本/共同Writes门禁、缩放导航身份。使用实际Demo文件、原命令与原Size cell，原按钮状态不手改。
- 静审补强了原夹具Identity：必须从当前实际页面重枚举五按钮，检查职责名集合/数量/原顺序引用和当前Command，不能仅对已保存数组自比。另将NOTRUN里的目录窗口术语改成owned非模态；原业务/几何/阈值/清理断言保持。三次仅编译及中间源码/DLL/PDB原件均保留，不能当产品红或通过。
- 最终compile-r03自然0/14.450秒，8条既有警告、0错误，两测试源实际PDB摘要相同；原始双流排空，无timeout/kill/清理错。三编译session98801/27275/23687全部终态，作者释放编译owner并停止编辑。
- 冻结清单`artifacts/files-workspace/frozen-r01/manifest.json` SHA256 `861BC82DDABC7B23BBEFD415C9BDB0029302ADCC7C9F376215AD83517B599357`；App `2A3117DB9211559406CB97EC9073C7883E6E919034374FF5C3C023022934C1EA`/28715B，新Smoke `DFBB55E489E9F15B92AA1112B6A3773522033D9F5DE418F42C1EAAA218FD78BD`/61079B。独立静审暂无剩余源码阻断，固定Red执行脚本/输入契约仍为下一准入门。

所有15组、原235/MCP、严格入口与文件操作仍NOT-RUN。主仓未导入测试或生产修改；先完成固定旧红，真实原件无夹具/前置/清理/报告失败后才做最小生产改动，不提前构造绿测。

## 首次旧生产对照

以上NOT-RUN为执行前状态；固定runner `5920E906…DEE297`/契约`4989F14F…20D21E`经静审及root全文复核后，唯一Red-r01/session77311已自然outer0。Build child49244 exit0/93865ms，原235名序全过，MCP14 PASS/13 NOT-RUN，三严格拒参/实际四源PDB通过；专项child18288 exit1/3432ms。outer0只表示有效旧红执行完成，不是产品通过。

- 实际15组为5PASS/10纯PRODUCT，314检查/806登记Task均终态；15唯一Shutdown/Closed、窗口/retained0，error/postflight为空，没有夹具、前置、报告或清理失败。五业务组的目标/pending快照、取消等待与重试、真实失败恢复、脚本/共同Writes/空门禁、导航身份通过。
- AP073两组原getter已50，同一bar100，Overall通知0；先真实完成100且backend/Dispatcher回流排空，新增原0行后才观察。AP074两组真实文件bytes/SHA和getter32.0KiB正确，同一已实现cell仍为待下载/1.0KiB，监视的row/cell/绑定未变。原始失败不能靠强刷或切页修饰。
- 六几何组五原按钮32高、安装开关24高，Name/RemotePath长值未完整显示且无提示；原路径首尾、原七列自然横滚与其它几何已通过，不按Width440增加外层布局。最小390实际截图已查看，属于自有client渲染，非物理Windows捕获或手机画面。AP077记录实际几何问题。
- 原件在tests树`artifacts/files-workspace/red-r01`：execution `D602285AD8F8AF8C92E4BFE55D798ABF367F7DEC5AC8A8CE37A97FEA079D76BF`，after `733AFE16D209A10B29509A4FB729E34F8A22139A1D17060B5D37093D7D85F60C`，专项报告 `CC671B1F834747A165A66A21BA9FEADA1FF423E13A450CFBD2BD9E5CAE8F1661`。实际原流/PDB/冻结产物与保护独审进行中，未改生产或集成主仓，不重跑这轮。

## 首次候选

旧红的原流/四PDB/195输入/255本轮冻结、主仓194/255/用户3/17proof及全部15业务/六图均获非实现者实际终审。五child/十raw自然终态，MCP14会话/42raw也已实核；没有把host注册或编译成功换算成业务通过。

root才在UI树修改两个生产文件；以下为候选执行前冻结的改动：

- VM仅在QueuePush/Pull新增行后、RefreshCommands前发布OverallProgress；成功Pull返回后对原row发布Size。平均算法、原目标、pending快照、门禁、错误/取消/安装和大小格式保持，不修改Presentation或新增框架。
- FilesView原5按钮/安装开关/路径框局部44，原DataGrid行/头44；七列沿原ElementStyle增加原Binding的全文提示和省略标记。Progress原列60扩为96，旧390与1920图均有“进度（%）”裁字证据；标题单位/列顺序/绑定不变，这项仍需候选实际图复核，不冒称已有机器断言覆盖标题。
- 新候选VM SHA256 `21DCC7558F15F22072C2EE35695D2D010DEE200129081A52CF1563BCCB626981`/72581B，FilesView `0E27AEDF94340159400BA39867E3A45EC9C3A69C622DDEDE3BCB330443A0CE7C`/4731B；diff仅两源19增/3删且check0。没有修改路径Width440、原WrapPanel、共享主题、七列/命令对象或已冻结测试断言。实际静审及固定Green为下一门。

## 同夹具候选复验

两源候选获独立静审后，root先保存旧生产及新候选原件，再精确复制到tests并提高mtime。App/Smoke仍为同一2A31/DFBB冻结输入，原15组、阈值、断言和清理合同均未改变。

- 首次机械生成Green runner误将PowerShell dollar-apostrophe交给JavaScript replacement展开，Parser在执行前拒绝。无效原件及失败记录`preflight-green-r01/green-runner-preflight-r01-failure.json`（SHA256 `27B3CFD7997A0777761A888223F94F69977E690EB00CF1E5554A97AF54F13161`）保留；仅改literal replacement callback，从未执行该无效脚本，不记作产品红或一次真实Green。
- 最终fixed Green runner `AAA4CC4869DE3D14313F73485E0340A942143B684987879ED4022FE08E510612`/契约`59EA85DD90977553A943C0C1C086F96E3F3EFBE0949A72451ECBD52141B82D75`经全文delta复核后唯一执行。session14440自然0，Build child8744/104044ms、专项42428/4460ms；原235名序、MCP14 PASS/13 NOT-RUN、三严格拒参及四源实际PDB通过。
- 15组全部PASS、326检查/842登记Task终态、15唯一Shutdown/Closed、窗口/retained0；error/postflight为空。比Red多12检查/36任务来自同夹具在实际存在的12个原全文Tooltip上继续检查与Popup settle，并非修改断言或放宽标准。
- AP073两组同原bar/getter均50，Overall通知1；AP074真实32769B/SHA正确，原已实现cell即时32.0KiB，原row/绑定保持且Unloaded/DataContextChanges为0。六图逐张独立查看，最小窗口仍有两行以上队列、44命中和完整进度单位；12个长值原提示实际可读。
- 原件位于tests树`artifacts/files-workspace/green-r01`：execution `85BCE6FECDFD3C24607E86E1848FADA1E4662777F41D148B5C4304FF669542DC`，after `475033051EA58671827B1BB435DEA5ACA26A1A0325F00282C4E84D592064052B`，专项报告 `897532877F5F14DF501CC349923F5E61537242B615E0DDF7F19CA09DE998A137`。独立终审已核实际原流、六图、四PDB、冻结195输入/255产物、主仓194原输入/旧255、用户3/17历史proof及旧Red保持，准许主仓有限代表复验。

## 主仓集成

root将获准的App/Smoke/VM/FilesView四源按raw摘要精确集成，先保留主仓旧三源，再提高新文件mtime；其余191原构建输入、旧255产物及用户3/17proof在首Build前保持。导入记录`artifacts/files-workspace/import-r01/copy.json`，不格式化其它文件或改变Git行尾配置。

Main-only runner `53D66FFF34301EDBB7905BC1FF7D2F3B71DE59F5161BCFAD10B82F5670BBD635`/契约`9D78C18902686257720A2C079026BCDDDB874F0FFEF9336F92031C0E20C44CB7`经全文读取及相对Green差异复核，唯一Main-r01/session44853已自然0。它按主仓原raw基线覆盖精确四源，不借tests的CRLF原件；旧255仅在编译前核一次，后续按本轮新255及冻结副本复核。tests Green的195/255、旧Red/Green三份报告和用户保护始终固定。

- Build child35308自然0/115654ms，专项child85912自然0/4513ms；原235名称/顺序全部通过，MCP14 PASS/13 NOT-RUN、三CP936精确拒参及四源实际PDB通过。五直接child/十原流自然终态且排空，无timeout/kill/清理错误；预期严格拒参的exit1不算残留。
- 主仓15PASS/326检查/842登记Task终态，15唯一Shutdown/Closed、窗口/retained0、error/postflight为空。原绑定即时更新及实际Demo字节回读通过；root另看实际390最小图。正式原件`artifacts/files-workspace/main-r01`：execution `AFE4657D3FE84B60CFD60C98F4728D05A72BC17C072282A134264945F15A1B8C`，after `E95F2F64997E5F1C7495C4BE31A6ED00E9F5FD89544CB6CE1CF29A842E2FB224`，专项报告 `C1C0DE57BAC54C627BD0DB626B7DC5418F301AC604F01BF0F1ED8C656F185F42`。
- 全部必需exec已终态，不再Build或重跑旧红/候选。主仓实际原流/六图/PDB/源与保护已获非实现者实际终审，无拒点；当前195输入/本轮255及冻结、tests Green195/255、其余191原输入、旧32Smoke、用户3/17proof和旧Red/Green原件保持。之后仅正式文档、完整核包及index审，再同私有feature单功能普通提交推送。实际交付状态由Git及新`artifacts/files-workspace/push-complete.json`确定，不预写push成功。

## 验证方式与未验项

构建沿用`tools/Build.ps1 -Verify`及现有SDK，不安装依赖或执行真实ADB客户端。专项入口为：

```powershell
../.tools/dotnet/dotnet.exe ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --verify-files-workspace --demo --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY
```

目录必须是本工程artifacts下全新绝对路径；入口拒绝缺少Demo、混用其它模式、额外截图/尺寸及未知或重复参数，拒绝发生在配置/runtime建立之前。正式runner另核原235、MCP、原始输出、源码/PDB、任务和冻结产物，单独命令不等于全部交付门。

专项十项NOT-RUN保持：物理GUI/input、真实DPI、原生上传/下载picker、目录owned非模态窗口、APK安装、实际API在途传输冲突、真实模拟器/ADB、generation/transport重连接管、任意文件I/O交错、最终双实例30分钟。FILE-01/02完整实机条目不关闭。源码包继承的82份tracked验收材料分类仍属AP021，不声称源包已隐私净化；本轮raw、配置、APK/依赖/二进制不上传，不清理旧证据/worktree。

## 交付静态检查例外

精确14路径首次暂存后，`git diff --cached --check`报告唯一诊断：新`FilesWorkspaceSmoke.cs:546: new blank line at EOF.`。首次脚本throw产生的外层exit1曾被误写为内层git退出码，原记录`artifacts/files-workspace/staged-lint-r01-failure.json` SHA256 `360214F51B129E85A64B290556582237C61829EB3119A3750DC639A1623D694B`保留但不作为内层退出码证据；直接只读复核实际git为exit2、一条相同诊断。此前未暂存的`git diff --check`不含新文件，也不能扩为全体lint通过。

非实现者独立重核同一唯一诊断、14条index及冻结DFBB源，批准仅这一无C#语义影响的EOF空行作为有限lint例外；保留与实际测试/PDB完全相同的正式夹具，不为格式改源或重跑业务。r02交付守卫因错误预期内层git1而拒绝，属于静态编排口径错误；原包/计划保留，澄清见`artifacts/files-workspace/staged-lint-r02-clarification.json`。最终r03只将守卫固定为实测exit2且必须恰为该一条诊断，不接受任意非零，也不报告staged check0或全部lint通过。仅重生成受此说明影响的文档清单/包，实际Main/Red/Green与源码保持；包/index/commit仍须独立交付审。

## 后继实机前置

现有代码没有可直接安全执行的窄终端/原文件队列入口。后继复用LiveDualSync的Ready/noLaunch、admission、原生5037 epoch/进程出生和实际唯一transport证明，冻结完整目录及精确1/2；无服务、非Ready或身份变化即停，不主动重启服务或模拟器。仅原VM队列/命令操作本轮唯一临时路径，固定只读Shell并按实际字节回读，清理也必须证明本轮路径归属。该实机层和物理GUI/原生picker不能互相替代。
