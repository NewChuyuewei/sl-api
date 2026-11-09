# Server Test Scenario Catalogue (instantiate per plugin)

## Lifecycle family (always all of these)
- L1 Cold boot with plugin: no exceptions; enabled line present; config generated correctly.
- L2 Disable mid-round: feature inert immediately; no handler ghost activity in log.
- L3 Re-enable mid-round: feature works exactly once per trigger (count effects!).
- L4 Framework reload (where supported): L2+L3 without server restart; no duplicate registrations.
- L5 Round restart during active feature state: new round starts clean (roles/items/timers reset).
- L6 Longevity: 5+ consecutive rounds; dictionary/state counts stable; no growing log spam.

## Player family
- P1 Verified/joined player receives join-facing presentation exactly once.
- P2 Disconnect during a pending delayed action: no exception when the delay fires.
- P3 Spectator triggers nothing player-only; NPC/dummy triggers nothing player-only.
- P4 Role change chain (spawn → custom assignment → death → respawn): state follows correctly.

## Economy-of-resources family (adapt per feature)
- C1 Cooldown enforced at boundary (early attempt denied, on-time attempt succeeds).
- C2 Caps enforced (max active instances); excess attempts degrade gracefully.
- C3 Chance gates: config 0% never fires in 20 trials; 100% always fires.
- C4 Item lifecycle: give → use → drop → other player pickup → use → destroy; custom
  behaviour follows the serial/owner rules, never the raw ItemType.

## Hostile-input family
- H1 Commands with missing/extra/garbage arguments: graceful response, no exception.
- H2 Commands targeting offline/spectator/NPC/host players: rejected or handled per spec.
- H3 Broken config YAML (syntax error, wrong types, negative durations): defaults + warning, server stable.
- H4 Permission removed mid-session: next invocation denied (no cached "allowed forever").
