param(
    [string]$Configuration = "Release",
    [string]$Version = "18.15.0",
    [switch]$KeepPublishDirectory
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src/CorpusLens.Desktop/CorpusLens.Desktop.csproj"
$distRoot = Join-Path $repoRoot "dist"
$publishDirectory = Join-Path $distRoot "CorpusLens-win-x64"
$zipPath = Join-Path $distRoot ("CorpusLens-win-x64-{0}.zip" -f $Version)
$hashPath = "$zipPath.sha256"

if (Test-Path $publishDirectory) {
    Remove-Item $publishDirectory -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
if (Test-Path $hashPath) {
    Remove-Item $hashPath -Force
}

New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null

Write-Host "Publishing CorpusLens $Version for win-x64..."
& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDirectory `
    -p:Version=$Version `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$executablePath = Join-Path $publishDirectory "CorpusLens.exe"
if (-not (Test-Path $executablePath)) {
    throw "Expected executable was not produced: $executablePath"
}

$forbiddenArtifactNames = @(
    "report.md",
    "words.csv",
    "ngrams.csv",
    "next_words.csv",
    "extracted_text.txt",
    "import_failures.csv",
    "import_diagnostics.md"
)
$forbiddenFiles = Get-ChildItem -Path $publishDirectory -Recurse -File | Where-Object {
    $_.Extension -in @(".db", ".sqlite", ".sqlite3", ".epub") -or
    $_.Name -in $forbiddenArtifactNames -or
    $_.Name -like "*.db-wal" -or
    $_.Name -like "*.db-shm"
}
$forbiddenDirectories = Get-ChildItem -Path $publishDirectory -Recurse -Directory | Where-Object {
    $_.Name -in @("artifacts", "books", "data")
}
$forbiddenEntries = @($forbiddenFiles) + @($forbiddenDirectories)
if ($forbiddenEntries.Count -gt 0) {
    $forbiddenList = ($forbiddenEntries.FullName -join [Environment]::NewLine)
    throw "Publish safety check failed. Local corpus data was found:$([Environment]::NewLine)$forbiddenList"
}

Write-Host "Creating package $zipPath..."
Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $zipPath -CompressionLevel Optimal

$hash = Get-FileHash -Path $zipPath -Algorithm SHA256
$hashLine = "{0}  {1}" -f $hash.Hash.ToLowerInvariant(), (Split-Path $zipPath -Leaf)
Set-Content -Path $hashPath -Value $hashLine -Encoding ascii

Write-Host "Package created: $zipPath"
Write-Host "SHA-256: $($hash.Hash.ToLowerInvariant())"

if (-not $KeepPublishDirectory) {
    Remove-Item $publishDirectory -Recurse -Force
}
