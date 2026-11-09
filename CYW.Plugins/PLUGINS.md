# CYW's Plugins — 工程注册表

> 本目录（`CYW.Plugins/`）对标 EXILED 仓库的 `EXILED/` 产品目录，容纳框架与全部插件。
> **构建**：`pwsh build.ps1` ｜ **打包**：`pwsh packaging.ps1` → `dist/release/`（真相源，CI 上传即单层 artifact）+ `dist/CYW-Plugins.zip`（分发副本，解压即部署）
> **定位（v1.1）**：轻量级综合 API——功能级**替代 EXILED**（能力矩阵与退役清单见 `tasks/plan.md` §1.5）；**二进制兼容**（运行他人的 EXILED 插件）不是目标。
> **轻量级约束**：运行时程序集 ≤4（当前 2）；外部依赖仅 `0Harmony`；补丁全可枚举；单层 zip 部署；启用耗时 <1s。
> 状态：✅ 已存在并参与构建 ｜ 🚧 规划中（空目录，加入 `.csproj` 并登记到 `../CywApi.slnx` 后自动进入构建/打包）
> **阶段**：计划里程碑（M1–M9，见 `tasks/plan.md`）。M7 = EXILED 对标子系统 ｜ M8 = 自研增强工具 ｜ M9 = 退役验收。

## 工程清单

| 目录 | EXILED 参照 | 职责 | 阶段 | 状态 |
|---|---|---|---|---|
| `CYW.API/` | Exiled.API + Exiled.Loader | **框架本体**（`0CywApi` 插件 + `CywApi.Core` 纯逻辑层；含命令框架与呈现层） | M1–M6 | ✅ M1 完成；M2 进行中 |
| `CYW.Event/` | Exiled.Events | 事件系统（LabAPI 桥接 + 自有事件管线） | M2 | 🚧 |
| `CYW.CustomItems/` | Exiled.CustomItems | 自定义物品 | M4a | 🚧 |
| `CYW.CustomRoles/` | Exiled.CustomRoles | 自定义角色 | M4b | 🚧 |
| `CYW.Permissions/` | Exiled.Permissions | 权限节点/组/继承（配置驱动） | M7a | 🚧 |
| `CYW.NameTag/` | Exiled.CreditTags | 名牌/头衔 | M7b | 🚧 |
| `CYW.Loader/` | Exiled.Loader | 插件加载/依赖解析（生态加载协议，互操作 API） | M7c | 🚧 |
| `CYW.Installer/` | Exiled.Installer | 安装/升级工具 | M7d | 🚧 |
| `CYW.Profiler/` | — | 性能剖析（模块/补丁/事件耗时） | M8 | 🚧 |
| `CYW.Scheduler/` | — | 调度器（任务队列 / tick 分片） | M8 | 🚧 |
| `CYW.NetOptimizer/` | — | 网络优化 | M8 | 🚧 |
| `CYW.MemoryManager/` | — | 内存管理 | M8 | 🚧 |
| `CYW.EntityManager/` | — | 实体管理 | M8 | 🚧 |
| `CYW.Printer/` | — | 输出/打印工具 | M8 | 🚧 |
| `CYW.LoadController/` | — | 加载顺序控制 | M8 | 🚧 |

## 约定

1. **一个工程一个目录**：`CYW.X/` 内含其 `.csproj`（或 `src/` 子结构）；加入根 `CywApi.slnx` 后参与全量构建。
2. **程序集拆分规则（Promotion Rule）**：目录 = 代码组织单元，DLL = 发布单元；仅当「需独立版本化的互操作 API / 单程序集 >150 文件 / 可选重量级」之一成立才从 `0CywApi` 拆出独立程序集（轻量级约束：运行时程序集 ≤4，见 `tasks/plan.md` §1.5）。
3. **共享构建属性**：全部由 [`Directory.Build.props`](Directory.Build.props) 提供（net48 / x64 / **C# 14** / Release 警告即错误 / **统一输出到 `bin/<Configuration>/`**；.NET 10 SDK 由根级 `global.json` 钉住）。
4. **打包分类**：`packaging.ps1` 头部规则决定 DLL 去 `plugins/global` 还是 `dependencies/global`
   （`*.Core / *.Shared / *.Library / 0Harmony` → dependencies；其余 → plugins）。新增依赖 DLL 时更新规则表。
5. **需求追溯**：新行为先登记 `docs/lifecycle/requirements.md`（见根 `AGENTS.md`）。
