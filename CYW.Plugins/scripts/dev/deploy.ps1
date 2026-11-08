# CYW's Plugins - deploy to the LOCAL LabAPI folder (dev loop).
# Mirrors the release flow: package once (one release folder), copy once.
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
$release = Join-Path $root 'dist\release'
$lab = Join-Path $env:APPDATA 'SCP Secret Laboratory\LabAPI'

if ($SkipBuild) {
    & $packaging -SkipBuild
} else {
    & $packaging
}

if (-not (Test-Path $release)) { throw "Release folder not found: $release" }

New-Item -ItemType Directory -Force -Path $lab | Out-Null
Copy-Item (Join-Path $release '*') $lab -Recurse -Force

Write-Output "Deployed to: $lab"
Get-ChildItem (Join-Path $lab 'plugins\global'), (Join-Path $lab 'dependencies\global') -File |
    Select-Object -ExpandProperty Name
