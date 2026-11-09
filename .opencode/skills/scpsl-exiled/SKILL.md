---
name: scpsl-exiled
description: Develops SCP Secret Laboratory server plugins with EXILED 9.15.3 (ExMod Team, net48) — Plugin<Config> entry points, Exiled.Events handlers and EventArgs, commands, YAML config and translations, Exiled.Permissions, NPCs, and MEC coroutines. Use when writing, reviewing, or debugging an EXILED plugin or code using Exiled.API, Exiled.Events, Exiled.CustomItems, or Exiled.CustomRoles.
license: MIT
compatibility: Requires an EXILED 9.15.3 server install, SCP:SL 14.2.x reference assemblies (publicized Assembly-CSharp for EXILED development), and a .NET SDK 8+ targeting net48.
metadata:
  version: "1.1.0"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# EXILED 9.15.3 Plugin Development

EXILED is the ExMod Team's high-level community framework for SCP:SL. Plugins are net48 class
libraries loaded from the EXILED `Plugins/` folder. EXILED 9.15 runs on LabAPI-era game builds
and references LabAPI internally — an EXILED plugin still **requires EXILED at runtime** and
cannot load on a LabAPI-only server.

New project? Scaffold it:

```bash
python3 ../scpsl-plugin-dev/scripts/new_plugin.py --api exiled --name MyPlugin --output ./MyPlugin
```

Template sources: [assets/exiled-plugin/](assets/exiled-plugin/).

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
- Register/unregister lists are mechanically diffed and identical; `Prefix` stable; `RequiredExiledVersion` not bypassed in public builds.
- `Config.IsEnabled` is never checked by plugin code (loader-enforced); every privileged command checks its node before acting.

### Stop conditions
- Target server's EXILED version is unknown or not 9.15.3: stop and confirm. NuGet availability of 9.15.3 is checked, never assumed.
- An event's args namespace (EXILED vs LabAPI-backed in 9.x) cannot be confirmed from pinned source: mark UNVERIFIED and verify before merging.

### Definition of done (this skill)
Done when: Release build clean, `+=`/`-=` audit passes, enable/round/reload scenarios identified, config/translation/README tables match shipped code.

## Project setup (non-negotiables)

- SDK-style class library, `<TargetFramework>net48</TargetFramework>`, `<LangVersion>latest</LangVersion>`
  (upstream `EXILED.props` uses net48 / C# 13, platform x64 — mirror that).
- References — all `Private=false`, never shipped:
  - EXILED assemblies at **9.15.3**: NuGet `ExMod.Exiled` (check the published version first —
    NuGet lagged at 9.15.2 when 9.15.3 shipped on GitHub; if so, reference the 9.15.3 release
    DLLs: `Exiled.API.dll`, `Exiled.Events.dll`, and only the feature assemblies you use:
    `Exiled.CustomItems.dll`, `Exiled.CustomRoles.dll`, `Exiled.Permissions.dll`, `Exiled.Loader.dll`).
  - Game assemblies from the EXILED install / server `SCPSL_Data/Managed/`: EXILED development
    uses **`Assembly-CSharp-Publicized.dll`** (not the plain assembly) plus
    `Assembly-CSharp-firstpass.dll`, `CommandSystem.Core.dll`, `Mirror.dll`, `NorthwoodLib.dll`,
    `LabApi.dll`, `UnityEngine.dll` / `UnityEngine.CoreModule.dll` / physics module as needed.
  - Use an `$(EXILED_REFERENCES)` MSBuild property for that folder, like upstream `Exiled.Example`.
- One plugin class per assembly inheriting `Plugin<Config>` (or `Plugin<Config, Translation>`).
- `RequiredExiledVersion` defaults to the EXILED version the plugin was compiled against —
  leave it correct; the loader enforces it. Only set `IgnoreRequiredVersionCheck = true`
  for private, knowingly cross-version builds, never in a public release.

## Plugin entry point

```csharp
using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace __PLUGIN_NAME__;

public sealed class Plugin : Exiled.API.Features.Plugin<Config>
{
    public override string Name => "My Plugin";
    public override string Prefix => "my_plugin"; // snake_case config key; never rename after release
    public override string Author => "Me";
    public override Version Version => new(1, 0, 0);
    public override PluginPriority Priority => PluginPriority.Default;

    public static Plugin Instance { get; private set; } = null!;
    private EventHandlers _handlers = null!;

    public override void OnEnabled()
    {
        Instance = this;
        _handlers = new EventHandlers(this);
        _handlers.Register();
        Log.Info($"{Name} v{Version} by {Author} has been enabled.");
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        _handlers.Unregister();
        _handlers = null!;
        Instance = null!;
        base.OnDisabled();
    }
}
```

Rules:

- `Name`, `Prefix`, `Author`, `Version` come from assembly attributes by default
  (`Assembly.GetName()`, company attribute). Overriding them explicitly (as upstream
  `Exiled.Example`-style plugins do via properties) is clearer — but keep `Prefix` stable
  forever: it is the YAML config section key and the translation key. Renaming it orphans
  every server owner's settings.
- `Priority` is `Exiled.API.Enums.PluginPriority` (`First` … `Last`, plus numeric values);
  order affects load and handler registration order. Default unless you are a library.
- `OnEnabled` → register everything; `OnDisabled` → unregister everything and null the fields;
  `OnReloaded` exists for reload-time re-reads. `Config.IsEnabled` is read by the **loader** —
  the plugin never checks it itself.
- Commands are auto-registered by the base class (`OnRegisteringCommands` scans the assembly
  for `ICommand` + `[CommandHandler]`). Do not hand-register commands that the scanner owns,
  and do not leave stray `ICommand` types in the assembly that should not be commands.

## Events

Handlers are plain classes; the plugin (or the handler) subscribes with `+=` and — critically —
unsubscribes with `-=` in `OnDisabled`. Upstream's `Exiled.Example` dedicates one handler class
per domain (`PlayerHandler`, `ServerHandler`, `MapHandler`, …). Follow that shape.

```csharp
using Exiled.Events.EventArgs.Player;
using PlayerHandlers = Exiled.Events.Handlers.Player;

public sealed class EventHandlers
{
    private readonly Plugin _plugin;
    public EventHandlers(Plugin plugin) => _plugin = plugin;

    public void Register()   => PlayerHandlers.Verified += OnVerified;
    public void Unregister() => PlayerHandlers.Verified -= OnVerified;

    private void OnVerified(VerifiedEventArgs ev)
    {
        if (ev.Player is null) return;
        ev.Player.ShowHint(_plugin.Config.JoinHint, _plugin.Config.JoinHintSeconds);
    }
}
```

- Handler classes: `Exiled.Events.Handlers.Player / Server / Map / Warhead / Item / Scp914 /
  Scp096 / Scp106 / Scp049 / …`; args in `Exiled.Events.EventArgs.<Domain>` (`…EventArgs`).
  Alias the handler class (`using PlayerHandlers = Exiled.Events.Handlers.Player;`) — `Player`
  alone collides with `Exiled.API.Features.Player`.
- Cancellable args expose `IsAllowed` (check before setting — respect earlier vetoes);
  many args are mutable (role, items, damage). Past-tense-style events (`Died`, `Spawned`)
  are notifications; `-ing` events (`Dying`, `ChangingRole`, `Hurting`) are the cancellable ones.
  Confirm in the pinned API docs per event — see [references/events-catalogue.md](references/events-catalogue.md).
- The example in upstream `Exiled.Example` even contains a real bug in its `UnregisterEvents`
  (`PickingUpItem +=` instead of `-=`). Copy the *structure*, then audit every `+=` for a
  matching `-=`. That audit is mandatory before shipping.
- Players: `Exiled.API.Features.Player` (`Nickname`, `DisplayName`, `Role.Type`, `ShowHint`,
  `Broadcast`, `AddItem`, …). Re-resolve, don't cache player objects across disconnects.

## Configuration and translations

```csharp
using System.ComponentModel;
using Exiled.API.Interfaces;

public sealed class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;   // required by IConfig; loader-enforced
    public bool Debug { get; set; } = false;      // required by IConfig

    [Description("Hint shown to a player after they are verified. Empty disables it.")]
    public string JoinHint { get; set; } = "Welcome to the server!";
    public float JoinHintSeconds { get; set; } = 5f;
}
```

- `IConfig` requires `IsEnabled` and `Debug`. Every extra property gets a `[Description]`
  (it becomes the YAML comment) and a safe default. Config YAML is snake_case, grouped under
  the plugin `Prefix` in EXILED `Configs/<port>-config.yml` — see
  [references/config-and-translations.md](references/config-and-translations.md).
- Use public get/set properties; upstream examples use `private set` in places, but plain
  `{ get; set; }` is the reliable serializable shape for owner-editable values.
- Translations: inherit `Plugin<Config, Translation>` with an `ITranslation` class when
  user-facing strings must be localizable; otherwise keep strings in config. Never hard-code
  a wall of messages in handlers.

## Commands and permissions

Same `CommandSystem` model as the base game (EXILED auto-registers):

```csharp
using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class HelloCommand : ICommand
{
    public string Command => "hello";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Prints a greeting. Usage: hello";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("__PLUGIN_NAME__.hello"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        response = "Hello from __PLUGIN_NAME__!";
        return true;
    }
}
```

Permission nodes (`Exiled.Permissions.Extensions.CheckPermission`) are granted in the EXILED
permissions config by group. Document every node; default to deny; validate targets/arguments.
Parent commands: subclass `ParentCommand`, register children with `[CommandHandler(typeof(MyParent))]`
and load them in `LoadGeneratedCommands()`.

## Coroutines, NPCs, logging

- Delays/loops: MEC `Timing.CallDelayed` / `Timing.RunCoroutine`; keep `CoroutineHandle`s and
  `Timing.KillCoroutines(...)` in `OnDisabled`. No threads, no `Task.Delay` for game logic.
- NPCs/dummies: use `Exiled.API.Features.Npcs` helpers and clean every spawned NPC up on
  disable/round end; NPCs are `Player`-shaped — exclude them from player-only logic (`ev.Player.IsNpc`-style checks).
- Logging: `Exiled.API.Features.Log.Info/Warn/Error/Debug`; Debug output follows `Config.Debug`.

## References

- [references/api-map.md](references/api-map.md) — assemblies, namespaces, wrapper surface
- [references/events-catalogue.md](references/events-catalogue.md) — handler domains, common events, cancellation
- [references/config-and-translations.md](references/config-and-translations.md) — YAML layout, IConfig/ITranslation, permissions config

Upstream truth: https://github.com/ExMod-Team/EXILED (`Exiled.Example`, `.github/documentation`)
and https://exiled-team.github.io/EXILED . If this skill disagrees with 9.15.3 upstream, upstream wins.
