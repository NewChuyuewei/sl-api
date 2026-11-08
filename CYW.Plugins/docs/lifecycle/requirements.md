# 需求规格（Requirements Specification）

> 文档编号：**CYWS-REQ-001** ｜ 版本：v1.0（2026-11-08） ｜ 基准：见 `docs/lifecycle/README.md` §配置管理
> 方法：DO-178C 等比缩放（适用性声明见 `docs/lifecycle/README.md`）。
> 验证方法图例：**T** = 离服单元测试 ｜ **D** = 实机演示（目标环境验证） ｜ **R** = 审查 ｜ **A** = 分析
> 验证证据编号（VR-xxx）对应 `docs/lifecycle/verification-records.md`。
> 路径约定：本文档中的路径相对于 `CYW.Plugins/`；以 `../` 开头的指仓库根。

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

### 2.3 补丁系统（PAT）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-PAT-001 | 仅 `[CywPatch]` 标记的类被应用；方法使用标准 Harmony 特性 | `CYW.API/src/CywApi/Patching/CywPatcher.cs` + `CywPatchAttribute.cs` | D/R | VR-003/VR-005（仅标记类被 적용） | ✅ |
| CYWS-PAT-002 | 单类补丁失败仅记录错误并计数，不阻断其余补丁类 | `CywPatcher.Apply` 逐类 try/catch | D | VR-005（`+1 applied, 1 failed`） | ✅ |
| CYWS-PAT-003 | 禁用匹配：全名/短名精确匹配；`Namespace.*` 前缀通配；空白容错 | `CYW.API/src/CywApi.Core/Patching/CywPatchFilter.cs` | T | VR-002（11 用例） | ✅ |
| CYWS-PAT-004 | 撤销仅作用于自有 Harmony ID（`cywapi.framework`），绝不影响他方补丁 | `CywPatcher.UnpatchAll` | R | VR-006 | ✅ |
| CYWS-PAT-005 | 应用/失败/跳过计数在每次应用与汇总时报告 | `CywPatcher` | D | VR-003/VR-005 | ✅ |
| CYWS-PAT-006 | 同一程序集重复应用为幂等操作（不产生重复补丁），并记录跳过 | `CywPatcher.Apply`（已应用集合守卫） | R | VR-006（F-003 修复）+ VR-009 回归 | ✅ |

### 2.4 模块系统（MOD）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-MOD-001 | 模块按 Priority 降序启用（稳定排序），按反向顺序禁用 | `CYW.API/src/CywApi/Modules/CywModuleManager.cs` | D/R | VR-003（顺序日志）、VR-005 | ✅ |
| CYWS-MOD-002 | 单模块 OnEnable 抛出异常仅记录并跳过；其余模块照常启用 | `CywModuleManager.EnableAll` 逐模块 try/catch | D | VR-005（`2/3 enabled`） | ✅ |
| CYWS-MOD-003 | 同类型模块重复注册抛出 `InvalidOperationException` | `CywModuleManager.Register` | R | VR-006 | ✅ |
| CYWS-MOD-004 | 已开始启用（EnableAll 之后）再注册模块必须抛出异常（防止静默不生效） | `CywModuleManager`（运行态守卫） | R | VR-006（F-004 修复）+ VR-009 回归 | ✅ |

### 2.5 兼容性（CMP）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-CMP-001 | 从 `GameCore.Version` 的 Major/Minor/Revision 构造运行版本 | `CYW.API/src/CywApi/Modules/CompatibilityModule.cs` | D | VR-003（读出 14.2.9） | ✅ |
| CYWS-CMP-002 | 版本低于最小支持版本（14.2.0）时输出醒目警告且不崩溃、不阻断启用 | `VersionRange`（Core）+ `CompatibilityModule` | D/T | VR-005（99.0.0 越界警告）、VR-002（VersionRange 5 用例） | ✅ |
| CYWS-CMP-003 | BuildInfo 注入为幂等（重复启用不重复追加），且注入行出现在 BuildInfo 输出中 | `CompatibilityModule.InjectBuildInfo` | D | VR-005（`buildinfo:` 输出含 `CYW's API v0.1.0`） | ✅ |

### 2.6 构建与生命周期（BLD）

| ID | 低层需求 | 实现 | 验证方法 | 验证证据 | 状态 |
|---|---|---|---|---|---|
| CYWS-BLD-001 | Release 构建零警告零错误（全解决方案，net48） | `../CywApi.slnx` + 三个项目 | T | VR-001 | ✅ |
| CYWS-BLD-002 | 构建期 publicize 流水线：引用原始游戏 DLL，产出可直接加载的插件（不携带游戏程序集） | `CYW.API/src/CywApi/CywApi.csproj`（BepInEx.AssemblyPublicizer） | D | VR-003（服务器实际加载） | ✅ |
| CYWS-BLD-003 | 部署脚本将产物放置到 `plugins\global` + `dependencies\global` | `scripts/dev/deploy.ps1` | D | VR-003/VR-004 | ✅ |
| CYWS-BLD-004 | Core 单元测试可离服运行（`dotnet test`），并产生覆盖率测量 | `tests/CywApi.Tests` + coverlet | T | VR-002/VR-008 | ✅ |

## 3. 追溯索引

### 3.1 代码 → 需求（倒查表）

| 源文件 | 覆盖需求 |
|---|---|
| `CYW.API/src/CywApi/CywPlugin.cs` | HLR-001/002（入口）、MOD-001 |
| `CYW.API/src/CywApi/CywConfig.cs` | HLR-002、CFG-001~004 |
| `CYW.API/src/CywApi/CywLog.cs` | HLR-003、LOG-001/003/004 |
| `CYW.API/src/CywApi/CywVersion.cs` | CMP-002 |
| `CYW.API/src/CywApi/Patching/CywPatchAttribute.cs` | PAT-001 |
| `CYW.API/src/CywApi/Patching/CywPatcher.cs` | HLR-004、PAT-001/002/004/005/006 |
| `CYW.API/src/CywApi/Modules/ICywModule.cs` | HLR-005 |
| `CYW.API/src/CywApi/Modules/CywModuleManager.cs` | HLR-005、MOD-001~004 |
| `CYW.API/src/CywApi/Modules/CompatibilityModule.cs` | HLR-006、CMP-001~003 |
| `CYW.API/src/CywApi/Modules/PatchingModule.cs` | HLR-004（接入模块生命周期） |
| `CYW.API/src/CywApi.Core/Colors/AnsiColorParser.cs` | HLR-003/007、LOG-002/004 |
| `CYW.API/src/CywApi.Core/Patching/CywPatchFilter.cs` | HLR-007、PAT-003 |
| `CYW.API/src/CywApi.Core/Compatibility/VersionRange.cs` | HLR-006/007、CMP-002 |
| `scripts/dev/deploy.ps1` | BLD-003 |
| `CywApi.slnx` + `*.csproj` | BLD-001/002/004 |

### 3.2 测试 → 需求

| 测试 | 覆盖需求 |
|---|---|
| `AnsiColorParserTests` | LOG-002、LOG-004 |
| `CywPatchFilterTests` | PAT-003 |
| `VersionRangeTests` | CMP-002 |
| `CoreSmokeTests` | HLR-007（测试基础设施） |

## 4. 需求变更流程

1. **新功能**：先在本文件新增/修改 HLR/LLR →（人工）批准 → 实现 → 在「验证证据」列登记证据 → 更新状态。
2. **行为变更**：必须更新对应 LLR 文本，并重新收集证据（旧证据标记失效）。
3. **紧急修复**：可先实现，但必须在**同一提交**内回填本文件（需求 + 证据）。
4. 未经本文件登记的框架行为不得进入主线（临时验证代码不适用此条，但必须在验证后移除并保持工作区干净——见 CONSTRAINTS.md 楼层条款）。
