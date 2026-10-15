# A06b1 节点自动首载调度

## 目标与范围

基线为已推送的 `e235657`。修复用户进入节点页或切换检查设备时，因为暂时忙碌而丢失自动加载意图的问题。正常显示已有节点不重复读取；暂缓期间不并发发起新读；阻塞结束后只加载仍有效的最终目标。只调整窗口调度及活动状态通知，不修改 A06a 发布守卫、授权、ADB、API、协议或设备数据。

截图页布局、已完成截图缓存的身份换代、保存与裁剪、同一行离线重连、Catalog-only 过期后的自动补偿均不在本小修复内，仍属于后续 A06。不能将本阶段宣称为全部自动刷新或实机验收完成。

## 已知事实与根因候选

- `MainWindow.EnsureDebugLoadedAsync` 在设置 `debugAutoPending` 之前以 `HasActiveOperations` 返回；后者已经包含 `Busy`，所以后面的 Busy 暂缓分支不可达。
- 窗口只在 Busy 结束且 pending 已存在时恢复。传输状态和脚本状态没有独立的恢复链。
- A 到 B 再回到同一 A 时，原 finally 的引用比较不能识别中途的新检查意图；A06a 会拒绝旧发布，但不负责重新调度。
- 当前 Demo 默认不执行自动首载，直接调用 ViewModel 只能验证显式读取，不能作为自动路径的证明。以上先按源码事实及待复现路径登记，不提前写成实测失败。

## 推荐方案与取舍

将“不需读取”与“暂时不能读取”分开：前者清理无效意图，后者保留一个合并的待办。设备变化及导航产生新意图，活动状态结束只唤醒已经存在的待办。为活动状态提供明确通知，避免借用文本、TargetContext 或偶发目录更新。防止自身 LoadNodes 的 Busy 结束被误判为新意图而造成错误重试风暴。

更简单的只交换两行判断，只能覆盖部分 Busy 场景，不能覆盖传输和脚本结束通知；拒绝删除安全守卫、以定时器无限重试或全局放开并发。推荐事件驱动的单一待办，复杂度小于轮询，测试成本主要在事件顺序及生命周期。

若受控测试发现活动状态通知存在同步重入或某个阻塞永不释放，应修正状态边界，不靠增加延时、重试次数或吞错取绿。单一拒绝点最多三轮修改，超过则记录并升级，不扩大到其他功能。

## 验收标准

1. 正常 Demo 默认无自动读取；受限验收许可只在自有 artifacts/STA Demo 窗口中临时生效，不增加用户设置或 CLI，不启动 Demo 定时器、API 或真实后端。
2. 真实导航事件进入节点页，正常发起一次截图和节点读取，并建立实际 Canvas/Tree；不直接调用 Ensure/LoadNodes 冒充自动触发。
3. Busy/ExternalBusy、真实受控传输、实际 ScriptScheduler 阻塞期间只登记待办；释放后恰好自动读取最终目标，含 A 到 B 及 A 到 B 到 A。
4. 自动读取在途切换设备/ABA，原 A06a 拒绝过期发布，待办仍能加载最终目标；不同时发起两个首载。
5. 已有成功节点离开返回不额外读取；等待期间离页/关闭不再新增读取；已有合法在途读取不伪称已取消。
6. 后端失败或取消不无条件递归重试；反馈沿用安全中文，用户重新进入页面可恢复。
7. 每个用例记录真实事件、Backend 请求、loading/pending、VM 状态、树/画布与清理，TCS 控制先后，不以任意等待猜测竞态；原 49 例保持不变。

## 任务与验证

- P0：隔离测试许可及夹具先冻结，在基线得到受控红；独立审查许可边界和实际事件覆盖。
- P0：在独立 worktree 最小修复窗口调度和活动状态通知；正式 150 项及新专项红绿。
- P1：独立 verifier 在同冻结源码重复专项及三个桌面、六公共 shell 尺寸；主仓整合后代表复验。不得把 Demo 证据扩张为真实 Android 或默认用户配置运行证明。
- P1：更新 README/问题/任务/测试矩阵、源码清单；独立审查后仅推原私有功能分支，不合并发布，不提交本机配置或新原始证据。

## 基线红测

2026-10-15，独立 verifier 仅导入正式夹具和两处测试接线，产品调度及 VM 保持 e235657。第一次 Build/Verify exit0、150/150（51.89 秒）；第一次 1000×700 fresh Demo smoke child exit1（47.188 秒），16 例实际为 8 PASS / 8 FAIL，16 次清理全部通过，无重试。

失败逐项为 `blocked-busy-a/b/aba`、`blocked-external-b`、`blocked-transfer-b`、`blocked-script-aba`、`dual-transfer-script`、`inflight-Dump-aba`。前七项阻塞释放后没有新 Discover/截图/节点请求；最后一项只有旧 A 截图与 Dump，A06a 正确拒绝过期发布后反馈“已停止”，但节点为零且无补载。各项在预设的 5 秒后端进入门限失败，不能将超时本身当根因。原始 trace 证明待办未登记/未被恢复，与源码候选一致。

默认 Demo、正常首载与已有节点返回、在途 A 到 B、等待离页再返回、在途离页、失败/取消后重新进入恢复、真实 Close/Closing 停止脚本均通过。失败/取消用例验证单次失败后不循环，不是被关闭功能掩盖。专项在原 49 项之前聚合失败，原 49 和其后旧完整 smoke 本轮未执行，不能计为通过。

- r0 正式夹具 patch SHA256：`1E1DE92087357E160C30A45DE0DEED446D1ABF096DC3ADE1CB73120E37D781FC`。
- 新夹具 raw SHA256：`8733568495117D19953A6A261ACD25E0B80933E2A4A323F915884052FBD7A6AC`；仅接线的 MainWindow：`AE441F0A179FC493EAC23D8DF9FBFD7D349D7826690B522D38A3A3F9ECA13875`。
- VM 仍为 A06a 冻结 `79137E2E6D0DF2FF28FFA82471A1E9FA5E093D64471F73F0A1084E34883A03D4`；旧 49 夹具 canonical Git blob `631827d62a83a4cc35de6e48976d423f4434df35` 不变。新 checkout 的 CRLF raw 与上一轮主仓 LF raw 不同，未改其内容。
- 独立 DLL：`16A6743C899919F70DBDF6E91863A4798239CCF49F3AB4D6FC6C4E61A2FAF3F3`。
- 原始报告：独立树 `artifacts/a06b/r1/artifacts/al/07f978dd/automatic-loading.json`；SHA256 `2F1755D610A257ACE5F0C02BCEA518B012690F6B87597CBB53CEED1A41543DAA`。主代理直接复读核验全部分项、请求及清理，证据留本机，不上传原始数据。
- Reads/MaximumActiveRead 只计截图与 Dump；Discover 保留于完整 Calls，不将此计数描述为所有后台操作并发证明。

## 当前状态

首轮两生产文件已完成实施树、独立最终及主仓代表复验，本阶段提交以 Git 记录为准；完整目标保持 active。

## 首轮实现与预检

- MainWindow 将导航/设备变化产生的 pending 与活动结束唤醒分开；以单一 Dispatcher 队列合并，并在执行前重新检查页面、当前目标、节点、活动和窗口生命周期。自身读取开始前消费待办，结束只处理仍存在的新意图；已有节点、离页和关闭清理待办。
- VM 在 Busy、ExternalBusy、Transferring、真实脚本状态回调与目录快照应用结束处通知 HasActiveOperations。其定义、命令门禁及 API 写保护未改；通知是重新核验信号，不缓存布尔去重，避免异步脚本通知已晚于实际结束时漏唤醒。
- 产品差异仅 MainWindow/WorkspaceViewModel；测试差异仅新 DebugAutomaticLoadingSmoke 与 MainWindow 两处接线。无 App/Core/Host/API/真实 ADB 修改，原 49 例未改。
- 首次导出 patch 的 PowerShell 参数写法有误，git 未生成产物；保留 `export-command-first-failure.log`，仅纠正机械导出命令，源码未变化。这不是产品测试失败，也不是第二轮产品修改。
- 2026-10-15 首次 Build/Verify 150/150、51.60 秒，0 警告/错误；唯一 fresh 1000×700 完整 smoke exit0。新 16/16、137 检查、729 观测、16 次清理；原 49/401/191/49cleanup/9 磁盘日志归属通过；原标签 36/11、外观 209、筛选 73/22、Native 守卫 25 与 12 页 idle 图执行完成。Native 守卫不是真实嵌入或外部确认时序的证据。
- 新自动首载 JSON SHA256：`87C3DE677EC0E1DE23D48F08EE8A1E51493A52ED70136CC0AB7E785E4676C7D8`；原发布专项 JSON：`DDDD1B7D4D33CA08A20ACAC798561159B6025BF3DC9DFEC5AED1F4E33383BF37`。最终 desktop.json：`704429694B8B2D6EB5CB3D3C0D0513A6733FF79D841791145B8C2D24F3DC5AE2`，Error 为空；其最终 nodes=0 是原套件后续清理状态，不据此否定或替代新专项的实际 Tree/Canvas 证据。root 已直接复读以上报告、16 分项请求，并目视最终 PNG。
- 实施 DLL SHA256：`228D576BEE32FF41438D0D6FA63C8318D3E54A7AB4442F0494E1810F10034DD5`；三源冻结 MainWindow `2E8DC93EA0CCD4E0F08A2FC467FBD703580AEA71E1EBEE4C78125C5C4DD98D49`、VM `894985B3DAB1EF02856B8AEDEBEAB78386F9BD866928F3D0EF69C64C600E029B`、新夹具 `8733568495117D19953A6A261ACD25E0B80933E2A4A323F915884052FBD7A6AC`。相对 fixturebase 的产品 patch：`B454AFF58F326943FFFFC73F60C15F477A37EA0A878E4EA61858A33A31B32CEF`。

实施进程全部退出后，唯一 Build/GUI 锁交独立 verifier：相同冻结源一次正式基线、三桌面完整 smoke、六公共 shell，每项新短 profile/独立截图目录，任一首败保留并停止，不自行重试。AP-023 原悬停首败仍开放，本轮旧外观通过不代表其根因已解决；真实 Android、生产定时器、同 row 重连及其他 A06 范围仍未验。

## 独立最终复验

结论 `PASS-OFFLINE-A06B1-AUTOMATIC-NODE-FIRSTLOAD`。独立非实现者只机械导入冻结 patch，未修改产品或断言；一次 Build/Verify 0 警告/错误、150/150（51.28 秒）。三个桌面 1000×700、1440×900、1920×1080 均首次完整 exit0：每轮新 16/137/16cleanup（分别 729/727/727 观测，最大单例 66 条，小于 128 条队列界限）及原 49/401/191/49cleanup/9 正确来源、外观 209、Native 守卫 25、筛选 73/32矩阵/22 观测、标签 36/11 与 12 页 idle、4 设备行、Error 为空。

六公共 shell 尺寸为 390×640、390×844、768×1024、1000×700、1440×900、1920×1080，各 135 项、请求/实际 HWND 尺寸一致。本轮每个截图目录独立，六尺寸共 12 张辅助模态图均留存未覆盖；实际目视范围为三桌面列表、390 壳层/抽屉与两模态、1920 配置模态，不将留存等同于逐图目视全部模态。不是全部 12 页正文或物理移动设备验收。

9 个 GUI 首次全部 exit0，无重试、无自有进程残留；18 份执行前后源码/DLL 快照均一致。1920 整轮用时 89.04 秒，另两桌面约 51 秒，未定位这一时长差异的原因，不虚称性能已解决。独立未认证真实用户 profile 前后哈希，只使用新 artifacts 配置。

- 独立报告 `artifacts/debug-autoload-verifier/final-verification.json`，SHA256 `D3128E92ADF5EB47223454D650E817AA829792CF8C00B0373ABE696A477A38FD`，root 已直接复读核对全部分项与限制。
- 独立 DLL SHA256 `7B0F6C37F446069889D0EA0632ED77F7151C1F53AC3DDD2BC9FDD9A3B527D32C`；与实施 DLL 不同是各树路径/PDB 产物，三源码 raw 与固定 patch 全部相同，不用 DLL 不同掩盖源码差异。
- AP-019/020/023 历史首败继续开放；本轮未操作真实 Android、ADB、视频进程、账号、系统代理/证书或接口权限，没有将隔离用例换算为实机业务成功。

## 主仓整合与复验

整合前核验主仓仍为 e235657 对应的两生产源及旧 49 内容；按已核 SHA 的夹具/产品补丁机械导入，逐文件核 canonical Git 内容与原冻结文件一致，再统一新文件的 LF/CRLF 原始字节，使三源码 raw 精确匹配独立版本。原 49 源文件未编辑，仍保留主仓原字节与 canonical 内容。

一次 `tools/Build.ps1 -Verify` 为 150/150（51.27 秒）、0 警告/错误。忽略目录的有界自有 runner 仅运行 fresh 1000×700 完整 smoke 和 390×640 公共 shell，两进程首次 exit0，无超时、无重试。源码和 DLL 前后固定；新专项16/137/729观测/16cleanup，旧专项49/401/191/49cleanup及9正确磁盘日志源，旧完整桌面套件与 shell135 通过，Error 为空。root 直接解析真实报告并目视两代表 PNG。

- 主仓 DLL：`E3A9FFE48553C980B857D3C5F05C498022E553510859CA2D57827AE0C58196E2`。
- 新专项报告 `artifacts/a06u/f1/g1/artifacts/al/d6eb8031/automatic-loading.json`，SHA256 `1329C107AB6EAACB78005A4576D9AC499BEE74F5A47A663D3D831F42BED3792D`。
- 旧 49 专项报告 SHA256 `47563A50BCDAB691CF582FE478DD823C1764400D89B73C501E3BD249B057F53F`，清理字段是原 `cleanupCompleted`，不是新专项的 `cleaned`。
- 实际用户配置 `data/monitor-verified/AppConfig.json` 前后 SHA256 仍为 `E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0`；本轮所有运行使用新 artifacts 配置，不把独立树默认路径不存在等同于用户配置保护证明。

所有必需 exec/GUI 已结束，Build/GUI 锁空闲归 root。原始证据及 runner 留在本机 artifacts，不提交；源码归档仍存在 AP-021 已知历史验收材料，路径/字节完整性不是隐私净化认证。下一步独立处理节点/截屏正文响应式，以及缓存换代、保存快照、未完成裁剪等候选；每个缺陷仍先红后修，不能由本次自动首载调度推断全部 R03 或整个目标完成。
