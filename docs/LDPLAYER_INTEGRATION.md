# 雷电接口与功能可行性

## 官方接口路线

依据[雷电命令行说明（更新于2026-08-31）](https://help.ldmnq.com/docs/LDPlayer-ming-ling-hang-kai-fa-zhe-shi-yong-shuo-ming/n)及[ADB说明（更新于2026-09-03）](https://help.ldmnq.com/docs/lei-dian-mo-ni-qi-ADB-ming-ling-shi-yong-shuo-ming)。官方控制入口是安装目录中的 `ldconsole.exe` 和 ADB。本工程提供自己的localhost HTTP网关，不宣称是雷电官方REST API。游戏开放平台支付SDK不是模拟器控制接口。

## 已接入接口

普通网关默认8080，响应继续使用 `{ok:true,data:...}` / `{ok:false,message:...}`。

| 方法和路径 | 字段/行为 |
|---|---|
| GET /ldplayer/instances | 只读刷新list2；返回index/name/句柄/PID/Android启动标志/尺寸/generation |
| GET /ldplayer/capabilities | 支持表、确认要求和限制；不等于本机动态测试通过 |
| POST /ldplayer/instances/{index}/launch | expectedGeneration |
| POST /ldplayer/instances/{index}/quit | expectedGeneration、confirmed:true |
| POST /ldplayer/instances/{index}/reboot | expectedGeneration、confirmed:true |
| POST /ldplayer/instances/{index}/runapp | expectedGeneration、packageName |
| POST /ldplayer/instances/{index}/killapp | expectedGeneration、packageName |
| POST /ldplayer/instances/{index}/installapp | expectedGeneration、file：已存在APK的Windows绝对路径 |
| POST /ldplayer/instances/{index}/input | expectedGeneration、text；发送call.input，保留中文和空格 |
| POST /ldplayer/instances/{index}/keyevent | expectedGeneration、key：back/home/volumeup/volumedown之一 |

POST要求application/json，拒绝跨站Origin。仅监听loopback，不能直接开放到局域网或公网；本机接口依赖可信本机调用者，不是权限认证系统。用户接受设备动作风险后才在真实实例使用。

写操作必须使用最近GET返回的generation；服务在逐实例队列内重新查询，拒绝已观测到目录指纹变化后的旧代次。代次保持JavaScript安全整数，新服务使用新的epoch。返回accepted表示命令进程正常完成，不代表目标应用/输入框已实际成功。launch/quit只检查可观察的目录状态，不代表ADB、首帧或所有guest服务就绪；其余动作不把退出码0当设备效果验证。HTTP错误含无配置503、坏字段400、过期代次409、未支持501及进程超时504。

调用示例中的代次是占位值，必须替换为刚查询的值：

```json
{"expectedGeneration": 1, "text": "测试内容"}
```

## 功能矩阵

| 类别 | 当前处理 | 真实验证边界 |
|---|---|---|
| 实例枚举、尺寸、窗口句柄、PID | 新增CLI和HTTP，兼容7/10列 | 本机只读证实 |
| 启动、退出、重启、运行/停止/安装应用 | 已接入，退出/重启需确认 | 新空白实例直接CLI启动、HTTP运行/停止通过；HTTP冷启动/退出/重启/CLI安装未实测 |
| 中文文字、返回/主页/音量 | 已接入官方action | HTTP中文精确回读与Home通过；CLI Back/音量未实测 |
| 截图、点击/滑动、文件、应用列表、节点树、脚本 | managed现有ADB socket+授权串号+固定服务/transport身份 | 本轮独立实例实际通过，见docs/testing；动态视频节点不保证任意时刻可读，多设备仍未实测 |
| 实时视频与控制 | 用户提供scrcpy的独立进程 | 不等于雷电私有视频接口；未验证实际scrcpy |
| 创建/复制/重命名、资源配置、GPS/传感器 | 官方有能力，暂未开放本轮写入口 | 需补范围、约束与真实测试 |
| 删除/整机恢复、root、身份、网络改变 | 不自动开放或执行 | 用户风险接受后另行实现与验收 |
| 限CPU、扫码、窗口缩放、CLI文件传输、应用备份、宏脚本、全局默认配置、菜单键 | 当前官网标记暂不可用，不因help存在而启用 | ADB文件传输与自建脚本并非这些专有命令 |
| 原私有手机JAR、完整SIM、代理部署、商业登录 | 无等价雷电公开契约 | 不声称可完整还原 |

## 本机只读证据

以下为创建测试实例之前的历史时点，不能据此判断当前服务所有者。用户自行关闭极星后，本轮批准创建并保留实例1；当前managed保护及实机结果见testing/REPORT.md与API_MATRIX.md。

2026-10-14：安装路径 `E:\leidian\LDPlayer14\ldconsole.exe`，启动器14.0.29.0、多开器14.0.25.1。list2仅实例0，原始一行：

```text
0,雷电模拟器,4986608,1967810,1,49596,75704,1600,900,240
```

isrunning返回running，list3第三列却为0。本工程用list2的Android启动标志与PID，不以list3第三列判定离线。虚拟化目录名ldplayer9box不证明同时运行雷电9。

5037共享服务由极星的 `C:\Program Files\JiXingCloudControl\Resources\Adb\adb.exe` 持有。本轮没有执行另一adb客户端。仅通过现有服务的smart socket发送host:version和host:devices-l，协议版本41、两个注册串号 `emulator-5554` 与 `192.168.31.197:5555`。后者是本机地址；LAN/loopback两个5555连接的服务端均属于list2的VBox PID75704，因此是同实例别名候选，不是两台设备。未读取guest身份或直接获取ADB内部transport/socket映射。

## 缺失与矛盾处理

- list2当前中文文档10列、旧英文文档7列：支持两版，可选尺寸未知时保留null；标题含逗号按数值后缀解析。两版都成立，或数字后缀看似新版却只有旧版合法时保守拒绝，避免把坏尺寸当作标题并误读句柄/PID；这种旧版纯数字逗号标题需用户改名后查询。
- 原生输出本机是GBK：严格UTF-8解码优先，否则CP936；单独argv、不拼Shell、限输出和超时，仅终止本工程启动的命令进程。
- 99999桌面哨兵：排除，不能作为可控制实例。
- 5554串号标签与5555 TCP端口混用：不使用公式自动指定实例。`LdSerialBindings`仅保存用户显式选择的对应关系，本轮不跨串号自动派发CLI动作，也不把配置值当已验证的硬件身份。
- 退出码0与实际效果不同：区分accepted/verified，没有效果证据保留未验证。
- 内存demo与真实接口：demo会明确标示；离线测试不作为双实例硬件或20台性能证据。
- 外部同时修改实例：list2没有进程出生时间，未观测且全部PID/句柄等恰好复用的变化无法识别；重查与按index实际投递之间也存在外部变更窗口。未验收真实零误投，动态测试时应停止其他控制程序对测试实例的操作。
- 本机雷电14启动阶段实测10列运行标志2，后变1。只将这一已观测值视为未就绪（AndroidStarted=false），不推断官方通用状态枚举，不放宽其它未知值；保留窗口/PID以避免整目录刷新失败。真正可用仍需ADB/boot/首帧验证。

验收顺序与命令见 `VERIFICATION.md`，本轮实际结果见 `LDPLAYER_RESULTS.md`。
