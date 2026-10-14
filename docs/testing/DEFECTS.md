# 缺陷账本

| ID | 等级 | 状态 | 现象/风险 | 复现与预期 | 修复与复验 |
|---|---|---|---|---|---|
| P1-UI-001 | 观察 | 未复现，不计FAIL | 原审查把默认CollectionView同步规则概括过强 | 三模式各自激活筛选B/空/还原，Current/Main/目标保持 | 两尺寸通过；生产使用默认view，不改Wall实现，不称“已修复” |
| P1-UI-002 | P1 | 已复现并修复，离线通过 | 新截图仍叠旧节点树/选择 | reproduce-nodes.txt退出1；刷新单图与墙图后树/视觉矩形/选择应清空 | ClearNodes同时清内部根/树/选择；异步读回仅更新仍被选中的设备；两尺寸回归通过，跨设备延迟分支待补 |
| P1-UI-003 | P1 | 已复现并修复，离线通过 | 普通Busy允许启动传输/退出，停止任务不可重试 | ui-gates-reproduce.txt退出1；忙时禁开始/退出，停止行可从头重传 | 统一队列待执行条件；HasActiveOperations含Busy，关闭等待普通操作；两尺寸smoke通过 |
| P0-SAFE-001 | P0 | 修复，离线及单实例实机通过 | native ADB可能自动创建/替换共享服务，发现全部guest | 缺服务/版本错不启动；自有两进程同端口/ID替换后旧客户端0请求 | managed socket+固定server PID/出生时刻，授权serial/正transport；实机owner保持，长测结果另记 |
| P0-SAFE-002 | P0 | 修复，离线通过；实机未验 | 同serial同尺寸快速重连可能接受旧排队输入 | fake gated命令+旧key+transport1→2始终Ready，旧key必须409且0投递 | 入队捕获ID、AsyncLocal固定、只transport-id绑定；Catalog增代次并停止旧脚本；60/60通过 |
| P1-ISO-001 | P1 | 源码修复及两尺寸隔离smoke通过 | 固定AppData、备份及恢复目录无法隔离验收 | --data-dir绝对路径，日志/数据库/备份/恢复/script/report须在该目录 | 路径统一使用DataDirectory；旧用户恢复记录不迁移、不读取或删除 |
| P1-APP-001 | P1 | 实机复现修复通过 | 雷电无monkey；隐式MAIN intent又因alias非DEFAULT失败 | live-install-round2、live-launch-round1失败保留 | 唯一launcher组件解析+显式am -W，错误/未确认拒绝；API/脚本/GUI共用修复，真实启动通过 |
| P1-NODE-001 | P1 | 错误处理修复；动态页仍有限制 | 动态页dump退出0无XML，cat错误被当XML | diagnostic证明输出空、文件不存在；core-round2失败 | 确认文件输出+shell-v2 cat/409/502；清旧树，静止页/API/GUI实测通过；不保证视频随时可读 |
| P1-APP-002 | P1 | 实机复现修复通过 | 系统包android被点号规则拒绝，list-packages失败 | live-core-round3失败 | 仅允许android特例；后续API矩阵通过 |
| P1-FILE-001 | P1 | 离线修复验证通过 | 完整early-DONE会覆盖不完整下载 | fake声明200KB却100B+DONE | 字节数/pre-post STAT，旧文件/部分文件保护，独立验证通过 |
| P0-BIND-001 | P0风险 | 校验修复、夹具拒绝与正例实机通过 | 配置误绑可能把Unicode发给错误index | 错PID/出生时间/未知串号0ADB字节夹具 | serial只是候选，实际反向TCP owner必须等于固定VBox身份，另保留LD代次验证；配置不视作硬件证明 |
| P1-GATE-001 | P1 | 离线及最新GUI复验通过 | LD忙与普通忙分离，API面板自身Busy又拒绝自身POST | 独立审查源码；owned demo POST200断言 | shared外部Busy/API入口守卫；API等待不占设备写操作，脚本写拒绝；不是完整并发互斥锁 |
| P2-UI-004 | P2 | 窄修及demo回归通过 | Context标题滞后、Targets与Current门控混用 | 独立同串号上栏Ready/行Offline；mixed scope | Info通知context，批量用Targets，下载仍Current；Shell历史带串号 |
| P2-API-001 | P2 | 修复、离线复验通过 | 未own仍可请求，合并配置端口可能指向别的监听，超时误报停止 | 原send仅Busy；运行端口与settings漂移 | 用实际运行端口，未own禁发送，超时清响应/报错、禁redirect |
| P0-CONFIG-001 | P0风险 | 修复，68项回归及独立复核通过 | 损坏/缺失主配置可能从备份恢复旧设备授权，旧agent还可阻塞设置页 | 有效.bak含old-authorized及agent，主文件无效或不存在 | 仅恢复一般设置，清空授权/LD/agent绑定并警告；两个恢复分支及host-only runtime有断言，不改变既有连接后端 |

新增缺陷包含复现日志ID、预期/实际、根因、修复文件、round、定向回归、全基线和独立结论。不会把缺条件或原有未实现能力包装成新代码修复已完成。

原始证据：artifacts/ui-redesign/reproduce-nodes.txt；artifacts/acceptance/20261014-initial/*。控件观察不算功能PASS。跨设备延迟回包、完整并发lease、真实双实例和危险分支仍是测试缺口，不把单实例记录扩张为全场景通过。
