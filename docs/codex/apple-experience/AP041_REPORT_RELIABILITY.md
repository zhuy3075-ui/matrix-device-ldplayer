# AP-041 设备验收报告稳健性

## 背景和边界

基线 `5263587911b0534fe56c60a864cf6fdcc6ff9d13`。上一阶段已实际交付安全离线几何诊断，原s01的指针前置失败和报告缺失仍开放。本阶段只修原 `device-layout.json` 出口：即使观察中出现NaN或正负无穷，也必须保存标准JSON，并保留原失败、阶段和异常。

已知事实：原报告直接使用严格全局JsonDefaults；实际s01的二次序列化错误已记录。新受控诊断证明无源图像可使旧公式非有限，但不直接证明s01的具体坏字段。硬约束是不向已知可能无效的坐标再次发送OS定位、不改生产缓存/输入/权限、不重跑旧指针套取绿、不放宽全局JSON。

根问题是失败报告使用了不支持实际观测值域的序列化出口；只修一个观察点不足以保护原有匿名对象、嵌套Point/Size/Rect及后续异常。因此推荐仅在原报告出口克隆JsonDefaults并加入局部double转换器。有限数保持原JSON number；非有限数编码为 `{value:null,finite:false,kind:"NaN|PositiveInfinity|NegativeInfinity"}`，明确新增联合类型，不偷换为0。

## 最小方案与取舍

- 原报告的写出、stderr和异常归并收进同一私有出口；路径和payload以工厂委托传入，在原try边界内按原顺序构造，不能把构造异常移到catch之外。原失败成功写出后仍返回同一失败对象，报告错误与原失败仍合成原样AggregateException，最终EDI重抛保持。不改任何Position、Negative、Down/Up、代次、等待、捕获释放和计数断言。
- 新纯报告契约夹具直接调用这个实际出口，不使用平行toy投影。由已有严格Demo诊断模式首调用，原几何诊断函数正文保持不变；不增加导航或真实业务功能。
- 更简单的单点安全投影不能修原报告中其它匿名对象，拒绝；三个WPF几何转换器也可行但需维护大量派生属性，先选更小的局部double转换器并以真实WPF类型验证。
- 拒绝全局AllowNamedFloatingPointLiterals、非有限改0、删观察/异常、catch后把FAIL改PASS或直接跳过设备负例。

## P0任务与验收

1. 隔离树先抽出原出口但保留原严格JSON，新增纯契约测试；单次构建原158项和新Demo模式，记录真实红测，不修改生产或发送OS/guest输入。
2. 验证实际出口的有限payload与原JsonDefaults的全文、键和值相同；原全局选项配置/转换器不被修改。覆盖List<object>匿名嵌套、Point/Size/Rect、Nullable、NaN/正负无穷、Empty派生字段。标准JsonDocument重新读取，所有实际JSON number必须有限，非有限分类和值完整。
3. 注入原失败及原失败+清理异常：成功写出仍保留FAIL、phase和原error字符串，返回同一原异常引用，不把测试失败消解为成功。
4. 在自有fresh目录用已有目录作文件目标制造I/O失败，再用不支持的序列化类型制造报告失败；验证原异常与报告异常双保留、stderr有实际输出、不覆盖任何原用户文件。另验证路径/payload工厂抛错仍被原出口捕获。预期stderr不计作意外错误，也不宣称stderr为空。
5. 红测证据冻结后只加入局部double转换器，再运行同一契约；不改期望/删除用例。正式源仅App单hook、原ResponsiveDevicesSmoke报告出口、新DeviceLayoutReportSmoke；其余旧20 Smoke/原安全几何函数/API/VM/Wall/Main/配置/原PNG不改。
6. 独立verifier执行：每child300秒、清理5秒、每原始流排空10秒，固定fresh目录，源码/PDB/原158名序与实际编译物核对，原始stdout/stderr保留，初错不自动重跑。修复版通过后原安全几何诊断仍需实际运行，报告/Task/清理保持；未进入的用例记NOT-RUN。

## 风险和后续

非有限double会成为显式对象，报告消费者必须先检查类型，不盲目GetDouble；有限原合同需逐字段证明不变。若WPF派生属性getter本身会抛异常或该转换器覆盖不足，保留失败并重新评估局部几何转换器，不扩大为全局放行。

本阶段不运行原device/monitor指针套，不能关闭原s01输入失败、AP019/020/033、11项NOT-RUN、真实GUI/双实例同步及安装卸载/最终30分钟或A08-A16。报告修复交付后再单独冻结“只对有效图像和有限坐标进行后置定位”的负例合同，保留每个原Down/Up及零副作用断言，继续完整六尺寸设备/监控补验。

## 当前状态

本阶段只修复报告出口，未改变任何原指针负例、动作或断言。原诊断交付5263587不重复推送。

## 实际验证记录

| 运行 | 构建及原测试 | 新报告契约 | 原安全几何诊断 |
|---|---|---|---|
| 独立 red-r01 | Build77664 exit0，原158/158，0警告错误 | 14类中6 PASS/8 FAIL；8红均为真实出口严格拒绝非有限double，summary `AA9EE39EDA68DC37336A0126326DBB5459F3C68F7BE39EA268F2163D6B46A18B` | NOT-RUN，首hook失败后未进入 |
| 独立 green-g01 | Build51048 exit0，原158/158，0警告错误 | 原14类不改期望，全PASS；summary `85D3406647A3D13E4255B85DE256D00C21D73754C0DE58A199AA35ECAD6BDF64` | 实际到达，原报告仍为下述3E原件 |
| 主仓 green-root-r01 | Build82636 exit0/72.112秒，原158/158用时50.97秒，0警告错误 | mode41336 exit0/2.082秒；14类全PASS，summary `4E7EC02F005AABDC97FFA7FC49AD91C58AA4437A0514EF5CC77E6769F687E7FA` | 实际390×640，原报告仍为3E原件 |

绿色两次各保留8条注入的EXPECTED-REPORT-ERROR，0条UNEXPECTED；不能把预期错误的非空stderr写成零错误流。实际11份payload及summary均为标准JSON，92个实际number全有限，58个非有限对象保留NaN16/正无穷26/负无穷16。独立绿的健康及两种原失败有限报告全文与同树红基线一致；主仓健康报告字节相同，两种失败报告仅精确归一化源码根前缀后与红参考全文相同，实际error仍是各自prior.ToString()，不冒称跨树异常栈原字节相同。原FAIL、phase、异常字符串和聚合顺序没有转绿或删减。summary中的几何NOT-RUN表示同步夹具写汇总的先序时刻，后续是否到达以实际几何文件为准。

原几何报告SHA始终为 `3E0F0766BF6AF78A5DDBA9B469CA87CE4872B6F852AEC3FBBA1536C0D2D1DACE`；六快照、22个任务均完成、cleanup=true/errors=0。三轮每轮仅两个child，全部终态/四原始流排空，无超时、kill、清理错误或owned残留。150构建输入中147原件与20个未改旧Smoke保持；实际PDB核28个唯一源码，主仓150项canonical及规范化全文与实际独立绿末快照相同。全局JsonDefaults仍实际拒绝NaN；主配置、两用户PNG、API/VM/Wall/Main和原几何源码均未改。

红、绿原件分别在独立树 `artifacts/ap041-report/{red-r01,green-g01}`；主仓原件在 `artifacts/device-report-main/green-root-r01` 与 `artifacts/dr/root-r01/diagnose-390x640`，不上传raw。主仓execution SHA `01DDF9A1E046D3AAAF0E0B8361C7628FE00E202CC98DF3CCB3B2B92E7BBA856B`，末快照SHA `FF153C174CEEC7F01C782C2393D70D4C9926BC1E98F979AA9865B9E94B0CA2C8`。准备时纯审计草稿有一次解析错误、一次复制前行尾假设拒绝，均未执行产品，保留说明，不算产品红或重跑。

主仓终态独审已通过，回执SHA `9BA487799DF60BBA6357F7635E7EBD5D7C6FFD17840D1BC2A2F1A4C8025AF518`。限域结果为报告子项通过；原s01、11项NOT-RUN、AP019/020/033、真实GUI、双实例同步及安装卸载、最终30分钟与A08-A16仍开放。文档、核包及精确index复核后按既有授权单独推私有开发分支；实际提交及交付以Git和本机push-complete回执为准，不合并或发布。
