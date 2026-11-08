# 验证记录（Verification Records）

> 文档编号：**CYWS-VR-001** ｜ 更新：2026-11-08
> 格式说明：每条记录含 **ID / 方法 / 环境 / 命令或步骤 / 结果 / 证据摘录 / 结论**。
> 方法图例：T 单测 ｜ D 实机演示 ｜ R 审查 ｜ A 分析。需求映射见 `requirements.md`。

---

## 环境基线（所有实机演示共用）

| 项 | 值 |
|---|---|
| 游戏版本 | SCP:SL **14.2.9**（Build 2026-11-04，GUID de44eb87…） |
| LabAPI | **1.1.7**（`SCPSL_Data\Managed\LabApi.dll`） |
| Harmony | 0Harmony **2.2.2**（由框架随 `dependencies\global` 部署） |
| 服务端 | `E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server`（Steam 安装） |
| 启动方式 | `LocalAdmin.exe 7777 --acceptEULA --useDefault --printStd` |
| 构建 | .NET SDK 10.0.302，Release，`SL_REFERENCES` → 服务端 Managed |
| 插件部署 | `LabAPI\plugins\global\0CywApi.dll` + `dependencies\global\{CywApi.Core.dll, 0Harmony.dll}` |

---

## VR-001 构建验证（T）

- **命令**：`dotnet build CywApi.slnx -c Release --no-incremental`
- **结果**：**0 警告 / 0 错误**（多次重复执行一致）
- **映射**：CYWS-BLD-001
- **结论**：✅ 通过

## VR-002 单元测试战役 #1（T）

- **命令**：`dotnet test tests\CywApi.Tests -c Release`
- **结果**：**23/23 通过**（AnsiColorParser 7 + CywPatchFilter 11 + VersionRange 5 + 基础设施 1）
- **映射**：LOG-002/004、PAT-003、CMP-002、HLR-007
- **结论**：✅ 通过（覆盖率测量见 VR-008）

## VR-003 实机演示 #1：加载与启用（D）— 2026-11-08 12:06

- **证据摘录**（真实服务器日志）：
  ```
  [LabApi] [LOADER] Successfully loaded 0Harmony, Version=2.2.2.0
  [LabApi] [LOADER] Successfully loaded CywApi.Core, Version=0.1.0.0
  [LabApi] [LOADER] Successfully loaded CywApi
  [0CywApi] CYW's API v0.1.0 starting (LabAPI 1.1.7)...
  [0CywApi] [compat] game version 14.2.9 is supported
  [DEBUG]   [modules] enabled: Compatibility（后接 Patching，顺序符合优先级）
  [0CywApi] [modules] 2/2 enabled
  [0CywApi] CYW's API v0.1.0 enabled.
  [LabApi] [LOADER] Successfully enabled 'CywApi', Version: 0.1.0, Author: 'CYW'
  ```
- **映射**：HLR-001/003/005/006、MOD-001、CMP-001、BLD-002/003、CFG-001
- **结论**：✅ 通过（服务器稳定运行 180s+，无异常）

## VR-004 实机演示 #2：坏配置容错 + 禁用开关（D）— 2026-11-08 15:58 / 16:01

- **步骤 A（损坏 YAML）**：将 config.yml 写入非法 YAML 后启动：
  ```
  [ERROR] [LabApi] [CONFIG_LOADER] Couldn't read the configuration ... YamlException (Line: 1 ...)
  [WARN]  [LabApi] Failed to load the configuration file, using default values.
  [0CywApi] CYW's API v0.1.0 starting ... [modules] 2/2 enabled ... enabled.
  ```
- **步骤 B（properties.yml `is_enabled: false`）**：插件被加载但**零运行时输出**（无 banner/模块/补丁/注入），禁用后恢复 `is_enabled: true`。
- **映射**：CFG-002、HLR-002；HLR-001（禁用边界）
- **结论**：✅ 通过

## VR-005 实机演示 #3：补充联测（故障注入）（D）— 2026-11-08 16:33

- **注入项**：① 抛异常模块（最高优先级）② 目标不存在的补丁类 ③ `MinGameVersion` 临时提升至 99.0.0 ④ BuildInfo 全文输出。
- **证据摘录**：
  ```
  [ERROR] [modules] 'TempThrowing' failed to enable: InvalidOperationException: temp exception module test
  [modules] 2/3 enabled
  [WARN]  [compat] game version 14.2.9 is outside the supported range (>= 99.0.0)...
  [DEBUG] [compat] buildinfo: Build info: ... CYW's API v0.1.0
  [ERROR] [patcher] failed to apply CywApi.Patching.TempFailingPatch: Patching exception in method null
  [cyw-selftest] good patch hit ×2
  [DEBUG] [patcher] applied 1 method(s) from CywApi.Patching.TempGoodPatch
  [patcher] 0CywApi: +1 applied, 1 failed, 0 skipped
  [0CywApi] CYW's API v0.1.0 enabled.
  ```
- **映射**：MOD-002（2/3 隔离）、PAT-002（+1/+1 隔离）、CMP-002（越界警告）、CMP-003（注入字面实锤）
- **观察项 OBS-001**（不影响本记录结论）：该次运行结束时进程早于测试脚本检查点自行结束（日志显示服务器已进入 idle 后）；证据窗口（启用后前 20 秒）完整。待后续运行复查，若无复现则关闭。
- **结论**：✅ 四项全部通过；注入代码已于测试后移除（工作区干净，见 VR-006 F-000）

## VR-006 审查记录：DO-178C 优化轮（R）— 2026-11-08

**审查范围**：Phase 0–1 全部产物（需求/代码/测试/脚本/文档），五轴（正确性/可读性/架构/安全/性能）。

| 发现 | 严重度 | 描述 | 处置 |
|---|---|---|---|
| F-000 | — | 验证用临时代码（3 个 Temp 文件 + 3 处临时修改）存在于工作区 | ✅ 已撤销（`git checkout` + 删除，工作区干净——注入/移除流程本身作为流程演练记录） |
| F-001 | Required | `CywConfig` 分区属性若被 YAML 显式置空（如 `log:`），后续访问可能空引用 | 🔧 修复：setter 空值防护（映射 CYWS-CFG-004） |
| F-002 | Required | `AnsiColorParser` 参数/返回对 null 的处理与可空性声明不符（可读性/正确性） | 🔧 修复：签名改可空 + 新增空值用例（映射 CYWS-LOG-004） |
| F-003 | Required | `CywPatcher.Apply` 对同一程序集重复调用可能重复打补丁（重载安全性） | 🔧 修复：已应用程序集集合守卫（映射 CYWS-PAT-006） |
| F-004 | Required | `CywModuleManager.Register` 在 `EnableAll` 之后调用会被静默忽略 | 🔧 修复：运行态守卫抛出异常（映射 CYWS-MOD-004） |
| F-005 | Nit | `CywModuleManager.EnabledCount` 未被使用 | ✅ 采纳：汇总日志改用该属性，消除未使用公开成员疑义 |
| F-006 | FYI | `CywPatchFilter` 对 `"*"` 模式（不匹配任何类型）不作为通配符 | 📄 已在需求 LLR CYWS-PAT-003 明示匹配语法；保持现状 |
| F-007 | Nit（流程） | 变异抽查恢复环节：增量构建因旧时间戳跳过重编译，复测跑在变异产物上（假红） | 🔧 流程加固：恢复后强制重建/touch；规则已写入 CONSTRAINTS.md 关键规则提醒 |

- **审查者**：AI 优化轮（作者与审查者同源，独立性受限已声明；人工评审为最终把关）
- **结论**：4 项 Required 修复 + 1 项 Nit 采纳；修复均处于本优化轮内完成并回归（见 VR-007/VR-008/VR-009）。

## VR-007 变异抽查：验证的验证（T）— 2026-11-08 完成

- **方法**：将 `AnsiColorParser.ToAnsi` 的 `char.ToLowerInvariant(next)` 变异为 `next`（大小写敏感化），以 TRX 报告记录；恢复文件（内容比对确认）后**强制重建**复测。
- **结果（TRX 计数）**：
  - 变异版：`total=10 passed=9 failed=1`——失败项 = `CywApi.Tests.AnsiColorParserTests.ToAnsi_Is_Case_Insensitive` ✅ 测试有效捕获注入缺陷
  - 恢复版（强制重建后）：`total=26 passed=26 failed=0` ✅
- **过程发现 F-007（已登记于 VR-006）**：直接复制恢复后复测仍红——MSBuild 增量构建因恢复文件时间戳旧而跳过重编译（复测跑在变异产物上）。流程加固已写入 CONSTRAINTS.md。
- **映射**：LOG-002（测试有效性）、HLR-007
- **结论**：✅ 通过

## VR-008 覆盖率测量（T）— 2026-11-08 完成

- **命令**：`dotnet test tests\CywApi.Tests -c Release --collect:"XPlat Code Coverage" --results-directory tests\TestResults`（coverlet.collector 6.0.4，cobertura 输出）
- **结果（CywApi.Core 包）**：**行覆盖 81/81 = 100%**；**分支覆盖 66/68 = 97.05%**
- **说明**：插件程序集（0CywApi）不在离服测量范围——其覆盖由实机演示（VR-003/004/005/009）承担（生命周期 README §4）
- **棘轮**：数字已写入 CONSTRAINTS.md 实测记录表（只许升不许降）
- **映射**：HLR-007、BLD-004
- **结论**：✅ 通过（未覆盖 2 分支列入遗留项：空/边界路径补充用例）

## VR-009 实机演示 #4：稳健性（null 配置段 + 全量回归）（D）— 2026-11-08 17:54 完成

- **配置注入**：`debug: true` + `log:`（null）+ `patches: disabled: null` + `compatibility:`（null）——全部为空值形态。
- **证据摘录**：
  ```
  [0CywApi] CYW's API v0.1.0 starting (LabAPI 1.1.7)...          ← 启用正常（含颜色）
  [0CywApi] [compat] game version 14.2.9 is supported
  [DEBUG]   [compat] build info updated
  [0CywApi] [patcher] 0CywApi: +0 applied, 0 failed, 0 skipped   ← disabled:null 被安全处理
  [0CywApi] [modules] 2/2 enabled
  [0CywApi] CYW's API v0.1.0 enabled.
  ```
- **全程无 NullReferenceException**；服务器完整运行至 idle 模式；测试后配置已恢复。
- **回归**：同批执行全量构建（警告清零后复验 0/0）、26/26 单测、部署脚本。
- **映射**：CYWS-CFG-004（演示 ✅）、CYWS-PAT-006 / CYWS-MOD-004（实现 + 审查 + 本批回归）
- **结论**：✅ 通过
