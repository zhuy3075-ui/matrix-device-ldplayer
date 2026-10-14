# 多实例压力与差异化任务测试计划

文档版本：doc-v20261014.2；状态：两实例基础压力完成，HTTP Toolkit业务抓包待网络隔离确认；日期：2026-10-14。

## 目标与范围

复用现有LdPlayerService/LdConsoleRunner、ADB按serial队列、ScriptScheduler和同一个MatrixRuntime，验证多开并行稳定性、差异任务隔离、文件/安装/脚本/数据回传、HTTP Toolkit捕获与模拟型号字符串生效。只使用独立测试实例；原0不授权、不读取guest、不写入。

主机32GiB/i5-14400F，初检空闲约5.8GiB且波动，不能据总内存直接启动很多台。推荐测试实例1加一个新空白实例，先1→2台；第三台仅在连续资源采样空闲>=5GiB、commit<80%后考虑。更简单方案是只做两台差异流水线；拒绝直接20台或复用原0凑数量，以免影响当前工作与账号。

## 第一性原理与约束

已知：已有多开CLI、逐index/serial队列及冻结服务/transport/PID保护；型号公开业务入口未开放；ADB SYNC/exec上传安装不是HTTP；HT1.27.2已安装但默认启动会探测5037。

根问题是同一个共享运行时中任务隔离与实际效果，不是出现多个列表行。最小实现是专用多实例测试runner，不新建业务多开管理器；每台一个顺序流水线，台间Task.WhenAll。旧全局LD脚本门禁需要按唯一已验证绑定serial收窄，未知绑定仍保守全局拒绝。入口检查不等于完整事务锁；不在同一台并发混入多个写控制器。

型号只修改model/manufacturer字符串，不改IMEI/IMSI/MAC/Android ID/root/CPU ABI。只在测试实例冷态modify，随后quit/launch和重建runtime；不得直接编辑雷电config冒充SDK生效。旧实例1将正常关闭/重开，保留APP数据。外部控制程序需保持不操作测试目标。

## 阶段与验收

| 阶段 | 任务 | 验证/证据 |
|---|---|---|
| P0-准备 | 只读主机/list2/服务身份/资源，创建差集唯一空白测试实例 | 不重复add、不copy；创建回执、原0两PID保持 |
| P0-离线 | 复现A脚本挡B LD写，窄修明确串号门禁 | A仍409、B200、未知绑定保守拒绝、UI Busy全局保护；完整离线回归与独立verifier |
| P0-型号 | 保存测试目标原model/manufacturer；冷态设置不同型号 | 重新启动后getprop及DeviceInfo.Model精确匹配，不把CLI退出0当生效 |
| P0-启动 | 复用launch，1→2等待就绪，记录真实TCP/PID/birth/transport | boot=1、非空实际截图、唯一index/serial，服务epoch变化则停止旧runtime并检查，不主动重启ADB |
| P1-并行 | 一个runtime，差异流水线：大文件低频vs小文件高频、不同脚本/间隔 | 相同guest路径不同nonce/SHA，push/pull及回传均对应本机；开始结束区间确实重叠 |
| P1-行为 | 每台提供APK安装/版本、启停、静止设置页脚本、上传/回传 | 不代接受协议/登录/社交/支付；脚本实际步骤/状态，安装取消后重查，不声称回滚 |
| P1-抓包 | HT隔离启动，仅显式HTTP代理本机API与测试回传 | 子进程ADB端口隔离+ANDROID_HOME不存在；共享5037身份前后不变；只使用免费可调用读取，不伪造Pro/HAR |
| P1-压力 | 至少15分钟两台差异并行，间隔带可重复think-time | JSONL每步时间、目标、效果/哈希；每10秒主机+自己/目标进程CPU/内存/句柄及服务/原0快照；延迟分位、错误与吞吐 |
| P0-收尾 | 停自己任务/监听，保留实例与应用，独立证据审核 | 原0不变、失败/复验保留；REPORT/LOG/目录与源码更新 |

## 止损与失败模式

串台/身份变化或任一业务失败/超时立即停止自有工作；空闲<2GiB、commit>85%、CPU>90%持续60秒停止加压并取消自有任务，不关原0或kill-server。启动前至少预留3GiB给系统；未达条件不加第三台，并如实报告测试容量。

资源监控只证明本机本次负载，不宣称所有硬件长期稳定。型号修改不改变真实CPU或设备兼容性。LD索引观测与命令投递仍有外部竞争窗口，重启后必须重建授权冻结。动态视频节点继续保持已知限制。

HTTP Toolkit capture只记录经过其代理的HTTP；APK/文件实际ADB传输另用字节哈希/安装版本。App自身HTTPS需要另行代理/证书范围确认，不注入证书、VPN或Frida，不改系统代理，不抓账号/支付。HT若监听全接口，先停推进并处理暴露风险，不擅自改防火墙。

## 日志与交付

原始证据在私有artifacts/multi-instance/<runId>/，不进源码ZIP。包含prepare receipts、targets.json、每步JSONL/HTTP request IDs/真实效果、resource samples、HT outline与合法抽样正文。公开docs/testing/multi-instance只保存结论与证据ID；重复失败最多3轮，同一根问题超过则升级，不吞异常。

使用当前可见pwsh7-windows（环境7.4.20，采用兼容语法不升级）、programming-management、loop-verifier及实际只读子代理。无superpowers使用手动等价SOP，不读CODEBRAIN，不创建自动化/持续goal，不发布或删除任何实例。

## 执行状态

- P0准备/门禁/型号/启动：完成；原0只读主机目录观测。CLI启动过渡2兼容修复受限定，不标就绪。
- P1并行/行为/压力：正式正常导航轮906113ms、A68/B58、129PASS/0FAIL，独立哈希/时长/连续并行/回传核验完成；失败轮不拼接。
- P1抓包：仅隔离启动及官方MCP只读接通，实际业务捕获未完成；代理全接口+当前Public防火墙关闭，等待用户风险确认或已有隔离环境，不自动改变网络。
- P0收尾：测试自有监听已关闭、实例保留；5037 PID/出生时刻及原0主机PID保持。静置3分钟句柄未回落，保留WATCH，不放行最大容量或全天稳定性。
