# AGENTS.md — CYW's API 代理与协作者须知

> **适用范围**：本仓库全部任务、全部协作者与代理（人类与 AI 同等适用）。
> **仓库形态**：对标 EXILED——根级仅保留元文件（`.github/`、`LICENSE`、本文件等），全部产品位于 `CYW.Plugins/`。
> **文件构成**：**A. 项目规则**（CYW's API 本体，始终有效）｜ **B. 技能套件与代理团队**（位于 `.opencode/`，随仓库发布）。

**关键词约定**：**必须 / 禁止** = 硬性要求，违反即验收不通过；**应** = 默认执行，偏离须在交付说明中写明理由；**可** = 明确授权但不强制。

---

## 0. 规则优先级与开工前阅读

**冲突仲裁顺序**（高 → 低）：

1. 当前用户指令与本文件 §A
2. [`CONSTRAINTS.md`](CONSTRAINTS.md)（质量护栏）
3. [`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/) 流程文档
4. 本文件 §B（技能套件与代理协议）
5. 代理自身默认行为

冲突时**就高不就低**；禁止以任何低优先级规则弱化高优先级约束。无法仲裁时停工、报告、询问，禁止静默选边。

**任何任务开工前，按顺序阅读**：

1. 本文件
2. [`CONSTRAINTS.md`](CONSTRAINTS.md)
3. 与任务相关的 lifecycle 文档（新行为必读 `requirements.md` 与对应验证记录规范）
4. 任务对应的 `scpsl-*` 技能（见 §B3 路由表）

未完成上述阅读，不得开始改动代码。

---

## A. 项目规则（CYW's API）

### A1. 红线（MUST / MUST NOT）

| #   | 规则                                                                                                                                                                                                       |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **需求先行**：新行为必须先登记于 [`CYW.Plugins/docs/lifecycle/requirements.md`](CYW.Plugins/docs/lifecycle/requirements.md)（HLR/LLR + 追溯关系），再实现；同一提交内补验证证据。无登记号，不写功能代码。  |
| 2   | **护栏不可弱化**：禁止为了让改动通过而放宽 `CONSTRAINTS.md`、测试或检查标准。护栏本身有问题时，单独提交修订并说明，禁止与功能改动混提。                                                                    |
| 3   | **验收在目标环境**：一切工具输出（构建、静态检查、离服单测）仅为辅助证据；**最终验收 = 在真实 SCP:SL 服务器上的演示结果**。                                                                                |
| 4   | **证据词汇强制**：_built_ = 构建命令与结果真实执行过；_tested_ = 场景 ID + 构建哈希可追溯；_faster_ = 有同场景前后对比数字。三者不满足时，一律写「未验证」并逐项点名未通过的检查，禁止用自信措辞替代证据。 |
| 5   | **临时代码纪律**：允许为验证注入临时代码（须以约定标记注释，如 `TEMP-VERIFY`）；验证完成后**必须移除**，并确认 `git status` 干净、无残留标记。                                                             |
| 6   | **参考实现只读**：`example/`（LabAPI / EXILED / LabExtended / LocalAdmin 源码副本）仅用于对标与接口核实；**禁止整段复制文本入产品**，须理解后重写（参考重写纪律）。                                        |
| 7   | **框架不混用**：同一程序集内绝不混用 LabAPI 与 EXILED；本仓库产品基线为 LabAPI，EXILED 仅作对标研究。                                                                                                      |
| 8   | **工作区洁净**：交付时工作区不得遗留未说明的改动、临时文件或调试输出。                                                                                                                                     |

### A2. 基线与工具链（不得猜测、不得自行降级）

| 项                 | 钉住值                                                                                                                         |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------ |
| 游戏               | SCP:SL **14.2.x** 专用服务器                                                                                                   |
| 产品框架           | **LabAPI 1.1.7**（官方、随服务端分发）                                                                                         |
| 对标框架           | EXILED 9.15.3（仅研究/参考分析，不产生产品程序集）                                                                             |
| 语言               | **C# 14**（`LangVersion=14.0`，由 `Directory.Build.props` 钉住）                                                               |
| SDK                | **.NET 10 SDK**（根级 `global.json` 钉住 10.0 特性带）                                                                         |
| 目标框架           | `net48`（SCP:SL/Mono 运行时要求；仅使用其运行时兼容范围内的语言特性）                                                          |
| 版本钉子与升级规则 | [`.opencode/skills/scpsl-plugin-dev/references/version-pins.md`](.opencode/skills/scpsl-plugin-dev/references/version-pins.md) |

任何 API 声明（事件名、参数、命名空间、签名）必须对照上述钉住版本的上游源码/官方示例核实；核实不了的标注 `UNVERIFIED`，禁止凭记忆或旧版本经验（EXILED ≤8、LabAPI 0.x、NwPluginAPI）填写。

### A3. 结构约定

1. 每个插件/库一个目录：`CYW.Plugins/CYW.<Name>/`。
2. 新项目必须同时：加入根 `CywApi.slnx` → 登记 [`CYW.Plugins/PLUGINS.md`](CYW.Plugins/PLUGINS.md) → 方自动参与全量构建与打包。
3. 打包分类规则见 `CYW.Plugins/packaging.ps1` 文件头注释，以脚本为准，不在本文件复述。

### A4. 标准工作流（每个任务的固定顺序）

```text
需求登记(HLR/LLR) → 加载对应技能 → 判断 K 级 → 实现 → build.ps1(构建+测试)
→ 代码审查 → 目标服务器演示 → 补验证证据(VR) → 更新文档/changelog → 交付
```

偏离此顺序（如先写代码后补登记）视为流程缺陷，须在交付说明中申报并补正。

### A5. 构件分级（K1 / K2）

| 级别   | 对象                           | 最低验收要求                                                |
| ------ | ------------------------------ | ----------------------------------------------------------- |
| **K1** | 运行时功能代码（插件本体、库） | 需求登记 + 追溯 + 离服单测（Core 可达部分）+ 真实服务器演示 |
| **K2** | 脚本与工具                     | 至少：代码评审 + 一次真实执行演示                           |

分级依据见 [`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/)（DO-178C 等比缩放；本仓库不作认证声明）。

### A6. 常用命令

| 用途        | 命令                                                                                                            |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| 构建 + 测试 | `pwsh "CYW.Plugins/build.ps1"`                                                                                  |
| 打包发布    | `pwsh "CYW.Plugins/packaging.ps1"`（产出 `dist/release/` 目录 + 同内容单层 `dist/CYW-Plugins.zip`，解压即部署） |
| 本地部署    | `pwsh "CYW.Plugins/scripts/dev/deploy.ps1"`                                                                     |
| 测试服操作  | 见 [`CYW.Plugins/docs/dev/test-server.md`](CYW.Plugins/docs/dev/test-server.md)                                 |

### A7. 交付说明格式（每次交付必须包含，缺项即未完成）

```markdown
## 交付说明

- 变更摘要：<改了什么、为什么>
- 需求追溯：<HLR/LLR 编号 → 实现文件 → 验证证据>
- K 级：K1 | K2
- built：<命令 + 结果，未执行则写未验证>
- tested：<场景 ID + 构建哈希，未执行则写未验证>
- 目标环境演示：<服务器/场景/结果，未执行则写未验证>
- 未通过检查：<逐项列出，无则写"无">
- 工作区状态：<干净 / 遗留项说明>
```

---

## B. 技能套件与代理团队（SCP:SL Skills & SubAgents）

> 工具位：`.opencode/skills/`（17 个 `scpsl-*` 技能）、`.opencode/agents/`（11 个代理）、根级 `opencode.jsonc`（`default_agent: scpsl-commander`）。`.opencode/` 与 `opencode.jsonc` 于 2026-11-09 起随仓库发布（`.mcp.json`、`example/` 仍排除）。细则见 [`.opencode/README.md`](.opencode/README.md)。

### B1. 团队与委派（默认指挥 `scpsl-commander`，委派深度 = 1）

以 `@scpsl-commander` 启动 SCP:SL 工作：其为**主代理（primary）**，只做规划、委派与证据集成，**不直接编辑代码**；也可 `@` 直达专家。专家为 **subagent**，**禁止再向下委派**——跨专家协作必须返回指挥重新分派。

| 代理                     | 调用时机                        | 权限                   | 交付物                    |
| ------------------------ | ------------------------------- | ---------------------- | ------------------------- |
| `scpsl-commander`        | 任何 SCP:SL 任务的入口          | 只规划/委派            | 任务包 + 集成后的交付说明 |
| `scpsl-planner`          | 需求不清、需规格/估算时         | 只读                   | 规格、里程碑、验收标准    |
| `scpsl-labapi-coder`     | LabAPI 实现                     | 可写（限其文件集）     | 代码 + built 证据         |
| `scpsl-exiled-coder`     | EXILED 实现（本仓库仅对标任务） | 可写（限其文件集）     | 代码 + built 证据         |
| `scpsl-reviewer`         | 代码变更后、合并前              | 只读                   | Blocker→Nit 分级评审报告  |
| `scpsl-tester`           | 需验证证据时                    | 仅测试/报告            | 场景证据、缺陷报告        |
| `scpsl-optimizer`        | 有实测瓶颈数据时                | 限实测支撑的改动       | 前后对比数字              |
| `scpsl-security-auditor` | 涉及权限/输入/隐私时            | 只读                   | 审计发现（可追溯路径）    |
| `scpsl-refactorer`       | 行为不变的清理/现代化           | 可写（限不变量清单内） | 不变量核对结果            |
| `scpsl-documenter`       | 发布前与文档任务                | 仅文档                 | 与代码双向核对过的文档    |
| `scpsl-release-manager`  | 打包、部署、移植签署            | 改动须确认             | G0–G5 闸门表 + 发布裁决   |

典型流水线：`commander → planner → coder → reviewer → tester →（security-auditor / optimizer 按需）→ documenter → release-manager`。

### B2. 全代理不可协商项

1. **技能先行**：动手前用 skill 工具加载匹配技能（路由见 §B3），并按 §A2 核实每个 API 声明；`UNVERIFIED` 标注不得在后续环节被转写为「已确认」。
2. **优先级阶梯**：服务器稳定性 > 生命周期/数据完整性 > 安全 > 正确性 > 实测性能 > 可维护性 > 风格（见 [engineering-standard.md](.opencode/skills/scpsl-plugin-dev/references/engineering-standard.md)）；冲突向上解决，禁止静默向下交易（如为性能删权限检查）。
3. **生命周期对称**：在 `enable` 中注册的一切（事件、MEC 协程、Harmony 补丁、自定义角色/物品、单例），必须在 `disable` 中逐项反注册，无一例外。
4. **仅主线程**：延迟一律走 MEC（`Timing`）；禁止 `Thread.Sleep` / `Task.Delay` / 后台线程触碰游戏对象；事件处理器中禁止阻塞 IO。
5. **权限先行**：每个特权命令必须在产生任何副作用**之前**校验其已文档化的权限节点；一切外部输入必须校验与钳制。
6. **一文件集一写入者**：同一时刻同一文件集只允许一个代理写入；验证者（reviewer/tester/security-auditor）不修被其判定之物；编码者不得自报「已审查/已测试」。
7. **闸门不可自免**：质量闸门 G0–G5（见 [quality-gates.md](.opencode/skills/scpsl-plugin-dev/references/quality-gates.md)）未过项必须在交付说明中点名；任何代理无权豁免自己负责的闸门。

### B3. 任务 → 技能/代理路由（最小加载原则：只加载当前阶段所需）

| 任务类型                 | 必载技能                                             | 主责代理                 |
| ------------------------ | ---------------------------------------------------- | ------------------------ |
| 新需求 / 功能规划        | `scpsl-planning`（+`scpsl-architecture` 涉及结构时） | `scpsl-planner`          |
| LabAPI 实现              | `scpsl-labapi` + `scpsl-agent-implementation`        | `scpsl-labapi-coder`     |
| EXILED 对标/实现         | `scpsl-exiled` + `scpsl-agent-implementation`        | `scpsl-exiled-coder`     |
| 自定义角色/物品          | `scpsl-custom-content` + 对应框架技能                | 对应 coder               |
| 无事件可用、需改原版行为 | `scpsl-harmony-patching`（须过其理由闸门）           | 对应 coder               |
| 代码审查                 | `scpsl-code-review` + `scpsl-agent-verification`     | `scpsl-reviewer`         |
| 测试与验收证据           | `scpsl-testing-qa` + `scpsl-agent-verification`      | `scpsl-tester`           |
| 性能问题                 | `scpsl-optimization`（无实测不开工）                 | `scpsl-optimizer`        |
| 权限/隐私/输入           | `scpsl-security-permissions`                         | `scpsl-security-auditor` |
| 清理/升级旧代码          | `scpsl-refactoring`（不变量清单先行）                | `scpsl-refactorer`       |
| 文档/changelog           | `scpsl-documentation`                                | `scpsl-documenter`       |
| 打包/部署/移植           | `scpsl-migration-release`                            | `scpsl-release-manager`  |
| 多专家协调               | `scpsl-agent-orchestration` + `scpsl-plugin-dev`     | `scpsl-commander`        |

### B3.1 Spec Kit 命令自动调度（自 2026-11-28 初始化起）

当前仓库已完成 Spec Kit + OpenCode 初始化。共享基础设施位于 `.specify/`，命令定义位于 `.opencode/commands/speckit.*.md`。AI 在任务符合下表触发条件时，**应主动调用对应流程，无需用户再次输入斜杠命令或另行提醒**；执行前先读取对应命令文件并遵守其中的检查、hook 与输出要求。宿主支持斜杠命令时可直接调用；不支持时必须按命令文件完成等价步骤，不得以此为由停摆。

| 触发条件 | 主动调用的命令 | 目的 |
| --- | --- | --- |
| 首次建立或明确修订项目原则 | `/speckit.constitution` | 建立/更新 constitution，结果须交用户复核 |
| 新功能、新行为或现有规格缺失 | `/speckit.specify` | 形成用户场景与可验收规格 |
| 规格存在影响架构、安全或行为的实质歧义 | `/speckit.clarify` | 在计划前消除关键歧义 |
| 规格已明确，需要技术路线与实施计划 | `/speckit.plan` | 形成架构、文件边界和验证计划 |
| 计划后需检查需求完整性与质量覆盖 | `/speckit.checklist` | 生成质量检查清单（按需） |
| 计划已明确，需要原子任务分解 | `/speckit.tasks` | 生成有序、可执行任务 |
| 任务完成后、实现前需一致性核对 | `/speckit.analyze` | 检查 spec/plan/tasks 与代码的覆盖和冲突 |
| 需求已登记、计划已获批准且实现条件满足 | `/speckit.implement` | 按任务执行实现与验证 |
| 实现后需核对剩余工作并收敛 | `/speckit.converge` | 评估代码与规格差距，补充剩余任务 |

默认顺序：`constitution（一次）→ specify → clarify（按需）→ plan → checklist（按需）→ tasks → analyze → implement → converge`。明显属于既有缺陷修复、纯文档、纯工具安装或已有获批规格的任务，不得机械地重复整套流程。

**自动调用的边界（不可协商）：**

1. Spec Kit 的 spec/plan/tasks **不能替代** §A1-1 的 HLR/LLR 登记。产品代码实现前，必须把规格映射到 `CYW.Plugins/docs/lifecycle/requirements.md` 的需求编号、实现文件和验收场景；验证后映射到 VR 证据。
2. **T3/T4 的计划批准仍是独立门禁**。`speckit.plan/tasks/analyze` 可主动执行，但只有用户已批准当前实现范围后，才可调用 `speckit.implement`。已经批准的 CYW.Printer（`CYWS-HLR-009`、`CYWS-PRN-001～020`）不得因本规则重复发起规格或重新规划，除非用户要求变更。
3. 自动调度不包含自动提交、推送、打标签、正式发布、强制覆盖、修改生产服务器或绕过 G0–G5。涉及这些动作仍须按 §A、§B 与用户授权执行。
4. 命令要求的 hook、检查或输入缺失失败时，必须显式报告并处理；禁止静默跳过、伪造命令结果或将计划场景写成已验证。
5. Spec Kit 流程与既有 commander 团队并行时，仍遵守“一文件集一写入者”、验证者不修代码、计划→实现→审查→QA→文档→发布的既有门禁；冲突按 §0 的优先级仲裁。
### B4. 编码前检查清单（coder 类代理每次必查，并随交付回传结果）

- [ ] 需求登记号已确认（或任务明确非新行为）
- [ ] 目标框架与版本钉子已确认（LabAPI 1.1.7；同程序集不混 EXILED）
- [ ] 文件所有权已在任务包中划定，未越界
- [ ] 新增注册点已规划对应反注册点
- [ ] 特权命令清单及其权限节点已列明
- [ ] 验收场景 ID 已从计划派生（无场景不算可交付）

### B5. 技能维护（本仓库模式）

- 本仓库**直接维护** `.opencode/skills/` 副本（仓库内未含规范源 `skills/` 目录）；需要双份维护的工作区再使用同步脚本：`python .opencode/skills/scpsl-agent-orchestration/scripts/sync_opencode_skills.py`（`--check` 供 CI 漂移检查）。
- 任何技能编辑后必须校验：`python .opencode/skills/scpsl-plugin-dev/scripts/validate_skills.py .opencode/skills`（预期：17 个技能、0 错误）；校验未过不得提交。
- 脚手架：`python .opencode/skills/scpsl-plugin-dev/scripts/new_plugin.py --api labapi --name <Name> --output CYW.Plugins/<Name>`，产物随后走 §A3 登记流程。

### B6. 套件与项目规则的关系

套件流程（§B）**叠加**于项目生命周期（§A）之上，用于提高执行质量；**一切冲突以 §A 与 `CONSTRAINTS.md` 为准**。典型叠加：需求登记（§A1-1）先于 `scpsl-planning` 产出；VR 验证记录既是 §A 证据，也是 `scpsl-tester` 的交付物载体。

---

## C. 停工与上报条件（任一触发即停，禁止带病推进）

1. 新行为无需求登记号，且当前任务未包含补登记。
2. 所需 API 在钉住版本中无法核实，而该 API 承载核心行为（非边缘装饰）。
3. 目标服务器不可用，导致 K1 任务无法完成目标环境演示——可继续实现，但交付必须标注「未验证」并停在未验收状态。
4. 两位以上专家结论冲突且按优先级阶梯无法仲裁。
5. 发现必须弱化护栏（§A1-2）、变更不变量（配置键/权限节点/内容 ID/插件前缀）或突破文件所有权才能继续。
6. 校验工具（`validate_skills.py`、`build.ps1` 护栏检查）报错且根因不在本次任务范围。

停工报告格式：触发条件编号 → 已完成部分 → 阻塞证据 → 需要谁（用户/指挥/专家）做什么决定。
