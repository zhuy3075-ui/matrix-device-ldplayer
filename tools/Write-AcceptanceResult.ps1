param(
    [Parameter(Mandatory)] [string] $OutputDirectory,
    [Parameter(Mandatory)] [string] $CaseId,
    [Parameter(Mandatory)] [ValidateSet('PASS-OFFLINE','PASS-LIVE','FAIL','BLOCKED','NOT-IMPLEMENTED','N/A')] [string] $Status,
    [Parameter(Mandatory)] [string] $Expected,
    [Parameter(Mandatory)] [string] $Actual,
    [string] $Phase = 'T02', [string] $Authorization = 'offline',
    [string] $Serial = '', [int] $Index = -1, [long] $Generation = 0,
    [string] $DefectId = '', [int] $Round = 1, [long] $DurationMs = 0,
    [string[]] $EvidenceFiles = @()
)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '测试日志目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$assembly = Join-Path $root 'src\MatrixDevice.Core\bin\Release\net10.0\MatrixDevice.Core.dll'
$hash = if (Test-Path -LiteralPath $assembly) { (Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash } else { $null }
$artifacts = @($EvidenceFiles | ForEach-Object {
    $file = Get-Item -LiteralPath $_ -ErrorAction Stop
    [ordered]@{ path = [IO.Path]::GetRelativePath($root, $file.FullName); bytes = $file.Length; sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
})
$record = [ordered]@{
    caseId = $CaseId; recordedUtc = [DateTime]::UtcNow.ToString('o'); phase = $Phase
    recordingBuildSha256 = $hash; executionBuildSha256 = $null; canonicalSerial = $Serial; targetIndex = $Index; generation = $Generation
    authorization = $Authorization; expected = $Expected; actual = $Actual; status = $Status
    durationMs = $DurationMs; defectId = $DefectId; round = $Round; artifacts = $artifacts
    cleanup = '见对应阶段记录'; measurement = 'manual-summary'
}
$record | ConvertTo-Json -Depth 8 -Compress | Add-Content -LiteralPath (Join-Path $OutputDirectory 'results.jsonl') -Encoding utf8
