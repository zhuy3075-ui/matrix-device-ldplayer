# 测试与修复阶段记录

## 计划阶段

- Goal：先制定计划，然后真实模拟器/用户APK全功能测试，日志驱动修复与复验。
- Context read：README、reference/PRD、API覆盖、最新UI与雷电实现、现有54项测试；未读取CODEBRAIN。
- Decision：先只读/离线，唯一目标与共享服务保护通过后再进入批准的应用动作；风险功能单列，不自动覆盖已安装应用。
- Files touched：plan.md、TEST_CASES.md、DEFECTS.md、TEST_LOG.md。
- Verification：计划独立审查和文件结构检查进行中；本轮真实测试尚未执行。
- Next step：独立审查结论，APK/环境只读清单，离线P1-UI-001复现修复，再启动有保护的真实连通测试。
- Risks：基本要求及独立实例全面测试已确认；新建空白实例或已有编号待回答。同实例ADB别名、共享server、APK签名/ARM64兼容性、个人账号与权限仍需保护。

本轮结果统计初始：PASS-LIVE=0；旧54项离线和UI demo通过不计本轮真实能力。原始真实证据将保存artifacts/acceptance/runId，不进公开源码包。

## 计划独立审查第1轮

- 三名审查者均要求真实阶段暂不放行，离线阶段可继续；无设备动作、APK执行或安装。
- 首次APK安装移到T03A/P0；T01仅主机静态清单，guest包查询后移；T03增加授权串号白名单与自身进程清理要求。
- native ADB缺服务/版本不匹配可能启动或替换共享服务；预检非原子保证。记录P0-ADB-001，不把配置路径当保护证明。
- APK只读目录检查：359908938字节；SHA256 DA4ED3EBCD2F76EEAA1061FD07BA10FDA5DA1C0173D22344C23A8ECAF675B8FA；47090条ZIP项、55个DEX，仅arm64-v8a原生库。尚未验证manifest/签名/CRC或实际兼容。
- 5037观察时由极星ADB持有，PID17036；这是时点记录，不当作后续固定PID。
- 下一步：列表模式主动过滤复现；继续审查修订计划，T03原理验证；专用实例选择仍待用户回答。

## 授权更新

用户已确认“新建空白测试实例”，允许安装给定APK；保留实例，不复制原账号，不自动删除。基本范围已接受；账号、支付、PII权限、root、条款确认仍不自动获批。

## 离线复现与修复阶段

- 旧节点残留先复现退出1，再修复并两尺寸通过；列表风险未复现，更正原审查推断，不为推测改Wall。
- 传输/退出忙态守卫先复现退出1，再修复；停止传输允许从头重试，不声称断点续传。
- 原生ADB改为直接既有loopback服务的托管连接，授权串号冻结；二进制截图保留原字节，shell-v2严格退出码、总输出限制，sync完整读写/完成尾部及部分下载原子提交。
- 服务缺失/版本错不启动或kill，未授权guest不查详情、不写；Agent路径同步校验授权。发现transport_id并固定排队请求身份，ID变化使目录代次失效。
- 假服务第一轮56/59，第二轮58/59，第三轮59/59；新增transport回归后60/60。失败日志保留，旧输入501测试前置新增授权，不降低501断言。
- 最新构建0警告0错误；ui-desktop、ui-small均退出0，使用不同--data-dir。每页idle/busy/offline控件清单保存.controls.json，状态观察不等于命令实测通过。
- aapt主机静态读取成功：com.ss.android.ugc.aweme，versionName40.7.0/versionCode400701，minSDK23/targetSDK34，仅arm64-v8a。没有执行APK；签名有效性仍由后续Android安装核验。
- 当前PASS-LIVE=0；真实实例尚未创建、尚未任何guest操作。下一步保护独立复核、边界回归，冻结服务所有者与只读目录，再创建获批空白实例。

## 真实实例与核心回归（2026-10-14 18:39起）

- 用户已自行关闭极星；创建前5037无所有者。批准的add仅执行一次，exit1但清单恰多出实例1，名称MatrixAcceptance-20261014-184112；按差集恢复记录，不重复add、不复制原数据。
- 仅launch实例1；雷电正常注册ADB PID44528，protocol0029。实例1 VBoxPID70760端口5557与服务建立唯一双向TCP连接；实际host清单注册emulator-5556/transport2。旧0两PID49596/75704保持。
- 管理连接再增加每条已连接socket反向四元组→服务PID/启动时刻固定代次。两自有进程接管同测试端口/同ID，旧backend409且新服务收到0请求；65/65离线通过，独立复跑65/65通过。
- 新实例Android34、ARM64及libhoudini；给定APK流式安装成功，实际版本40.7.0/400701。原API不存在monkey导致启动失败；改唯一launcher解析+显式am -W，支持alias，实机启动/首帧/初次节点通过。隐式am不能resolve的中间失败也保留。
- 应用隐私页面未代点击，用户已回复“已处理”。协议前后截图/节点证据仅保存在私有artifacts，不进源码ZIP。
- 动态视频页dump有时无XML且命令退出0，旧raw cat误当XML抛异常；诊断确认为文件未生成。改为确认dump输出、shell-v2 cat退出检查及明确409/502，静止系统页节点回归通过。动态页不承诺任意时刻可读取。
- 真实核心最新轮：连接、系统静止节点、截图/裁剪/取色、APP版本列表、停止及安装保留、Home、文件双向SHA256、脚本启动/查重/取消无后续点击、本机五条只读API均通过。
- 系统包android无点号，原过严校验导致API app/list-packages失败；允许且仅允许该系统特例，复测通过。下载新增pre/post STAT与字节数校验，完整early-DONE不得覆盖旧本地文件。
- 输入测试：设置列表实际滑动已通过；第一轮错误假定搜索属于Settings EditText，诊断发现真实为settings.intelligence AutoCompleteTextView。第二轮未重置前次搜索task导致无可滚动列表，调整测试前置，不修改生产输入逻辑。下一轮核精确回读与中文路径。

日志目录artifacts/acceptance/20261014-initial。分组run round递增不等于同一缺陷重复修复次数；每个缺陷单独记录，最多3轮。当前30分钟长测、全面API矩阵及最终GUI实机回归尚未完成，不宣称稳定验收完成。

## 接口与界面逐项阶段

- API矩阵round2：A01-A27全部登记，20业务/请求成功、6拒绝用例、1诊断（service/info）。旧原始日志A16标PASS-LIVE但caseId明确diagnostic，公开统计排除它，不覆写原证据；新版runner改PASS-DIAGNOSTIC。
- 普通B01-B10均实机成功，含中文/空格/符号精确回读；请求/响应、效果截图、Shell实际输出逐条保存。雷电L04/L05/L07/L08 HTTP真实效果通过；其他LD条件项不算业务PASS。
- 脚本launch/wait/swipe/click在安全设置页真实完成，另有给定APP启动/重复/取消测试。真实GUI8组含导航、截图/节点选择/清旧框、文件队列、APP与Shell及中文文本通过。
- 独立审查先跑65/65、1000x700 demo，再指出跨页Busy/API面板自我阻断、绑定配置可信度、标题滞后等；均有窄修与新回归，不以旧截图冒充最新修复证据。
- 新增GUI/API写互锁；API测试面板只连接本runtime启动的监听，等待不占设备写锁、HTTP超时报错且清旧响应；批量控件看Targets，Shell历史带串号；设备Info更新联动标题与命令；截图前刷新方向/尺寸，尺寸不一致手势拒绝。
- Unicode新增每次发送前实际端口反向TCP连接所有者→固定VBox PID/出生时刻校验，配置只提供候选，不能直接授权错误索引；错误PID/出生时刻/未知串号夹具拒绝且0ADB请求，正确绑定实机再回读通过。当前67/67离线通过。
- 第一段长测在92次读取后因绑定保护加强而主动停止自有runner，退出-1及停止回执保留；没有停止或重启共享ADB。最终构建将重新完整计时，前段不算30分钟通过。

## 最终离线与交付准备阶段

- final-mixed-demo通过Current离线但Targets在线的下载/上传门控断言；API发送使用本runtime实际运行端口，不以合并后的配置端口推测目标。
- 新增null/非法授权回归68/68后，独立审查指出损坏主配置仍可能从.bak恢复旧allowlist。按最小权限原则只恢复一般设置、清空授权与LD绑定并中文警告；主文件非法和缺失两个分支均有断言。
- authorization-backup-tests初轮67/68：旧恢复用例只接受1条warning，但新保护增加第2条警告。保留失败输出，更新断言要求两条且包含重新授权提示；final-build-and-tests最终68/68、0警告0错误。
- 本机启动入口tools/Start-VerifiedDesktop.cmd/.ps1使用唯一授权测试配置，不分发该配置；7份PowerShell脚本AST检查无语法错误，PowerShell 7与配置均存在。
- 文档reference11份与原Desktop目录SHA256逐字节一致。历史38/54项报告加历史提示，当前分类集中REPORT/TEST_RESULTS/API_MATRIX，实机原始截图不进源码ZIP。
- 最新Core DLL与长测冻结Core SHA256相同：9653A0071621E31A7E8CA681E047A745436E701F7F3692631E8DEEB10A2E05E7。后续Host运行端口/配置恢复修复独立复验，不宣称最新整个GUI也持续运行30分钟。
- 独立复跑备份保护68/68后还指出旧agent设备绑定会在授权清空后导致启动403。备份恢复同步清空AgentSerial/AgentBaseUrl，并扩展同一回归；这些是设备授权而非一般偏好，不在恢复时自动恢复。
- final-runtime-recovery-tests额外验证恢复后的真实模式运行时可在零授权下打开设置（不发现guest），68/68通过、0警告0错误；生产Host未再改动。独立verifier-settings-recovery-final也68/68，agent绑定缺口关闭。
- 最新Host独立delivery demo1000×700及1440×900均退出0、12页、zh-CN/error为空；verifier-delivery-ui-small.json与verifier-delivery-ui.json。Core/Desktop哈希未变，Host为F5BB911ADF763FE56932963451A9BF9EE03BB93122915F2DA3D6686E3938ECE7。

## 最终稳定性与实机复验

- session8124正常exit0；19:54:51至20:24:52，1801518ms，222真实读取、30截图、10节点、8资源；272行全部PASS-LIVE，失败0。首段停止的92次不纳入本段计数。
- 工作集采样57008128至68669440字节，最终68034560；private bytes23162880至29011968，handles321至374，读取最大14ms。不能从30分钟证据推出零泄漏、长期生产保证或20台容量。
- host-final只读快照guestCommands=0，仍server PID44528/原始出生时刻/protocol0029，授权目标transport2；整个长测原0主机PID49596/75704未变，未读取其guest。
- 20:25:12起最新Host/Desktop在live-ui-delivery再跑8组全部PASS-LIVE-UI，正常exit0。真实输入/中文精确回读、截图节点/清旧树、文件队列、应用/Shell及12页/三视图均复验；不是全部控件逐按钮业务测试。
- 交付保留实例1、给定APK与本机已核验独立配置。源码ZIP排除data、artifacts、APK、SDK、厂商二进制及实机截图；原参考11份哈希一致。
- 独立长测审核只读核272份证据SHA256、30PNG签名/尺寸1280×720/哈希、10树各37nodes、17冻结文件manifest全部一致，read0..221连续；222条ShellTrace均仅授权serial5556读取boot且0error。P95读取9ms/截图167ms/节点2083ms；限定冻结运行时证据通过，不扩张Host/GUI资源和零泄漏。
- 源码交付核验210源文件、66 C#、108份源码/构建文本、213ZIP条目，manifest/本地/ZIP逐文件长度与SHA256一致，敏感目录/编译二进制条目为0；全部源代码与目录分别在SOURCE_BUNDLE.md和PROJECT_TREE.md，旧包保留。
