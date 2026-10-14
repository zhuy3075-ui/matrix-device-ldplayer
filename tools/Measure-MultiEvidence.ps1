param([Parameter(Mandatory)][string] $Directory, [Parameter(Mandatory)][string] $SpecFile)
$ErrorActionPreference = 'Stop'
$directoryPath = (Resolve-Path -LiteralPath $Directory).Path
function Read-Journal([string] $Name) {
    @(Get-Content -LiteralPath (Join-Path $directoryPath $Name) | Where-Object { $_.Trim() } | ConvertFrom-Json)
}
function Percentile([double[]] $Values, [double] $Fraction) {
    if ($Values.Count -eq 0) { throw 'No values for percentile' }
    $ordered = @($Values | Sort-Object)
    $ordered[[math]::Max(0, [math]::Ceiling($ordered.Count * $Fraction) - 1)]
}
$summaries = Read-Journal 'summary.jsonl'
if ($summaries.Count -ne 1 -or $summaries[0].status -ne 'PASS-LIVE-MULTI') { throw 'Only a completed successful run can be analyzed as acceptance evidence' }
$summary = $summaries[0]
$specPath = (Resolve-Path -LiteralPath $SpecFile).Path
$spec = Get-Content -LiteralPath $specPath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath (Join-Path $directoryPath 'run-manifest.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $specPath -Algorithm SHA256).Hash -ne $manifest.specSha256) { throw 'Run manifest does not match the supplied specification' }
if ($spec.minutes -lt 1 -or $spec.minutes -gt 30 -or $summary.pressureMs -lt $spec.minutes * 60000 -or $summary.targetCount -ne $spec.targets.Count) { throw 'Completed duration or target count is below specification' }
$cases = Read-Journal 'results.jsonl'
if (@($cases | Where-Object { $_.status -ne 'PASS-LIVE' }).Count) { throw 'Failed cases cannot be counted as successful acceptance' }
foreach ($case in $cases) {
    $file = [IO.Path]::GetFullPath((Join-Path $directoryPath $case.file))
    if (-not $file.StartsWith($directoryPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Case path is outside evidence directory' }
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $case.sha256) { throw "Case hash mismatch: $($case.file)" }
}
$rounds = @($cases | Where-Object { $_.task -match '^round-\d+$' })
$returns = Read-Journal 'returns.jsonl'
if ($rounds.Count -ne $returns.Count) { throw 'Round and return counts differ' }
$http = Read-Journal 'http.jsonl'
$requestIds = @($http.id | Select-Object -Unique)
if ($requestIds.Count -ne $http.Count) { throw 'Duplicate HTTP request ID' }
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($returned in $returns) {
    $target = @($spec.targets | Where-Object { $_.serial -eq $returned.serial -and $_.index -eq $returned.index })
    if ($target.Count -ne 1) { throw 'Return is not from a unique specified target' }
    if (-not $keys.Add("$($returned.serial):$($returned.round)")) { throw 'Duplicate device/round return' }
    $case = @($rounds | Where-Object { $_.serial -eq $returned.serial -and $_.task -eq "round-$($returned.round)" })
    if ($case.Count -ne 1) { throw 'Return has no unique successful round' }
    $payload = Get-Content -LiteralPath (Join-Path $directoryPath $case[0].file) -Raw | ConvertFrom-Json
    if ($payload.nonce -ne $returned.nonce -or $payload.sha256 -ne $returned.sha256 -or $payload.text -ne $returned.text) { throw 'Return does not match recorded actual round' }
    $ack = @($http | Where-Object { $_.task -eq 'data-return' -and $_.body.serial -eq $returned.serial -and $_.body.round -eq $returned.round })
    if ($ack.Count -ne 1 -or $ack[0].status -ne 200) { throw 'Missing unique HTTP acknowledgement' }
    if ($ack[0].body.sha256 -ne $returned.sha256 -or $ack[0].body.nonce -ne $returned.nonce -or $ack[0].body.text -ne $returned.text) { throw 'HTTP return payload does not match actual round' }
    $reply = $ack[0].response | ConvertFrom-Json
    if (-not $reply.accepted -or $reply.serial -ne $returned.serial -or $reply.nonce -ne $returned.nonce) { throw 'Incorrect HTTP acknowledgement' }
    $textRequests = @($http | Where-Object { $_.task -eq 'text-input' -and $_.serial -eq $returned.serial -and $_.body.text -eq $returned.text })
    if ($textRequests.Count -ne 1 -or $textRequests[0].status -ne 200) { throw 'Missing unique successful text request' }
    $identity = @($http | Where-Object { $_.task -eq 'identity-return' -and $_.serial -eq $returned.serial -and $_.response.Contains($returned.sha256, [StringComparison]::OrdinalIgnoreCase) })
    if ($identity.Count -ne 1 -or $identity[0].status -ne 200) { throw 'Missing unique successful device readback' }
    $readback = ($identity[0].response | ConvertFrom-Json).data.output
    if (-not $readback.StartsWith($returned.nonce, [StringComparison]::Ordinal) -or -not $readback.Contains($returned.sha256, [StringComparison]::OrdinalIgnoreCase)) { throw 'Device readback does not match instance nonce and file hash' }
}
$pressureStart = @($rounds | Sort-Object { [datetimeoffset] $_.start })[0].start
$resources = @(Read-Journal 'resources.jsonl' | Where-Object { [datetimeoffset] $_.utc -ge [datetimeoffset] $pressureStart })
if ($resources.Count -lt 2) { throw 'Not enough pressure resource samples' }
$workers = Read-Journal 'workers.jsonl'
$deviceStats = foreach ($group in $returns | Group-Object serial) {
    $serial = $group.Name
    $items = @($rounds | Where-Object { $_.serial -eq $serial } | Sort-Object { [datetimeoffset] $_.start })
    $end = @($workers | Where-Object { $_.serial -eq $serial -and $_.state -eq 'end' })
    if ($end.Count -ne 1 -or $end[0].rounds -ne $items.Count -or $end[0].returns -ne $items.Count) { throw 'Worker totals do not match successful rounds' }
    $summaryCount = @($summary.returns.PSObject.Properties | Where-Object { $_.Name -eq $serial })
    if ($summaryCount.Count -ne 1 -or $summaryCount[0].Value -ne $items.Count) { throw 'Summary return totals do not match successful rounds' }
    $nonce = @($group.Group.nonce | Select-Object -Unique)
    if ($nonce.Count -ne 1) { throw 'Worker nonce changed' }
    $gaps = @(for ($i = 1; $i -lt $items.Count; $i++) { ([datetimeoffset]$items[$i].end - [datetimeoffset]$items[$i-1].end).TotalMilliseconds })
    [pscustomobject]@{
        Serial = $serial; Index = $group.Group[0].index; Rounds = $items.Count
        UploadMiB = ($group.Group.bytes | Measure-Object -Sum).Sum / 1MB
        MedianRoundMs = Percentile $items.durationMs .5; P95RoundMs = Percentile $items.durationMs .95
        MaxRoundMs = ($items.durationMs | Measure-Object -Maximum).Maximum
        MaxReturnGapMs = ($gaps | Measure-Object -Maximum).Maximum
        FirstStart = $items[0].start; LastEnd = $items[-1].end
    }
}
if (@($deviceStats | Select-Object -ExpandProperty Index -Unique).Count -ne $summary.targetCount) { throw 'Target count mismatch' }
if (@($returns | Group-Object nonce).Count -ne $summary.targetCount) { throw 'Different devices shared a nonce' }
if (@($http | Where-Object { $_.proxy -ne $spec.proxyPort }).Count) { throw 'HTTP proxy port does not match specification' }
$processStats = foreach ($pidGroup in @($resources.processes | Group-Object pid)) {
    $items = @($pidGroup.Group)
    [pscustomobject]@{
        Pid = [int]$pidGroup.Name; Name = $items[0].processName
        FirstWorkingSetMiB = $items[0].workingSet64 / 1MB; LastWorkingSetMiB = $items[-1].workingSet64 / 1MB
        PeakWorkingSetMiB = ($items.workingSet64 | Measure-Object -Maximum).Maximum / 1MB
        FirstPrivateMiB = $items[0].privateMemorySize64 / 1MB; LastPrivateMiB = $items[-1].privateMemorySize64 / 1MB
        PeakPrivateMiB = ($items.privateMemorySize64 | Measure-Object -Maximum).Maximum / 1MB
        FirstHandles = $items[0].handleCount; LastHandles = $items[-1].handleCount
        PeakHandles = ($items.handleCount | Measure-Object -Maximum).Maximum
        CpuTimeDeltaMs = $items[-1].cpuTimeMs - $items[0].cpuTimeMs
    }
}
[pscustomobject]@{
    Summary = $summary; SpecSha256 = $manifest.specSha256; VerifiedCaseHashes = $cases.Count; RoundReturnPairs = $returns.Count
    DeviceStats = @($deviceStats); HttpCount = $http.Count
    HttpStatuses = @($http | Group-Object status | Select-Object Name,Count)
    HttpProxyPorts = @($http.proxy | Select-Object -Unique)
    HttpMedianMs = Percentile $http.durationMs .5; HttpP95Ms = Percentile $http.durationMs .95
    PressureResourceSamples = $resources.Count
    CpuMedianPercent = Percentile $resources.cpuPercent .5; CpuP95Percent = Percentile $resources.cpuPercent .95
    CpuMaxPercent = ($resources.cpuPercent | Measure-Object -Maximum).Maximum
    MinAvailableGiB = ($resources.availablePhysical | Measure-Object -Minimum).Minimum / 1GB
    MaxCommitPercent = (@($resources | ForEach-Object { 100 * (1 - $_.availablePageFile / $_.totalPageFile) }) | Measure-Object -Maximum).Maximum
    ProcessStats = @($processStats)
    Limits = @('File-only validation; no live host or guest checks or current DLL hash verification', 'Direct HTTP journals are not HTTP Toolkit captures', 'Per-process working sets are not summed as deduplicated physical RAM', 'No GPU or long-term leak conclusion')
} | ConvertTo-Json -Depth 10
