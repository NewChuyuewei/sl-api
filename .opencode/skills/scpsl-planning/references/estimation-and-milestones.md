# Estimation and Milestones

## Task sizing

Size by risk, not lines of code. Anchors for a competent agent/developer on the pinned stack:

| Size | Typical SCP:SL examples |
|---|---|
| XS (≤1h) | One config-gated hint/broadcast handler; one simple RA command with permission check |
| S (hours) | New event feature with presentation + config; parent command with 2-3 children |
| M (a day-ish) | Custom item with full lifecycle (EXILED) or LabAPI DIY item tracking; NPC spawn/cleanup flow |
| L (multi-day) | Custom role with abilities and spawn logic; persistence; inter-plugin API |
| XL (split it) | Anything Harmony-transpiler-based, dual-target full feature, or "rewrite the spawn system" — XL means the breakdown failed; split until no task exceeds L |

Add explicit tasks (never fold in silently): Register/Unregister audit, config table docs,
README updates, reload/round-restart test pass, release packaging. Historically these are
20-30% of real plugin work.

## Milestones

- **M1 Skeleton that loads**: scaffolded project builds, enables/disables cleanly on a test
  server, one end-to-end behaviour path works. Exit: enable/round/disable log lines clean.
- **M2 Feature complete**: all spec rules implemented behind config; commands + permissions;
  presentation done. Exit: acceptance scenarios pass on a test server (hand to `scpsl-testing-qa`).
- **M3 Hardened release**: edge cases (disconnect/reload/round restart), performance sanity
  on a populated server, docs + changelog, release zip verified clean. Exit: `scpsl-migration-release`
  release checklist fully checked.

## Risk buffers, honestly

State estimates as `likely` and `if-the-risk-hits` numbers. Named risks that justify the gap:
unverified API behaviour (spike first, then re-estimate), event-ordering against unknown
plugins, Harmony target drift, balance iteration count. A buffer without a named risk is
padding; a risk without a spike or fallback is a plan defect.
