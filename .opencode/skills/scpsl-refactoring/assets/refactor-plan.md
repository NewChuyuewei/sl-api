# Refactor Plan — <Plugin Name>

## Invariants (must not change — any change re-plans this as a feature)
- Config keys/defaults: <list>
- Permission nodes: <list> · Commands/aliases: <list>
- Custom content IDs: <list> · Persisted data format: <version>
- Plugin Name/Prefix, framework + pins: <...>

## Characterization safety net
- [ ] Boot + lifecycle scenarios green on current build: <test plan link/hash>
- [ ] Top features covered: <list>

## Ranked moves (risk × churn; one move per commit)
| # | Move | Files | Risk | Verification |
|---|---|---|---|---|
| 1 | Lifecycle repair |  |  | disable/enable/restart cycle |
| 2 |  |  |  |  |

## Scanner baseline
`scan_plugin.py` findings before: <n + top hits> · After target: no increase, lifecycle leads zero.

## Temporary dualities (if any)
| Two patterns coexisting | Finish task | Date |
|---|---|---|
