# 独立雷电实例验收报告

日期：2026-10-14。结论：本机单独实例的基本设备调试范围已完成实机验收与修复复验；不是原厂全部功能、全部控件或长期生产稳定性的无条件认证。

## 目标与测试环境

- 先保存根目录plan.md，再按测试用例执行、留存失败、定位、修复及复验；未读取或使用旧CODEBRAIN。
- 用户批准创建空白实例、安装提供的APK并保留实例。仅操作实例1，名称MatrixAcceptance-20261014-184112，授权串号emulator-5556；未读取或操作原实例0的guest数据。
- 雷电14，Android API34，兼容ARM64的native bridge。APK包名com.ss.android.ugc.aweme，实际版本40.7.0/400701；Android接受安装不等于厂商来源或签名认证。
- 应用隐私指引由用户自行处理。未代登录、点赞关注、发布、支付或接受敏感权限；未清数据、卸载、root、恢复或删除实例。

## 已完成结果

| 范围 | 证据与结论 |
|---|---|
| 构建与离线回归 | 六项目Release，0警告0错误；最新68/68通过（final-runtime-recovery-tests.txt），含带旧agent备份恢复后host-only运行时可打开；离线与demo不计实机成功 |
| 简体中文与布局 | 12页、侧栏、搜索、卡片/屏幕/列表、1000×700及1440×900独立渲染通过；繁忙/离线/空态与未支持控件门控有断言 |
| 连接保护 | 只连接既有ADB服务，授权串号、固定服务PID/出生时刻、transport-id及LD绑定实际进程核对；不执行adb.exe或start/kill-server |
| 截图与节点 | 真实PNG、裁剪/取色、静止页节点、选择与旧框清理通过；动态视频页面节点可能无法生成，明确报错，不伪造旧节点 |
| 输入 | 实际点击、滑动、Home/Back；ASCII与中文/空格/符号精确回读；中文全局输入走经进程核验的雷电接口，不冒充指定节点set-text |
| 应用 | 给定APK首次安装、版本读取、launcher alias启动及进程/截图、停止且安装保留通过 |
| 文件与脚本 | 临时文件双向SHA256一致；实际队列、launch/wait/swipe/click、查重、停止取消通过；停止重试为从头重传，不是断点续传 |
| 普通API | B01-B10全部真实验证，包含普通文字API中文回读；逐条请求/响应和实际效果保存 |
| Automator | 27项全部登记：20项业务/请求成功、6项拒绝路径、1项backend诊断；不是27项业务全通过，详见API_MATRIX.md |
| 雷电写API | runapp/killapp/input/Home四条HTTP实际效果通过；最初CLI创建/启动实例通过。HTTP冷启动、退出、重启、CLI安装及音量/Back分支未实测 |
| 实机GUI | 最新Host构建在长测后live-ui-delivery再跑8组全部通过，退出0，包含真实ViewModel输入、中文、截图/节点、应用/Shell、文件队列及12页/3视图；控件清单OBSERVED-NOT-EXERCISED不算逐按钮业务PASS |

## 修复记录

实际失败及处理保留在TEST_LOG.md与DEFECTS.md：缺monkey与launcher alias、动态节点无XML、系统android包校验、旧节点残留、下载长度校验、跨功能忙态、API面板自我阻断/运行端口、Context滞后及错误绑定风险。测试前置错误也如实记录，不当成生产代码缺陷。

配置恢复额外保护：null授权归一为空，非法授权拒绝；从备份恢复时只恢复一般设置，清空授权、雷电绑定及agent设备地址绑定，提示重新确认，不能静默恢复旧的设备权限或因旧agent授权阻塞设置页。

## 稳定性与独立验证

- 第一段长测因绑定保护加强停止自有runner，只完成92次读取，不计30分钟通过；共享服务没有停止或重启。
- 最终长测正常退出0：2026-10-14 19:54:51至20:24:52（北京时间），实际1801518ms（30分1.518秒），222次真实读取、30次截图、10次节点、8次资源采样；272条记录全部PASS-LIVE、FAIL=0，摘要272-soak-summary.json。冻结frozen-final目录及证据保留。
- 运行进程工作集采样54.4至65.5MiB，最终64.9MiB；private bytes22.1至27.7MiB，句柄321至374；读取最大14ms。记录未发现本场景崩溃或服务代次变化，不据30分钟采样宣称无内存泄漏或所有环境长期稳定。
- 长测冻结的是后端/运行时程序集；后续Host改动仅涉及运行端口读取及配置恢复，须以最新离线/GUI定向回归补证，不能称最新完整GUI二进制也持续运行30分钟。
- 两名独立审查者复跑离线测试/两尺寸demo，并检查真实证据边界；配置恢复最终独立68/68（verifier-settings-recovery-final.txt），原发现已闭环，未扩张为未知功能放行。
- 最新Host修复后独立delivery demo两尺寸均退出0、12页、zh-CN、error为空，证据verifier-delivery-ui.json与verifier-delivery-ui-small.json。当前Desktop SHA256为778140716B47FD1269AE65A66BBF73E1D6EFF8C9D65EA856BA31C4D42536EA5E，Host为F5BB911ADF763FE56932963451A9BF9EE03BB93122915F2DA3D6686E3938ECE7。
- 冻结Core与最新Core一致，SHA256为9653A0071621E31A7E8CA681E047A745436E701F7F3692631E8DEEB10A2E05E7。host-final再次记录服务PID44528/出生时刻10:42:23.355051Z/protocol0029/transport2不变；原实例0进程在8次资源采样中保持49596/75704，未操作其guest。
- 第三位独立审查者只读核验长测：272份证据哈希、30份PNG签名/尺寸/哈希、10份各37节点的树及17份冻结文件清单全部一致；读取序号0至221连续无重复。P95读取9ms、截图167ms、树2083ms。核验通过仅限冻结运行时单实例长测，不代表最新Host/GUI资源或零泄漏。

## 剩余限制

本机API只监听loopback、没有身份认证，应限制为可信本机调用者和单一写控制器；GUI/脚本入口忙态不是全生命周期并发lease或跨进程安全锁。其他控制器改变实例时，LD目录观测与投递仍有时间窗口；实例重启后需重启本程序重新核对绑定。

未承诺：任意动态视频节点、指定节点文本/私有phone agent、实时scrcpy与音频、双实例不同尺寸同步及20台容量、商业登录/市场、ROM/SIM/代理部署、root/时区/恢复、全部控件逐按钮点击、跨设备延迟回包和所有并发竞态。这些保持未实现或未验证，不能靠成功拒绝请求算业务完成。

## 复验与运行

在本机双击tools/Start-VerifiedDesktop.cmd（或PowerShell 7运行对应.ps1），使用data/verified-acceptance中的唯一已核验配置；先保持测试实例运行，不同时使用其他控制器。源码ZIP不含该机器授权配置、APK、SDK或实机截图；其他电脑需构建并显式重新授权，不按索引公式猜测串号。

离线复验：tools/Build.ps1 -Verify；界面复验：Desktop.dll --smoke --data-dir 独立绝对路径 --screenshot 绝对路径。源码按SOURCE_MANIFEST.json重新计算SHA256，并将docs/reference的11份文档与原目录逐字节比较。

完整原始日志在私有artifacts/acceptance/20261014-initial，公开报告只保留摘要及证据ID。SOURCE_BUNDLE.md包含全部源代码文本，PROJECT_TREE.md列目录，SOURCE_MANIFEST.json列文件哈希。

测试用例逐行结论见TEST_RESULTS.md，文档缺失与冲突处理见../ASSUMPTIONS.md。docs/reference的11份文件已与原参考目录逐字节SHA256核对一致。
