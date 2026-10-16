# A09a 设置工作区

## 背景与目标

基线 `bc471bf`。AP042父链负实验已交付，未修复GUI；本阶段不再重复该诊断。设置页的有效目标是“用户输入、保存结果、重新加载一致”，并使全部原设置在六种尺寸可达，不以更换颜色替代保存正确性。

启动时静读候选：Editor与Store的数值范围不同；Agent地址保存校验缺少Runtime已有的HTTP/HTTPS约束；WPF数值转换失败可能保留旧源值；保存后的Editor未吸收三方合并结果；窗体字段可被正常关闭时的旧实际尺寸覆盖。实际红绿证据见执行记录，不能把静读候选当作已运行结果。

## 范围与非目标

- 修复设置保存的校验、成功后基线与显示同步、明确提交窗体尺寸的应用；保留原Runtime/Backend/API/Input/Scheduler和原SettingsEditor身份，不重建连接或增添授权。
- 设置页原21个输入、4个复选框、7个按钮、两分区、固定简体中文与4项未接入功能及其禁用解释全部保留。布局只移动原对象，小屏改单列，大屏保留双列，内容可滚动；不改共享主题或其它页面。
- 不更改SettingsStore既有有效范围：投屏尺寸180..4096、FPS1..120、命令超时1..300；Editor应在保存前拒绝范围外输入，不静默夹取后称成功。
- 不操作真实配置、模拟器、ADB、账号、Agent网络、系统权限、VPN或本机已有服务；只用fresh artifacts Demo配置与自有loopback API。原生文件选择器、遗留真实窗口恢复、物理输入/DPI/完整GUI仍单列未验。
- 备份恢复后旧坏主文件覆盖备份的候选另记后继，不在本阶段重写Store、迁移配置或重定义自动恢复授权策略。

## 最小方案与取舍

推荐复用原SaveSettingsCommand和SettingsStore：保存前检验本页实际绑定错误与现有Runtime URL约束；成功后同一Editor接收Store返回的合并值及新baseline，通知绑定刷新。明确修改的窗体尺寸立即应用到当前窗口，使正常关闭的原保存逻辑不再覆盖它；只保存其它字段时，不把用户手动调整的窗口强制缩回旧尺寸。

几何按轴区分意图：Editor与自身保存基线不同的轴是明确输入；窗口只报告自初始/上次成功保存后实际变化的轴。未明确输入但实际拖动的轴带入本次保存，成功后只应用明确输入轴并更新窗口观察基线；失败不更新。外部writer改尺寸而当前窗口未动时，连续保存不回写旧窗口尺寸。活动的同步鼠标/键盘和列数沿用原VM控件权威，不因外部配置写入自行启用同步。

更简单的仅换成单列无法解决“保存成功却不同”的根问题。拒绝扩大Store/后端数值上限来迎合Editor、为保存而重建Runtime或直接覆盖整个磁盘配置；这些会改变资源边界、破坏并发合并和连接身份。也不把程序内赋值/命令调用称作外部GUI输入验收。

## 验收标准

1. P0：先冻结现有225项和相关输入，在独立Demo用实际VM保存及本页TextBox绑定复现上述候选；每例步骤/预期/实际/失败类型、文件前后字节、Runtime与Editor身份单列，不改原断言取绿。
2. P1：合法值/边界值及nullable清空可保存；新Store加载与同一Editor及Runtime.Settings一致。HTTP/HTTPS合法地址只解析、不连接；其它scheme/相对地址保存前拒绝。JSON坏格式、重复串号和端口、数值越界/非有限值保持明确拒绝。
3. P1：实际TextBox输入abc或空数值时不得落盘、改变Runtime.Settings或显示新成功；修正后可保存。保存命令仍沿用原Busy禁用/恢复逻辑，不把旧整数值当作新输入。
4. P1：外部writer改未编辑字段后，两次连续保存均保留其修改；Editor刷新但引用不变。写入故障或运行中API改端口拒绝时，原配置字节/Runtime/监听保持，Busy恢复，修正或释放自有故障后可重试。
5. P1：通过原保存控件明确提交窗体宽高后，实际窗口与配置一致；真实原WindowClosing组件路径完成后fresh Store仍保留该尺寸。其它字段保存不重置手动resize；正常关闭与受控Abort/Dispose分别报告，不重复释放。
6. P1：390×640、390×844、768×1024、1000×700、1440×900、1920×1080均覆盖原控件、绑定、Command和身份保持；首中末滚动、长路径/JSON/错误文本、44px交互区域和4项禁用原因分别核。额外1280×800不得替代原六格。
7. P1：独立verifier核实际源码/PDB、原始流、所有任务/自有进程终态与用户文件保持；主仓单次集成代表后再文档/清单/核包、精确提交原private feature。失败/未验不计通过，不合并发布。

## 执行与风险

- 生产保存逻辑由主代理在settings-save修改VM/MainWindow；设置正文与输入校验桥由独立settings-ui修改SettingsView；测试作者仅settings-tests的App严格入口和新fixture。settings-verifier用于冻结集成与唯一执行，作者不得并发运行窗口或设备。
- 新入口须最早拒绝无Demo、混模式、未知/重复/缺值和既存目录，固定六尺寸、不接受外部截图。默认目录/真实授权不许进入；不会绕开旧AP042完整GUI入口。
- 先复现，再最小修改、原基线与专项、独立审、主仓代表。清理先排空原任务再唯一释放；pending保留所有者、停止后继，绝不把WaitAsync超时等同取消完成。PowerShell等待返回值必须显式抑制并核helper只返回一份record。
- 已有URL、字段与Store合并可能出现更多互相矛盾；新增根因超出本页时另列，不吞异常或顺手重写全局配置。全部GUI/实机/RPA/MCP/抓包与最终双实例30分钟仍必交付。

## 页面与组件

| 文件 | 本阶段改动 |
|---|---|
| WorkspaceViewModel.cs | 原保存命令先检实际绑定、对齐Store有效范围及HTTP/HTTPS校验；成功后同一Editor完整回填并重建保存基线，不创建第二Runtime |
| MainWindow.xaml.cs | 按轴识别窗口自上次保存后的实际变化；只应用明确输入的轴，保存失败不消费几何意图，保留原正常关闭路径 |
| Views/SettingsView.cs | 原两分区随实际正文900阈值单/双列重排；原控件44逻辑高度；授权串号JSON高120并内部滚动，保原21输入/4复选框/7按钮与禁用原因 |
| App.xaml.cs | 新固定六尺寸、显式Demo及fresh绝对artifacts参数的最早验收分支，不进入默认配置启动路径 |
| Views/SettingsWorkspaceSmoke.cs | 16组实际自有WPF控件/原绑定与命令测试，单列业务、夹具、前置、报告与清理失败，不替代外部GUI/实机 |

## 复验方式

在本机项目根目录使用已配置SDK，先确保本次源码确实重新编译；从其它工作区复制保留旧mtime的源码时，不应把增量Build的退出0当作新代码已编译。此次本机runner固定校验实际五源PDB后才进入设置专项，原件在本机artifacts而不上传。

```powershell
$ErrorActionPreference = 'Stop'
$dotnet = Join-Path (Split-Path -Parent $PWD.Path) '.tools/dotnet/dotnet.exe'
& ./tools/Build.ps1 -DotnetPath $dotnet -Verify
$profile = Join-Path $PWD.Path ('artifacts/settings-recheck-' + [Guid]::NewGuid().ToString('N'))
& $dotnet ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --verify-settings-workspace --demo --data-dir $profile
```

检查profile根的settings-workspace.json与各case.json：固定16组、原控件身份、磁盘前后摘要、原任务与唯一清理owner；同时阅读notRun，不能只看PASS总数。复验会显示自有演示窗口和建立自有临时loopback服务，不使用真实模拟器或用户配置。完整外部GUI/真实设备/30分钟仍由后续阶段完成。

## 执行记录

- r01：旧生产仅加冻结入口/夹具后的首次Build自然失败（PID75008，21.399秒）。夹具误用不存在的`StopBasicAsync`，另两处已追踪但未显式discard的Task造成CS4014；不是产品红，原225、MCP与16专项均NOT-RUN。外层session75540终态1，双流完整、无超时/kill/pending，主仓与用户保护通过。原件在settings-verifier的`artifacts/settings-workspace/red-r01`，execution SHA256 `E75A0C5434B4604E12B2A34DBD9F57B68C209797AF82FA9FD6A3FBBC81136597`；209个部分编译物已按after清单复制保留，不能称完整构建。后继只修既有StopAsync调用与两个明确discard，不改业务断言。
- r02：修正编译调用后，原225名序精确通过、MCP Demo14/13NOT-RUN、三种拒参通过。Build27492自然0/103.809秒；设置78388自然1/7.670秒，11组纯PRODUCT、4组PASS、1组MIXED。六尺寸与转换/范围/URL/合并/尺寸意图均取得业务红；额外实际超宽前置未建立，不猜平台或产品原因。16次清理、1618登记任务与6子进程/12原始流均终态，无超时/kill/保留/残窗。完整255编译物与实际五源PDB获独核；execution `EBB2D516A9CF77CFC938F73A04D15B8468E226B7312B5F896A17C32D67DDA009`、summary `6A512A204746C5EBAFFEFD0B6B1DF3AF1D1A033C21B578E0C44967352539D1C8`保持。后继r04夹具只补独立target7780记录，并将实际未超过7680的该额外子步明确NOT-RUN；原七核心范围、finally恢复/重试及其它16组断言不改。不能据后继结果抹去此MIXED或宣称全部尺寸边界已验。
- r03：相同最终入口/夹具下，旧生产原225精确通过、MCP14/13、三拒参通过；16组为4PASS/12纯PRODUCT，额外超宽子步骤NOT-RUN。Build73224自然0/104.101秒、设置29372自然1/7.363秒；1621登记任务、16清理和6个直接自有子进程/12流全终态，正常关闭只有原WindowClosing唯一owner。execution `8A6777B242EE342D7B170D9CF3AFA736CBC1AB84511A5A557270428CC86CAEE4`、summary `2DB2EDD102DBD70B8294AC48A5F3092A4E302B081518DE3BB6F6F53FFF9A3B3E`已独核；旧五源从compiled-frozen PDB对sources清单核，不以切换后的候选源码冒充旧构建。红测外层0只说明预期失败被正确记录，不表示业务已过。
- 候选green-r01：外层1、原回归过，但三个生产文档的实际PDB仍是旧294E/DC97/12F，与该轮来源E50/4C7/B42不符，完整DLL/PDB也与旧red-r03逐字相等。源复制保留的mtime早于产物，增量构建跳过；该轮4PASS/12红不能据为候选业务结论，原execution `6EFC7ACC8AC158570E0E3BE324DB638F45CACA12A423D1EF0C1C06DA619936D4`及stale观察保留。后继只推进自有五源mtime，逐SHA核字节未变；固定PDB脚本加入Build之后、设置实测之前，五文档不符就拒绝运行设置用例，不修改Build.ps1。
- 候选green-r02：实际五源PDB匹配，原225/56.44秒、MCP14/13与三拒参过，7个直接自有子进程/14流及16清理正常终态。实际候选16组为15PASS/1PRODUCT：十组保存业务与五尺寸过，仅390×640授权设备串号JSON的末字裁剪失败（step84、verticalOffset0、tail为空）。不是缓存问题；execution `F2C7BF901143B57A4222EA00EB15E11FEA8249EAC795EE4D61045D70726C0AD0`、summary `F2034405CA72D1B9A0CA46F1806690CDD233C90C9D3B0D28A76D1BD023A486A5`保留。后继只拟给原JSON输入明确高度，使其内部可滚，不改原用例/绑定或判据。
- 候选green-r03：仅原JSON输入Height120后，相同50F78夹具16/16通过、3066检查、1621登记任务及16清理正常终态；五源实际PDB、原225、MCP14/13NOT-RUN和三拒参过。Build80668自然0/102.805秒、设置76396自然0/7.532秒；7个直接自有子进程/14流全排空，无超时/kill/保留，窗口0/MainWindow空。execution `6FDEBB0D2F3E9C5518E7DE8B5D36927E95D83E34BB4DE80CF3AD6E5A5D331151`、summary `2EA62B08CEF5C41369605FE94509DA7CE2822B6F81E7654D1EA562C3A57DCD2B`；仅own-WPF/Demo，不代表真实输入、Agent网络、原生picker、最大化/RestoreBounds或实机通过，额外7780实际超宽仍NOT-RUN。主仓代表及最终交付以其实际记录为准，不先写已推送。
- 主仓main-r01：五源按冻结字节集成，先更新时间戳确保重编译，五源实际PDB再次通过；原225、MCP14/13NOT-RUN、三拒参与16/16、3066检查、1621登记任务全过。Build24140自然0/117.376秒、设置55236自然0/7.275秒；7个直接自有子进程/14流全终态，无超时/kill/保留/清理错，窗口0/MainWindow空。原件在主仓`artifacts/settings-workspace/main-r01`；execution `0865CA9749F2B7D55424C2FC91EBFFDEAD341B8D6286322F1D3CFB6E12FE6B1F`、summary `B31884431E64D15E49DEDEA032BEBA94484CBE0AB7F57203195D558B930AEE0A`、PDB记录 `032DA5B60F73A0818C45BBEAC0C869B3E873243E75CEC9436E9F74F576B5E897`。不改变未执行范围，不把main代表等同完整GUI/真实设备验收；是否提交推送读取实际Git与回执。
- 交叉审指出活动VM的WallColumns/SyncMouse/SyncKeyboard仍按原控件权威保存；本阶段不把Editor字段回填扩称全部AppSettings的外部并发意图已修，也不把外部同步开关灌入活动VM。该保守边界与非Normal窗口、真实picker/恢复、物理GUI/DPI及实机仍需后继覆盖。
