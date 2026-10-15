# 当前目标交接

更新：2026-10-15。完整目标仍active，未达到全功能、全控件、真实设备、六尺寸和最终稳定性完成门槛；不要调用complete或把离线通过换算为实机通过。

## 用户目标与权限

自主逐路径测试/修复全部原功能，消除真实重复职责，采用Apple HIG启发的浅色、简短中文和响应式体验；保持12页Tag/接口/数据与安全边界。每个功能独立验证后push原私有功能分支，不问重复问题，不合并/发布/部署。原实例0、用户账号/抖音数据、共享ADB、系统代理/证书/权限不自动修改；真实测试仅专用1/2重新核验后执行。CODEBRAIN不读写，全部状态在本项目。

## 已保存版本

- 8303cdc：列表精简基线已推送，完成删除全部分组入口/卡片和屏幕模式/实例目标下拉及重复连接入口，三态表头统一选择，同步操作归主控。
- c1be294：完整PRD/SPEC/UI_SPEC/TASKS、测试矩阵和控件目录计划已推送；两代理独立审查。
- 7beea8315eafad0118d5189a784afe8236111db6：主控A/当前B投屏目标混用修复已推送；完整证据见A02_MAIN_TARGET.md。
- A03共享外观与命名：19源文件已独立验收，由本阶段提交推送；完整结果和边界见A03_APPEARANCE.md，当前HEAD以Git记录为准。
- test备份始终49e471081d2648a4d533cc98ec00be4f5a103c77。

A02主仓149/149（51.17秒）和最终1000×700界面通过；独立verifier149/149（51.60秒）及三尺寸12页、25守卫通过。主仓DLL摘要A58A27BD9437F07503A8F5637C28528DDB48BC930F9867A514E0B3381E5FBC29。没有调用真实外部确认、scrcpy启动或嵌入；不能声称PASS-LIVE。

## 当前进行

A03代码及19源文件已整合，独立apple-theme-verifier的149/149（52.32秒）和三尺寸每轮195外观/25主控守卫/12 idle捕获通过；主仓149/149（51.52秒）与最终1000回归通过，DLL91ABD9705C30B0E31531A0E58850690CB0D1C174C4E25BBC38CFEF3E3993D525。逐页空表证据已拒绝并修复有界取证等待；新tab0与最终图三尺寸SHA相同、目视4行/连接2。失败日志保留。全目标未完成，下一行动A04，不重做A02/A03或把已有绿扩张到实机。当前各测试进程已退出，无必需运行会话。

## 后续依赖

A03先验收并push，随后从新HEAD做A04，不提前混入尺寸修改。A04推荐最低外窗390×640；四契约统一（XAML、smoke Clamp、SettingsEditor.Apply、SettingsStore.Normalize），默认1440×900不变；导航699/700与1099/1100边界、Field自身宽460重排、单一导航抽屉、目标窄屏换行、可点击更多菜单、页脚/错误有界和Owner-fit弹窗。六尺寸真实宽高与390/768保存重载断言。A05-A09仍需逐页固定双栏适配，A10触控/DPI/键盘精修，A11最终版本双实例全路径及30分钟，A12完成审计。

A01目录为103个页面可见键，加全局/源码补充和逐字段细表，全部未执行项维持NOT-RUN。已纠正不存在的终端取消、文件目录编辑、API归档、脚本编辑等推测，不新增这些未要求功能。当前工具页是代理解析、root核验、时区和本地资料包，不是设备备份恢复。

## 测试与环境

便携SDK：上级.tools/dotnet/dotnet.exe；pwsh7.4.20，不升级。Build.ps1 -Verify、Desktop --smoke使用artifacts内独立配置且GUI串行。原始图/日志/数据不上传，正式源码/测试/脱敏报告及三份生成清单提交。不要强制关闭用户进程。

只读ready检查（不是最终目标证明）：雷电23实例，仅5运行，专用1/2关闭；共享5037为E:/leidian/LDPlayer14/adb.exe，PID62248、出生2026-10-15T09:23:48.773808+08:00。原始日志artifacts/apple-experience/readiness。真实动作前重新冻结，不沿用该旧快照放行。

用户profile data/monitor-verified/AppConfig.json摘要E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0，autoStartApi=false，授权1/2/3/5；保留现有变化，不回滚成旧摘要/旧授权。

AP-008：没有AndroidSDK/build-tools/android.jar或自有测试APK。发现已签名Temurin21 JDK但不等于完整工具链。A07需先核验隔离官方/开源工具与许可、构建无账号唯一本轮包；不拿抖音清数据/卸载凑覆盖，不自动接受第三方或敏感许可。继续其他安全任务。

## 保留工作

两个原有未跟踪文件docs/verification/desktop-english.png和desktop-english-settings.png，不提交、不删除。所有旧worktree不清理。不要回滚用户/代理变更。每次push前检查清单/ZIP排除data/artifacts/bin/obj、APK/SDK/二进制并核远端；分支codex/unified-device-access，私有zhuy3075-ui/matrix-device-ldplayer。
