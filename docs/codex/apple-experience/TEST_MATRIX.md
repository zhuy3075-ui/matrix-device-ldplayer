# 当前版本功能与交互验收矩阵

日期：2026-10-16。初始基线8303cdc；当前目标仅有分阶段有限实测，完整验收尚未完成，逐阶段离线与实机证据分别标记。控件键见CONTROL_CATALOG.md。以下每个分项为独立caseId，不因同页其他操作通过而继承成功。

## 状态与运行方式

- PASS-OFFLINE：仅协议、状态或Demo夹具通过；PASS-LIVE：当前冻结版本真实目标效果通过。
- FAIL：预期不符；BLOCKED：依赖/风险阻止执行；NOT-IMPLEMENTED：能力未实现；NOT-RUN：未执行。
- 正常/空/离线/忙重入/取消/失败恢复/目标变化均独立记录；不适用必须解释。
- 最终版本逐路径重跑；旧报告只作线索。原始证据在artifacts/apple-experience/runId，脱敏报告不包含账号、密钥、节点全文和真实应用画面。

## 功能矩阵

| caseId及页面 | 正常路径与实际效果 | 异常与隔离 | 当前状态/证据 |
|---|---|---|---|
| DEV-01 设备 | 冷实例行启动→阶段进度→Android就绪→截图→已连接；运行实例只连接；勾选启动/全部启动同一计划 | 0保护、资源未知不弹确认、明确不足确认、取消后不继续、失败保留原因、代次/授权变化拒绝 | NOT-RUN；上轮列表三尺寸为历史PASS-OFFLINE，不证明当前实机 |
| DEV-02 设备 | 三态表头/单行勾选、更多菜单、目录自动更新、显示已有窗口 | 空可见/隐藏勾选清除、多选不沿用旧单实例、别名去重、自动发现不授权 | NOT-RUN |
| DEV-03 设备 | 新建唯一空白实例、资源读取/配置、关闭/重启后list2与PID实际改变 | 默认不对现有实例实施；配置需完全关闭，拒绝旧generation；取消无副作用 | NOT-RUN；确需风险行为时单独BLOCKED而非默认实做 |
| DEV-LABEL 设备操作含义 | 当前筛选表头范围、同一row冷/过渡/未知/运行/连接/保护等动作名称与描述 | Connected+Device优先、空Device矛盾、phase不授信；命令身份和原Demo启用性保留 | 独立PASS-OFFLINE：[A05b文案](A05_DEVICE_LABELS.md)，三桌面每36项/11观察，主仓最终复验同样通过；未调用命令，不代替DEV-01/02实机效果或权限验证 |
| DEV-LAYOUT 设备正文 | 六尺寸原DataGrid/虚拟长列表/冻结选择动作/三动作44/14信息边缘与原值/主控自然滚动 | 599/600、919/920边界；MainA/CurrentB/隐藏勾选/原滚动意图；新wheel不被旧恢复覆盖 | 独立PASS-OFFLINE-WPF：[A05b1](A05_DEVICE_RESPONSIVE.md)，六正文889或847检查、纯几何命令调用0；主仓两390各889。非实机、全值同时像素或其它页面小屏通过 |
| DEV-OFFLINE-DIAG 离线诊断 | 独立严格Demo新入口，原发布图绑定到MainA/CurrentB；同代与增代row Offline对照 | 窗口实际390×640、六快照只读、非有限分类/Empty/null标准JSON、Task/进程清理；三输入反例预创建拒绝 | PASS-OWN-DEMO-DIAGNOSTIC：[AP-041](AP041_OFFLINE_DIAGNOSTICS.md)独立/主仓各158及新390/27记录检查/22Task过；Catalog仍Ready，不是原指针套、原日志出口修复、实机或S01恢复 |
| DEV-REPORT 验收日志 | 同一原device-layout出口有限全文不变，NaN/正负无穷及Point/Size/Rect/Empty可写标准JSON | 原FAIL/phase/异常及清理聚合保留，真实I/O/不支持类型/两factory失败保留stderr；全局仍严格 | PASS-DEVICE-LAYOUT-REPORT-CONTRACT：[报告修复](AP041_REPORT_RELIABILITY.md)实际红6/8、同14类绿全过，独立/主仓各158、原3E几何/22Task/cleanup保持；8条注入预期错误不等于无stderr。仅日志子项，不关闭原S01/11NOTRUN、实机或完整GUI |
| MAIN-01 主控 | 已连接行控制→主控画面；主页/返回/任务/音量；单点/滑动坐标实际效果 | MainA/CurrentB、主控离线、脚本冲突、切换/关闭/拖动中断；不影响另一目标 | NOT-RUN |
| MAIN-02 主控 | 同步点击滑动和同步按键各自只作用明确勾选其他设备 | 未连接/旧transport目标拒绝；不监听原生雷电窗口/不冒充物理键盘镜像 | NOT-RUN |
| MAIN-UI 主控路由与截图守卫 | 实际Image路由Tap/Swipe/同步B，核Demo副作用与冻结snapshot；同一主控关闭/离页/失活后拒绝新截图任务 | resize/失捕/离线/忙/各代次/正文滚动/同形不同像素/父留白与捕获拖出均零新输入；已接纳截图可完成但不接纳第二轮 | 独立及主仓代表尺寸PASS-OFFLINE-WPF：[A05b1](A05_DEVICE_RESPONSIVE.md)；默认内部gutter不适用，实际外侧留白已测。不是物理点击、Android、自动timer或ADB wire证明；MAIN-01/02实机仍NOT-RUN |
| NATIVE-01 主控投屏 | 启动/嵌入/恢复均为同一明确主控、同一自有进程 | A/B反例、confirm/await切目标或runtime、PIDbirth、退出/复用、窗口恢复失败保留记录 | 守卫PASS-OFFLINE：[A02独立验收](A02_MAIN_TARGET.md)；真实确认/启动/嵌入NOT-RUN，不得无人值守运行外部ADB |
| MON-01 监控 | 两个实例差异测试画面、每卡实际动态视频帧、全局暂停/画面/资源/范围选择、卡片选择/双击/窗口/重试 | 静止帧非失败；滚出/切页/关窗释放server/decoder，nonce身份失效拒绝；不以截图当视频 | NOT-RUN |
| MON-02 监控 | 主机/实例CPU和工作集趋势，首样本未知、后续增量，打开主控/原生窗口 | PIDbirth变化不续用旧样本，缺权限未知非0；高负载不阻止目录自动更新 | NOT-RUN；不扩张双路为23路容量 |
| MON-LAYOUT 监控正文 | 六尺寸真实视口/44动作、两模式/范围/暂停、长文滚尾、同卡片与Bitmap、图表实际宽 | SCP条带/相交边界、九组Auto阈值、history/Latest不伪采样、同nonce待帧、空目录及Demo协调清理 | 独立PASS-OFFLINE-WPF-LAYOUT-OBSERVATION：[A05b2](A05_MONITOR_RESPONSIVE.md)，六尺寸各952/308、主仓两390同样通过。Open/Retry纯几何回调0，合成像素非视频；AP019/020原首败未解，MON-01/02实机仍NOT-RUN |
| NODE-01 界面检查 | 自动首载、截图与节点、树/框双向选中、属性、展开/折叠、JSON导出 | 空树/超时/离线、刷新中切目标、旧节点不发给新目标、导出取消 | NOT-RUN |
| NODE-LAYOUT 界面检查正文 | 六尺寸及正文640/920±1，同原画面/树/属性重排，原工具/树头44、首中末节点/框双向选择、长文尾部和持焦/锚点/选区 | 空/真实Busy/失败重试、无新增后端请求；中段/底部意图恢复与五种新用户信号取消旧恢复，绑定及目标不变 | 独立及主仓PASS-OFFLINE-WPF：[A06b2a](A06_NODE_RESPONSIVE.md)，三桌面每11例/905检查/319观察/11清理及旧16/49与原套、六shell135；主仓150/完整11及原套/390shell135通过。不是物理输入、私有ticket、18px树箭头、原生导出/Android或全部NODE-01实机通过 |
| DEBUG-PUBLISH 界面检查/截屏及后台源缓存 | Capture/LoadNodes/Wall/Automatic四入口固定来源；正常几何刷新、合法后台缓存返回、节点导出 | 首次await切目标/ABA、行及Catalog-only身份失效、新B检查不被旧Auto清除、Busy no-op、固定源故障日志、后端/外部取消与恢复 | 独立PASS-OFFLINE-A06A：[A06a](A06_DEBUG_PUBLICATION.md)，三桌面每49case/401检查/191观察、49清理/9正确来源；独立与主仓150、主仓49/390shell及原回归通过。不是NODE-01自动首载、按钮、生产timer、真实ADB或runtime热换证明；旧Appearance首败AP023保持开放 |
| AUTO-FIRSTLOAD 界面检查事件调度 | 真实导航Click、Current setter、Busy/External/Transfer/Script事件释放后自动读取最终目标并建立真实Tree/Canvas；已有节点返回不重读 | ABA、双阻塞不越过剩余脚本、在途切换、离页、真实Close/脚本取消、失败/取消后不循环且重新进入恢复 | 独立PASS-OFFLINE-A06B1：[报告](A06_AUTOMATIC_LOADING.md)，基线16例8失败，首轮修复后三桌面各16/137/16cleanup+旧49及原套；六公共shell135、主仓150/完整16+49/390shell通过。仅自有Demo窗口临时许可与真实事件链，不是生产timer/Android/同row离线重连、缓存换代或全部NODE-01控件证明 |
| CAP-01 截屏 | 取色坐标/RGB/HEX、缩放、裁剪、PNG保存、颜色双击复制 | 缩放坐标、边界/零面积裁剪、目标改变丢弃旧裁剪、无图/保存失败/取消 | NOT-RUN |
| CAP-NATIVE 原生截图保存 | 实际裁剪/保存按钮与原生框；合法晚到帧仍保存开框前原图/选区 | 无图/过期拒绝、Offline历史、取消、Busy/写错反馈及清理 | [A06d](A06_NATIVE_IMAGE_SAVE.md)：首次独立Build150/0警告错误通过，但sky窗口捕获两次超时，Down/Up/SaveClick0，原生路径NOT-RUN（前置失败）。child91.520秒正常exit1/cleanedtrue，保留原FAIL；无保存产品红/修复/通过结论 |
| INPUT-DIAG 截屏鼠标前置 | 单次OS定位及原WPF命中/事件；新增三点有限只读观测，不改变合同 | 对照OS直接转换、Mouse输入源/捕获/焦点；异常独立留证、不修复输入状态 | [AP-033 p01](AP033_INPUT_DIAGNOSTICS.md)：独立150过，完整GUI仍FAIL；旧19/18过，截屏9PASS1前置FAIL/10清理，后三快照源null/直接映射正确/OS后移，未RaiseEvent，其后新旧套和12页NOT-RUN。只确认本轮状态，根因未定；root未重跑，非产品或实机通过 |
| CAP-LAYOUT 截屏正文 | 六尺寸与640±1，同图/Canvas/Scale/色表重排、44工具、100合法样本首中末三列、实际缩放/双向滚动取色 | 无图/真实Busy/安全错误恢复；完成crop种子与目标/焦点/合法滚动offset保持、零额外设备读；轨道仅实际可见中心命中 | [A06b2b](A06_SCREENSHOT_RESPONSIVE.md)独立PASS-OFFLINE：150、三桌面每10例630检查257观察10清理+旧11/16/49原全套、六shell135；主仓150/51.43秒、1000全套及390shell135通过。四档单次系统定位+合成WPF路由不是物理点击/Android；未证明轨道点击、原生保存/剪贴板、在途裁剪或全部CAP-01 |
| DEBUG-COMPLETED 已完成检查缓存 | 新Ready传输/几何/重连身份不复用旧完成图/节点，Catalog-only新身份的返回/本地PNG/JSON消费检查 | 明确Offline历史查看和真实本地导出、metadata-only稳定刷新、同身份后台新缓存不清当前旧检查、主控/当前隔离及恢复 | [A06c1](A06_COMPLETED_CACHE.md)PASS-OFFLINE：原15红/首14-4/七叶诊断均保留，r1只纠正有效目标门禁误预期。独立150/三桌面各18/295/163/18清理及旧10/11/16/49原套、六shell135；主仓150/同18与旧套/390shell135。实际文件、明确拒绝反馈已核；outer.colors=0不算外层取色成功，不包含同row自动首载、原生保存、在途crop、真实runtime热换或AP028完整历史 |
| DEBUG-RECONNECT 同行恢复首载 | 完整VM目录接纳当前同row新Ready后自动图/树；仅宽高与仅旋转、metadata、单/双真实活动合并最终身份 | 在途旧payload不发布且Shot/Dump最大1，自身几何失败/取消同身份不循环；外部pending被本次采用后消费，离页/真实Close/defaultDemo不新增读 | [A06c2](A06_RECONNECT_LOADING.md)独立及主仓PASS-OFFLINE：原19=8PASS11FAIL19清理；首生产150/0警告错误、三外层桌面每19/289/19清理、事件2214/2214/2212，旧五套/完整流程、六shell135过；根150/同19/289/2214及原五套/390shell135过。19自有窗口固定1000，不是每例六尺寸；非生产timer/任意Info或Catalog-only、Android/native保存/runtime热换证明，outer.colors/nodes仍0 |
| SHELL-01 终端 | 目标选择→执行→输出/错误码、历史上下；输出框系统文本复制语义 | 长输出/空命令/超时/离线；不因切页发旧命令、不劫持文本编辑快捷键；专用取消/复制/清空控件不存在，记不适用 | NOT-RUN |
| APP-01 应用 | 当前独立目标区双选→读取真实包/版本；第三方/系统应用过滤；应用行选择 | 空/离线目标、重入、别名去重、与设备群控/主控选择不互相授权 | NOT-RUN；旧LiveUiAcceptance未走当前批次UI |
| APP-02 应用 | 自有唯一APK双目标安装→包名/签名/精确版本→启动前台→停止后不运行→卸载后包消失 | 多APK、部分失败/取消/重试、确认后identity变化；只卸载本轮新增包；不碰抖音数据 | NOT-RUN；须补当前UI到效果链 |
| APP-03 应用 | 系统应用不可卸载；清数据独立警告与目标快照 | 取消无副作用，清数据仅对本轮自有测试包；业务返回不等于实际清空 | NOT-RUN |
| APP-IDENTITY 应用改名 | 当前应用页旧行/勾选保留，metadata改名后同连接启停、确认和在途结果；表格即时显示新名 | 同名不串号，选择/代次/transport/离线变化拒绝；原Task与结果清理 | PASS独立离线限域：[A07a](A07_APPLICATION_IDENTITY.md)原3红保留；独立f02新10/114检查/10清理及全旧套/12页通过。主仓源逻辑同且150过，但root完整GUI在新hook前旧坐标前置FAIL，新10和后续当轮NOT-RUN，AP-033开放；不替代APP-01..03实机 |
| APP-AUTO 应用目标自动读取 | 原Checkbox/TwoWay与350ms Tick，脚本三终态/稳定Push/Busy及双阻塞释放后只读最终选择；全局B与应用A独立 | 旧A脚本不全局阻塞改选B、清空/Close零List、离页不读返回恢复、失败无循环后新意图恢复，原Task和门排空 | [A07b1](A07_APPLICATION_PENDING.md)原7红保留；独立及主仓各12/310/12清理、150/0警告错误及旧七套（含身份10）/原应用/12页全过。真实应用勾选与原timer、受控Demo后端；不是API释放唤醒、布局/物理输入/原生模态、APP-01..03实机或完整目标通过 |
| APP-API 应用接口排空读取 | 自有真实loopback HTTP阻塞原350ms Tick，单/双请求末次释放只读最终选择；API与Push/脚本双门、503、客户端取消 | 首个请求结束仍阻塞；清空/离页/Close、异常observer、已排队原回调、List失败后新意图；所有Task/HTTP门清理 | [A07b2](A07_API_DRAIN.md)原生产12例8产品红/4绿保持；r2原12路径加2合同，独立及主仓各14/496/14清理及158/全部旧套/12页通过。是逻辑WPF与真实HTTP，非物理输入、runtime热换、APP-01..03实机或30分钟 |
| APP-LAYOUT 应用正文 | 六实际尺寸、原三表首中末行/水平滚读、长值/批次停止与恢复、选择/状态/焦点保持 | 单390×844字形150%不冒充真实DPI/原字体绑定；布局不改全局或加backend调用，默认提示框完整可读 | PASS有限域：[A07c](A07_APPLICATION_RESPONSIVE.md)/[A07d](A07_TOOLTIP_DIAGNOSTICS.md)保留全部原红；c02及主仓r01各158、新12/2890/12清理、原17源Smoke链/12页过。当前505字体成员含344隐藏全登记/1.5。s01六壳层各135过，首设备指针前置及报告失败，11格NOT-RUN/AP041；20只计冻结旧源，不称全模式已过。限域独审允许开发分支交付，不是APP-01..03实机或全GUI验收 |
| FILE-01 文件 | 设备浏览/目录→上传→下载→SHA一致；多目标结果；上传APK后安装流程 | 坏路径/权限/覆盖取消、传输取消、进度/部分失败/断连；清理仅唯一测试目录 | NOT-RUN |
| FILE-02 文件 | ls目录文本与上传/下载队列的重试、停止等已有入口逐项验证 | 不对未知目录做清理；当前没有设备目录新建/重命名/删除入口，不列作原功能或新需求 | NOT-RUN |
| TEXT-01 文本 | 粘贴、加载文本文件、明确单/多目标发送中文/英文/空格/换行，测试包回读一致 | 文件选择取消/坏编码、空串/离线/旧目标拒绝、输入模式不支持如实报错；无账号页面使用 | NOT-RUN |
| SCRIPT-01 自动化 | 导入有效jxscript、清单/参数→单/双目标运行→步骤/回传→完成 | 无效ZIP/动作/参数、取消2秒内不再执行、重复启动/目标变化/步骤失败隔离 | NOT-RUN |
| API-01 接口 | 普通网关/设备端口/自动化启动，health/devices、请求实际设备效果与响应一致 | 端口占用/服务拥有权/停后排空、错误状态、旧transport/未授权拒绝 | NOT-RUN |
| API-02 接口 | 当前UI方法/路径/body编辑→发送→结果；设备地址双击复制 | 格式坏值、超时、日志脱敏；停止设备端口不误认为关模拟器；当前无归档/密钥显示/专用取消入口 | NOT-RUN |
| API-03 接口 | 27条自动化路由逐一正常/坏输入/目标隔离 | 私有agent、节点setText/ROM等缺支持要NOT-IMPLEMENTED或BLOCKED，不以501当业务成功 | NOT-RUN |
| TOOL-01 工具 | 当前代理解析/脱敏导出、root核验、时区、本地资料包导入/导出/删除逐一登记 | 不把本地资料包当设备备份恢复或剪贴板业务；不修改全局代理/证书或启用root；仅本轮自有包与目录可做安全夹具 | NOT-RUN；依赖缺口待逐条列明 |
| SET-01 设置 | 所有字段/路径/组合框、保存→重启→值保留，坏值明确标出 | runtime与编辑设置不混用，忙时拒绝危险变更、端口/序列号JSON/授权边界 | NOT-RUN |
| SET-02 设置 | 390/768宽窗口设置保存/重载，恢复、减弱动画、中文状态 | XAML/smoke/Apply/Normalize四处一致；不修改用户实际配置作夹具 | 几何Apply/Save/新Store.Load PASS-OFFLINE：[A04](A04_RESPONSIVE_SHELL.md)；恢复及真实OS偏好操作仍NOT-RUN，应用内动画分支见A03 |
| DETAIL-01 全局详情 | 只读状态/目录与改名持久化 | 不暗改群控/主控/应用目标；详情不再持有全局筛选/群控勾选 | PASS-OFFLINE：[A05a](A05_DEVICE_STATE.md)，实际目录选择/保存命令、新Catalog连接回读及GroupId保留；三桌面每73专项。真实设备目录效果仍待A11，不代替MON-01视频验收 |
| UI-SHELL-01 公共层 | 六尺寸实际窗口、699/700与1099/1100导航、Field459/460、菜单/指定自有弹窗 | 同对象/绑定/输入/焦点/选择；几何坏值、Owner跟随、实际模态取消 | PASS-OFFLINE：[A04](A04_RESPONSIVE_SHELL.md)，独立六尺寸各135、实际HWND/大小；AP-011/012保持开放；不替代下行全页 |
| UI-ALL-01 全12页 | 390×844、390×640、768×1024、1000×700、1440×900、1920×1080真实控件路径 | ActualWidth/Height精确匹配请求、长内容/空/忙/禁用、菜单/模态不被裁切 | 全范围NOT-RUN；A04仅公共层六尺寸和三桌面完整Demo通过，固定正文仍A05-A09，不能以局部代替 |
| UI-ALL-02 全12页 | 简体短名称、层级/对比、键盘/触控命中、焦点/减少动画 | 普通字4.5:1/非文本3:1、44逻辑命中、Tab/Enter/Esc、真实150%DPI与字体分别记录 | 共享外观/名称PASS-OFFLINE：[A03](A03_APPEARANCE.md)，195断言及三桌面尺寸；44触控、全面键盘、字体放大/真实DPI与移动尺寸仍NOT-RUN |
| FINAL-01 全局 | 最终冻结二进制双实例30分钟；错误率/延迟/资源/清理与共享ADB owner不变 | 每次失败和重试独立计数，不删日志；运行中切页/取消/重连，禁止0/账号数据 | NOT-RUN |

## 实机前门槛

只读重新冻结目标1/2的实例名、index、generation、Player/VBox PIDbirth、canonical serial与transport，以及共享5037 owner/PIDbirth/path；禁止按5554端口公式授信。隔离配置且API默认关闭，记录buildSHA。自有APK记录源工程、签名证书摘要、SHA、版本和安装前不存在证明。优先现有可信工具链；缺工具不下载未知APK。

顺序为只读→单目标安全写→双目标差异任务→最终30分钟→核验清理。旧MultiInstanceAcceptance prepare会quit/modify，不可复用。HTTP Toolkit网络暴露/代理风险尚未核验时不抓包或更改代理，普通HTTP日志不等于抓包证据。

## 本轮历史证据引用

8303cdc保存上轮已独立验证列表精简：149项、1000/1440/1920三尺寸12页与中文结果；它证明对应离线和Demo基线。当前目标未复验的其它控件/实机项保持NOT-RUN。每项实现后附脱敏当前报告和独立复验结论，不回写旧报告制造新绿。
