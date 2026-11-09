---
name: scpsl-testing-qa
description: Tests SCP Secret Laboratory plugins — test strategy, Core unit tests, dedicated-server scenario testing, lifecycle and reload cycles, coexistence and regression testing for LabAPI 1.1.7 and EXILED 9.15.3 plugins. Use when writing a test plan, verifying a feature works, reproducing a bug, or signing off an SCP:SL plugin before release.
license: MIT
compatibility: Unit tests need a .NET test runner over the net48 Core project; server tests need an SCP:SL 14.2.x dedicated server with LabAPI 1.1.7 and/or EXILED 9.15.3. Python 3 for the bundled matrix generator.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Testing and QA

SCP:SL plugins cannot be fully tested off-server — but the parts agents usually skip testing
(Core logic) are exactly the parts that *can* be, and the parts that need a server fail in
predictable, scenario-shaped ways. This skill tests both layers deliberately.

Input: acceptance criteria from `scpsl-planning`. Output: a filled test plan
([assets/test-plan.md](assets/test-plan.md)) with pass/fail evidence, and bug reports
([assets/bug-report.md](assets/bug-report.md)) a developer can reproduce from.

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
- Sign-off names build hash, framework, scenarios run, scenarios skipped with reasons, and accepted defects with owners. Anything less is not sign-off.
- Lifecycle family (boot/disable/enable/round-restart) is all-pass with zero new exceptions before any feature result counts.

### Stop conditions
- Tests are requested against an unnamed framework version or an unrecorded build: stop and pin both first.
- A defect cannot be reproduced from the report's steps: return for evidence (full log, first exception) instead of guessing fixes.

### Definition of done (this skill)
Done when acceptance criteria each map to a scenario ID with recorded pass/fail and evidence, and every failure has a filed bug report or an accepted-defect entry.

## Layer 1 — Core unit tests (off-server, every commit)

- Test framework-free Core only (see `scpsl-architecture`): rules, chance/cooldown logic,
  parsing, state machines, config clamping. If a behaviour cannot be unit-tested, that is an
  architecture finding — feed it back, don't accept "test it in-game" for pure logic.
- Deterministic: inject seedable randomness and explicit inputs; no wall-clock, no filesystem
  outside temp dirs, no game types anywhere in the test project.
- Minimum coverage bar: every spec behaviour rule has a test; every config clamp/validation
  has a boundary test; every state machine transition (including illegal ones) has a test.
- net48 test projects run with the repo's standard .NET test tooling; keep them fast (<1 min)
  or nobody runs them.

## Layer 2 — Server scenario tests (dedicated test server, per milestone)

Run on a **dedicated test server**, never by reasoning about code. Record: framework + version,
player count, other plugins enabled, and the exact steps. Core scenario families — write the
concrete steps from the plugin's acceptance criteria:

1. **Boot and enable**: clean log, plugin enabled line, config generated with expected keys.
2. **Lifecycle cycle**: enable → play → disable (behaviour stops dead) → enable (works once,
   not twice) → reload/config reload where supported. Duplicate behaviour = Blocker bug.
3. **Round cycle**: feature across round start, respawn waves, warhead/decontamination
   (if relevant), round end, round restart. Any state surviving into round 2 is a bug.
4. **Player churn**: join mid-round, disconnect mid-interaction, reconnect, spectator↔player
   transitions, NPC/dummy presence (must be ignored by player-only logic).
5. **Feature scenarios**: one per behaviour rule — happy path, boundary values (chances 0%/100%,
   caps, cooldown edges), and the denied path (no permission, wrong role, wrong item).
6. **Command matrix**: each command × {allowed user, denied user, server console, bad args,
   missing target, spectator target}. Denials must be graceful and logged sensibly.
7. **Config matrix**: defaults, each toggle off/on, deliberately broken YAML (plugin must
   enable on defaults + warn), extreme values clamped.
8. **Coexistence**: with the server's real plugin set (custom roles/spawn/damage plugins at
   minimum). Watch event vetoes (`IsAllowed`) and ordering assumptions; test with the
   conflicting plugin disabled to isolate.
9. **Population/performance sanity**: fill with players/bots where possible; watch tick
   behaviour, log spam, and memory across several rounds (`scpsl-optimization` for depth).
10. **Custom content (if any)**: full item lifecycle (give→use→drop→pickup by another→destroy→
    SCP-914), role assign→ability→death→reassign, spawn chance/min-player gates at boundaries.

Generate the tracking table with `python3 scripts/generate_test_matrix.py` (edit the feature
list in the script call per the help text) or copy [assets/test-matrix.md](assets/test-matrix.md).

## Bug reports that get fixed

Use [assets/bug-report.md](assets/bug-report.md). Non-negotiables: framework + exact versions,
plugin version/commit, **full log from boot to failure** (first exception, not the last),
minimal repro steps from a clean round, expected vs actual, and whether reload/round restart
changes it. "It crashed sometime last night" without a log is returned for evidence.

## Regression discipline

- The scenario list is append-only: every shipped bug becomes a permanent scenario with its
  bug ID. Re-run Layers 1+2 in full for releases; for hotfixes, run impacted families + 2,3,5.
- After any framework/game version bump (new LabAPI/EXILED/SCP:SL stable), the full Layer 2
  suite is the upgrade verification — pass it before updating the plugin's supported-version
  claims (see `scpsl-migration-release` bump checklist).
- Dual-target plugins run Layer 2 per adapter; shared Layer 1 results carry over. Record
  per-framework results separately — "tested" without a framework named is untested.

## Sign-off rule

QA sign-off names: scenarios run, scenarios skipped (with reason), known defects accepted
(by whom), and the exact build hash tested. Release (`scpsl-migration-release`) consumes this
verbatim; do not let release pressure rewrite history in the checklist.

## References

- [references/server-test-scenarios.md](references/server-test-scenarios.md) — detailed scenario catalogue
- [references/test-strategy.md](references/test-strategy.md) — layering, coverage bars, regression policy
