# LabAPI 1.1.7 — Wrappers and Features

## Player wrapper (`LabApi.Features.Wrappers.Player`)

The wrapper you receive in event args (`ev.Player`). Members agents use most (verify against
the pinned API docs for the full surface):

- Identity/state: `DisplayName`, `Nickname`-style name members, `UserId`, `Role` (`RoleTypeId`),
  `IsAlive`-style checks, `Position`, `Health`-related members via the stats surface.
- Presentation: `SendBroadcast(string message, ushort duration, …)`,
  `SendHint(string text, float duration = 3f)`, hint/broadcast clearing helpers.
- Actions: `SetRole(RoleTypeId, RoleChangeReason, RoleSpawnFlags)`, inventory/item members,
  teleport/position assignment. Prefer these over `ReferenceHub` calls.
- Lookup: static dictionary/get patterns on `Player` for resolving a wrapper from a
  `ReferenceHub`, UserID, or command argument — use them instead of caching wrappers long-term.
  Wrappers of disconnected players go stale; re-resolve per use and null-check.

## Other wrappers

Organised under `LabApi.Features.Wrappers` subfolders: **Server** (round/server state),
**Facility** (rooms, doors, zones, hazards such as Tesla/amnestic cloud), **Items** and
**Pickups** (in-inventory vs world items — `Pickup.Create(...)` spawns over the network by
default since 1.1.7), **AdminToys** (spawnable primitives/text toys). Rule: if a wrapper exists
for the object you need, the wrapper is the API; raw Unity/Mirror access needs a written reason.

## Logger (`LabApi.Features.Console.Logger`)

`Logger.Debug(object, bool canBePrinted = true)`, `Info`, `Warn`, `Error`, plus `Raw(message, ConsoleColor)`.
Messages are auto-prefixed with the calling assembly — do not add your own `[MyPlugin]` prefix.
Gate chatty logs behind a `Debug` config property.

## MEC coroutines (`MEC.Timing`)

```csharp
private CoroutineHandle _loop;

_loop = Timing.RunCoroutine(MyLoop());
Timing.CallDelayed(3f, () => { /* runs once on the main thread */ });

private IEnumerator<float> MyLoop()
{
    while (true)
    {
        yield return Timing.WaitForSeconds(60f);
        // periodic work; bail out if the plugin is disabling
    }
}
```

Kill every stored handle in `Disable()` (`Timing.KillCoroutines(_loop)`); unstored fire-and-forget
delays must re-validate plugin/player state when they fire. Wikipedia of the obvious: no
`Thread.Sleep`, no `async void` event handlers, no background-thread game access.

## Permissions

LabAPI ships a permissions API (`LabApi.Features.Permissions`) with `HasPermission` checks on
command senders (`sender.HasPermission("myplugin.admin")`) and support for custom providers
backed by server-owner group data. Design nodes as `<plugin>.<area>.<action>`, check them in
commands **and** in any event that can be triggered to bypass a command, and document every
node in the README. Treat a missing permission system response as deny.

## DisplayKit / Outlines (pointers)

LabAPI includes **DisplayKit** (server-driven UI elements sent to clients) and **Outlines**
highlighting. These are specialised — read the wiki's DisplayKit pages (Creating/Modifying/
Deleting Elements, Sending to Players, Element Reference) before using them, keep element
updates batched (not per-frame), and always delete per-player elements on leave/disable.
