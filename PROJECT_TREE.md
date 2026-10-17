# 完整项目文件目录

```text
MatrixDevice.Reconstructed/
|-- .gitignore
|-- Directory.Build.props
|-- docs/
|   |-- ASSUMPTIONS.md
|   |-- codex/
|   |   |-- apple-experience/
|   |   |   |-- A02_MAIN_TARGET.md
|   |   |   |-- A03_APPEARANCE.md
|   |   |   |-- A04_RESPONSIVE_SHELL.md
|   |   |   |-- A05_DEVICE_LABELS.md
|   |   |   |-- A05_DEVICE_RESPONSIVE.md
|   |   |   |-- A05_DEVICE_STATE.md
|   |   |   |-- A05_MONITOR_RESPONSIVE.md
|   |   |   |-- A06_AUTOMATIC_LOADING.md
|   |   |   |-- A06_COMPLETED_CACHE.md
|   |   |   |-- A06_DEBUG_PUBLICATION.md
|   |   |   |-- A06_NATIVE_IMAGE_SAVE.md
|   |   |   |-- A06_NODE_RESPONSIVE.md
|   |   |   |-- A06_RECONNECT_LOADING.md
|   |   |   |-- A06_SCREENSHOT_RESPONSIVE.md
|   |   |   |-- A07_API_DRAIN.md
|   |   |   |-- A07_APPLICATION_IDENTITY.md
|   |   |   |-- A07_APPLICATION_PENDING.md
|   |   |   |-- A07_APPLICATION_RESPONSIVE.md
|   |   |   |-- A07_CMFA_APPLICATIONS.md
|   |   |   |-- A07_OWNED_CLEANUP.md
|   |   |   |-- A07_TOOLTIP_DIAGNOSTICS.md
|   |   |   |-- A08_FILES_WORKSPACE.md
|   |   |   |-- A08_SCRIPT_FORMAT.md
|   |   |   |-- A08_SCRIPT_WORKSPACE.md
|   |   |   |-- A08_TERMINAL_WORKSPACE.md
|   |   |   |-- A08_TEXT_WORKSPACE.md
|   |   |   |-- A09_API_STARTUP.md
|   |   |   |-- A09_API_WORKSPACE.md
|   |   |   |-- A09_SETTINGS_WORKSPACE.md
|   |   |   |-- A13_APPLICATION_GUI.md
|   |   |   |-- A13_DUAL_INSTANCE_SYNC.md
|   |   |   |-- A13_SQLITE_PATH.md
|   |   |   |-- A14_SCRIPT_JOBS.md
|   |   |   |-- A14_SCRIPT_PARAMETER_BINDINGS.md
|   |   |   |-- AP008_DEPENDENCIES.md
|   |   |   |-- AP033_INPUT_DIAGNOSTICS.md
|   |   |   |-- AP040_APPLICATION_FEEDBACK.md
|   |   |   |-- AP041_OFFLINE_DIAGNOSTICS.md
|   |   |   |-- AP041_REPORT_RELIABILITY.md
|   |   |   |-- AP042_CLIENT_OBSERVATION.md
|   |   |   |-- AP042_PARENT_CHAIN_RESULT.md
|   |   |   |-- AP042_PEER_CACHE_DIAGNOSTICS.md
|   |   |   |-- AP042_WORKSPACE_ACCESSIBILITY.md
|   |   |   |-- AP043_SKY_AB.md
|   |   |   |-- CONTROL_CATALOG.md
|   |   |   |-- DEFECTS.md
|   |   |   |-- HANDOFF.md
|   |   |   +-- TEST_MATRIX.md
|   |   |-- automation-expansion/
|   |   |   |-- CAPTURE_RESEARCH.md
|   |   |   |-- MCP_DESIGN.md
|   |   |   |-- MCP_STDIO.md
|   |   |   +-- MCP_WRITE_OWNERSHIP.md
|   |   |-- device-action-dedup/
|   |   |   +-- REPORT.md
|   |   |-- launch-memory-policy/
|   |   |   +-- REPORT.md
|   |   |-- launch-resource-confirmation/
|   |   |   +-- REPORT.md
|   |   |-- list-console/
|   |   |   |-- LIVE_RECHECK_2026-10-16.md
|   |   |   +-- REPORT.md
|   |   |-- realtime-monitoring/
|   |   |   |-- goal-prompt.md
|   |   |   |-- HANDOFF.md
|   |   |   |-- plan.md
|   |   |   +-- requirements.md
|   |   +-- unified-console-ui/
|   |       |-- plan.md
|   |       +-- REPORT.md
|   |-- COVERAGE.md
|   |-- IMPLEMENTATION.md
|   |-- LDPLAYER_INTEGRATION.md
|   |-- LDPLAYER_PLAN.md
|   |-- LDPLAYER_RESULTS.md
|   |-- PRD.md
|   |-- PROGRESS.md
|   |-- reference/
|   |   |-- API.md
|   |   |-- ARCHITECTURE.md
|   |   |-- CLOUD.md
|   |   |-- DEBUG_SHARED.md
|   |   |-- DEBUG.md
|   |   |-- LEGACY_DEVICE_TASKS.md
|   |   |-- PRD.md
|   |   |-- PSEUDOCODE.md
|   |   |-- SHARED.md
|   |   |-- spec.md
|   |   +-- uispec.md
|   |-- RESULTS.md
|   |-- SPEC.md
|   |-- TASKS.md
|   |-- testing/
|   |   |-- API_MATRIX.md
|   |   |-- DEFECTS.md
|   |   |-- HANDOFF.md
|   |   |-- multi-instance/
|   |   |   |-- PLAN.md
|   |   |   |-- REPORT.md
|   |   |   +-- TEST_LOG.md
|   |   |-- one-click-start/
|   |   |   |-- PLAN.md
|   |   |   |-- REPORT.md
|   |   |   +-- TEST_LOG.md
|   |   |-- realtime-monitoring/
|   |   |   +-- REPORT.md
|   |   |-- REPORT.md
|   |   |-- TEST_CASES.md
|   |   |-- TEST_LOG.md
|   |   |-- TEST_RESULTS.md
|   |   +-- unified-devices/
|   |       |-- OFFICIAL-RESEARCH.md
|   |       |-- PLAN.md
|   |       |-- REPORT.md
|   |       +-- TEST_LOG.md
|   |-- UI_REDESIGN.md
|   |-- UI_SPEC.md
|   |-- verification/
|   |   |-- build-and-tests.txt
|   |   |-- build-final.txt
|   |   |-- desktop-crop.png
|   |   |-- desktop-final.json
|   |   |-- desktop-nodes.json
|   |   |-- desktop-pixels.json
|   |   |-- desktop-small-tab10.png
|   |   |-- desktop-small.json
|   |   |-- desktop-tab1.png
|   |   |-- desktop.json
|   |   |-- ldplayer-build-and-tests.txt
|   |   |-- ldplayer-desktop-pixels.json
|   |   |-- ldplayer-desktop-small-tab0.png
|   |   |-- ldplayer-desktop-small-tab1.png
|   |   |-- ldplayer-desktop-small-tab10.png
|   |   |-- ldplayer-desktop-small-tab11.png
|   |   |-- ldplayer-desktop-small-tab2.png
|   |   |-- ldplayer-desktop-small-tab3.png
|   |   |-- ldplayer-desktop-small-tab4.png
|   |   |-- ldplayer-desktop-small-tab5.png
|   |   |-- ldplayer-desktop-small-tab6.png
|   |   |-- ldplayer-desktop-small-tab7.png
|   |   |-- ldplayer-desktop-small-tab8.png
|   |   |-- ldplayer-desktop-small-tab9.png
|   |   |-- ldplayer-desktop-small.json
|   |   |-- ldplayer-desktop-small.png
|   |   |-- ldplayer-desktop-tab0.png
|   |   |-- ldplayer-desktop-tab1.png
|   |   |-- ldplayer-desktop-tab10.png
|   |   |-- ldplayer-desktop-tab11.png
|   |   |-- ldplayer-desktop-tab2.png
|   |   |-- ldplayer-desktop-tab3.png
|   |   |-- ldplayer-desktop-tab4.png
|   |   |-- ldplayer-desktop-tab5.png
|   |   |-- ldplayer-desktop-tab6.png
|   |   |-- ldplayer-desktop-tab7.png
|   |   |-- ldplayer-desktop-tab8.png
|   |   |-- ldplayer-desktop-tab9.png
|   |   |-- ldplayer-desktop.json
|   |   |-- ldplayer-desktop.png
|   |   |-- ldplayer-live-query.txt
|   |   +-- ui-redesign/
|   |       |-- build-and-tests.txt
|   |       |-- desktop-cards.png
|   |       |-- desktop-crop.png
|   |       |-- desktop-empty.png
|   |       |-- desktop-expanded.png
|   |       |-- desktop-list.png
|   |       |-- desktop-nodes.json
|   |       |-- desktop-screens.png
|   |       |-- desktop-small-cards.png
|   |       |-- desktop-small-empty.png
|   |       |-- desktop-small-expanded.png
|   |       |-- desktop-small-list.png
|   |       |-- desktop-small-screens.png
|   |       |-- desktop-small-tab0.png
|   |       |-- desktop-small-tab1.png
|   |       |-- desktop-small-tab10.png
|   |       |-- desktop-small-tab11.png
|   |       |-- desktop-small-tab2.png
|   |       |-- desktop-small-tab3.png
|   |       |-- desktop-small-tab4.png
|   |       |-- desktop-small-tab5.png
|   |       |-- desktop-small-tab6.png
|   |       |-- desktop-small-tab7.png
|   |       |-- desktop-small-tab8.png
|   |       |-- desktop-small-tab9.png
|   |       |-- desktop-small.json
|   |       |-- desktop-small.png
|   |       |-- desktop-tab0.png
|   |       |-- desktop-tab1.png
|   |       |-- desktop-tab10.png
|   |       |-- desktop-tab11.png
|   |       |-- desktop-tab2.png
|   |       |-- desktop-tab3.png
|   |       |-- desktop-tab4.png
|   |       |-- desktop-tab5.png
|   |       |-- desktop-tab6.png
|   |       |-- desktop-tab7.png
|   |       |-- desktop-tab8.png
|   |       |-- desktop-tab9.png
|   |       |-- desktop.json
|   |       +-- desktop.png
|   +-- VERIFICATION.md
|-- examples/
|   |-- basic/
|   |   |-- manifest.json
|   |   +-- script.json
|   |-- basic-example.jxscript
|   |-- mcp-demo-policy.json
|   +-- parameterized/
|       |-- manifest.json
|       +-- script.json
|-- global.json
|-- MatrixDevice.slnx
|-- NuGet.Config
|-- plan.md
|-- PROJECT_TREE.md
|-- README.md
|-- SOURCE_BUNDLE.md
|-- SOURCE_MANIFEST.json
|-- src/
|   |-- MatrixDevice.Cli/
|   |   |-- MatrixDevice.Cli.csproj
|   |   |-- packages.lock.json
|   |   +-- Program.cs
|   |-- MatrixDevice.Core/
|   |   |-- Devices/
|   |   |   |-- AdbDeviceBackend.cs
|   |   |   |-- AdbServerIdentity.cs
|   |   |   |-- DemoDeviceBackend.cs
|   |   |   |-- DeviceCatalog.cs
|   |   |   |-- DeviceWriteCoordinator.cs
|   |   |   |-- ExistingAdbSocket.cs
|   |   |   |-- ExistingAdbTransport.cs
|   |   |   |-- IdentityPinnedDeviceCommands.cs
|   |   |   |-- NetworkDeviceScanner.cs
|   |   |   |-- PngImage.cs
|   |   |   +-- SerialCommandQueue.cs
|   |   |-- Input/
|   |   |   +-- InputCoordinator.cs
|   |   |-- IO/
|   |   |   +-- BoundedStreamRead.cs
|   |   |-- LdPlayer/
|   |   |   |-- DemoLdConsoleRunner.cs
|   |   |   |-- LdConsoleRunner.cs
|   |   |   |-- LdInstanceEditor.cs
|   |   |   |-- LdLaunchCoordinator.cs
|   |   |   |-- LdPlayerAdbBinding.cs
|   |   |   |-- LdPlayerModels.cs
|   |   |   +-- LdPlayerService.cs
|   |   |-- MatrixDevice.Core.csproj
|   |   |-- Models/
|   |   |   +-- Contracts.cs
|   |   |-- packages.lock.json
|   |   |-- Properties/
|   |   |   +-- AssemblyInfo.cs
|   |   +-- Protocols/
|   |       |-- PhoneAgentClient.cs
|   |       +-- ScrcpyProtocol.cs
|   |-- MatrixDevice.Desktop/
|   |   |-- app.manifest
|   |   |-- App.xaml
|   |   |-- App.xaml.cs
|   |   |-- LdWindowVisibility.cs
|   |   |-- LocalizationService.cs
|   |   |-- MainWindow.xaml
|   |   |-- MainWindow.xaml.cs
|   |   |-- MatrixDevice.Desktop.csproj
|   |   |-- MotionPreferences.cs
|   |   |-- NativeWindowHost.cs
|   |   |-- packages.lock.json
|   |   |-- Presentation.cs
|   |   |-- ResponsiveLayout.cs
|   |   |-- ScriptJobWorkspace.cs
|   |   |-- Themes/
|   |   |   +-- ModernTheme.xaml
|   |   |-- UiErrorPresentation.cs
|   |   |-- UnifiedDeviceRow.cs
|   |   |-- Views/
|   |   |   |-- ApiView.cs
|   |   |   |-- ApiWorkspaceSmoke.cs
|   |   |   |-- AppleAppearanceSmoke.cs
|   |   |   |-- ApplicationApiPendingSmoke.cs
|   |   |   |-- ApplicationFeedbackSmoke.cs
|   |   |   |-- ApplicationIdentitySmoke.cs
|   |   |   |-- ApplicationManagementSmoke.cs
|   |   |   |-- ApplicationPendingSmoke.cs
|   |   |   |-- ApplicationRows.cs
|   |   |   |-- ApplicationsView.cs
|   |   |   |-- CompletedDebugCacheSmoke.cs
|   |   |   |-- DebugAutomaticLoadingSmoke.cs
|   |   |   |-- DebugPublicationSmoke.cs
|   |   |   |-- DebugView.cs
|   |   |   |-- DeviceFilterSmoke.cs
|   |   |   |-- DeviceLayoutReportSmoke.cs
|   |   |   |-- DeviceWriteWorkspaceSmoke.cs
|   |   |   |-- DirectoryView.cs
|   |   |   |-- ExtensionsView.cs
|   |   |   |-- FilesView.cs
|   |   |   |-- FilesWorkspaceSmoke.cs
|   |   |   |-- LdPlayerView.cs
|   |   |   |-- LdWindowVisibilitySmoke.cs
|   |   |   |-- LiveDualSyncAcceptance.cs
|   |   |   |-- LiveUiAcceptance.cs
|   |   |   |-- MonitorAcceptance.cs
|   |   |   |-- MonitorSmoke.cs
|   |   |   |-- MonitorView.cs
|   |   |   |-- NativeTargetSmoke.cs
|   |   |   |-- NativeView.cs
|   |   |   |-- NetworkScannerView.cs
|   |   |   |-- OfflineGeometryDiagnostics.cs
|   |   |   |-- OneClickStartAcceptance.cs
|   |   |   |-- ReconnectedDebugLoadingSmoke.cs
|   |   |   |-- ResponsiveApplicationsSmoke.cs
|   |   |   |-- ResponsiveColumnsConverter.cs
|   |   |   |-- ResponsiveDebugSmoke.cs
|   |   |   |-- ResponsiveDevicesSmoke.cs
|   |   |   |-- ResponsiveMonitorSmoke.cs
|   |   |   |-- ResponsiveScreenshotSmoke.cs
|   |   |   |-- ResponsiveScriptsSmoke.cs
|   |   |   |-- ResponsiveShellSmoke.cs
|   |   |   |-- ScreenshotView.cs
|   |   |   |-- ScriptJobEditingSmoke.cs
|   |   |   |-- ScriptParameterEditingSmoke.cs
|   |   |   |-- ScriptsView.cs
|   |   |   |-- SettingsView.cs
|   |   |   |-- SettingsWorkspaceSmoke.cs
|   |   |   |-- ShellSimplificationSmoke.cs
|   |   |   |-- ShellView.cs
|   |   |   |-- ShellWorkspaceSmoke.cs
|   |   |   |-- TextView.cs
|   |   |   |-- TextWorkspaceSmoke.cs
|   |   |   |-- UiControlAudit.cs
|   |   |   |-- UnifiedDeviceActionsSmoke.cs
|   |   |   |-- UnifiedDevicesAcceptance.cs
|   |   |   |-- UnifiedDevicesView.cs
|   |   |   |-- UnifiedDeviceTile.cs
|   |   |   |-- WallView.cs
|   |   |   |-- WorkspaceAccessibilitySmoke.cs
|   |   |   |-- WorkspaceClientObservation.cs
|   |   |   +-- WorkspaceShell.cs
|   |   |-- WorkspaceTabControl.cs
|   |   +-- WorkspaceViewModel.cs
|   |-- MatrixDevice.Features/
|   |   |-- Applications/
|   |   |   |-- ApplicationBatch.cs
|   |   |   +-- ApplicationService.cs
|   |   |-- Automation/
|   |   |   |-- AutomationService.cs
|   |   |   +-- NodeSearch.cs
|   |   |-- Clipboard/
|   |   |   +-- ClipboardService.cs
|   |   |-- Debug/
|   |   |   +-- DebugWorkspace.cs
|   |   |-- Extensions/
|   |   |   |-- ApplicationBackupService.cs
|   |   |   +-- ProxyService.cs
|   |   |-- Files/
|   |   |   +-- TransferQueue.cs
|   |   |-- MatrixDevice.Features.csproj
|   |   |-- packages.lock.json
|   |   +-- Scripts/
|   |       |-- ScriptExecutionCompiler.cs
|   |       |-- ScriptJobs.cs
|   |       |-- ScriptPackageService.cs
|   |       +-- ScriptScheduler.cs
|   |-- MatrixDevice.Host/
|   |   |-- Api/
|   |   |   +-- ApiServiceManager.cs
|   |   |-- Diagnostics/
|   |   |   +-- RollingLog.cs
|   |   |-- LdConnectionAdmission.cs
|   |   |-- LdPlayer/
|   |   |   +-- LdLaunchMemory.cs
|   |   |-- MatrixDevice.Host.csproj
|   |   |-- MatrixRuntime.cs
|   |   |-- Monitoring/
|   |   |   |-- LdRunningWindow.cs
|   |   |   |-- ResourceSampler.cs
|   |   |   +-- ScreenVideoSession.cs
|   |   |-- packages.lock.json
|   |   +-- Persistence/
|   |       |-- CatalogRepository.cs
|   |       +-- SettingsStore.cs
|   +-- MatrixDevice.Mcp/
|       |-- BoundedTransportStreams.cs
|       |-- ImportedScriptStore.cs
|       |-- MatrixDevice.Mcp.csproj
|       |-- McpToolHost.cs
|       |-- OperationLedger.cs
|       |-- OwnedConsoleIo.cs
|       |-- packages.lock.json
|       |-- Program.cs
|       |-- ProtocolAdmission.cs
|       |-- Schemas.json
|       |-- StablePath.cs
|       |-- StartupOptions.cs
|       +-- StrictJson.cs
|-- tests/
|   |-- MatrixDevice.Mcp.Tests/
|   |   |-- ContractSchema.json
|   |   |-- MatrixDevice.Mcp.Tests.csproj
|   |   |-- OwnedStdioProcess.cs
|   |   |-- packages.lock.json
|   |   |-- Program.cs
|   |   |-- ProtocolSchema.json
|   |   |-- README.md
|   |   |-- StartupRejections.cs
|   |   +-- StdioSession.cs
|   +-- MatrixDevice.Tests/
|       |-- ApiStartupTests.cs
|       |-- ApplicationBatchTests.cs
|       |-- ApplicationPreflightAcceptance.cs
|       |-- CatalogPathTests.cs
|       |-- DeviceWriteTests.cs
|       |-- ExistingAdbTests.cs
|       |-- LdLaunchMemoryTests.cs
|       |-- LdRunningWindowTests.cs
|       |-- LdWorkspaceTests.cs
|       |-- LiveAcceptance.cs
|       |-- MatrixDevice.Tests.csproj
|       |-- MonitorResourceTests.cs
|       |-- MultiInstanceAcceptance.cs
|       |-- packages.lock.json
|       |-- Program.cs
|       |-- ScreenVideoTests.cs
|       |-- ScriptJobTests.cs
|       +-- ScriptParameterTests.cs
+-- tools/
    |-- Build.ps1
    |-- csc-managed.cmd
    |-- Get-AdbHostState.ps1
    |-- Invoke-HttpToolkitMcp.ps1
    |-- Measure-MultiEvidence.ps1
    |-- New-AcceptanceInstance.ps1
    |-- New-SourcePackage.ps1
    |-- Start-Desktop.ps1
    |-- Start-IsolatedHttpToolkit.ps1
    |-- Start-MultiVerifiedDesktop.cmd
    |-- Start-MultiVerifiedDesktop.ps1
    |-- Start-VerifiedDesktop.cmd
    |-- Start-VerifiedDesktop.ps1
    +-- Write-AcceptanceResult.ps1
```
