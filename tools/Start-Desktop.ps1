param([switch] $Demo, [string] $LdConsole = '', [string] $DataDirectory = '')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$portable = Join-Path (Split-Path -Parent $root) '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $portable -PathType Leaf) { $portable } else { (Get-Command dotnet -CommandType Application -ErrorAction Stop).Source }
$assembly = Join-Path $root 'src\MatrixDevice.Desktop\bin\Release\net10.0-windows\MatrixDevice.Desktop.dll'
if (-not (Test-Path -LiteralPath $assembly -PathType Leaf)) { throw '请先运行 tools/Build.ps1 构建桌面程序。' }
$arguments = @($assembly)
if ($Demo) { $arguments += '--demo' }
if ($DataDirectory) {
    if (-not [IO.Path]::IsPathFullyQualified($DataDirectory)) { throw '配置目录必须是绝对路径。' }
    $arguments += @('--data-dir', $DataDirectory)
}
if ($LdConsole) {
    if (-not [IO.Path]::IsPathFullyQualified($LdConsole) -or -not (Test-Path -LiteralPath $LdConsole -PathType Leaf)) { throw '雷电命令行程序必须是已存在的绝对路径。' }
    $arguments += @('--ldconsole', $LdConsole)
}
& $dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "桌面程序异常退出，退出码：$LASTEXITCODE" }
