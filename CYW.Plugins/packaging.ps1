#! pwsh
# CYW's Plugins - packaging script (mirrors EXILED's packaging.ps1 idea).
# Builds the whole solution and packages ALL plugins into ONE ready-to-extract zip
# that mirrors the LabAPI folder layout:
#
#   plugins/global/        plugin assemblies
#   dependencies/global/   shared dependency assemblies (0Harmony, *.Core, ...)
#   README-INSTALL.txt     install instructions
#
# Usage:
#   pwsh packaging.ps1               # build + package
#   pwsh packaging.ps1 -SkipBuild    # package only (artifacts already built, e.g. CI)
#   pwsh packaging.ps1 -Configuration Debug
#
# Output: dist/CYW-Plugins.zip
#
# NOTE: keep this file ASCII-only - Windows PowerShell 5.1 reads non-BOM .ps1
# files as ANSI and non-ASCII characters can break parsing.
param(
    [switch]$SkipBuild,
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$repo = Split-Path $root -Parent
$sln = Join-Path $repo 'CywApi.slnx'
$bin = Join-Path $root "bin\$Configuration"
$dist = Join-Path $root 'dist'
$stage = Join-Path $dist 'stage'
$zip = Join-Path $dist 'CYW-Plugins.zip'

# --- classification rules (edit here when new projects are added) ---------------
# Files matching these patterns are treated as SHARED DEPENDENCIES and packaged
# into dependencies/global; every other assembly goes into plugins/global.
$DependencyPatterns = @(
    '0Harmony.dll',
    '*.Core.dll',
    '*.Shared.dll',
    '*.Library.dll',
    'Newtonsoft.Json.dll',
    'NVorbis.dll'
)

# Never packaged: build tooling / test infrastructure that may leak into bin.
$ExcludePatterns = @(
    'AsmResolver*.dll',
    'BepInEx*.dll',
    '*.Tests.dll',
    'testhost*.dll',
    'xunit*.dll',
    'coverlet*.dll',
    'Mono.Cecil*.dll',
    'Microsoft.*.dll',
    'System.*.dll'
)
# --- end of rules ---------------------------------------------------------------

if (-not $SkipBuild) {
    Write-Host "==> Building solution ($Configuration)" -ForegroundColor Cyan
    & dotnet build $sln -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Build failed ($LASTEXITCODE)." }
}

if (-not (Test-Path $bin)) {
    throw "Build output folder not found: $bin (run without -SkipBuild first)."
}

if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
$pluginsDir = Join-Path $stage 'plugins\global'
$depsDir = Join-Path $stage 'dependencies\global'
New-Item -ItemType Directory -Force -Path $pluginsDir, $depsDir | Out-Null

$pluginCount = 0
$depCount = 0
foreach ($file in Get-ChildItem $bin -File -Filter '*.dll') {
    $name = $file.Name

    $excluded = $false
    foreach ($p in $ExcludePatterns) { if ($name -like $p) { $excluded = $true; break } }
    if ($excluded) { continue }

    $isDependency = $false
    foreach ($p in $DependencyPatterns) { if ($name -like $p) { $isDependency = $true; break } }

    if ($isDependency) {
        Copy-Item $file.FullName $depsDir -Force
        $depCount++
    } else {
        Copy-Item $file.FullName $pluginsDir -Force
        $pluginCount++
    }
}

$installReadme = @'
CYW's Plugins - release package
===============================

Extract this zip INTO the LabAPI folder of your SCP:SL dedicated server:

    %APPDATA%\SCP Secret Laboratory\LabAPI\        (Windows)
    ~/.config/SCP Secret Laboratory/LabAPI/        (Linux)

The archive mirrors the LabAPI layout:

    plugins/global/        plugin assemblies
    dependencies/global/   shared dependency assemblies (0Harmony, *.Core, ...)

After extraction, restart the server.
'@
[IO.File]::WriteAllText((Join-Path $stage 'README-INSTALL.txt'), $installReadme, [Text.UTF8Encoding]::new($false))

Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
Remove-Item $stage -Recurse -Force

Write-Host "==> Package ready: $zip" -ForegroundColor Green
Write-Host "    plugins: $pluginCount, dependencies: $depCount"

Add-Type -AssemblyName System.IO.Compression.FileSystem
$z = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    $z.Entries | ForEach-Object { Write-Host ("    " + $_.FullName) }
} finally {
    $z.Dispose()
}
