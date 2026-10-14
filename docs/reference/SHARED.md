# 云控的投屏、代理、日志与备份模块

> 开发参考资料，来自 2026-10-14 静态分析；不是已批准的首版建设规格，也不是当前运行结果。原始日志、CIL/IDA 导出、样本和资源中间文件已按用户清理要求移除。正文中的 token/RVA、evidence 路径仅保留为历史定位，不能当作现存文件；动态修正见 [已知问题](../KNOWN_ISSUES.md)，来源与可复核边界见 [来源说明](../SOURCES.md)。

分析范围：云控安装目录中的 `JXScrcpyCore.dll`、`ProxyCore.dll`、`CommonCore.dll`、`JXAPPCK.dll`。采用 REA 静态元数据/CIL 与 IDA CLI 交叉核对；没有启动这些程序集或执行其中的设备操作。调试工具的同名 DLL 另见补充报告，不能套用本节。

## 证据坐标与覆盖

| 模块           | IDA MethodDef 条目 | REA 成员证据 ID                                                           | 主要产物                                             |
| ------------ | ---------------: | --------------------------------------------------------------------- | ------------------------------------------------ |
| JXScrcpyCore |             1925 | `ev_70d2b091c1a454af9378bb311eb68d6e386f6728d650141b02fc3ed928522086` | `scrcpy-ida-functions.json`、`scrcpy-full.il.txt` |
| ProxyCore    |              330 | `ev_9f0b10d0b394cc4496454ba8ccf5d1b083d3de8b39ba831185311ec3a4158c50` | `proxy-ida-functions.json`、`proxy-full.il.txt`   |
| CommonCore   |              424 | `ev_6c8751d6f4c9fad7e2ac24a2996c727f3c7da3f08e8ae94daacc9ec94e9e4a7a` | `common-ida-functions.json`、`common-full.il.txt` |
| JXAPPCK      |              132 | `ev_2ceae6015dca8e5be7303bf6404534801be5eb6a73ec2c324b521c70c9a82135` | `appck-ida-functions.json`、`appck-full.il.txt`   |

以上文件历史上位于 `../evidence/`，REA 原始记录为 `root-rea.json`，现均已清理。`*-method-index.json` 包含 SHA256、token、PE RVA、IDA EA、签名和方法体尺寸。REA MethodDef 表顺序与 IDA CLI 函数顺序逐项比较，四个模块的名称匹配全部通过（含 IDA 自动添加的重载后缀）。下列 `EA` 仅指 IDA 合成地址，不是进程内 VA。条目数包括接口/抽象方法与 async 状态机。

## 1. 投屏连接与运行模型

`ScrcpySessionManager → SessionWorker → AdbLiteClient/ScrcpySocketAcceptor → ScrcpySession → 共享解码调度器 → FFmpeg/Direct3D11 → WPF Dispatcher` 是可观察的分层。会话按设备建立，接收、解码、控制写入使用异步任务和队列；不能将一个 session 等同于一个 Windows 进程或固定一条线程。

| 节点                             | token / RVA / EA                 | 输入输出与作用                                                                                                                            |
| ------------------------------ | -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| ScrcpyOptions 构造               | `0x060004e8 / 0x20590 / 0x1d5a0` | 默认 ADB=127.0.0.1:5037；MaxSize=180、MaxFps=25；ControlEnabled=true；连接超时5s；Version="3.3.4"；ScrcpyPath="/data/local/tmp/screen"；宿主会覆盖参数 |
| ConnectCoreAsync 状态机           | `0x06000752 / 0x2ef94 / 0x2c4c0` | serial/options → 检查设备在线 → 建立客户端 → new ScrcpySession → 订阅断线事件；失败清理客户端及 reverse mapping                                              |
| ConnectClientsOnlyAsync 状态机    | `0x06000750 / 0x2e5e0 / 0x2bb90` | 选PC端口；清理旧 reverse；建立 `localabstract:{socket} → tcp:{port}`；推送嵌入资源；启动服务；按选项接受视频/音频/控制连接                                             |
| StartServiceAsync 状态机          | `0x06000694 / 0x2b7d0 / 0x28970` | 对指定 serial 发起 ADB transport/shell；拼接 CLASSPATH 与 app_process 命令；返回服务生命周期 Task                                                      |
| VideoLoopAsync 状态机             | `0x060005e2 / 0x249c8 / 0x21fb0` | 读取二进制帧头与payload，管理缓冲池、配置包/关键帧和解码mailbox；断线发事件                                                                                      |
| TouchEventControlMessage.Bytes | `0x0600058a / 0x2125c / 0x1ea80` | Action/PointerId/Position/Buttons → 32字节控制包；非法ScreenSize报错                                                                         |
| Position.ToBytes               | `0x06000519 / 0x20d24 / 0x1de30` | 点和屏幕尺寸 → 12字节大端布局                                                                                                                  |
| TryCoalesceTouchMoveOnEnqueue  | `0x06000066 / 0x3e44 / 0x1df0`   | 可合并MOVE请求 → 原子替换尚未发送的同一队列尾占位；遇中间事件不合并                                                                                              |

实际启动类为 `com.jx.screen.Server`，不是仅凭 scrcpy 名称假定的官方类。连接链直接出现该字符串。程序集同时嵌入 `JXScrcpyCore.Resources.scrcpy-server.jar`（90980字节）与 `...Resources.screen`（90820字节）；`Version="3.3.4"` 是选项常量，不足以证明整个DLL或设备端实现等同于官方该版本。

投屏链使用 **ADB reverse** 将设备抽象socket连接到PC监听口；自动化 HTTP 核心使用 **ADB forward** 将PC端口连到设备9008。两者方向不同。投屏端口是运行时选择值；`scrcpy` 为socket名字段，不能写成TCP端口。

### 视频帧格式（已完成连接握手之后）

`VideoLoopAsync` 先准确读取12字节，再按长度读取payload：

| 偏移 |     长度 | 编码与含义                                                               |
| -: | -----: | ------------------------------------------------------------------- |
|  0 |      8 | 大端 uint64；bit63 表示 codec config，bit62 表示 key frame；低位的时间戳语义未在本轮单独验证 |
|  8 |      4 | 大端 int32 payload length；本路径跳过非正长度                                   |
| 12 | length | 编码后的视频包，交给FFmpeg解码路径                                                |

这不是完整连接握手格式；设备名、codec/分辨率初始化及音频握手仍需结合 `ScrcpySocketAcceptor` 全量IL继续验证。接收循环包含缓冲池、活动状态下的解码门控、丢包后寻找下一关键帧、暂存codec config、向 `SharedVideoDecodeMailbox.TrySubmitOwnedNonBlocking` 移交缓冲所有权。由这些机制可以解释“缩略图多设备调度”的实现意图，但不能从代码推算20台实测帧率。

### 触摸控制包

|      偏移 | 长度 | 字段                                  |
| ------: | -: | ----------------------------------- |
|       0 |  1 | type=2                              |
|       1 |  1 | action                              |
|       2 |  8 | pointerId，大端                        |
| 10 / 14 | 各4 | x / y，大端int32                       |
| 18 / 20 | 各2 | screenWidth / screenHeight，大端uint16 |
|      22 |  2 | pressure，经Pressure01量化              |
|      24 |  4 | actionButton；本兼容实现写1                |
|      28 |  4 | buttons，大端uint32                    |

此兼容方法在UP(1)/CANCEL(3)时将压力与buttons置0，其他动作使用压力1和传入buttons；不是所有事件类型共用该包长。

## 2. 代理模块

`ProxyProfileParser → ProxyService.Resolve → Socks5Runner/VlessRunner/Hysteria2Runner → ProxyRunnerBase → AdbLiteClient`。这是设备端部署与路由配置能力；存在实现代码不等于任意ROM都支持，也不等于宣传的检测结果已验证。

关键数据：`ProxyPayload {Profile, Serial}`；`ProxyProfile {ProxyType, Host, Port, RawUrl}`；Socks5Profile还含User/Password/Area；Hysteria2Profile含Sni/Password/Insecure。报告不读取用户现有代理凭据。

`JXProxyCoreOptions..ctor`（`0x06000067 / 0x3b49 / EA 0x1ca0`）默认 ADB5037、工作目录 `/data/local/tmp`、LocalSocksPort=10808、PingUrl=`https://api.ipify.org`、PingTimeoutMs=8000；这些是库默认值，不能一概当作宿主最终配置。Hysteria模板生成函数 `0x06000014 / 0x2494` 返回YAML；Xray SOCKS5生成函数 `0x06000019 / 0x25f0` 返回JSON模板替换结果。

`ProxyRunnerBase.Start`（`0x060000ea / 0x4fb0`；实际状态机EA `0x7710`）顺序可证：

1. EnsureRootAsync；无ADB root或su抛RootNotAvailableException（`0x06000111 / 0x63c4`）。
2. Stop旧实例，清理旧配置；探测UDP支持。
3. BuildConfig → WriteRemoteTextAsync。
4. 遍历BinarySpec，EnsureBinaryAsync；资源内含xray、hysteria及geoip/geosite。
5. ShellPrivilegedNoWaitAsync启动；逐条执行对应路由规则；检查进程是否存在。
6. 返回ServiceResult成功或错误码；进程不存在时失败，不能只凭命令已发送显示成功。

嵌入文件合计解释了ProxyCore接近80MB的体积：hysteria20027168字节、xray30343485字节、geoip.dat20105462字节、geosite.dat9368012字节。它们在本轮仅做资源盘点，未把第三方代理核心完整反编译。

## 3. 日志与公共服务

CommonCore含 `AdbService` 设备操作门面、DeviceModel/AppInfoModel，以及 `LogUtil`。云控主程序通常经AdbService连接JiXingAdb，API核心/投屏模块另有轻量ADB实现；不能把所有ADB流量归为一个类。

`LogUtil..cctor`（`0x06000017 / 0x24e4`）初始化 `BlockingCollection<LogWriteEntry>(ConcurrentQueue)`、14天保留期、5分钟清理间隔、3秒退出等待；库内MinimumLevel=3，EnableFileLogging=false。最终是否开启取决于宿主设置。

`WriteEntryToDisk`（`0x06000035 / 0x2b78`）准备延迟消息、计算UTF8长度、在锁内建目录/清理/选文件并AppendAllText。`ShutdownAndFlush`（`0x06000037 / 0x2cf4`）Interlocked保证只关闭一次，CompleteAdding、限时等writer、尝试排空。`CompressOversizedLogs`（`0x06000048 / 0x3500`）压缩达到5MiB的非当日基准日志；`CleanupLogsByTotalSize`（`0x0600004d / 0x36ac`）按时间清理至200MiB以内并跳过当日两个基准文件，同日轮转文件仍可清理，因此200MiB并非绝对硬上限。`ResolveWritableLogFilePath`（`0x06000047 / 0x3434`）以现有大小加本条UTF8字节数超过10MiB决定轮转；超大单条等情形仍可超出阈值。

## 4. 应用资料备份

JXAPPCK可证能力：Backup、Restore、ClearAppData、Import、Export、Delete；以AppBackupContext聚合Device/AppInfo/Options，Planner决定路径、Gateway封装ADB和文件操作，结果对象携带Status/Success/Message/ErrorKind/Record/OutputPath。

`AppBackupService.Backup`（EA `0x180`）先检查root和路径集，在设备端tar打包并gzip压缩，检查命令退出标记及文件非空，得到备份记录。`Restore`（EA `0x550`）检查归档、停止应用、清数据、解压、恢复owner及文件安全上下文，报告进度。**Restore包含清除现有应用数据的行为**；本次没有执行。Import有 `ValidateImportedArchive` 验证路径；并不因此证明任意恶意归档均已安全处理。

`AppBackupOptions..ctor`（EA `0x1ad0`）RequireRoot=true、ValidateImportedArchive=true、RestoreGoogleAccountManagerData=true、ArchiveExtension=".tar.gz"。这些敏感恢复选项只在规格中记录能力与风险，不读取或导出实际账号数据。全量token/RVA见 `appck-method-index.json`。

## 5. 重构伪代码

以下省略编译器状态机，保留资源所有权与分支。为便于复刻而采用语义名称，不是原始源码。

```text
async ConnectScreen(serial, options, ct):
    if not await adb.CheckDeviceOnline(serial, ct):
        return recoverable_failure("device not online")
    clients = await ConnectScreenClients(serial, options, ct)
    if clients == null: return recoverable_failure("clients unavailable")
    try:
        session = ScrcpySession(serial, options, clients)
        session.VideoStreamDisconnected += schedule_reconnect
        return success(session)
    catch:
        try: await stop_and_wait(clients)
        catch: pass  # 原实现清理尽力而为，不覆盖最初的连接异常
        try: dispose(clients)
        catch: pass
        try: await adb.RemoveReverse(serial, "localabstract:" + options.SocketName)
        catch: pass
        rethrow_original_exception()

async ConnectScreenClients(serial, options, ct):
    await serialConnectLock.WaitAsync(ct)
    reverseCreated = false
    reverseOwnershipTransferred = false
    try:
        port = choose_available_pc_port()
        remote = "localabstract:" + options.SocketName
        await adb.RemoveReverse(serial, remote)
        await adb.Reverse(serial, remote, "tcp:" + port)
        reverseCreated = true
        await adb.PushEmbeddedJar(serial, options.ScrcpyPath)
        serviceTask = adb.StartService(serial, options.ScrcpyPath,
                           "com.jx.screen.Server", options.StartupParameters)
        clients = await accept_video_audio_control(port, options, ct)
        if clients == null: return null  # 必须经过finally清理，不能直接泄漏映射
        attach_service_lifetime_to_clients(clients, serviceTask)
        reverseOwnershipTransferred = true
        return clients
    finally:
        try:
            if reverseCreated and not reverseOwnershipTransferred:
                try: stop_and_dispose_partial_clients_and_service()
                catch: pass
                try: await adb.RemoveReverse(serial, "localabstract:" + options.SocketName)
                catch: pass  # 清理失败不能阻止释放连接锁
        finally:
            serialConnectLock.Release()
    # 辅助函数为语义名；原链保存service token和traceId，见0x06000750/752。

async ReadVideo(stream, mailbox, ct):
    while not cancelled:
        header = await ReadExactly(stream, 12)
        if EOF: break
        flags = UInt64BE(header[0:8])
        isConfig = (flags & (1 << 63)) != 0
        isKey = (flags & (1 << 62)) != 0
        length = Int32BE(header[8:12])
        if length <= 0: continue
        payload = rent_buffer(length)
        await ReadExactlyInto(stream, payload, length)
        if decoding_suspended: release_or_hold_required_config(payload)
        else if congestion_recovery:
            hold_latest_codec_config_and_wait_for_keyframe(payload)
        else:
            transfer_owned_buffer_to_mailbox(payload, isConfig, isKey)
    finally: return_remaining_owned_buffers_to_pool()
    # 所有权移交后不能再次Return；细枝分支以0x060005e2的IL为准。

TryCoalesceTouchMove(request):
    if not eligible_move(request): return false
    lock controlQueueTailSync:
        if pending_move_exists:
            if placeholderVersion != queueTailVersion: return false
            previous = pending_move
            pending_move = request
            return_buffer(previous)
            return true
        queueTailVersion += 1
        pending_move = request
        placeholderVersion = queueTailVersion
        enqueue_move_placeholder()
        return true
    # 禁止跨越DOWN/UP/键盘等中间事件；0x06000066。

async StartProxy(payload, ct):
    await adb.EnsureRoot(payload.serial)
    runner = resolve_by_profile_type(payload.profile)
    await runner.Stop(payload)
    await remove_old_config()
    await runner.DetectUdpSupport(payload.profile)
    await write_remote_config(runner.BuildConfig(payload.profile))
    for binary in runner.Binaries: await EnsureBinary(binary)
    await start_privileged_background_command(runner.BuildStartCommand())
    for rule in runner.RoutingRules: await execute_privileged(rule)
    return success_if_process_exists_else_structured_failure()

WriteLog(level, messageFactory):
    if filtered_or_disabled(level): return
    queue.Add(LogWriteEntry(level, messageFactory))
    ensure_background_writer()

Writer():
    for entry in queue.ConsumingEnumerable:
        line = prepare_once(entry)
        lock fileSync:
            cleanup_if_interval_elapsed()
            path = choose_rotating_file(utf8_byte_count(line))
            append_utf8(path, line)
```


```

## 6. 失败模式与待验证项

- 同设备多客户端同时部署同名screen/JAR或清理reverse映射可能互相影响；文件不同已证明，具体冲突尚未运行复现。
- 小图、多图、主屏的参数与解码策略必须分别验证；构造函数默认值不能替代UI覆盖后的值。
- MOVE合并的正确性依赖事件类型与队列尾版本；简单“丢弃所有旧事件”会破坏按下/抬起关系。
- 构建配置的字符串替换、手机shell参数引用、备份归档验证均值得专项测试；本轮不把静态存在的过滤器视为安全证明。
- 第三方FFmpeg、Xray/Hysteria、ADB原生实现、设备JAR的内部语义未在此报告逐函数恢复；其调用边界与资源身份已记录。
```
