# AGENTS.md（代理与协作者须知）

1. **先读约束**：[`CONSTRAINTS.md`](CONSTRAINTS.md)——质量护栏，不得为使改动通过而弱化。
2. **需求先行**：新行为先登记于 [`docs/lifecycle/requirements.md`](docs/lifecycle/requirements.md)（HLR/LLR + 追溯），再实现；同一提交内补验证证据。
3. **生命周期框架**：规划/验证/配置管理/QA/工具置信度见 [`docs/lifecycle/`](docs/lifecycle/)（DO-178C 等比缩放；非认证声明）。
4. **构建环境**：`SL_REFERENCES` → 服务端 `SCPSL_Data\Managed`；构建/部署/测试循环见 [`docs/dev/test-server.md`](docs/dev/test-server.md)。
5. **验收判定**：一切工具输出为辅助证据；**最终验收 = 目标环境（真实 SCP:SL 服务器）演示**。
6. **临时代码纪律**：允许为验证注入临时注入，但验证后必须移除并保持工作区干净（流程记录见验证记录 VR-005）。
7. **K1/K2 分级**：运行时功能代码（K1）必须满足需求登记+追溯+离服单测（Core 可达部分）+实机演示；脚本工具（K2）至少评审+执行演示（见生命周期 README §3）。
