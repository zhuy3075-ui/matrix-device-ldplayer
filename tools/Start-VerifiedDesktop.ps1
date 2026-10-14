param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$directory = Join-Path $root 'data\verified-acceptance'
$configuration = Join-Path $directory 'AppConfig.json'
if (-not (Test-Path -LiteralPath $configuration -PathType Leaf)) { throw '没有本机已核验的独立实例配置。请先按README配置，不能从索引公式猜测串号。' }
$settings = Get-Content -LiteralPath $configuration -Raw | ConvertFrom-Json
if (@($settings.adbAllowedSerials).Count -ne 1 -or @($settings.ldSerialBindings.PSObject.Properties).Count -ne 1) { throw '独立测试配置缺少唯一授权串号及显式索引绑定。' }
& (Join-Path $PSScriptRoot 'Start-Desktop.ps1') -DataDirectory $directory
