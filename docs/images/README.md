# 界面截图说明

日期：2026-10-22。截图来自本机已构建的 WPF 程序，使用独立 Demo Runtime、新建配置目录和实际页面控件渲染，客户区尺寸均为 1424 × 861。不是 HTML 预览或手绘设计图。

生成时仅发现演示设备、读取演示截图/节点/应用、导入仓库基础脚本示例，没有执行脚本，没有启动接口或连接真实模拟器。监控仅显示未授权视频的实际 Demo 状态，没有加入伪造视频、资源值或 FPS；设置不含本机授权配置。Demo 与真实模式的接入限制可能不同。

图片均由当前页面自身的 `SaveRender` 保存，展示客户区而不含操作系统标题栏。未裁剪、合成或替换控件；不代表真实鼠标键盘、DPI 或实机性能验收。当前截图所用 `MatrixDevice.Desktop.dll` SHA-256：

```text
FEEDA34A457B5E014B5B93CB1AD105B05589C7055E1B3E3BC96C55B07EB2B7FD
```

| 文件 | 展示内容 | SHA-256 |
| --- | --- | --- |
| [devices.png](devices.png) | 统一实例与设备列表、状态筛选、启动入口 | `9FDEC0BB9D7D8FBFEEF6BFBCD5EDC2167EF4DA02CE4EB540A8BB18ADADE074CD` |
| [monitor.png](monitor.png) | 双卡片监控布局，未授权视频和未知资源状态 | `ECD99DD3BE31F76C82F4B5A2D810FE7D1FC3AA34A82A257660C13B2C1B098257` |
| [inspector.png](inspector.png) | 演示图像、节点树和属性面板 | `47514426E439F5D095C76FE509BD3EF039CF2066D12156CCFD3F861FADD1020F` |
| [applications.png](applications.png) | 两台演示目标及其应用列表、读取完成记录 | `B8B4A837275A9CD660C0C375449B8BFF1208D3A41DC7373729E34F7F8DE796A9` |
| [automation.png](automation.png) | 导入基础脚本后的参数视图，尚未运行 | `90DFB9168C037C7D01E4AE08B8860095E6D2DFDA2B6E72172F1A379FF830E410` |
| [api.png](api.png) | 未启用网关的接口管理与请求编辑区 | `80CA701EBF127C765AE294E78F8BEE707040C3F89039E2D91A6E312DCD8DCB00` |
| [settings.png](settings.png) | 空授权配置、视频组件和窗体设置 | `BBBEE5F0AF38C6B45E64A17B4095B8FFB85854B94880D6BD08737CCCC2828881` |

历史截图仍位于 `docs/verification/`，可能含已删除的分组、搜索、卡片或独立雷电入口，不作为当前界面介绍。本轮未使用两张未提交的英文截图，也未公开真实应用内容。
