# CYW's Plugins — 工程注册表

> 本目录（`CYW.Plugins/`）对标 EXILED 仓库的 `EXILED/` 产品目录，容纳框架与全部插件。
> **构建**：`pwsh build.ps1` ｜ **打包**：`pwsh packaging.ps1`（产出**一个** `dist/CYW-Plugins.zip`，解压即部署）
> 状态：✅ 已存在并参与构建 ｜ 🚧 规划中（空目录，加入 `.csproj` 并登记到 `../CywApi.slnx` 后自动进入构建/打包）

## 工程清单

| 目录 | EXILED 参照 | 职责 | 状态 |
|---|---|---|---|
| `CYW.API/` | Exiled.API + Exiled.Loader | **框架本体**（当前：`0CywApi` 插件 + `CywApi.Core` 纯逻辑层） | ✅ |
| `CYW.Loader/` | Exiled.Loader | 插件加载/依赖解析（生态加载协议） | 🚧 |
| `CYW.Event/` | Exiled.Events | 事件系统（LabAPI 桥接 + 自有事件管线） | 🚧 |
| `CYW.CustomItems/` | Exiled.CustomItems | 自定义物品 | 🚧 |
| `CYW.CustomRoles/` | Exiled.CustomRoles | 自定义角色 | 🚧 |
| `CYW.Permissions/` | Exiled.Permissions | 权限节点/组体系 | 🚧 |
| `CYW.NameTag/` | Exiled.CreditTags | 名牌/头衔 | 🚧 |
| `CYW.Installer/` | Exiled.Installer | 安装/升级工具 | 🚧 |
| `CYW.LoadController/` | — | 加载顺序控制（待定义） | 🚧 |
| `CYW.EntityManager/` | — | 实体管理（待定义） | 🚧 |
| `CYW.MemoryManager/` | — | 内存管理（待定义） | 🚧 |
| `CYW.NetOptimizer/` | — | 网络优化（待定义） | 🚧 |
| `CYW.Printer/` | — | 输出/打印工具（待定义） | 🚧 |
| `CYW.Profiler/` | — | 性能剖析（待定义） | 🚧 |
| `CYW.Scheduler/` | — | 调度器（待定义） | 🚧 |

## 约定

1. **一个工程一个目录**：`CYW.X/` 内含其 `.csproj`（或 `src/` 子结构）；加入根 `CywApi.slnx` 后参与全量构建。
2. **共享构建属性**：全部由 [`Directory.Build.props`](Directory.Build.props) 提供（net48 / x64 / Release 警告即错误 / **统一输出到 `bin/<Configuration>/`**）。
3. **打包分类**：`packaging.ps1` 头部规则决定 DLL 去 `plugins/global` 还是 `dependencies/global`
   （`*.Core / *.Shared / *.Library / 0Harmony` → dependencies；其余 → plugins）。新增依赖 DLL 时更新规则表。
4. **需求追溯**：新行为先登记 `docs/lifecycle/requirements.md`（见根 `AGENTS.md`）。
