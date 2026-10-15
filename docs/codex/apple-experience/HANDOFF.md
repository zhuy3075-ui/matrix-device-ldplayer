# 当前目标交接

更新：2026-10-15。完整目标仍active，未达到全功能、全控件、真实设备、六尺寸和最终稳定性完成门槛；不要调用complete或把离线通过换算为实机通过。

## 用户目标与权限

自主逐路径测试/修复全部原功能，消除真实重复职责，采用Apple HIG启发的浅色、简短中文和响应式体验；保持12页Tag/接口/数据与安全边界。每个功能独立验证后push原私有功能分支，不问重复问题，不合并/发布/部署。原实例0、用户账号/抖音数据、共享ADB、系统代理/证书/权限不自动修改；真实测试仅专用1/2重新核验后执行。CODEBRAIN不读写，全部状态在本项目。

## 已保存版本

- 8303cdc：列表精简基线已推送，完成删除全部分组入口/卡片和屏幕模式/实例目标下拉及重复连接入口，三态表头统一选择，同步操作归主控。
- c1be294：完整PRD/SPEC/UI_SPEC/TASKS、测试矩阵和控件目录计划已推送；两代理独立审查。
- 7beea8315eafad0118d5189a784afe8236111db6：主控A/当前B投屏目标混用修复已推送；完整证据见A02_MAIN_TARGET.md。
- 08c4f8c9b0057a8c1eda0abb43367f8fb93bb7b4：A03共享外观与命名19源文件已独立验收推送；见A03_APPEARANCE.md。
- 2b1fe6e：A04公共响应式已独立验收推送；15155a708802199d725d05523d02ceb8cb49f78a：A05a详情/筛选隔离已推送。
- test备份始终49e471081d2648a4d533cc98ec00be4f5a103c77。

A02主仓149/149（51.17秒）和最终1000×700界面通过；独立verifier149/149（51.60秒）及三尺寸12页、25守卫通过。主仓DLL摘要A58A27BD9437F07503A8F5637C28528DDB48BC930F9867A514E0B3381E5FBC29。没有调用真实外部确认、scrcpy启动或嵌入；不能声称PASS-LIVE。

## 当前进行

A04最终已独立PASS-OFFLINE共享层、由本阶段提交推送，当前HEAD以Git记录为准：14源文件；A04.patch摘要EF7D087D56AA2CA5AA825483214C715E6679AE48E2BB4322A96952A771432054及仅夹具增量5BB4928E12B68EB4AE9505784DC6DA1AC560F156983D9966DD442A80582849E9。两个新增文件git apply仅CRLF转LF，14归一化内容逐字相同。主仓150/150（51.41秒），最终DLL79E3F633E6594DF811D5F6DB563A849CC534BA9569E470128A09B7CF515771BF；390shell135及1000完整209外观/25守卫/12idle通过，root目视。

独立apple-responsive-verifier最终150/150（51.90秒）、六shell每轮135、三桌面每轮209外观/25Native守卫/12idle，error空、恢复设备一致，目视390壳层/抽屉/实际模态及中大屏；三桌面tab0与最终图SHA相同。证据artifacts/apple-responsive-verifier/final-toggle。首轮1440旧Toggle断言FAIL+r2通过仍保留；仅补焦点前置/真实帧/分阶段诊断后重建重验，生产Theme未改，旧失败根因未确诊。旧modal透明黑底只Content取证已拒，最终取实际Window，系统标题栏像素未验。

所有必需exec/GUI已结束、GUI锁释放；未操作设备/profile/ADB，完整目标仍active。A04报告和P2 AP-011/012是当前范围边界，不可直接外推全部页面或实机通过。不重做A02/A03/A04或将历史绿色替代最终业务验收。

A05a已整合11个Desktop源/测试文件，最终补丁A12E4C2E3134B1D425E9EB04971C2A6CE3AA30EA705FBFB29521D861256C9ED4；与实现/独立归一化一致，仅新DeviceFilterSmoke行尾差异。正式主仓150/150（51.34秒）、独立150/150（51.40秒）；独立三桌面每73专项（含32分类/22实际监控协调）、209外观/25Native/12idle；六公共shell各135。主仓最终1000完整及390shell复验并目视通过，DLL DE075D83B35FA9DA5322AA0CD15731BC29E5F77F0E71D7FDD84E5FA76D4A8D48。详情控件/新数据库回读由实际断言验证，没有独立详情截图或tooltip像素证明；不宣称实机视频连续性、命令权限负例或小屏正文全过。红测4→0和green1/2夹具错误保留。最终报告A05_DEVICE_STATE，本阶段commit/push以Git为准。

A05b文案AP-013/014已独立验收：冻结patch298F8EBACF345A22BB028847B1B90C8B4D95E5A2E40757F5433984A3ED9E5A46，5源raw SHA全同，无分类/门禁/协调器改动。实现150/53.67s、独立150/52.14s及三桌面各36labels/11观察、原73/32/22、209外观/25Native/12idle、390shell135首次全过。主仓150/51.60s、最终1000完整同样通过并目视，最终DLL2EF968DDC1C51C2171246928A50D2ECC693AF497C6D052EDB3190F7B3145395D，用户配置未变，全部exec/GUI已结束。报告A05_DEVICE_LABELS，当前commit/push以Git为准。专项commands never invoked，不代替权限/实机；首次red-1000旧Appearance失败和red2文案19失败/17通过保留，AP-015原因未解。没有声称旧静态Tooltip必然陈旧。

## 后续依赖

A04实现已统一四契约与默认值，完成导航699/700与1099/1100、Field自身460、More菜单、Owner-fit。六尺寸只证明公共层，三尺寸完整smoke业务断言保留；A05-A09固定多栏不得冒称适配通过。AP-011首次Fit失败事件订阅可能残留、AP-012页脚ellipsis/tooltip在触屏不完整，两项P2已登记A10；不把这些路径算已修。

A05a和文案已完成，A05b1本阶段提交：紧凑选择48/操作148冻结两列，44实际命中、原14信息横滚保留；workspace920切同一主控侧栏/同区域、固定返回与自然正文滚动；MainA/CurrentB/隐藏选择/原滚动意图保留。resize/LostCapture/离页等取消未完成手势，正常Up冻结局部快照后release。下一步A05b2监控330卡/308图表按实际宽，不能伪造资源采样或重建视频。AP-013/014/016不重做；AP-011/012/015及未复现Tab/指针前置候选仍A10。A06-A12保持，不将离线绿色替代实机与全页小屏验收。

A05b1最终7源patch摘要25CD4082425831E6803D6237C33088569ACF69DD49437986D451815E1F92C5E8，source清单070423D33AACE96F26EC4952DD1744C6AF48340C8F4CAF0898C884830BC21032，root/实现/独立7源raw全同。实现150/51.13s、独立150/51.98s；两方各六正文（两390各889/136，其余847/135）、三桌面36/73/32/22/209/25/12idle及六shell135全部通过。独立20入口负例拒绝；其采集器UTF8误读中文stderr，不证明console中文编码。root最终150/51.10s，首轮390640在normal-tap Position前置失败656checks、未Down；后验输入持续变化/当时采样全屏Clip，未确定首败原因。未改源/断言或禁用用户输入，等5秒会话idle后新目录一次复验：两390各889、1000完整及390shell135通过并目视；首败保留，不能称root首次全过。root DLL D4F9CD5A3218D19189E2CD0D3E31152AF52095FE2F29EA8EF4C3CBE554FF03F3，用户profile摘要未变，全部exec/GUI已结束且锁释放。完整报告A05_DEVICE_RESPONSIVE；本阶段commit/push以Git为准，不称实机、自动timer、ADB wire或DPI通过。

本阶段真实红另含header resize grip遮挡、默认非null TextColumn样式使tooltip未装、旧队列恢复盖新wheel、390844中间宽视口maxV5消费原V6快照；均保留红绿。保护行隐形RowHeader低层hit、Number类型、指针缓存/前置、无法构造Image内部gutter、构建与启动交叠均分开记录，不混作产品缺陷；默认外侧父Grid留白和capture拖出实测通过，内部gutter N/A。A05b2仍未实施，当前阶段完成并非全目标完成。

AP-015只读诊断线索：Appearance的probe经AsyncCommand在Space Up时同步CanExecute false→true；可能影响focus，但Task.CompletedTask无必然让出，不能据源码断言丢焦点。模板需要FocusWithin与AnimationsEnabled同时成立，旧恢复断言前没有当前Active/Focus证明。A10首轮仅加覆盖失败finally的有界事件时间线（Click/CanExecute/IsEnabled、Got/LostFocus、Activated、偏好与Rendering，记录实际FocusedElement、Opacity/clock/Popup及阶段）；不补Focus、不改主题/命令取绿。若仍focus有效且有帧却无动效，再查触发器；若失活先发生则分离前置失败。必要第二轮用同步/完成Task/延迟TCS控制组，不能换掉原probe冒称修复。

A01目录为103个页面可见键，加全局/源码补充和逐字段细表，全部未执行项维持NOT-RUN。已纠正不存在的终端取消、文件目录编辑、API归档、脚本编辑等推测，不新增这些未要求功能。当前工具页是代理解析、root核验、时区和本地资料包，不是设备备份恢复。

## 测试与环境

便携SDK：上级.tools/dotnet/dotnet.exe；pwsh7.4.20，不升级。Build.ps1 -Verify、Desktop --smoke使用artifacts内独立配置且GUI串行。原始图/日志/数据不上传，正式源码/测试/脱敏报告及三份生成清单提交。不要强制关闭用户进程。

只读ready检查（不是最终目标证明）：雷电23实例，仅5运行，专用1/2关闭；共享5037为E:/leidian/LDPlayer14/adb.exe，PID62248、出生2026-10-15T09:23:48.773808+08:00。原始日志artifacts/apple-experience/readiness。真实动作前重新冻结，不沿用该旧快照放行。

用户profile data/monitor-verified/AppConfig.json摘要E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0，autoStartApi=false，授权1/2/3/5；保留现有变化，不回滚成旧摘要/旧授权。

AP-008：仍无完整AndroidSDK或自有测试APK；已只读取得Central framework API16、Google Maven AAPT2 8.6.1、D8 8.6.27、apksig 8.6.1候选工件并核checksum/POM/归档，证据AP008_DEPENDENCIES。不要再称“无android.jar候选”，也不要把候选链称为可用构建。D8 manifest入口缺失应走官方class入口，apksig没有apksigner CLI；未执行、编译、签名、接受SDK条款或改环境。A07下一步仍需自有最小源码/签名/Android效果，不能拿抖音清数据/卸载凑覆盖。

A07额外只读准备：fixed apksig public member有setAlignmentPreserved、V1/V2及Verifier范围；官方当前源码的4字节对齐说明不等于8.6.1工件效果，必须最终ZIP LocalHeader offset与签名验证。无res原生Activity/匿名监听器/Prefs回显、min23/target34/exported/无权限是最小候选；framework不含java/javax，D8 JDK-provider实际成员存在，但精确Temurin根路径当前尚待恢复，不能假装已具备完整JDK。工具执行/生成测试密钥/构建APK/安装均未做。不要全盘扫路径或借用用户APP数据；候选研究不算AP008解除。

## 保留工作

两个原有未跟踪文件docs/verification/desktop-english.png和desktop-english-settings.png，不提交、不删除。所有旧worktree不清理。不要回滚用户/代理变更。每次push前检查清单/ZIP排除data/artifacts/bin/obj、APK/SDK/二进制并核远端；分支codex/unified-device-access，私有zhuy3075-ui/matrix-device-ldplayer。
