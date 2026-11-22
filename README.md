# CYW's API / CYW's Plugins

> SCP: Secret Laboratory 的 LabAPI 扩展框架与插件套件——为自有 Mod 与服务端修改提供底层开发体验层。
> 仓库结构**对标 EXILED**：单一产品目录 `CYW.Plugins/` 容纳框架与全部插件；**一次构建全部，一个 zip 解压即部署**。

*An extension framework for the LabAPI modding framework (SCP:SL), built as an independent implementation inspired by [LabExtended](https://github.com/marchellcx/LabExtended). No LabExtended code is included in this project.*

## 服务器所有者快速开始

**运行基线**：CYW's API / Core **0.1.0**，需要 **LabAPI 1.1.7** 与 **SCP:SL 14.2.9** 专用服务器。本插件不是 EXILED 插件，不能在仅安装 EXILED 的服务器上运行。

**Gameplay impact / transparency（仅当前 0.1.0 构建）**：当前启用 Compatibility 与 Patching 模块，提供游戏版本检查/告警、向最终 `buildinfo` / Mod-Description 添加框架版本行，以及框架日志和补丁管理。当前**未随附或应用任何补丁类**（实机汇总为 `+0 applied, 0 failed, 0 skipped`），不新增命令或权限节点、自定义角色/物品，不收集玩家数据，也不包含其他影响玩法的行为。因此 `CywPlugin.IsTransparent` 按 CMP-004 的用户裁决暂为 `true`；该声明只描述上述当前构建，不是对未来版本的透明性承诺。任何 M2 或其他影响玩法/侵入性功能落地前，必须重新评审此声明，并按门禁预期切换为 `false`。

### 安装

> **干净服务器安装验收：verified-on-clean-server（Windows，QA-A2 / VR-018）。** 已在备份并清空目标目录后，仅使用 `[System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $labRoot)` 将发布 zip 解压到 LabAPI 根目录并完成启动验证；未使用 `deploy.ps1`，也未从 `bin` 复制文件。该结果属于最终候选 QA 证据，不代表发布或 tag 批准。

1. 将 `CYW-Plugins.zip` 解压到 LabAPI 目录（`%APPDATA%\SCP Secret Laboratory\LabAPI\`，或含 `LabAPI` 的服务端根）。zip 内部即 LabAPI 布局，无需再嵌套一层目录。
2. 确认以下文件就位；第三方组件及版本见[依赖与第三方](#依赖与第三方)：
   - `plugins/global/0CywApi.dll`
   - `dependencies/global/CywApi.Core.dll`
   - `dependencies/global/0Harmony.dll`
3. 启动服务器。配置文件位于 `configs/<port>/CywApi/config.yml`（例如端口 7777 为 `configs/7777/CywApi/config.yml`），与插件的 `properties.yml` 同目录。
4. 需要停用时，在同目录 `properties.yml` 设置 `is_enabled: false` 并重启服务器；恢复为 `true` 后再次重启即可重新启用。LabAPI 1.1.7 不提供本插件的热重载周期。

本地开发循环（打包 + 解压到本机 LabAPI）：

```powershell
powershell -File "CYW.Plugins/scripts/dev/deploy.ps1"
```

> Windows `Compress-Archive` 生成的 zip 可能以 `\` 存储条目分隔符；非 Windows 系统的直接解压当前 **UNVERIFIED**。

### 配置

LabAPI 1.1.7 生成的 `config.yml` **不含键说明注释**，因此下表是服务器所有者的配置键参考。默认 YAML 形态为：

```yaml
debug: false
log:
  use_colors: true
  prepend_source: false
patches:
  disabled: []
compatibility:
  check_game_version: true
```

| YAML 键 | 类型 | 默认值 | 作用 | 已确认的空值/损坏配置行为 |
|---|---|---|---|---|
| `debug` | `bool` | `false` | `true` 时输出框架 DEBUG 日志；`false` 时仅抑制 DEBUG，其他级别仍输出 | 整个 YAML 损坏时回退为 `false` |
| `log.use_colors` | `bool` | `true` | 将框架日志中的 `&` 风格颜色码渲染为 ANSI 颜色 | `log:` 显式为 null 时，整个分区回退默认值，即 `true` |
| `log.prepend_source` | `bool` | `false` | 在框架日志行前添加调用程序集名（框架自身程序集除外） | `log:` 显式为 null 时，整个分区回退默认值，即 `false` |
| `patches.disabled` | `List<string>` | `[]` | 跳过指定 `[CywPatch]` 类；支持类全名/短名精确匹配，以及末尾 `.*` 的命名空间通配（如 `CywApi.Patches.*`）。当前构建无随附补丁类，因此默认没有补丁被跳过 | `patches:` 或 `disabled:` 显式为 null 时回退为空列表 `[]` |
| `compatibility.check_game_version` | `bool` | `true` | 游戏版本低于支持范围时记录警告但不阻断启用；设为 `false` 可跳过该检查，不影响 BuildInfo 注入 | `compatibility:` 显式为 null 时，整个分区回退默认值，即 `true` |

非法 YAML 会触发 LabAPI 加载器警告，随后使用全部默认值，框架仍照常启用。上述空值处理已验证；其他标量类型错误的逐项转换行为 **UNVERIFIED**，不要依赖其被自动纠正。

## 仓库结构

```text
.
├── .github/workflows/build.yml        # CI：构建全部 + 测试 + 打包 + artifact
├── CywApi.slnx                        # 解决方案（含全部项目）
├── AGENTS.md / CONSTRAINTS.md         # 协作须知 / 质量护栏
└── CYW.Plugins/                     # 产品目录（对标 EXILED 仓库的 EXILED/ 目录）
    ├── Directory.Build.props          # 共享构建属性（对标 EXILED.props）
    ├── build.ps1                      # 构建脚本（还原 + Release 构建 + 单测）
    ├── packaging.ps1                  # 打包脚本 → dist/CYW-Plugins.zip
    ├── PLUGINS.md                     # 工程注册表（各插件/库的职责与状态）
    ├── CYW.API/                       # 框架本体（当前：CywApi 插件 + CywApi.Core）
    ├── CYW.*/                         # 规划中的插件与库（见 PLUGINS.md）
    ├── docs/ tasks/ scripts/          # 文档 / 计划 / 脚本
    └── tests/                         # 单元测试（离服运行）
```

## 构建与打包

```powershell
pwsh "CYW.Plugins/build.ps1"          # 还原 + Release 构建 + 全部单测
pwsh "CYW.Plugins/packaging.ps1"      # 构建 + 打包 → CYW.Plugins/dist/CYW-Plugins.zip
```

> 未安装 `pwsh`（PowerShell 7）时，以上命令的等价形式（Windows PowerShell 5.1）为
> `powershell -NoProfile -ExecutionPolicy Bypass -File "<script>"`——例如
> `powershell -NoProfile -ExecutionPolicy Bypass -File "CYW.Plugins/build.ps1"`。

## 依赖与第三方

发布包（`dist/CYW-Plugins.zip`，与 `dist/release/` 同内容的**单层** zip）**只包含插件本体、第一方 Core 与 0Harmony**——
不含任何游戏 / 框架 DLL（`LabApi.dll`、`Assembly-CSharp*.dll` 等由服务端自身提供，绝不随包分发）：

| zip 内路径 | 组件 | 版本 | 为什么需要 |
|---|---|---|---|
| `plugins/global/0CywApi.dll` | 框架插件本体（`CYW's API`） | 0.1.0 | LabAPI 插件入口 |
| `dependencies/global/CywApi.Core.dll` | 第一方纯逻辑层 | 0.1.0 | 日志净化 / 补丁过滤 / ANSI 颜色 / 版本兼容等纯逻辑——离服可单测；不引用 LabAPI / 游戏程序集 |
| `dependencies/global/0Harmony.dll` | Lib.Harmony | **2.2.2** | Harmony 补丁运行时——**SCP:SL 服务端不自带 Harmony，必须随包提供**；版本与实机加载日志一致 |

构建 / 测试专用依赖（**从不进入发布包**）：`BepInEx.AssemblyPublicizer.MSBuild` 0.4.3、
`Microsoft.NETFramework.ReferenceAssemblies` 1.0.3、`xunit` 2.9.2、`xunit.runner.visualstudio` 2.8.2、
`Microsoft.NET.Test.Sdk` 17.11.1、`coverlet.collector` 6.0.4。

## 状态

🚧 开发中——**Phase 1（核心层）已完成**：配置系统 / 日志 / Harmony 补丁器 / 模块系统 / 兼容性检查。
最终候选（HEAD `8522c39bae624b3b413740f4a6e7afe7c9a7249a`；`0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`）的 QA-A + QA-A2 合并结论为 PASS，QA-B 为 PASS，证据归档为 VR-017～VR-019：构建 0 警告/0 错误、单元测试 45/45、Core 行覆盖 113/113（100%）与分支覆盖 90/92（97.83%）；S1/S3 生命周期、Windows 仅 zip 干净安装、真实第三方 ServerQoL 共存、LOG-005 与回合重启均通过，零非预期插件异常。这些是最终候选 QA 证据，**不代表发布、tag 或发布批准**。

验证范围仍有明确排除：多玩家与玩家进出、性能、ServerQoL 玩法规则、非 Windows 解压及交互式桌面渲染均为 **UNVERIFIED**；实机验证使用 headless LocalAdmin、0 玩家且无可见窗口。历史实机与单元测试证据仍见 VR-011～VR-014。

- 计划与任务清单：[`CYW.Plugins/tasks/`](CYW.Plugins/tasks/)
- 本地测试服手册（重要坑位）：[`CYW.Plugins/docs/dev/test-server.md`](CYW.Plugins/docs/dev/test-server.md)
- 参考实现功能规格：[`CYW.Plugins/docs/reference/labextended-feature-spec.md`](CYW.Plugins/docs/reference/labextended-feature-spec.md)
- 生命周期与质量（DO-178C 方法对标）：[`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/) + [`CONSTRAINTS.md`](CONSTRAINTS.md)
- 工程注册表：[`CYW.Plugins/PLUGINS.md`](CYW.Plugins/PLUGINS.md)

## 环境要求

- .NET SDK **10**（C# 14；由根级 `global.json` 钉住 10.0 特性带）
- SCP:SL 专用服务器文件（提供 `SCPSL_Data\Managed` 构建引用）
- 环境变量 `SL_REFERENCES` 指向 `SCPSL_Data\Managed`

## 许可

[MIT](LICENSE)。独立实现，受 LabExtended（作者 marchellcx）启发并致谢。
