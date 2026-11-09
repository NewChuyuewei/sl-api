---
name: scpsl-optimization
description: Optimizes SCP Secret Laboratory plugin performance — profiling, hot event handlers, allocations, MEC coroutine loops, wrapper lookups, networking and logging costs on LabAPI 1.1.7 and EXILED 9.15.3 servers. Use when a plugin lags the server, spikes CPU or memory, spams logs, or needs a performance audit before release on populated servers.
license: MIT
compatibility: Profiles plugins built for LabAPI 1.1.7 / EXILED 9.15.3 on SCP:SL 14.2.x dedicated servers. Bundled log analyzer needs Python 3 and a server log file.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Performance Optimization

An SCP:SL server is a single main thread keeping 30+ players in sync. A plugin that is
"fine with 5 testers" can halve usable server capacity at full population. Optimize from
**measurement**, in the order the game actually spends time — never from intuition.

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
- No optimization without a before measurement, and no completion claim without an after measurement in the same scenario.
- Correctness guards (permission, ownership, null, IsAllowed checks) are never removed for speed without profiling proof they are the cost.

### Stop conditions
- The lag cannot be reproduced or isolated to a plugin/subsystem: stop optimizing and return to measurement (binary-search the plugin set).
- A proposed fix moves game-state work off the main thread or caches players/wrappers across rounds: reject the approach, not just the code.

### Definition of done (this skill)
Done when the top measured cost is reduced with numbers recorded, the budget is written into the report, and QA lifecycle/feature scenarios re-pass.

## Ground rules

1. **Measure first, on a populated server or the closest simulation.** Capture baseline
   (tick health/FPS as reported by the server, per-round log size, exception count) before
   touching code. A change without a before/after measurement is a guess — record both in
   [assets/optimization-report.md](assets/optimization-report.md).
2. **The main thread is sacred.** Anything that blocks it (IO, big loops, lock contention,
   sync-over-async) taxes every player simultaneously. Budget plugin work per event in
   microseconds, per tick in low milliseconds *total across all plugins*.
3. **Hot paths are events, not loops you chose.** Damage, shooting, item use, movement-adjacent
   and per-player recurring events fire hundreds of times per second at population. Optimize
   those first; a slow admin command run twice a round is irrelevant.

Quick triage with the bundled analyzer:

```bash
python3 scripts/analyze_server_log.py path/to/server-log.txt
```

It counts exceptions, warning sources, and repeated log lines (the usual fingerprints of
per-event spam and failing tight loops). See [references/profiling-guide.md](references/profiling-guide.md)
for full profiling method.

## The usual suspects, in hit order

1. **Per-frame / high-frequency loops.** MEC coroutines with sub-second waits iterating all
   players. Fix: throttle to the feature's real cadence (seconds), iterate only affected
   players (track a set; see `scpsl-architecture`), and skip work when the round/feature
   is inactive. `while(true)` loops without a disable/round exit are defects, not style.
2. **Allocations in hot handlers.** LINQ (`Where/Select/ToList`) per event, interpolated
   strings for disabled logs, `new List<>` per call, boxing via `object` parameters.
   Fix: plain loops with early-outs, cached collections reused across calls, cheap guard
   checks *first* (feature enabled? right role/item? `Check`/registry hit?) before any work,
   log lazily behind the Debug flag.
3. **Repeated expensive lookups.** Resolving players/rooms/doors, `GetComponent`-style or
   reflection lookups, dictionary rebuilding per event. Fix: cache at round start or on
   first use with explicit invalidation (map regenerates per round — caches must die with it).
   Wrapper references are cached only for the round, keyed lookups stay UserID/serial-keyed.
4. **Unbounded state growth.** Per-player dictionaries never cleaned, tracked serial lists
   only growing, log/history buffers. Fix: cleanup matrix from `scpsl-architecture`;
   verify with the longevity scenario (5+ rounds) in `scpsl-testing-qa`.
5. **Network fan-out.** Broadcasts/hints/effects sent to all players per event, AdminToy or
   DisplayKit updates per frame. Fix: send to affected players only, batch updates, respect
   the presentation cadence players can perceive (hints/broadcasts are seconds-scale media).
6. **IO on the main thread.** Config saves, stats writes, web calls inside gameplay events.
   Fix: batch + flush on round end/timer; pure-data IO may go off-thread, game-state
   application returns via MEC (never touch wrappers off the main thread).
7. **Exception-driven flow.** Code throwing/catching per event (failed parses, missing keys)
   pays exception costs at scale. Fix: `TryParse`/TryGet patterns, validate once at config load.

## Optimization workflow

1. Reproduce the complaint: which round moment, player count, plugin set? Isolate by
   disabling half the plugins (binary search) before blaming your own code.
2. Profile: server log analysis + targeted timing (`Stopwatch` around suspect handlers at
   Debug level, aggregated counters — never a log line per call). Identify the top 1-3 costs.
3. Fix the top cost only, re-measure the same scenario, record delta. Repeat. (Fixing five
   things at once teaches you nothing and ships regressions.)
4. Regression-guard with `scpsl-testing-qa` lifecycle + feature scenarios, then lock the win:
   note the budget (e.g. "hurt handler < 0.05 ms avg at 30 players") in the optimization report.

## Anti-patterns that are not optimizations

- Moving game-object work to background threads (crashes/corruption, not speed).
- Caching wrappers/players across rounds or disconnects (stale-object bugs).
- Removing permission checks, ownership `Check`s, or null guards "for speed" — they are
  almost never the cost; measure before sacrificing correctness.
- Micro-optimizing cold paths (enable, commands) while hot handlers allocate.
- Adding object pooling everywhere: pool only where profiling shows allocation pressure,
  starting with per-event collections in hot handlers.

## References

- [references/profiling-guide.md](references/profiling-guide.md) — measurement method, log analysis, timing harness patterns
- [references/hot-path-patterns.md](references/hot-path-patterns.md) — before/after patterns for handlers, loops, lookups, network, IO
