# 正式界面截图

日期：2026-10-22。以下 7 张图由无测试支撑的正式 WPF 构建渲染，客户区均为 1424 × 861。使用新建、未配置的真实 Runtime：`IsDemo=false`、`AdbDeviceBackend`、设备 0 台、无雷电路径、无授权串号、API 关闭。不读取用户旧配置、不连接设备、不载入脚本或应用，不制造节点、画面、资源或任务。

为避免说明图片触发设备访问，独立截图工具在创建窗口后、首次定时触发前停止了目录和截图定时器，只切换已有页面并调用页面自身的 `SaveRender`。未替换后端、控件、文案或图片内容，不将此次渲染作为自动刷新、物理 GUI、实机、视频或性能验收。窗口按原关闭流程退出，结果为 0。旧演示截图已被替换，`docs/verification/` 的 66 张历史图片已从当前树删除，Git 历史不改写。

截图所用正式 `MatrixDevice.Desktop.dll` SHA-256：

```text
FF3D0EC44CDAEA2E436C70AD95DB6BEE81F51C39B77FAFBFB7088975E454308A
```

| 文件 | 实际状态 | SHA-256 |
| --- | --- | --- |
| [devices.png](devices.png) | 未配置雷电、设备空列表 | `322EEB473853ACE9E694D4048AA8EBD64999D0566EF31BA083217E303F06BB89` |
| [monitor.png](monitor.png) | 可见设备 0、视频 0、主机资源待采样 | `DBF20CF915FD9202CB92F1DE4DC22135878526E835ED17AC4AF078FE9872D5A2` |
| [inspector.png](inspector.png) | 无截图、无节点、未选中节点 | `C0DCD79FFBD04E2F201AA97D9F32D0E8F6E5169A2972C89D89444E13ECF7D27D` |
| [applications.png](applications.png) | 目标、应用和任务列表为空 | `CDD10D2326CD94A0F29EEC8C1DE71F0F75B2EACA2B28D2E1240C461E4E0C151F` |
| [automation.png](automation.png) | 未导入脚本、未运行任务 | `2592DC9D653742EC87F889F9ED01C37825401D81D1A2CC6912A99F1B433DDC53` |
| [api.png](api.png) | 未启动接口、没有设备 | `46791CF06AC3D6FE82CDA31DD690AAAA26091B8C8EFDC2FAC0A9486B2A6813F7` |
| [settings.png](settings.png) | 真实模式、空路径和授权配置 | `B1A499158DE72B1A9F3666DD116983AD8EC8E85D8411A9EC04D7FAF551220157` |
