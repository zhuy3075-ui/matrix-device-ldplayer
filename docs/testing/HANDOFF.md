# 测试阶段交接

## 当前多实例专项

- Goal：复用现有多开，独立1/2并发上传/APP安装启停/差异脚本/文字/回传/型号，并监测资源；HTTP Toolkit按安全范围采集，不擅自改变防火墙、证书或共享ADB。
- Current：pressure-normal-final-direct session58073已退出0；pressure906113ms、A68/B58、129PASS/0FAIL，126回传。安装时间独立排除，Tests/Core/Features/Host冻结清单与当前DLL/spec经独立核对。
- Fixes：Host按已核唯一serial收窄跨实例LD写门禁；Core仅10列启动flag2视为未就绪。harness先经历脚本/旧坐标/Home断言缺陷，短预检通过后首次长测B设置-W超时失败；改正常导航/45秒预算，并实际确认搜索hint后精确限定。
- Verification：最新完整72/72、独立72/72、0警告0错误；正式129哈希、126文字/shell nonce/SHA/ack及末轮文件/节点独立限域通过。CPU median19.40/max25.13，最低可用2.758GiB。两个demo窗口12页/3视图通过，不冒充全部控件实机验收。
- Live：原0 PID49596/VBox75704；server44528/birth639275713433550518。实例1 Pixel 8/Google、emulator-5556/transport6、VBox58700/birth639275803518833964、player36812；实例2 SM-S9180/samsung、emulator-5558/transport7、VBox49164/birth639275803629299939、player35824。旧单实例回执代次已过期，保留只作历史。
- Evidence：artifacts/multi-instance/20261014-2045；失败长测pressure-final-direct不能算通过或拼接时长。正式normal轮proxy0，直连HTTP记录不是HTTP Toolkit。
- Blocked scope：HTTP Toolkit隔离启动/MCP只读成功后因代理全接口监听+Public防火墙关闭停止；用户尚未回答网络隔离风险确认，不再启动、不改规则/profile。没有APP自身TLS抓包。
- Cleanup：idle session90935已退出0，3分钟VBox句柄未回落；CLI同多配置实际bootstrap退出0/两个Ready/无error；host-after核原0及5037身份，自有测试/HT端口均释放，实例和APP保留。双实例WPF启动器静态通过，真实入口未实跑。
- Next：生成/核验新版源码包后交付；HT业务抓包等用户网络隔离风险确认。未来先1小时资源/句柄类型专项，不扩实例、不替用户升级或重启平台。
- Risks：VBox句柄正式A+803/B+715且停载3分钟未回落，WATCH未归因；两台不是最大容量，无GPU/24小时长测。型号只是字符串，未请求/读取敏感硬件标识。API门禁非跨进程事务锁，GUI忙态仍全局。

## 历史单实例记录

- Goal：独立雷电实例基本功能真实验收，日志驱动修复与复验。
- Context：已保存plan.md与测试计划；不使用旧CODEBRAIN。用户授权空白实例及APK安装，已自行关闭极星。
- Done：APK静态包名com.ss.android.ugc.aweme/40.7.0/arm64-v8a；现有服务socket保护、授权串号、transport/服务PID代次及Unicode实际VBox绑定；最新68/68离线测试，0警告0错误，独立68/68与两尺寸delivery demo通过。
- Evidence：artifacts/acceptance/20261014-initial/server-owner-tests.txt、ui-desktop.txt、ui-small.txt、host-before-create/。
- Live：已创建并保留实例1，serial emulator-5556、VBox70760、server44528、transport2；目标回执在instance/verified-target.json。原0 PID49596/VBox75704保持，无guest读写。
- Done live：APK安装40.7.0/400701；启动alias/停止、文件SHA256、脚本4种动作及取消；点击/滑动/ASCII+LD中文+全局绑定中文+普通API中文；A01-A27逐项含6拒绝及1诊断，B01-B10全部成功；LD run/kill/input/home四写操作实际效果通过；GUI8组包括中文、真实节点与队列通过。
- Soak：最终session8124已exit0，1801518ms/222读取/30截图/10节点/8资源，272条0FAIL；首段23640停止的92次不计本段。Core冻结与当前哈希相同；Host后修只作最新离线/GUI补证，不声称完整GUI同二进制30分钟。
- Latest：live-ui-delivery最新实机8组全部通过exit0；配置backup恢复一般设置但清空串号/LD/agent授权，host-only设置入口有回归。下载Current/上传Targets、实际运行API端口、跨功能Busy均已有新证据，未发现本轮基本流程新开放P0/P1。
- Next：交付源码包/目录/全部源码文本；其他机器需显式重新授权，不分发本机data配置。后续多实例、动态节点/可信agent/scrcpy及危险操作需专项验收，不使用原0凑覆盖。
- Risks：用户自行处理协议且已回复已处理，不代理登录或敏感权限。节点set-text/agent/实时scrcpy/双实例/root等无真实通过证据。LD quit/reboot/installapp业务路径未实测，不包装8操作全通过。本机API无认证，单一写控制器约束仍在；新互锁不是跨进程安全锁。
