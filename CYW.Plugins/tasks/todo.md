# 任务清单：CYW's API

> 配套计划：`tasks/plan.md`
> 图例：P0/P1/P2/P3 = 优先级；S/M/L = 规模（1-2 / 3-5 / 5+ 文件）
> Phase 0–2 已细化；Phase 3+ 滚动细化（见计划 §5.3）｜ **v1.1 增补 Phase 7–9**（EXILED 对标子系统 / 自研增强工具 / 退役验收）

---

## Phase 0：环境与骨架

### Task T0.1：服务端文件获取（P0，S，依赖：无）✅
**描述**：~~steamcmd 匿名下载~~ → 实际改经 **Steam 客户端**安装专用服务器到 `E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server`（Valve CDN 直连超时所致，见进度日志）。

**验收**：
- [x] `SCPSL_Data\Managed\LabApi.dll` 存在（**1.1.7.0** ✓）
- [x] 关键 DLL 版本已记录：LabApi 1.1.7.0；**0Harmony/Newtonsoft 服务器不自带**（Harmony 由插件自带，采用 Lib.Harmony 2.2.2）
- [x] 服务端可启动：经 `LocalAdmin.exe 7777 --acceptEULA --useDefault --printStd` 完整启动并稳定运行（2026-11-08 验证）

**验证**：`Test-Path` 检查 + `(Get-Item <dll>).VersionInfo`；启动服务端读日志头部。
**备选方案**：Steam 客户端"库→工具"安装；或用户提供 Managed 文件夹路径。

### Task T0.2：构建引用环境变量（P0，S，依赖：T0.1）
**描述**：设置用户级环境变量 `SL_REFERENCES=D:\SCPSL-Server\SCPSL_Data\Managed`；新终端生效。

**验收**：
- [x] 用户级 `SL_REFERENCES` 已写入注册表（指向 Steam 安装的专用服务器 `SCPSL_Data\Managed`）
- [x] 路径下存在 `Assembly-CSharp.dll`、`LabApi.dll`（1.1.7.0）、`Mirror.dll`、各 UnityEngine 模块（2026-11-08 验证）

**验证**：`echo $env:SL_REFERENCES` + `Test-Path` 清单核对。

### Task T0.3：仓库骨架与 build spike（P0，M，依赖：T0.2）
**描述**：在 `CYW's API/` 建立 `CywApi.sln` + 三个项目（`CywApi` 插件 / `CywApi.Core` 纯逻辑 / `CywApi.Tests`），配好 net48、`Publicize="true"` 引用、NuGet（Lib.Harmony 对齐服务器版本、Newtonsoft.Json 13.x、BepInEx.AssemblyPublicizer.MSBuild）；`git init`（待确认）。

**验收**：
- [x] `dotnet build -c Release` 全绿（net48）——2026-11-08 全解决方案 0 警告 0 错误；**构建期 publicize 方案验证通过**
- [x] 产物 DLL 生成（AssemblyName=0CywApi）
- [x] Core 项目无任何游戏/LabAPI/Unity 引用（引用清单核对）

**验证**：构建命令输出；`dotnet list package`；csproj 评审。
**备注**：这是"构建期 publicize 原始 DLL"方案的验证点；若失败，改用脚本预生成 publicized 变体。

### Task T0.4：最小插件入口 + 服务器加载冒烟（P0，S，依赖：T0.3）
**描述**：`CywPlugin` 入口：`Name="CywApi"`、`LoadPriority.Highest`、`Enable/Disable` 对称、启动 banner 日志（版本+LabAPI 版本）。

**验收**：
- [x] 服务器日志出现完整加载链（2026-11-08）：`Successfully loaded 0Harmony 2.2.2` → `Successfully loaded CywApi.Core 0.1.0` → `Successfully loaded CywApi` → `[0CywApi] CYW's API v0.1.0 enabled (LabAPI 1.1.7)`
- [x] 无异常/错误（仅 LabAPI 的 transparently-modded 提示，属预期——`IsTransparent=true`）
- [x] 禁用周期验证（properties.yml `is_enabled: false` → 插件零运行时行为，2026-11-08 实测 ✓；热重载 N/A——LabAPI 1.1.7 无热重载机制）

**验证**：服务器日志（从第一行读起）；冒烟循环第 1 次记录。

### Task T0.5：CI 工作流（P0，S，依赖：T0.3）
**描述**：`.github/workflows/build.yml`：ubuntu-latest + steamcmd 匿名下载 + build Release + 上传 artifact；先落地本地，远端建立后启用。

**验收**：
- [x] YAML 语法通过（本地校验：`yaml.safe_load` → OK）
- [x] 绿色运行：**run #2（`823944d`）success** ✓（含修复后的安装链路 + 缓存保存 + 构建 + 26 单测 + artifact）

**验证**：workflow 运行记录；artifact 下载检查。

### Task T0.6：参考规格沉淀（P0，M，依赖：无，可并行）
**描述**：依据盘点报告整理 `docs/reference/labextended-feature-spec.md`：子系统清单、行为描述、补丁目标（方法名级别的**事实信息**）、已知坑与耦合点。作为重写规格；不含任何源码文本复制。

**验收**：
- [x] 覆盖 §5.2 全表 + 92 补丁文件的目标方法清单（2026-11-08 完成初版）
- [x] 明确标注"行为参考，非代码复制"纪律

**验证**：与盘点报告交叉核对；计划修订时回填引用。

### Checkpoint CP0
- [x] 本地 Release 构建绿（0 警告 0 错误 + 单测 1/1 通过）
- [x] 服务器加载干净（插件成功加载并启用；卸载/重载周期后续补测）
- [x] CI 就绪并**已验证**：修复首跑问题后 run #2（`823944d`）全绿 ✓（含服务端拉取/缓存/构建/单测/artifact）
- [x] 👤 人工评审通过（2026-11-08）→ 已进入 Phase 1

---

## Phase 1：核心层

### Task T1.1：CywConfig（P0，S，依赖：T0.4）
**描述**：`config.yml` 基础：Debug、真彩色日志开关、补丁禁用表、版本兼容检查开关、存储路径等分区骨架；损坏配置回退默认值+告警。

**验收**：
- [x] 首次启动生成默认 `config.yml`（`configs/7777/CywApi/config.yml`，实测生成 ✓）
- [x] 人工破坏 YAML 后插件仍正常加载（用默认值）并输出告警（实测：`CONFIG_LOADER` 错误 + `using default values` 告警 + 插件照常启用 ✓）
- [x] `SaveConfig()` 往返正确（实测存盘后完整结构：log/patches/compatibility 分区 + snake_case ✓）

### Task T1.2：CywLogger（P0，S，依赖：T1.1）
**描述**：日志封装：INFO/WARN/ERROR/DEBUG、颜色代码、来源程序集前缀、Debug 门控；纯格式化逻辑下沉 Core。

**验收**：
- [x] DEBUG 仅在 Debug=true 输出；颜色开关生效（实测：debug:true 时 `[DEBUG]` 行出现、默认时消失；ANSI 转义在服务器输出中生效 ✓）
- [x] Core 内格式化单测（AnsiColorParser 7 项测试 ✓）

### Task T1.3a：CywPatcher — 扫描与应用（P0，M，依赖：T1.1）
**描述**：`[CywPatch]` 标记扫描（Harmony 前缀/后缀/Transpiler/Finalizer）、按 Harmony ID 应用、补丁计数器。

**验收**：
- [x] 示例补丁类被正确应用（临时 SelfTestPatch 打 `CywPlugin.Name` getter：`patch hit` ×2 + `+1 applied` 实测 ✓；验证后临时补丁已删除）
- [x] 异常补丁不拖垮其它补丁（逐类 try/catch + 失败计数 + 错误日志；运行期样例待首个真实补丁联测）

### Task T1.3b：CywPatcher — 撤销/禁用/报告（P0，M，依赖：T1.3a）
**描述**：`UnpatchAll`/按类撤销、配置禁用匹配（`类名.方法名`、`类名.*`）、启动时补丁清单报告。

**验收**：
- [ ] 禁用配置生效且可在日志中确认（运行时样例待首个真实补丁；匹配逻辑已单测覆盖）
- [x] 禁用插件时补丁全部还原（`UnpatchAll(HarmonyId)` 仅禁用路径调用、对称实现；LabAPI 1.1.7 无热重载，运行时断言留待重载机制联测）
- [x] 匹配逻辑的 Core 单测（CywPatchFilter 11 项 ✓）

### Task T1.4：模块系统（P0，S/M，依赖：T1.1）
**描述**：`ICywModule`（Name/Priority/OnEnable/OnDisable）+ 注册表；确定性顺序；单模块失败记日志并继续。

**验收**：
- [x] 注册/反注册对称；重载后无重复模块（实测 `[modules] 2/2 enabled`、按优先级顺序 ✓；重复注册有类型守卫）
- [x] 故意抛异常的测试模块不阻断其它模块启用（逐模块 try/catch 实现完成；运行期样例随后续联测）

### Task T1.5：版本兼容与 banner（P0，S，依赖：T0.4）
**描述**：`CywVersion` 常量、游戏版本范围校验（≥14.2.x）、LabAPI RequiredApiVersion、启动 banner 注入 BuildInfo。

**验收**：
- [x] 版本不在范围时输出醒目警告（不崩溃）（范围内实测 `is supported` 日志 ✓；越界告警路径 + VersionRange 单测覆盖）
- [x] `BuildInfo` 中可见框架版本行（注入经 `build info updated` 日志确认 + 幂等防重；输出字面随后续控制台联测复核）

### Task T1.6：Core 第一批单测（P0，S，依赖：T1.2/T1.3b）
**描述**：xUnit 工程落地：版本范围、禁用匹配、日志格式化等纯逻辑测试。

**验收**：
- [x] `dotnet test` 全绿（**23/23** ✓）；CI 中执行待远端启用

### Checkpoint CP1
- [x] 模块/补丁/日志/配置全部可用，`dotnet test` 绿（23/23 ✓）
- [x] 服务器冒烟循环（多次启动/禁用/坏配置容错全部干净、无残留；"完整回合"需真实玩家加入，留待后续）
- [ ] 👤 **人工评审 ← 等待确认**

---

## Phase 2：玩家与事件层

### Task T2.1：CywPlayer 注册表 + 构造替换补丁（P0，M，依赖：CP1）
**描述**：`CywPlayer : Player` 骨架 + `Players/Npcs/All` 注册表 + 构造/移除补丁（替换 LabAPI Player 实例化）+ 释放清理 + 静态查询（Get/ByUserId/TryGet…）。

**验收**：
- [ ] 进服后 `CywPlayer.Get/All` 可见该玩家；退服后移除
- [ ] 加载/重载后 instanceof 判定正确（外部拿到的 Player 即 CywPlayer）
- [ ] 无空引用异常（断线竞态防护）

### Task T2.2a：CywPlayer 容器 — 角色/背包基础（P0，M，依赖：T2.1）
**描述**：`RoleContainer`（角色/血量基础）、`InventoryContainer`（物品基础）最小实现，挂接生命周期清理。

**验收**：
- [ ] 角色变更事件后容器数据一致；退服清理无泄漏

### Task T2.2b：CywPlayer 容器 — 位置/旋转/效果基础（P0，M，依赖：T2.1）
**描述**：`PositionContainer`/`RotationContainer`/`EffectContainer`（基础查询与开关）。

**验收**：
- [ ] 位置读写、效果开关冒烟通过（RA 命令或临时调试输出验证）

### Task T2.3：事件桥 InternalEvents（P0，M，依赖：T2.1）
**描述**：`CustomEventsHandler` 注册：认证/验证/加入/离开/生成/角色变更/回合（开/结束/重启）/服务器事件；维护内部状态（UserId→Region 等）。

**验收**：
- [ ] 事件按预期顺序触发（加入→验证→生成→…）；记录日志序列核对
- [ ] 重载后无重复订阅（同一事件只触发一次）

### Task T2.4：Cyw 事件系统（P0，M，依赖：T2.3）
**描述**：`CywEvents.*` 静态事件 + `CywEventArgs`/`BooleanEventArgs`（IsAllowed）+ `InvokeSafe` 异常隔离；首批事件集（玩家 Joined/Left/Verified/Spawned/ChangedRole/Died 等 + 回合/服务器）。

**验收**：
- [ ] 依赖方插件可订阅并取消（BooleanEventArgs 示例验证）
- [ ] 订阅者抛异常不影响其它订阅者与主流程

### Task T2.5：玩家更新循环 `[CywPlayerUpdate]`（P0，S/M，依赖：T2.1）
**描述**：扫描特性注册每玩家 tick（参考 PlayerUpdateHelper），统一组件驱动；启停对称。

**验收**：
- [ ] 示例 update 每秒正确回调；禁用后停止；重载不重复

### Checkpoint CP2
- [ ] 冒烟：进服→CywPlayer 可见→事件序正确→退服清理→重载无重复
- [ ] 👤 人工评审

---

## Phase 3+（滚动细化，进入时拆分为 T 级任务）

### Phase 3：命令框架（P0）
- [ ] 3.1 命令注册与调度（`[CywCommand]` 扫描、`CywCommandManager`、`RegisterAssembly`）
- [ ] 3.2 参数解析器：基础（字符串/数字/布尔/枚举/时间）
- [ ] 3.3 参数解析器：复杂（玩家/向量/颜色/射线/集合）
- [ ] 3.4 限制器（数值范围/枚举白黑名单/字符串长度/玩家精度）
- [ ] 3.5 Runner（同步/异步/协程/续命令）
- [ ] 3.6 嵌套命令组 + 权限节点集成（LabAPI Permissions）
- [ ] 3.7 三面接入（RA / 客户端控制台 / 服务器控制台）+ RA 命令表注入
- [ ] 3.8 内置命令组骨架（后续随内容子系统补齐）
- [ ] CP3：RA+客户端命令可用

### Phase 4：自定义内容（P1）
- [ ] 4a 自定义物品基类+序列级跟踪器 → 枪械 → 投掷物
- [ ] 4b 自定义角色 → 自定义队伍 → 分波角色选择算法
- [ ] 4c 自定义效果（含 tick/时长变体）→ 能力 → 弹药
- [ ] 4d 游戏模式基类
- [ ] CP4a/b/c 各自冒烟验收

### Phase 5：呈现与交互（P1/P2/P3）
- [ ] 5a Hints 叠层（控制器/元素/图片/拦截补丁）
- [ ] 5b Settings API
- [ ] 5c RemoteAdmin API（最脆弱补丁组，慎重）
- [ ] 5d Toys/Prefabs/电梯/特斯拉/地图/相机包装
- [ ] 5e 语音/音频（P3，可裁剪）

### Phase 6：打磨与发布
- [ ] 6.1 修复类补丁按需移植（如客户端自定义命令修复）
- [ ] 6.2 文档：README / API XML / 使用教程
- [ ] 6.3 示例插件（examples/）
- [ ] 6.4 发布清单核对（技能 scpsl-migration-release 的 release-checklist）
- [ ] 6.5 v0.1 公开发布

### Phase 7：EXILED 对标子系统（v1.1 新增，P1/P2，见计划 §1.5）
- [ ] 7a 权限：节点/组/继承（配置驱动）→ LabAPI 权限集成 → 命令框架对接（`CYW.Permissions`）
- [ ] 7b 名牌/头衔展示（`CYW.NameTag`，对标 CreditTags）
- [ ] 7c 加载器生态协议：互操作 API 程序集拆分评审（promotion rule）→ 第三方依赖解析（`CYW.Loader`）
- [ ] 7d 安装/升级工具（`CYW.Installer`）
- [ ] CP7：M7 四项（Permissions/NameTag/Loader/Installer）交付并实机验证 → §1.5 矩阵必须项自此齐备

### Phase 8：自研增强工具（v1.1 新增，P2/P3，按需取舍）
- [ ] 8.1 Profiler（性能剖析：模块/补丁/事件订阅者耗时）
- [ ] 8.2 Scheduler（调度器：任务队列 / tick 分片）
- [ ] 8.3 NetOptimizer（网络优化）
- [ ] 8.4 其余按需取舍：MemoryManager / EntityManager / Printer / LoadController（评审决定去留）
- [ ] CP8：按需交付清单确定，交付项实机验证

### Phase 9：EXILED 退役验收（v1.1 新增，P0，发布 v1.0）
- [ ] 9.1 §1.5 退役验收清单逐项签收（每项链接 VR 证据）
- [ ] 9.2 自有 Mod 套件在纯 LabAPI 服务器完成 ≥1 场完整对局（无 EXILED）
- [ ] 9.3 配置/权限迁移对照表（EXILED 设置 → CYW 设置）
- [ ] 9.4 性能基线对比（装载 EXILED 前后：TPS / 内存不劣化 >5%）
- [ ] 9.5 v1.0 发布（替代 EXILED 里程碑）
- [ ] CP9：退役清单全过 → v1.0

---

## 备注
- 每完成一个任务立即勾选并在会话中汇报；检查点须人工确认后再前进。
- 任何任务发现规模超预期时：暂停→拆分→更新本文件。
- 服务器冒烟循环（固定动作）：`启动 → 插件加载 → 完整回合 → 重载 ×3 → 禁用 → 读日志从第一行起`。

## 进度日志

- **2026-11-08**
  - T0.1：服务端经 Steam 客户端装至 `E:\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server`（LabApi 1.1.7.0 ✓）；实测服务器**不自带** 0Harmony/Newtonsoft；服务端已验证可稳定运行（详见 T0.4）。
  - T0.2：✅ 完成。`SL_REFERENCES` 指向该服务器 Managed。
  - T0.3：✅ 完成。`CywApi.slnx` + 3 项目；NuGet 切华为镜像（9MB/s）；全解决方案 Release 构建 0 警告 0 错误；Core 单测 1/1 通过。
  - T0.6：✅ 初版完成（`docs/reference/labextended-feature-spec.md`）。
  - T0.4：✅ 完成。2026-11-08 12:06 复测日志确认完整加载链：`0Harmony 2.2.2` + `CywApi.Core 0.1.0` 依赖加载 → `CywApi` 插件加载 → `[0CywApi] CYW's API v0.1.0 enabled (LabAPI 1.1.7)`，服务器稳定运行（180s+ 无异常）。
  - **CP0 达成**（除人工评审）：环境、骨架、构建、部署、加载全链路当日打通；git 仓库已初始化并完成初始提交。
  - 启动攻关结论（2026-11-08）：Windows 下必须用 `LocalAdmin.exe [port] --acceptEULA --useDefault --printStd` 启动；直接启动 `SCPSL.exe` 会在启动序列后自行退出且 LabAPI 不初始化。
  - 产出：`docs/dev/test-server.md`（测试服手册）、`scripts/dev/deploy.ps1`（部署脚本）。
  - 网络实录：直连 Valve CDN 超时；本机代理仅 15KB/s；华为 NuGet 镜像 9MB/s；Steam 客户端通道正常。
- **2026-11-08（Phase 1 完成）**
  - T1.1–T1.6 全部实现并实机验证：配置分区/容错/存盘往返、日志门控/ANSI、补丁器（应用+计数+报告）、模块系统、版本校验 + BuildInfo 注入、23 项单测。
  - 实测记录：坏 YAML → 默认值 + 照常启用 ✓；`properties.yml` 禁用 → 零运行时行为 ✓；临时补丁命中日志 ✓（验证后已移除）；配置存盘完整往返 ✓。
  - 提交：`ed745df feat(core): Phase 1 core layer`（18 文件 +769 行）。
  - 遗留联测项：异常模块/异常补丁运行期样例、越界版本警告样例、BuildInfo 输出字面、热重载取消补丁（LabAPI 无热重载）。
- **2026-11-08（DO-178C 对标优化轮，Skills + MCP）**
  - 生命周期数据：`docs/lifecycle/{README,requirements,verification-records}.md`（适用性声明/目标映射/CM/QA/工具置信度；HLR 8 + LLR 25 + 双向追溯；VR-001~009 证据档案）；仓库根新增 `CONSTRAINTS.md`（可机械检查护栏）+ `AGENTS.md`。
  - 五轴审查：7 项发现——F-001~F-004 修复（配置空值防护/解析器可空性/补丁幂等/模块运行态守卫）、F-005 采纳（EnabledCount 应用）、F-006/F-007 记录（F-007=变异恢复的增量构建陷阱 → 流程加固）。
  - 验证动作：变异抽查（TRX：红 9/1 命中目标测试 → 恢复 → 强制重建 → 绿 26/26）；覆盖率实测 **Core 行 100%（81/81）、分支 97.05%（66/68）**（棘轮入 CONSTRAINTS）；依赖漏洞 0 高危；VR-009 空配置段实机 + 全量回归通过。
  - 追溯落地：12 个源文件加 `<remarks>Requirements:`、测试加 `[Trait("Req")]`、测试总数 26。
  - **全部联测遗留项关闭**（异常隔离/越界警告/BuildInfo 字面 ✓ + 坏配置/禁用 ✓）；剩余："完整回合"需真实玩家（可选）。
  - 基线标签：`v0.1.0-phase1`（注释标签）。
  - **CI 首跑失败修复**（同日）：根因=steamcmd 首次运行仅自举自我更新、链式指令未执行 → refs 目录缺失；次要隐患=失败时可能把空 refs 目录缓存。修复：工作流改为「先自举一次 → 安装循环 + `LabApi.dll` 存在性验证（3 次重试）→ 缓存 restore/save 分离 + key 升级 `v2`」。
  - **CI 修复验证通过**：run #2（commit `823944d`）**全绿**（104 秒：服务端拉取 + 缓存保存 + 构建 + 26 单测 + artifact 上传）；修复前 run #1 的失败已成为历史。远端仓库：`github.com/NewChuyuewei/sl-api`（public）。
  - **Action 版本升级**（消除 Node20 弃用警告）：`checkout@v7` / `setup-dotnet@v6` / `cache(restore|save)@v6` / `upload-artifact@v7`（以 GitHub API 查得的最新主版本为准）；推送后由 run #3 验证（同时验证缓存命中路径）。
- **2026-11-08（EXILED 对标整合版仓库）**
  - 仓库根上移（`.git`/`.github`/`CywApi.slnx`/README/AGENTS/CONSTRAINTS 位于根）；全部产品归入 `CYW.Plugins/`（对标 EXILED 的 `EXILED/` 产品目录）。
  - **重命名 `CYW's.Plugins` → `CYW.Plugins`、`CYW's.NameTag` → `CYW.NameTag`**：发现 F-008——撇号路径劈断 MSBuild item-transform（`MSB3021` 实锤），目录名从此禁止撇号（规则入 CONSTRAINTS）。
  - 新增（对标 EXILED）：`Directory.Build.props`（共享属性 + 统一输出 `bin/<Configuration>` + Release 警告即错误）、`build.ps1`、`packaging.ps1`（**一个 zip 解压即部署**）、`PLUGINS.md`（15 个工程分级注册表）、14 个 `.gitkeep`。
  - `deploy.ps1` 重写为"打包 + 解压"单一真相源（修复 F-009 参数绑定 bug）。
  - CI 改为：全量构建（警告即错误）→ 26 单测 → `packaging.ps1` → 单包 artifact `cyw-plugins`。
  - 本地全链路验证 ✅：构建 0/0、26/26、zip 内容正确（plugins:1 / deps:2）、解压落位正确。
- **2026-11-08（单层 artifact 修复，F-010）**
  - 用户报告：打包 zip 套娃（需解压两次）→ 根因：CI 上传预打包 zip，artifact 服务再包一层。
  - 修复：CI 改上传 `dist/release` 目录（下载即单层 zip）；`packaging.ps1` 双形态输出（`dist/release/` 真相源 + `dist/CYW-Plugins.zip` 分发副本）；`deploy.ps1` 改为目录复制（免解压往返）。
  - 本地复验：打包内容正确、目录落位正确；提交 `daf5d04` 并推送；F-010 登记（VR-006 表 + VR-010 补充记录）。
  - 远端复核（2026-11-09）：runs #5（`daf5d04`）/#6（`719a174`）全绿；下载 run #5 artifact 解包验证单层（根 = `plugins/`+`dependencies/`+README）→ **F-010 闭环**。
- **2026-11-08（计划 v1.1：轻量级综合 API / 替代 EXILED）**
  - `plan.md` v1.1：新增 §1.5（替代语义 + 轻量级量化约束表 + 程序集拆分 promotion rule + EXILED→CYW 能力矩阵 + 退役验收清单）；里程碑增补 **M7–M9**；§5.2/§5.3/§6/§8 同步增补；非目标明确"EXILED 二进制兼容不做，功能级替代是目标"。
  - 本文件：增补 Phase 7（EXILED 对标子系统）/ Phase 8（自研增强工具）/ Phase 9（退役验收）。
  - `PLUGINS.md`：状态列改阶段标注（M1–M9）。
- **2026-11-09（AGENTS.md 融合 + 工具链明确）**
  - `AGENTS.md`：并入 SCP:SL Skills & SubAgents 使用规范（A 项目规则 / B 技能套件与代理团队：B1 目标与版本、B2 团队表、B3 不可协商项、B4 工作方式）。
  - 工具链显式钉住：**C# 14**（`LangVersion=14.0`）+ **.NET 10 SDK**（根级 `global.json`，10.0 特性带）；目标框架保持 `net48`。
  - `.opencode/`（17 技能 + 11 代理）与 `opencode.jsonc` 解除忽略、随仓库发布（发布前安全扫描通过）。
