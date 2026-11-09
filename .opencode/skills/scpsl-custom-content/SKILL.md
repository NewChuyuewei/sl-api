---
name: scpsl-custom-content
description: Builds custom content for SCP Secret Laboratory plugins — EXILED 9.15.3 CustomRoles, CustomItems, CustomWeapons and abilities, plus LabAPI 1.1.7 do-it-yourself custom roles and items built from events and wrappers. Use when adding a custom role, custom item or weapon, custom ability, spawn logic, or NPC-driven content in an SCP:SL plugin.
license: MIT
compatibility: Requires the scpsl-labapi or scpsl-exiled skill for the chosen framework (LabAPI 1.1.7 or EXILED 9.15.3, net48, SCP:SL 14.2.x).
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Custom Content — Roles, Items, Abilities, NPCs

Custom content is where the two frameworks differ most:

- **EXILED 9.15.3** ships first-class APIs: `Exiled.CustomRoles` and `Exiled.CustomItems`.
  Use them. Do not hand-roll tracking/registration that the framework already does.
- **LabAPI 1.1.7** ships no custom role/item framework. Build the same features from events,
  wrappers, and your own registry — and own the cleanup that EXILED would have done for you.

Load the framework skill (`scpsl-labapi` or `scpsl-exiled`) alongside this one.
Templates: [assets/](assets/) (`CustomRole.cs`, `CustomItem.cs`, `LabApiCustomRoleService.cs`).

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
- Every role/item ID is unique, documented, and never reused; every handler's first statement is the ownership gate (`Check`/registry).
- Custom content registers in enable and unregisters in disable; spawn logic is config-gated (chance/caps/min-players).

### Stop conditions
- The framework for the target server is undecided (EXILED API vs LabAPI DIY are different builds): stop — content code is not portable between them.
- Serial/ownership tracking design is missing for a LabAPI DIY item: stop, design it first (see references).

### Definition of done (this skill)
Done when the full lifecycle per content type (give/spawn -> use -> drop/transfer -> destroy/914 -> round end) has a decided, implemented, scenario-tested behaviour.

## Shared design rules (both frameworks)

1. **IDs are forever.** Custom role/item numeric IDs and string names are saved in configs and
   referenced by other plugins. Allocate from a documented range, never renumber after release,
   never reuse a retired ID.
2. **Content is data + behaviour.** Keep stats (health, damage multipliers, chances, cooldowns)
   in config-backed content definitions; keep behaviour (event reactions) in handler code.
   Server owners must be able to tune or disable each content type individually.
3. **State lives per player/item instance, cleanup is total.** Track who has the role / which
   serials are the custom item; remove every trace on death, role change, drop, round end,
   and plugin disable. Leaked state after a round restart is the classic custom-content bug.
4. **Presentation is part of the feature.** Players need a name/description/hint on receiving
   the role or item (`CustomInfo`/hint/broadcast). Silent custom content reads as a bug.
5. **Balance + listing honesty.** Custom roles/items change gameplay: say so in the README
   (transparently-modded implications) and gate spawning behind config chances/min-player counts.

## EXILED path — CustomRoles

Reference `Exiled.CustomRoles.dll` (+ `Exiled.CustomItems.dll` if roles grant custom items).
A role is a class extending `Exiled.CustomRoles.API.Features.CustomRole`:

- Abstract/essential members: `Id` (`uint`), `Name`, `Description`, `CustomInfo`, `MaxHealth`,
  `Role` (the base `RoleTypeId` to spawn as).
- Common virtuals: `Inventory` (`List<string>` of item/custom-item names), `Ammo`
  (`Dictionary<AmmoType, ushort>`), `SpawnProperties`, `CustomAbilities`,
  `Scale`, health/armour-style stats per the pinned API reference.
- Behaviour hooks: override `SubscribeEvents()` / `UnsubscribeEvents()` for role-scoped event
  handlers, `RoleAdded(Player)` / `RoleRemoved`-style lifecycle per the pinned source, and
  `ShowMessage(Player)` for the receive message. Guard every handler with a tracked-player
  check (`TrackedPlayers` / `Check(player)`-style helpers in the pinned API) — role handlers
  receive events for **all** players, not just role holders.
- Abilities: `CustomAbility` / `ActiveAbility` subclasses attached via `CustomAbilities`;
  keep cooldowns and per-player ability state in the ability instance pattern shown upstream.
- Registration (in `OnEnabled`, symmetric unregistration in `OnDisabled`):
  `CustomRole.RegisterRoles()` registers every `CustomRole` type in the calling assembly;
  targeted overloads (`RegisterRoles(IEnumerable<Type>, …)`, `RegisterRolesFromSource(config, …)`)
  exist when roles are config-defined. Unregister with `CustomRole.UnregisterRoles()` (or its
  targeted overloads) in `OnDisabled` — registered roles left behind duplicate on reload.
  Assign/remove/test with `role.AddRole(player)`, `role.RemoveRole(player)`, `role.Check(player)`.

See [references/exiled-custom-roles.md](references/exiled-custom-roles.md) for the full pattern.

## EXILED path — CustomItems / CustomWeapons

Reference `Exiled.CustomItems.dll`. A item is a class extending
`Exiled.CustomItems.API.Features.CustomItem` (weapons: the firearm-oriented base in the same API):

- Abstract/essential members: `Id` (`uint`), `Name`, `Description`, `Weight`,
  `SpawnProperties`; `Type` is the base `ItemType` the custom item is built on; `Scale` for pickups.
- Behaviour: override `SubscribeEvents()` / `UnsubscribeEvents()`; at the top of every handler,
  check the item is yours (`Check(item)` / `Check(pickup)` against `TrackedSerials` pattern in
  the pinned API) before acting. Handlers see every player's items — the check is not optional.
- Registration mirrors roles: `CustomItem.RegisterItems()` (assembly-wide) or targeted overloads,
  in `OnEnabled`; `CustomItem.UnregisterItems()` in `OnDisabled`. Registration must happen after EXILED's own
  CustomItems plugin is ready — registering from your `OnEnabled` via the framework's normal
  plugin load order is the supported path; if content intermittently "doesn't exist", check
  load order/priority before adding delays.
- Spawning/giving: use the API's give/spawn helpers (`Give(player)` / spawn-at-position style
  methods in the pinned API) rather than adding the raw `ItemType` and trying to tag it after.

See [references/exiled-custom-items.md](references/exiled-custom-items.md).

## LabAPI path — do-it-yourself content

No framework registry exists, so the plugin provides one (template:
`assets/LabApiCustomRoleService.cs`):

1. **Definition**: a config-backed class (role ID/name, base `RoleTypeId`, max health,
   inventory `ItemType` list, spawn chance, min players).
2. **Registry**: `Dictionary<Player, CustomRoleState>` (keyed by a stable player identifier,
   cleaned on `Left`/death/role-change) + a HashSet of custom item serials for custom items
   (track `Item`/`Pickup` serial at give/spawn; validate on every use event).
3. **Assignment**: hook role-change/spawn events (`CustomEventsHandler` overrides), apply the
   role with wrapper calls (`Player.SetRole`, health/inventory via wrappers), then present it
   (hint + `CustomInfo`-style display where the wrapper surface allows).
4. **Behaviour**: every gameplay handler first checks the registry (`IsCustomRole(player)` /
   `IsCustomItem(serial)`) and returns immediately for non-holders — same discipline as EXILED's `Check`.
5. **Teardown**: round-end and plugin-disable clear both registries, kill MEC coroutines,
   and restore/leave players in a sane base role state. Never persist wrapper references
   across rounds.

Custom items in LabAPI follow the same shape: wrap give/pickup/use/drop/shoot events, key all
state by item serial, and re-validate ownership at use time — an item can change hands.

## NPCs and dummies

- EXILED: prefer `Exiled.API.Features.Npcs` helpers; NPCs look like players to most events —
  exclude them from role/item assignment, stats, and permission logic unless explicitly intended.
- LabAPI: NPC creation is lower-level (dummy/network-connection territory). Isolate it in one
  service, document the game-version sensitivity, and guarantee despawn on disable.
- Both: cap NPC counts in config; an NPC leak per round is a server-killing bug.

## References

- [references/exiled-custom-roles.md](references/exiled-custom-roles.md)
- [references/exiled-custom-items.md](references/exiled-custom-items.md)
- [references/labapi-custom-content-patterns.md](references/labapi-custom-content-patterns.md)

Upstream truth: `Exiled.CustomRoles` / `Exiled.CustomItems` API references at
https://exiled-team.github.io/EXILED and the 9.15.3 source. Custom content APIs churn faster
than core APIs — verify every override name against the pinned build before compiling.
