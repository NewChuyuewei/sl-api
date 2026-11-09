---
name: scpsl-refactoring
description: Refactors and modernizes SCP Secret Laboratory plugin codebases — behaviour-preserving cleanup, legacy API upgrades to LabAPI 1.1.7 or EXILED 9.15.3, extracting a shared Core, removing lifecycle and state debt. Use when cleaning up, restructuring, upgrading, or paying down technical debt in an existing SCP:SL plugin without changing its gameplay behaviour.
license: MIT
compatibility: Works on net48 SCP:SL plugin codebases targeting LabAPI 1.1.7 / EXILED 9.15.3 (or legacy stacks being upgraded to them), SCP:SL 14.2.x.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Refactoring and Modernization

Refactoring means **same behaviour, better structure** — verified, not asserted. The moment a
"refactor" changes a config key, an event moment, a permission node, or a balance number, it
became a feature change and needs planning + QA sign-off as one. Keep the two apart ruthlessly.

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
- The invariant list (config keys/defaults, permission nodes, commands, content IDs, data format, Prefix) is written before the first edit.
- One behaviour-preserving move per commit, each followed by build + smoke + touched-feature scenarios. Scanner finding count never increases.

### Stop conditions
- No characterization safety net exists and none can be built for the area being touched: stop and build the minimal net first.
- A move requires changing an invariant: stop — it is now a feature change and re-enters via scpsl-planning.

### Definition of done (this skill)
Done when invariants verify unchanged, scenario suite passes identically to baseline, and no temporary duality lacks a named finishing task + date.

## Before touching anything

1. **Characterize current behaviour.** Run the plugin; record the acceptance-relevant
   behaviours (or reconstruct them into scenarios with `scpsl-testing-qa` — a plugin with no
   test net gets a minimal one *first*: boot, lifecycle cycle, top 3 features). Refactoring
   without a safety net is rewriting with extra steps.
2. **Baseline the smells.** Run `../scpsl-code-review/scripts/scan_plugin.py`, note hot spots:
   giant Plugin/handler classes, wrapper-keyed state, duplicated wrapper lookups, static
   state webs, per-event allocations, copy-pasted framework code. Rank by *risk × churn*
   (code that changes often and breaks rounds first), not by ugliness.
3. **Declare the invariant list** in the PR/plan: public contracts that will not change —
   config keys + defaults, permission nodes, command names/aliases, custom content IDs,
   persisted-data format (or its migration), plugin `Prefix`/`Name`. Any change to this list
   stops the refactor and re-plans.

## Safe refactoring moves (in usual order)

1. **Lifecycle repair first** (highest value, smallest diffs): pair every unsubscribe, kill
   orphaned coroutines, null singletons, add round-end cleanup. Verify with the disable/
   enable/restart cycle after *each* move.
2. **Guard-clause flattening** in handlers: null/state/ownership checks as early returns;
   behaviour identical, nesting gone. Extract repeated guard+lookup chains into one helper.
3. **Extract services from handlers/Plugin class**: move rules into plain classes taking
   primitives (the `scpsl-architecture` handler/service split). Move one feature at a time;
   handlers keep calling the same logic through the new seam.
4. **De-static**: static dictionaries/fields → owned service instances created in enable,
   disposed in disable. Key state by UserID/serial as debt is touched (never leave a
   wrapper-keyed dictionary behind after "cleaning" its file).
5. **Extract Core** (when dual-target or testability justifies it): pure logic moves to the
   framework-free project; adapters keep translation. Do this feature-by-feature behind
   the existing handler surface — big-bang moves are rejected.
6. **Config hygiene**: magic numbers → named config with *the same effective defaults*;
   add `[Description]`/comments; validation/clamping at load where absent. Defaults are
   contracts: extract the current hard-coded value, don't "improve" it mid-refactor.
7. **Dead code and obsolete API removal**: unsubscribed handlers, unused commands, references
   to pre-1.1.7 LabAPI / pre-9 EXILED APIs, commented-out blocks. Delete; git remembers.

## Legacy modernization tracks

- **NwPluginAPI / LabAPI 0.x → LabAPI 1.1.7**: rebuild on `Plugin`/`Plugin<TConfig>`,
  wrapper-based access, `CustomEventsHandler`; folder/loader layout changed entirely —
  treat as adapter rewrite behind characterization tests, using `scpsl-migration-release`
  mapping discipline. Old event/handler names do not transliterate.
- **EXILED ≤8 → 9.15.3**: namespace/args moves (including LabAPI-args re-exports in 9.x),
  CustomItems/CustomRoles API changes, required-version gate. Diff against `Exiled.Example`
  at 9.15.3; upgrade references first, fix breaks in lifecycle→events→custom-content order.
- **Direct game-type code → wrappers**: replace `ReferenceHub` plumbing with wrapper calls
  feature by feature; where no wrapper exists, isolate behind one `GameAccess`-style class
  (see `scpsl-harmony-patching` discipline) instead of scattering game types.
- Harmony found during modernization goes through the patching skill's gate again — many
  legacy patches exist only because an event didn't *then*; if it exists now, the patch dies.

## Verification protocol (per move, not per branch)

Build green → boot smoke (clean log) → lifecycle cycle → the characterization scenarios for
the touched feature → `scan_plugin.py` diff (findings must not increase). For state/lifecycle
moves, add the longevity scenario (multi-round). One move per commit, message naming the
invariant preserved ("refactor: extract SpawnService, no behaviour change") so a bisect can
find the guilty step when — not if — something subtle shifts.

## Anti-patterns

- Refactoring + rebalancing + framework port in one branch (three risks, zero attribution).
- Renaming config keys/permission nodes "while I'm here" (breaks every server owner silently).
- Rewriting from scratch because reading the old code is slower — allowed only with a written
  decision (ADR via `scpsl-architecture`) showing the characterization suite will be re-run
  against the rewrite, plus feature-parity checklist from `scpsl-planning`.
- Leaving the codebase half-migrated: two patterns for the same concern (two state stores,
  two handler styles) with no tracked finish task. Temporary duality gets a named issue and date.
