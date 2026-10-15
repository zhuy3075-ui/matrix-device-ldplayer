# 当前目标交接

更新：2026-10-15。完整目标仍active，未达到全功能、全控件、真实设备、六尺寸和最终稳定性完成门槛；不要调用complete或把离线通过换算为实机通过。

## 用户目标与权限

自主逐路径测试/修复全部原功能，消除真实重复职责，采用Apple HIG启发的浅色、简短中文和响应式体验；保持12页Tag/接口/数据与安全边界。每个功能独立验证后push原私有功能分支，不问重复问题，不合并/发布/部署。原实例0、用户账号/抖音数据、共享ADB、系统代理/证书/权限不自动修改；真实测试仅专用1/2重新核验后执行。CODEBRAIN不读写，全部状态在本项目。

## 已保存版本

- 8303cdc：列表精简基线已推送，完成删除全部分组入口/卡片和屏幕模式/实例目标下拉及重复连接入口，三态表头统一选择，同步操作归主控。
- c1be294：完整PRD/SPEC/UI_SPEC/TASKS、测试矩阵和控件目录计划已推送；两代理独立审查。
- 7beea8315eafad0118d5189a784afe8236111db6：主控A/当前B投屏目标混用修复已推送；完整证据见A02_MAIN_TARGET.md。
- 08c4f8c9b0057a8c1eda0abb43367f8fb93bb7b4：A03共享外观与命名19源文件已独立验收推送；见A03_APPEARANCE.md。
- test备份始终49e471081d2648a4d533cc98ec00be4f5a103c77。

A02主仓149/149（51.17秒）和最终1000×700界面通过；独立verifier149/149（51.60秒）及三尺寸12页、25守卫通过。主仓DLL摘要A58A27BD9437F07503A8F5637C28528DDB48BC930F9867A514E0B3381E5FBC29。没有调用真实外部确认、scrcpy启动或嵌入；不能声称PASS-LIVE。

## 当前进行

A04最终已独立PASS-OFFLINE共享层、由本阶段提交推送，当前HEAD以Git记录为准：14源文件；A04.patch摘要EF7D087D56AA2CA5AA825483214C715E6679AE48E2BB4322A96952A771432054及仅夹具增量5BB4928E12B68EB4AE9505784DC6DA1AC560F156983D9966DD442A80582849E9。两个新增文件git apply仅CRLF转LF，14归一化内容逐字相同。主仓150/150（51.41秒），最终DLL79E3F633E6594DF811D5F6DB563A849CC534BA9569E470128A09B7CF515771BF；390shell135及1000完整209外观/25守卫/12idle通过，root目视。

独立apple-responsive-verifier最终150/150（51.90秒）、六shell每轮135、三桌面每轮209外观/25Native守卫/12idle，error空、恢复设备一致，目视390壳层/抽屉/实际模态及中大屏；三桌面tab0与最终图SHA相同。证据artifacts/apple-responsive-verifier/final-toggle。首轮1440旧Toggle断言FAIL+r2通过仍保留；仅补焦点前置/真实帧/分阶段诊断后重建重验，生产Theme未改，旧失败根因未确诊。旧modal透明黑底只Content取证已拒，最终取实际Window，系统标题栏像素未验。

所有必需exec/GUI已结束、GUI锁释放；未操作设备/profile/ADB，完整目标仍active。A04报告和P2 AP-011/012是当前范围边界，不可直接外推全部页面或实机通过。下一行动A05，不重做A02/A03/A04或将历史绿色替代最终业务验收。

## 后续依赖

A04实现已统一四契约与默认值，完成导航699/700与1099/1100、Field自身460、More菜单、Owner-fit。六尺寸只证明公共层，三尺寸完整smoke业务断言保留；A05-A09固定多栏不得冒称适配通过。AP-011首次Fit失败事件订阅可能残留、AP-012页脚ellipsis/tooltip在触屏不完整，两项P2已登记A10；不把这些路径算已修。

A04批准并push后才实施A05：详情移除全局StatusFilter、全选/清空/群控checkbox，保留独立无过滤单设备选择、四子系统状态、改名/保存。设备列表唯一可见状态筛选应包含明确完全停止的已关闭实例，不用进度文案推断Ready；监控使用未过滤统一目录与原范围，不再继承列表筛选。保留隐藏勾选清除、Current/Main/应用独立目标不变保护，并迁移原测试而非删断言。随后列表关键操作冻结可达，主控窄屏可展开返回，监控330卡/308图表适应宽度。该全局筛选已确定会跨页丢项/停流，是P1而非仅候选。A06-A12按原任务推进。

A01目录为103个页面可见键，加全局/源码补充和逐字段细表，全部未执行项维持NOT-RUN。已纠正不存在的终端取消、文件目录编辑、API归档、脚本编辑等推测，不新增这些未要求功能。当前工具页是代理解析、root核验、时区和本地资料包，不是设备备份恢复。

## 测试与环境

便携SDK：上级.tools/dotnet/dotnet.exe；pwsh7.4.20，不升级。Build.ps1 -Verify、Desktop --smoke使用artifacts内独立配置且GUI串行。原始图/日志/数据不上传，正式源码/测试/脱敏报告及三份生成清单提交。不要强制关闭用户进程。

只读ready检查（不是最终目标证明）：雷电23实例，仅5运行，专用1/2关闭；共享5037为E:/leidian/LDPlayer14/adb.exe，PID62248、出生2026-10-15T09:23:48.773808+08:00。原始日志artifacts/apple-experience/readiness。真实动作前重新冻结，不沿用该旧快照放行。

用户profile data/monitor-verified/AppConfig.json摘要E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0，autoStartApi=false，授权1/2/3/5；保留现有变化，不回滚成旧摘要/旧授权。

AP-008：没有AndroidSDK/build-tools/android.jar或自有测试APK。发现已签名Temurin21 JDK但不等于完整工具链。A07需先核验隔离官方/开源工具与许可、构建无账号唯一本轮包；不拿抖音清数据/卸载凑覆盖，不自动接受第三方或敏感许可。继续其他安全任务。

## 保留工作

两个原有未跟踪文件docs/verification/desktop-english.png和desktop-english-settings.png，不提交、不删除。所有旧worktree不清理。不要回滚用户/代理变更。每次push前检查清单/ZIP排除data/artifacts/bin/obj、APK/SDK/二进制并核远端；分支codex/unified-device-access，私有zhuy3075-ui/matrix-device-ldplayer。
