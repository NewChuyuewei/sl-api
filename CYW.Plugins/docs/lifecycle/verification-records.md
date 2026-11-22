# 验证记录（Verification Records）

> 文档编号：**CYWS-VR-001** ｜ 更新：2026-11-22
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
| F-008 | **Critical（构建阻断）** | 目录名含撇号（`CYW's.Plugins`）劈断 MSBuild item-transform（`@(DocFileItem->'$(OutDir)...')` 的单引号被展开路径中的撇号截断）→ `MSB3021` 构建失败 | ✅ 已修复：全仓库目录去撇号（`CYW.Plugins` / `CYW.NameTag`），品牌名仅保留在显示元数据；规则写入 CONSTRAINTS.md |
| F-009 | Required（脚本） | `deploy.ps1` 以数组 splat 传参，`-SkipBuild` 被位置绑定为 `Configuration` 值 → 打包失败 | ✅ 已修复：改为条件直接调用；执行演示复测（打包+解压落位）通过 |
| F-010 | Required（分发） | CI artifact 双层 zip：上传预打包 `CYW-Plugins.zip` 被 artifact 服务再包一层 → 用户下载后需解压两次（用户报告发现） | ✅ **已闭环**：CI 改上传 `dist/release` 目录；`packaging.ps1` 双形态输出；`deploy.ps1` 目录复制（提交 `daf5d04`）。**远端复核（2026-11-09）：run #5 artifact 下载解包 = 单层（根即 plugins/dependencies/README）** |

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

## VR-010 结构迁移（EXILED 对标整合）与打包链路（K2 执行演示）— 2026-11-08

- **变更范围**：仓库根上移（`.git` / `.github` / `CywApi.slnx` / README / AGENTS / CONSTRAINTS 位于根）；全部产品迁入 `CYW.Plugins/`（对标 EXILED 的 `EXILED/` 产品目录模式）；新增 `Directory.Build.props`（共享属性 + **统一输出 `bin/<Configuration>`**）、`build.ps1`、`packaging.ps1`；CI 改为全量构建 + **单包 artifact**。
- **命令与结果（本地实测）**：
  - `dotnet build CywApi.slnx -c Release --no-incremental`（警告即错误）→ **0 警告 / 0 错误**
  - `dotnet test CywApi.slnx -c Release --no-build` → **26/26 通过**（解决方案级运行 ✓）
  - 共享输出目录（构建产物全集）：`0CywApi.dll` / `0CywApi.xml` / `0Harmony.dll` / `CywApi.Core.dll` / `CywApi.Core.xml`——无构建工具泄漏；publicizer 照常运行（`obj/…/publicized` 已生成，`ExcludeAssets=runtime` 未破坏其功能）
  - `pwsh packaging.ps1 -SkipBuild` → `dist/CYW-Plugins.zip`（388 KB）：`plugins/global/0CywApi.dll` + `dependencies/global/{CywApi.Core.dll, 0Harmony.dll}` + `README-INSTALL.txt`
  - `deploy.ps1 -SkipBuild` → 解压落位 `LabAPI\plugins\global` + `dependencies\global` ✓
- **本批发现（执行演示捕获）**：F-008（撇号路径劈断 MSBuild，Critical，已修复）、F-009（deploy 参数绑定 bug，已修复）。
- **映射**：CYWS-BLD-001/002/003/004、HLR-008（可复现环境）。CI 远端的全量绿灯将作为推送后的补充证据。
- **结论**：✅ 通过（CI 远端复验后最终关闭）
- **补充：F-010 修复后复验（2026-11-08）——单层 artifact 链路**：
  - `pwsh packaging.ps1 -SkipBuild` → `dist/release/`（`README-INSTALL.txt` + `plugins/global/0CywApi.dll` + `dependencies/global/{CywApi.Core.dll, 0Harmony.dll}`）+ `dist/CYW-Plugins.zip`（同内容分发副本）
  - `deploy.ps1 -SkipBuild` → 目录复制落位 `LabAPI\{plugins\global, dependencies\global}` ✓（免解压往返）
  - CI：`upload-artifact` 改上传 `CYW.Plugins/dist/release` → 下载的 artifact 即单层（根为 `plugins/ + dependencies/`）
  - 修复提交 `daf5d04`；**远端复核（2026-11-09）**：run #5（`daf5d04`）/ run #6（`719a174`）全绿；下载 run #5 artifact 解包 → 根即 `plugins/` + `dependencies/` + `README-INSTALL.txt`（**单层确认**）——该发现闭环

## VR-011 质量加固战役 QA 轮：构建 / 单测 / 覆盖率（T）— 2026-11-15 13:10

- **基线**：战役代码（`fc25d5a` + `90ae9c4` + `c7da147`）
- **命令与结果**：
  - `dotnet build CywApi.slnx -c Release --no-incremental` → **0 警告 / 0 错误**
  - `dotnet test CywApi.slnx -c Release --no-build` → **45/45 通过**（36 Fact + 1 Theory 含 9 InlineData，静态清点与运行时计数精确一致）
  - 覆盖率（coverlet/cobertura）→ **行 113/113 = 100%**；**分支 90/92 = 97.83%**（较基线提升；未覆盖 2 点 = `CywPatchFilter` 历史缺口，已登记 CONSTRAINTS）
  - 双次 `--no-incremental` 重建字节一致；最终工件 SHA256 `8C744868C0F2896C06641D843EF76852373AEB0F2D49B99D70C030D810965030`（bin=dist=部署=zip 条目）
- **补充（CI 外部证据）**：runs #11（`fc25d5a`）/ #13（`c7da147`，含全部战役代码）/ #14（`77d4d52`）/ #15（`045777c`）全绿
- **映射**：BLD-004、HLR-007、CONSTRAINTS 棘轮（45 / 113-113 / 90-92）
- **结论**：✅ 通过

## VR-012 实机演示 #5：S1 启动+启用+CMP-003 修复验证（D）— 2026-11-15 13:36

- **环境**：SCP:SL 14.2.9 专用服务器（LocalAdmin 7777，headless，0 玩家）
- **证据摘录**：
  - 加载链 `Successfully loaded 0Harmony / CywApi.Core / CywApi`
  - `Successfully enabled 'CywApi', Version: 0.1.0, Author: 'chuyuewei'`（身份变更实机可见）
  - `[compat] plugins enabled` 行后 **`build info updated` 出现 2 次** = CMP-003 后置重注入执行的双标记
  - 启动 Mod Description 块含 `- CywApi by chuyuewei [0.1.0]: ...` 与 `CYW's API v0.1.0`（最终输出含注入行，用户可见半达成）
  - `[modules] 2/2 enabled`；全日志零异常
- **映射**：CMP-003（修订后）、CMP-004（IsTransparent 现状符合裁决）、身份元数据
- **结论**：✅ 通过

## VR-013 实机演示 #6：S2 LOG-005 活体探针 + 清理证明（D）— 2026-11-15 17:14

- **方法**：`TEMP-VERIFY` 探针（ESC/CR/BEL/C1/LF 构造串）→ 构建/部署/启动捕获 → `git checkout` 移除 → 重建 → 清洁证明（依 A1-5 纪律）
- **证据摘录**：
  - 探针输出行（节选）：`TEMP-PROBE esc[?[31m] cr[?] bel[?] c1[\u009B] lf-ok`（第二行 `TEMP-PROBE line2`，LF 保留）
  - 归因裁决：C1 以字面 `\u009B` 呈现 = 插件层净化实锤；ESC/CR/BEL 显示为 `?`（内存为控制图形符 U+241B 等，单测证明；GB2312 控制台写出层对不可映射字形降级）。**安全属性达成**：零原始控制字节、零异常；"统一为 \uXXXX 字面表示"列为可选精化
  - 清理证明：树洁净；`TEMP-VERIFY`/`TEMP-PROBE` 在源码/部署/bin/dist/zip 全零；重建 SHA256 与 dist/部署/zip 条目字节一致
- **映射**：LOG-005（D 半）、CONSTRAINTS 临时代码纪律
- **结论**：✅ 通过

## VR-014 实机演示 #7：S3 禁用周期（D）— 2026-11-15 18:06 / 18:16

- **步骤 A（is_enabled: false）**：插件加载但零运行时输出（无 banner/modules/patcher/compat 行）、零异常
- **步骤 B（恢复 true）**：完整启用链回归（banner → compat 14.2.9 → modules 2/2 → enabled，Author: chuyuewei）；零异常；无 TEMP-PROBE 输出（清洁构建确认）
- **映射**：HLR-001（禁用边界）、G1 生命周期家族、MOD-005 禁用路径回归
- **备注**：LabAPI 1.1.7 无热重载，周期为跨两次启动语义
- **结论**：✅ 通过

## VR-015 独立复评：RV3（R）— 2026-11-15

- **范围**：批次 A+B 全部修复对照 requirements v1.1；历史发现 RV2-01 至 RV2-17 / SEC1 全项处置核对
- **结论摘要**：**Approve-with-fixes**；修复逐项 verify 通过；2 Minor（RV3-01 注释措辞，修复于 `cda3d58` 且重建字节恒等；RV3-02 措辞建议，免处理）；无 Blocker；生命周期对称判定通过
- **按设计保留开放**：RV2-07（配置文档以 XML 注释承担）、RV2-17（补丁层离服测试按 D14 不做）、SEC1-03（异常日志策略文档化，随文档轮）
- **UNVERIFIED 要点**：`example/` 为 master 线索 vs 发布版 1.1.7（由 VR-012 实机裁决）；Harmony API 形状以用法一致性核实
- **结论**：✅ 通过（含 RV3-01 修复闭环）

## VR-016 澄清记录：config.yml 注释机制 + 工件哈希基线（A）— 2026-11-15

- **S4 澄清（闭合 RV2 评审 UNVERIFIED#1）**：读取部署产物 `configs/7777/CywApi/config.yml`（134 字节）：LabAPI 1.1.7 生成的配置不含任何注释（XML 文档注释与 Description 均不渲染；纯键值序列化）。键说明职责改由 README 配置表承担（后续文档项）。
- **工件哈希基线澄清**：构建为同 HEAD 确定性（受控实验：同一 HEAD 下 csproj 注释变更前后字节恒等 `47144720...`）；哈希随提交变化系因 SDK 将 HEAD SHA 嵌入 `AssemblyInformationalVersion`。三个证据工件 `D7D621A1...`（HEAD `77d4d52`）/ `8C744868...`（HEAD `045777c`）/ `47144720A47D2824C153EAB235D88EA602ED1861EF8270B2CEFCB3B4D13792C3`（HEAD `5adb18a`）之间**零 .cs 变化**（git diff --name-status 逐段核实）→ 行为等价；发布候选将在 RM 阶段以最终 HEAD 重新构建并实机冒烟认证。
- **映射**：RV2 UNVERIFIED#1（闭合）、QA/CI 哈希一致性方法论
- **结论**：✅ 记录归档

## VR-017 最终候选 QA-A：保留证据、构建/覆盖率与 S1/S3（T/D/R）— 2026-11-22

- **候选基线**：HEAD / source baseline `8522c39bae624b3b413740f4a6e7afe7c9a7249a`。
- **最终候选 SHA256**：
  - `0CywApi.dll`：`71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`
  - `CywApi.Core.dll`：`59F3E101B4AFA985EBB9D234B9F24C9D3E3764D7CFD92320E23D02EB6EF92FC3`
  - `0Harmony.dll`：`0DE0118C8F1D4408DE389CA33B46D2FF7778F3A8541B430CAE729EC913D899C7`
- **保留证据复核**：Release 构建 **0 警告 / 0 错误**；单元测试 **45/45 通过**；Core 行覆盖 **113/113（100%）**、分支覆盖 **90/92（97.83%）**。`bin` / `release` / zip / 已部署载荷身份完全一致。
- **实机场景**：S1 启动/启用与 S3 禁用/恢复均通过，零非预期插件异常。S3 仅是跨启动禁用/恢复回归，不替代 MOD-005 未启用状态 `DisableAll` 的定向演示。
- **证据位置**：`%TEMP%\opencode\qa-a-8522c39`；后续干净安装补证见 `%TEMP%\opencode\qa-a2-8522c39`（VR-018）。
- **评审结论说明**：QA-A 保留证据评审原判 **PARTIAL**，唯一缺口是原始干净安装 transcript 缺失；该缺口已由 QA-A2 的 `00-clean-install-transcript.log` 闭合。QA-A + QA-A2 合并结论为 **PASS**。
- **映射**：BLD-001/004、HLR-007、CMP-003（S1）、HLR-001、MOD-005（仅 S3 回归，定向缺口保持开放）、最终候选身份核验。
- **结论**：✅ 保留证据范围内通过；与 VR-018 合并后 QA-A 干净安装证据链完整。

## VR-018 最终候选 QA-A2：Windows 干净服务器仅 zip 安装（D）— 2026-11-22

- **候选**：`0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`；发布 zip SHA256 `2C69FBD21F17799E10A480947421711BAE1CB10424F5EED3C9A720C3EA426834`。
- **方法**：备份并清空目标目录后，仅执行 `[System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $labRoot)` 将 `CYW-Plugins.zip` 解压到 LabAPI 根目录；未使用 `deploy.ps1`，未从 `bin` 复制文件。随后启动服务器验证安装。
- **结果**：Windows 干净服务器仅 zip 安装与启动验证通过，零非预期插件异常；与 VR-017 合并关闭 QA-A 的原始 transcript 缺口。
- **证据位置**：`%TEMP%\opencode\qa-a2-8522c39`
  - `00-clean-install-transcript.log`：干净安装完整 transcript。
  - `01-extracted-files-and-hashes.txt`：解压文件与哈希。
  - `02-s1-stdout-gb2312.log`：S1 启动输出。
  - `03-verification-summary.txt`：验证摘要。
- **边界**：本记录仅证明 Windows 解压与干净安装；非 Windows 解压仍为 **UNVERIFIED**。
- **映射**：HLR-008、Windows 仅 zip 干净安装发布边界、QA-A 证据缺口闭合。
- **结论**：✅ 通过（verified-on-clean-server，Windows）。

## VR-019 最终候选 QA-B：第三方共存、LOG-005 与回合重启（D）— 2026-11-22

- **候选**：`0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`。
- **共存对象**：真实第三方 ServerQoL `1.0.0.0`，SHA256 `BA466B2ACDCD6EA883493745C6BF2817AA8355EE7F4BB7800200AE25F5E740A7`；仅引用 LabAPI 1.1.7，无 EXILED / Harmony 依赖。外部测试专用 CywQaProbe SHA256 `9DF595250DA1A5BE1FA10627C89A29C03BC5C309A7F9C19839150457BAAB1C96`，仅用于验证，**从未随发布包分发**。
- **场景结果**：QAB-BOOT-01、QAB-CMP003-01、QAB-COEX-01、QAB-LOG005-01、QAB-ROUND-01、QAB-CLEAN-01 全部 **PASS**。
- **LOG-005 原始载荷计数**：ESC=0、CR=0、BEL=0、C1=0；LF 保留。GB2312 控制台显示 `?` 替换及字面 `\u009B`，与既有控制台降级归因一致。
- **回合重启**：恰好一次 round-restart 请求与一次 `RoundRestarted` 观察；第二次 `WaitingForPlayers` 未产生新请求，ServerQoL 保持启用。
- **清理证明**：最终恢复为仅 CYW 的部署与配置；最终候选哈希不变；无测试插件、测试进程或测试标记残留；QA-B 清理时仓库干净。
- **证据位置**：`%TEMP%\opencode\qa-b-8522c39`
  - `QA-B-report.md`、`03-la-test-stdout.log`、`04-log-analysis.txt`、`05-probe-byte-proof.txt`、`06-cleanup-final.txt`。
- **范围排除**：多玩家与玩家进出、性能、ServerQoL 玩法规则、非 Windows 解压、交互式桌面渲染均未验证。此次使用 headless LocalAdmin、0 玩家，未使用可见窗口。
- **映射**：CMP-003、LOG-005、真实第三方共存、回合重启、最终候选清理与身份保持。
- **结论**：✅ 在上述范围内通过；零非预期插件异常。

## VR-020 v0.1.0 正式发布（R/G5）— 2026-11-22

- **发布基线**：release commit `3ecfe36332a899ae08aab16ed5bde4c9f9617e62`；发布说明 `releases/v0.1.0.md`。本次发布未重建、未修改产品文件或发布工件。
- **CI 证据**：Build & Package run #22，run ID `44991469250`，结果 **success**：https://github.com/NewChuyuewei/sl-api/actions/runs/44991469250
- **标签证据**：annotated tag `v0.1.0`，tag object `9ee15e5e761b90ddf431fd2f448726d9f5114a4d`，目标 commit `3ecfe36332a899ae08aab16ed5bde4c9f9617e62`；已推送至 origin。
- **GitHub Release**：release ID `454255335`，名称 `CYW's API v0.1.0`，`draft=false`、`prerelease=false`：https://github.com/NewChuyuewei/sl-api/releases/tag/v0.1.0
- **唯一发布资产**：`CYW-Plugins.zip`，asset ID `778160003`，大小 **390117 bytes**：https://github.com/NewChuyuewei/sl-api/releases/download/v0.1.0/CYW-Plugins.zip
- **哈希同一性**：远端 API 资产 digest 与下载文件实测 SHA256 均为 `2C69FBD21F17799E10A480947421711BAE1CB10424F5EED3C9A720C3EA426834`；与 VR-018 认证发布 zip 哈希一致。
- **zip 内容核对**：恰好四项：`README-INSTALL.txt`、`plugins/global/0CywApi.dll`、`dependencies/global/CywApi.Core.dll`、`dependencies/global/0Harmony.dll`。内嵌候选 `0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`；Core `59F3E101B4AFA985EBB9D234B9F24C9D3E3764D7CFD92320E23D02EB6EF92FC3`；Harmony `0DE0118C8F1D4408DE389CA33B46D2FF7778F3A8541B430CAE729EC913D899C7`，均与最终候选身份一致。
- **独立复核**：Publication RM 裁决 **COMPLETE**。Commander 独立查询 release `454255335`，确认 `draft=false` / `prerelease=false` 与远端 digest；本地标签及下载文件哈希亦相符。
- **证据位置**：`%TEMP%\opencode\release-v0.1.0`
  - `07-release.json`：release 创建结果。
  - `08-asset.json`：资产上传结果。
  - `09-release-and-assets.json`：release 与资产最终查询。
  - `10-download-verification.txt`：下载、内容条目与哈希核验。
  - `11-final-state.txt`：发布后最终状态。
- **最终状态**：发布时仓库干净，已部署候选不变；无测试插件或测试进程，证据中无凭据。VR-017～VR-019 已登记的范围排除与开放项保持不变，本记录不扩大实机验证范围。
- **结论**：✅ **v0.1.0 发布完成（publication-complete）**；CI、标签、Release、唯一资产及远端/下载哈希证据链闭合。

## VR-021 定向验证：MOD-005 / PAT-007 / PAT-008 / PAT-009（D）— 2026-11-22

- **冻结候选**：已发布候选 `0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`，验证前后保持不变；未重建任何 CYW 源码或二进制，未修改产品或发布工件。
- **环境**：SCP:SL **14.2.9**、LabAPI **1.1.7**、Harmony **2.2.2**；headless LocalAdmin、**0 玩家**。
- **场景结果**（行号均指 `10-final-server-stdout-gb2312.log`）：

  | 场景 | 需求 | 实测结果 | 日志行 | 结论 |
  |---|---|---|---|---|
  | D-MOD-005 | CYWS-MOD-005 | 使用全新隔离模块管理器；未启用状态 `DisableAll` 的警告已捕获，无异常，注册表保持 **1**；随后 enable/disable 生命周期完整完成 | 232 | ✅ PASS |
  | D-PAT-007 | CYWS-PAT-007 | 首次类型发现抛出 `InvalidOperationException`；程序集保持未标记且计数为零。重试应用 **1** 个补丁，目标计数变为 **2**；第三次调用无操作，发现计数为 **2** | 233–240 | ✅ PASS |
  | D-PAT-008 | CYWS-PAT-008 | 真实 missing-base 依赖触发 WARN，包含 `1 loader exception(s)` 与首条异常消息；可用补丁成功应用，目标计数变为 **4** | 241–246 | ✅ PASS |
  | D-PAT-009 | CYWS-PAT-009 | 数值序列 **`0 → 10 → 11 → 10 → 0`**；INFO 审计准确报告 **1** 个受影响方法；外部 owner 的补丁在 CYW 撤销后存活；最终 QA 注册计数为 **0** | 247–251 | ✅ PASS |

- **外部测试工程与汇总**：三个外部 harness 项目均构建 **0 警告 / 0 错误**；定向场景汇总 **`passed=4 failed=0`**。这些工程仅用于验证，不构成 CYW 产品重建。
- **清理证明**：LabAPI 备份已恢复；plugins / dependencies / configs 清单差异均为 **0**；仅保留原候选依赖；无 QA 工件或 QA 进程残留；仓库干净；无 release 或 tag 变更。
- **证据位置**：`%TEMP%\opencode\qa-directed-mod-pat`
  - `QA-directed-report.md`：完整定向验证报告。
  - `10-final-server-stdout-gb2312.log`：最终服务器输出及上述行号证据。
  - `11-cleanup-final.log`：清理结果。
  - `13-post-report-state.txt`：报告后的最终状态。
- **UNVERIFIED（Mono 归因）**：自定义 PAT-007 Assembly wrapper 附近出现一次非致命 Mono `gpath.c:115: assertion 'filename != NULL' failed`。其来源与根因未确定；该输出未阻止重试、未引发托管异常，也未影响任何场景断言。**不将其认定为 CYW 缺陷，也不视为已解释。**
- **范围边界**：可选非法标量配置强制转换未运行；本任务未执行无关重建或玩家场景。本记录仅关闭上述四项定向验证缺口，不扩大其他验证范围。
- **映射**：CYWS-MOD-005、CYWS-PAT-007、CYWS-PAT-008、CYWS-PAT-009。
- **结论**：✅ 四项定向实机演示全部通过，对应定向验证债务关闭；冻结候选与发布状态不变。
