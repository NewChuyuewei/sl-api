# 实施计划：CYW's API — SCP:SL 插件框架

> **状态**：已评审并确认（2026-11-07）｜ **v1.1 修订（2026-11-08）**：定位升级为「轻量级综合 API——对标并**替代 EXILED**」（新增 §1.5；里程碑增补 M7–M9；§5.2/§5.3/§6/§8 同步增补）
> **任务清单**：`tasks/todo.md`（Phase 0–2 已细化；Phase 3+ 为滚动细化）
> **制定方法**：context7 MCP（LabAPI 官方文档）+ SCP:SL 技能套件（scpsl-plugin-dev / scpsl-labapi / scpsl-migration-release / planning-and-task-breakdown）+ 对 `example/LabExtended-master` 的只读全量盘点（探索代理）+ GitHub 公开信息核查

---

## 1. 背景与目标

### 目标与定位
开发自有 SCP:SL 插件框架 **CYW's API**——一个基于 **LabAPI 1.1.7** 的库插件（`0CywApi.dll`，最先加载）。

**定位（2026-11-07 确认）**：CYW's API 是**自有 Mod 与后续服务端修改的底层框架**，角色对标 LabAPI / EXILED 所处的"开发者体验层"——为构建其上的插件提供高质量扩展能力（玩家包装、事件系统、命令框架、自定义物品/角色/效果/能力、提示叠层、Settings API、Remote Admin API 等），并作为后续深度服务端修改的底座。API 面向"框架使用者"设计，开发体验与稳定性优先。

**替代 EXILED（2026-11-08 新增目标）**：CYW's API 的完备形态（1.0）要达到"**可以用它替换 EXILED**"的程度——自有 Mod 套件（`CYW.*` 插件集）在**纯 LabAPI 服务器**上即可完整运行，不再需要安装 EXILED。原则是**轻量级综合 API**：功能面覆盖实际使用的 EXILED 能力（旗舰为 CustomItems / CustomRoles / Permissions / Events / 命令），但用更少的程序集、更小的依赖面、可枚举的补丁面实现。详见 §1.5。

### 融合方式：参考重写（已确认）
以 `example/LabExtended-master`（v1.4.0.2，**517 个 .cs 文件 / 58,726 行**）及其公开 Wiki 作为**功能规格与行为参考**，从零实现 CYW's API：

- ✅ 允许：阅读参考代码理解行为、参考公开 Wiki、参考游戏/LabAPI 官方文档
- ❌ 禁止：复制任何代码文本、注释、文档文字；不搬运其 `refs.rar` 等第三方构建产物
- 📌 发布时必须：README 声明"独立实现，受 LabExtended 启发"并致谢 marchellcx

### 成功标准（v0.1 发布定义）
- P0 + P1 子系统可用（见 §5.2 优先级表），能支撑开发实际插件
- 干净服务器验证：enable → 完整回合 → 重载 → 禁用 无残留、无重复订阅
- 有 README / 文档 / 示例插件 / CI 绿 / Release 产物只含插件 DLL
- 公共发布合规：自有 LICENSE + 致谢声明 + 无版权风险的代码

### 非目标（本期不做）
- 替代 LabAPI（服务器内置、不可移除；CYW's API 构建在其之上——EXILED 9.x 自身同样引用 LabAPI 内核）
- **EXILED 二进制兼容**（跑他人的 EXILED 插件）与双框架适配——**功能级替代是目标**（见 §1.5），接口级兼容不是
- LabExtended 二进制兼容（第三方 LabExtended 插件无需能运行在我们框架上）
- 100% 功能等价（按优先级裁剪，如语音/音频可延后或砍掉）

---

## 1.5 EXILED 替代策略与能力矩阵（v1.1 新增）

### 替代语义
- **目标**：自有 Mod 套件（`CYW.*` 插件集）＋ CYW's API 在**纯 LabAPI 服务器**上完整运行——卸载 EXILED 后服务器功能不受影响（对照下方矩阵逐项签收）。
- **非目标**：直接运行他人的 EXILED 插件（二进制兼容，不做）；照搬 EXILED 的 API 形状（自由设计，见 D2 参考重写纪律）。

### 「轻量级」量化约束（v1.0 目标）

| 维度 | 约束 | 现状（2026-11-08） |
|---|---|---|
| 运行时程序集 | ≤ **4** 个（EXILED 全套 ≈10+） | **2 个**（`0CywApi` + `CywApi.Core`） |
| 外部依赖 | `0Harmony` 必需；其余第三方按需且默认不部署 | `0Harmony` 1 个 |
| 补丁面 | 全部可枚举（`[CywPatch]` 注册表）+ 可按配置禁用；禁止隐式补丁 | ✅ 已具备（`CywPatcher`） |
| 部署面 | 一个 zip、一次解压、重启即用 | ✅ 单层 release 包 |
| 加载开销 | 启用耗时 < 1s（服务器日志时间戳为准）；无后台线程 | 实测 ≈0.9s（VR-003 时间窗） |

### 程序集拆分规则（Promotion Rule，轻量级优先）
`CYW.*` 目录 = **代码组织单元**；程序集（DLL）= **发布单元**。仅当以下之一成立才从 `0CywApi` 拆出独立程序集：
1. **互操作 API 需独立版本化**（第三方插件编译依赖——主要是 `CYW.Loader` 生态协议）；
2. 单程序集文件数 > 150 或编译明显劣化；
3. 子系统为**可选重量级**（缺失不影响核心——如未来语音）。
否则保持合并。

### 能力矩阵（EXILED → CYW 对标）

| EXILED 组件 | 能力 | CYW 对标 | 阶段 | 状态 |
|---|---|---|---|---|
| Exiled.API | 插件基类/配置/日志/工具 | `0CywApi`（`CywPlugin`/`CywConfig`/`CywLog`） | M1 | ✅ 已具备 |
| Exiled.Events | 事件体系（桥接 + 自定义） | `CywEvents.*` + 事件桥 | M2 | 🚧 Phase 2 |
| （内建于 EXILED） | 属性化命令框架 | 命令框架（Phase 3） | M3 | 🚧 Phase 3 |
| Exiled.CustomItems | 自定义物品/枪械/投掷物 | M4a 内容（`CYW.CustomItems`） | M4a | 🚧 Phase 4a |
| Exiled.CustomRoles | 自定义角色/队伍/分波 | M4b 内容（`CYW.CustomRoles`） | M4b | 🚧 Phase 4b |
| Exiled.Permissions | 权限节点/组/继承/配置驱动 | `CYW.Permissions` | **M7a** | 🚧 |
| Exiled.CreditTags | 名牌/头衔 | `CYW.NameTag` | **M7b** | 🚧 |
| Exiled.Loader | 加载器与生态协议（第三方依赖） | `CYW.Loader`（互操作 API 程序集） | **M7c** | 🚧 |
| Exiled.Installer | 安装器工具 | `CYW.Installer` | **M7d** | 🚧 |
| （无对标，自研） | Profiler / Scheduler / NetOptimizer / MemoryManager / EntityManager / Printer / LoadController | 各自目录（按需取舍） | M8 | 🚧 |

### EXILED 退役验收清单（Replacement Definition of Done）

- [ ] 矩阵"必须项"（API / Events / 命令 / CustomItems / CustomRoles / Permissions / NameTag）全部通过实机演示（VR 记录归档）
- [ ] 自有 Mod 套件在纯 LabAPI 服务器完成 ≥1 场完整对局，无 CYW's API 引发的错误
- [ ] 卸载 EXILED 后服务器：启动 / 回合 / 重载 / 禁用循环全部干净
- [ ] 配置/权限迁移对照表（EXILED 设置项 → CYW 设置项）齐备
- [ ] 性能基线：与装 EXILED 时对比（TPS / 内存）不劣化 > 5%

---

## 2. 已完成的调研结论

| 项 | 结论 |
|---|---|
| 环境 | .NET SDK **10.0.302** ✓；Python 3.14.6 ✓；**无** SCP:SL 服务端文件；磁盘：D: ≈67GB / E: ≈67GB 可用，Z: 仅 1.2GB（服务端不装 Z:） |
| Steam | Steam 客户端位于 `E:\Steam`（已有 SCP:SL 游戏本体，但**无**专用服务器） |
| 网络（2026-11-08 实测） | 直连 Valve CDN 超时；本机代理（127.0.0.1:10808）≈15KB/s 不可用；**NuGet 已切华为镜像（9MB/s）**；服务端文件走 Steam 客户端通道（专用服务器已安装，LabApi.dll 1.1.7.0 ✓；服务器**不自带** 0Harmony/Newtonsoft，须随插件部署） |
| MCP | context7 已查证 LabAPI：库插件用 `LoadPriority` 提前加载；`CustomHandlersManager.RegisterEventsHandler / UnregisterEventsHandler` 成对注册/注销 |
| 参考盘点 | LabExtended 全量盘点完成：92 个补丁文件 / 122 处 `[HarmonyPatch]`；子系统与耦合度已分类（见 §5.2） |
| 法律 | LabExtended 仓库**无 LICENSE 文件**（默认保留所有权利）→ 已选择参考重写路线规避；发布走"独立实现+致谢" |
| 版本锚定 | SCP:SL **14.2.x**（参考实现要求 ≥14.2.8）；LabAPI **1.1.7**；net48；Harmony 版本以**服务器实际自带版本**为准（Phase 0 实测对齐） |
| 构建参考策略 | 不使用 LabExtended 的 `refs.rar`（授权+供应链风险）。自建：steamcmd 下载官方服务端 → `$(SL_REFERENCES)` 指向 `SCPSL_Data\Managed` → 引用**原始 DLL** + `BepInEx.AssemblyPublicizer.MSBuild` 构建期 publicize |

---

## 3. 架构决策（已确认）

| # | 决策 | 理由 |
|---|---|---|
| D1 | **LabAPI 库插件形态**：net48、`AssemblyName=0CywApi`、`LoadPriority.Highest`、`IsTransparent=true` | 0 前缀+HIGHEST 保证最先加载（生态惯例，参考 README 强调"不许改名"的约束）；LabAPI 为官方框架、随服务器分发 |
| D2 | **参考重写纪律**（见 §1） | 规避无许可证代码的版权风险；保证可公开发布 |
| D3 | **命名**：root namespace `CywApi.*`；程序集 `0CywApi`；插件 `Name="CywApi"`（品牌名 CYW's API 用于 Description/日志，避免带撇号的配置目录名）；起始版本 `0.1.0` | 配置目录为 `configs/<port>/<Name>/`，撇号/空格会带来路径与工具链摩擦 |
| D4 | **玩家策略**：`CywPlayer : Player`（LabAPI Player 子类）+ 构造/移除替换补丁（等效 ExPlayer） | 参照对象的核心卖点：全生态拿到的都是增强 Player。风险隔离：单独补丁类、空值防护、文档声明；备选方案（并行注册表）已记录——侵入性低但插件无法自然获得 CywPlayer |
| D5 | **模块系统**：`ICywModule`（Name/Priority/OnEnable/OnDisable）+ 显式注册表，替代参考实现的硬编码 30+ 步初始化序列 | 可维护性改进：增删子系统不改加载器；单一模块失败不拖垮框架（记日志继续） |
| D6 | **补丁管理**：自有 `CywPatcher`——`[CywPatch]` 标记扫描、配置化禁用（`类名.方法名` / `类名.*`）、启停对称、运行时补丁报告；提供 API 供其他程序集注册补丁（框架协议） | 参考实现反射式扫描思路；改进：更清晰的标记与报告；Harmony 版本对齐服务器实际版本 |
| D7 | **事件系统**：用官方推荐的 `CustomEventsHandler` 桥接 LabAPI；自有 `CywEvents.*` 静态事件 + `EventArgs`（可取消=`BooleanEventArgs`）+ `InvokeSafe` | 框架最先加载 ⇒ 最先注册 ⇒ Bridge 最先执行；避免参考实现的反射 InsertFirst 黑魔法 |
| D8 | **命令框架**：基于 `CommandSystem` 的属性化框架——`[CywCommand]` 自动注册、类型化参数/限制器/重载/Runner/嵌套组、RA/客户端/服务器控制台三面；提供 `RegisterAssembly` 供依赖插件注册自己的命令 | 参考盘点确认这是最自包含、最先该做的子系统（120 文件/9.7k 行）；API 设计可自由改进 |
| D9 | **解决方案布局**：`CywApi`（插件，含游戏引用）+ `CywApi.Core`（纯逻辑，**无任何游戏/LabAPI/Unity 引用**，可单测）+ `CywApi.Tests`（net48，xUnit） | 遵循技能规范：Core 是唯一可离服单测的层；命令解析、版本范围、限制器等纯逻辑下沉 |
| D10 | **配置**：单一 `config.yml`（`CywConfig`，分区：Debug/补丁/命令/提示/存储…）；损坏配置回退默认值并告警，绝不崩溃 | 比参考实现的双文件更简洁；坏配置不崩是硬性要求 |
| D11 | **构建引用**：`$(SL_REFERENCES)`（用户环境变量）→ 服务端 `SCPSL_Data\Managed`；csproj 引用原始 DLL + `Publicize="true"`；net48 引用程序集缺失时补 `Microsoft.NETFramework.ReferenceAssemblies` | 技能规范的 `$(SL_REFERENCES)` 惯例；构建期 publicize 免去预生成步骤 |
| D12 | **CI**：GitHub Actions `ubuntu-latest` + steamcmd 匿名下载服务端 + `dotnet build -c Release` + artifact | 不依赖第三方预置引用包；与本地构建同法 |
| D13 | **许可**：CYW's API 使用 **MIT**（已确认）；README 致谢声明 | 公开分发合规 |
| D14 | **测试**：Core 单测（`dotnet test`，离服） + 每里程碑服务器冒烟清单（手工核对）；不为补丁层建设自动化集成测试 | 务实：补丁层离服不可测，冒烟清单更有效 |
| D15 | **示例插件**：仓库内 `examples/` 放一个演示 CYW's API 的示例插件（后期）+ 开发期最小冒烟测试插件 | 公开项目的标配；也用于回归验证 |
| D16 | **框架定位**：自有 Mod/服务端修改的底层框架；对标 LabAPI/EXILED 的开发者体验层；构建于 LabAPI 之上；1.0 前允许破坏性 API 变更 | API 设计取舍：开发体验、扩展性、稳定性优先 |

---

## 4. 里程碑与检查点

```
M0 环境与骨架 ──▶ CP0: 构建绿 + 服务器加载/卸载干净 + CI 就绪
M1 核心层 ──────▶ CP1: 模块/补丁/日志/配置可用 + 单测绿 + 重载无残留
M2 玩家与事件层 ─▶ CP2: 进服→CywPlayer 事件序正确；退服/重载清理干净
M3 命令框架 ────▶ CP3: RA 与客户端控制台命令可用（含权限/参数/限制器）
M4 自定义内容 ──▶ CP4a 物品/枪械/投掷物；CP4b 角色/队伍/分波；CP4c 效果/能力/弹药
M5 呈现与交互 ──▶ CP5a Hints 叠层；CP5b Settings；CP5c RA API；CP5d Toys/地图；CP5e 语音(可选)
M6 打磨与发布 ──▶ CP6: 发布清单全过 + 干净服务器验收 + v0.1 公开
M7 EXILED 对标完备 ─▶ CP7: Permissions / NameTag / Loader 生态协议 / Installer（§1.5 矩阵）可用
M8 自研增强工具轨 ──▶ CP8: Profiler / Scheduler / NetOptimizer 等按需交付
M9 EXILED 退役验收 ─▶ CP9: §1.5 退役清单逐项签收 → v1.0
```

每个检查点都包含固定的**服务器冒烟循环**：`启动 → 插件加载 → 完整回合 → 重载/重启 → 禁用 → 无错误日志`。

---

## 5. 任务分解

### 5.1 Phase 0–2 详细任务
见 `tasks/todo.md`（含验收标准、验证方式、依赖、规模）。摘要：

- **Phase 0**：steamcmd 与服务端（T0.1）→ 引用环境变量（T0.2）→ 仓库骨架与 build spike（T0.3）→ 最小插件入口与冒烟（T0.4）→ CI（T0.5）→ 参考规格沉淀（T0.6）
- **Phase 1**：CywConfig（T1.1）→ CywLogger（T1.2）→ CywPatcher（T1.3a/b）→ 模块系统（T1.4）→ 版本兼容与 banner（T1.5）→ Core 第一批单测（T1.6）
- **Phase 2**：CywPlayer 注册表+构造补丁（T2.1）→ 容器子集（T2.2a/b）→ 事件桥（T2.3）→ Cyw 事件系统（T2.4）→ 玩家更新循环（T2.5）

### 5.2 功能规格与优先级（来自盘点报告）

> 优先级：**P0** = v0.1 必须 / **P1** = 高（支撑实际插件）/ **P2** = 中 / **P3** = 低（可按需裁剪）

| 子系统 | 参考规模 | 优先级 | 阶段 | 备注 |
|---|---|---|---|---|
| 加载器/核心/配置/日志 | Core 25 文件 | P0 | M1 | 我们以模块系统重构其"上帝对象" |
| 补丁器 | — | P0 | M1 | 参考 92 文件/122 处补丁的扫描与禁用机制 |
| CywPlayer + 容器 | API 直属 10 文件 ≈5.6k 行 + Containers 9 文件 ≈3.7k 行 | P0 | M2 | 替换 LabAPI Player 为增强子类（D4） |
| 事件桥 + 事件系统 | Events 85 文件 ≈3.7k 行（91 个公开事件） | P0 | M2 | 首批事件集先做（玩家生命周期/回合/服务器） |
| 玩家更新循环 | Utilities/Update | P0 | M2 | 效果/能力/工具依赖的 tick 机制 |
| 命令框架 | Commands 120 文件 ≈9.7k 行 | P0 | M3 | 最自包含子系统；内置命令组随内容子系统补充 |
| 自定义物品/枪械/投掷物 | ≈2.5k 行 + 6 补丁 | P1 | M4a | 序列级跟踪（TrackedCustomItem 等效） |
| 自定义角色/队伍/分波 | Roles 471 行 + Teams ≈1.1k + RoleSelection 9 文件 | P1 | M4b | 分波选择算法（Human/Scp RoleSelector） |
| 自定义效果/能力/弹药 | Effects 261 行 + Abilities 753 行 | P1 | M4c | 每帧 tick 与持续时间变体 |
| Hints 叠层 | 13 文件 ≈1.9k 行 | P1 | M5a | 图片 hint 依赖 System.Drawing |
| 游戏模式基类 | 327 行 | P2 | M4d | 可启停/波次抑制/互斥 |
| Settings API | 15 文件 ≈1.9k 行 + 3 补丁 | P2 | M5b | patch 游戏 ServerSpecificSettingsSync |
| RemoteAdmin API | 10 文件 ≈1k 行 + 9 补丁 | P2 | M5c | 最脆弱的补丁组（NMS 内部），慎重排期 |
| Toys/Prefabs/电梯/特斯拉/地图/相机 | Toys 13 ≈2k 行 + Prefabs 623 行 + 其他 | P2 | M5d | 包装器层，独立性较好 |
| 语音/音频 | ≈1.1k 行 + Voice 补丁 | P3 | M5e | 后台线程 Opus；可整体裁剪（README 提及的 NVorbis 实际零引用） |
| 修复杂项（Fixes/节假日/SCP079奖励等） | Patches 内 ≈15 文件 | P3 | M6 | 按需搬运移植（如客户端自定义命令修复很实用，可提前） |
| 工具层（Extensions/Utilities/Pooling） | 19+42+8 文件 ≈11k 行 | 伴随 | 各阶段 | 随需实现，不单独成阶段 |
| EXILED 对标：Permissions | Exiled.Permissions 对标 | P1 | M7a | 节点/组/继承 + 配置驱动 + 命令集成（§1.5） |
| EXILED 对标：NameTag | Exiled.CreditTags 对标 | P2 | M7b | 名牌/头衔展示 |
| EXILED 对标：Loader 生态协议 | Exiled.Loader 对标 | P1 | M7c | 版本化互操作 API + 依赖解析；触发 promotion rule 评审 |
| EXILED 对标：Installer | Exiled.Installer 对标 | P2 | M7d | 安装/升级工具 |
| 自研增强工具 | — | P3 | M8 | Profiler/Scheduler/NetOptimizer/MemoryManager/EntityManager/Printer/LoadController（按需） |

### 5.3 Phase 3+ 滚动细化（到达该阶段时再拆成 T 级任务）

- **Phase 3 命令框架**：注册与调度 → 参数解析器（基础类型/枚举/玩家/向量/时间）→ 限制器（范围/白名单/长度）→ Runner（同步/异步/协程/续命令）→ 嵌套命令组 → RA/客户端/控制台接入与权限 → 内置命令组（随内容子系统补齐）
- **Phase 4 自定义内容**：4a 物品基类+跟踪器 → 枪械/投掷物 → 4b 角色+队伍 → 分波选择 → 4c 效果/能力/弹药 → 4d 游戏模式
- **Phase 5 呈现与交互**：5a Hints（控制器/元素/补丁）→ 5b Settings → 5c RA API → 5d Toys/Prefabs/地图包装 → 5e 语音（可选，默认最后）
- **Phase 6 打磨发布**：修复类补丁按需 → 文档（README/API XML/教程）→ 示例插件 → CI 完善 → 发布清单 → v0.1
- **Phase 7 EXILED 对标子系统**：7a Permissions（节点/组/继承 → 命令集成）→ 7b NameTag → 7c Loader 生态协议（互操作 API 拆分评审 + 依赖解析）→ 7d Installer
- **Phase 8 自研增强工具**（按需）：Profiler → Scheduler → NetOptimizer → MemoryManager / EntityManager / Printer / LoadController
- **Phase 9 EXILED 退役验收**：§1.5 矩阵签收 → 退役清单 → 迁移对照表 → 性能基线 → v1.0

---

## 6. 风险与缓解

| 风险 | 影响 | 缓解 |
|---|---|---|
| 游戏更新（14.2.x→+）破坏 publicized 内部引用与补丁 | 高 | 补丁与游戏类型访问集中在 `Patches/` 与包装层、每补丁单文件；版本兼容检查；冒烟清单；关注上游更新 |
| 重写规模大（参考 58.7k 行） | 高 | 里程碑每步可交付可用框架；P0/P1 优先；Phase 3+ 滚动细化避免过度规划 |
| steamcmd 网络受限（国内网络） | 中 | 备选：Steam 客户端"库→工具"安装专用服务器；再备选：用户提供 Managed 文件夹 |
| Harmony 版本与服务器不一致 | 中 | T0.1 实测服务器 `0Harmony.dll` 版本并对齐 NuGet 引用；CI 同法验证 |
| publicize 构建兼容性 | 中 | T0.3 build spike 尽早验证；备选：脚本预生成 publicized DLL |
| 早期加载深度不足（参考实现在构造函数期修复 LabAPI 自身行为的场景） | 中 | v0.1 先用标准早加载路径；预留"加载前补丁"增强项（日志钩子级）到 Phase 6 按需 |
| 与 LabExtended 混淆/权利争议 | 低 | 独立实现声明 + 致谢；不复制文本/不搬运其构建产物 |
| Z: 盘空间不足 | 低 | 服务端与 steamcmd 一律安装到 D:/E:；仓库本身很小 |
| EXILED 替代目标扩大范围（范围膨胀） | 中 | §1.5「轻量级」量化约束表防膨胀；M7 各子系统"够用即停"；promotion rule 控制程序集数量 |
| 与 EXILED 行为差异导致迁移摩擦 | 中 | 退役清单包含配置/权限迁移对照表；关键行为在需求文档登记基线（requirements.md） |

---

## 7. 已确认决策（2026-11-07）

1. **LICENSE**：MIT ✓
2. **玩家策略 D4**：`CywPlayer : Player` + 构造替换补丁 ✓
3. **优先级**：按 §5.2 默认顺序（P0 → P1）✓
4. **安装位置**：`D:\steamcmd` + `D:\SCPSL-Server` ✓
5. **框架定位**：自有 Mod 与服务端修改的底层框架（对标 LabAPI/EXILED 的开发者体验层）✓
6. **待定（可后置）**：`git init` 时机、GitHub 远端仓库名与可见性、示例插件范围

---

## 8. 参考资源

| 资源 | 位置/链接 |
|---|---|
| 参考实现（只读） | `Z:\SCPSL-Worker\example\LabExtended-master` |
| 参考入口文件（速查） | `LabExtended/Core/ApiLoader.cs`、`Core/ApiPatcher.cs`、`API/ExPlayer.cs`、`Events/InternalEvents.cs`、`Commands/CommandManager.cs`、`.github/workflows/dotnet.yml` |
| LabExtended 公开 Wiki（规格参考） | https://github.com/marchellcx/LabExtended/wiki（Home / API 两页） |
| LabAPI 官方 Wiki（权威文档） | https://github.com/northwood-studios/LabAPI/wiki |
| EXILED 官方仓库（对标/结构参考） | https://github.com/ExMod-Team/EXILED（`EXILED/` 产品目录、`Directory.Build.props`/`EXILED.props`、`build.ps1`/`packaging.ps1` 模式已采纳） |
| EXILED 技能参考 | `.opencode/skills/scpsl-exiled/references/`（api-map / events-catalogue / config-and-translations） |
| 技能参考文件 | `.opencode/skills/scpsl-labapi/references/`（api-map / events-catalogue / wrappers-and-features）、`scpsl-plugin-dev/references/`（version-pins / decision-matrix） |
| 生命周期与质量框架（DO-178C 对标） | `docs/lifecycle/`（需求/验证记录/CM/QA/工具置信度）、`CONSTRAINTS.md`（可机械检查护栏）、`AGENTS.md`（协作须知） |
| 服务端实际安装路径（2026-11-08） | `E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server`（`SL_REFERENCES` 指向其 `SCPSL_Data\Managed`）；LabAPI 部署目录 `%APPDATA%\SCP Secret Laboratory\LabAPI\{plugins,dependencies,configs}` |
