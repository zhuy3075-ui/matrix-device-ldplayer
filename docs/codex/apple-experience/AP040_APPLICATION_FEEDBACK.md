# AP-040 应用批次摘要反馈

## 目标与边界

基线 `9117fd29fb0f6f9522112c645bd70da0e9b942a9`。已有 c02 观察及 `ApplicationBatchSummary` 证实：读取应用也固定显示“安装命令完成（包名未核验）0”。本阶段只隐藏这个零计数安装专属段，不修改安装、读取、列表发布/过滤、身份、确认、失败或取消业务。

范围仅 `ApplicationsView.cs` 的摘要函数、新 `ApplicationFeedbackSmoke.cs`、App 已有严格 Demo 诊断分支的单一 hook 及本文。原 169 项、23 个旧 Smoke、14 类报告契约、AP042 和原离线几何正文保持。原生文件选择、Sky/物理输入、截图、真实 APK/guest/ADB 和 CMFA 权限/VPN 不在范围。

## 验收合同

- `CompletedUnverified` 为零时不显示安装命令完成段；非零时仍显示准确数量及“包名未核验”，不改为已核验安装。
- 原 `进度`、`已完成`、`失败`、`取消` 计数保持，包含原“失败 1”文本；失败及取消仍属终态，Waiting/Running 不属终态。
- 正式同步夹具直接调用实际 `ApplicationBatchSummary`，不复制实现；只新建自有 `ApplicationTaskRow` 并用原 `SetResult` 设置输入，不将其加入业务集合。
- 六个固定用例：List 成功、Install 非零未核验、混合终态含失败 1、取消读取、空集合、Waiting/Running。记录原输入、实际摘要及调用后输入值不变。
- 夹具不导航、不 Invoke/Focus、不调用后端、不创建异步任务；记录 Runtime/Current/Main、业务行集合及 Info 引用、选择、输入、业务摘要/进度、派发计数及 API/调度状态前后相同。此证据限所列公开/自有字段，不声称观测全部私有后端状态。
- App 新同步 hook 仅放原 `VerifyWorkspaceAccessibilityAsync` 之后、原 `RunOfflineGeometryDiagnosticsAsync` 之前，旧相对顺序和旧方法正文不变。夹具失败须保存 FAIL 并抛出，后置几何标 NOT-RUN，不消解为通过。

## 最小方案与风险

推荐仅在摘要函数中按 `CompletedUnverified` 非零条件追加原安装段，保留所有其余段及计数。直接删整个安装段更简单但丢失命令完成/包名未核验的重要区别，拒绝采用。不要借此将空/过滤后列表认定安装失败，或修改 `ApplicationBatch` 的 `CompletedUnverified` 语义。

## 红绿与证据

先固定 spec/夹具/App hook，保留旧摘要。静审放行后仅一次现有 `tools/Build.ps1 -Verify`，再一次 fresh 390x640 的严格 `--diagnose-offline-geometry --demo`。预期原 169 项和前置 14/AP042 保持，六个新用例中四个无安装段合同产品红、另两个通过，后置几何 NOT-RUN。

再仅修改摘要函数，同一固定夹具/期望，唯一绿轮重复上述命令；六用例应全过，前置契约和原几何诊断均须通过。构建/前置错误停止交证据，不算产品红。每个子进程 300 秒，原 stdout/stderr、退出、源 SHA 与旧 Smoke 守卫保存在 ignored artifacts；不重试凑绿、不自行批准或提交。

## 状态

- root 全文静审放行后，`red-r01` 唯一构建与原 169 项通过，0 警告/0 错误，76.433 秒、exit 0；严格 Demo 诊断 1.637 秒、exit 1。原 14 类和 AP042 两页往返的三阶段前置通过，新合同恰为四个 PRODUCT-FAIL、两个 PASS，全部输入和所记录业务状态不变；后置几何未运行。
- 随后只在摘要函数缓存 `CompletedUnverified` 数量并条件追加原安装段；App/夹具及期望保持相同 SHA。`green-g01` 唯一构建与原 169 项通过，0 警告/0 错误，65.596 秒、exit 0；严格 Demo 诊断 2.258 秒、exit 0。新六例全部通过、输入和所记录业务上下文保持；原 14 类与 AP042 通过，原几何 SHA 仍为 `3E0F0766BF6AF78A5DDBA9B469CA87CE4872B6F852AEC3FBBA1536C0D2D1DACE`。
- 两轮原 169 项名称/顺序相同，23 个旧 Smoke 与每轮四输入原 bytes 前后保持。四个自有子进程均退出、原双流排空，无超时；绿轮诊断 stderr 中原 14 类刻意注入的八条预期报告错误保持，不冒称 stderr 为空。
- 原件在 ignored `artifacts/ap040/<run>/`（原流/进程元数据/各轮源副本）与 `artifacts/of/<run>/diagnose-390x640/`（实际 JSON）；未覆盖红报告。后续独立验证和主仓集成仍需按实际记录判断，不自行批准。
- 独立`verify-v01`：从9117新树机械应用，夹具仅CRLF/LF差异且规范化全文逐字同作者；实际Build52020 exit0/77.853秒，169项52.00秒/0警告错误；mode66836 exit0/2.925秒，新6、原14/AP042和3E保持，26选定PDB路径与23旧Smoke已核，流完整/无timeout/owned残留。execution SHA `124AEAA4E83FBDAF1B3181C81846602C458B0F537C4EE0CC4C58831366B11C82`。
- 主仓`main-r01`：三源码raw与独立相同；session41366终态，Build17600 exit0/77.126秒，169项51.69秒/0警告错误，mode35036 exit0/2.651秒，新6及原14/AP042/3E通过，输入/context和23旧源保持。execution SHA `EF2EADDA047A9DCDE0BC8289D3E47D4326A97DFB340B8636A5912F3B059CD080`，6例summary SHA `C1BFA006D13AC462EE7C8E837B9065F99CBF8C124D4FD659A741887704F2140B`，独立与作者绿同；原8条预期报告错误stderr9842B保留，不称无诊断。全部进程与原流终态，无timeout；原件在ignored `artifacts/application-feedback-main/main-r01`及`artifacts/af/main-r01/diagnose-390x640`。
- 当前仅批准限域集成，待正式文档/核包/精确index独审后按原私有开发分支单独提交；是否已交付以实际Git及本机回执为准，不预写推送成功、不合并发布。
- 这仅解决零计数安装摘要误导，不证明列表零条的原因；GUI 安装卸载、真实模拟器及完整 A08/A14 不因此通过。
