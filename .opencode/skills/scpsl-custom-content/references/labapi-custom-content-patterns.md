# LabAPI 1.1.7 — Custom Content Patterns (DIY)

LabAPI has no CustomRole/CustomItem framework. These patterns reproduce the essential
guarantees (identity, tracking, cleanup) with events + wrappers. Anything less leaks state.

## Custom role pattern

```text
Config definition ──► CustomRoleService (registry + assignment + behaviour gate)
                          ▲                     │
        CustomEventsHandler overrides ──────────┘
        (spawn/role-change/death/leave/round-end hooks call into the service)
```

1. **Define** roles in config: stable numeric ID + name, base `RoleTypeId`, max health,
   starting `ItemType` list, spawn chance, minimum players, description text.
2. **Assign** from a spawn/role-change handler override: validate round state and gates,
   `player.SetRole(baseRole, …)` via the wrapper, apply health/inventory, record
   `registry[playerId] = roleState`, present with `SendHint`/broadcast.
3. **Gate behaviour**: first line of every gameplay handler —
   `if (!service.TryGetRole(ev.Player, out var state)) return;`
4. **Remove** on death, base role change away, disconnect (`OnPlayer…` leave/died overrides),
   round end, and `Disable()`: delete registry entries, kill per-player coroutines,
   clear hints. Death/role-change ordering matters — test all three exit paths.
5. **Persist nothing across rounds** unless the feature is explicitly meta-progression
   (then persist keyed by UserID in your own storage, never by wrapper reference).

## Custom item pattern

1. **Define**: stable ID + name, base `ItemType`, description, spawn rules.
2. **Mint**: when giving/spawning (command, role inventory, map spawn hook), capture the
   created item/pickup **serial** into `HashSet<ushort> customSerials` (exact serial member
   per the pinned wrapper API).
3. **Gate**: use/shoot/reload/pickup/drop handlers check `customSerials.Contains(item.Serial)`
   before doing anything; on drop/pickup the serial persists — on destroy/914-upgrade,
   remove or transfer it deliberately.
4. **Re-validate at use time**: the holder may have traded the item; serial, not player, is truth.

## What you give up vs EXILED (state this in design docs)

- No shared registry: two LabAPI plugins' custom content cannot discover each other without
  a bespoke interop API. Publish one (public events / a shared service assembly) if integration matters.
- No framework spawn-property system: locker/room spawn logic is yours to write and balance.
- All cleanup is manual. Budget tests for reload/round-restart cycles accordingly.
