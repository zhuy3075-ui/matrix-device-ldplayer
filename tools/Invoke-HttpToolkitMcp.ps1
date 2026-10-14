param([Parameter(Mandatory)][string] $OutputPath, [string] $ToolName = '', [string] $ArgumentsJson = '{}')
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputPath)) { throw '输出路径须为绝对路径。' }
$command = Join-Path $env:LOCALAPPDATA 'Programs\HTTP Toolkit\resources\httptoolkit-mcp.cmd'
if (-not (Test-Path -LiteralPath $command -PathType Leaf)) { throw '未安装官方HTTP Toolkit MCP CLI。' }
$start = [Diagnostics.ProcessStartInfo]::new('cmd.exe')
$start.Arguments = '/d /s /c ""' + $command + '""'
$start.UseShellExecute = $false; $start.CreateNoWindow = $true; $start.WindowStyle = 'Hidden'
$start.RedirectStandardInput = $true; $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
$process = [Diagnostics.Process]::Start($start)
$errorRead = $process.StandardError.ReadToEndAsync()
$responses = [Collections.Generic.List[object]]::new()
function Call-Rpc([int] $Id, [string] $Method, [object] $Parameters) {
    $request = @{jsonrpc='2.0'; id=$Id; method=$Method; params=$Parameters}
    $process.StandardInput.WriteLine(($request | ConvertTo-Json -Depth 40 -Compress)); $process.StandardInput.Flush()
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ([DateTime]::UtcNow -lt $deadline) {
        $read = $process.StandardOutput.ReadLineAsync()
        if (-not $read.Wait(45000)) { throw "HTTP Toolkit MCP超时：$Method" }
        if ($null -eq $read.Result) { throw 'HTTP Toolkit MCP输出提前结束。' }
        $response = $read.Result | ConvertFrom-Json -Depth 80
        $responses.Add($response)
        if ($response.id -eq $Id) { return $response }
    }
    throw "HTTP Toolkit MCP超时：$Method"
}
try {
    [void](Call-Rpc 1 'initialize' @{protocolVersion='2025-11-25'; capabilities=@{}; clientInfo=@{name='matrix-multi-acceptance';version='1.0'}})
    $process.StandardInput.WriteLine('{"jsonrpc":"2.0","method":"notifications/initialized"}'); $process.StandardInput.Flush()
    $listed = Call-Rpc 2 'tools/list' @{}
    if ($ToolName) {
        $tool = @($listed.result.tools | Where-Object name -eq $ToolName)
        if ($tool.Count -ne 1 -or $tool[0].annotations.readOnlyHint -ne $true) { throw '仅允许当前官方列表中明确标记只读的工具。' }
        $arguments = $ArgumentsJson | ConvertFrom-Json -AsHashtable -Depth 80
        $called = Call-Rpc 3 'tools/call' @{name=$ToolName;arguments=$arguments}
        if ($called.error -or $called.result.isError) { throw 'HTTP Toolkit拒绝此只读调用，结果未计为捕获成功。' }
    }
} finally {
    New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
    $responses | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
    try { $process.StandardInput.Close() } catch [IO.IOException] { Write-Warning $_.Exception.Message }
    if (-not $process.WaitForExit(3000)) { $process.Kill() }
    if ($errorRead.Wait(3000)) { $errorRead.Result | Set-Content -LiteralPath ($OutputPath + '.stderr.txt') -Encoding utf8NoBOM }
    $process.Dispose()
}
if ($ToolName) { $called | ConvertTo-Json -Depth 40 }
else { $listed.result.tools | Select-Object name,description,annotations,inputSchema | ConvertTo-Json -Depth 30 }
