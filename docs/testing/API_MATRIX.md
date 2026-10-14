# 接口逐项验收

依据reference/API.md及实际实现；最新原始请求/响应、效果截图和Shell跟踪在私有live-api-matrix-round2。成功响应与效果验证分开，拒绝路径不是业务成功。单一写控制器；本机API不提供身份认证，界面/脚本互锁不是跨进程安全锁。

| 编号 | Automator POST | 本轮结论 | 效果或限制 |
|---|---|---|---|
| A01 | device/dump | 实机通过（静止页） | 真实树非空；动态视频可能409 |
| A02 | device/screenshot | 实机通过 | 40x40 PNG实际解码 |
| A03 | device/shell | 实机通过 | boot属性精确1 |
| A04 | device/execute | 实机通过 | shell同义结果 |
| A05 | device/set-prop | 拒绝用例通过；业务未验 | 非法prop400，无root变更 |
| A06 | device/set-timezone | 拒绝用例通过；业务未验 | 无效时区400，不改系统 |
| A07 | device/set-sim-info | 未实现，501验证 | 缺ROM契约 |
| A08 | app/list | 实机通过 | 给定APP40.7.0 |
| A09 | app/list-packages | 实机通过 | 含系统android及真实安装目录 |
| A10 | app/start | 实机通过 | 明确launcher alias，PID/截图 |
| A11 | app/stop | 实机通过 | 包及子进程消失 |
| A12 | app/clear | 拒绝用例通过；业务未验 | 非法包400，不清数据 |
| A13 | input/tap | 实机通过 | 安全设置搜索页面出现 |
| A14 | input/swipe | 实机通过 | 设置列表文字序列改变 |
| A15 | input/keyevent | 实机通过 | Home离开设置 |
| A16 | service/info | 仅诊断通过 | 无agent返回backend信息，不是原ServiceInfo |
| A17 | service/stop | 未接入，501验证 | 不停止共享服务 |
| A18 | function/sleep | 实机请求通过 | 显式小数秒；缺省未还原 |
| A19 | function/sleep-ms | 实机请求通过 | 显式10ms；缺省未还原 |
| A20 | element/find | 实机通过 | 已知设置节点精确匹配 |
| A21 | element/find-all | 实机通过 | 已知节点非空匹配 |
| A22 | element/wait-exists | 实机通过（已存在） | 真实dump，不宣称延迟出现实测 |
| A23 | element/click-if-exists | 实机通过 | 搜索框出现 |
| A24 | element/click-text | 实机通过 | 精确文本搜索控件 |
| A25 | element/set-text | 未接入，501验证 | 节点输入仍需兼容phone agent |
| A26 | element/wait-any | 实机通过（已存在） | 数组首项优先 |
| A27 | element/click-node | 实机通过 | 正bounds中心打开搜索框 |

27项都有结果：20项业务/请求成功、1项仅backend诊断、6项拒绝路径；不能报告27项业务全通过。sleep计时边界/取消、selector完整nullable语义、坏JSON/Origin/ContentType/方法/串号、队列取消等另由离线回归覆盖。

| 编号 | 普通接口 | 结果与证据 |
|---|---|---|
| B01 | 网关GET /health | 实机通过，ok独立envelope |
| B02 | 网关GET /devices | 实机通过，唯一授权测试设备；不代表主机只有一实例 |
| B03 | 逐设备GET /health | 实机通过，绑定serial |
| B04 | 逐设备GET /device | 实机通过，绑定serial |
| B05 | POST /shell | 实机通过，boot=1 |
| B06 | POST /tap | 实机通过，安全搜索页变化 |
| B07 | POST /swipe | 实机通过，动作前新截图尺寸、列表变化 |
| B08 | POST /text | 实机通过，中文/空格/ASCII符号精确回读 |
| B09 | POST /app/start | 实机通过，APP进程/截图 |
| B10 | POST /app/stop | 实机通过，进程消失 |

| 编号 | 雷电操作 | 结果与限制 |
|---|---|---|
| L01 | launch | 初次新实例直接CLI实际启动通过；HTTP冷启动未测 |
| L02 | quit | 确认/拒绝离线通过，真实业务未测 |
| L03 | reboot | 确认/拒绝离线通过，真实业务未测 |
| L04 | runapp | HTTP实机通过，提供APP PID/截图 |
| L05 | killapp | HTTP实机通过，APP无进程且仍安装 |
| L06 | installapp | 路由/参数离线通过；首次安装实机走managed ADB，不能算此CLI业务通过 |
| L07 | input | HTTP实机中文空格精确回读 |
| L08 | keyevent | HTTP实机Home通过；音量/Back CLI分支未实测 |

剩余危险或条件路径保留未验，不用原实例0补足覆盖。Capabilities supported=已接线，accepted=提交，verified仅对应实现中的观测；原始表述与这份实际结果不是同一概念。
