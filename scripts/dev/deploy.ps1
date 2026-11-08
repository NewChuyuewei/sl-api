# Deploys the CYW's API build output into the local LabAPI plugin folders (Windows).
# Usage: powershell -File scripts\dev\deploy.ps1 [-Configuration Release]
# NOTE: keep this file ASCII-only — Windows PowerShell 5.1 reads non-BOM .ps1 files as ANSI.
param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$bin = Join-Path $repoRoot "src\CywApi\bin\$Configuration\net48"
$lab = Join-Path $env:APPDATA 'SCP Secret Laboratory\LabAPI'
$pluginsDir = Join-Path $lab 'plugins\global'
$depsDir = Join-Path $lab 'dependencies\global'

if (-not (Test-Path (Join-Path $bin '0CywApi.dll'))) {
    throw "Build output not found: $bin\0CywApi.dll (run 'dotnet build -c $Configuration' first)"
}

New-Item -ItemType Directory -Force -Path $pluginsDir, $depsDir | Out-Null

Copy-Item (Join-Path $bin '0CywApi.dll') $pluginsDir -Force
Copy-Item (Join-Path $bin 'CywApi.Core.dll') $depsDir -Force
Copy-Item (Join-Path $bin '0Harmony.dll') $depsDir -Force

Write-Output "Deployed to: $lab"
Get-ChildItem $pluginsDir, $depsDir -File | Select-Object -ExpandProperty Name
