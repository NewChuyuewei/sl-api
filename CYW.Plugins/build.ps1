#! pwsh
# CYW's Plugins - build script (mirrors EXILED's build.ps1).
# Restores, builds (Release, warnings-as-errors) and tests the whole solution.
#
# Usage:
#   pwsh build.ps1            # restore + build + test
#   pwsh build.ps1 -NoTest    # restore + build only
#
# NOTE: keep this file ASCII-only - Windows PowerShell 5.1 reads non-BOM .ps1
# files as ANSI and non-ASCII characters can break parsing.
param(
    [switch]$NoTest
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$repo = Split-Path $root -Parent
$sln = Join-Path $repo 'CywApi.slnx'

Write-Host '==> Restoring' -ForegroundColor Cyan
& dotnet restore $sln
if ($LASTEXITCODE -ne 0) { throw "Restore failed ($LASTEXITCODE)." }

Write-Host '==> Building (Release)' -ForegroundColor Cyan
& dotnet build $sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed ($LASTEXITCODE)." }

if (-not $NoTest) {
    Write-Host '==> Testing' -ForegroundColor Cyan
    & dotnet test $sln -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw "Tests failed ($LASTEXITCODE)." }
}

Write-Host 'Build OK.' -ForegroundColor Green
