# Acceptance Criteria — <Feature Name>

Each criterion must be observable in-game or in the server log. Copy to the QA test plan.

## Happy path
- [ ] Given <round state/role>, when <trigger>, then <observable effect within stated time>.

## Configuration
- [ ] Setting `<key>` to <value> changes <behaviour>; invalid values fall back with a warning, plugin still enables.

## Permissions
- [ ] User without `<node>` gets a clear denial and nothing happens; user with `<node>` succeeds.

## Lifecycle
- [ ] Plugin disable stops the behaviour immediately; re-enable restores it exactly once (no duplicates).
- [ ] Round restart leaves no state from the previous round (roles, items, timers, NPCs).
- [ ] Player disconnect mid-interaction causes no exception and no leaked state.

## Coexistence and performance
- [ ] With <named common plugins> enabled, <ordering assumption> holds / degrades gracefully.
- [ ] On a populated test server, no per-frame allocations or tick hitches attributable to this feature.
