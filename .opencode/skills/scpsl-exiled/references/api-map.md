# EXILED 9.15.3 — API Map

## Assemblies (reference only what you use; all `Private=false`)

| Assembly | When you need it |
|---|---|
| `Exiled.API.dll` | Always — `Plugin<TConfig>`, `Player`, `Log`, enums, features |
| `Exiled.Events.dll` | Always in practice — `Exiled.Events.Handlers.*`, `Exiled.Events.EventArgs.*` |
| `Exiled.Permissions.dll` | Permission checks beyond basic RA access (`CheckPermission`) |
| `Exiled.CustomItems.dll` | Custom items/weapons (see `scpsl-custom-content`) |
| `Exiled.CustomRoles.dll` | Custom roles/abilities (see `scpsl-custom-content`) |
| `Exiled.Loader.dll` | Loader/plugin interop (`Loader.GetPlugin`, cross-plugin lookups) |
| `Exiled.CreditTags.dll` | Rarely; credit-tag integration only |

Plus game-side: publicized `Assembly-CSharp-Publicized.dll`, `Assembly-CSharp-firstpass.dll`,
`CommandSystem.Core.dll`, `Mirror.dll`, `NorthwoodLib.dll`, `LabApi.dll` (EXILED 9.15 references
the LabAPI bundled with the game), Unity modules. Upstream `Exiled.Example.csproj` is the
reference list — mirror it rather than inventing one.

## Namespaces

| Namespace | Contents |
|---|---|
| `Exiled.API.Features` | `Plugin<TConfig>`, `Plugin<TConfig, TTranslation>`, `Player`, `Log`, `Map`, `Server`, `Cassie`, `Round`, items/pickups/roles sub-namespaces |
| `Exiled.API.Enums` | `PluginPriority`, `ItemType`/`RoleTypeId` come from the game; EXILED enums (sides, effects, damage, spawn, …) |
| `Exiled.API.Interfaces` | `IConfig`, `ITranslation`, `IPlugin<TConfig>` |
| `Exiled.Events.Handlers` | `Player`, `Server`, `Map`, `Warhead`, `Item`, `Scp049`, `Scp079`, `Scp096`, `Scp106`, `Scp173`, `Scp914`, `Scp939`, … |
| `Exiled.Events.EventArgs.<Domain>` | `VerifiedEventArgs`, `ChangingRoleEventArgs`, `DyingEventArgs`, `HurtingEventArgs`, … |
| `Exiled.Permissions.Extensions` | `sender.CheckPermission("node")`, player permission helpers |
| `Exiled.API.Features.Npcs` | NPC/dummy creation and management |
| `MEC` | `Timing` coroutines (same discipline as LabAPI) |

## Plugin base class (pinned source: `Exiled.API/Features/Plugin.cs`)

- Constructor derives `Name`/`Prefix`/`Author`/`Version` from the **assembly**
  (name, snake-cased name, company attribute, assembly version) unless overridden.
  `Prefix` defaults via `ToSnakeCase()` — set it explicitly and never change it after release.
- `RequiredExiledVersion` defaults to the compiled-against EXILED assembly version;
  `IgnoreRequiredVersionCheck` bypasses the loader check (private builds only).
- `Config` is instantiated by the base (`new TConfig()`), then populated from YAML by the loader.
  `ConfigPath`/`TranslationPath` come from `Exiled.API.Features.Paths` keyed by `Prefix`.
- Lifecycle virtuals: `OnEnabled()`, `OnDisabled()`, `OnReloaded()`, `OnRegisteringCommands()`
  (base implementation auto-registers assembly commands — call/keep `base` behaviour if overridden),
  `OnUnregisteringCommands()`.
- `PluginPriority` ordering: lower/`First`-side loads earlier; event handlers fire in
  registration order, so priority indirectly orders your handlers vs other plugins.

## Player wrapper essentials (`Exiled.API.Features.Player`)

`Nickname`, `DisplayName`, `UserId`, `Role` (role object; `.Type` is `RoleTypeId`),
`CurrentItem`/`Items`, `ShowHint(text, duration)`, `Broadcast(...)`-style messaging,
`AddItem(ItemType)`, `Health`/effects members, `IsScp`-style team helpers, `SessionVariables`
for per-player plugin state (handy; still clean up on leave). Always null-check event players
and branch on `ev.IsAllowed` before mutating cancellable events.
