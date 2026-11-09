---
name: scpsl-labapi
description: Develops SCP Secret Laboratory server plugins with Northwood's official LabAPI 1.1.7 (net48) — Plugin and Plugin<TConfig> entry points, CustomEventsHandler and static C# events, wrappers, commands, YAML config, permissions, and MEC coroutines. Use when writing, reviewing, or debugging a LabAPI plugin, LabApi.Features or LabApi.Events code, or code mentioning LabAPI.dll.
license: MIT
compatibility: Requires SCP:SL dedicated server 14.2.x reference assemblies, LabAPI 1.1.7 (NuGet Northwood.LabAPI or the bundled LabAPI.dll), and a .NET SDK 8+ targeting net48.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  game: "14.2.x"
  target-framework: "net48"
---

# LabAPI 1.1.7 Plugin Development

LabAPI is Northwood's **official** SCP:SL plugin loader, bundled with the dedicated server
since game 14.1. Plugins are net48 class libraries loaded by the LabAPI loader; they interact
with the game through **wrappers and events**, not raw game objects.

New project? Scaffold it, don't hand-write boilerplate:

```bash
python3 ../scpsl-plugin-dev/scripts/new_plugin.py --api labapi --name MyPlugin --output ./MyPlugin
```

Template sources also live in [assets/labapi-plugin/](assets/labapi-plugin/) for manual copying.

## Mandatory operating contract

This skill operates under the suite-wide priority hierarchy
(P0 server stability > P1 lifecycle/data > P2 security > P3 correctness > P4 performance > P5 maintainability > P6 style)
defined in `scpsl-plugin-dev/references/engineering-standard.md`. Conflicts resolve upward; never trade downward silently.

- **MUST** verify every framework API claim (event name, arg property, namespace, signature) against the pinned upstream
  (LabAPI 1.1.7 / EXILED 9.15.3) before using it. Anything unverifiable is written as `UNVERIFIED: <claim>` and must not ship as fact.
- **MUST NOT** invent events, wrappers, config keys, permission nodes, or file paths. Unknown means look it up or stop — never guess plausibly.
- **MUST** distinguish executed evidence from intention: never claim built/tested/reviewed/faster without having actually run it (see Evidence Standard).
- **MUST** be the whole change: code + config + docs + changelog + test evidence land together, or the delivery note names what is missing.
- If a missing fact would change framework, architecture, security, or gameplay behaviour, **STOP and ask**. Otherwise state the assumption in one line and proceed.

### Hard gates (this skill)
- Exactly one plugin class; Enable/Disable perfectly symmetric; `RequiredApiVersion` pinned to the built-against LabAPI.
- Config is properties-only with safe defaults; enable/disable is never implemented as a config key (that is `properties.yml`).

### Stop conditions
- The LabAPI version on the target server is unknown or not 1.1.7-compatible: stop and confirm before choosing APIs.
- A needed behaviour has no event/wrapper: do not improvise raw game calls inline — escalate to `scpsl-harmony-patching`'s gate.

### Definition of done (this skill)
Done when: Release build clean, handler registration symmetric, join/round/disable scenarios identified, config + README tables match the shipped classes.

## Project setup (non-negotiables)

- SDK-style class library, `<TargetFramework>net48</TargetFramework>`, `<LangVersion>latest</LangVersion>`,
  `Nullable` per template. Build with .NET SDK 8+ (the wiki walks through Rider/VS/CLI creation).
- References — mark every game/framework reference `Private=false` (never ship them):
  - `Northwood.LabAPI` **1.1.7** from NuGet, **or** `LabAPI.dll` from the server's
    `SCPSL_Data/Managed/` (the two must not be mixed at different versions).
  - From `SCPSL_Data/Managed/` as needed: `Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`,
    `CommandSystem.Core.dll`, `Mirror.dll`, `UnityEngine.CoreModule.dll` (+ further Unity modules on demand).
  - Use an `$(SL_REFERENCES)` MSBuild property for the Managed folder so collaborators/CI can relocate it.
- Exactly **one public plugin class** per assembly inheriting `Plugin` / `Plugin<TConfig>`.
- Deploy the built DLL to the LabAPI `plugins/` folder (see `scpsl-migration-release`); dependencies
  that are not plugins go to `dependencies/`.

## Plugin entry point

```csharp
using System;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;

namespace __PLUGIN_NAME__;

public sealed class Plugin : LabApi.Loader.Features.Plugins.Plugin<Config>
{
    public override string Name => "__PLUGIN_NAME__";
    public override string Description => "TODO: one-line description.";
    public override string Author => "__AUTHOR__";
    public override Version Version { get; } = new(1, 0, 0);
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
    public override LoadPriority Priority => LoadPriority.Medium;

    public static Plugin Instance { get; private set; } = null!;
    private EventHandlers _handlers = null!;

    public override void Enable()
    {
        Instance = this;
        _handlers = new EventHandlers(this);
        LabApi.Events.CustomHandlers.CustomHandlersManager.RegisterEventsHandler(_handlers);
        Logger.Info($"{Name} v{Version} enabled.");
    }

    public override void Disable()
    {
        LabApi.Events.CustomHandlers.CustomHandlersManager.UnregisterEventsHandler(_handlers);
        _handlers = null!;
        Instance = null!;
    }
}
```

Rules:

- `Name`, `Description`, `Author`, `RequiredApiVersion` are abstract — all four are mandatory.
  `RequiredApiVersion = new Version(LabApiProperties.CompiledVersion)` pins the plugin to the
  LabAPI it was built against; keep it unless intentionally supporting older loaders.
- `Version` defaults to the assembly version in 1.1.7 if omitted — set it explicitly anyway.
- `Priority` (`LoadPriority` in `LabApi.Loader.Features.Plugins.Enums`): events fire in
  registration order, so priority is load/registration order. Library plugins load early
  (`High`/`Highest`); most plugins stay `Medium`. Do not abuse priority to "win" event ordering.
- `IsTransparent` — override to `true` only for strictly non-intrusive admin/QoL plugins
  (Northwood's transparently-modded guidelines). Gameplay plugins leave it `false` and say so in the README.
- Enable/disable must be perfectly symmetric. `Enable()` registers; `Disable()` unregisters
  **and** nulls handler/instance fields so a reload cannot double-subscribe.

> Name collision warning: the LabAPI plugin base class and a plugin class both want to be called
> `Plugin`. Inherit with a fully-qualified base (`LabApi.Loader.Features.Plugins.Plugin<TConfig>`)
> or rename your class (e.g. `MyPluginPlugin`). Never add `using Plugin = ...` hacks that hide the clash.

## Events — prefer CustomEventsHandler

Two styles exist; use **CustomEventsHandler** for anything beyond a 20-line plugin
(style A is upstream's "legacy" C# events; both are shown in `LabApi.Examples`).

**Style A — static C# events** (`LabApi.Events.Handlers`): subscribe in `Enable`, unsubscribe in `Disable`.

```csharp
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

public override void Enable()  => PlayerEvents.Joined += OnJoined;
public override void Disable() => PlayerEvents.Joined -= OnJoined;

private static void OnJoined(PlayerJoinedEventArgs ev)
{
    Logger.Info($"Player {ev.Player.DisplayName} joined!");
    ev.Player.SendBroadcast("Hello! Thanks for joining!", 10);
}
```

**Style B — CustomEventsHandler (preferred):** override virtual `OnXxx` methods; one
`RegisterEventsHandler` call wires only the overridden methods, one call removes them all.

```csharp
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;

public sealed class EventHandlers : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        if (ev.Player is null) return;
        Logger.Info($"Player {ev.Player.DisplayName} joined!");
        ev.Player.SendBroadcast("Hello! Thanks for joining!", 10);
    }
}
```

Handler classes live in `LabApi.Events.CustomHandlers` (`CustomEventsHandler`,
`CustomHandlersManager.RegisterEventsHandler/UnregisterEventsHandler`); event args live in
`LabApi.Events.Arguments.<Domain>Events` (`PlayerEvents`, `ServerEvents`, `Scp914Events`, …).
Method names are the event names prefixed with `On` (`OnPlayerJoined`, `OnRoundStarted`-style
per domain — confirm the exact virtual in the pinned source/docs before overriding).

Event discipline:

- Check the catalogue before inventing hooks: [references/events-catalogue.md](references/events-catalogue.md).
- Read the args type: many LabAPI args expose `IsAllowed` for cancellation. Respect a
  `false` set by an earlier handler unless overriding it is the plugin's documented purpose.
- Guard nulls (`ev.Player`, items, doors) and round state; never assume a joined player is in-round.
- Keep handlers fast and allocation-light; defer work with MEC coroutines, never threads.

## Wrappers and features

Work through `LabApi.Features.Wrappers` — `Player`, server/map/facility, doors/rooms, items,
pickups, hazards, AdminToys. Highlights (see [references/wrappers-and-features.md](references/wrappers-and-features.md)):

- `ev.Player` is already the wrapper `Player` (`DisplayName`, `Role`, `SendBroadcast(msg, duration)`,
  `SendHint(text, duration)`, `SetRole(...)`, position/inventory members). Prefer wrapper
  members over reaching into `ev.Player.ReferenceHub`.
- Logging is `LabApi.Features.Console.Logger` (`Logger.Info/Warn/Error/Debug`).
- Timing is MEC: `Timing.CallDelayed(float, Action)`, `Timing.RunCoroutine(IEnumerator<float>)`,
  store the returned `CoroutineHandle` and `Timing.KillCoroutines(handle)` in `Disable()`.
- Permissions: check `sender.HasPermission("node")` (LabAPI permissions API; custom providers
  exist for group systems) in every privileged command — see the wiki "Permissions" pages.

Stay on wrappers. Touching `ReferenceHub`, `PlayerRoles`, Mirror, or Unity types directly is
occasionally unavoidable, but each such use must be isolated, null-checked, and commented with
why no wrapper exists — it is the first thing that breaks on a game update.

## Configuration

`Plugin<TConfig>` gives a single YAML config with default fallback:

```csharp
public sealed class Config
{
    public bool Debug { get; set; } = false;
    public string JoinBroadcast { get; set; } = "Hello! Thanks for joining!";
    public ushort JoinBroadcastSeconds { get; set; } = 10;
}
```

- **Properties only** — fields are not serialized. Defaults in initializers are the fallback values.
- `Config` is available in `Enable()` (loaded in `LoadConfigs()`); `SaveConfig()` persists changes.
  Broken YAML → LabAPI falls back to defaults and the plugin must still enable.
- Override `ConfigFileName` only with a good reason. Multiple files / advanced serialization →
  override `LoadConfigs()` and use the loader's `TryLoadConfig`/`SaveConfig` helpers (wiki:
  "Advanced Configurations").
- Enable/disable is **not** a config key in LabAPI — the loader's `properties.yml` controls it.
  Do not add an `IsEnabled` property expecting the loader to honour it (that's EXILED).

## Commands

Commands are `CommandSystem.ICommand` implementations auto-discovered from the assembly —
no manual registration in `Enable()`:

```csharp
using System;
using CommandSystem;
using LabApi.Features.Wrappers;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class HelloCommand : ICommand
{
    public string Command => "hello";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Greets the sender or a target player.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasPermission("myplugin.hello"))   // LabAPI permissions API
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        response = "Hello!";
        return true;
    }
}
```

- Handler types: `ClientCommandHandler` (client `.` console), `RemoteAdminCommandHandler` (RA),
  `GameConsoleCommandHandler` (server console). Multiple attributes = multiple surfaces.
- Group related commands under a `ParentCommand` (`LoadGeneratedCommands` not needed in LabAPI —
  children use `[CommandHandler(typeof(MyParentCommand))]`).
- Always set `response`, return `true` only on success, validate argument count/targets,
  and resolve players through wrappers (`Player.Get(...)`-style lookups per the pinned API).

## References

- [references/api-map.md](references/api-map.md) — namespaces, base classes, where things live in 1.1.7
- [references/events-catalogue.md](references/events-catalogue.md) — handler classes, common events, args/cancellation
- [references/wrappers-and-features.md](references/wrappers-and-features.md) — Player/Map/Server wrappers, Logger, MEC, permissions, DisplayKit pointers

Upstream truth: https://github.com/northwood-studios/LabAPI/wiki and `LabApi.Examples` in the repo.
If this skill and the pinned wiki disagree, the wiki wins — then fix this skill.
