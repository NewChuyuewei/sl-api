# CYW's API

> SCP: Secret Laboratory 的 LabAPI 扩展框架 —— 为自有 Mod 与服务端修改提供底层开发体验层。

*An extension framework for the LabAPI modding framework (SCP:SL), built as an independent implementation inspired by [LabExtended](https://github.com/marchellcx/LabExtended). No LabExtended code is included in this project.*

## 状态

🚧 开发中——**Phase 1（核心层）已完成**：配置系统 / 日志 / Harmony 补丁器 / 模块系统 / 兼容性检查均已实现，
并在真实 SCP:SL 14.2.9 服务器上验证（加载、启用、禁用、损坏配置容错全部通过；23 项单元测试全绿）。

- 计划与任务清单：[`tasks/plan.md`](tasks/plan.md) / [`tasks/todo.md`](tasks/todo.md)
- 本地测试服搭建（重要坑位手册）：[`docs/dev/test-server.md`](docs/dev/test-server.md)
- 参考实现功能规格：[`docs/reference/labextended-feature-spec.md`](docs/reference/labextended-feature-spec.md)

## 环境要求

- .NET SDK 8+（建议 10）
- SCP:SL 专用服务器文件（提供 `SCPSL_Data\Managed` 构建引用）
- 环境变量 `SL_REFERENCES` 指向 `SCPSL_Data\Managed`

## 构建

```bash
dotnet build -c Release
```

产物：`src/CywApi/bin/Release/net48/0CywApi.dll`

## 部署

把 `0CywApi.dll` 放进服务端的 `LabAPI\plugins\` 目录。
**勿改名**：`0` 前缀保证它在所有插件之前加载。

## 许可

[MIT](LICENSE)。独立实现，受 LabExtended（作者 marchellcx）启发并致谢。
