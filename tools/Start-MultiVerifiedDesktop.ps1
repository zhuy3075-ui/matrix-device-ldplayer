param([switch] $Monitor)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$directory = Join-Path $root 'data\multi-verified'
$configuration = Join-Path $directory 'AppConfig.json'
if (-not (Test-Path -LiteralPath $configuration -PathType Leaf)) { throw '没有本机多实例授权配置。请先按README确认串号和索引绑定，不从公式猜测设备。' }
$settings = Get-Content -LiteralPath $configuration -Raw | ConvertFrom-Json
$serials = @($settings.adbAllowedSerials)
$bindings = @($settings.ldSerialBindings.PSObject.Properties)
if ($serials.Count -lt 2 -or $bindings.Count -ne $serials.Count -or @($serials | Select-Object -Unique).Count -ne $serials.Count) { throw '多实例配置必须有至少两个不同串号及一一对应绑定。' }
foreach ($binding in $bindings) {
    $index = 0
    if (-not [int]::TryParse($binding.Name, [ref] $index) -or $index -le 0 -or $index -ge 99999 -or $binding.Name -ne $index.ToString([Globalization.CultureInfo]::InvariantCulture) -or $binding.Value -notin $serials) { throw '该启动入口只允许规范索引的非零独立测试实例，不允许桌面哨兵或重复索引别名。' }
}
if (@($bindings.Value | Select-Object -Unique).Count -ne $serials.Count) { throw '多个索引不能绑定同一串号。' }
& (Join-Path $PSScriptRoot 'Start-Desktop.ps1') -DataDirectory $directory -Monitor:$Monitor
