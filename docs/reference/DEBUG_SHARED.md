# 调试工具 CommonCore / JXScrcpyCore 静态覆盖补充

> 开发参考资料，来自 2026-10-14 静态分析；不是已批准的首版建设规格，也不是当前运行结果。原始日志、CIL/IDA 导出、样本和资源中间文件已按用户清理要求移除。正文中的 token/RVA、evidence 路径仅保留为历史定位，不能当作现存文件；动态修正见 [已知问题](../KNOWN_ISSUES.md)，来源与可复核边界见 [来源说明](../SOURCES.md)。


日期：2026-10-14。仅分析 `D:\Program Files\极星API调试工具` 中两个库，先复制到 `../artifacts/debug/` 后交给 REA 和 IDA CLI。没有运行目标、连接设备或修改安装目录。本文与云控目录中同名库作构建级比较，不能把同名程序集视为同一版本。

## 1. 身份与覆盖

| 项目 | debug CommonCore.dll | debug JXScrcpyCore.dll |
|---|---|---|
| 文件大小 | 40,960 字节 | 243,200 字节 |
| SHA-256 | `c09361e3505ce6cb78f3129ee35c8ffcba7869dc5d44b1b45b1d28b8503d56d3` | `c8782091327035bf2419f6d87f7105d63697b712410afb8b5ce7b44e75a11058` |
| MVID | `38cd8735-8c9e-4539-77ce-059ef20dc844` | `84561d37-5615-4e33-91fb-5f3d42de400b` |
| 托管框架/程序集版本 | .NET Framework 4.8 / 1.0.0.0 | .NET Framework 4.8 / 1.0.0.0 |
| REA 类型/方法 | 48 / 219 | 145 / 689 |
| 有体/无体方法 | 212 / 7 | 639 / 50 |
| IDA 导出方法条目 | 219 | 689 |

REA 未报告方法体解码 issue；覆盖状态仍为 `partial`。无体方法包括接口等条目，数量不等于失败数量。静态阶段曾保存完整方法库存与每个有体方法的反汇编（现已清理）；本报告没有声称全部 908 个方法均恢复成源代码或逐一做过语义审计。

证据文件：

- `../evidence/debug-shared-rea.json`：身份 Common `ev_b22fdc5d30c2fa8e6f99b9997bf804f03271335a6a26d2012c94cf275d3ab5f3`；成员 Common `ev_77aaec1de8257cd6fad6ee7404c3f7ed4feb7d05c7905cbe4d6a9fabef95a482`；身份 Scrcpy `ev_0c5e9ddf0cc76f1e2acde94cb2329bdd767f6167e83a8bc95efca802a89d0b92`；成员 Scrcpy `ev_282506ed961bd4ab3000f3c498b635d882bd7458712e62beeea61f2a96613231`。
- `debug-common-ida-functions.json`、`debug-common-full.il.txt`、`debug-common-function-map.csv`；对应 `debug-scrcpy-*` 三个文件。历史上均位于 `../evidence/`，现已清理；JSON 的 `assembly` 是完整方法文本。
- 云控比较依据为 `common-method-index.json` / `common-ida-functions.json` 与 `scrcpy-method-index.json` / `scrcpy-ida-functions.json`。

**地址约定：**下表依次为元数据 MethodDef token、PE RVA、IDA CLI EA。CLI EA 是 IDA 的 CLI 处理器地址，不是运行中原生 VA；跨构建 token/RVA 均不能复用。多数私有名字和字符串被保护，描述性别名是分析标签。

## 2. CommonCore：ADB 门面与日志空实现

| 节点 | token / RVA / IDA CLI EA | 已观察的行为 |
|---|---|---|
| AdbService.StartService | `0600008c / 3F1C / 21f0` | 保存 adb 路径；调用 JXAdbCore.AdbServer.Instance.StartServer(path,false)；丢弃其 bool 返回值 |
| RestartService | `0600008d / 3F38 / 2210` | 再次调用 StartServer(path,false)；此方法本身没有显式 KillServer |
| Devices | `0600008e / 3F4C / 2230` | 设备枚举入口，返回门面模型供 EXE 使用 |
| Connect | `06000090 / 3FC8 / 2290` | ADB 连接入口；主 EXE 的地址扫描可到达它 |
| CreateForward | `06000095 / 40EC / 2350` | DeviceModel.DeviceData + 两个格式化端点 + bool → AdbClient.CreateForward；异常返回 false，成功返回 true |
| Push / Pull | `06000097 / 41B0 / 23e0`；`06000099 / 42CC / 24a0` | 文件传输门面；Push 另有 `06000098 / 423C / 2440` 重载 |
| ExecuteRemoteCommandAsync | `0600009a / 4350 / 24f0` | 异步 Shell 门面 |
| ExecuteRemoteCommand | `0600009e / 4440 / 25e0`；`0600009f / 4494 / 2620` | 同步 Shell 门面 |
| ExecuteRemoteCommandNoWait | `060000a3 / 4670 / 27c0` | 不等待返回的命令入口 |
| InstallFromDevice / InstallApk / InstallApkSet | `060000a7 / 471C / 2870`；`060000a8 / 47B4 / 2900`；`060000a9 / 480C / 2940` | 供 EXE 的应用安装流程调用 |
| LogUtil.WriteLine(string) | `060000d2 / 5528 / 35c0` | 方法体只有 `ret` |
| LogUtil.WriteLine(string,string) | `060000d3 / 552C / 35d0` | 第一参数空白时调用上述空实现，其他情况直接返回；同样无日志输出 |

调用关系是 `EXE command → CommonCore.AdbService → JXAdbCore`；此库不是另一套 HTTP API 服务。引用还包括 System.Drawing、SQLite-net 等，但“程序集引用存在”不足以断言具体业务数据库表或本轮主流程必经数据库。

**日志更正：**主 EXE 确有自己的文件日志器（EXE token `06000394` / `06000398`），所以整个程序不是完全无日志；但看到 CommonCore.LogUtil.WriteLine 调用不能算作日志已写出的证据。其行为与云控版本差异明显。

## 3. JXScrcpyCore：会话、传输、解码、输入

引用直接支持以下分层：`ScrcpySessionManager → ScrcpySession / 会话工作对象 → socket + 控制消息 + 视频/音频解码 → WPF 显示 / NAudio 播放`。FFmpeg.AutoGen、SharpDX.DXGI、SharpDX.Direct3D9、PresentationCore、WindowsBase、NAudio.Core/WinMM 均在引用表。资源包含 `JXScrcpyCore.Resources.scrcpy-server.jar`。这证明库携带设备端资源，不证明设备上本次已部署或启动它。

| 节点 | token / RVA / IDA CLI EA | 输入、输出与依赖 |
|---|---|---|
| ScrcpyOptions..ctor | `06000188 / 9778 / 7cb0` | 默认 ADB port=5037、MaxSize=180、BitRateKbps 原始值=10,000,000、MaxFps=25、ControlEnabled=true、ConnectTimeout=5秒。EXE 会覆盖部分默认值 |
| ScrcpyOptions.get_Parameters | `060001a5 / 993C / 7f10` | Version + 控制/音频开关 + BitRateKbps/MaxSize/MaxFps 等组成字符串列表后 Join；保护字符串未还原，参数键和单位不能凭名称猜定 |
| SessionManager.Initialize | `06000218 / D064 / b1b0` | BaseDirectory 拼接受保护的两级路径设置 FFmpeg.RootPath；保存/转交 UI Dispatcher；调用初始化帮助函数 |
| SessionManager.ConnectAsync | `0600021a / D0CC / b220` | serial → ConcurrentDictionary.GetOrAdd → 每设备对象的连接方法(options,cancellationToken)；返回 Task&lt;ConnectResult&gt; |
| SessionManager.DisconnectAsync | `0600021b / D0F0 / b240` | serial + token → 异步状态机 |
| SessionManager.UpdateOptionsAsync | `0600021c / D144 / b290`；`0600021d / D150 / b2a0` | 更新指定会话选项的两个入口 |
| ScrcpySession.SendTouchEvent | `060001bd / 9DCC / 8440` | Position + AndroidMotionEventAction + token → 异步发送入口 |
| ScrcpySession.SendKeycodeEvent | `060001bf / 9E84 / 84e0` | AndroidKeycode + AndroidKeyEventAction + AndroidMetastate + token → 异步发送入口 |
| 触控消息序列化器（描述性别名） | `06000084 / 49B4 / 2a20` | 验证 Position.ScreenSize，生成固定 32 字节大端序控制消息，见下表 |
| ScrcpySocketAcceptor 状态机 | `06000276 / 10B10 / e990` | 接受多个 TcpClient，读取模型/宽高等握手信息，构造 ScrcpyClients(video,control,audio,model,width,height)；错误路径关闭客户端 |
| ScrcpyClients | `06000278 / 110C8 / ee90` | 聚合 Video、Control、Audio 连接以及 Model、VideoWidth、VideoHeight |
| AacDecoder.Configure / Decode | `06000116 / 89B8 / 69c0`；`06000117 / 8AA8 / 6ab0` | AAC 解码入口；结合 FFmpeg 引用和调用确认音频解码分支存在 |
| PcmPlayer.StartIfNeeded / Push | `060002a4 / 12F7C / 10ce0`；`060002a5 / 13058 / 10db0` | PCM 播放启用与数据输入接口 |

并发模型已观察到 UI Dispatcher、ConcurrentDictionary、Task/async 状态机、取消令牌以及视频/控制/音频分流。静态代码不能给出实际线程数、单机 20 台吞吐量或延迟。默认 bitrate 字段名与其原始值值得核查；在确定服务端参数语义前不能把字段名中的 Kbps 当作已证实单位。

### 3.1 可直接复刻的触控消息结构

依据 `06000084` 的 Span.Slice 与 BinaryPrimitives.Write*BigEndian 调用。Position 或 ScreenSize 不可用、宽高为零会抛 InvalidOperationException。

| 偏移 | 长度 | 字段 |
|---|---|---|
| 0 | 1 | 消息类型枚举 getter 的值（本表未推定数值） |
| 1 | 1 | AndroidMotionEventAction |
| 2 | 8 | 指针 ID，uint64 大端 |
| 10 | 4 | X 从 double 转 int32，大端 |
| 14 | 4 | Y 从 double 转 int32，大端 |
| 18 | 2 | 屏幕宽 uint16，大端 |
| 20 | 2 | 屏幕高 uint16，大端 |
| 22 | 2 | 压力编码函数(float32) 返回的 uint16，大端；action=1或3输入0.0，否则1.0 |
| 24 | 4 | 固定值1，uint32大端 |
| 28 | 4 | action=1或3时0，否则使用对象的枚举 getter，uint32大端 |

这里恢复了编码端字段布局；消息类型值、压力转换细节、服务端接受约束仍须用相应枚举/帮助方法及设备端验证，不能据此声称整个 scrcpy 协议已完整复刻。

### 3.2 Scrcpy 诊断回调与真实文件日志不同

- `ScrcpyCoreTrace.EnsureFFmpegLogBridge`，`0600026a / 10438 / e2e0`：Interlocked.Exchange 确保一次注册；av_log_set_level(40)；av_log_set_callback。
- FFmpeg 回调 `0600026c / 104B0 / e340`：1024 字节栈缓冲，av_log_format_line2，ANSI 字符串 Trim，过滤后调用 WriteLine。过滤关键词在保护字符串中，未恢复。
- `ScrcpyCoreTrace.WriteLine`，`0600026b / 10478 / e320`：读取可空 Action&lt;string,string&gt; Writer；非空则 Invoke(tag,message)；异常被 catch 吞掉。它本身没有写文件。
- .cctor `06000267 / 10414 / e2a0` 只保存 FFmpeg 回调委托，未给 Writer 赋默认输出。主 EXE 全方法 CIL 中未检出 `ScrcpyCoreTrace::set_Writer` 调用。因此本轮不能证明该回调已被应用接入持久日志，也不能把未观察到的注册排除到运行时反射等未知路径之外。

## 4. 与云控同名库的差异

| 比较项 | 调试工具构建 | 云控构建 |
|---|---|---|
| CommonCore SHA | `c09361e3…3d56d3` | `ced33722b69c3cbc6dbb91f54118100c69277ee1d06f2a6741055fd4889bf419` |
| CommonCore 方法数 | 219 | 424 |
| CommonCore 日志 | 两个 WriteLine 均无实际输出 | WriteLine → WriteCore / WriteCoreLazy；队列、后台处理、WriteEntryToDisk、ShutdownAndFlush、压缩/清理实现 |
| ScrcpyCore SHA | `c8782091…11058` | `6faa3a6123f460db84397bd9875002f9487e432bbf20fb2d9bd56b435ecda2fe` |
| ScrcpyCore 方法数 | 689 | 1925 |
| 会话入口 | SessionManager + ScrcpySession，含视频/音频/控制通道 | 同名会话接口仍存在，但方法坐标和实现均不应复用 |
| 解码/资源诊断 | 有 FFmpeg/AAC/PCM、SharpDX.DXGI/Direct3D9 引用 | 另有明确命名的 D3D11MemoryPhaseProbe、ExperimentalD3D11VaDecoderPool/Lease/Shard 及资源快照方法 |

云控差异的具体锚点：CommonCore.WriteCore `06000029 / 263c / 690`；WriteEntryToDisk `06000035 / 2b78 / b70`；ShutdownAndFlush `06000037 / 2cf4 / cb0`；CompressLogFile `06000049 / 357c / 14b0`。云控 ScrcpyCore 的 D3D11MemoryPhaseProbe.RegisterSession `06000147 / 9ad8 / 7850`、ExperimentalD3D11VaDecoderPool.TryAcquire `06000153 / 9dfc / 7b80`、ScrcpyCoreTrace.BuildD3D11ResourceDiagnosticsSnapshot `06000139 / 9434 / 7210`。

**推导：**两套产品共享组件命名、领域模型及相近的分层，但安装物来自不同构建，云控库有更丰富的日志和多会话图形资源管理表面。不能仅依据方法数宣布发布时间先后，也不能断言云控特定实验分支在默认配置启用。复刻实现可共享自建 ADB/scrcpy 抽象；不建议直接互换原产品同名 DLL。

## 5. 控制流伪代码

```text
// debug CommonCore 0600008c
StartAdbService(adbPath):
    this.AdbPath = adbPath
    discard AdbServer.Instance.StartServer(adbPath, false)

// debug CommonCore 06000095
CreateForward(device, localPort, remotePort, allowRebind):
    try:
        local = ProtectedFormat(localPort)
        remote = ProtectedFormat(remotePort)
        discard AdbClient.CreateForward(device.DeviceData, local, remote, allowRebind)
        return true
    catch:
        return false

// debug ScrcpyCore 0600021a
ConnectAsync(serial, options, cancellationToken):
    worker = workers.GetOrAdd(serial, CreateDeviceWorker)
    return worker.ConnectAsync(options, cancellationToken)

// debug ScrcpyCore 06000084；类型值和压力编码调用保留明确依赖
EncodeTouch(message):
    require message.position.screenSize != null
    require width != 0 and height != 0
    released = message.action in {1, 3}
    packet = byte[32]
    packet[0] = message.type
    packet[1] = byte(message.action)
    WriteBE64(packet, 2, message.pointerId)
    WriteBE32(packet, 10, int(message.position.x))
    WriteBE32(packet, 14, int(message.position.y))
    WriteBE16(packet, 18, width)
    WriteBE16(packet, 20, height)
    WriteBE16(packet, 22, EncodePressure(released ? 0.0 : 1.0))
    WriteBE32(packet, 24, 1)
    WriteBE32(packet, 28, released ? 0 : message.buttonsEnum)
    return packet

// debug ScrcpyCore 0600026b；这里不承诺日志持久化
WriteTrace(tag, message):
    try:
        if Writer != null: Writer(tag, message)
    catch:
        pass
```

## 6. 验证与未完成边界

已完成：复制件 SHA/身份记录；REA 成员库存；IDA CLI 完成自动分析并导出全部方法条目；token/RVA/EA 映射；选定关键调用逐条核查；与云控同名库比较；纠正主 EXE 报告把日志调用等同于实际输出的风险。自己的两个 IDA lease 均已关闭，未关闭父分析实例。

未完成：保护字符串的系统性还原、所有消息类型/设备端 JAR 语义重构、运行时协议抓包、FFmpeg/SharpDX 原生依赖反汇编、20 台设备容量测试。上述限制影响“逐字节互操作/性能保证”，不影响本文已附坐标的静态代码事实。
