# 测试与正式产品的边界

默认 `MatrixDevice.slnx` 仅构建正式产品，`src/` 不再包含演示设备/虚拟雷电数据生成器，也不发布仅支持演示的 MCP 程序。正式 Desktop/CLI 拒绝演示和验收入口，Host 拒绝 `demo:true`。

测试夹具没有删除，避免为了清理界面而牺牲回归覆盖：

- `MatrixDevice.TestSupport/`：隔离的设备和雷电测试后端，不属于产品源码。
- `MatrixDevice.Desktop.Support/`：原有桌面验收入口、partial 与明确测试钩子；由测试构建链接，不进入默认程序集。
- `MatrixDevice.Mcp/`、`MatrixDevice.Mcp.Tests/`：仅用于离线协议验证的服务器与客户端，不提供真实群控 MCP。
- `MatrixDevice.Release.Tests/`：对默认产品做程序集、真实空 Runtime 和拒绝参数检查，不连接模拟器。
- `MatrixDevice.Tests/`：原正式回归用例；未知服务、ADB、CLI 和设备动作由测试自己的有限夹具验证，不是对本机模拟器操作。
- `fixtures/`：仅供测试的策略，不得作为真实设备授权配置。

从仓库根目录执行 `./tools/Build.ps1 -Verify`。先生成正式产品，再以 `EnableTestSupport=true` 编译 `MatrixDevice.Tests.slnx`，每次输出到新建 `artifacts/test-support/<unique>/`，不复用正式 `bin/obj`。缺少隔离输出时构建会拒绝，不能把测试构建复制到发布目录。不要使用 `dotnet build` 直接编译夹具来代替正式发布。

如已构建正式产品，可单独运行 `./tools/Test.ps1`。视频组件集成测试需要事先在本机设置 `MATRIX_TEST_SCRCPY_SERVER` 指向经摘要校验的官方 server；未提供时原用例会明确报告跳过。默认测试不启动真实模拟器，不执行真实安装卸载或长期压力验收。

`New-SourcePackage.ps1` 默认不打包测试目录；需要完整开发归档时显式使用 `-IncludeTests`。这不重写公开 Git 历史，历史报告中的演示记录仍是当时的测试证据，不能改称真实设备验收。
