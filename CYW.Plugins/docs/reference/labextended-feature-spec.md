# LabExtended 功能规格（参考重写用）

> ⚠️ **纪律声明**：本文档由对 LabExtended v1.4.0.2（`example/LabExtended-master`）的**只读观察**整理而成，仅包含**功能级描述与事实性标识符**（类名、方法名、补丁目标等），**不含任何源码文本复制**。CYW's API 为独立实现，见 README 致谢声明。
> 来源：2026-11-07 全量只读盘点 + 公开 Wiki（github.com/marchellcx/LabExtended/wiki）。
> 用途：CYW's API 各子系统的重写规格；实现与验收时对照本文件。

## 1. 规模与基线事实

- 517 个 .cs / 58,726 行；**单项目**；`AssemblyName=0LabExtended`（0 前缀保证最先加载）；`LoadPriority.Highest`；`IsTransparent=true`
- 游戏兼容范围：**≥ 14.2.8**（版本范围校验，不满足则警告但不崩溃）
- NuGet：Lib.Harmony 2.4.2、Newtonsoft.Json 13.0.4、System.Drawing.Common 10.0.1、BepInEx.AssemblyPublicizer.MSBuild 0.4.3
- 游戏引用（publicized）：Assembly-CSharp、Assembly-CSharp-firstpass、LabApi、NorthwoodLib、Pooling、Snake；普通引用：CommandSystem.Core、Mirror、Mirror.Components、YamlDotNet、Unity.TextMeshPro、UnityEngine + 模块
- 无测试项目、无内嵌资源文件

## 2. 生命周期与加载协议（行为规格）

- 入口 `ApiLoader : LabAPI Plugin`；**Enable()/Disable() 为空**，真正的加载发生在**构造函数**中（早于 LabAPI 加载其它插件）。
- 两阶段流程：
  1. **构造函数阶段**：解析配置目录（LabAPI 配置目录；启动参数 `LabExGlobal` 切换到全局目录）→ 加载 `config.yml` + `api_config.yml` → 游戏版本兼容检查 → 注入 BuildInfo 描述 → 扫描 `[PlayerUpdate]` 与 `[Command]` → preload 级初始化（LabAPI 日志抑制/修复类补丁、SwitchContainer、日志拦截补丁）。
  2. **等待 LabAPI 日志行**（`"[LOADER] Enabling all plugins"`，由 ServerConsole.AddLog 拦截补丁触发）→ 为每个插件程序集注册 updates/commands → 对标记 `[LoaderPatch]` 的程序集应用补丁 → 反射调用各插件的 `ExtendedLoad()` → 按 `[LoaderInitialize(priority)]` 排序调用 → 完整子系统初始化（约 30 个 `Internal_Init` 硬编码顺序）。
- 退出：`ServerConsole.OnApplicationQuit` 拦截 → Quitting 事件 → 可选卸载所有插件（配置开关）。
- **框架插件协议**（供第三方程序集）：`ExtendedLoad()` 反射钩子、`[LoaderInitialize(priority)]`、`[LoaderPatch]`、`LoadPriority` 用法。
- CYW's 对应：标准 `Enable/Disable` 路径 + 显式模块系统（plan D5）；不做构造函数期拦截，除非未来确需修复 LabAPI 自身行为（预留"加载前补丁"增强项）。

## 3. 子系统清单（规模 / 职责 / CYW's 优先级）

| 子系统 | 规模 | 职责要点 | 优先级 |
|---|---|---|---|
| 核心（加载器/配置/日志/补丁器） | Core 25 文件 | ApiLoader、ApiPatcher（反射扫描补丁）、ApiLog（颜色日志）、BaseConfig/ApiConfig（7 个 Section）、Pooling（Array/Dictionary/Object/Wrapper 池） | P0 |
| CywPlayer（原 ExPlayer） | API 直属 10 文件 ≈5.6k 行 | 替换 LabAPI Player 实例；静态 Players/NpcPlayers/AllPlayers/Host；13 个容器；序列化/查询 | P0 |
| 容器 | Containers 9 文件 ≈3.7k 行 | Role/Ammo/Stats/Effect/Inventory/Position/Rotation/Subroutine/Switch | P0 |
| 事件系统 | Events 85 文件 ≈3.7k 行 | 91 个公开静态事件 + InternalEvents 桥（8 个内部事件）；BooleanEventArgs 可取消；InvokeSafe | P0 |
| 玩家更新循环 | Utilities/Update 4 文件 | `[PlayerUpdate]` 扫描 + MonoBehaviour 组件驱动 | P0 |
| 命令框架 | Commands 120 文件 ≈9.7k 行 | CommandManager/Base/Context；特性；12 解析器 + 6 集合包装；15 限制器；5 Token；5 Runner；31 内置命令文件 | P0 |
| 自定义物品 | Custom/Items 4+4 文件 ≈2.5k 行 | CustomItem/TrackedCustomItem（序列级跟踪）/CustomFirearm/CustomProjectile；6 事件 | P1 |
| 自定义角色 | Custom/Roles 471 行 | 配置化角色（血量/外观/规模/物品/出生点） | P1 |
| 自定义队伍 | Custom/Teams 6 文件 ≈1.1k 行 | Handler/Instance/Registry/Timer；分波集成 | P1 |
| 分波角色选择 | Utilities/RoleSelection 9 文件 | RoleSelector/Context/Options/Result；Human/Scp 两套算法 | P1 |
| 自定义能力 | Custom/Abilities 753 行 | Duration/Cooldown/MaxUses/AddOnJoin；玩家 tick 驱动 | P1 |
| 自定义效果 | Custom/Effects 3 文件 | CustomPlayerEffect/Ticking/Duration；按类型注册；反射实例化 | P1 |
| Hints 叠层 | Hints 13 文件 ≈1.9k 行 | HintController（静态）/State/Cache/Data/Processor；元素体系；图片元素 | P1 |
| Settings API | Settings 15 文件 ≈1.9k 行 | Manager/Builder/Menu；8 类条目；3 个补丁 | P2 |
| RemoteAdmin API | RemoteAdmin 10 文件 ≈1k 行 | Controller（每玩家）/Action Provider（伪装 dummy）/Buttons；9 个补丁 | P2 |
| Toys/Prefabs/地图 | Toys 13 文件 ≈2k 行 + Prefabs 2 + 其他 | AdminToy 8 子类 + 3 图元；PrefabDefinition/List；Elevator/ExTeslaGate/Camera/ExMap/ExRound/ExServer | P2 |
| 语音与音频 | Voice ≈1.1k 行 | VoiceController（每玩家）/VoiceThread（后台线程 Opus 编解码）/Profile/变声；4 个无参事件 | P3（可裁剪） |
| 游戏模式 | Gamemodes 327 行 | 可启停；PreventWaveSpawns；IncompatibleGamemodes | P2 |
| 工具层 | Extensions 19 ≈3.9k + Utilities 42 ≈7.4k 行 | FastEvent/ReflectionUtils/PlayerLoopHelper/VersionRange/FakeValue/Mirror 与 Unity 扩展等 | 伴随各阶段 |

## 4. 各子系统行为规格

### 4.1 玩家层（ExPlayer）
- 通过补丁替换 LabAPI 的 Player 实例化（`Player.AddPlayer/RemovePlayer`），使全生态拿到的均为增强子类。
- 静态集合：`Players` / `NpcPlayers` / `AllPlayers` / `Host`；大量 Get/By* 查询。
- 构造时挂接 13 个容器（Role/Ammo/Stats/Effects/Position/Rotation/Inventory/Subroutines/RemoteAdmin/Voice/Hints/PersistentStorage/Toggles）；`Dispose()` 清理。
- **持久化存储**：`PlayerStorage` 按 UserId 索引，跨会话；可选自动加载。
- 假数值支持（如 FakeSpeed 一类伪造读取）。
- CYW's：`CywPlayer : Player` + 构造替换补丁（plan D4 已确认）；容器按优先级分批实现。

### 4.2 事件系统
- 组织：每域一个静态类（如 ExPlayerEvents 41 个事件、MirrorEvents 14、ExMapEvents 13、ExRoundEvents 9… 共 91 个）+ 统一调用模式（静态 event + On* 触发方法 + EventArgs 子类）。
- 可取消事件继承 `BooleanEventArgs`（`IsAllowed`）；单例式事件有 SingletonEventArgs 变体。
- 安全调用：`InvokeSafe`（隔离订阅者异常）；可取消事件返回 `IsAllowed`。
- **桥接**：`InternalEvents` 用反射把处理器插到 LabAPI 事件表**最前**（保证先执行）；验证/生成/离开等走补丁触发；反向也订阅 LabAPI 事件。
- 语音事件**无 EventArgs 类**（调用频率极高，性能考虑）。
- CYW's：`CustomEventsHandler` 官方路径（框架先加载⇒先注册）+ `CywEvents.*`（plan D7）。

### 4.3 命令框架
- 核心：CommandManager（static，Commands 列表、按程序集反射注册、Executed 事件）；CommandBase（Context/Sender/Args/Tokens/Parameters/Respond/Ok/Fail）；ContinuableCommandBase（交互式续命令）；CommandData/Overload/Context/Response/Search。
- 特性体系：CommandAttribute（Name/Permission/Description/IsStatic/IsHidden/TimeOut/Aliases）、CommandOverloadAttribute、CommandParameterAttribute。
- 参数系统：12 个基础解析器（Char/Color/Delegate/Enum/PlayerList/Player/Quaternion/Raycast/String/TimeSpan/Vector2/Vector3）+ 6 个集合包装（Array/Axis/CollectionBase/Dictionary/HashSet/List）；15 个限制器（数值范围 8 种、枚举黑白名单、字符串长度/黑白名单、玩家精度）。
- Token 系统：String/Collection/Dictionary/Property/Method（token 字符可配置）。
- Runner：Regular/Async/Coroutine(MEC)/Continuable/Input。
- 内置命令（31 文件）：customitems/customroles/customteams/customammo/customeffects/gamemode/abilities/holidays/attachments/enum/ghost/gravity/performance/pitch/speed/stamina/toggles/despawn + 子命令组 reload/spawn/text/view。
- RA 接入：补丁把命令表展开注入 RA（嵌套用 `_` 连接）；客户端控制台经修复补丁支持 `.command`；权限走 LabAPI Permissions + 配置（Host/Regex/Override）。
- CYW's：基于 CommandSystem 的属性化框架（plan D8），提供 `RegisterAssembly` 供依赖插件。

### 4.4 自定义内容
- **注册表基类** `CustomObject<T>`（Register/Unregister/Id）。
- **物品**：CustomItem（静态 TrackedItems 按序列号跟踪；数据/权重/拾取丢弃循环；6 事件 Registered/Unregistered/ItemAdded/ItemSpawned/ItemDropped/ItemDestroyed）；TrackedCustomItem（归属玩家/物品实例/自定义 Data）；CustomFirearm（最大弹药/友伤/附件白黑名单/伤害修改）；CustomProjectile（投掷物：碰撞爆炸/锁拾取/引信）。
- **角色**：CustomRole 配置化定义（血量/体力/人工血、Type 与外观、Scale/Gravity、Toggles、物品与自定义物品、出生点、事件回调）；配合分波补丁与 RoleSelection。
- **队伍**：CustomTeamHandler（抽象：SelectRole/IsSpawnable/CanDamage/DespawnAll）+ 泛型实例版；CustomTeamInstance/Registry/Timer；参与分波与友伤判定。
- **能力**：CustomAbility（Duration/Cooldown/MaxUses/AddOnJoin；挂在玩家上由 tick 驱动）。
- **效果**：CustomPlayerEffect（ApplyEffects/RemoveEffects；每玩家独立实例；按类型注册；反射实例化）；CustomTickingEffect（Tick + Delay）；CustomDurationEffect（GetDuration/CheckDuration/RemainingDuration）。
- **弹药**：CustomAmmo（**ID 为 16-bit 无符号整数** —— 跨插件需协调 ID 空间）。
- **游戏模式**：CustomGamemode（可启停、PreventWaveSpawns、IncompatibleGamemodes、运行计时）。

### 4.5 Hints 叠层
- HintController（static）：元素列表、每帧发送控制、临时 hint、`MaxHintTextLength = 65534`、预序列化的空提示（Mirror NetworkWriter）、Pause/Resume/Toggle/ShowHint 扩展。
- 每玩家：HintCache（挂 ExPlayer.Hints）/HintState/HintData（Pool 对象）/HintProcessor（合成 overlay + 元素绘制 + 限长告警）。
- 元素：HintElement（抽象：Builder/OnDraw/OverridesOthers/ShouldParse）→ Static/Dynamic；Personal 变体（PersonalStatic/Dynamic）；PersonalImageElement（图片 hint，System.Drawing + Unity ImageConversion）。
- 拦截原版 hint 显示（HintDisplay.Show 补丁）；默认 500ms 更新间隔、2.5s 显示时长（可配置）。

### 4.6 Settings API
- SettingsManager（static）：AllBuilders、**GlobalSettingsByAssembly（以 Assembly 为 key）**、Version 代理、SyncEntries 合并玩家/全局设置。
- SettingsBuilder / SettingsMenu（抽象）/ ICustomReaderSetting。
- 条目：SettingsEntry 基类 + Group/KeyBind/PlainText/Slider/TextArea/Button/TwoButtons/Dropdown(+Option)。
- 补丁：接管 `ServerSpecificSettingsSync` 的全部同步入口（DefinedSettings getter/setter、SendOnJoinFilter、SendToAll、SendToPlayer ×2、SendToPlayersConditionally）+ 客户端响应处理 + 状态转发。
- 事件：TabOpened/Closed、EntryCreated/Updated、StatusReportReceived。

### 4.7 RemoteAdmin API
- RemoteAdminController（每玩家，IDisposable）：面板开关、请求计时、四种 0 号按钮（Request/RequestIp/RequestAuth/ExternalLookup）、BindObject/UnbindObject、图标前缀常量。
- Actions：RemoteAdminAction / ActionModule / ActionProvider（实现 IRootDummyActionProvider，**伪装成 NPC dummy 的 RA 动作提供者**）。
- 补丁 9 个：patch `NetworkManagerUtils` 的 Dummies 体系（RaDummyActions / DummyActionCollector / ActionDummyCommand）、QueryProcessor、RaPlayerAuth、ExternalLookupCommand、HelpCommand —— **本组最脆弱**（深入第三方库内部实现）。

### 4.8 语音与音频
- VoiceController（每玩家，IDisposable）：在线/说话状态、抓包开关、会话包、Profile 体系、加入事件。
- VoiceThread：**后台线程** Opus 编解码（游戏 `VoiceChat.Codec`）+ 输入/输出并发队列 + GlobalPitch/InstancePitch 变声；主线程每帧输出。
- 补丁：`VoiceTransceiver.ServerReceiveMessage`。
- 事件：4 个高频无参事件（加入/离开/说话/静音）。
- 音频（Wiki 规格）：AudioManager.GetOrAddHandler/GetClip；AudioHandler（AddPlayer/EnsureSpeaker/Transmit/PlayAt/PlayOn）；向 SpeakerToy 发送音频；转发语音包做音频广播。
- 事实：**NVorbis 在全库代码中零引用**（README 残留）；图片/音频等对 System.Drawing 有依赖。

### 4.9 地图/实体包装
- AdminToy（NetworkWrapper<AdminToyBase>，Lookup + Spawn/Despawn）子类：Text/Waypoint/Speaker/Camera/Capybara/Interactable/Light/Target；Primitives：PrimitiveToy/PrimitiveDynamicImage/PrimitiveImageToy/PrimitiveTraceLine。
- Prefabs：PrefabDefinition + PrefabList（静态预定义）。
- Elevator（Wrapper<ElevatorChamber> + IMapObject/位置旋转接口）、ExTeslaGate、Camera（SCP-079）、ExMap（储物柜/房间/拾取物/尸体/种子/灯光/假日）、ExRound（回合计时/状态/Lock 管理与掉线自动解锁）、ExServer（版本/TPS/端口/配额/权限）。
- MirrorMethods：RPC 名称/哈希、dirty bit、属性 writer、直接 SendSpawnMessage（反射/Emit 级底层）。

### 4.10 工具与基础设施
- FastEvent/FastEvents（高性能委托事件）、FastReflection、ReflectionUtils（AppDomain 类型/程序集索引 + AssemblyLoad 订阅）。
- PlayerLoopHelper（自定义 PlayerLoop 插入/裁剪 —— 配置默认移除几十项 Loop 段）。
- VersionRange / RangeFloat / FakeValue / CompressedRotation / UniqueList / ThreadUtils / TimingUtils / ProfilerUtils / PhysicsUtils / WeightUtils / FileUtils / UserIdHelper。
- Firearms 工具 7 文件（模块缓存/属性/附件/音频播放/左轮弹巢）。
- Core/Pooling：ArrayPool/DictionaryPool/ObjectPool/WrapperPool。
- 配置对象：YamlVector3/YamlQuaternion/YamlColor/ConfigEffect。

## 5. Harmony 补丁总清单（92 文件 / 122 处，目标一览）

> 格式：补丁文件/类 → 游戏目标。均为 14.2.x 行为事实，供实现时逐一对照。

**Events/ 事件类（31）**：地图分发（ItemDistributor.ServerRegisterPickup、LockerChamber.SpawnItem/OnFirstTimeOpen、CollisionDetectionPickup.ProcessCollision、TimedGrenadePickup.Update/OnExplosionDetected、StructureDistributor.SpawnStructure）；Mirror（NetworkIdentity.AddObserver/RemoveObserver/ClearObservers、NetworkBehaviour.SerializeServer、NetworkServer.DestroyObject/SpawnObject、SendRPC/SendTargetRPC、SetSyncVar）；玩家（AttachmentsServerHandler.ServerReceiveChangeRequest、Scp330Bag.TryRemove/TryAddSpecific、LiteNetLib4MirrorServer.OnPeerDisconnected、FirstPersonMovementModule.ServerOverridePosition、HitscanHitregModuleBase.ServerAppendPrescan/ApplyDestructibleDamage/ApplyObstacleDamage、ChaosKeycardItem.ServerProcessCustomCmd、PlayerRoleManager.ServerSetRole、Inventory.UserCode_CmdDropItem、LobbyLockCommand/RoundLockCommand.Execute、PlayerAuthenticationManager.ProcessAuthenticationResponse）；回合（RoleAssigner.OnRoundStarted/CheckLateJoin）；SCP（Scp079Recontainer.BeginOvercharge、MimicPointController.ServerProcessCmd/UpdateMimicPoint 等）。

**Fixes/ 修复类（8）**：QueryProcessor.ProcessGameConsoleQuery（客户端命令 "Command not found" 修复）、InvisibleInteractableToy 校验、PlayerAuthenticationManager.RejectAuthentication（本地认证超时）、PickupStandardPhysics.ServerSendFreeze、ReserveAmmoSync.UpdateDelta、InventoryExtensions.ServerRemoveItem、LabAPI 修复×2（PluginLoader.ValidateVersion 空版本修复；Pickup.CreateItemWrapper 日志抑制）。

**Functions/ 功能类（约 53）**：
- 物品：AutomaticActionModule.ServerShoot / DisruptorActionModule / DoubleActionModule.FireLive（弹药消耗）、Inventory.CreateItemInstance、EventManagerModule 层事件、14 处 Weight getter、Pickup 重量体系（5 处）、Inventory.ServerSelectItem。
- 玩家：Inventory.RefreshModifiers、DisarmedPlayers.ValidateEntry、StatusEffectBase.ForceIntensity、HintDisplay.Show、PlayerStats.DealDamage、**Player.AddPlayer/RemovePlayer（ExPlayer 替换入口）**、FpcServerPositionDistributor.LateUpdate、WaveSpawner.CanBeSpawned、NicknameSync.customPlayerInfoString setter、VoiceTransceiver.ServerReceiveMessage。
- RemoteAdmin：RaPlayerAuth.ReceiveData、QueryProcessor.ParseCommandsToStruct、Dummy 体系 6 处、ExternalLookupCommand.Execute、HelpCommand.Execute。
- Settings：ServerSpecificSettingsSync 全家（3 文件）。
- SCP：ZombieConsumeAbility、Scp049SenseAbility、Scp079Recontainer.IsScpButNot079、Scp079TeslaAbility、Scp079 奖励 5 处（TerminationRewards/TeammateProtection/TierManager/HumanBlocking/HidStopped）、Scp096TargetsTracker.IsObservedBy、Scp106Attack.ServerShoot、Scp106PocketItemManager、Scp173ObserversTracker.IsObservedBy、Scp939 4 处（Lunge/EnvironmentalMimicry/Visibility/AmnesticCloud）。
- 其它：ServerConsole.AddLog（**Transpiler，注入日志事件，用作启动钩子**）、RoundSummary.Start、ServerConsole.OnApplicationQuit、Scp956Pinata 假日概率 setter、Scp2536Controller.ServerFindTarget、TeslaGateController.FixedUpdate、NetworkServer.BroadcastToConnection。

## 6. 已知坑与耦合点（实现前必读）

1. **全局静态可变状态**：ExPlayer.Players、HintController.Elements、CommandManager.Commands、SettingsManager 系列——多单例清单需在设计时统一管理生命周期。
2. **初始化顺序硬编码**（约 30 步，含"必须最后执行"的步骤）——我们以模块系统 + Priority 明确化。
3. **RA Dummy 补丁组最脆弱**（深入 NMS 内部），版本漂移风险最高，排期最后且需独立开关。
4. **Settings 以 Assembly 为 key** —— 多插件共享设置注册表，注意隔离。
5. **限制与常量**：hint 文本上限 65534；自定义弹药 ID 是 16-bit；字符串/序列化上限（Mirror）。
6. **掉线竞态防护**：掉线自动解除 Round/Lobby Lock、TokenIpOverride、加入/离开跨事件状态（验证→加入→生成链条）。
7. **补丁禁用语法**：`类名.方法名` 或 `类名.*`（用户配置），重命名类会破坏配置兼容——我们需定义自己的稳定命名策略。
8. **主线程约束**：所有游戏对象操作必须主线程；延迟/循环用 MEC；语音是唯一有意的后台线程场景。
9. **部署事实（2026-11-08 实测）**：官方服务器**不自带** 0Harmony/Newtonsoft；Harmony 需随插件部署（LabAPI `dependencies/` 目录）。我们参考实现用 2.4.2；本框架按技能规范用 **Lib.Harmony 2.2.2**。

## 7. CYW's API 设计差异对照（详见 tasks/plan.md §3）

| 方面 | LabExtended 做法 | CYW's API 设计 | 理由 |
|---|---|---|---|
| 加载 | 构造函数期 + 拦截 LabAPI 日志行 | 标准 Enable + Highest 优先级 | 简单、可维护；黑魔法仅按需引入 |
| 初始化顺序 | 30+ 步硬编码调用 | ICywModule 注册表 + Priority | 可插拔、单模块故障隔离 |
| 事件桥 | 反射把处理器插入事件表最前 | CustomEventsHandler 官方路径 | 官方 API，无反射脆性 |
| 补丁管理 | 全程序集反射扫描 + 配置禁用 | [CywPatch] 显式标记 + 报告 + API 供他方注册 | 更清晰的边界与可观测性 |
| 项目布局 | 单项目 | 插件 + Core（纯逻辑，可单测）+ Tests | 可测试性（参考实现零测试） |
| 许可 | 无 LICENSE 文件 | MIT（自有） | 可公开发布；参考重写规避版权 |
