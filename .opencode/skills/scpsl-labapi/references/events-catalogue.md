# LabAPI 1.1.7 — Events Catalogue (orientation, not exhaustive)

LabAPI exposes 200+ events. This page teaches where to find the exact one and how its args
behave; always confirm the final signature in the pinned docs/source before coding.

## Where events live

- Static classes: `LabApi.Events.Handlers.<Domain>Events` — e.g. `PlayerEvents.Joined += handler`.
- Custom-handler virtuals: same events as `On<Event>` methods on `CustomEventsHandler`
  (e.g. override `OnPlayerJoined(PlayerJoinedEventArgs ev)`). Override detection is automatic;
  only overridden methods get subscribed by `CustomHandlersManager.RegisterEventsHandler`.
- Args: `LabApi.Events.Arguments.<Domain>Events.<Name>EventArgs`.

## Domains and representative events

| Domain (static class) | Representative events |
|---|---|
| `PlayerEvents` | `Joined`, `Left`, `ChangedRole`, `ChangingRole`, `Died`, `Hurting`/`Hurt`, `UsedItem`, `PickedUpItem`, `InteractedDoor`, `TogglingNoclip`, … |
| `ServerEvents` | Round start/end, wave respawn (`WaveRespawned`), `WaitingForPlayers`-style round lifecycle, `PluginsEnabled` (1.1.7) |
| `WarheadEvents` | Started, stopped, detonated lifecycle |
| `Scp914Events` | Activating, knob changes, upgrading players/pickups |
| Per-SCP (`Scp049Events`, `Scp079Events`, `Scp096Events`, `Scp106Events`, `Scp173Events`, `Scp939Events`, `Scp3114Events`, `Scp127Events`, …) | SCP-specific abilities and interactions |
| `ObjectiveEvents`, `ScpEvents` | Objectives and shared SCP behaviour |

Exact event names change between LabAPI versions (e.g. past-tense for completed actions:
`Joined`, `Died` vs `-ing` for cancellable pre-events: `ChangingRole`). Pattern:
**`-ing` events are usually cancellable; past-tense events are notifications.**

## Cancellation and mutation

- Cancellable args typically expose `bool IsAllowed { get; set; }` (default `true`).
  Setting `IsAllowed = false` vetoes the action. Check the incoming value first — an earlier
  plugin may already have vetoed; only re-allow deliberately and document it.
- Mutable args (new role, item lists, damage amounts, positions) are the intended way to
  change behaviour — prefer mutating args over re-implementing the action after the fact.
- Post-events (past tense) cannot cancel; use them for logging, stats, broadcasts, cleanup.

## Handler hygiene

- One responsibility per handler method; shared logic goes in a service class.
- Cache the handler instance (`private EventHandlers _handlers`) so unregister removes the
  exact subscriptions made at enable. With static events, handler delegates must match
  (`+=`/`-=` with the same method) — a classic reload-duplication bug.
- Exceptions in a handler must not escape into the loader/game loop: validate inputs,
  and let genuinely unexpected failures surface as a single `Logger.Error` with context
  during development — then fix the root cause rather than blanket-catching in production.
