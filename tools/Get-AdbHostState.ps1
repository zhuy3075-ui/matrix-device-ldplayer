param([Parameter(Mandatory)] [string] $OutputDirectory)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '证据目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
function Read-Exact([IO.Stream] $Stream, [int] $Length) {
    $bytes = [byte[]]::new($Length)
    $offset = 0
    while ($offset -lt $Length) {
        $count = $Stream.Read($bytes, $offset, $Length - $offset)
        if ($count -eq 0) { throw 'ADB host响应截断。' }
        $offset += $count
    }
    return ,$bytes
}
function Read-HostQuery([string] $Request) {
    if ($Request -notin @('host:version', 'host:devices-l')) { throw '仅允许主机只读查询。' }
    $client = [Net.Sockets.TcpClient]::new()
    try {
        $connection = $client.ConnectAsync([Net.IPAddress]::Loopback, 5037)
        if (-not $connection.Wait(3000)) { throw '现有ADB服务连接超时；不启动服务。' }
        $stream = $client.GetStream()
        $stream.ReadTimeout = 3000
        $stream.WriteTimeout = 3000
        $body = [Text.Encoding]::UTF8.GetBytes($Request)
        $prefix = [Text.Encoding]::ASCII.GetBytes($body.Length.ToString('X4'))
        $stream.Write($prefix)
        $stream.Write($body)
        $status = [Text.Encoding]::ASCII.GetString((Read-Exact $stream 4))
        $length = [Convert]::ToInt32([Text.Encoding]::ASCII.GetString((Read-Exact $stream 4)), 16)
        $result = [Text.Encoding]::UTF8.GetString((Read-Exact $stream $length))
        if ($status -ne 'OKAY') { throw "ADB host拒绝：$result" }
        return $result
    }
    finally { $client.Dispose() }
}
$owners = @(Get-NetTCPConnection -LocalPort 5037 -State Listen -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique | ForEach-Object {
        $process = Get-CimInstance Win32_Process -Filter "ProcessId=$_"
        [ordered]@{
            processId = $_
            executable = $process.ExecutablePath
            createdUtc = $process.CreationDate.ToUniversalTime().ToString('o')
            sha256 = (Get-FileHash -LiteralPath $process.ExecutablePath -Algorithm SHA256).Hash
        }
    })
if ($owners.Count -gt 1) { throw "ADB5037所有者不唯一，实际=$($owners.Count)；不创建或停止服务。" }
$snapshot = [ordered]@{
    capturedUtc = [DateTime]::UtcNow.ToString('o')
    owner = if ($owners.Count -eq 1) { $owners[0] } else { $null }
    protocol = if ($owners.Count -eq 1) { Read-HostQuery 'host:version' } else { $null }
    devices = if ($owners.Count -eq 1) { Read-HostQuery 'host:devices-l' } else { $null }
    guestCommands = 0
}
$path = Join-Path $OutputDirectory ('adb-host-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '.json')
$snapshot | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $path -Encoding utf8
$snapshot | ConvertTo-Json -Depth 6
