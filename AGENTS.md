# AGENTS.md（代理与协作者须知）

> 仓库结构对标 EXILED：根级仅为元文件（`.github/`、LICENSE、本文件等），全部产品位于单一目录 `CYW.Plugins/`。

1. **先读约束**：[`CONSTRAINTS.md`](CONSTRAINTS.md)——质量护栏，不得为使改动通过而弱化。
2. **需求先行**：新行为先登记于 [`CYW.Plugins/docs/lifecycle/requirements.md`](CYW.Plugins/docs/lifecycle/requirements.md)（HLR/LLR + 追溯），再实现；同一提交内补验证证据。
3. **生命周期框架**：规划/验证/配置管理/QA/工具置信度见 [`CYW.Plugins/docs/lifecycle/`](CYW.Plugins/docs/lifecycle/)（DO-178C 等比缩放；非认证声明）。
4. **构建与打包**：`pwsh "CYW.Plugins/build.ps1"`（构建+测试）｜ `pwsh "CYW.Plugins/packaging.ps1"`（全量打包为一个 `dist/CYW-Plugins.zip`，解压即部署）；本地部署 `CYW.Plugins/scripts/dev/deploy.ps1`；测试服手册 [`CYW.Plugins/docs/dev/test-server.md`](CYW.Plugins/docs/dev/test-server.md)。
5. **验收判定**：一切工具输出为辅助证据；**最终验收 = 目标环境（真实 SCP:SL 服务器）演示**。
6. **临时代码纪律**：允许为验证注入临时代码，但验证后必须移除并保持工作区干净（流程见验证记录 VR-005/VR-009）。
7. **K1/K2 分级**：运行时功能代码（K1）必须满足需求登记+追溯+离服单测（Core 可达部分）+实机演示；脚本工具（K2）至少评审+执行演示（见生命周期 README §3）。
8. **结构约定**：每个插件/库一个目录 `CYW.Plugins/CYW.X/`；新项目加入根 `CywApi.slnx` 后自动参与全量构建；打包分类规则见 `packaging.ps1` 头部；工程职责登记于 [`CYW.Plugins/PLUGINS.md`](CYW.Plugins/PLUGINS.md)。
