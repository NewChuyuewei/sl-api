# Common SCP:SL Plugin Defects (with runtime consequences)

## Lifecycle
- **`+=` in Unregister (or a missing `-=`)** — handlers double-fire after every reload;
  effects stack (double damage, duplicate spawns). Upstream's own EXILED example once shipped
  this exact bug, which is why the mechanical list-diff is mandatory.
- **Static event handler instances / statics surviving disable** — same doubling, plus ghost
  behaviour from a "disabled" plugin.
- **Unstored MEC coroutines** — loops outlive the plugin and the round; NRE spam after disable.
- **`UnpatchAll()` without own ID** — silently removes other plugins' and EXILED's patches;
  server-wide weirdness blamed on everyone else.

## Events
- **Joined vs Verified confusion** — LabAPI `Joined` fires earlier than EXILED `Verified`;
  porting code between them changes when items/roles are applied and can race role assignment.
- **Overwriting `IsAllowed = true`** — resurrects actions another plugin deliberately vetoed
  (anti-grief, custom spawn protection). Read first; re-allow only as the documented feature.
- **Mutating after the fact** — forcing roles/items in a post-event instead of using the
  cancellable event's mutable args causes flicker, duplicate inventories, and client desync.

## State
- **Wrapper-keyed dictionaries** — disconnected players leak; stale wrappers throw or act on
  reused hubs. UserID/serial keys + cleanup events are the only accepted pattern.
- **No round-end cleanup** — round 2 inherits custom roles, cooldowns, and timers from round 1.
- **Serial-blind custom items (LabAPI DIY)** — checking `ItemType` instead of serial turns
  every vanilla medkit into the custom item after the first one exists.

## Commands/permissions
- **RA-access-as-permission** — any admin (or any player, for client commands) can run
  privileged actions. Node check or it is a blocker, full stop.
- **Unvalidated targets** — commands resolving players by partial name/index without existence
  and eligibility checks crash on typos and can target spectators/NPCs/host unintentionally.

## Performance
- **`Timing` loop at frame rate doing dictionary scans over all players** — works with 5 test
  players, melts at 30. Throttle to seconds and keep per-iteration work O(affected), not O(server).
- **Logging built eagerly** — interpolated strings constructed for disabled debug logs in hot
  events; gate with the Debug flag *before* building the message where the framework logger
  doesn't already make it cheap.
