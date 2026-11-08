# Constraints（质量护栏）

> 最后评审：**2026-11-08**（DO-178C 优化轮）｜配套过程框架见 [`docs/lifecycle/`](docs/lifecycle/)
> 规则：撰写代码前阅读本文件；**不得为了让某个改动通过而弱化本文件**。
> 收紧（提升标准）应保持安静；放松（降低标准）必须显式评审并登记。

## 楼层（始终强制，无需额外工具）

- **零警告构建**：`dotnet build -c Release` 必须 0 warning / 0 error（net48）
- **无临时/验证代码入库**：`Temp*` 文件与 `TEMP` 标记在验证后必须移除；工作区保持干净（注入→验证→移除流程见验证记录 VR-005/VR-006 F-000）
- **无未登记行为**：K1（运行时）代码的每个行为必须登记于 [`docs/lifecycle/requirements.md`](docs/lifecycle/requirements.md)
- **公开 API 必有 XML 文档**：Release 生成 DocumentationFile；不新增 `#pragma warning disable`
- **无密钥/凭据**入源码、日志与提交信息
- **不得删除/跳过测试**而不在提交信息中说明原因
- **无自由 TODO**：K1 代码中不允许无需求/问题引用的 `TODO` 注释

## 数字护栏（已启用）

| 维度 | 规则 | 检查命令 | 运行时机 |
|---|---|---|---|
| 构建 | 0 警告 0 错误 | `dotnet build CywApi.slnx -c Release --no-incremental` | 每次改动 |
| 单元测试 | 全部通过且数量不得下降（当前 **26**） | `dotnet test tests\CywApi.Tests -c Release` | 每次改动 |
| Core 行覆盖率 | 不得低于实测（当前 **100%**，81/81） | `dotnet test ... --collect:"XPlat Code Coverage"` + 解析 cobertura | 任务结束 |
| Core 分支覆盖率 | 不得低于实测（当前 **97.05%**，66/68） | 同上 | 任务结束 |
| 依赖漏洞（外部） | 无 High/Critical | `dotnet list CywApi.slnx package --vulnerable --include-transitive` | 发布前/CI |
| 实机演示（外部） | 每个基线必须有真实服务器演示证据 | `LocalAdmin` 冒烟流程（见 `docs/dev/test-server.md`） | 每个检查点 |
| 测试有效性 | 每轮优化至少一次变异抽查（注入缺陷必须变红） | 变异→TRX 计数→恢复→**强制重建**→复测 | 每轮优化 |

> 环境前置：`$env:SL_REFERENCES = '<服务端>\SCPSL_Data\Managed'`

## 实测记录（棘轮基准，2026-11-08）

| 指标 | 当前值 | 方向 |
|---|---|---|
| Core 行覆盖 | 81/81 = **100%** | 不得下降 |
| Core 分支覆盖 | 66/68 = **97.05%** | 不得下降 |
| 单元测试数 | **26** | 不得下降 |
| 构建警告 | **0** | 不得上升 |
| 依赖漏洞（High+） | **0** | 不得出现 |

## 例外

| ID | 规则 | 路径 | 原因 | 负责人 | 过期 |
|---|---|---|---|---|---|
| （暂无） | | | | | |

## 关键规则提醒

- **变异恢复必须强制重建**：MSBuild 增量构建会因恢复文件的旧时间戳跳过重编译，导致复测跑在变异产物上（假红/假绿）——恢复后执行 `dotnet build --no-incremental` 或 touch 源文件（发现 F-007）。
- **工具输出均为辅助证据**；最终验收判定 = 目标环境（真实服务器）演示。
