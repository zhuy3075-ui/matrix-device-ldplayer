# 缺失、冲突与处理

| 文档问题 | 实现采用的假设或处理 | 影响 |
|---|---|---|
| 所有参考文档声明是静态反推，不是原始源码；原 BAML/证据和完整 schema 已清理 | 独立实现已描述行为；按 spec 的领域边界建立六个项目和模块目录 | 不能声明原代码或像素级复原 |
| LEGACY_DEVICE_TASKS 把执行入口改为外部雷电任务；本次用户明确要求 reference 全目录还原 | 以本次请求覆盖全部参考功能，保留原生外部 scrcpy 宿主；不读取目录之外的旧 SPEC/CODEBRAIN | 不擅自假定新的雷电索引/API 契约 |
| 原技术栈 .NET Framework 4.8、混淆 DLL 同名不同 SHA | 新工程采用 .NET 10/WPF；使用标准 ADB 与自有协议客户端，不拼接 DLL | 无原产品二进制/数据库兼容保证 |
| 未提供原始 XAML、颜色/字体/间距、登录/额度/远程市场协议 | 实现同职责页面和忙/空/失败状态；不制造商业登录或远程业务 | 界面是功能重建 |
| 未提供原始完整多语言资源 | 按最新要求固定简体中文，旧en-US配置归一为zh-CN；设备身份、包名、命令、用户输入和JSON协议数据不翻译 | 不是恢复原厂多语言资源；本轮雷电特有假设另见LDPLAYER_INTEGRATION.md |
| 视频握手、服务资源和完整解码链未提供 | 保留原协议包解析/外部scrcpy入口；新增监控改用固定SHA官方scrcpy-server3.3.4、既有ADB独立socket和自有FFmpeg解码，截图/节点独立 | 私有JX视频/音频链路未恢复；新监控不冒充原握手，容量以专项验收为准 |
| 同步触摸只给出编码，无可用控制 socket | ADB 模式保存 DOWN/MOVE，UP 执行一段 tap/swipe，切换/掉线取消未注入手势 | 无逐 MOVE 实时触摸承诺；代次检查覆盖鼠标/键盘/文本 |
| Automator swipe为steps/stepMs，普通API为DurationMs | 独立解析，steps*stepMs映射时长；两后端与两API最大60000ms，非法值边界拒绝 | 不是逐MOVE实时触摸；限额为安全假设 |
| 原错误分类与nullable sleep缺省未恢复 | Automator校验400/1007、不存在404/1002、非Ready409/1003、未知404/1006、方法405/1005、内部500/2000、缺能力501/2001；普通逐设备不可用410且ok:false，无code；LD过期409。sleep显式给值 | 分层契约，不声称原版完全一致 |
| 普通字段 PascalCase 与 Automator camelCase | 分离协议但反序列化允许大小写不敏感；重复同名字段拒绝 | 不把缺省坐标偷偷作为 0 |
| 原 HttpListener 依赖 HTTP.sys 权限 | 新 Kestrel 仅 loopback，无系统 URLACL 修改，保持路径/JSON 契约 | 网络宿主不同 |
| 每设备端口冷却时间未提供 | 冷却 30 秒；端口占用跳过；8001..8999 耗尽503 | 防地址立即串台 |
| AppConfig 46 字段仅给类别，无完整名称/schema | 实现已知配置的存储与归一化，三方合并包括清空 null；主→备份→默认；墙列数5..20 | 小屏视频参数/硬解偏好仅保留，未实现其私有解码行为；界面禁用不支持项 |
| 数据库11类实体只有名称和部分字段 | version1 新 SQLite，实体 Id/Payload/UpdatedUtc+明确设备元数据表，参数/分组/备注以受控 repository保存；迁移前备份与完整性检查 | 未读取旧数据，不自动隔离/迁移旧产品坏库 |
| 脚本参数的动作替换规则和 Probability 语义未证实 | 保存并校验参数类型/范围；保存 Probability，不随机跳步或扩展任意脚本语言 | 未知业务规则不生造 |
| 原通用脚本跳过未知动作，spec要求坏包不能假成功 | 导入/运行前严格拒绝未知动作与坏坐标；串行步骤；每serial查重；日志500 | 记录为有意改进 |
| 特定应用循环流程只有名称，无完整行为 | 通用脚本执行一遍，不实现 TikTok 专用工作流 | 专用私有逻辑待补证 |
| 原手机 JAR缺失，调试版私有协议不完整 | 可配置显式 AgentSerial→AgentBaseUrl，已定义 HTTP 请求可接入；无自动部署/共享 forward 清理 | 需用户提供兼容 agent 并验证 |
| 截图 quality 为PNG，未定义对压缩/画质影响 | 校验0..100，PNG保持无损；裁剪真实像素；quality不改像素 | 不按JPEG质量处理 |
| Unicode输入与私有剪贴板/输入法不完整 | ASCII走ADB；全局Unicode仅授权serial+唯一运行LD索引显式绑定时走call.input，先核ADB固定身份；实例代次变化须重启重新核对。指定节点set-text仍需phone agent，无agent501 | 文本页/普通API中文已精确回读；不是节点指定输入或通用剪贴板 |
| 代理runner二进制/路由规则、ROM字段未知 | 可解析SOCKS/VLESS/Hysteria2配置及导出结构化profile；实际部署拒绝未知能力 | 未修改任何真实路由/凭据 |
| 资料备份root/owner/SELinux与Google账号数据细节不足 | 独立应用-only tar.gz校验和可选root服务；恢复需confirmed，账号数据恢复拒绝 | 未执行真实清数据或恢复，不保证ROM兼容 |
| 文档无输出/ZIP/网络扫描安全限额 | 请求1MiB；ADB输出32MiB/错误1MiB；手机响应32MiB；图像24MP；脚本16MiB/展开32MiB；扫描最多1024 IPv4/并发16 | 超限明确失败；需按业务调整后重验 |
| 当前Windows不支持.NET新版编译器CET apphost | 未修改SDK的托管编译入口；新项目CETCompat=false | 环境兼容配置，非系统设置变更 |
| native ADB客户端可能自动处理5037版本不匹配 | 已移除native执行；managed socket仅现有loopback5037/protocol41；每条已连接socket核反向TCP四元组服务PID+出生时间，固定runtime代次；所有guest需授权serial/已观察正transport-id；无native start/kill或共享forward清理 | 自有两进程重用端口/ID零请求回归通过；真实连续操作owner保持。厂商LD/scrcpy外部启动行为不在此保护内，LD仍有观测与投递窗口 |
| 配置损坏恢复可能重新授予旧串号权限 | 从备份仅恢复一般设置，清空AdbAllowedSerials、LdSerialBindings、AgentSerial及AgentBaseUrl并警告重新确认；null授权为空，非法授权拒绝 | 不静默恢复设备权限或留未授权agent阻塞设置页；主文件损坏及缺失都有离线回归 |
| 旋转观测格式因Android版本而异 | 读取已识别的dumpsys input方向与视口尺寸；无法识别时Rotation=null，不默认为0 | 已观测方向/尺寸变化推进代次；未知系统仍需现场验证 |

未验证项不等于实现成功：root属性/时区、agent、scrcpy附着/恢复、代理系统效果、真实双实例同步与20台性能仍缺条件。动态视频页节点快照可能失败，明确409并清旧树；原APK签名来源未作厂商认证，Android接受安装不等于可信来源认证。本轮不读取或删除CODEBRAIN。
