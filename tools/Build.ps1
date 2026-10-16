param([string] $DotnetPath, [switch] $Verify)
$ErrorActionPreference = 'Stop'
$projectPath = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
if (-not $DotnetPath) {
    $bundled = Join-Path (Split-Path -Parent $projectPath) '.tools/dotnet/dotnet.exe'
    $DotnetPath = if (Test-Path -LiteralPath $bundled) { $bundled } else { (Get-Command dotnet -ErrorAction Stop).Source }
}
$DotnetPath = (Resolve-Path -LiteralPath $DotnetPath).Path
$sdkVersion = & $DotnetPath --version
if ($LASTEXITCODE -ne 0) { throw 'Cannot read .NET SDK version' }
$sdkRoot = Join-Path (Split-Path -Parent $DotnetPath) ('sdk/' + $sdkVersion)
$oldDotnet = $env:MATRIX_DOTNET_EXE
$oldCompiler = $env:MATRIX_CSC_DLL
try {
    $env:MATRIX_DOTNET_EXE = $DotnetPath
    $env:MATRIX_CSC_DLL = Join-Path $sdkRoot 'Roslyn/bincore/csc.dll'
    $buildArgs = @('build', (Join-Path $projectPath 'MatrixDevice.slnx'), '-c', 'Release', '--nologo', '-m:1', '-p:UseSharedCompilation=false', '-p:CscToolExe=csc-managed.cmd', ('-p:CscToolPath=' + $PSScriptRoot))
    & $DotnetPath @buildArgs
    if ($LASTEXITCODE -ne 0) { throw 'Project build failed' }
    if ($Verify) {
        & $DotnetPath (Join-Path $projectPath 'tests/MatrixDevice.Tests/bin/Release/net10.0/MatrixDevice.Tests.dll')
        if ($LASTEXITCODE -ne 0) { throw 'Verification failed' }
        $mcpEvidence = 'artifacts/mcp-stdio-main/verify-' + [Guid]::NewGuid().ToString('N')
        Write-Output ('MCP_EVIDENCE_RELATIVE=' + $mcpEvidence)
        & $DotnetPath (Join-Path $projectPath 'tests/MatrixDevice.Mcp.Tests/bin/Release/net10.0/MatrixDevice.Mcp.Tests.dll') --dotnet $DotnetPath --server (Join-Path $projectPath 'src/MatrixDevice.Mcp/bin/Release/net10.0/MatrixDevice.Mcp.dll') --output (Join-Path $projectPath $mcpEvidence)
        if ($LASTEXITCODE -ne 0) { throw 'MCP Demo verification failed' }
        Write-Output ('MCP_DEMO_VERIFIED_RELATIVE=' + $mcpEvidence)
    }
} finally {
    $env:MATRIX_DOTNET_EXE = $oldDotnet
    $env:MATRIX_CSC_DLL = $oldCompiler
}
