# 多实例测试与调整记录

文档版本：doc-v20261014.3；日期：2026-10-14；当前阶段：正式正常导航15分钟直连压力通过，最终独立审证及静置资源观察。

## 只读准备

- 读取README、已有测试handoff、核心LD/ADB/API/脚本实现；不读取CODEBRAIN，不操作原实例0 guest。
- 三位只读子代理核复用边界、官方型号文档、HTTP Toolkit启动风险与最小验证。现有按index/serial队列和一个MatrixRuntime可复用，不新建业务多开控制器。
- 本机i5-14400F、10核心/16逻辑处理器，可用物理内存约31.84GiB，初检空闲约5.8GiB且波动。两实例后不足新增第三台所需5GiB门槛，没有加第三台，也没用原0冒充压力实例。

## 程序门禁修复

- 新用例先复现68/69：A脚本使B雷电写API也409。修复API按实际验证的唯一LD串号绑定判断脚本状态，未知/过期绑定保守全局阻断，UI Busy仍保留全局保护。
- 初次复验的demo实例B默认未启动，调整离线前置：先启动B，明确注入fixture映射测HTTP门禁；该夹具不冒充真实Runtime自动绑定。最终实机cross-instance-LD-api补证Runtime绑定：A脚本Running/A409、B200且Home前台效果成立。
- 当前完整回归71/71、0警告0错误，独立复跑71/71退出0；覆盖两类脚本格式及已观测启动flag2。离线偶发的TCP owner=0保护拒绝留日志，不绕过保护；另一次FakeAdbServer关闭Accept发生ObjectDisposed，按stop已取消条件修复夹具析构，不吞其它错误。

## 型号与启动

- 创建空白实例2只add一次，exit2，但清单恰新增唯一名称/索引2，按差集保存CREATED-WITH-NONZERO-EXIT，不复制、不重复add。
- 原1冷态设置Pixel 8/Google；新2冷态设置SM-S9180/samsung，资源配置CPU2/2048MiB。未请求IMEI/IMSI/MAC/Android ID/root等字段，也未读取这些敏感标识，不能据未传参数证明SDK绝无联动。
- prepare-round1冷态设置已成功，launch等待因list2暂时非法失败；保存日志。prepare-round2记录原始SDK输出，实测10列启动标志2，后变1。原Parser将2拒绝；本轮只将已观测10列2视为未就绪，不放宽3或任意未知值，也不把2标Ready。
- 严格有效目录、Android boot=1、TCP实际反向所有者、PID/出生时刻、实际首帧与型号回读全部验证。prepare-round2六组通过，原0两PID/共享server保持；1 transport6/VBox58700，2 transport7/VBox49164。
- 型号通过依据getprop与DeviceInfo.Model，不是CLI退出码0；只证明型号字符串，不改变CPU/ABI/物理硬件。

## 预检失败与复验

- smoke-round1：两安装/启停验证组通过且区间重叠；B测试脚本swipe Value只有四数，原契约要求第五时长。B失败后取消A，A Script incomplete是次生取消。修五数并抽取SettingsScript工厂，两role离线Validate。
- smoke-round2：A round0通过，B滑动后仍点击滑动前坐标，文字回读不一致。改B脚本仅wait/swipe/wait，重新dump定位搜索及编辑框后才发文字；A也确认真实编辑框，保存输入节点。
- smoke-round3：跨实例门禁HTTP已经A409/B200，但Home验证写死launcher且过早观察，报告失败。改解析真实HOME组件、等待实际resumed匹配并保存诊断，不把accepted当效果。
- smoke-round4整体exit0：pressure68069ms；A5/B4轮、12条PASS/0FAIL，9条设备数据回传。独立哈希/nonce/SHA/时间区间审计通过；29条HTTP含1条预期409、其它28成功。proxyPort0，不计HTTP Toolkit抓包。

## HTTP Toolkit安全暂停

- 已装1.27.2。默认启动会发现Android并可能启动ADB，不直接默认使用；专用进程ANDROID_ADB_SERVER_PORT=65534、ANDROID_HOME=不存在SDK路径、ADB_SERVER_SOCKET隔离。5037前后仍PID44528/同出生时刻/协议41；未更改全局环境、代理或信任证书。
- 官方MCP initialize/tools/list及免费只读proxy_get-config已实际成功，原始结果保存；未调用付费或升级操作，不绕过Pro，也不伪造HAR。
- 实测代理8000监听::，Public/Private防火墙关闭。仅端口规则不能保证隔离，已向用户更新风险；未获当前网络范围确认前停止自有HTTP Toolkit主进程及已核父链server，记录暂停回执，未更改系统防火墙。
- 正式程序控制流量暂用直连loopback，本轮不宣称已完成抓包或抖音自身TLS捕获。ADB SYNC/exec的文件/APK传输不是HTTP，另以字节哈希和安装/版本验证。

## 正式压力阶段

- pressure-final-direct使用同一个运行时和两个串行worker；台间并行。A每轮8MiB/点击脚本/4秒think-time；B256KiB/滑动脚本/2秒think-time；每台独立marker与nonce，guest路径相同但SHA不同，上传、pull、shell回读及HTTP回传四方对照。
- 先各实例安装/启停给定APP，再barrier开始完整15分钟压力时钟，安装时间不充数。每10秒记录系统CPU/可用物理及commit、两台VBox/dnplayer与测试进程内存/句柄/CPU累计；GPU未测。
- 每轮日志含目标serial/index/transport、起止时间、结果/哈希、请求ID与脚本日志。最终摘要必须监控结束及最后身份核验后才能写PASS；身份/阈值/业务失败取消自有任务，不关原0或重启ADB。

## 正式轮失败及最小调整

- pressure-final-direct退出1，65条PASS/2条FAIL，62次完整回传(A44/B18)。B round18在`am start -W -a android.settings.SETTINGS`等待300201ms超时；A round44因共同取消而失败。完整压力只有602156ms，B最后300秒没有新进展，不计15分钟双实例稳定通过，不与新轮拼接。
- 独立审查确认传输deadline正常触发，未发现本地无限socket等待的证据。高频force-stop设置/搜索服务后等待冷启动不贴近正常用户；仅能定位这条启动等待失败，不能据此认定Android平台根因已修复。
- 新增只读diagnose阶段，仍要求冻结实例实际TCP所有者与transport。cold-start-diagnostics的8份主机授权guest诊断均返回：两个guest无boot以来ANR；B仍有设置启动Splash窗口/活动记录，但进程列表没有settings，不能把活动记录单独当就绪证明。不采账号、标识、整机logcat或原0数据。
- 只改测试驱动导航：主页返回、无`-W`的CLEAR_TOP设置入口、不再每轮force-stop；确认实际前台与fresh搜索/空编辑框，再执行点击/滑动和文字。整个UI步骤共享45秒取消预算，HTTP文字也受该预算约束；begin/前台/输入阶段日志落盘。不自动重试写入、不放宽SHA/nonce/节点校验。
- 调整后全构建0警告0错误，离线71/71；先1分钟完整预检，再独立15分钟正式轮。失败轮、诊断和每次build日志全部保留。
- smoke-normal-navigation第一轮因新增“空框”校验失败；input-diagnostics实际节点和PNG证明空框灰色hint为`搜索…`，uiautomator把hint写入Text，并非旧输入残留。只接受实测的搜索服务/AutoCompleteTextView/资源ID且空或这个精确hint，保留本轮最终全文精确回读；新增离线反例拒绝旧Round文字和其它package。最新完整72/72、0警告0错误。
- 前台检查改45秒budget内轮询，不重复启动；readiness必须节点确认，Splash或活动记录本身不足。smoke-normal-navigation-round2先验证修正策略，后续正式轮从零独立计时。

## 独立界面复验

- 使用现有Desktop.dll在1000x700与1440x900分别运行demo smoke，均exit0/error为空；12页/3视图/中文/筛选上下文/脚本完成/忙态与离线禁用断言通过。两张首页无明显重叠或空白，小窗滚动正常。
- 这只证明demo布局及控件状态，不冒充所有控件的实机业务覆盖。证据为verifier-ui-small与verifier-ui-desktop；该桌面二进制哈希8F6E17CAC628835EE78274DE5CBB4FEDEB7E5E466B76A01236DFC448B5541A63。

## 正式通过轮

- pressure-normal-final-direct session58073退出0，elapsed979025ms、安装后pressure906113ms；A68/B58、129PASS/0FAIL、126设备回传。380条HTTP=379成功200+1预期同实例忙409，全proxy0。
- 每台完整回传最长间隔A13462.6ms/B16079.7ms，持续差异任务并行，未复现旧-W停滞。两台上传544MiB/14.5MiB并pull同量SHA校验；每轮中位/P95分别A9286/9415ms、B13579/13895ms。
- 只读分析工具要求本轮spec SHA/最小时长/目标数，并核case SHA、return/HTTP文字与shell nonce/SHA/ack及摘要计数；短预检实跑成功，旧失败轮缺summary按预期拒绝。正式analysis已生成；4份当前程序集/spec另由独立verifier核，不由实现者自批准。
- 压力90资源样本CPU中位19.40/P95 21.50/最大25.13%，最低可用2.758GiB，最大commit67.27%。VBox1句柄3007→3810，VBox2 2559→3274，列WATCH；停止驱动后做3分钟仅主机进程静置采样，不guest操作，不自动重启平台。
- 新双实例入口与私有配置静态核验通过，禁止0/99999/重复索引别名，默认API/自动连接/输入同步关闭；未在压力运行时启动UI。所有PS脚本Parser无错误；11份参考文档与原桌面目录SHA仍一致。
- 独立正式审证限域通过：129 case/4 DLL/spec、126文字/shell nonce/SHA/ack配对、末轮实际文件与节点一致；99对任务区间重叠552354ms。全部GUI实机、HT及全天容量没有获批通过。
- idle-resources session90935退出0，180125ms/7样本：VBox句柄3809→3811及3275→3277，未回落，保留WATCH。首次临时统计命令foreach直接接pipe发生ParserError，改集合赋值再pipe后成功，仅分析命令无guest副作用。
- 静置后多私有配置CLI bootstrap退出0，两个Ready/Model正确、discovery/LD error空；host-after固定共享5037身份/原0PID，并确认自有测试端口全部释放。未启动双实例真实WPF，静态入口校验与CLI启动补证不得冒充GUI实跑。
