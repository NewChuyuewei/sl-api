---
name: scpsl-code-review
description: Reviews SCP Secret Laboratory plugin code written for LabAPI 1.1.7 or EXILED 9.15.3 — correctness, event lifecycle symmetry, main-thread safety, performance, security, config and API usage. Use when reviewing, auditing, or critiquing an SCP:SL plugin, pull request, or snippet, or when asked to check code quality before release.
license: MIT
compatibility: Reviews net48 C# against LabAPI 1.1.7 / EXILED 9.15.3 and SCP:SL 14.2.x. Works on source snippets, projects, or diffs; optional bundled scanner needs Python 3.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Code Review

Review for **server-killing defects first, style last**. A SCP:SL plugin runs inside a live
game server: a lifecycle leak lags every future round, a main-thread block freezes every
player, an unchecked command is an admin-escalation hole. The review must find those before
naming or formatting opinions.

Identify the framework first (LabAPI or EXILED — check base class, namespaces, `.csproj`
references). Review against that framework's pinned APIs only (1.1.7 / 9.15.3); code that is
"correct" for EXILED 8 or LabAPI 0.x is a finding, not a pass. Load the matching framework
skill when API behaviour is in question; load `scpsl-harmony-patching` if patches are present.

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
- Review order is fixed: lifecycle -> thread safety -> event correctness -> API/config/permissions -> state -> custom content -> Harmony -> security -> performance -> style.
- Every Blocker/Major cites file + location + runtime consequence + concrete fix. Scanner output is leads; a finding requires eyes-on confirmation.

### Stop conditions
- Framework or target version of the code under review cannot be determined: stop and establish it — reviewing against the wrong API line is worse than not reviewing.
- The change mixes feature + refactor + version bump inseparably: block and require a split (see scpsl-refactoring).

### Definition of done (this skill)
Done when the report ends with lifecycle verdict, top-3 risks, an explicit approve / approve-with-fixes / block, and an Unverified Claims section (empty only if truly none).

## Review order (do not skip ahead)

1. **Lifecycle symmetry** — the highest-defect area in SCP:SL plugins.
   Every registration in enable has an exact unregistration in disable: event `+=`/`-=` pairs
   (EXILED manual handlers — diff the two lists mechanically), `CustomHandlersManager`
   register/unregister (LabAPI), MEC coroutine handles killed, Harmony `UnpatchAll(ownId)`,
   custom roles/items unregistered, NPCs despawned, singletons/`Instance` nulled, static
   state cleared. Handler instances created at enable, not surviving in statics.
   Run the bundled scanner for a first pass: `python3 scripts/scan_plugin.py <project-dir>`
2. **Main-thread and timing safety.** No `Thread.Sleep`, `Task.Delay`, `async void` handlers,
   `.Result`/`.Wait()`, background-thread game access, or blocking IO in handlers.
   Delays/loops are MEC with stored handles. Fire-and-forget delayed actions re-validate
   plugin-enabled and player state when they fire.
3. **Event correctness.** Right event moment (`-ing` cancellable vs past-tense notification;
   LabAPI `Joined` ≠ EXILED `Verified`); incoming `IsAllowed` respected before overriding;
   args mutated through the intended surface; null guards on `ev.Player`/items/doors before
   use; no assumption that a joined/verified player is alive, in-round, or non-NPC;
   per-event work is small and allocation-light (hot events: damage, shots, item use).
4. **Framework API usage.** Wrappers preferred over `ReferenceHub`/game types; direct game
   access isolated and justified; config read via framework (properties only in LabAPI,
   `IConfig` + `[Description]` in EXILED); commands auto-registered (no double registration);
   permission node checked in **every** privileged command path; logging via framework logger,
   verbose output behind `Debug`. Version gates (`RequiredApiVersion`/`RequiredExiledVersion`)
   correct and not bypassed.
5. **State and cleanup.** State keyed by UserID/item serial (not wrappers); cleanup wired to
   round end, leave, death/role-change as applicable (see `scpsl-architecture` cleanup matrix);
   no round bleed; dictionaries cannot grow unboundedly during a round.
6. **Custom content (if present).** Ownership gate first line (`Check(...)` in EXILED,
   registry check in LabAPI); unique never-reused IDs; full item lifecycle decided
   (use/drop/pickup/destroy/914); role spawn gating (chance/min-players) in config.
7. **Harmony (if present).** The skill gate: written justification, smallest stable target,
   fail-open error handling, own ID unpatch, config kill-switch, no per-call reflection.
   An unjustified patch is a blocking finding.
8. **Security and robustness.** See `scpsl-security-permissions` for depth: argument
   validation (counts, ranges, target resolution), no trusting client-sent values, no
   hard-coded staff IDs/tokens, safe handling of UserIDs in logs, dependencies pinned
   and actually needed. Exceptions cannot escape handlers into the game loop.
9. **Performance.** Handled by `scpsl-optimization` for depth; in review, flag the patterns:
   LINQ/allocations in hot handlers, per-frame loops, uncached reflection/`GetComponent`-style
   lookups per event, string-built logging when disabled, file/DB writes in gameplay events.
10. **Clarity (last).** Naming, file organization (handlers/commands/services), comments on
    *why* (especially game-version-sensitive code), README/config docs updated. Never let
    style comments dilute blocking findings — separate the sections.

## Severity and reporting

Report with [assets/review-report.md](assets/review-report.md). Severities:

| Level | Meaning | Examples |
|---|---|---|
| **Blocker** | Breaks servers, rounds, security, or reload | Missing unsubscribe, main-thread block, missing permission check, unjustified Harmony skip-prefix, state keyed by wrapper |
| **Major** | Wrong behaviour in realistic conditions | Wrong event moment, `IsAllowed` overridden blindly, round bleed, broken-config crash, duplicate command registration |
| **Minor** | Debt that will bite later | Uncached lookups, unbounded dictionary, missing `[Description]`, magic numbers instead of config |
| **Nit** | Style/clarity only | Naming, comment quality, file layout |

Each finding: file + location, severity, **what happens at runtime** (not just "bad practice"),
and a concrete fix (code where short). End with: lifecycle-symmetry verdict, the top 3 risks,
and an explicit approve / approve-with-fixes / block recommendation. If you could not verify
an API claim against the pinned docs, say so instead of asserting.

## Fast heuristic scan

```bash
python3 scripts/scan_plugin.py path/to/plugin-project
```

It flags risky patterns (thread blocking, `UnpatchAll()` without ID, `+=`/` -=` mismatches
heuristics, hard-coded permission bypasses, TODO/HACK). Scanner output is leads, not verdicts —
confirm each hit in context before reporting it. See [references/common-defects.md](references/common-defects.md)
for the defect catalogue with runtime consequences, and
[references/review-checklist.md](references/review-checklist.md) for the full pass/fail checklist.
