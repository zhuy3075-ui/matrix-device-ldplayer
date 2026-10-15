# 技术规格：全面验收与体验重设计

日期：2026-10-15。版本：1.0。依据docs/PRD.md；任务和阶段状态见docs/TASKS.md、docs/PROGRESS.md。

## 架构与变更边界

继续使用WPF、WorkspaceViewModel、现有Core/Features/Host。共享主题和界面助手负责样式/布局；Views负责页面结构，DisplayName映射不改变Tag、API路径或持久数据。业务问题按独立缺陷改动对应边界，不以全仓重写处理。

先以独立测试复现P0目标错误候选，再修投屏主控目标冻结；如涉及外部scrcpy，不启动共享ADB或自动接受风险。任何新命令必须沿用既有授权、generation、transport、运行时及繁忙门禁。

## 视觉、布局与命名

详细token、断点、页面结构和命名见docs/UI_SPEC.md。将硬编码页面色收敛到主题资源时逐页验证；不能只更新ModernTheme却留下旧色/旧字体和重复标题。

公共Field、Toolbar和页面容器须在窄宽重排，表格保留可辨序列号并允许有意水平滚动，关键动作和目标不能依赖横向滚动才找得到。不使用整页缩放或隐藏内容来凑小屏通过。

最低窗口尺寸须同步修正MainWindow.xaml、App.xaml.cs的smoke参数clamp、WorkspaceViewModel.cs中SettingsEditor.Apply、Host/Persistence/SettingsStore.Normalize；新增请求尺寸与ActualWidth/Height相等、保存/重载不被放大到旧最低值的断言。设备详情的重复群控选择及隐藏状态过滤纳入A05，不合并应用独立目标。

## 测试和证据

1. 正式单元/协议基线：tools/Build.ps1 -Verify，当前149项；新增缺陷先红后绿。
2. 独立Demo路径：所有12页和每种控件正常、空/离线、忙、取消、失败、目标变化；实际触发控件，不把OBSERVED-NOT-EXERCISED当执行。
3. 响应式：390×844、390×640、768×1024、1000×700、1440×900、1920×1080；自动检查可视边界/文字/目标与动作可达，截图目视。实际100%/150% DPI与字体放大是不同用例，分别记录系统DPI、逻辑尺寸和像素尺寸；仅字体放大不能证明真实DPI已测。
4. 实机：先冻结1/2实例、PID/出生时刻、canonical serial、transport和ADB owner；不调用多实例prepare的quit/modify。使用隔离数据和唯一本轮包/文件，走当前页面/批次，不用旧ViewModel捷径替代。
5. 每条实机成功有实际效果：活动窗口/进程、节点文本、文件SHA、包版本/消失、真实视频帧、脚本步骤和HTTP实际结果。命令返回成功仅是其中一项。
6. 30分钟最终版本双实例稳定性，记录资源、错误、取消/切页/重连和所有权清理；不将两台扩张为23路容量。

自有测试APK须记录本机源工程或官方来源、包名、签名证书摘要、文件SHA、版本及安装前目标包不存在证明；使用本轮唯一路径和包名，逐项记录安装、执行、卸载与清理，不覆盖用户包。缺构建工具时先定位已有可信SDK，不下载不明APK或借用用户抖音做破坏性代替测试。

日志在artifacts/apple-experience/runId；JSONL包含caseId、buildSHA、阶段、UI入口、预期/实际、目标证明ID、状态、耗时、证据摘要、defectId、轮次。原始图/节点/请求响应不上传；正式测试源码和脱敏报告可以提交。

状态使用PASS-OFFLINE、PASS-LIVE、FAIL、BLOCKED、NOT-IMPLEMENTED、NOT-RUN；每条说明覆盖层级。不存在以禁用/拒绝成功覆盖业务成功的换算。

## 独立验证与Git

实现子代理必须使用独立worktree且文件边界明确，主代理整合。重要变更由非实现者运行正式测试和UI回归；单个拒绝点不超过3轮review-fix，未解则升级记录，继续互不依赖任务。

沿用私有zhuy3075-ui/matrix-device-ldplayer、codex/unified-device-access。每个功能或缺陷单独提交并push，经测试/README/报告/清单更新后执行。保持test备份，不强推、不合并发布，排除data/bin/obj/APK/SDK/原始证据和两张原有英文截图。

## 回退与停止条件

依赖异常、目标证明失效、共享服务变化即停止对应真实动作并保留证据，不停止整项目中无关安全工作。不自动回滚用户配置或清理worktree。方案需要更换平台/破坏数据/提升权限时记录未满足项，不能用视觉替代功能或关闭失败测试。
