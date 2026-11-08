# CYW's API / CYW's Plugins

> SCP: Secret Laboratory 的 LabAPI 扩展框架与插件套件——为自有 Mod 与服务端修改提供底层开发体验层。
> 仓库结构**对标 EXILED**：单一产品目录 `CYW.Plugins/` 容纳框架与全部插件；**一次构建全部，一个 zip 解压即部署**。

*An extension framework for the LabAPI modding framework (SCP:SL), built as an independent implementation inspired by [LabExtended](https://github.com/marchellcx/LabExtended). No LabExtended code is included in this project.*

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

## 部署

把 `CYW-Plugins.zip` **解压到 LabAPI 目录**（`%APPDATA%\SCP Secret Laboratory\LabAPI\`，或含 `LabAPI` 的服务端根）——
zip 内部即 LabAPI 布局（`plugins/global/` + `dependencies/global/`），**一次解压即完成全部插件部署**。

本地开发循环（打包 + 解压到本机 LabAPI）：

```powershell
powershell -File "CYW.Plugins/scripts/dev/deploy.ps1"
```

## 状态

🚧 开发中——**Phase 1（核心层）已完成**：配置系统 / 日志 / Harmony 补丁器 / 模块系统 / 兼容性检查，
已在真实 SCP:SL 14.2.9 服务器上验证（加载、启用、禁用、损坏配置容错全部通过；单元测试全绿、Core 行覆盖 100%）。

- 计划与任务清单：[`CYW.Plugins/tasks/`](CYW.Plugins/tasks/)
- 本地测试服手册（重要坑位）：[`CYW.Plugins/docs/dev/test-server.md`](CYW.Plugins/docs/dev/test-server.md)
- 参考实现功能规格：[`CYW.Plugins/docs/reference/labextended-feature-spec.md`](CYW.Plugins/docs/reference/labextended-feature-spec.md)
- 生命周期与质量（DO-178C 方法对标）：[`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/) + [`CONSTRAINTS.md`](CONSTRAINTS.md)
- 工程注册表：[`CYW.Plugins/PLUGINS.md`](CYW.Plugins/PLUGINS.md)

## 环境要求

- .NET SDK 8+（建议 10）
- SCP:SL 专用服务器文件（提供 `SCPSL_Data\Managed` 构建引用）
- 环境变量 `SL_REFERENCES` 指向 `SCPSL_Data\Managed`

## 许可

[MIT](LICENSE)。独立实现，受 LabExtended（作者 marchellcx）启发并致谢。
