# 本地测试服手册（SCP:SL 专用服务器 + LabAPI）

> 2026-11-08 实测整理。路径示例来自本机安装；命令是经过验证的"正确姿势"。

## 1. 关键路径

| 用途 | 路径 |
|---|---|
| 服务端程序 | `E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server\`（Steam 安装） |
| 构建引用 | `…\SCPSL_Data\Managed\`（环境变量 `SL_REFERENCES` 指向这里） |
| LabAPI 数据目录 | `%APPDATA%\SCP Secret Laboratory\LabAPI\` |
| ├ 通用插件 | `LabAPI\plugins\global\` **← 框架/通用插件放这里** |
| ├ 端口专属插件 | `LabAPI\plugins\<port>\` |
| ├ 通用依赖 | `LabAPI\dependencies\global\`（如 0Harmony.dll、CywApi.Core.dll） |
| ├ 端口专属依赖 | `LabAPI\dependencies\<port>\` |
| ├ 加载器配置 | `LabAPI\LabApi-<port>.yml`（`plugin_paths` / `dependency_paths`） |
| └ 权限配置 | `LabAPI\configs\permissions.yml` |
| 服务器配置 | `%APPDATA%\SCP Secret Laboratory\config\<port>\`（首次启动自动生成） |

## 2. 启动（必须用 LocalAdmin）

```powershell
$srv = 'E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server'
$env:ACCEPT_SCPSL_EULA = 'TRUE'
Start-Process -FilePath "$srv\LocalAdmin.exe" `
  -ArgumentList '7777','--acceptEULA','--useDefault','--printStd' `
  -WorkingDirectory $srv
```

- `--acceptEULA`：首次运行的非交互式 EULA 接受（**必须**，否则 LocalAdmin 会在无 stdin 时无限刷 EULA 提示）
- `--useDefault`：无配置时用默认值（避免交互式配置向导）
- `--printStd`：把游戏 stdout/stderr 转发到 LocalAdmin 控制台输出

LocalAdmin 实际传给 `SCPSL.exe` 的参数（官方格式，供参考）：
```
-batchmode -nographics -txbuffer 200000 -rxbuffer 25000 -port7777 -console<端口> -id<LocalAdminPID> -heartbeat
```

## 3. 坑清单（全部实测踩过）

1. **不要绕过 LocalAdmin 直接用旧参数启动 `SCPSL.exe`**：实测 `-batchmode -nographics -port 7777`（空格）会导致端口解析失败（配置目录变成 `config\0`），且服务器在启动序列完成后**主动退出**、LabAPI 不初始化。官方支持的 Windows 启动路径就是 LocalAdmin。
2. **`-port` 必须连写**（`-port7777`），LocalAdmin 源码里就是连写格式。
3. **插件不要放 `plugins\` 根目录**：LabAPI 1.1.7 的 `LabApi-<port>.yml` 只扫描 `global` 和 `$port` 两个子目录。放错位置**不会报错也不会加载**。
4. **服务器不自带 Harmony/Newtonsoft**：`SCPSL_Data\Managed` 里没有 `0Harmony.dll`。使用 Harmony 的插件必须把 `0Harmony.dll` 放到 `dependencies\global\`。
5. 首次启动会生成服务器配置（`config\<port>\`）；日志文件位置分散（LocalAdmin 输出 / `%LOCALAPPDATA%Low\Northwood\SCPSL\`），以 LocalAdmin 控制台输出为主要诊断源。

## 4. 开发循环

```
1. dotnet build -c Release
2. scripts\dev\deploy.ps1          # 部署到 LabAPI\plugins\global + dependencies\global
3. 启动 LocalAdmin（上文命令）
4. 冒烟检查：
   - 日志出现 "[LOADER] Enabling all plugins"
   - 日志出现 "CYW's API v0.1.0 enabled"
   - 无 Exception/ERROR
   - （可选）进服完整回合 → 重启 → 无残留
```
