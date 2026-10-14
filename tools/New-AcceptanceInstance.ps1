param([Parameter(Mandatory)] [string] $OutputDirectory,
      [string] $ConsolePath = 'E:\leidian\LDPlayer14\ldconsole.exe')
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '证据目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$name = 'MatrixAcceptance-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$before = @(& $ConsolePath list2)
if ($LASTEXITCODE -ne 0) { throw '创建前list2失败。' }
$before | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ld-before.txt') -Encoding utf8
$indices = @($before | Where-Object { $_ -match '^\d+,' } | ForEach-Object { [int]($_ -split ',')[0] })
$output = @(& $ConsolePath add --name $name)
$code = $LASTEXITCODE
[IO.File]::WriteAllLines((Join-Path $OutputDirectory 'ld-add.txt'), [string[]]$output)
$after = @(& $ConsolePath list2)
if ($LASTEXITCODE -ne 0) { throw '创建后list2失败；不重试add。' }
$after | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ld-after-add.txt') -Encoding utf8
$added = @($after | Where-Object { $_ -match '^\d+,' -and [int]($_ -split ',')[0] -notin $indices })
if ($added.Count -ne 1) { throw "新增实例不唯一，actual=$($added.Count)；不启动。" }
$fields = $added[0] -split ','
if ($fields[1] -ne $name) { throw '新增实例名称不匹配；不启动。' }
$index = [int]$fields[0]
$receipt = [ordered]@{ createdUtc = [DateTime]::UtcNow.ToString('o'); index = $index; name = $name; beforeIndices = $indices; createdRow = $added[0]; addExitCode = $code; result = if ($code -eq 0) { 'CREATED' } else { 'CREATED-WITH-NONZERO-EXIT' }; copied = $false; retained = $true }
$receipt | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'instance.json') -Encoding utf8
$receipt | ConvertTo-Json -Depth 5
