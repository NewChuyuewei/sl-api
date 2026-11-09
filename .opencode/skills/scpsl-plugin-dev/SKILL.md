---
name: scpsl-plugin-dev
description: Routes and plans SCP Secret Laboratory (SCP:SL / SCPSL) server plugin development across LabAPI 1.1.7 and EXILED 9.15.3. Use when starting a new SCP:SL plugin, choosing LabAPI vs EXILED, scaffolding a net48 C# plugin project, structuring a dual-target solution, or deciding which framework API, event system, or skill to use for a feature.
license: MIT
compatibility: Requires a .NET SDK 8+ (10 recommended) targeting net48, SCP:SL dedicated server 14.2.x reference assemblies, LabAPI 1.1.7 and/or EXILED 9.15.3. Designed for Claude Code and Agent Skills compatible runtimes.
metadata:
  version: "1.2.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Development — Router and Shared Workflow

This skill is the entry point for every SCP: Secret Laboratory plugin task.
It decides **which framework**, **which architecture**, and **which sibling skill** to load.
It does not replace the framework skills — it routes to them.

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
- Framework decision (LabAPI / EXILED / dual-target) is stated with its reason before any implementation skill is loaded.
- Latest-stable pins only: a request for beta/dev/legacy APIs stops for explicit user confirmation.

### Definition of done (this skill)
Routing is done when: framework chosen + reason recorded, correct stage/framework skill(s) named, pins confirmed, scaffold command identified.

## Pinned targets — never guess versions

| Component | Pin |
|---|---|
| Game / dedicated server | SCP:SL **14.2.x** |
| LabAPI | **1.1.7** (`Northwood.LabAPI`, net48, bundled with the server) |
| EXILED | **9.15.3** (GitHub release; NuGet `ExMod.Exiled` may lag at 9.15.2) |
| Target framework | **net48** for every plugin and shared-core project |
| Harmony | 2.2.2, only via `scpsl-harmony-patching` |

Full details, URLs, and bump procedure: [references/version-pins.md](references/version-pins.md).
If the user asks for a beta, pre-release, EXILED 10, or a dev branch, stop and confirm —
this suite targets latest **stable** only.

## Step 1 — Choose the framework

Read [references/decision-matrix.md](references/decision-matrix.md), then apply these defaults:

- **LabAPI** when: the server runs vanilla LabAPI only, the plugin is admin tooling / QoL /
  "transparently modded" friendly, minimal dependencies matter, or the feature maps cleanly
  onto LabAPI wrappers and events. Official and bundled with the game since 14.1.
- **EXILED** when: the server already runs EXILED, the feature needs CustomRoles / CustomItems /
  CustomAbilities, EXILED permissions or translations, NPC helpers, or the mature EXILED ecosystem.
- **Dual-target** when: the plugin must ship for both kinds of servers. Build **two adapter
  assemblies over one shared core** (below). Never reference LabAPI and EXILED in the same
  plugin assembly, and never subscribe to both event systems for the same behaviour.
- **Either** when the user has no preference: default to **LabAPI** for a brand-new standalone
  plugin (zero extra loader for the server owner), unless a required feature exists only in
  EXILED — then say so explicitly and use EXILED.

State the choice and the reason in one sentence before writing code.

## Step 2 — Load the right sibling skill

**Full lifecycle, in order** — load the stage skill that matches where the work actually is:

| Stage / need | Load |
|---|---|
| Plan, spec, milestones, estimation, acceptance criteria | `scpsl-planning` |
| Solution structure, Core/adapters, state, persistence, inter-plugin APIs | `scpsl-architecture` |
| LabAPI plugin class, events, wrappers, commands, config | `scpsl-labapi` |
| EXILED plugin class, events, commands, config, translations | `scpsl-exiled` |
| Custom roles / items / weapons / abilities / NPCs | `scpsl-custom-content` (+ the framework skill) |
| No event exists; must change base-game behaviour | `scpsl-harmony-patching` (+ the framework skill) |
| Permission design, input validation, player-data privacy | `scpsl-security-permissions` |
| Review / audit code or a PR before merge or release | `scpsl-code-review` |
| Test plans, server scenarios, regression, bug reports | `scpsl-testing-qa` |
| Lag, CPU/memory, hot handlers, log spam — measure and optimize | `scpsl-optimization` |
| Cleanup, legacy upgrade, behaviour-preserving restructuring | `scpsl-refactoring` |
| README, config/command docs, changelog, API docs | `scpsl-documentation` |
| Port LabAPI ↔ EXILED, deploy, debug load errors, release | `scpsl-migration-release` |

Load only what the task needs. Do not load both framework skills for a single-framework plugin,
and do not load review/optimization skills to write green-field code (or vice versa).

## Step 3 — Scaffold, do not hand-write boilerplate

```bash
python3 <this-skill>/scripts/new_plugin.py --api labapi --name MyPlugin --output ./MyPlugin
python3 <this-skill>/scripts/new_plugin.py --api exiled --name MyPlugin --output ./MyPlugin
# Dual-target: scaffold both adapters, then add the shared core project by hand (see below).
```

The scaffolder copies the matching `assets/` template from `scpsl-labapi` or `scpsl-exiled`
and substitutes `__PLUGIN_NAME__` / `__AUTHOR__` / version tokens.
If the script is unavailable, copy the template folder manually and rename tokens.

## Architecture rules (all frameworks)

1. **One plugin assembly = one framework.** File layout per assembly:
   `Plugin` entry point, `Config`, `EventHandlers/` (or LabAPI `CustomEventsHandler`),
   `Commands/`, `Services/` for logic. Keep the entry point thin.
2. **Shared core for dual-target work.** Pure logic (rules, maths, parsing, state machines)
   goes in a `*.Core` net48 class library with **no** LabAPI, EXILED, Unity, Mirror, or
   `Assembly-CSharp` references. Adapters translate framework events ↔ core calls.
   This is also the only unit-testable layer — test it off-server.
3. **Lifecycle symmetry.** Everything registered in enable must be unregistered in disable:
   events, coroutines (`Timing.KillCoroutines`), Harmony patches, custom roles/items,
   singleton/`Instance` fields. Asymmetric cleanup is the #1 cause of "plugin works until reload".
4. **Main thread only.** Game objects, roles, items, and players are Unity main-thread state.
   Never touch them from `Task.Run`, timers, sockets, or background threads. Use MEC
   (`Timing.CallDelayed`, `Timing.RunCoroutine`) for delays and loops; kill coroutines on disable
   and on round end where appropriate. Never block the main thread (no `Thread.Sleep`,
   no sync-over-async, no disk/network IO in event handlers).
5. **Null and state guards first.** In every handler: check `ev.Player` / sender, round state,
   role, and `IsAllowed`-style cancellation flags before acting. Players disconnect mid-event;
   wrappers can be null or stale (LabAPI) / players mid role-change (EXILED).
6. **Config over hard-coding.** Durations, chances, item/role IDs, messages, and toggles belong
   in config with safe defaults and `[Description]`/comments. A broken config must never crash
   the plugin — fall back to defaults and log a warning.
7. **Permissions on every command.** Remote Admin and client commands check a permission node
   before doing anything privileged; deny with a clear response. Never trust arguments.
8. **Log through the framework** (`Logger` in LabAPI, `Log` in EXILED), prefix-free and concise.
   Guard verbose logging behind a `Debug` config flag. Never log tokens, full UserIDs at info
   level, or per-frame spam.
9. **No copy-paste between frameworks.** Event names, arg properties, cancellation semantics,
   and player wrappers differ. Translate via the mapping table in `scpsl-migration-release`,
   do not transliterate.
10. **Verify against pinned source.** Prefer the pinned upstream examples
    (`LabApi.Examples`, `Exiled.Example`) and API docs over memory or old forum snippets.
    EXILED ≤ 8 / NwPluginAPI / LabAPI 0.x snippets found online are obsolete — do not reuse
    their namespaces, handler names, or folder layouts.

## Shared workflow for any feature

0. **Plan** (`scpsl-planning`) for anything beyond a one-line change: spec, task breakdown,
   config/permission tables, acceptance criteria. **Architect** (`scpsl-architecture`) when
   the solution shape, shared Core, state, or persistence is non-trivial.
1. Restate the feature in one line and name the framework (Step 1).
2. Find the event/hook: search the framework skill's event catalogue first; only consider
   Harmony (topic skill) if no event or wrapper method exists.
3. Scaffold or locate the plugin project; confirm `net48`, LangVersion, and references resolve
   (LabAPI: NuGet `Northwood.LabAPI` 1.1.7 or bundled `LabAPI.dll`; EXILED: `ExMod.Exiled`
   or release DLLs — see framework skills).
4. Implement in this order: config → core logic/service → event handlers → commands → cleanup.
   Design permissions and validate all input per `scpsl-security-permissions`.
5. Build in Release. Fix warnings about missing game references by checking
   `SCPSL_Data/Managed` paths — never by downgrading the framework pin.
6. **Review** (`scpsl-code-review`, incl. its scanner) → **test** (`scpsl-testing-qa`:
   lifecycle/round/coexistence scenarios from the acceptance criteria) → fix and re-run.
7. Deploy and release per `scpsl-migration-release`; write owner docs with
   `scpsl-documentation`. If the server lags under load, switch to `scpsl-optimization`
   (measure first) rather than guessing; pay down structure debt with `scpsl-refactoring`.

## Common traps

- Mixing `LabApi.*` event registration with `Exiled.Events.*` in one assembly.
- Using EXILED's publicized `Assembly-CSharp-Publicized.dll` assumptions in a LabAPI plugin
  (LabAPI plugins normally reference the plain `Assembly-CSharp.dll` and stay on wrappers).
- Forgetting that EXILED 9.15 itself references LabAPI internally — that does **not** make an
  EXILED plugin a LabAPI plugin, and LabAPI servers without EXILED cannot load it.
- Treating CASSIE, broadcasts, and hints as interchangeable; each framework wrapper differs.
- Shipping `Assembly-CSharp.dll`, `LabAPI.dll`, `Exiled.*.dll`, Unity, or Mirror DLLs inside a
  plugin release zip. Ship only the plugin DLL (+ real third-party dependencies).

## Final self-verification (run before every delivery)

The agent MUST NOT deliver until each applicable line is answered with evidence:

1. Framework + pins named? Every API claim verified against pinned upstream, or labelled `UNVERIFIED`?
2. Lifecycle symmetry written/checked (register↔unregister, coroutines, patches, custom content)?
3. Main-thread discipline kept (MEC only, no blocking/threaded game access)? Null/state/permission guards in place?
4. Config defaults safe, keys/nodes/IDs stable (or migration note written)? Presentation targets only intended players?
5. Risk tier (T1–T4) declared; gates G0–G5 for that tier pass or are listed as open with owners?
6. Delivery note uses verified vocabulary honestly: built (command+result), tested (scenario IDs+hash), reviewed (report) — or explicitly *not yet*?

If any answer is "no" and cannot be fixed in this turn, deliver as **in-progress** and name the open
gates. Confidence is not evidence; an elegant explanation is not a test.

## References

- [references/version-pins.md](references/version-pins.md) — exact versions, URLs, bump checklist
- [references/decision-matrix.md](references/decision-matrix.md) — LabAPI vs EXILED vs dual-target
- [references/glossary-and-layouts.md](references/glossary-and-layouts.md) — terms, server folder layouts, solution layout
- [references/engineering-standard.md](references/engineering-standard.md) — production engineering bar: priority hierarchy P0–P6, evidence standard, review culture
- [references/quality-gates.md](references/quality-gates.md) — gates G0–G5 by risk tier T1–T4, gate-failure protocol
