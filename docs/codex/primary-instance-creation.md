# 空目录新建编号0的闭环修复

日期：2026-10-18。仅处理本次“新建空白无反应”反馈；全面测试继续暂停。

## 事实与目标

截图显示唯一实例0已实际生成，却被`LdInstanceEditor`拒绝确认；后继配置、启动、接入、列表和监控又有独立的0限制。雷电官方说明`add`自动分配索引，并明确示例`launch --index 0`。此前“AI仅测试1/2、不碰原0”是测试执行范围，不能等同产品永久禁用正常编号0。

目标是空目录新建0可准确确认、配置，用户之后可显式启动和接入；当前已生成的0也能正常显示，不必再次创建。新建仍不自动启动，成功反馈明确下一步。

## 方案与边界

推荐统一正常索引下限为0；负数、桌面哨兵99999及原有上界保持拒绝。只改索引条件和对应文案，不改正数generation/PID/birth/transport、Frozen授权、实际串号绑定、逐目标写租约和操作白名单。关闭/重启/资源修改仍需原确认，启动仍仅明确用户动作，刷新不启动。

更小方案“仅修Create识别0”会立即卡在Configure，仍不可用，故拒绝。保留占位0后自动再add浪费磁盘且扩副作用，拒绝。新增名字猜测、额外授权数据库或临时内存令牌不能可靠解决重启后0又不可用，也不采用。

真实范围变化：用户“一键启动全部”的目录包含0；用户启用的本机HTTP接口也接受0。原接口仅有loopback、JSON/Origin检查，没有token或逐请求UI确认；launch/quit/reboot按目录代次及写入门执行，不要求先接入ADB，应用/input才有额外原已接入身份门，不能泛称所有请求都具有serial/PID/transport授权。默认不自动启用接口。不把此产品能力当作AI获准操作0，当前回归仅假CLI/Demo与自有WPF；不新建、删除、配置或启动任何真实实例，不发guest请求、不重启ADB。

## 实施与验收

1. P0：独立测试树用空目录→唯一新0、配置0、显式启动0、负数/99999、歧义/错名/失败不重发的红测定义语义。
2. P0：创建/配置/启动/接入/列表的7源保持一致；关联资源读取、监控资格、采样和窗口目标的5源同步修正，否则不能声称新0完整可用。共12生产文件，单行局部条件变更为主；这是从单文件修补升级为有规格的同一索引规则修复，不做其它架构调整。
3. P0：同一正式测试旧红→新绿；保持241项旧回归，旧测试中“0非法”的预期明确迁移为负数/99999反例并补0正例，不删断言掩盖失败。严格实机入口限定1/2不变。
4. P1：自有未显示WPF专项检查0行可选、状态和命令资格，以及创建成功的反馈；不冒充物理GUI。独立审查源码、失败记录和清理所有权后集成。
5. P1：验证通过后仅保存该项源码、正式测试、README/本报告和三源码清单到原私有开发分支。保留四份无关未提交文档、两PNG、真实配置与A07候选。替换时仅正常退出矩阵程序，不强杀或关闭雷电；程序不愿正常退出则停止替换。

## 当前状态

代码根树`D:\MatrixDevice.Acceptance\primary-instance-fix`，独立测试树`D:\MatrixDevice.Acceptance\primary-instance-tests`。12生产源及8正式测试源已经独立只读静审；所有运行仅由主代理执行。

- 本机只读`list2`确认唯一实例为`0,矩阵测试-1018-113241`且完全停止。直接PowerShell首次读取的中文解码不正确，原记录保留；后续使用原CLI专用只读路径及既有GBK解码取得正确JSON，不构造ADB后端或API。没有修改或启动此实例。
- 旧生产加新测试的red-r01构建通过，9项既有警告/0错误。Host实际239/248、9项预期产品红、0跳过：创建0、配置0、启动0、内存读取0、采样0、自有窗口恢复0、凭据0、写租约0及陈旧接入0。
- red-r01 WPF在原按钮定位处失败，0项产品检查、任务全终态/单次关闭/0窗口及保留所有者；属于夹具定位前置未建立，不追认产品红。原日志保留，修正夹具后需重编重跑。
- 界面夹具补原Shell的未显示布局，按原按钮精确Automation Name定位，仍使用原Command，未新造按钮或调用Show。red-r02旧生产WPF实际4/12、8个预期产品红（含负数模型行保护），无夹具异常，全部原任务终态、0清理错误/残留窗口/所有者。
- 候选首轮构建9项既有警告/0错误；最终green-r02增量构建8项既有警告/0错误（15.00秒），Host248/248、0失败/跳过（60.81秒），新编号0WPF12/12、旧删除残留WPF12/12均通过。两个WPF报告全部登记Task终态、单次shutdown、0窗口及保留所有者、无清理错误。
- 严格专项入口8项早拒通过：缺Demo、相对/已有目录、混合模式、重复参数、未知参数、缺值、非artifacts目录；均退出1且未创建待测目录。20个最终源码字节摘要保持，八测试文件在红/绿树精确一致；测试夹具修正不改变12项产品预期。

所有原始流在两树`artifacts/primary-instance/`保留，不上传配置或原始证据。Host原自有窗口用例会显示自有进程的合成HWND，不是真实模拟器0；WPF不执行创建对话框、启动/连接或真实反馈点击。没有对当前真实0执行add/modify/launch/quit/guest，也没有为了修复再新建或删机。

## 如何复验

先用`tools/Build.ps1 -DotnetPath ABSOLUTE_DOTNET_PATH`构建，再运行`tests/MatrixDevice.Tests/bin/Release/net10.0/MatrixDevice.Tests.dll`。本机视频相关旧回归通过进程级`MATRIX_TEST_SCRCPY_SERVER`和`MATRIX_TEST_FFMPEG`使用已有组件，不下载或修改全局环境变量。

```powershell
dotnet ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --verify-primary-instance --demo --data-dir ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY
dotnet ./src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll --verify-deleted-devices --demo --data-dir ANOTHER_ABSOLUTE_FRESH_ARTIFACTS_DIRECTORY
```

两入口使用各自尚不存在的artifacts绝对目录，分别输出`primary-instance.json`和`deleted-devices.json`，不能混用模式。测试通过仅代表本次离线索引规则与原控件命令资格，不代表真实创建/配置/启动0、原生新建对话框、物理GUI、实际视频0、本机HTTP0全链路或全面验收通过；这些仍未执行。

源码归档沿用上一项的字节核验方式：对未修改文件，仅在CRLF→LF后精确匹配上一版长度/SHA时沿用原归档字节，避免无关换行噪音。20个被测源码不改写；四份未提交工作文档、两图片、配置及D盘A07候选不纳入。未来恢复A07须合并本次App专项入口，不可直接覆盖App。

## 主仓收尾

精确集成20源码后，主仓Release构建9项既有警告/0错误（25.06秒），`main-r01` Host248/248、0跳过（59.82秒），primary与deleted各12/12、共13任务全终态，各唯一shutdown、0残留窗口/所有者/清理错误。六份既有无关文件和真实配置SHA保持，接口自动启动仍关闭；所有专项进程已退出，不恢复此前全面测试。

此项仍保存到私有仓库`zhuy3075-ui/matrix-device-ldplayer`的`codex/unified-device-access`开发分支，仅20源码、README、本报告和三生成清单；排除本机配置、APK、二进制、原始证据及A07候选，不合并或发布。现有真实0不重复创建、不自动启动；正常程序重新打开后由用户选择该行“启动”。

旧版这次误报在Create阶段抛出，后继Configure未执行；升级不自动重放历史表单或补写资源。现有0可保留当前配置直接启动，需要当时输入的CPU/RAM/尺寸时，由用户在“实例操作→资源配置”重新保存再启动。

来源：[雷电官方命令行说明](https://help.ldmnq.com/docs/LDPlayer-ming-ling-hang-kai-fa-zhe-shi-yong-shuo-ming)，2026-10-18重新读取；使用创建章节与索引0的正常启动示例，不据端口公式授信。
