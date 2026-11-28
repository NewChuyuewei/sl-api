# 需求规格（Requirements Specification）

> 文档编号：**CYWS-REQ-001** ｜ 版本：v1.2（2026-11-28，CYW.Printer 0.2.0 CYW 自有输出需求登记；上一版 v1.1：2026-11-14 质量加固战役登记） ｜ 基准：见 `docs/lifecycle/README.md` §配置管理
> 方法：DO-178C 等比缩放（适用性声明见 `docs/lifecycle/README.md`）。
> 验证方法图例：**T** = 离服单元测试 ｜ **D** = 实机演示（目标环境验证） ｜ **R** = 审查 ｜ **A** = 分析
> 验证证据编号（VR-xxx）对应 `docs/lifecycle/verification-records.md`。
> 路径约定：本文档中的路径相对于 `CYW.Plugins/`；以 `../` 开头的指仓库根。
> 当前发布边界：最终候选 `0CywApi.dll`（SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`）的 QA-A + QA-A2 合并结论为 PASS，QA-B 为 PASS，证据归档为 VR-017～VR-019；Windows 干净服务器仅 zip 安装、S1/S3 生命周期、真实第三方 ServerQoL 共存、LOG-005 与回合重启均已在该候选通过。此为 QA 证据归档，不代表发布或 tag 批准。
> 仍为 **UNVERIFIED** / 范围排除：多玩家与玩家进出、性能、非 Windows 解压、ServerQoL 玩法规则、交互式桌面渲染；实机验证使用 headless LocalAdmin、0 玩家且无可见窗口。
> 规划边界（2026-11-28）：CYW.Printer 0.2.0 仅登记**经批准、尚未实现**的 CYW 自有输出可选 stdout 路由；PRN-S1 全局接管裁决为 **NO-GO**（VR-023），PRN-S2 可见 stdout 前置条件裁决为 **CONDITIONAL**（VR-022）。下文 PRN 证据状态均为 **待收集**，不得复用 v0.1.0 QA 认证。

---

## 1. 高层需求（HLR）

| ID | 高层需求 | 验证方法 | 状态 |
|---|---|---|---|
| CYWS-HLR-001 | 框架以 LabAPI 库插件形态分發，并在所有其他插件之前加载与启用 | D/R | ✅ 已满足 |
| CYWS-HLR-002 | 框架提供分区化配置；任何配置损坏都不得阻止框架启用（回退默认值并告警） | D/T | ✅ 已满足 |
| CYWS-HLR-003 | 框架提供统一日志接口：级别门控（DEBUG 受配置控制）、可选 ANSI 颜色、可选来源前缀 | D/T/R | ✅ 已满足 |
| CYWS-HLR-004 | 框架提供受控 Harmony 补丁系统：显式标记、单类故障隔离、配置化禁用、与自有 Harmony ID 对称撤销、计数报告 | D/T/R | ✅ 已满足 |
| CYWS-HLR-005 | 框架提供确定性模块系统：优先级排序启用、反向禁用、单模块故障隔离、生命周期对称 | D/R | ✅ 已满足 |
| CYWS-HLR-006 | 框架校验游戏版本并在越界时告警而不崩溃；将框架版本注入服务器 BuildInfo | D/T | ✅ 已满足 |
| CYWS-HLR-007 | 框架的纯逻辑层（Core）可在离开游戏服务器的环境中独立构建与单元测试，并与需求双向追溯 | T/R | ✅ 已满足 |
| CYWS-HLR-008 | 开发/构建/部署/验证环境可复现：脚本化构建部署、固定工具链版本、文档化的目标环境 | D/R | ✅ 已满足 |
| CYWS-HLR-009 | CYW.Printer 0.2.0 为经 `CywLog` 主动发出的 CYW 自有记录提供显式 opt-in 的 direct stdout 路由；默认保持既有 LabAPI Logger 路径，失败可回退，且不全局接管或改变第三方输出 | D/T/R/A | ⏳ 已批准，待实现/验证 |

> 备注（2026-11-14，评审 OBS-2）：HLR-001 的最先加载与启用语义依赖 `LoadPriority.Highest` + `0` 文件名前缀约定；LabAPI 对同优先级插件按发现顺序启用（非契约保证）。M2 若需要 CYW 最先注册的硬保证，必须显式验证该假设。

## 2. 低层需求（LLR）

### 2.1 配置（CFG）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-CFG-001 | 配置类型仅使用属性（LabAPI 序列化约束），分区为 Log / Patches / Compatibility | `CYW.API/src/CywApi/CywConfig.cs` | R | VR-003 | ✅ |
| CYWS-CFG-002 | YAML 损坏时：加载器告警 + 全部默认值 + 框架照常启用 | LabAPI 加载器 + `CywConfig` 默认值 | D | VR-004 | ✅ |
| CYWS-CFG-003 | `SaveConfig()` 往返：序列化后重新加载保持结构（含分区与空列表） | `CywConfig`（经 LabAPI `Plugin<TConfig>`） | D | VR-002 | ✅ |
| CYWS-CFG-004 | 配置分区为 null（如 `log:` 空值）时，访问不得抛出空引用——setter 以空对象替换 null | `CYW.API/src/CywApi/CywConfig.cs`（setter 防护） | D/R | VR-009 ✅ | ✅ |

### 2.2 日志（LOG）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-LOG-001 | `Debug()` 仅在 `Config.Debug == true` 时输出；其余级别始终输出 | `CYW.API/src/CywApi/CywLog.cs` | D | VR-005（debug 行随开关出现/消失） | ✅ |
| CYWS-LOG-002 | `&x` 颜色码 → ANSI 转义；`&&` → 字面 `&`；未知码原样保留；`StripCodes` 对应剥离 | `CYW.API/src/CywApi.Core/Colors/AnsiColorParser.cs` | T | VR-002（7 用例） | ✅ |
| CYWS-LOG-003 | 可选的来源程序集名前缀（排除框架自身程序集） | `CYW.API/src/CywApi/CywLog.cs` | R | VR-006 | ✅ |
| CYWS-LOG-004 | 颜色解析器对 null/空输入返回原值（不抛出） | `AnsiColorParser`（参数宣告为可空） | T | VR-002（26 用例，含 3 项空值用例） | ✅ |

| CYWS-LOG-005 | 输出净化（choke-point）：`CywLog.Format` 在颜色处理前将原始控制字符（C0/C1；换行 `\n` 除外）替换为可见表示，使不可信字符串无法注入终端控制序列或伪造日志行；另提供 `EscapeData(string)` 将一切控制字符（含 `\n`/`\r`）转为可见表示，供数据字段使用 | `CywLog.cs` + Core `ControlCharFilter`（新增）+ 单测 | T/D | VR-011（T：45/45，含 `ControlCharFilterTests`）、VR-013（D：活体探针）、VR-019（最终候选 QAB-LOG005-01） | ✅ |

### 2.3 补丁系统（PAT）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-PAT-001 | 仅 `[CywPatch]` 标记的类被应用；方法使用标准 Harmony 特性 | `CYW.API/src/CywApi/Patching/CywPatcher.cs` + `CywPatchAttribute.cs` | D/R | VR-003/VR-005（仅标记类被 적용） | ✅ |
| CYWS-PAT-002 | 单类补丁失败仅记录错误并计数，不阻断其余补丁类 | `CywPatcher.Apply` 逐类 try/catch | D | VR-005（`+1 applied, 1 failed`） | ✅ |
| CYWS-PAT-003 | 禁用匹配：全名/短名精确匹配；`Namespace.*` 前缀通配；空白容错 | `CYW.API/src/CywApi.Core/Patching/CywPatchFilter.cs` | T | VR-002（11 用例） | ✅ |
| CYWS-PAT-004 | 撤销仅作用于自有 Harmony ID（`cywapi.framework`），绝不影响他方补丁 | `CywPatcher.UnpatchAll` | R | VR-006 | ✅ |
| CYWS-PAT-005 | 应用/失败/跳过计数在每次应用与汇总时报告 | `CywPatcher` | D | VR-003/VR-005 | ✅ |
| CYWS-PAT-006 | 同一程序集重复应用为幂等操作（不产生重复补丁），并记录跳过 | `CywPatcher.Apply`（已应用集合守卫） | R | VR-006（F-003 修复）+ VR-009 回归 | ✅ |

| CYWS-PAT-007 | 补丁器状态一致性：程序集仅在类型发现成功后标记为已应用；发现/处理失败时回滚标记（保持可重试）；计数覆盖单次 Apply 调用范围并如实汇总 | `CywPatcher.Apply` | T/R | VR-015（R：修复复核通过）、VR-021（D：D-PAT-007 通过，发现失败后可重试且计数正确）；T 未执行（RV2-17 按设计保留） | ✅ |
| CYWS-PAT-008 | 部分加载诊断：`ReflectionTypeLoadException` 时将 LoaderExceptions 数量与首条消息记录为 Warn | `CywPatcher.Apply` | T/R | VR-015（R：修复复核通过）、VR-021（D：D-PAT-008 通过，LoaderExceptions 数量与首条消息 WARN 已证实）；T 未执行（RV2-17 按设计保留） | ✅ |
| CYWS-PAT-009 | 公共契约与审计：`Apply`/`UnpatchAll`/`HarmonyId` 的契约（共享框架 ID、随框架禁用一并撤销、ID 保留语义）在 XML 文档中明示；`UnpatchAll` 成功路径输出 Info 审计行（尽量含受影响方法数） | `CywPatcher.cs` | R/D | VR-015（R：公共契约与生命周期对称复核通过）、VR-021（D：D-PAT-009 通过，`UnpatchAll` INFO 审计与自有 ID 隔离） | ✅ |

### 2.4 模块系统（MOD）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-MOD-001 | 模块按 Priority 降序启用（稳定排序），按反向顺序禁用 | `CYW.API/src/CywApi/Modules/CywModuleManager.cs` | D/R | VR-003（顺序日志）、VR-005 | ✅ |
| CYWS-MOD-002 | 单模块 OnEnable 抛出异常仅记录并跳过；其余模块照常启用 | `CywModuleManager.EnableAll` 逐模块 try/catch | D | VR-005（`2/3 enabled`） | ✅ |
| CYWS-MOD-003 | 同类型模块重复注册抛出 `InvalidOperationException` | `CywModuleManager.Register` | R | VR-006 | ✅ |
| CYWS-MOD-004 | 已开始启用（EnableAll 之后）再注册模块必须抛出异常（防止静默不生效） | `CywModuleManager`（运行态守卫） | R | VR-006（F-004 修复）+ VR-009 回归 | ✅ |

| CYWS-MOD-005 | `DisableAll` 在未启用状态下调用：输出警告并保持注册表不变（不清空、不抛出）；禁用路径绝不抛出 | `CywModuleManager` | R/T | VR-014（D：跨启动禁用/恢复回归）、VR-015（R）、VR-017（最终候选 S3 跨启动回归）、VR-021（D：D-MOD-005 通过，未启用状态告警、注册表不变且后续生命周期正常） | ✅ |

### 2.5 兼容性（CMP）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-CMP-001 | 从 `GameCore.Version` 的 Major/Minor/Revision 构造运行版本 | `CYW.API/src/CywApi/Modules/CompatibilityModule.cs` | D | VR-003（读出 14.2.9） | ✅ |
| CYWS-CMP-002 | 版本低于最小支持版本（14.2.0）时输出醒目警告且不崩溃、不阻断启用 | `VersionRange`（Core）+ `CompatibilityModule` | D/T | VR-005（99.0.0 越界警告）、VR-002（VersionRange 5 用例） | ✅ |
| CYWS-CMP-003 | BuildInfo 注入为幂等（重复启用不重复追加），且注入行在**最终** `buildinfo` 输出中可见（须在 LabAPI `AddPluginsToBuildInfo` 覆盖之后写入；经 `ServerEvents.PluginsEnabled` 或等价后置时机） | `CompatibilityModule`（后置注入） | D | VR-012（S1：后置重注入双标记，最终 Mod Description 含框架版本行，零异常）、VR-017（最终候选 S1）、VR-019（QAB-CMP003-01） | ✅ |

| CYWS-CMP-004 | 服务器透明性声明门禁：`IsTransparent` 在零侵入性补丁阶段保持 `true`；任何侵入性补丁（M2 D4 玩家替换等）落地前必须复评并预期切换为 `false` | `CywPlugin.cs` | R/D | 用户裁决 2026-11-14（RV2-01） | ⏳ M2 门禁 |

### 2.6 构建与生命周期（BLD）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-BLD-001 | Release 构建零警告零错误（全解决方案，net48） | `../CywApi.slnx` + 三个项目 | T | VR-001、VR-017（最终候选 0/0） | ✅ |
| CYWS-BLD-002 | 构建期 publicize 流水线：引用原始游戏 DLL，产出可直接加载的插件（不携带游戏程序集） | `CYW.API/src/CywApi/CywApi.csproj`（BepInEx.AssemblyPublicizer） | D | VR-003（服务器实际加载） | ✅ |
| CYWS-BLD-003 | 部署脚本将产物放置到 `plugins\global` + `dependencies\global` | `scripts/dev/deploy.ps1` | D | VR-003/VR-004 | ✅ |
| CYWS-BLD-004 | Core 单元测试可离服运行（`dotnet test`），并产生覆盖率测量 | `tests/CywApi.Tests` + coverlet | T | VR-002/VR-008、VR-017（最终候选 45/45；行 113/113；分支 90/92） | ✅ |

| CYWS-BLD-005 | 语言版本单点钉住：全解决方案 `LangVersion` 由 `Directory.Build.props`（14.0）唯一定义；项目不得覆盖 | `Directory.Build.props` + `*.csproj` | R/T | VR-015（R：修复复核通过）、VR-011（T：Release 0 警告/0 错误，45/45） | ✅ |
| CYWS-BLD-006 | publicize 面最小化：每个 `Publicize="true"` 引用在 csproj 注明理由；`LabApi` 引用默认不 publicize（确需内部成员时逐项注明） | `CywApi.csproj` | R | VR-015（R：修复复核通过） | ✅ |

### 2.7 CYW.Printer：CYW 自有输出（PRN，0.2.0 规划）

> **批准边界**：仅允许 CYW 自有输出 opt-in direct stdout。PRN-S1 已否决全局接管/全局 `AddLog` 替换（**NO-GO**，[VR-023](verification-records.md)）；PRN-S2 仅批准满足可见 stdout 前置条件的 best-effort 展示路线（**CONDITIONAL**，[VR-022](verification-records.md)）。本节是 0.2.0 实现前需求基线，不表示任何产品行为已经存在或通过验证。

#### 2.7.1 行为需求

| ID | 低层需求 | 计划实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-PRN-001 | 输入范围仅限经公共 `CywLog.Debug/Info/Warn/Error` 主动发出的记录，包括选择 `CywLog` 的 CYW 生态插件；不得改变第三方输出。`printer.mode` 默认 `disabled`，默认路由与格式逐项保持 v0.1.0 LabAPI Logger 行为 | `CywLog.cs`、`PrinterRouteDecider.cs` | T/D/R | 待收集（PRNC-T-003、PRNC-D-DEFAULT-01、PRNC-D-COEX-01） | ⏳ 待实现 |
| CYWS-PRN-002 | direct stdout 必须同时满足 `printer.mode: stdout` 与 `printer.stdout_display_acknowledged: true`；任一条件不满足均保留 Logger。选择 `stdout` 但缺少确认时须通过 Logger 告警，不得写入 direct sink；默认 `disabled` 不得因此产生额外告警 | `PrinterSettingsValidator.cs`、`PrinterRouteDecider.cs` | T/D | 待收集（PRNC-T-002/003、PRNC-D-CONFIG-01、PRNC-D-HIDDEN-01A/01B） | ⏳ 待实现 |
| CYWS-PRN-003 | direct 模式的操作者前置条件是已通过 `--printStd` 或 `la_show_stdout_and_stderr: true` 启用可见 stdout；不得声称自动检测可见性。用户已接受：虚假确认可能使普通记录不在外层显示，direct 接收记录也不镜像到结构化/游戏日志目的地 | `PrinterSettingsValidator.cs`、`CywConsoleRouter.cs`、后续 README | T/D/R | 待收集（PRNC-D-VISIBLE-01、PRNC-D-HIDDEN-01A/01B、PRNC-D-MAN-TERMINAL-01）；拓扑前置 VR-022 | ⏳ 待实现 |
| CYWS-PRN-004 | 新增且仅新增下表五个 `printer.*` 配置键；键名、类型、默认值与范围不得偏离配置契约 | `CywConfig.cs`、`PrinterSettings.cs` | T/D/R | 待收集（PRNC-T-002/006、PRNC-D-CONFIG-01） | ⏳ 待实现 |
| CYWS-PRN-005 | 未知 mode、null Printer 分区或非法数值（含 NaN/infinity、越界值）必须使整个 Printer 分区恢复默认值并经 Logger 告警；不得只保留同分区的其他非法/半有效设置，不得阻止框架启用 | `CywConfig.cs`、`PrinterSettingsValidator.cs` | T/D | 待收集（PRNC-T-002/006、PRNC-D-CONFIG-01） | ⏳ 待实现 |
| CYWS-PRN-006 | Printer 设置仅在 enable 时取得快照；运行中修改配置不得改变本启用周期的路由，变更须重启后生效 | `CywPlugin.cs`、`PrinterModule.cs`、`PrinterSettings.cs` | T/D/R | 待收集（PRNC-T-002、PRNC-D-CONFIG-01、PRNC-D-LIFE-01） | ⏳ 待实现 |
| CYWS-PRN-007 | 超过 `max_message_chars` 或 `max_lines_per_record` 的记录必须**完整**改走 Logger，不得截断或部分 direct；资格绕过提示按 `bypass_notice_interval_seconds` 聚合，禁止逐记录提示刷屏 | `PrinterRouteDecider.cs`、`CywConsoleRouter.cs` | T/D | 待收集（PRNC-T-003、PRNC-D-CONFIG-01、PRNC-D-VOLUME-01） | ⏳ 待实现 |
| CYWS-PRN-008 | 所有 Logger 路径使用既有 Logger 调用映射与 `CywLog.Format` 输出；DEBUG 门控、`log.use_colors`、`log.prepend_source`、`EscapeData` 与 CYWS-LOG-005 继续作为格式化/安全 choke points。禁用、不可用、超限及失败回退均走 Logger | `CywLog.cs`、`CywConsoleRouter.cs`、`PrinterRouteDecider.cs` | T/D/R | 待收集（PRNC-T-003/005、PRNC-D-DEFAULT-01、PRNC-D-INJECT-01、PRNC-D-FAIL-01） | ⏳ 待实现 |
| CYWS-PRN-009 | 一条成功 direct 写入的记录只能有一个目的地，不得再镜像到 LabAPI Logger 或结构化/游戏日志；仅被 direct 路由实际接收的记录适用此语义，回退记录仍由 Logger 输出 | `CywConsoleRouter.cs`、`SystemConsoleLineSink.cs` | T/D | 待收集（PRNC-T-003、PRNC-D-VISIBLE-01、PRNC-D-COEX-01） | ⏳ 待实现 |
| CYWS-PRN-010 | stdout 路由启用时启动诊断必须包含两条可区分记录：经结构化 Logger 发出的“不镜像及如何恢复 Logger”警告，以及 direct 写出的 `CYW stdout route active` 确认；两者不得互相替代 | `PrinterModule.cs`、`CywConsoleRouter.cs` | D/T | 待收集（PRNC-D-VISIBLE-01） | ⏳ 待实现 |
| CYWS-PRN-011 | direct 普通行前缀为 `[LEVEL] [0CywApi]`，续行前缀为 `[LEVEL] [0CywApi] +`。对**已净化**消息按 LF 分割：空内部行加续行前缀；末尾 LF 产生最终空续行；空输入产生一条普通行；超限整记录回退，不得静默截断 | `DirectConsoleLineFormatter.cs`、`CywConsoleRouter.cs` | T/D | 待收集（PRNC-T-004/005、PRNC-D-MULTI-01、PRNC-D-UNICODE-01） | ⏳ 待实现 |
| CYWS-PRN-012 | `log.use_colors=false` 时 Printer 不生成 ANSI 并保持既有 StripCodes 行为；为 true 时保留既有 CYW SGR，并增加级别基色/重置（DEBUG 灰、INFO 白、WARN 黄、ERROR 红），每行均重置，意外 ESC 必须净化 | `DirectConsoleLineFormatter.cs`、`CywLog.cs` | T/D | 待收集（PRNC-T-004/005、PRNC-D-ANSI-01、PRNC-D-INJECT-01、PRNC-D-MAN-TERMINAL-01） | ⏳ 待实现 |
| CYWS-PRN-013 | direct sink 仅在每条记录锁内同步调用完整行 `System.Console.Out.WriteLine`；不得引入队列、MEC、Thread 或 Task。不得保证 CYW/第三方/LocalAdmin 跨通道顺序，也不得依赖 Flush、stderr 或输出确认来推断已显示 | `SystemConsoleLineSink.cs`、`CywConsoleRouter.cs` | T/D/A | 待收集（PRNC-D-VISIBLE-01、PRNC-D-VOLUME-01、PRNC-D-COEX-01）；拓扑约束 VR-022 | ⏳ 待实现 |
| CYWS-PRN-014 | 写前失败时整条记录回退 Logger；可能已部分写入时仍回退并警告“部分写入可能重复”，随后本启用周期禁用 direct sink。失败警告必须经 Logger，不得递归经过失败 sink；其他后续记录继续使用 Logger | `SystemConsoleLineSink.cs`、`CywConsoleRouter.cs` | T/D | 待收集（PRNC-T-003、PRNC-D-FAIL-01、PRNC-D-LIFE-01） | ⏳ 待实现 |
| CYWS-PRN-015 | `disabled` 模式不得注册 `PrinterModule`，默认模块计数/输出保持 2/2；`stdout` 模式以 Priority **90** 注册。禁用/回合/重启不得遗留 direct 状态或重复诊断 | `CywPlugin.cs`、`PrinterModule.cs` | D/T/R | 待收集（PRNC-D-DEFAULT-01、PRNC-D-LIFE-01、PRNC-D-ROUND-01、PRNC-D-CLEAN-01） | ⏳ 待实现 |
| CYWS-PRN-016 | 明确禁止范围外实现：全局拦截、Harmony/新补丁类、`ServerConsole.AddLog`/`PrintOnOutputs`/`IServerOutput` 改动、`Console.SetOut`/`SetError`/`OutputEncoding` 改动、新运行时程序集、新命令/权限/玩家数据；不得把本需求扩展为玩家或事件功能 | 全部计划文件；R 扫描与实机共存检查 | R/D | 待收集（PRNC-D-COEX-01、PRNC-D-CLEAN-01、PRNC-D-REL-01）；全局否决 VR-023 | ⏳ 待实现 |
| CYWS-PRN-017 | 目标版本为 **0.2.0**；v0.1.0 tag/artifact 不得修改。包仍只含当前两个第一方 DLL 与 `0Harmony`，不得新增 `CywPrinter.Core` 或其他运行时程序集；QA 必须记录全新候选精确哈希，不得复用 v0.1.0 认证 | `CywPlugin.cs`、现有两个项目、`packaging.ps1`（仅核对，不计划改动） | R/D | 待收集（PRNC-T-001、PRNC-D-CLEAN-01、PRNC-D-REL-01） | ⏳ 待实现 |
| CYWS-PRN-018 | 离服门禁：Release 0 警告/0 错误；现有 45 测试不得减少并新增 validator/route/formatter 测试；Core 行覆盖保持 100%，分支覆盖至少 97.83%；至少一次变异抽查必须按流程变红、恢复、强制重建并复绿 | `tests/CywApi.Tests/Printing/`、`../CywApi.slnx` | T/R | 待收集（PRNC-T-001..008） | ⏳ 待实现 |
| CYWS-PRN-019 | 高容量验收使用两个并发的纯日志生产者写入 10,000 条唯一记录：账目恰好一次、单记录行不交织、无持续保留内存增长且服务器保持响应；记录 Logger 与 stdout 的耗时、分配与 GC 对比。端到端退化 >25% 必须修复，或由所有者显式接受后方可放行 | `CywConsoleRouter.cs`、`SystemConsoleLineSink.cs`、QA 探针 | D/A | 待收集（PRNC-D-VOLUME-01） | ⏳ 待实现 |
| CYWS-PRN-020 | 0.2.0 README/发布文档必须说明可见性与无镜像限制，并明确文件保留需 `la_log_stdout_and_stderr: true`；五个新键及重启生效语义须有所有者操作说明。未重新验证前不得继承 v0.1.0 的安装/QA 通过声明 | 后续 `README.md`、`CHANGELOG.md`、发布说明 | R/D | 待收集（PRNC-D-REL-01、PRNC-D-MAN-TERMINAL-01） | ⏳ 待实现 |

#### 2.7.2 配置契约（对应 CYWS-PRN-004；全部为计划值）

| 配置键 | 类型 | 默认值 | 允许值/范围 | 效果与回退 | 验收场景 | 证据状态 |
|---|---|---|---|---|---|---|
| `printer.mode` | string | `disabled` | `disabled` / `stdout` | `disabled` 保持 Logger；`stdout` 仍须显示确认。未知值使整个 Printer 分区恢复默认并告警 | PRNC-T-002/003/006、PRNC-D-CONFIG-01 | 待收集 |
| `printer.stdout_display_acknowledged` | bool | `false` | `true` / `false` | direct 必需确认为 `true`；`stdout` 模式下缺失/false 时保持 Logger 并告警；默认 `disabled` 不额外告警 | PRNC-T-002/003、PRNC-D-HIDDEN-01A/01B | 待收集 |
| `printer.max_message_chars` | int | `16384` | `256..65536` | 超过限制的完整记录走 Logger；非法值使整个分区恢复默认并告警 | PRNC-T-003/006、PRNC-D-CONFIG-01 | 待收集 |
| `printer.max_lines_per_record` | int | `64` | `1..1024` | 超过限制的完整多行记录走 Logger；非法值使整个分区恢复默认并告警 | PRNC-T-003/006、PRNC-D-CONFIG-01 | 待收集 |
| `printer.bypass_notice_interval_seconds` | float | `30.0` | `1..300` | 资格绕过提示的聚合间隔；NaN/infinity/越界使整个分区恢复默认并告警 | PRNC-T-002/006、PRNC-D-CONFIG-01 | 待收集 |

#### 2.7.3 离服验收映射（全部待收集）

| 场景 ID | 必须覆盖的判定 | 需求追溯 | 目标文件/测试 | 证据状态 |
|---|---|---|---|---|
| PRNC-T-001 | Release 构建 0/0；现有 45 测试及新增 validator/route/formatter 测试全部通过 | PRN-017/018 | `../CywApi.slnx`、`tests/CywApi.Tests/Printing/` | 待收集 |
| PRNC-T-002 | 默认值、两键确认、null/未知/非法分区恢复与告警、enable 快照 | PRN-002/004/005/006 | `PrinterSettingsValidatorTests.cs`（计划） | 待收集 |
| PRNC-T-003 | disabled、缺确认、字符/行数限制、不可用与失败路由决策；成功 direct 不镜像 | PRN-001/002/003/007/008/009/014/015 | `PrinterRouteDeciderTests.cs`（计划） | 待收集 |
| PRNC-T-004 | 级别/来源前缀；空输入、空内部行、末尾 LF；级别颜色与每行 reset、colors-off | PRN-008/011/012 | `DirectConsoleLineFormatterTests.cs`（计划） | 待收集 |
| PRNC-T-005 | 原始 ESC/OSC/CSI/BEL/CR/C1/DEL、伪造前缀、意外 ESC 与不可信输入不得注入 | PRN-008/011/012 | `DirectConsoleLineFormatterTests.cs`、既有 `ControlCharFilterTests`（计划扩展/复核） | 待收集 |
| PRNC-T-006 | 五个键的上下边界、越界、NaN、infinity 与整分区默认恢复 | PRN-004/005/006 | `PrinterSettingsValidatorTests.cs`（计划） | 待收集 |
| PRNC-T-007 | Core 行覆盖 100%，分支覆盖 ≥97.83%，不得借新增代码降低棘轮 | PRN-018 | `tests/CywApi.Tests/Printing/` + coverlet | 待收集 |
| PRNC-T-008 | 变异抽查：目标缺陷变红 → 恢复 → 强制重建 → 全量复绿 | PRN-018 | validator/route/formatter 相关测试与 TRX/覆盖率记录 | 待收集 |

#### 2.7.4 目标环境验收映射（全部待收集）

| 场景 ID | 必须覆盖的判定 | 需求追溯 | 目标文件/测试或演示探针 | 证据状态 |
|---|---|---|---|---|
| PRNC-D-DEFAULT-01 | 默认 Logger 输出与 v0.1.0 一致，PrinterModule 不注册、模块 2/2 | PRN-001/008/015 | `CywLog.cs`、`CywPlugin.cs`、`PrinterModule.cs`；默认启动日志 | 待收集 |
| PRNC-D-VISIBLE-01 | `--printStd` 或 `la_show_stdout_and_stderr: true` 下 direct 可见、单一目的地及双启动诊断 | PRN-002/003/009/010/013 | `CywConsoleRouter.cs`、`SystemConsoleLineSink.cs`；可见 stdout 演示 | 待收集 |
| PRNC-D-HIDDEN-01A | 无可见 stdout 且未确认：Logger fallback 与告警，无 direct 写入 | PRN-002/003/008 | `PrinterSettingsValidator.cs`、`PrinterRouteDecider.cs`；隐藏模式演示 | 待收集 |
| PRNC-D-HIDDEN-01B | 无可见 stdout 但虚假确认：证明已接受的隐藏限制，不声称自动检测；文件保留配置单独核对 | PRN-002/003/020 | `CywConsoleRouter.cs`；隐藏模式与 LocalAdmin 配置演示 | 待收集 |
| PRNC-D-ANSI-01 | colors on/off、四级别基色/reset、既有 CYW SGR 与无 Printer 额外 ANSI | PRN-012 | `DirectConsoleLineFormatter.cs`；stdout 字节捕获 | 待收集 |
| PRNC-D-UNICODE-01 | 中文、重音字符、emoji 的实际捕获结果；不得外推通用编码保真 | PRN-011/013/020 | `SystemConsoleLineSink.cs`；UTF-8/LocalAdmin 双层捕获 | 待收集 |
| PRNC-D-MULTI-01 | 空输入、空内部行、末尾 LF、连续多行及超限整记录回退 | PRN-007/011 | `DirectConsoleLineFormatter.cs`、`CywConsoleRouter.cs`；QA 探针 | 待收集 |
| PRNC-D-INJECT-01 | 原始控制序列、伪造级别/来源前缀、CR/LF 变体及 colors-off 注入防护 | PRN-008/011/012 | `CywLog.cs`、`DirectConsoleLineFormatter.cs`；不可信 QA 探针 | 待收集 |
| PRNC-D-CONFIG-01 | 两键组合、五键边界、整分区恢复、告警与重启后快照生效 | PRN-002/004/005/006/007 | `CywConfig.cs`、`PrinterSettingsValidator.cs`；配置矩阵 | 待收集 |
| PRNC-D-LIFE-01 | enable/disable、禁用 direct sink 后的本周期行为、重启恢复与无重复诊断 | PRN-006/014/015 | `CywPlugin.cs`、`PrinterModule.cs`、`CywConsoleRouter.cs` | 待收集 |
| PRNC-D-ROUND-01 | 回合重启与服务器生命周期无 Printer 残留、重复输出或新增玩家依赖 | PRN-015/016 | `PrinterModule.cs`；0 玩家回合/重启演示 | 待收集 |
| PRNC-D-FAIL-01 | 写前失败、可能部分写入、“部分写入可能重复”警告、递归防护及本周期 Logger fallback | PRN-008/014 | `SystemConsoleLineSink.cs`、`CywConsoleRouter.cs`；故障注入探针 | 待收集 |
| PRNC-D-VOLUME-01 | 10,000 唯一记录、两个并发纯日志生产者；恰好一次、无行交织、内存/响应与 Logger 对比；记录 time/allocations/GC 与 >25% 裁决 | PRN-007/013/019 | `SystemConsoleLineSink.cs`、`CywConsoleRouter.cs`；高容量 QA 探针 | 待收集 |
| PRNC-D-COEX-01 | 第三方/LocalAdmin 输出不被接管或修改；无跨通道顺序承诺；CYW 生态插件可选择 CywLog | PRN-001/003/009/013/016 | `CywLog.cs`、`CywConsoleRouter.cs`；共存探针 | 待收集 |
| PRNC-D-CLEAN-01 | 干净服务器默认/stdout 安装与禁用循环；包仍两个第一方 DLL + 0Harmony，无新补丁/程序集/残留 | PRN-015/016/017 | 现有 `0CywApi`/`CywApi.Core` 项目与包清单 | 待收集 |
| PRNC-D-MAN-TERMINAL-01 | 用户提供的可见终端上人工核对 SGR、Unicode、长行/多行；未执行前保持 UNVERIFIED | PRN-003/012/020 | 可见交互终端；人工检查表 | 待收集 |
| PRNC-D-REL-01 | 0.2.0 文档、包内容、全新候选精确哈希与发布证据；v0.1.0 tag/artifact 不动 | PRN-016/017/020 | README/CHANGELOG/发布说明、`dist/CYW-Plugins.zip`（后续产物） | 待收集 |

> **玩家与证据边界**：本特性仅处理 CYW 自有管理输出，不要求真实玩家场景；类玩家不可信字符串由 QA 探针提供。可见终端由用户提供，但 PRNC-D-MAN-TERMINAL-01 未执行前仍为 **UNVERIFIED**。所有场景必须绑定 0.2.0 新候选精确哈希，v0.1.0 证据不可复用。

## 3. 追溯索引

### 3.1 代码 → 需求（倒查表）

| 源文件 | 覆盖需求 |
|---|---|
| `CYW.API/src/CywApi/CywPlugin.cs` | HLR-001/002（入口）、MOD-001、CMP-004 |
| `CYW.API/src/CywApi/CywConfig.cs` | HLR-002、CFG-001~004 |
| `CYW.API/src/CywApi/CywLog.cs` | HLR-003、LOG-001/003/004/005 |
| `CYW.API/src/CywApi/CywVersion.cs` | CMP-002 |
| `CYW.API/src/CywApi/Patching/CywPatchAttribute.cs` | PAT-001 |
| `CYW.API/src/CywApi/Patching/CywPatcher.cs` | HLR-004、PAT-001/002/004/005/006/007/008/009 |
| `CYW.API/src/CywApi/Modules/ICywModule.cs` | HLR-005 |
| `CYW.API/src/CywApi/Modules/CywModuleManager.cs` | HLR-005、MOD-001~005 |
| `CYW.API/src/CywApi/Modules/CompatibilityModule.cs` | HLR-006、CMP-001~003 |
| `CYW.API/src/CywApi/Modules/PatchingModule.cs` | HLR-004（接入模块生命周期） |
| `CYW.API/src/CywApi.Core/Colors/AnsiColorParser.cs` | HLR-003/007、LOG-002/004 |
| `CYW.API/src/CywApi.Core/Patching/CywPatchFilter.cs` | HLR-007、PAT-003 |
| `CYW.API/src/CywApi.Core/Compatibility/VersionRange.cs` | HLR-006/007、CMP-002 |
| `scripts/dev/deploy.ps1` | BLD-003 |
| `CywApi.slnx` + `*.csproj` | BLD-001/002/004/005/006 |

| `CYW.API/src/CywApi.Core/Logging/ControlCharFilter.cs`（新增） | HLR-007、LOG-005 |
| `CYW.API/src/CywApi.Core/Printing/PrinterMode.cs`（计划） | HLR-009、PRN-001/002/004 |
| `CYW.API/src/CywApi.Core/Printing/PrinterLogLevel.cs`（计划） | HLR-009、PRN-011/012 |
| `CYW.API/src/CywApi.Core/Printing/PrinterSettings.cs`（计划） | HLR-009、PRN-004/005/006 |
| `CYW.API/src/CywApi.Core/Printing/PrinterSettingsValidator.cs`（计划） | HLR-009、PRN-002/004/005/006/007 |
| `CYW.API/src/CywApi.Core/Printing/PrinterRouteDecider.cs`（计划） | HLR-009、PRN-001/002/003/007/008/009/014/015 |
| `CYW.API/src/CywApi.Core/Printing/DirectConsoleLineFormatter.cs`（计划） | HLR-009、PRN-011/012 |
| `CYW.API/src/CywApi/Printing/PrinterModule.cs`（计划） | HLR-009、PRN-006/010/014/015 |
| `CYW.API/src/CywApi/Printing/CywConsoleRouter.cs`（计划） | HLR-009、PRN-001/003/007/008/009/010/013/014/019 |
| `CYW.API/src/CywApi/Printing/SystemConsoleLineSink.cs`（计划） | HLR-009、PRN-009/013/014/019 |
| `CYW.API/src/CywApi/CywConfig.cs`（计划小改） | PRN-004/005/006 |
| `CYW.API/src/CywApi/CywLog.cs`（计划小改） | HLR-009、PRN-001/008/009/011/012/014 |
| `CYW.API/src/CywApi/CywPlugin.cs`（计划小改） | HLR-009、PRN-006/010/015/016/017 |
| `tests/CywApi.Tests/Printing/`（计划） | PRN-018；PRNC-T-001..008 |

### 3.2 测试 → 需求

| 测试 | 覆盖需求 |
|---|---|
| `AnsiColorParserTests` | LOG-002、LOG-004 |
| `CywPatchFilterTests` | PAT-003 |
| `VersionRangeTests` | CMP-002 |
| `CoreSmokeTests` | HLR-007（测试基础设施） |
| `ControlCharFilterTests`（新增） | LOG-005 |
| `PrinterSettingsValidatorTests`（计划） | PRN-002/004/005/006；PRNC-T-002/006 |
| `PrinterRouteDeciderTests`（计划） | PRN-001/002/003/007/008/009/014/015；PRNC-T-003 |
| `DirectConsoleLineFormatterTests`（计划） | PRN-008/011/012；PRNC-T-004/005 |

## 4. 需求变更流程

1. **新功能**：先在本文件新增/修改 HLR/LLR →（人工）批准 → 实现 → 在「验证证据」列登记证据 → 更新状态。
2. **行为变更**：必须更新对应 LLR 文本，并重新收集证据（旧证据标记失效）。
3. **紧急修复**：可先实现，但必须在**同一提交**内回填本文件（需求 + 证据）。
4. 未经本文件登记的框架行为不得进入主线（临时验证代码不适用此条，但必须在验证后移除并保持工作区干净——见 CONSTRAINTS.md 楼层条款）。
