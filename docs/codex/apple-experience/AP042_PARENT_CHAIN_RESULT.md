# AP-042 标准窗口父链负实验

## 背景与范围

2026-10-17，基线为已推送的 `a96ae0e4c87ad625f7b754fefefb3954e345063b`。上一阶段225项回归通过不代表GUI验收通过。本次只检验“补齐标准Window父链是否足以消除旧页控件残留”，不更改生产控件，不关闭AP042/AP045，不重试官方截图超时或错误坐标输入分支。

隔离工作树为项目父目录下 `.worktrees/workspace-parent-chain`。候选仅App专用Demo早入口、新测试源和实验规格；原 `WorkspaceAccessibilitySmoke.cs` 字节、断言、五个Dispatcher任务及5秒Flush保持。实验C#不合入正式程序。本报告及状态记录不构成新功能交付。

## 方案与约束

- A为新进程直接调用原fixture；B为新进程先公开创建实际标准Window peer，从其原GetChildren返回边找到同一tabs peer即停止下探，再调用原fixture。两臂同一二进制、fresh Demo目录和390×640窗口。
- 不创建监听、提供者或叶子替身，不调用GetParent、Invalidate/Reset、私有反射或原生输入，不增加等待或重试到绿。准备前必须无Window/tabs peer及两类listener，窗口未激活、业务和接口调度关闭。
- 更简单的重复原fixture不能区分父链条件，未采用。直接生产代码强制清缓存也未采用：当前证据不能证明唯一生产根因，且直属缓存重建不等于所有后代更新。[官方缓存实现](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Automation/Peers/AutomationPeer.cs)、[窗口提供者路径](https://github.com/dotnet/wpf/blob/4726a1a94599e1c2aa090f5326e2ca930f016c7d/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/HwndTarget.cs)。未宣称本机框架逐方法SourceLink已核。

## 实际执行与原失败

首次outer实际exit1，但唯一Build自然exit0/125.420秒；225项名称顺序与基准完全一致，225/225耗时57.09秒，3项既有编译警告、0错误。MCP实际14通过、13 NOT-RUN、14会话；13正常退出，另1会话为原预期RPC ID拒绝。后继strict3/A/B均未启动。

outer在读取 `exitCode` 时因helper返回流合同失效而失败，不是产品或编译失败。公共BCL纯内存例确认未抑制的GetResult可输出VoidTaskResult；原运行未保存变量类型，不能倒称已取得当时的具体对象证据。新续跑仅抑制三处返回值、严格断言恰一份record，增加own pwsh返回7/16字节ASCII的驱动契约；没有取数组最后项掩盖问题，也没有重新Build。

原失败execution、raw和helper均保留。随后单独采样冻结255个实际Release产物与副本，另核原156个selected输出；这是post-failure冻结，不改写原outer的products0字段。实际Portable PDB中App、原fixture及新Probe三份源码checksum均经两名独立reviewer核对。

## A/B 结果

| 项目 | A：原链 | B：标准Window父链准备 |
|---|---|---|
| 原fixture结果 | PRODUCT-FAIL | PRODUCT-FAIL |
| 三阶段节点数 | 146→146→146 | 146→146→146 |
| 应用页预期owner匹配 | 0、0、0 | 0、0、0 |
| 旧页owner残留 | 111 | 111 |
| 原Product失败条数 | 4 | 4 |
| 原任务恢复/排空 | 完成 | 完成 |
| 窗口关闭/唯一VM清理 | 完成、1次 | 完成、1次 |

B实际记录Window peer 1的原children边包含tabs peer 12；tabs的parent为1、depth为1、回溯path为[1,12]，且tabs的childrenQueried=false。B准备后Window/tabs peer已存在，两类listener仍false。报告顶层after只表示“父链准备后、原fixture之前”，不是合同结束快照；原fixture另有14个原位快照，不能由公开布尔值推断内部provider根登记状态。

续跑outer自然exit0仅表示实验和保护门完成。驱动契约、三项参数早拒及两臂共6个child、12份raw均终态，无timeout、kill、retained或清理错误。候选191项记录（190输入加规格）、255当前/冻结产物、主仓189输入、156 selected产物和用户3文件均保持。未操作真实模拟器、共享ADB、账号、APK安装、卸载或VPN。

## 证据索引

原件留在隔离树 `artifacts/workspace-parent/`，不上传原始证据、二进制或本机配置：

- `observed-r01/execution.json`：`1E0FA77868588789C5D6BDE2C1D963ABE02D550A464F1029E434ECA951516741`，原outer1保持。
- 原Build stdout：`3586C3E4C244759611077670EB43C0477A1A02AF60DE8FCFA9F9CD7768DE9BCC`，32174字节；stderr0字节。
- `after-build-freeze-r01/products.json`：`E2E29DDF6EB3D8E33A5D89EA9EB0D78D3C4F9D6908CCEFFD39EEF47DBFC9AA8A`，255项。
- `continuation-r01/execution.json`：`461E80776F2E226B0C461792940D9161A2C6A6DCDA1D7F3957E573C0BC1D979B`。
- A summary：`9A069E458FE225CF67CCCD8C15FEC602A26636D979CC385A50EC0A395F5C9545`；B summary：`A47D54FA926137DE93E22868AEBEFEBD306DFBBD1094D1596C0136659D66A5A3`。

## 裁决与下一步

两名独立verifier均只批准负实验结论：**本次受控条件下，标准Window父链准备不足以消除AP042**。不确定真正原因，不将实验代码合入生产，不把outer0当产品通过；旧PRODUCT失败、AP045及完整GUI验收继续开放。

下一安全工作为A09接口/工具/设置页面：先验证fresh Demo保存重载、错误恢复、禁用解释及自有loopback/端口路径。另做provider实验前须先明确受支持公开入口和可隔离变量，不继续盲目追加诊断。真实应用GUI安装卸载、12页实际操作/DPI、完整RPA/MCP/抓包和最终双实例30分钟仍必交付，约13工作块不因本次负实验扣减。
