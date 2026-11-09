---
name: scpsl-architecture
description: Designs SCP Secret Laboratory plugin architecture — solution structure, shared Core and framework adapters, state management, services, inter-plugin APIs, persistence, and configuration design for LabAPI 1.1.7 and EXILED 9.15.3 plugins. Use when structuring a new plugin solution, splitting a growing plugin, designing dual-target code sharing, or deciding where logic, state, or storage belongs.
license: MIT
compatibility: Works with scpsl-plugin-dev, scpsl-labapi, and scpsl-exiled. Targets LabAPI 1.1.7 / EXILED 9.15.3, net48, SCP:SL 14.2.x.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Architecture

Good SCP:SL architecture has one goal: **game/framework contact is thin, explicit, and
replaceable; everything else is plain C# that can be reasoned about and tested off-server.**
Most plugin pain (reload bugs, porting pain, untestable logic) is architecture debt, not API debt.

Start from `scpsl-planning` output (spec, framework decision). Record significant decisions
with [assets/architecture-decision-record.md](assets/architecture-decision-record.md) — one
short file per decision, never a giant design doc nobody updates.

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
- Core contains zero LabAPI/EXILED/Unity/Mirror/Assembly-CSharp references — checkable by inspecting the .csproj, not by intention.
- Every state container has its cleanup row (created/removed/owner) written before implementation begins.

### Stop conditions
- A design requires game types inside Core, static mutable cross-player state, or persistence with no schema/deletion story: stop and redesign.

### Definition of done (this skill)
Architecture is done when every component has one owner, every framework contact point is in an adapter, and each significant decision has an ADR or a one-line rejection note.

## Reference solution shape

```text
MyPlugin.Core/        net48. Rules, models, parsing, state machines.
                      NO LabAPI / EXILED / Unity / Mirror / Assembly-CSharp references.
MyPlugin.LabApi/      LabAPI 1.1.7 adapter (only when targeting LabAPI)
MyPlugin.Exiled/      EXILED 9.15.3 adapter (only when targeting EXILED)
MyPlugin.Tests/       Unit tests over Core (off-server, fast)
```

Single-framework plugins still benefit: `Core` may live as a `Services/` + `Domain/` folder
inside the one project, but the dependency rule below still applies. Dual-target without a
shared Core is two plugins that happen to share a name — see `scpsl-plugin-dev`.

## Dependency rules (enforced in review)

1. **Adapters depend on Core; Core depends on nothing game-related.** Core's public API uses
   primitives and its own DTOs/enums. `Player`, `ReferenceHub`, LabAPI wrappers, EXILED types,
   `Vector3`, and `ItemType`/`RoleTypeId` do not cross the Core boundary — adapters translate
   (game enum ↔ Core enum mapping lives in the adapter, in one file).
2. **The plugin entry point is wiring only.** It constructs services, registers handlers/
   commands, and owns lifecycle. Feature logic in the `Plugin` class fails architecture review.
3. **Handlers translate, services decide, adapters present.** Event handler =
   extract primitives + null/state guards → call service → apply results through wrappers
   (roles, items, hints). Business rules inside handlers become untestable within a week.
4. **One owner per piece of state** (see State, below). Two classes mutating the same
   dictionary is a defect, even if it "works".

## State management

Classify every piece of state before writing it; the class decides its cleanup:

| Class | Examples | Storage | Cleanup |
|---|---|---|---|
| Round state | active custom roles, ongoing minigame, per-round counters | Service-owned dictionary keyed by stable player ID (UserID), never by wrapper alone | Round end + disable |
| Player session state | cooldowns, open menus, pending actions | Same, keyed by UserID; re-resolve the player per use | Leave, death/role-change where relevant, round end |
| Item instance state | custom item behaviour per serial | Keyed by item serial (`ushort`) | Destroy, pickup transfer handled deliberately, round end |
| Persistent state | stats, loadouts, bans, currency | Repository behind an interface; file/database (below) | Survives rounds by design; versioned schema |
| Config snapshot | values read at enable | Copy into readonly service settings at enable | Reload re-reads; handlers never re-parse YAML |

Rules: wrappers/players are **not** dictionary keys for anything living past the current event
(players disconnect; wrappers go stale). Coroutines (MEC handles) are state too — owned by the
service that started them, killed at the same cleanup points. Static mutable state is forbidden
outside an explicitly-owned singleton service, and the singleton dies in `Disable()`.

## Services and boundaries

- One service per feature area (`SpawnService`, `CustomRoleService`, `EconomyService`), each
  with a small interface when another adapter/test/other plugin consumes it; concrete classes
  when only handlers do. Avoid interface-per-class ceremony.
- Cross-feature communication goes through service methods or plugin-internal events
  (plain C# `event` on a service), never through handler-to-handler references.
- Time/randomness/filesystem enter through injectable seams (`Func<DateTime>`-style clocks are
  overkill; a small `IRandom` wrapper and explicit file paths are not) so Core tests stay deterministic.

## Inter-plugin APIs (when other plugins must integrate)

- Publish a separate, versioned **API assembly** (interfaces + DTOs + events only, no plugin
  implementation) that consumers reference `Private=false`. Version it independently;
  breaking it breaks strangers' plugins.
- Discovery: LabAPI — expose a documented static service accessor or loader lookup;
  EXILED — `Exiled.Loader.Loader.GetPlugin(...)`-style lookup then cast to the API interface.
  Document the exact pattern in the README; consumers must null-check (dependency absent =
  feature degrades, never crashes).
- Never let consumers touch your internal services/handlers, and never depend on another
  plugin's internal types — only its published API assembly. Optional dependencies are
  checked at `Enable`, logged once, and feature-flagged.

## Persistence

- Default answer is **no persistence**. Add it only when the spec demands cross-round data.
- Small data: YAML/JSON file in the plugin's own config/data folder (framework path APIs —
  never hard-coded server paths), written atomically (temp file + replace), loaded with
  defaults-on-corruption. Writes are batched/debounced off the hot path; never write files
  inside damage/item/shot events.
- Larger/queried data: SQLite via a repository class in Core-adjacent infrastructure code.
  All IO off the main thread is allowed only for pure data — results re-enter game state
  exclusively through MEC on the main thread. Schema version field from day one.
- Player identity key is UserID (Steam/Discord/etc. as provided by the wrapper); handle
  missing/changed IDs gracefully. Privacy: store the minimum, document it, offer deletion
  (see `scpsl-security-permissions`).

## Configuration architecture

- Config classes are **contracts**: stable key names, safe defaults, validation/clamping at
  enable with warnings. Group growing configs into nested sections instead of 60 flat keys.
- Feature flags per major subsystem (`Enabled` sub-toggles) so owners can disable a broken
  subsystem (esp. Harmony patches, custom content) without removing the plugin.
- LabAPI multi-file needs → advanced config (`LoadConfigs` override); EXILED growth →
  nested config objects or translations for strings. Mirror key semantics across dual-target
  adapters so owners can carry settings over.

## Scaling red flags (trigger a refactor conversation, see `scpsl-refactoring`)

Plugin class > ~300 lines; any handler file subscribing to > ~15 events; the same wrapper
lookup repeated in 5+ places; static dictionaries in two classes tracking the same players;
a feature that cannot be described without naming a game type. Each is cheap to fix early
and brutal after release.

## References

- [references/core-adapter-pattern.md](references/core-adapter-pattern.md) — Core/adapter translation discipline
- [references/state-and-persistence.md](references/state-and-persistence.md) — state classes, storage choices, cleanup matrix
