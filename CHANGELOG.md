# Changelog

本项目所有值得注意的变更记录于此；格式遵循 [Keep a Changelog](https://keepachangelog.com/)，
版本号与插件程序集 `Version` / git tag 保持一致，未发布内容统一放在 `[Unreleased]`。

技术基线：**LabAPI 1.1.7** · **SCP:SL 14.2.9** · 插件版本 `0.1.0`。

## [0.1.0] - 2026-11-22

> 本版本（`0CywApi.dll` SHA256 `71CEA7AE72322DCA1099DA6FEA7698AA79C84C65EBF9EBFF132BE9D50AC22A8E`）的 QA-A + QA-A2 合并结论为 PASS，QA-B 为 PASS（VR-017～VR-019）：Windows 仅 zip 干净安装、S1/S3 生命周期、真实第三方共存、LOG-005 与回合重启证据均已归档，发布候选已获批准。

**⚠ Owner action**：本轮无配置键 / 权限节点 / 内容 ID 变更；README 新增「依赖与第三方」、完整配置键表与当前构建透明性说明——手工部署（直接拷贝 DLL）的服务器请对照依赖表核对 `0CywApi.dll`、`CywApi.Core.dll`、`0Harmony.dll` 均已就位。

### Fixed

- 框架版本行现**可靠出现在服务器最终 `buildinfo` / Mod-Description 输出**中（修复注入时机问题）。实测启动输出含 `- CywApi by chuyuewei [0.1.0]: …` 与 `CYW's API v0.1.0`，服务器所有者可直接从启动输出确认框架版本。

### Security

- 日志输出**中和原始控制字符**（ESC / CR / BEL / C1 / DEL 等）：不可信文本无法再向服务器控制台注入终端控制序列或伪造日志行。新增 `CywLog.EscapeData(...)`，供数据字段在嵌入日志前安全转义（日志消息保留换行 LF；数据字段连 CR/LF 一并转义）。

### Changed

- 身份元数据统一为 `chuyuewei`（作者 / 公司 / 版权，含 `LICENSE` 文件）。
- `LabApi` 程序集引用不再 publicize——框架仅针对 LabAPI **公开接口面**编译，明确不依赖框架内部实现。
- 补丁注册表 API 契约已文档化；撤销补丁（unpatch）时输出 INFO 审计行。共享 Harmony ID `cywapi.framework` 为框架保留，第三方插件请勿占用。

### Internal

- 测试用例 26 → 45；`CywApi.Core` 行覆盖 113/113（100%）、分支覆盖 90/92（97.83%）。
- `LangVersion` 单一来源化（`14.0`，由 `Directory.Build.props` 统一钉住）。
- 补丁器失败路径可重试，部分加载时输出诊断信息（partial-load diagnostics）。
- 模块禁用路径加固：禁用不再清空模块注册表，且禁用路径永不抛异常。
