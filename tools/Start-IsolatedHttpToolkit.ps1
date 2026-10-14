param([Parameter(Mandatory)][string] $EvidenceDirectory)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($EvidenceDirectory)) { throw '证据目录须为绝对路径。' }
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$missingSdk = Join-Path $root 'artifacts\http-toolkit-no-android-sdk'
if (Test-Path -LiteralPath $missingSdk) { throw '隔离SDK路径必须不存在，不能让HTTP Toolkit找到adb。' }
if (Get-NetTCPConnection -State Listen -LocalPort 65534 -ErrorAction SilentlyContinue) { throw 'ADB隔离端口65534已被占用。' }
if (Get-Process -Name 'HTTP Toolkit' -ErrorAction SilentlyContinue) { throw 'HTTP Toolkit已运行，不能保证其继承隔离环境。' }
$exe = Join-Path $env:LOCALAPPDATA 'Programs\HTTP Toolkit\HTTP Toolkit.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw '未安装HTTP Toolkit，本脚本不自动安装。' }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$start = [Diagnostics.ProcessStartInfo]::new($exe)
$start.UseShellExecute = $false
$start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
$start.Environment['ANDROID_ADB_SERVER_PORT'] = '65534'
$start.Environment['ANDROID_HOME'] = $missingSdk
$start.Environment['ADB_SERVER_SOCKET'] = 'tcp:127.0.0.1:65534'
$process = [Diagnostics.Process]::Start($start)
if ($null -eq $process) { throw 'HTTP Toolkit未启动。' }
$receipt = [pscustomobject]@{ StartedUtc = [DateTime]::UtcNow.ToString('O'); ProcessId = $process.Id;
    StartedUtcTicks = $process.StartTime.ToUniversalTime().Ticks; Executable = $exe;
    Sha256 = (Get-FileHash -LiteralPath $exe).Hash; AdbPort = 65534; AndroidHome = $missingSdk;
    ChangedGlobalEnvironment = $false; InstalledCertificate = $false; ChangedSystemProxy = $false }
$receipt | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'isolated-start.json') -Encoding utf8NoBOM
$receipt | ConvertTo-Json
