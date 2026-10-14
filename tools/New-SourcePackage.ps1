param([string] $ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$projectPath = (Resolve-Path -LiteralPath $ProjectRoot).Path
$artifactDirectory = Join-Path $projectPath 'artifacts'
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$sources = @(Get-ChildItem -LiteralPath $projectPath -Recurse -File | Where-Object {
    $relative = [IO.Path]::GetRelativePath($projectPath, $_.FullName)
    $relative -notmatch '(^|[\\/])(bin|obj|artifacts|data|\.git)([\\/]|$)' -and
    $_.Name -notin @('SOURCE_BUNDLE.md', 'PROJECT_TREE.md', 'SOURCE_MANIFEST.json') -and
    $_.Name -notlike 'desktop-english*'
} | Sort-Object FullName)
$tree = @('# 完整项目文件目录', '', '```text', 'MatrixDevice.Reconstructed/')
$relativePaths = @($sources | ForEach-Object { [IO.Path]::GetRelativePath($projectPath, $_.FullName).Replace('\', '/') })
function Format-Tree([string[]] $Paths, [string] $Prefix = '') {
    $parts = @($Paths | ForEach-Object { $_.Split('/')[0] } | Sort-Object -Unique)
    for ($index = 0; $index -lt $parts.Count; $index++) {
        $name = $parts[$index]
        $children = @($Paths | Where-Object { $_.StartsWith($name + '/', [StringComparison]::Ordinal) } | ForEach-Object { $_.Substring($name.Length + 1) })
        $last = $index -eq $parts.Count - 1
        $branch = if ($last) { '+-- ' } else { '|-- ' }
        $suffix = if ($children.Count -gt 0) { '/' } else { '' }
        $Prefix + $branch + $name + $suffix
        if ($children.Count -gt 0) {
            $continuation = if ($last) { '    ' } else { '|   ' }
            Format-Tree -Paths $children -Prefix ($Prefix + $continuation)
        }
    }
}
$tree += @(Format-Tree -Paths ($relativePaths + @('PROJECT_TREE.md','SOURCE_MANIFEST.json','SOURCE_BUNDLE.md')))
$manifest = foreach ($source in $sources) {
    $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
    [pscustomobject]@{ Path = $relative; Bytes = $source.Length; Sha256 = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$tree += '```'
Set-Content -LiteralPath (Join-Path $projectPath 'PROJECT_TREE.md') -Value $tree -Encoding utf8NoBOM
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $projectPath 'SOURCE_MANIFEST.json') -Encoding utf8NoBOM
$bundle = [Text.StringBuilder]::new()
[void] $bundle.AppendLine('# 全部源代码与构建文件')
foreach ($source in $sources | Where-Object { $_.Extension -in @('.cs','.xaml','.csproj','.props','.slnx','.ps1','.cmd','.manifest','.json','.Config') }) {
    $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
    $language = switch ($source.Extension.ToLowerInvariant()) { '.cs' { 'csharp' } '.ps1' { 'powershell' } '.cmd' { 'batch' } '.json' { 'json' } default { 'xml' } }
    [void] $bundle.AppendLine("`n## $relative`n")
    [void] $bundle.AppendLine('```' + $language)
    [void] $bundle.AppendLine([IO.File]::ReadAllText($source.FullName))
    [void] $bundle.AppendLine('```')
}
[IO.File]::WriteAllText((Join-Path $projectPath 'SOURCE_BUNDLE.md'), $bundle.ToString(), [Text.UTF8Encoding]::new($false))
$archivePath = Join-Path $artifactDirectory ('MatrixDevice.Source.' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.zip')
$stream = [IO.File]::Open($archivePath, [IO.FileMode]::CreateNew)
try {
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        foreach ($source in $sources) {
            $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $source.FullName, 'MatrixDevice.Reconstructed/' + $relative) | Out-Null
        }
        foreach ($name in @('PROJECT_TREE.md','SOURCE_MANIFEST.json','SOURCE_BUNDLE.md')) {
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $projectPath $name), 'MatrixDevice.Reconstructed/' + $name) | Out-Null
        }
    } finally { $archive.Dispose() }
} finally { $stream.Dispose() }
[pscustomobject]@{ Archive = $archivePath; SourceFiles = $sources.Count; Sha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash } | ConvertTo-Json
