# State and Persistence — Cleanup Matrix

Every state container ships with its cleanup row filled in before code is written:

| State | Key | Created on | Removed on | Owner class |
|---|---|---|---|---|
| e.g. Custom role holders | UserID | Role assign | Death / role change / leave / round end / disable | `CustomRoleService` |
| e.g. Item serial registry | Item serial | Give/spawn | Destroy / round end / disable | `CustomItemService` |
| e.g. Ability cooldowns | UserID | Ability use | Leave / round end / disable | Ability service |
| e.g. MEC loop handles | — | Enable/feature start | Disable / feature stop | Starting service |

## Failure modes this prevents

- **Ghost players**: wrapper-keyed dictionaries keep disconnected players "alive" forever.
  UserID keys + leave-event cleanup + per-use re-resolution close the hole.
- **Round bleed**: round 2 inherits round 1's roles/timers because cleanup hooked only disable.
  Round-end is a first-class cleanup trigger in both frameworks — wire it at M1, not M3.
- **Reload duplication**: state + subscriptions both survive a reload. Cleanup symmetry is
  audited by `scpsl-code-review`; persistence must also tolerate being loaded twice.
- **Half-applied persistence**: crash mid-write corrupts the only copy. Atomic replace +
  backup of last-good file + defaults-on-corruption are mandatory, not polish.

## Storage decision table

| Need | Choose |
|---|---|
| Owner-tunable values | Framework config (YAML) — not your own file |
| Small runtime data (stats, cooldowns across rounds), single server | JSON/YAML in plugin data folder, atomic writes, batched |
| Concurrent/queried/large data, multi-server sync | SQLite (single-server) or external DB (multi-server) behind a repository interface |
| Anything per-event hot-path | In-memory only; persist on round end / timed flush |

Threading rule, restated because it breaks here most: file/DB IO may run off-thread only as
pure data work; applying results to players/items/roles happens on the main thread via MEC.
