# 极星双程序核心伪代码总集

> 开发参考资料，来自 2026-10-14 静态分析；不是已批准的首版建设规格，也不是当前运行结果。原始日志、CIL/IDA 导出、样本和资源中间文件已按用户清理要求移除。正文中的 token/RVA、evidence 路径仅保留为历史定位，不能当作现存文件；动态修正见 [已知问题](../KNOWN_ISSUES.md)，来源与可复核边界见 [来源说明](../SOURCES.md)。


这是从静态CIL重构的控制流，不是原始源码或可编译交付。省略编译器async样板，以模块+token/RVA对照证据。部分函数用语义占位表达未恢复细节；不得把占位当作实现完成。未运行目标、设备命令或本伪代码。

详见[主报告](ARCHITECTURE.md)的版本和覆盖边界。


## 一、调试工具


来源：debug-rea.json（历史定位：`evidence/debug-rea.json`，原文件已清理） 的 REA 证据 `ev_514a44b0f1beeb3209403bdaa902196d400e7ad8906a5fd7cc4a0c2af1948212`，与 IDA debug-ida-functions.json（历史定位：`evidence/debug-ida-functions.json`，原文件已清理） 的 `assembly`。语义变量名由分析者命名；加密字符串、SDK 内部实现仍用占位符。以下不是已编译测试的源码。

### 入口与服务依赖

```text
// 0x06000868, PE RVA 0x28324, IDA CLI EA 0x274e0
Main():
    app = new App()
    app.InitializeComponent()       // WPF资源与StartupUri；字符串尚未恢复
    app.Run()

// 0x06000857 / 0x0600085e
App.constructor():
    if IsDesignMode():
        AppVersionDisplay = DECODED_DESIGN_VERSION
        return
    try:
        InitializeFileLogger()
        ServicePointManager.SecurityProtocol = 0xFC0
        ServicePointManager.Expect100Continue = false
        AdbService.Instance.StartService(ResolveAdbPath())
        InitializeDeviceService()
    catch error:
        CommonCore.LogUtil.WriteLine(error.message)
        # 调试版CommonCore此路径最终为ret；这个调用本身不产生文件日志。
        FileLogger.error(error)
    AppVersionDisplay = Format(ExecutingAssembly.Version)

App.OnStartup(args):
    base.OnStartup(args)
    if IsDesignMode(): return
    InitializeLocalizationService()
    ScrcpySessionManager.Default.Initialize(Application.Current.Dispatcher)
    StartObservedBackgroundTask()   // 0x06000864；具体任务内部另行追踪
```

### 设备扫描与调试准备

```text
// 0x0600017f, PE RVA 0x902C, IDA CLI EA 0x73e0
ProbeAndConnect(ip, port, timeout):
    using tcp:
        result = tcp.BeginConnect(ip, port)
        if not result.AsyncWaitHandle.WaitOne(timeout):
            tcp.Close()
            return
        tcp.EndConnect(result)            // 异常分支终止本地址
    lock scanCounters:
        found += 1
        NotifyFoundCount(found)
    if AdbService.Instance.Connect(DnsEndPoint(ip, port)):
        lock scanCounters:
            connected += 1
            NotifyConnectedCount(connected)

// 0x06000668, PE RVA 0x1C444, IDA CLI EA 0x1b5d0
async PrepareDebugContext(deviceRow, jxDevice):
    context = {deviceRow, jxDevice}
    serial = deviceRow?.DeviceModel?.Serial
    if serial is not blank:
        options = ScrcpyOptions(
            DeviceSerial=serial, MaxSize=1080,
            BitRateKbps=0x100000,       // 原样观察；复刻时须核查下游单位
            MaxFps=60, ControlEnabled=true,
            AudioEnabled=false, ConnectTimeout=8 seconds)
        try:
            result = await ScrcpySessionManager.Default.ConnectAsync(serial, options)
            context.castFlag = (result != null)  // 仅此判定，不等于实测成功
        catch:
            context.castFlag = false
    try: context.image = await jxDevice.Screenshot()
    catch: context.image = null
    try: context.node = await jxDevice.Dump(true)
    catch: context.node = null
    return context
```

### UI 刷新与状态管理

```text
// LoadUIDumpCommand 0x06000468 → 0x06000413 → 0x06000484
async RefreshUiDump():
    if IsLoadUIDump: return
    if AutoDevice == null:
        Growl.Warning(LOCALIZED_NO_DEVICE)
        return
    IsLoadUIDump = true
    try:
        image = await AutoDevice.Screenshot()
        if image != null: Messenger.Send(image, IMAGE_MESSAGE_TOKEN)
        else: Growl.Error(LOCALIZED_SCREENSHOT_FAILURE)

        node = await AutoDevice.Dump(true)
        if node != null:
            Messenger.Send(node, TREE_MESSAGE_TOKEN)
            if MainTab == 0 and ScreenShowTabType != 2:
                ScreenShowTabType = 0
        else: Growl.Error(LOCALIZED_DUMP_FAILURE)
    catch error:
        Growl.Error(error.message)
    finally:
        IsLoadUIDump = false

// 0x060003d5–0x060003de；节点绘图和选择，不发送手机点击
BuildNodeOverlays(node, depth, scaleX, scaleY, output, renderOrder):
    if node is null: return
    rectangle = BoundsToOverlay(node.Bounds, depth, scaleX, scaleY, renderOrder)
    if ObservedVisibilityPredicate(rectangle):
        AddOrMergeOverlay(output, rectangle)  // 确切过滤/归并条件需单独核查
    for child in node.ChildNodeList:
        BuildNodeOverlays(child, depth + 1, scaleX, scaleY, output, renderOrder)

SelectOverlay(rectangle, broadcast):
    for item in NodeRectangleList:
        if item.IsSelected: item.IsSelected = false
    rectangle.IsSelected = true
    if broadcast and rectangle.SourceNode != null:
        Messenger.Send(rectangle.SourceNode, NODE_SELECTION_TOKEN)
```

### 输入坐标与投屏参数

```text
// 0x060000cf, PE RVA 0x4DBC, IDA CLI EA 0x3130
MapPoint(uiPoint, displayWidth, displayHeight, session):
    vw = session.VideoWidth
    vh = session.VideoHeight
    if min(vw, vh, displayWidth, displayHeight) <= 0: return null
    x = int(uiPoint.X * ScaleHelper(vw, displayWidth))
    y = int(uiPoint.Y * ScaleHelper(vh, displayHeight))
    // helper数学语义尚需独立确认；调用形状表明它负责尺寸比值
    x = max(0, min(x, vw-1))
    y = max(0, min(y, vh-1))
    return Position(ScreenSize(uint16(vw), uint16(vh)), Point(x,y))

// 0x060000bd；普通投屏页参数来自用户选择，不等同于初次准备的常量
BuildOptions(device):
    return ScrcpyOptions(
        DeviceSerial=device.Serial,
        MaxSize=ResolveResolution(device, SelectedResolution),
        BitRateKbps=SelectedBitRateKbps,
        MaxFps=SelectedFrameRate,
        ControlEnabled=true, AudioEnabled=false, ConnectTimeout=8 seconds)
// 0x0600012f 调用 UpdateOptionsAsync；Applied/Selected 字段用于未应用状态。
```

### Shell 与 HTTP JSON

```text
// 0x06000521, PE RVA 0x17248, IDA CLI EA 0x161e0
async ExecuteShell():
    if CurrentInput is blank: return
    if AutoDevice is null:
        Growl.Warning(LOCALIZED_NO_DEVICE)
        return
    try:
        IsExecuting = true
        command = CurrentInput.Trim()
        output = await AutoDevice.Shell(command)
        AppendHistoryItem(command, output)
        CommandHistory.Add(command)
        CurrentInput = ""
        historyCursor = -1
        historyDraft = ""
    catch error:
        Growl.Error(error.message)
    finally:
        IsExecuting = false

// 0x06000076 + 0x06000079；EXE辅助HTTP客户端，具体服务地址尚未恢复
sharedHttp = HttpClient(BaseAddress=ResolveBaseUri(), Timeout=30 seconds)
async Request<T>(method, path, body, headers, cancellation):
    if method == null: throw ArgumentNullException
    using request = HttpRequestMessage(method, ResolveUri(path)):
        if body != null:
            request.Content = StringContent(JsonSerialize(body), UTF8, MEDIA_TYPE_TOKEN)
        for name,value in headers:
            if name not blank and value not blank:
                request.Headers.TryAddWithoutValidation(name,value)
        try:
            using response = await sharedHttp.SendAsync(request,cancellation):
                text = response.Content == null ? "" : await response.Content.ReadAsStringAsync()
                if not response.IsSuccessStatusCode:
                    Log(method,path,response.StatusCode,text)
                    throw HttpRequestException
                if T == string: return text ?? ""
                if text is blank: return default(T)
                return JsonDeserialize<T>(text)
        catch error:
            Log(method,path,error)
            throw
```

### 结构导出与日志

```text
// 0x06000597/598/5c7；key具体拼写受字符串保护，字段来源可证
NodeToJson(node):
    result = JObject()
    result[FIELD_INDEX] = node.Index
    // 其他节点属性按getter读取，具体key不能凭惯例填充
    result[FIELD_BOUNDS] = {
        FIELD_LEFT: node.Bounds.Left, FIELD_TOP: node.Bounds.Top,
        FIELD_RIGHT: node.Bounds.Right, FIELD_BOTTOM: node.Bounds.Bottom,
        FIELD_CENTER_X: node.Bounds.CenterX, FIELD_CENTER_Y: node.Bounds.CenterY,
        FIELD_WIDTH: node.Bounds.Width, FIELD_HEIGHT: node.Bounds.Height
    }
    result[FIELD_CHILDREN] = [NodeToJson(child) for child in node.ChildNodeList]
    return result

// 0x06000394/398
InitializeFileLogger():
    lock logLock:
        directory = Combine(AppDomain.BaseDirectory, LOG_DIRECTORY_TOKEN)
        CreateDirectory(directory)
        logPath = Combine(directory, Format(LOG_NAME_FORMAT_TOKEN, DateTime.Now))
        if not Exists(logPath): WriteAllText(logPath,"",UTF8)

AppendLog(level, category, message, exception):
    try:
        lock logLock:
            if logPath blank: InitializeFileLogger()
            line = FormatObservedLogFields(DateTime.Now,level,category,message)
            if exception != null: line += exception.ToString()
            AppendAllText(logPath,line,UTF8)
    catch:
        pass                    // 这是被分析程序的吞错路径，不建议新实现沿用
```

没有足够证据重构“脚本生成器”。上面的结构导出是节点 JSON；Shell 是一次命令执行。这些功能与脚本编辑、录制、调度是不同的控制流。

## 二、云控主程序


以下为人工从 CIL 重建的模块级伪代码，作用是说明可独立实现的控制流和数据协议。类型、异常处理及编译器状态机被简化；外部 DLL 接口保留为边界。它不是原项目完整源码，也未进行运行验证。token/RVA 属于本次样本。

### 1. 启动（App.Main / OnStartup）

~~~text
Main():                                  // 0x060002bc / RVA 0x19eec
    app = new App()
    app.InitializeComponent()
    app.Run()

OnStartup():                             // 0x0600019c / RVA 0xff1c
    LogUtil.EnableFileLogging = true
    LogUtil.MinimumLevel = 1
    if not TryAcquireSingleInstanceMutex(): Shutdown(); return
    base.OnStartup()
    CrashReportService.Initialize()
    install ADB / Scrcpy / Core trace adapters
    subscribe device recovery and disconnection events
    RegisterGlobalExceptionHandlers()
    AppGlobal.LoadAppConfig()
    SyncMiniOptions(); SyncMaterControlOptions()
    LocalizationService.Init(assembly, resourcePrefix, "Lang", supportedLanguages)
    MainWindow = new LoginStartWindow()
    MainWindow.Show(); MainWindow.Activate()
    start watchdog and schedule deferred startup

RunDeferredStartupWork():                 // 0x060001bb / RVA 0x11130
    AdbService.AdbPath = bundledAdbPath
    InitializeStartupWindowInfrastructure()   // single-init guard + Dispatcher
    observe(RunDeviceStartupPipelineAsync())
    observe(Task.Run(AddAdbToSystemPathSafely))

async RunDeviceStartupPipelineAsync():    // 0x060001bc / RVA 0x1118c
    infrastructureTask = StartDeviceInfrastructureForShellAsync()
    scanConfigurationTask = Task.Run(InitializeStartupDeviceScanConfiguration)
    infrastructureReady = await infrastructureTask
    await scanConfigurationTask           // two startup jobs started in parallel
    if not infrastructureReady: log("skip reconnect"); return
    if not TryGetAuthorizedDeviceLimit(out count):
        log("defer scan until authorization valid"); return
    await RunStartupConfiguredNetworkScanAsync(startupReason)
    await EnsureMiniDevicesConnectedAfterStartupAsync()
~~~

上述最后两个 await 的依赖表达简化了编译器状态机，不定义账户授权内部协议；首次登录、续期及授权上限由专门业务函数决定。

### 2. 普通 API 网关/单设备服务

依据：ApiHttpServer.Start 0x06002f92/RVA 0xca2f4；ApiDeviceRequestHandler 0x06002fa8..0x06002fb8；端口 Reserve 0x06003003/RVA 0xcd56c。

~~~text
BuildBaseUrl(port): return "http://localhost:" + port + "/"

ReserveDevicePort():
    lock(portGate):
        assert not disposed
        RemoveExpiredCooldowns()
        used = BuildUsedPortSet()          // listener/reserved/cooldown/gateway
        for port in 8001 .. 8999:
            if port not in used:
                reserve port
                return port
        throw NoAvailablePort

NormalizePath(path):                      // IDA EA 0xd52d0
    value = blank(path) ? "/" : trim(path)
    if not startsWith(value,"/"): value = "/" + value
    value = trimTrailing(value,"/")
    return empty(value) ? "/" : lowerInvariant(value)

async HandleGateway(request):
    path = NormalizePath(request.Url.AbsolutePath)
    if request.Method == GET and path in ["/", "/health"]:
        return Json(200, {ok:true, data:{port:gatewayPort,
                    runningCount:runningDevices.Count, status:"running"}})
    if request.Method == GET and path == "/devices":
        return Json(200, {ok:true, data:runningDevices.map(CreateDeviceSnapshot)})
    return Json(404, {ok:false,message:notFoundMessage})

async HandleDevice(host, request):
    if device unavailable: return Json(410,{ok:false,message:unavailableMessage})
    path = NormalizePath(request.Url.AbsolutePath)
    if GET and path in ["/","/health"]:
        return Json(200,{ok:true,data:{serial:host.serial,port:host.port,status:"running"}})
    if GET and path == "/device":
        return Json(200,{ok:true,data:CreateDeviceSnapshot(host)})
    if POST and path == "/shell":
        q = await ReadJson<ShellRequest>(request)
        if q == null or blank(q.Command): return BadRequest()
        output = await Task.Run(q.UseRoot
            ? ExecuteRemoteRootCommand(q.Command,host.device)
            : ExecuteRemoteCommand(q.Command,host.device))
        return Json(200,{ok:true,data:{output}})
    if POST and path == "/tap":
        q = await ReadJson<TapRequest>(request)
        if q == null: return BadRequest()
        await command(host.device, format("input tap {0} {1}",q.X,q.Y))
        return Json(200,{ok:true})
    if POST and path == "/swipe":
        q = await ReadJson<SwipeRequest>(request)
        if q == null: return BadRequest()
        duration = q.DurationMs <= 0 ? 300 : q.DurationMs
        await command(host.device, format("input swipe {0} {1} {2} {3} {4}",
                      q.StartX,q.StartY,q.EndX,q.EndY,duration))
        return Json(200,{ok:true})
    if POST and path == "/text":
        q = await ReadJson<TextRequest>(request)
        validate according to handler; AdbService.SendText(host.device,q.Text)
        return Json(200,{ok:true})
    if POST and path == "/app/start":
        q = await ReadJson<StartAppRequest>(request)
        require nonblank q.PackageName
        command = not blank(q.Activity)
          ? "am start -n " + q.PackageName + "/" + q.Activity
          : "monkey -p " + q.PackageName + " -c android.intent.category.LAUNCHER 1"
        output = await command(host.device,command)
        return Json(200,{ok:true,data:{output}})
    if POST and path == "/app/stop":
        q = await ReadJson<StopAppRequest>(request)
        require nonblank q.PackageName
        output = await command(host.device,"am force-stop " + q.PackageName)
        return Json(200,{ok:true,data:{output}})
    return Json(404,{ok:false,message:notFoundMessage})

async ReadJson<T>(request):
    if not request.HasEntityBody: return default(T)
    text = await StreamReader(request.InputStream,
                request.ContentEncoding ?? UTF8).ReadToEndAsync()
    return JsonConvert.DeserializeObject<T>(text)

async Json(code,value):
    bytes = UTF8(JsonConvert.SerializeObject(value))
    response.StatusCode = code
    response.ContentType = "application/json; charset=utf-8"
    response.ContentLength64 = bytes.length
    await response.OutputStream.WriteAsync(bytes)
    close response stream
~~~

以上是普通 API。Automator 服务默认 17171（0x06003043/RVA 0xce8f1），使用 JXAutomatorService/JXAutomatorCore，不能套用上述普通 API 全部路径。新产品若需要相同端口/契约，应明确服务类型，避免端口误接。

### 3. 多设备输入调度

依据：Enqueue 0x060031dd/RVA 0xd3f48；CanReplaceMove 0x060031df/0xd407c；Dispatch 0x060031f7/0xd4880；ScaleControlPoint 0x0600321b/0xd5ef4；fanout Enqueue 0x06003223/0xd618c。

~~~text
CanReplaceMove(old,new):
    return old.generation == new.generation
       and old.gestureId == new.gestureId
       and old.excludedSourceSession is new.excludedSourceSession
       and equalsIgnoreCase(old.excludedSourceSerial,new.excludedSourceSerial)

Enqueue(request):
    if disposed or request.targets empty: return
    lock(queueGate):
        if request is mouse MOVE:
            if pendingMove exists and not CanReplaceMove(pendingMove,request):
                readyQueue.push(pendingMove)
            pendingMove = request           // replace only compatible latest MOVE
        else:
            if pendingMove exists: readyQueue.push(pendingMove); pendingMove = null
            readyQueue.push(request)        // preserve DOWN/UP/keyboard boundaries
    SignalWorkerOnce()

InputWorker():                              // dedicated background thread
    while not canceled:
        await semaphore
        for request in DrainReadyRequests(): Dispatch(request)

Dispatch(request):
    if not handoffPolicy.ShouldDispatch(request.generation,currentGeneration,
                  request.completesGesture,request.isHandoffCancellation): return
    handle cancellation branch and gesture state
    dispatch source session first when applicable
    for target in request.targets:
        TargetFanout.Enqueue(target.key,request,dispatchForTarget)

TargetFanout.Enqueue(key,request,dispatch):
    lock(mailboxGate):
        mailbox = GetOrCreateMailbox(key)
        remove obsolete MOVE work according to generation/gesture policy
        mailbox.queue.push({request,dispatch})
        if not mailbox.scheduled:
            mailbox.scheduled = true
            readyMailboxes.push(mailbox)
            semaphore.Release()

TargetWorker():
    while active:
        await semaphore
        mailbox = next ready mailbox
        process queued work in order for this mailbox
        catch target failure: log and continue  // one target does not stop every device
        clear/reschedule mailbox under lock according to remaining items

ScalePoint(point,actualWidth,actualHeight,videoWidth,videoHeight):
    aw = actualWidth > 0 ? actualWidth : 1
    ah = actualHeight > 0 ? actualHeight : 1
    x = clamp(trunc(point.x * videoWidth / aw),0,videoWidth-1)
    y = clamp(trunc(point.y * videoHeight / ah),0,videoHeight-1)
    return (x,y)

DispatchMouseTarget(target,request):
    (width,height) = target.currentVideoSize
    point = ScalePoint(request.point,request.actualWidth,request.actualHeight,width,height)
    message = cached touch/wheel snapshot for this dimension or construct one
    target.TryEnqueueMouseMessage(message)
~~~

mailbox 的具体公平调度批次、线程数量公式以及手势取消 policy 没有在该伪代码展开；完整分支在原 CIL 附件，不能把这一简化实现当作严格字节级等价。

### 4. 脚本包、任务与日志

依据：Load 0x06003edb/RVA 0x10c18c；StartAsync 0x06003f3f/0x10f594；ExecuteStepAsync 0x06003ef1/0x10ccf0；Stop 0x06003f40/0x10f69c；Append 0x06003f59/0x10fcf8。

~~~text
LoadPackage(path):
    assert file exists
    using archive = ZipArchive(path,Read):
        manifest = Deserialize<ScriptManifest>(readEntry("manifest.json"))
        package = Deserialize<ScriptPackage>(readEntry("script.json"))
    require manifest.Id not blank
    package.Manifest = manifest
    package.Steps ??= []
    package.Parameters ??= []
    NormalizeAndValidate(package)
    return package

StartScript(device,package):
    require device,package,normalizedSerial
    lock(tasksGate):
        if tasks contains serial:
            Append(serial,"already running"); return completedTask
        source = new CancellationTokenSource()
        tasks[serial] = source
    ClearLog(serial); Append(serial,"start")
    return RunAndRelease(serial,device,package,source)

async RunAndRelease(serial,device,package,source):
    try:
        repeat:
            await RunScript(device,package,source.Token)
            if not IsWarmupPackage(package): break
            await Delay(minutes(package.RunIntervalMinutes),source.Token)
        until source.IsCancellationRequested
    catch cancellation: pass                // finally still releases task
    catch error:
        Append(serial,localizedError(error))
        throw
    finally:
        lock(tasksGate):
            if tasks[serial] is source: tasks.remove(serial)
        source.Dispose()
        Append(serial,"ended")

StopScript(serial):
    lock(tasksGate): source = tasks.get(serial)
    if source exists: source.Cancel(); Append(serial,"pause")
    // This cancels a task. It does not preserve an instruction pointer for resume.

async RunScript(device,package,token):
    Append(serial,"begin")
    for step in package.Steps ?? []:
        token.ThrowIfCancellationRequested()
        if lower(trim(step?.Action)) not in ["launchapp","launch","wait","sleep","click","swipe"]:
            continue
        Append(serial,localizedStep(step))
        await ExecuteStep(device,step,token)
    if IsWarmupPackage(package): await SpecificAppWorkflow(device,package,token)
    Append(serial,"completed")

async ExecuteStep(device,step,token):
    action = lower(trim(step.Action))
    switch action:
      "launchapp","launch":
        packageName = RequirePackageName(step.Value) // recovered regex check
        await RunCommand("monkey -p " + packageName + " -c android.intent.category.LAUNCHER 1",device,token)
      "wait","sleep":
        await Delay(max(0,step.DurationMs),token)
      "click":
        if TryParsePoint(step.Value,out x,out y):
            await RunCommand(format("input tap {0} {1}",x,y),device,token)
      "swipe":
        if TryParseSwipe(step.Value,out arguments):
            await RunCommand("input swipe " + arguments,device,token)

TryParsePoint(text):
    parts = (text ?? "").split(',')
    return length(parts)==2 and each part Int32.TryParse

TryParseSwipe(text):
    parts = (text ?? "").split(',')
    arguments = join(' ',parts.map(trim))
    return length(parts)==5 and all parts pass integer predicate

Append(serial,message):
    if blank(serial) or blank(message): return
    entry = {Timestamp:Now,Message:trim(message)}
    lock(logGate):
        list = logs.getOrCreate(serial)
        list.add(entry)
        if list.count > 500: remove oldest list.count - 500 entries
    EntryAdded(serial,entry)
~~~

Probability 字段存在，但上述通用动作运行路径未见使用；特定应用工作流另有参数/概率逻辑，不能据字段名宣称通用随机执行。

### 5. 配置持久化与本地化

~~~text
LoadAppConfig():                            // 0x060004d6 / RVA 0x2b668
    config = tryReadValidJson(AppConfigPath)
          ?? tryReadValidJson(AppConfigPath + ".bak")
          ?? DefaultConfig()
    normalize ranges and initialize missing collections
    config.ScreenCastHorizontalCount = clamp(value,5,20)
    if normalized changed: SaveAppConfig()
    remember clone baseline and last modification timestamp

SaveAppConfig():                            // 0x060004dc / RVA 0x2bda8
    lock(configGate):
        disk = tryReadValidJson(AppConfigPath)
        if disk valid and savedBaseline exists:
            model = MergeLocalChangesOntoDiskModel(savedBaseline,current,disk)
        else: model = current
        refresh reboot timing display fields
        if SaveTextFile(AppConfigPath,ToJson(model)):
            remember clone baseline and timestamp

SaveTextFile(path,text):                    // 0x0600281a / RVA 0xad0e4
    ensure parent exists
    lock(fileGate):
        temp=path+".tmp"; backup=path+".bak"
        try:
            write temp using UTF8 without BOM
            if exists(path): File.Replace(temp,path,backup,true)
            else: File.Move(temp,path)
            return true
        catch:
            delete temp if present
            return false

CreateDatabaseConnection(settings):        // SQLCore 0x0600003c / RVA 0x2554
    require settings != null and nonblank settings.DatabasePath
    if Interlocked.Exchange(initialized,1)==0: SQLitePCL.Batteries_V2.Init()
    connectionString = SQLiteConnectionString(path,storeDateTimeAsTicks,password,...)
    return SQLiteAsyncConnection(connectionString)
    // Do not copy an embedded original-product password into a new implementation.

ChangeCulture(name):                       // LanguageCore 0x06000024 / RVA 0x2514
    culture = CultureInfo.GetCultureInfo(name)
    set default thread culture and UI culture
    set current thread culture and UI culture
    CurrentCulture = culture
    publish LanguageChanged
    Language.Default.AppCulture = name
    Language.Default.Save()
~~~

## 三、自动化网关与ADB


以下重构只表达已观察的控制流，不保留编译器 async 状态机样板。证据简称、二进制hash范围和完整IL见 [API分析报告](API.md)。调用中配置变更、设备shell等均未在本次分析执行。

### A. 网关与请求处理

```text
Gateway(options):
    sessions = DeviceSessionManager()
    service = AutomationDeviceService(sessions)
    dispatch = AutomationRequestDispatcher(service,
                 json = camelCase + ignoreNull)
    server = HttpListener(options.prefix ?? http(options.host, options.port))
    # 默认 host=127.0.0.1, port=17171；S 0x06000172/0x4372

Start():
    if running: return
    listener.Start()
    running = true
    Task.Run(AcceptLoop)

async AcceptLoop():
    while running:
        context = await listener.GetContextAsync()
        Task.Run(() => Dispatch(context))
    # 每个请求可并发；设备动作在下一层按设备串行

async Dispatch(context):
    path = TrimSlashes(context.Url.AbsolutePath)
    if startsWithIgnoreCase(path, "api/"): path = path.substring(4)
    path = lowercase(path)
    if method == GET:
        if path == "health": return Ok({running, prefix: listenPrefix})
        if path == "devices": return Ok(deviceService.GetDevices())
        return Error(http=404, code=1006, "route not found")
    if method != POST:
        return Error(http=405, code=1005, "method not allowed")
    route = ROUTES[path]  # 27条表见 API.md，S 0x060001CA/0x6974
    if absent(route): return Error(404,1006,"route not found")
    body = await ReadAllText(context.request)
    request = whitespace(body) ? new route.RequestType : Deserialize(body)
    request ??= new route.RequestType
    result = await ExecuteCore(route.handler, request)
    # 写HTTP状态、UTF8 JSON字节长度、响应body并关闭流

async ExecuteDevice(request, action):
    require request != null and action != null
    require not whitespace(request.serial)
    if serial not in CommonCore.AdbService.Devices():
        throw KeyNotFoundException("device not found")
    session = await sessions.ConnectAsync(request.serial, timeoutMs=0)
    session.lastSeenUtc = UtcNow
    return await action(session.device, request)
    # S 0x060001B0/0x55D8；参数0是观察到的调用值
```

### B. 设备会话与 agent 引导

```text
async Connect(serial, cancellation, remoteDirectory):
    require not whitespace(serial)
    remoteJar = (remoteDirectory if set else "/data/local/tmp") + "/JXAutomator.jar"
    if cachedClient(serial) is live:
        atomic_increment(client.refCount)
        return client.device
    await bootLock[serial].WaitAsync(cancellation)
    try:
        # 锁内复查，避免两个请求同时部署
        if cachedClient(serial) is live:
            atomic_increment(client.refCount)
            return client.device
        client = await CreateClient(serial, cancellation, remoteJar)
        client.refCount = 1
        clients[serial] = client
        return client.device
    finally: bootLock[serial].Release()
    # C 0x06000179/0x5A68

async CreateClient(serial, ct, remoteJar):
    localPort = GetFreeTcpPort()
    mainClass = "com.jixing.automator.auto.Main"
    if await adb.IsServiceAlreadyRunning(serial, remoteJar, mainClass):
        reused = await adb.TryGetReusableForwardPort(serial, 9008)
        if reused != null: localPort = reused
        else: await adb.Forward(serial, localPort, 9008)
    else:
        await adb.RemoveForwardByDevicePort(serial, 9008)
        await PushJarIfDifferent(serial, remoteJar)
        await adb.Forward(serial, localPort, 9008)
        await PrepareAndStartAgent(serial, remoteJar, mainClass, 9008)
    client = JXClient.Connect("http://127.0.0.1:" + localPort + "/")
    deadline = UtcNow + 20 seconds
    while UtcNow < deadline:
        ct.ThrowIfCancellationRequested()
        if await IsTcpReachable("127.0.0.1", localPort, 1.5 seconds, ct):
            await Delay(5000, ct)
            return client
        await Delay(200ms, ct)
    client.Dispose()
    throw JXApiException("ConnectAsync timeout: serial=..., baseUrl=...")
    # C 0x0600017B/0x5D14：这只是TCP就绪，不是HTTP语义健康检查

async PushJarIfDifferent(serial, remoteJar):
    localJar = OpenManifestResourceEndingWith(".Resources.JXAutomator.jar")
    require localJar exists
    localHash = SHA256(localJar)
    remoteHash = await ReadRemoteSha256UsingSha256sumOrToybox(serial, remoteJar)
    if nonempty(remoteHash) and equalsIgnoreCase(remoteHash, localHash): return
    adb = ConnectTcp("127.0.0.1", 5037)
    await adb.Request("host:transport:" + serial)
    await adb.ReadOkayOrFail()
    await AdbSyncPush(adb, localJar, remoteJar, mode=0644)
    # C 0x06000190/0x763C

async PrepareAndStartAgent(serial, jar, mainClass, port):
    await adb.Shell(serial, prepareCommand(removeOldStartupLog,
                     chmodJar0644, stopMatchingOldAgentProcesses, wait1s))
    command = format("CLASSPATH={0} app_process / {1} --port {2}", jar, mainClass, port)
    Task.Run(async => {
        try: await adb.Shell(serial, command, CancellationToken.None)
        catch: /* 原实现忽略后台启动异常；由后续就绪探测感知失败 */
    })
    # C 0x0600019E/0x84CC，后台状态机0x060001CC/0xA7A0
```

### C. ADB smart socket 与 HTTP

```text
async SendAdbRequest(socket, payload):
    bytes = ASCII(payload)
    header = ASCII(hexUpper(bytes.length, width=4))
    await socket.Write(header)
    await socket.Write(bytes)

async ReadOkayOrFail(socket):
    status = ASCII(await ReadExactly(socket, 4))
    if status == "OKAY": return
    if status == "FAIL":
        length = parseHex(ASCII(await ReadExactly(socket,4)))
        reason = ASCII(await ReadExactly(socket,length))
        throw Error("ADB FAIL: " + reason)
    throw Error("ADB invalid status: " + status)
    # C 0x060001AA/0x8DF4、0x060001A8/0x8BA4

async HttpSend<T>(method, path, query, body, ct):
    require not whitespace(path)
    uri = BuildUri(baseUrl, removeLeadingSlash(path), urlEncoded(query))
    using http = HttpClient(baseAddress=baseUrl, timeout=15s)
    http.headers.ConnectionClose = true
    addDefaultHeaders(http)
    using request = HttpRequestMessage(method, uri)
    if body is HttpContent: request.content = body
    else if body != null: request.content = UTF8_JSON(body)
    try:
        response = await http.SendAsync(request, ct)
        text = await response.content.ReadAsStringAsync()
    catch ex: throw JXApiException("HTTP request failed...", inner=ex)
    if not response.IsSuccessStatusCode:
        throw JXApiException(httpStatus=response.status, message=text)
    envelope = Deserialize<JXResponse<T>>(text)
    require envelope != null
    if envelope.code != 0:
        throw JXApiException(apiCode=envelope.code, message=envelope.message)
    return envelope.data
    # C 0x0600017F/0x6630；实际IL有错误JSON包装及Dispose finally
```

### D. 按设备串行与节点算法

```text
async DeviceQueue.Run(action, ct):
    require action != null
    EnsureUsable()
    require clientIsAlive()
    await semaphoreForThisDevice.WaitAsync(ct) # 容量1
    try:
        EnsureUsable()
        require clientIsAlive()
        return await action()
    finally: semaphoreForThisDevice.Release()

FindAll(root, selector):
    results = []
    Traverse(root)
    return results
    function Traverse(node):
        if node == null: return
        if AllSpecifiedConditionsMatch(node, selector): results.add(node)
        for child in node.childNodeList or []: Traverse(child)
    # S 0x06000034/0x28EC；原实现递归前序遍历，保留children顺序

async WaitAny(selectors, timeoutMs, pollMs, ct):
    require selectors nonempty and contains no null
    require timeoutMs >= 0 and pollMs > 0
    started = TickCount
    repeat:
        ct.ThrowIfCancellationRequested()
        for index, selector in enumerate(selectors):
            element = Device.Element(selector)
            if await element.ExistsAsync(ct):
                node = await element.InfoAsync(ct)
                return {index, selector, node}
        await Delay(pollMs, ct)
    until elapsed(TickCount, started) >= timeoutMs
    return null
    # C 0x06000172/0x555C；依输入顺序优先，不并行
```

### E. 重建时应验证的差异

- 静态构建中的TCP可达+5秒等待不保证agent API就绪；复刻可改健康/版本探测，但须记录为新设计。
- 网关与设备端字段名不同，例如 `packageName` 映射为 `package`，`command` 映射为 `cmd`。
- 请求级并发与每设备串行都需要测试；20台吞吐、启动风暴、USB断线恢复均未实测。
- D 构建的混淆私有实现未按 CIL 全量语义复原；本伪代码中 C/S 标记内容不声称直接对应 D/B。

## 四、云控投屏、代理与日志


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



## 五、补充定位

调试版投屏与触摸序列化见[调试共享库补充](DEBUG_SHARED.md)；备份/恢复流程及其破坏性步骤见[共享模块分析](SHARED.md)。本总集不能替代完整IL：具体边界分支、异常区域和未知字符串需要恢复原始证据或重新提取后核验。
