# LabAPI 1.1.7 — API Map

## Namespaces you will actually use

| Namespace | Contents |
|---|---|
| `LabApi.Loader.Features.Plugins` | `Plugin`, `Plugin<TConfig>` (plugin entry points) |
| `LabApi.Loader.Features.Plugins.Enums` | `LoadPriority` (`Highest` … `Lowest`, default `Medium`) |
| `LabApi.Features` | `LabApiProperties` (`CompiledVersion`, `CurrentVersion`) |
| `LabApi.Features.Console` | `Logger` (`Info`, `Warn`, `Error`, `Debug`, `Raw`) |
| `LabApi.Features.Wrappers` (+ subfolders) | `Player`, server, map/facility, doors, rooms, items, pickups, hazards, AdminToys wrappers |
| `LabApi.Events.Handlers` | Static event classes: `PlayerEvents`, `ServerEvents`, `WarheadEvents`, `Scp914Events`, per-SCP classes (`Scp049Events`, `Scp079Events`, `Scp096Events`, `Scp106Events`, `Scp173Events`, `Scp939Events`, `Scp3114Events`, …), `ObjectiveEvents`, `ScpEvents` |
| `LabApi.Events.Arguments.*` | Event args per domain (`PlayerEvents.PlayerJoinedEventArgs`, `ServerEvents.…`, …) |
| `LabApi.Events.CustomHandlers` | `CustomEventsHandler` (abstract, partial), `CustomHandlersManager.RegisterEventsHandler` / `UnregisterEventsHandler` |
| `LabApi.Features.Permissions` | Permission checks/providers (`HasPermission` on command senders, custom provider API) |
| `MEC` | `Timing.CallDelayed`, `Timing.RunCoroutine`, `CoroutineHandle`, `Timing.KillCoroutines` |
| `CommandSystem` | `ICommand`, `ParentCommand`, `CommandHandlerAttribute`, `RemoteAdminCommandHandler`, `ClientCommandHandler`, `GameConsoleCommandHandler` |

## Plugin base class (pinned source: `LabApi/Loader/Features/Plugins/Plugin.cs`)

Abstract members every plugin provides: `Name`, `Description`, `Author`, `RequiredApiVersion`,
`Enable()`, `Disable()`.
Virtual members: `Version` (defaults to assembly version since 1.1.7), `Priority` (`LoadPriority.Medium`),
`IsTransparent` (`false`), `LoadConfigs()` (called before `Enable()`; `Plugin<TConfig>` uses it
to load YAML). Loader also sets `Properties` (from `properties.yml`) and `FilePath`.

`Plugin<TConfig>` (`Plugin{TConfig}.cs`): `Config` property (never null after load; defaults on
broken YAML + warning), `ConfigFileName` (default `"config.yml"`), `SaveConfig()`.
Config classes: **properties only** — fields are skipped by the serializer.

## Loader behaviour worth knowing

- Lifecycle order: assembly load → plugin instantiation (sorted by `LoadPriority`) →
  `properties.yml` check → `LoadConfigs()` → commands auto-registered from the assembly →
  `Enable()`. 1.1.7 also fires `PluginsEnabled` after all plugins are enabled.
- Commands in the plugin assembly are discovered automatically; there is no `RegisterCommands`
  call to write in LabAPI 1.1.7.
- Plugins are expected to interact via wrappers/events; direct base-game access is possible
  (you reference `Assembly-CSharp.dll`) but unsupported as a stability contract.
- Distribution: plugin DLL → LabAPI `plugins/`; non-plugin dependencies → `dependencies/`;
  other *plugins* it depends on → also `plugins/`, never `dependencies/`.
