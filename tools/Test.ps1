param([string] $DotnetPath)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
if (-not (Test-Path -LiteralPath (Join-Path $root 'tests/MatrixDevice.Tests.slnx'))) { throw '此源码包未包含测试工程；请克隆完整仓库或使用 New-SourcePackage.ps1 -IncludeTests。' }
if (-not $DotnetPath) {
    $portable = Join-Path (Split-Path -Parent $root) '.tools/dotnet/dotnet.exe'
    $DotnetPath = if (Test-Path -LiteralPath $portable) { $portable } else { (Get-Command dotnet -ErrorAction Stop).Source }
}
$DotnetPath = (Resolve-Path -LiteralPath $DotnetPath).Path
$sdk = & $DotnetPath --version
if ($LASTEXITCODE -ne 0) { throw 'Cannot read .NET SDK version' }
$oldDotnet = $env:MATRIX_DOTNET_EXE
$oldCompiler = $env:MATRIX_CSC_DLL
$output = Join-Path $root ('artifacts/test-support/' + [Guid]::NewGuid().ToString('N'))
try {
    $env:MATRIX_DOTNET_EXE = $DotnetPath
    $env:MATRIX_CSC_DLL = Join-Path (Split-Path -Parent $DotnetPath) ('sdk/' + $sdk + '/Roslyn/bincore/csc.dll')
    $releaseOutput = Join-Path $output 'release-contracts'
    & $DotnetPath build (Join-Path $root 'tests/MatrixDevice.Release.Tests/MatrixDevice.Release.Tests.csproj') -c Release --artifacts-path $releaseOutput --nologo -m:1 -p:UseSharedCompilation=false -p:CscToolExe=csc-managed.cmd ('-p:CscToolPath=' + $PSScriptRoot)
    if ($LASTEXITCODE -ne 0) { throw 'Release contract build failed' }
    & $DotnetPath (Join-Path $releaseOutput 'bin/MatrixDevice.Release.Tests/release/MatrixDevice.Release.Tests.dll') $DotnetPath $root (Join-Path $output 'release-evidence')
    if ($LASTEXITCODE -ne 0) { throw 'Release contracts failed' }
    Write-Output ('TEST_SUPPORT_OUTPUT=' + $output)
    & $DotnetPath build (Join-Path $root 'tests/MatrixDevice.Tests.slnx') -c Release --artifacts-path $output --nologo -m:1 -p:EnableTestSupport=true -p:UseSharedCompilation=false -p:CscToolExe=csc-managed.cmd ('-p:CscToolPath=' + $PSScriptRoot)
    if ($LASTEXITCODE -ne 0) { throw 'Isolated test-support build failed' }
    & $DotnetPath (Join-Path $output 'bin/MatrixDevice.Tests/release/MatrixDevice.Tests.dll')
    if ($LASTEXITCODE -ne 0) { throw 'Offline regression failed' }
    $evidence = Join-Path $output 'mcp-evidence'
    & $DotnetPath (Join-Path $output 'bin/MatrixDevice.Mcp.Tests/release/MatrixDevice.Mcp.Tests.dll') --dotnet $DotnetPath --server (Join-Path $output 'bin/MatrixDevice.Mcp/release/MatrixDevice.Mcp.dll') --output $evidence
    if ($LASTEXITCODE -ne 0) { throw 'MCP fixture verification failed' }
    Write-Output ('MCP_DEMO_VERIFIED_RELATIVE=' + [IO.Path]::GetRelativePath($root, $evidence))
} finally {
    $env:MATRIX_DOTNET_EXE = $oldDotnet
    $env:MATRIX_CSC_DLL = $oldCompiler
}
