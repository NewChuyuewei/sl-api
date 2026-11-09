# Code Review — <Plugin Name> (<framework + version>)

- Reviewer: <agent/human> · Date: <yyyy-mm-dd>
- Scope: <PR / files / full plugin> · Built against: LabAPI 1.1.7 | EXILED 9.15.3, SCP:SL 14.2.x
- Scanner run: `scan_plugin.py` <clean / N leads, each confirmed or dismissed below>

## Verdict
**Approve | Approve with fixes | Block** — <one sentence why>

## Lifecycle symmetry verdict
<Register/unregister audit result: events, coroutines, Harmony, custom content, singletons.>

## Findings
### Blockers
- [ ] `<file>:<line>` — <finding>. Runtime effect: <...>. Fix: <...>

### Major
- [ ] `<file>:<line>` — <finding>. Runtime effect: <...>. Fix: <...>

### Minor / Nits
- [ ] `<file>:<line>` — <finding>. Fix: <...>

## Top 3 risks if merged as-is
1.
2.
3.

## Unverified claims
<API behaviours the reviewer could not confirm against pinned LabAPI 1.1.7 / EXILED 9.15.3 docs.>
