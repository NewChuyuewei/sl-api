# Decision Matrix — LabAPI vs EXILED vs Dual-Target

## Capability comparison (pinned: LabAPI 1.1.7 / EXILED 9.15.3)

| Concern | LabAPI 1.1.7 | EXILED 9.15.3 |
|---|---|---|
| Maintainer / distribution | Northwood (official); bundled with every dedicated server since 14.1 | ExMod Team (community); server owner installs the EXILED release |
| Plugin base class | `LabApi.Loader.Features.Plugins.Plugin` or `Plugin<TConfig>` | `Exiled.API.Features.Plugin<TConfig>` / `Plugin<TConfig, TTranslation>` |
| Lifecycle | `Enable()` / `Disable()`, `LoadConfigs()`, `LoadPriority` | `OnEnabled()` / `OnDisabled()` / `OnReloaded()`, `PluginPriority` |
| Events | Static C# events (`PlayerEvents`, `ServerEvents`, …) **or** `CustomEventsHandler` overrides (preferred for non-trivial plugins) | Static handler classes (`Exiled.Events.Handlers.Player`, `.Server`, `.Map`, `.Scp914`, …) with `EventArgs` in `Exiled.Events.EventArgs.*` |
| Game-object model | Official wrappers (`LabApi.Features.Wrappers.Player`, doors, rooms, items, pickups, …); plugins are steered away from raw game types | Rich wrappers (`Exiled.API.Features.Player`, `Map`, `Cassie`, items, roles, NPCs) over publicized game assemblies |
| Config | `Plugin<TConfig>` single config or advanced multi-file config; YAML under the LabAPI configs folder; enable/disable is `properties.yml`, not a config key | `IConfig` (`IsEnabled`, `Debug` required) + optional `ITranslation`; one port-keyed YAML in EXILED `Configs/` |
| Commands | `ICommand` + `[CommandHandler(typeof(ClientCommandHandler / RemoteAdminCommandHandler / GameConsoleCommandHandler))]`; auto-registered from the assembly | Same `CommandSystem` model; EXILED auto-registers `ICommand` types from the plugin assembly in `OnRegisteringCommands()` |
| Permissions | LabAPI permissions API + custom providers (`sender.HasPermission(...)`) | `Exiled.Permissions` (`sender.CheckPermission("node")`), group-based config |
| Custom roles / items | No first-class framework API — build from events + wrappers (see `scpsl-custom-content`) | First-class `Exiled.CustomRoles` / `Exiled.CustomItems` APIs with registration, tracking, spawn properties, abilities |
| NPCs / dummy players | Manual, wrapper/game-type level | Higher-level NPC helpers in `Exiled.API.Features.Npcs` |
| Harmony | Possible but uncommon; stay on wrappers/events first | Common in the ecosystem; EXILED itself is Harmony-based. Still a last resort per-feature |
| Server-owner friction | None beyond dropping the DLL in — LabAPI is already there | Owner must install/maintain EXILED; plugin requires a matching EXILED version (`RequiredExiledVersion`) |
| "Transparently modded" fit | Designed with Northwood's guidelines in mind (`Plugin.IsTransparent`) | Depends on the plugin's behaviour, not the framework |

## Decision rules

1. **Server environment beats preference.** If the target server(s) don't run EXILED, an EXILED
   plugin is undeployable — choose LabAPI (or dual-target). Ask if unknown and it matters.
2. **Feature gravity.** Custom roles/items/abilities, translations, or EXILED-only ecosystem
   plugins (dependencies) pull strongly to EXILED. Plain event reaction, admin tools, round
   logic, and wrapper-level manipulation pull to LabAPI.
3. **Dependency check.** If the plugin must interoperate with a specific existing plugin,
   match that plugin's framework for the integration surface; keep any shared logic in Core.
4. **Dual-target cost is real.** Two adapters, two configs, two test passes. Choose it when
   distribution to both server types is an explicit requirement — not "just in case".
5. **Harmony is orthogonal.** Needing one patch is not a reason to switch frameworks;
   load `scpsl-harmony-patching` inside the chosen framework.

## Dual-target architecture

```text
MyPlugin.Core      net48, no framework/game references — rules, models, services
MyPlugin.LabApi    references Core + Northwood.LabAPI 1.1.7 + game assemblies
MyPlugin.Exiled    references Core + ExMod.Exiled (9.15.x) + game assemblies
```

- Core exposes framework-agnostic inputs/outputs (IDs, enums of its own, plain DTOs).
- Each adapter owns: plugin entry point, config shape, event translation, commands,
  player/message presentation. Config *keys* should mirror each other so server owners can
  move between builds; document both files in the README.
- Never share an assembly between adapters, never let Core leak `Player`, `ReferenceHub`,
  LabAPI wrappers, or EXILED types through its public API.
