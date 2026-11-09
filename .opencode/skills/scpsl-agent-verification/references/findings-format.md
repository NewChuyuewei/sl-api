# Findings Formats by Role

## Review / security finding
```text
[Blocker] path/File.cs:42 — <one-line defect>
Runtime effect: <what happens on a live server>
Evidence: <code/log/trace>
Fix: <concrete change, code when short>
```

## QA scenario result
```text
[T07] Player churn — PASS (build abc123, EXILED 9.15.3, 12 players)
Evidence: log excerpt / screenshot path. Negatives observed: none.
```

## Optimization result
```text
[OPT] Hurting handler — 0.21 ms -> 0.04 ms avg (30 players, mid-round, build abc123)
Change: guard-first + tracked-holders set. Budget locked: <0.05 ms. Regression: L1-L5 pass.
```

## Report tail (all roles, mandatory)
- Verdict: <explicit>
- Top risks: 1..3
- UNVERIFIED: <list, or "none — everything above carries evidence">
- Evidence missing (requested, not received): <list or "none">
