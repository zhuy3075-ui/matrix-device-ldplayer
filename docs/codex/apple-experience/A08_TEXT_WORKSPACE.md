# A08d 文本工作区

日期：2026-10-17。起点为已推送 `c63b0e9af17bcca0d0f1740a114ba930dcec964f`。完整目标继续 active；本节承接 A08，文件队列、物理 GUI、原生剪贴板、实机中文回读和最终双实例 30 分钟仍须分别验收。

## 目标与边界

- 保留原文本页、三个动作、原 TextInput 即时双向绑定、单/多目标 TargetContext 和原 TextCommand。加载、粘贴和发送职责不同，不删除有效功能。
- TextView 原文件加载在 `ReadAllTextAsync` 后无条件赋值，会覆盖请求之后编辑或粘贴的新草稿；本轮以等价提取旧行为实际复现，再用相同正式夹具验证最小修复。
- 原发送在 action 内快照文本和目标，从不清空草稿；不把终端清空逻辑套用到文本，也不改变原发送次序或多目标门禁。
- 三个动作在六尺寸内可达，保留多行编辑、长文本完整内容和滚动。按现有主题局部适配，不新造第二套工具栏或修改共享样式。
- 本轮不触碰 FilesView、真实配置、共享 ADB、实例 0、账号、权限或网络设置。专用 1/2 的安全实机路线另行冻结和审查；只读调查不是实测成功。

## 第一性原理与选择

已知事实是文件读取和用户编辑可以交错；根问题是旧读取没有发布所有权，而不是文件内容或编码。只检查完成时文本相等不足以区分 A→B→A；禁用编辑会改变原交互，也无法覆盖同步通知中的重入。

推荐保留原文件读取与错误反馈，将其收敛到 VM 的明确加载方法；在请求入口记录编辑版本，只有没有新编辑才发布。版本在实际值变化且通知之前递增，重复赋同值不算编辑。更简单的仅值比较方案不能保护 ABA；替换文件读取器、锁整个页面或新增导入框架均不采用。不得把读取失败或草稿保留显示为已替换成功。

原文件选择器不可通过当前已停止的物理 GUI 分支验证。为得到可比较的业务红测，先将旧 lambda 原样提取到加载方法并接回原按钮，单独冻结这两项等价改动；其本身不是修复，红测版本不称为未经改动的 c63。后续同一正式夹具验证候选，保留所有失败原件。

## 任务与验收

| 优先级/阶段 | 工作 | 完成证据 |
|---|---|---|
| P0 / 定义与复现 | 隔离 worktree、原行为等价提取、正式夹具；禁止改原 235 断言和旧 Smoke | 原方法/按钮连线可核；新编辑、ABA、读取失败、同值赋值的实际状态，不把缺少方法算产品红 |
| P0 / 草稿所有权 | 最小 VM 修复；原发送及所有门禁保持 | 同夹具旧红/候选绿；Busy 同步通知与文件实际读写，失败可见，后续恢复可用 |
| P1 / 文字和控件 | 三动作命中区域、多行全文阅读、目标与绑定身份 | 390×640、390×844、768×1024、1000×700、1440×900、1920×1080 的实际 own-WPF 几何和图像；非物理 GUI 或 DPI |
| P0 / 业务与清理 | 原 TextCommand 正常/多目标/空/离线/忙/脚本与传输门禁；编辑中发送不误改草稿 | 实际 Demo 后端节点回读、逐目标调用、原任务排空、唯一 VM Shutdown，无假 Busy 或手填成功日志 |
| P0 / 交付 | 独立审查、主仓复验、报告/README/源码清单、普通提交推送 | 当前源/PDB/产物一致，用户三文件和旧证据保护；仅原私有 feature，不合并发布 |

## 开始时状态

- 已复核主仓 tracked/index 干净，仅原两张英文 PNG 未跟踪；上轮终端回执与实际 Git 决定其已交付状态，不重跑上轮测试。
- 已创建从 c63 detached 的 `text-workspace-ui`、`text-workspace-tests`、`text-workspace-verifier`；原生 worktree 入口因聊天 cwd 非 Git 不适用，沿用已验证的 Git 本地隔离方式，不清理旧 worktree。
- 计划与源码审查进行中；本轮尚未构建、运行或操作模拟器，所有新用例均未验。没有宣称加载修复、六尺寸或实机通过。

## 首次对照与候选

上述未运行描述为开始时记录，以下按实际时间追加，不覆盖原件。测试作者只在独立树 restore/compile，保留标准 CET 环境失败、首次兼容编译缺少 using 的夹具错误；最终按项目现有 managed-csc 路径编译成功，不把编译成功当用例通过。

- 冻结 r01 App `A8377232…FABD4F`、新 Smoke `32A06898…2873FA`；原 235 测试及旧 31 Smoke 不变。先完成四项执行前审查：状态不能谎报完成、从当前页面重查控件身份、首尾完整字形、runner 按实际 Text schema 读取；原 draft 首行始终拒绝执行。
- 唯一 `red-r01` session58865 已自然结束，outer0仅表示有效对照执行完成，专项 child42772 exit1。Build child44080 exit0/117829ms、原235全过、MCP14 PASS/13 NOT-RUN；三拒参、四源实际PDB、15唯一Shutdown/Closed、窗口/retained0。无真实模拟器、Clipboard 或 picker 操作。
- 原报告为 7PASS/8组PRODUCT-FAIL：六尺寸三个原按钮均为32高；B/ABA两组新草稿被实际文件内容覆盖，且误报“加载文本完成”。正常文件、同值赋值、通知重入、发送与门禁等七组通过。768另有一条摘要度量假阳性，须按下项单独裁定，不能把它当产品裁剪或追认所有叶均正确。
- 768实际PNG四行目标末尾完整，实际652×68、测试测量69.16。主题原控件使用Display及LayoutRounding，而[官方WPF v10 FormattedText](https://raw.githubusercontent.com/dotnet/wpf/v10.0.0/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/FormattedText.cs)七参构造使用Ideal。独立审查准许仅改测量构造使用实际TextFormattingMode，保持原内容、阈值及全部断言；不得为它修改产品padding或布局。旧r01源/全部原始失败保留，须新夹具另补同旧生产对照。
- 本轮原件在 verifier `artifacts/text-workspace/red-r01`：execution `232C2DA1B79A5BC1DC2F3FD291FAAF632E5C1EBB0A99E797FF1E35D752D45055`，after `ED63F705A384D231DE6C1415A287CDC7ADC2525E12773E1D8F9778B5797DD789`，专项报告 `291B2A2B959ADB1A43E610EA3552A0A83A0C61F787E59B4105530BC55709375F`。194测试构建输入/255实际冻结产物；main193/255、用户3和上轮11proof保护由runner核对，独立原流复核进行中。
- 仅在 ui 树形成候选 VM `C55A101D…E0CD5` / View `DD314D4A…BDD06`：加载请求的编辑版本、失效停止及原三按钮局部44；原发送、目标、编码及完整摘要未改。候选静审通过，尚未编译或执行，不标修复验收通过。主仓未导入候选；下一步固定度量窄修夹具、补旧红、同夹具验候选。

### 度量一致后的旧行为对照

- r02只将FormattedText构造改用实际Display模式；Smoke `5AE998FA…5A4E0C`，其余App/生产源、15例和全部断言不变。旧r01源码和日志仍在，未追认其度量错误。
- 唯一 `red-r02` session51319已自然outer0，Build child17536 exit0/103767ms、原235/MCP14+13 NOT-RUN、严格三拒参/四PDB通过，专项 child9040 exit1/3027ms。15组227检查/495登记Task终态、唯一Shutdown/Closed，窗口/retained0，无前置、夹具、报告或清理失败。
- 同旧生产实际7PASS/8纯PRODUCT：六尺寸每组三个32高按钮，B/ABA两组覆盖新草稿并误报完成；原768摘要检查已通过，不需要改产品布局。当前只有候选静审，不把旧红执行成功当产品通过。
- 本轮execution `591EA75B432A2B0DFD196A6C8FD1EE505E5E10AC33AFEAEA054340BF1B171800`，after `F865E3973E446F9409E0C5ACC81B9D6DF23CF744259A046E521C422988E66A19`，专项 `8662C8FCD9FB5E953B0F69DF54FFF63F34F5FC0D36F01AED0B272E683CDBA698`，均在新的 `artifacts/text-workspace/red-r02`。实际原件独审进行中；主仓和专用实例未操作。

### 首次候选复验

- red-r02实际原件/原流/四PDB/六图和保护已获独审，才复制C55A/DD314到verifier并提高mtime；四源摘要均固定，不复用旧编译物。Green-r01固定runner `06B2EA68…FC548`、合同 `286EF675…1E774`与r02测试完全同版，没有修改断言或新增产品摘要padding。
- 唯一 `green-r01` session73418已自然0，Build child22452 exit0/103724ms，原235及MCP14 PASS/13 NOT-RUN、三拒参/四源PDB通过；专项 child66900 exit0/3117ms。实际15PASS、227检查、495登记Task均终态，15唯一Shutdown/Closed，窗口/retained0，error/postflight均空。
- 文件B/ABA的新草稿和“加载文本已停止”反馈、同值及重入正常完成、原发送快照/节点回读/空离线脚本传输共享Writes门禁均通过。六尺寸原3按钮44、完整正文首尾字形及完整目标保持；图是自有Shell渲染，绝不是物理Windows输入或实机画面。
- 本轮execution `DF2BE926EC1FF3B2FDCA3749444DB4521AA80B80852FE0D82CBAD8AA58F7984E`，after `91BD91AA962CB10AFD362820A4488AD32DD33E32A84A27F07AEEF7A0FDDE75CC`，专项 `ABB37CDA1C7B4F1C848E41112265207F7303771CCD6353F73BA4B14A8D3912CE`。业务/六图及原流/PDB/源与保护两项实际独审均通过，才准四源主仓集成，不扩成整个TEXT-01完成。

### 主仓复验

- 精确导入四源并提高mtime；其余190旧构建输入保持，原235断言、31旧Smoke和全部失败原件不改。首次写导入记录误用不存在的`Set-Content -NoClobber`参数，toolChunk8e4a74在任何主仓Build前终止，不是环境或产品红。源已拷贝，不重复覆盖；核对旧源备份/其它190/旧255/用户3/11proof后，仅用CreateNew补记录。原件在`artifacts/text-workspace/import-r01/prelaunch-failure.json`。
- root唯一实际`Main-r01`/session49344自然0，同已审Green runner/合同；Build child53496 exit0/115108ms，专项child48232 exit0/3101ms。235原名序全过，MCP14 PASS/13 NOT-RUN，三严格拒参、四源实际PDB、194输入/255本轮冻结产物均核对。五child/十原流自然终态且排空，无timeout或kill，仍有既有编译警告，不称零警告。
- 主仓实际15PASS/227检查/495登记Task终态，15唯一VM Shutdown/Closed、窗口/retained0，error/postflight为空。非实现者业务/六尺寸实际终审通过：B/ABA草稿及停止状态、同值/重入、原12次文本调用和12节点回读、三组在途快照、原门禁及对象身份保持，六图逐一核摘要并实际查看。另一路实际原流/PDB/保护终审通过：原235名序、MCP14会话/42原流、三936拒参、四源PDB、194输入/其余190原源、255当前与本轮冻结、用户3及11份历史proof匹配。两位审查者未重跑或编辑产品。
- 主仓原件`artifacts/text-workspace/main-r01`：execution `AAC5B69E4B3AE2D4B8315FA86D1CDBACF9ACF3DD386CD81769BCCC737D2092D0`，after `197DD7A42958BCD1CDACDD477FDA17004AA99750AB6271EC275E933B214396B2`，专项报告 `FDCEEFAEE0B35009D3F609DA40BC69AF3BA6D95EB44E6B94B9BAFEA7B2136850`。合法主仓重建后使用本轮255冻结值，不再要求等于旧c63产物。

## 修改与复核方式

| 文件 | 本功能改动 |
|---|---|
| `WorkspaceViewModel.cs` | 增加文本编辑版本，在原setter通知前记录；加载入口捕获版本，实际读取后只发布未失效内容。原发送全文不变 |
| `Views/TextView.cs` | 原文件选择器接回VM加载方法；原三个按钮局部44，其余编辑/目标绑定保持 |
| `App.xaml.cs` | 增加最早分发的严格Demo专项入口，拒缺Demo、混用/重复/未知参数和非fresh目录，错误只写stderr |
| `Views/TextWorkspaceSmoke.cs` | 正式15例：六尺寸、真实本地文件、草稿通知边界、原命令节点回读与门禁、导航身份及唯一清理 |

先按现有`tools/Build.ps1 -DotnetPath <已配置dotnet绝对路径> -Verify`构建，再使用该构建的Desktop DLL执行`--verify-text-workspace --demo --data-dir <artifacts下尚不存在的绝对目录>`。每次使用fresh目录，原三种拒参不得创建profile或截图；检查`text-workspace.json`及15个`case.json`中的实际状态、失败列表、Task终态和清理，不只看退出码。入口无宽高/截图参数，六尺寸由正式夹具依次执行；本轮不另启普通真实窗口或操作模拟器。

完整物理GUI、DPI、原生Clipboard/picker、真实模拟器/ADB中文、任意文件I/O交错和最终双实例30分钟继续NOT-RUN。实际Adb.TextAsync拒控制字符/换行，本轮Demo多行回读不改变或证明该能力。文件进度和Size绑定AP073/074另立受控复现，不混进此功能。

交付范围为上述四源、README/正式记录与三份生成清单；新原始证据、APK、依赖和用户配置/英文PNG不提交。完整源包仍含继承的82份tracked验收材料，AP021分类开放，不宣称包已完成隐私净化。核包、精确index和单功能普通push须独立审查；目标仅原私有`codex/unified-device-access`，test保持，不合并发布。实际是否已推送以Git及`artifacts/text-workspace/push-complete.json`为准，不能预写提交成功。
