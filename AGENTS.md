# AGENTS.md（代理与协作者须知）

> 仓库结构对标 EXILED：根级仅为元文件（`.github/`、LICENSE、本文件等），全部产品位于单一目录 `CYW.Plugins/`。
> 本文件两部分：**A. 项目规则**（CYW's API 本体，所有协作者与代理、所有任务始终有效）｜ **B. 技能套件与代理团队**（SCP:SL Skills & SubAgents——工作区工具，位于 `.opencode/`，不入公开仓库）。

---

## A. 项目规则（CYW's API）

1. **先读约束**：[`CONSTRAINTS.md`](CONSTRAINTS.md)——质量护栏，不得为使改动通过而弱化。
2. **需求先行**：新行为先登记于 [`CYW.Plugins/docs/lifecycle/requirements.md`](CYW.Plugins/docs/lifecycle/requirements.md)（HLR/LLR + 追溯），再实现；同一提交内补验证证据。
3. **生命周期框架**：规划/验证/配置管理/QA/工具置信度见 [`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/)（DO-178C 等比缩放；非认证声明）。
4. **构建与打包**：`pwsh "CYW.Plugins/build.ps1"`（构建+测试）｜ `pwsh "CYW.Plugins/packaging.ps1"`（产出 `dist/release/` 目录 + 同内容单层 `dist/CYW-Plugins.zip`，解压即部署）；本地部署 `CYW.Plugins/scripts/dev/deploy.ps1`；测试服手册 [`CYW.Plugins/docs/dev/test-server.md`](CYW.Plugins/docs/dev/test-server.md)。
5. **验收判定**：一切工具输出为辅助证据；**最终验收 = 目标环境（真实 SCP:SL 服务器）演示**。证据词汇——*built* = 命令与结果真实执行过；*tested* = 场景 ID + 构建哈希；*faster* = 前后对比数字；否则一律称"未验证"并点名未过检查。
6. **临时代码纪律**：允许为验证注入临时代码，但验证后必须移除并保持工作区干净（流程见验证记录 VR-005/VR-009）。
7. **K1/K2 分级**：运行时功能代码（K1）必须满足需求登记+追溯+离服单测（Core 可达部分）+实机演示；脚本工具（K2）至少评审+执行演示（见生命周期 README §3）。
8. **结构约定**：每个插件/库一个目录 `CYW.Plugins/CYW.X/`；新项目加入根 `CywApi.slnx` 并登记 [`CYW.Plugins/PLUGINS.md`](CYW.Plugins/PLUGINS.md) 后自动参与全量构建；打包分类规则见 `packaging.ps1` 头部。

---

## B. 技能套件与代理团队（SCP:SL Skills & SubAgents）

> 工作区工具位：`.opencode/skills/`（17 个 `scpsl-*` 技能）、`.opencode/agents/`（11 个代理）、根级 `opencode.jsonc`（`default_agent: scpsl-commander`）。`.opencode/` 已被 `.gitignore` 排除（不入公开仓库）；细则见 [`.opencode/README.md`](.opencode/README.md)。

### B1. 目标与版本（Targets — never guess）

- 游戏：SCP:SL **14.2.x** 专用服务器
- 框架：**LabAPI 1.1.7**（官方、随服务端分发）或 **EXILED 9.15.3**（社区）——**同一程序集内绝不混用**。本仓库产品基线为 LabAPI 1.1.7；EXILED 用于对标研究与参考分析。
- 语言/工具链：**C# 14**（`LangVersion=14.0`，`Directory.Build.props` 显式钉住）+ **.NET 10 SDK**（根级 `global.json` 钉住 10.0 特性带）；目标框架 `net48`（SCP:SL/Mono 运行时要求；仅使用其运行时兼容范围内的语言级特性）
- 版本钉子与升级规则：[`.opencode/skills/scpsl-plugin-dev/references/version-pins.md`](.opencode/skills/scpsl-plugin-dev/references/version-pins.md)

### B2. 团队（默认 `scpsl-commander`；委派深度 1）

以 `@scpsl-commander` 启动 SCP:SL 工作（主代理：只规划、委派、集成证据，不直接编辑）；也可 `@` 直达专家（如 `@scpsl-reviewer`）。专家代理**不得向下再委派**：

| 代理 | 职责 |
|---|---|
| `scpsl-planner` | 规格、里程碑、估算、验收标准（只读） |
| `scpsl-labapi-coder` / `scpsl-exiled-coder` | 框架实现（各自只做一个框架） |
| `scpsl-reviewer` | 只读代码审查：Blocker→Nit 分级发现 |
| `scpsl-tester` | 构建、Core 单测、服务器场景证据、缺陷报告 |
| `scpsl-optimizer` | 仅在实测数据支撑下做性能工作 |
| `scpsl-security-auditor` | 只读权限/隐私/输入审计 |
| `scpsl-refactorer` | 行为保持的清理与现代化 |
| `scpsl-documenter` | 面向服主的 README/配置/命令文档、changelog |
| `scpsl-release-manager` | 闸门 G0–G5、打包、部署、移植签署 |

### B3. 全代理不可协商项（Non-negotiables）

1. **技能先行**：动手前用 skill 工具加载匹配的 `scpsl-*` 技能；对照钉住上游（LabAPI 1.1.7 / EXILED 9.15.3）核实每个 API 声明（事件名、属性、命名空间、签名）；无法核实者标注 `UNVERIFIED`——绝不虚构事件、命名空间或签名。
2. **优先级阶梯**：服务器稳定性 > 生命周期/数据完整性 > 安全 > 正确性 > 实测性能 > 可维护性 > 风格（[engineering-standard.md](.opencode/skills/scpsl-plugin-dev/references/engineering-standard.md)）；冲突向上解决，不得静默向下交易。
3. **生命周期对称**：enable 中注册的一切（事件、MEC 协程、Harmony 补丁、自定义角色/物品、单例）disable 必须反注册。
4. **仅主线程**：延迟用 MEC（`Timing`）；禁止 `Thread.Sleep` / `Task.Delay` / 后台线程访问游戏对象；处理器中禁止阻塞 IO。
5. **权限先行**：每个特权命令在产生任何副作用前先校验其文档化权限节点；所有输入必须校验。
6. **证据纪律**：交付按 §A-5 证据词汇；点名未过闸门（G0–G5，[quality-gates.md](.opencode/skills/scpsl-plugin-dev/references/quality-gates.md)）。
7. **一文件集一写入者**：验证者不修被其判定之物；编码者不得自评"已审查/已测试"。
8. **技能维护**：技能规范源为 `skills/`、OpenCode 副本为 `.opencode/skills/`；同步用 `python .opencode/skills/scpsl-agent-orchestration/scripts/sync_opencode_skills.py`（`--check` 供 CI 漂移检查；本工作区未含规范源目录，直接维护副本）。任何技能编辑后校验：`python .opencode/skills/scpsl-plugin-dev/scripts/validate_skills.py .opencode/skills`（应报 17 个技能、0 错误）。

### B4. 在本仓库工作（Working in this repo）

- **脚手架**：`python .opencode/skills/scpsl-plugin-dev/scripts/new_plugin.py --api labapi --name <Name> --output CYW.Plugins/<Name>`；产物须加入 `CywApi.slnx` 并按 §A-8 登记 `PLUGINS.md`。
- **典型流程**：`scpsl-commander` → planner → coder → reviewer → tester →（security-auditor / optimizer 按需）→ documenter → release-manager。
- **参考实现（只读）**：`example/`（LabAPI / EXILED / LabExtended / LocalAdmin 源码副本，不入公开仓库）——用于对标与接口核实；**禁止复制文本入产品**（参考重写纪律）。
- **流程叠加关系**：套件流程叠加在项目生命周期（§A-2 需求登记 / §A-3 VR 记录 / CONSTRAINTS.md 护栏）之上；冲突时以项目规则与 CONSTRAINTS.md 为准，不得弱化。
