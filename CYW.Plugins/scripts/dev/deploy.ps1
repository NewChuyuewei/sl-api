# CYW's Plugins - deploy to the LOCAL LabAPI folder (dev loop).
# Mirrors the release flow: package once (one zip), extract once.
# Requirements: CYWS-BLD-003.
#
# Usage:
#   powershell -File scripts\dev\deploy.ps1              # package + extract
#   powershell -File scripts\dev\deploy.ps1 -SkipBuild   # extract the existing package
#
# NOTE: keep this file ASCII-only - Windows PowerShell 5.1 reads non-BOM .ps1
# files as ANSI and non-ASCII characters can break parsing.
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent   # CYW.Plugins
$packaging = Join-Path $root 'packaging.ps1'
$zip = Join-Path $root 'dist\CYW-Plugins.zip'
$lab = Join-Path $env:APPDATA 'SCP Secret Laboratory\LabAPI'

if ($SkipBuild) {
    & $packaging -SkipBuild
} else {
    & $packaging
}

if (-not (Test-Path $zip)) { throw "Package not found: $zip" }

New-Item -ItemType Directory -Force -Path $lab | Out-Null
Expand-Archive -Path $zip -DestinationPath $lab -Force

Write-Output "Deployed to: $lab"
Get-ChildItem (Join-Path $lab 'plugins\global'), (Join-Path $lab 'dependencies\global') -File |
    Select-Object -ExpandProperty Name
