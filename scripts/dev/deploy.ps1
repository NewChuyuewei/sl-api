# 将 CYW's API 构建产物部署到本地 LabAPI 插件目录（Windows）。
# 用法：powershell -File scripts\dev\deploy.ps1 [-Configuration Release]
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
    throw "未找到构建产物：$bin\0CywApi.dll（先运行 dotnet build -c $Configuration）"
}

New-Item -ItemType Directory -Force -Path $pluginsDir, $depsDir | Out-Null

Copy-Item (Join-Path $bin '0CywApi.dll') $pluginsDir -Force
Copy-Item (Join-Path $bin 'CywApi.Core.dll') $depsDir -Force
Copy-Item (Join-Path $bin '0Harmony.dll') $depsDir -Force

Write-Output "已部署到 $lab :"
Get-ChildItem $pluginsDir, $depsDir -File | Select-Object -ExpandProperty FullName
