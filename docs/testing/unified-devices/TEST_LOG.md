# 本轮测试记录

日期：2026-10-15。下列均是独立阶段结果，不将失败轮与后续成功轮拼接成容量通过。

| 阶段 | 证据/结果 | 调整 |
|---|---|---|
| 优化前备份 | 私有test分支49e4710，222文件，远端SHA一致 | 之后才改业务代码 |
| Core初轮 | 旧72/72；新增首次15/16 | 修正测试对自然就绪应skip的预期，保留过渡2独立负例 |
| Core复验 | 16/16及旧72/72 | 独立审查发现资源期generation变化又还原风险 |
| 资源漂移复现 | 新负例16/17失败 | launch前比原generation，修后17/17，独立17/17 |
| UI/Host初接线 | 六项目0警告0错误，88/88 | 补全身份guard、epoch继承、冷态配置路径一致性 |
| 授权专项 | 94/94 | host目录不授权、ASCII/PNG/文件/应用/中文零guest拒绝、旧授权不变测试 |
| 确认主体审查 | 发现确认后Resolve重试可接管新主体 | 首个启动快照冻结，完整快照集合验证；Core19/19，独立19/19 |
| 综合复验 | 97/97；双尺寸demo exit0、error空 | 12页/3视图/11侧栏/统一页内切换及去除旧按钮均断言 |
| 请求排空 | 98/98，0警告0错误 | 加HTTP在途计数；对应LD agent入口核验身份；Resolve冻结VBox出生时刻 |
| 本机只读目录 | 23实例：4运行/19关闭，0PID34740/VBox17948与前阶段一致 | 不启动/关闭/删除实例 |
| 授权bootstrap | 两授权设备Ready，23本机实例，discoveryError/ldPlayerError为空 | 只读原授权1/2，不查询其它guest |
| 真实WPF接入 | 先取消保留旧授权/运行时，再将1接入到初始仅2的隔离工作区；2保留、两台Connected、PNG非空，exit0/passed=true | 配置只写artifacts；0launch/不读其它guest，提交后缓存更新 |
| 独立综合复验 | 98/98，10.14秒，exit0，无新增可复现P0/P1阻断 | 仅源码/离线结论，不放大23台容量 |

日志事件：directory-refresh-error、auto-screenshot-error、ld-launch-*、ld-create、ld-configure、ld-command-result、ld-workspace-complete/error/cancel。失败不抹掉原始诊断；窗口仅显示简短状态。

最终构建、独立审查和远端同步结果以本轮REPORT与Git提交记录为准。真实冷启动/新建/配置及全量容量未验收，保持待验证，不写PASS。
