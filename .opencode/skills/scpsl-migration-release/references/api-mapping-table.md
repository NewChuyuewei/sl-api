# API Mapping Table — LabAPI 1.1.7 ↔ EXILED 9.15.3

Use for porting inventories. "≈" means *similar purpose, different contract — re-verify*.

## Lifecycle and identity

| Concern | LabAPI | EXILED |
|---|---|---|
| Base class | `LabApi.Loader.Features.Plugins.Plugin` / `Plugin<TConfig>` | `Exiled.API.Features.Plugin<TConfig>` (`+ <TConfig, TTranslation>`) |
| Enable / disable | `Enable()` / `Disable()` | `OnEnabled()` / `OnDisabled()` (+ `OnReloaded()`) |
| Pre-enable config load | `LoadConfigs()` (built into `Plugin<TConfig>`) | Loader-populated `Config` before `OnEnabled` |
| Identity | abstract `Name`, `Description`, `Author`, `RequiredApiVersion`; virtual `Version` | `Name`, `Prefix`, `Author`, `Version` (assembly-derived defaults) |
| Load order | `Priority` (`LoadPriority` enum) | `Priority` (`PluginPriority`) |
| Version gate | `RequiredApiVersion` (vs `LabApiProperties.CompiledVersion`) | `RequiredExiledVersion` (compiled-against default) |
| Enable switch | loader `properties.yml` (no config key) | `Config.IsEnabled` (loader-enforced) |
| Logging | `LabApi.Features.Console.Logger.Info/Warn/Error/Debug` | `Exiled.API.Features.Log.Info/Warn/Error/Debug` |

## Events

| Concern | LabAPI | EXILED |
|---|---|---|
| Subscription | `CustomEventsHandler` overrides + `CustomHandlersManager.RegisterEventsHandler` (preferred) or static `LabApi.Events.Handlers.PlayerEvents.Joined += …` | `Exiled.Events.Handlers.Player.Verified += …` with explicit `-=` |
| Args namespace | `LabApi.Events.Arguments.<Domain>Events` | `Exiled.Events.EventArgs.<Domain>` |
| Player joined ≈ | `PlayerEvents.Joined` (`PlayerJoinedEventArgs`) — join moment | `Player.Verified` (`VerifiedEventArgs`) — post-authentication; **not the same moment as Joined** |
| Round lifecycle ≈ | `ServerEvents` round/wave events (`WaveRespawned`, …) | `Handlers.Server.WaitingForPlayers`, `RoundStarted`, `RoundEnded`, `RespawningTeam` |
| Cancellation | `IsAllowed` on cancellable (`-ing`) args | `IsAllowed` on cancellable (`-ing`) args — **defaults/mutability differ per event; re-check each** |
| SCP-914 ≈ | `Scp914Events` | `Handlers.Scp914` (`Activating`, `UpgradingPlayer`, `UpgradingPickup`, …) |

## Objects, commands, misc

| Concern | LabAPI | EXILED |
|---|---|---|
| Player wrapper | `LabApi.Features.Wrappers.Player` (`DisplayName`, `SendBroadcast`, `SendHint`, `SetRole`) | `Exiled.API.Features.Player` (`Nickname`/`DisplayName`, `ShowHint`, `Broadcast`, `AddItem`) |
| Map/server | `LabApi.Features.Wrappers` (Server/Facility/Doors/Rooms…) | `Exiled.API.Features.Map`, `Server`, `Round`, `Cassie` |
| Commands | `ICommand` + `[CommandHandler(typeof(RemoteAdminCommandHandler))]`; auto-discovered | Same `CommandSystem` shape; auto-registered by `OnRegisteringCommands` |
| Parent commands | `ParentCommand`, children `[CommandHandler(typeof(Parent))]` | `ParentCommand` + `LoadGeneratedCommands()` registration |
| Permissions | `sender.HasPermission("node")` (`LabApi.Features.Permissions`) | `sender.CheckPermission("node")` (`Exiled.Permissions.Extensions`) |
| Coroutines | MEC `Timing` (identical discipline) | MEC `Timing` |
| Custom roles/items | DIY registry (scpsl-custom-content) | `Exiled.CustomRoles` / `Exiled.CustomItems` first-class APIs |
| Config file | per-plugin `config.yml` in LabAPI configs folder | section under `Prefix` in `Configs/<port>-config.yml` |
| Config class | plain class, properties-only serialization | `IConfig` (`IsEnabled`, `Debug` + `[Description]` properties) |
| Game assemblies | plain `Assembly-CSharp.dll` | publicized `Assembly-CSharp-Publicized.dll` (dev) |

## Porting worksheet (copy per plugin)

```text
Feature: ____________________
Old event/API: ______________  ->  New event/API: ______________
Cancellation semantics checked in pinned docs? [ ]  Arg properties re-verified? [ ]
Config keys old -> new: ____________________  Owner migration note written? [ ]
Commands/permissions kept identical node strings? [ ]
Custom content: framework API adopted / DIY registry planned? [ ]
Harmony patches: gate re-run, still needed in target framework? [ ]
Parity scenarios passed (join/round/trigger/death/restart/reload): [ ]
```
