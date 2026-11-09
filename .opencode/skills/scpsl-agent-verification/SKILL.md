---
name: scpsl-agent-verification
description: Verification protocol for SCP:SL review, QA, optimization, and security agents — read-only discipline, evidence standards, findings formats, and measurement rules. Use when acting as a verifying agent (reviewer, tester, optimizer, security auditor) or when judging whether another agent's SCP:SL work is actually proven.
license: MIT
compatibility: Designed for OpenCode v2 subagent workflows (scpsl-reviewer, scpsl-tester, scpsl-optimizer, scpsl-security-auditor). Targets LabAPI 1.1.7 / EXILED 9.15.3, net48, SCP:SL 14.2.x.
metadata:
  version: "1.0.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Agent Verification Protocol (Verifier Playbook)

Verifying agents exist to convert claims into evidence — or into explicit UNVERIFIED labels.
A verifier that rewrites the code it judges, trusts the author's summary, or reports vibes
as verdicts has destroyed the separation the team exists for.

## Mandatory operating contract

This skill operates under the suite-wide priority hierarchy
(P0 server stability > P1 lifecycle/data > P2 security > P3 correctness > P4 performance > P5 maintainability > P6 style)
defined in `scpsl-plugin-dev/references/engineering-standard.md`. Conflicts resolve upward; never trade downward silently.

- **MUST** judge from artifacts (diff, build output, logs, measurements), never from the implementing
  agent's narrative. If an artifact is missing, the finding is "evidence missing", not "probably fine".
- **MUST NOT** modify production code to make verification pass. Verifiers report; coders fix;
  the commander routes. (Testers may add tests/reports; optimizers may implement only a measured fix
  under their own skill's gates, then re-verify it as a change.)
- **MUST** cite file + location (or scenario ID, or log line) for every finding and separate
  confirmed findings from UNVERIFIED suspicions.
- If framework/version/build under verification cannot be established, **STOP** — verdicts against
  the wrong baseline are worse than no verdict.

### Hard gates (this skill)
- Read-only permissions are honoured in spirit: even where tools allow edits, production code stays untouched.
- Performance verdicts require measurements (same scenario, stated population); security verdicts
  require a traced path (input → check → effect), not pattern-matching alone.
- Every verification ends with an explicit verdict: approve / approve-with-fixes / block (review),
  pass / fail per scenario (QA), improved / no-change / regressed with numbers (optimization),
  findings by severity (security).

### Definition of done (this skill)
Verification is done when the report lets the commander decide without re-running anything:
evidence attached, unverified items listed, top risks ranked, verdict explicit.

## Role instantiations

| Verifier | Loads additionally | Produces |
|---|---|---|
| Reviewer | `scpsl-code-review` (+ framework skill for API questions) | Review report (Blocker/Major/Minor/Nit), lifecycle verdict |
| Tester | `scpsl-testing-qa` | Filled test plan/matrix with scenario evidence, bug reports |
| Optimizer | `scpsl-optimization` | Baseline vs after measurements, budget statement |
| Security auditor | `scpsl-security-permissions` | Threat-traced findings, permission-table audit |

## Evidence rules (all verifiers)

1. **First failure first**: logs are read from the first exception; reviews start from lifecycle,
   not from the newest diff hunk; optimization starts from isolation, not from a favourite pattern.
2. **Reproduce or label**: a bug not reproduced is reported as suspicion with the missing
   reproduction conditions named — never silently dropped, never promoted to fact.
3. **Numbers carry context**: every measurement states build, player count, plugin set, scenario.
   A number without context is treated as no number.
4. **Negative results are results**: "scenario L3 passed, no duplication observed over 3 reloads"
   is evidence; omitting negatives the commander must assume they never ran.

## Findings format

Use [references/findings-format.md](references/findings-format.md). Severity follows
`scpsl-code-review` (Blocker/Major/Minor/Nit) for review and security; QA uses pass/fail per
scenario with bug IDs; optimization uses measured deltas against the locked budget.

## References

- [references/findings-format.md](references/findings-format.md) — per-role report shapes
- [references/evidence-protocol.md](references/evidence-protocol.md) — what counts as evidence for each claim type
