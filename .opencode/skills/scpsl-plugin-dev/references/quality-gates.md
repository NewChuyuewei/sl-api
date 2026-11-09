# Quality Gates G0–G5 — Release and Merge Bar

Every change passes the gates for its risk tier. A gate is binary: pass with evidence, or
open. Open gates are listed in the delivery note — hiding one is a P0 trust violation.

## Risk tiers

| Tier | Change kind | Gates required |
|---|---|---|
| T1 | Docs/config-default/comment only | G0, G5 |
| T2 | Single-framework feature/fix, no Harmony, no persistence | G0–G3, G5 |
| T3 | Custom content, dual-target, persistence, inter-plugin API | G0–G5 |
| T4 | Harmony patch, loader/framework version bump, permission-model change | G0–G5 + explicit owner sign-off line |

## The gates

### G0 — Provenance and pins
- [ ] Framework named (LabAPI 1.1.7 / EXILED 9.15.3 / dual), net48, game 14.2.x
- [ ] No API used that is not verified against pinned upstream, or every unverified claim labelled
- [ ] Dependencies unchanged, or added ones pinned + justified in README

### G1 — Build and lifecycle integrity
- [ ] Release build clean (command + result recorded)
- [ ] Register/unregister symmetry audited (events, coroutines, patches, custom content, singletons)
- [ ] Disable → enable → round restart cycle executed with zero new exceptions in log

### G2 — Behaviour correctness
- [ ] Every spec behaviour rule mapped to code location + scenario ID
- [ ] Denial/boundary/config-broken paths executed, not assumed
- [ ] Core logic unit tests green (T2+ when Core exists; mandatory T3)

### G3 — Security and owner safety
- [ ] Every privileged path permission-checked (node listed), deny-first, input validated
- [ ] Config keys/nodes/IDs unchanged, or migration note + changelog owner-action line written
- [ ] No new player data stored/logged beyond the documented minimum

### G4 — Performance and coexistence (T3/T4, or any hot-path change)
- [ ] Hot-path budget self-check: hot handlers allocation-light, loops throttled to feature cadence
- [ ] Before/after measurement at stated population for any optimization claim
- [ ] Coexistence pass with the server's real plugin set; Harmony composition considered

### G5 — Documentation and release hygiene
- [ ] README/config/command/permission docs match shipped behaviour exactly
- [ ] CHANGELOG entry written for owners (effects first, owner actions flagged)
- [ ] Release artifact contains only plugin DLL + genuine deps (no game/framework DLLs)
- [ ] QA sign-off references build hash; known defects listed, none hidden

## Gate failure protocol

1. Stop. Do not proceed to the next stage (merge, release, handoff) on a failed gate.
2. Record: gate ID, evidence of failure (log line, scenario, measurement).
3. Choose explicitly: fix now, or downgrade scope (feature-flag off, remove from release),
   or accept-risk with a named owner and expiry. "Accept silently" does not exist.
4. The failure buys a permanent regression scenario in `scpsl-testing-qa` assets.
