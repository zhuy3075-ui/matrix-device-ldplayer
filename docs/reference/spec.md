# spec：极星双程序技术规格反推

> 开发参考资料，来自 2026-10-14 静态分析；不是已批准的首版建设规格，也不是当前运行结果。原始日志、CIL/IDA 导出、样本和资源中间文件已按用户清理要求移除。正文中的 token/RVA、evidence 路径仅保留为历史定位，不能当作现存文件；动态修正见 [已知问题](../KNOWN_ISSUES.md)，来源与可复核边界见 [来源说明](../SOURCES.md)。


版本0.1，2026-10-14。适用构建由 ARCHITECTURE.md 和 ../SOURCES.md 的 SHA256 固定。下述“现状”依据静态代码；“复刻验证要求”是待执行测试，不表示已通过。原完整方法与地址索引已清理；核心流程仍可参考 PSEUDOCODE.md，深挖未知分支需恢复原证据或重新采集。

## 1. 模块边界与依赖方向

| 模块                 | 输入/职责                                          | 输出/依赖                                      |
| ------------------ | ---------------------------------------------- | ------------------------------------------ |
| WPF View/ViewModel | 用户选择、命令、状态绑定                                   | 服务调用；Dispatcher更新；WeakReferenceMessenger通知 |
| 设备目录/生命周期          | serial、发现事件、授权/在线状态                            | 会话创建/断开、设备模型；依赖CommonCore/ADB              |
| 调试工作台              | 选定设备、刷新/取色/节点操作                                | 截图、节点树、属性、Shell历史、文件/应用结果                  |
| 云控输入服务             | 输入snapshot、source/targets、generation/gestureId | 源投递与逐目标mailbox；依赖JXScrcpyCore              |
| 脚本服务               | ScriptPackage、设备、参数、取消token                    | 步骤执行与日志；依赖ADB及可选Automator设备会话              |
| 普通API服务            | HttpListener请求、已绑定设备                           | 基础ADB命令；ok/data响应                          |
| Automator网关        | serial + DTO                                   | 会话/节点搜索/核心API；code/message/data响应          |
| JXAutomatorCore    | 设备serial或baseURL、动作                            | 引导手机JAR、串行队列、HTTP客户端                       |
| JXScrcpyCore       | serial、视频/控制选项                                 | 设备投屏服务、音视频接收/解码、控制包                        |
| ProxyCore/JXAPPCK  | 代理配置/备份上下文                                     | root条件下的设备操作及结构化结果                         |
| 持久化                | AppConfig/实体/语言偏好                              | JSON、SQLite接口、.NET Settings、日志             |

禁止将同名但不同SHA的库当作同一个协议版本。调试UI直接引用自身Core；没有证据要求它依赖云控本地API。第三方库和Android端实现具有独立版本边界。

## 2. 启动与生命周期

云控入口 `App.Main 0x060002bc/RVA0x19eec`；OnStartup `0x0600019c/0xff1c` 进行单实例、日志、异常/trace注册、配置和语言初始化，然后打开启动/登录窗。延迟初始化使用Interlocked防重复，先配置ADB路径、Dispatcher和ScrcpySessionManager，再构建小屏/主控/恢复协调器。扫描/补连存在登录和设备额度门限；服务端授权语义未恢复。

调试入口 `0x06000868/0x28324`；App构造初始化ADB/日志，OnStartup初始化投屏；连接工作台后按顺序获取截图与Dump并发布消息。各步骤有独立异常处理，不能把“显示截图”推导成“所有自动化能力均就绪”。

建议复刻的可见状态（语义归纳，不是宣称原枚举名）：Discovered → Connecting → Ready；连接失败→Failed/Retrying；断开→Offline；关闭→Disposing。API会话、投屏会话、脚本任务应分别跟踪，避免单一“在线”图标掩盖子系统失败。

运行模型：WPF UI线程；ADB监控后台线程；HTTP AcceptLoop/请求任务；云控专用输入worker与目标mailbox；共享视频解码调度器；按设备脚本任务。任务数与线程数不一一对应。

## 3. 网络边界与契约

### 3.1 端口、方向与数据格式

| 边界           | 默认/范围                          | 格式                                             | 证据                                                |
| ------------ | ------------------------------ | ---------------------------------------------- | ------------------------------------------------- |
| 普通API网关      | `http://localhost:8080/`       | HTTP/JSON，ok/data                              | 云控 `0x06002f80/0xca1a1`、BuildBaseUrl `0x06002fac` |
| 普通每设备服务      | PC端口8001..8999                 | HTTP/JSON，URL已绑定serial                         | 云控Reserve `0x06003003/0xcd56c`                    |
| Automator网关  | `http://127.0.0.1:17171/`      | HTTP/JSON，code/message/data                    | Service ServerOptions `0x06000172/0x4372`；宿主可改    |
| Core到手机agent | PC随机空闲口 → 设备9008               | HTTP，经ADB forward                              | 云控Core `0x0600017b/0x5d14`                        |
| ADB客户端       | PC127.0.0.1:5037               | smart socket四位hex长度文本；payload编码因实现而异；sync二进制传输 | Core Forward/SendRequest，JiXingAdb                |
| 云控投屏服务       | 设备localabstract socket → PC动态口 | ADB reverse后音视频/控制流                            | Scrcpy `0x06000750/0x2e5e0`                       |
| 云控代理探测       | 配置远端；默认检查api.ipify.org         | HTTPS；代理配置为JSON或YAML                           | ProxyOptions `0x06000067/0x3b49`                  |

这些不是实测监听结果。调试版Core私有路由/端口尚有混淆缺口；9008契约以云控Core为适用范围。8080的`localhost`前缀和17171的loopback默认不能替代运行环境HTTP.sys权限/绑定验证。外网业务/更新/登录接口未形成完整已验证清单。

### 3.2 普通设备API

现状请求：StreamReader读取body，Newtonsoft反序列化；路由trim、补前导斜线、去尾斜线、lowerInvariant。响应UTF8、`application/json; charset=utf-8`。DTO声明字段如下，不额外假设严格大小写限制。

| 方法/地址范围  | 路径            | 请求                                     | 结果                                                                         |
| -------- | ------------- | -------------------------------------- | -------------------------------------------------------------------------- |
| GET 网关   | `/`、`/health` | 无                                      | `{ok:true,data:{port,runningCount,status}}`                                |
| GET 网关   | `/devices`    | 无                                      | 设备快照数组                                                                     |
| GET 单设备  | `/`、`/health` | 无                                      | serial/port/status                                                         |
| GET 单设备  | `/device`     | 无                                      | serial/name/brand/model/androidVersion/connectionType/groupId/port/baseUrl |
| POST 单设备 | `/shell`      | Command:string，UseRoot:bool            | output；按UseRoot选择ADB普通或root命令                                              |
| POST 单设备 | `/tap`        | X:int，Y:int                            | ADB input tap；ok                                                           |
| POST 单设备 | `/swipe`      | StartX/StartY/EndX/EndY/DurationMs:int | 非正DurationMs归一为300；ok                                                      |
| POST 单设备 | `/text`       | Text:string                            | AdbService.SendText；ok                                                     |
| POST 单设备 | `/app/start`  | PackageName、可选Activity                 | 指定activity启动或monkey launcher；output                                        |
| POST 单设备 | `/app/stop`   | PackageName                            | force-stop；output                                                          |

错误体为`{ok:false,message}`；已观察400/404/410分支，不保证所有坏JSON都落同一状态。端口分配器锁内排除已用/保留/冷却端口，耗尽返回失败，不能静默复用另一个serial的地址。锚点：云控 `0x06002fa8..0x06002fb8`。

### 3.3 Automator网关

支持GET health/devices与27个POST，路径允许`/api/`前缀。POST均含serial；首个动作经会话管理自动连接。没有对外connect/disconnect路由的证据。成功`{code:0,message:"ok",data:...}`；忽略null字段。未知路由404/code1006，不允许动词405/code1005；其余错误详见API报告，勿按名称猜异常分类。

| POST集合 | 路径/关键参数                                                                                                                                                                                                                            |
| ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 设备     | device/dump；device/screenshot(x,y,width,height,quality可空)；device/shell(command)；device/execute(command)；device/set-prop(prop,value)；device/set-timezone(timezone)；device/set-sim-info(operatorName,mcc,mnc,iccid,imsi,phoneNumber) |
| 应用     | app/list(showSystem=false,needVersion=true)；app/list-packages；app/start、app/stop、app/clear(packageName)                                                                                                                            |
| 输入     | input/tap(x,y)；input/swipe(x1,y1,x2,y2,steps=55,stepMs=5)；input/keyevent(code)                                                                                                                                                     |
| 服务/等待  | service/info；service/stop；function/sleep(seconds可空)；function/sleep-ms(milliseconds可空)                                                                                                                                              |
| 元素     | element/find、find-all(selector)；wait-exists、click-if-exists(selector,timeoutMs=3000,pollMs=200)；click-text(text,相同超时)；set-text(selector,text)；wait-any(selectors[],相同超时)；click-node(node)                                          |

完整DTO/结果表见 `API.md` 第4节。截图响应format=`png`、base64；不是二进制直接下载。Swipe的steps/stepMs不是普通API的DurationMs，两者不应共用同一序列化DTO。

### 3.4 Core的手机客户端契约

云控Core手机API与上述网关不同：Dump为GET `device/dump?compressed=...`；Shell为POST `device/cmd` body `{cmd}`；Screenshot为GET；节点set-text字段为`set_text`；app/start使用`package`。服务停止在该客户端为GET `service/stop`。完整表在API报告第5节。

HttpTransport默认15秒、每请求创建HttpClient、ConnectionClose、UTF8 JSON；解包JXResponse<T>。HTTP失败、空/坏JSON、code非0变成JXApiException。仅证实客户端行为，未穷举Android服务器校验。

### 3.5 ADB与投屏编码

ADB smart socket请求 = 四位十六进制字节长度文本 + payload；先读OKAY/FAIL。编码不能跨实现泛化：云控Core轻量客户端使用ASCII；JiXingAdb以UTF-8计算GetByteCount并编码请求（`.cctor 0x06000051/RVA0x3da8`、`FormAdbRequest 0x0600002e/RVA0x2b80`）。非ASCII命令/路径需按具体库验证。设备选择`host:transport:{serial}`；forward命令携带serial和两端口；sync使用SEND/DATA/DONE。JAR默认路径`/data/local/tmp/JXAutomator.jar`，云控入口类`com.jixing.automator.auto.Main`。

云控投屏启动类`com.jx.screen.Server`，默认远端路径`/data/local/tmp/screen`；用reverse连接。视频接收循环头为12字节，前8字节大端flag（bit63 codec config、bit62 keyframe），后4字节BE有符号payload长度。32字节触摸控制包布局详见shared-analysis，Position包含目标屏幕尺寸，不能仅发送x/y。此规格不冒充完整投屏握手协议。

## 4. 数据、算法与并发约束

**设备会话：** 按serial启动锁、双查缓存、引用计数；每设备SemaphoreSlim(1,1)串行化核心命令。连接使用已有agent/forward或校验SHA后部署；TCP端口可连后固定等待5秒返回，默认20秒启动窗口、1.5秒探测、200ms轮询。新实现若改为HTTP版本健康检查，应标记为改进，不是原行为。

**节点查询：** AndroidNode树含bounds/childNodeList和属性；前序递归、指定条件AND。可空bool/int表示不限制。字符串精确/contains区分大小写。WaitAny按选择器输入顺序依次查询，第一个存在者胜出，不是并行OR。依据Service `0x06000034/35`、Core `0x06000172/174`。

**输入同步：** source+target快照+generation+gestureId构成事件上下文。仅合并上下文相容的MOVE；非MOVE先冲刷待发MOVE再入FIFO。目标mailbox独立排队，同目标保序，跨目标并行。目标坐标按视频尺寸缩放并clamp；主控交接策略保留必要手势结束，不简化成“丢弃全部旧generation”。依据云控 `0x060031dd/031f7/03223/03227/0321b`。

**脚本结构：** ZIP内manifest.json/script.json；Manifest至少Id，另有Name/Version/Description/Entry/MinimumAppVersion/AppName/Category/AppDownloadUrl。Step含Action/DurationMs/Probability/Value；参数含Key/Type/LabelZh/LabelEn/Unit/Min/Max/Step/DefaultValue/Value。字段存在不证明完整语义：Probability的通用执行含义未确认。

通用动作仅确认launchapp/launch、wait/sleep、click、swipe；普通包执行一遍，特定应用脚本另有循环分支，不据此宣称任意代码执行引擎。调度字典`serial → CTS`，重复启动返回，不创建第二任务；停止/断线Cancel；finally仅删除属于本次CTS的条目。取消异常结束任务；其他异常记录后重新抛出。每设备内存日志最多500条。依据云控 `0x06003ef0/03f3f/03f42/03f59`。

## 5. 持久化与日志

云控AppConfig为46字段JSON，Load先主文件、再.bak、再默认并归一化。Save在锁中基于last-saved/current/disk合并本地修改，UTF8无BOM写.tmp；已有文件File.Replace并留.bak，首次Move。墙列数归一范围5..20。依据 `0x060004d6/004dc/0281a`。

SQLCore包装SQLite-net，初始化SQLitePCL，传路径/password/ticks。LocalDatabaseService有迁移、备份、完整性检查与无效库隔离；实体schema见 `evidence/cloud-schema.json`。本轮不读取行，不声称磁盘数据库加密已验证。语言还使用Language.Default.AppCulture的.NET Settings，并非全部仅在AppConfig中。

云控CommonCore日志库默认false/3，但宿主OnStartup覆盖为true/1。异步队列写文件；14天保留、5分钟清理检查、退出3秒等待；>=5MiB历史文件可gzip、总量200MiB尽力清理，受保护当日基准文件使其不是绝对上限。调试版CommonCore.WriteLine为ret；EXE自有logger才有可证的文件追加链；Scrcpy trace仅可空回调，无绑定不能断言落盘。

## 6. 错误与复刻验证要求

| 场景              | 必须验证的结果                       | 静态限制                      |
| --------------- | ----------------------------- | ------------------------- |
| ADB未启动/设备未授权/离线 | 不误报Ready，错误定位serial与阶段        | 未连接设备实测                   |
| 端口占用/分配耗尽       | 启动失败可见，不串台                    | 未监听/发送请求                  |
| 手机JAR不同版本/启动失败  | 能区分部署、TCP就绪与HTTP不兼容           | 当前原实现主要用TCP探测             |
| 取消/重复请求         | 同serial任务不重复；CTS/转发/缓冲释放      | 需竞争条件测试                   |
| 混合尺寸/旋转/主控切换    | 不错点，不产生卡住按下或跨设备误投             | 坐标公式已读，整链未实测              |
| 配置主文件损坏/磁盘写失败   | 备份或默认可恢复，保存失败可见               | FileUtil可返回false，调用方显示需验证 |
| 脚本坏包/未知动作       | 明确校验结果，无假成功                   | .NET ZIP及包校验未做模糊测试        |
| 恢复备份/代理失败       | 先展示影响范围；错误可定位；不承诺自动回滚         | 原样本含破坏性步骤，未执行             |
| 两程序同设备并用        | 检查JAR/forward/reverse/ADB重启冲突 | 目前为风险推导                   |
| 20台并发           | 分阶段记录帧率/延迟/资源/恢复指标            | 本次无性能实测                   |

最小验证顺序：离线格式/算法测试 → 单测试设备 → 5台 → 20台。原业务数据、账号/凭据与生产路由不进入验证样本。修改原程序、绕过授权、部署ROM或迁移数据库均不属于本次静态分析。
