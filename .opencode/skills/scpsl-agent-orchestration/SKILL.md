---
name: scpsl-agent-orchestration
description: Orchestrates the SCP:SL OpenCode agent team — delegation, task packets, phase gates, parallel work, conflict resolution, and evidence-based delivery led by the scpsl-commander agent. Use when coordinating multiple agents, splitting a plugin task across specialists, reviewing subagent output, or deciding whether work should be delegated at all.
license: MIT
compatibility: Designed for OpenCode v2 primary/subagent workflows and the scpsl-* agent team. Targets LabAPI 1.1.7 / EXILED 9.15.3, net48, SCP:SL 14.2.x.
metadata:
  version: "1.0.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Agent Orchestration (Commander Playbook)

The commander owns the outcome, not the typing. It plans, delegates with complete task
packets, enforces phase gates between specialists, integrates results, and is the only
voice that declares work done. A commander that codes around its own process has failed
before the first bug.

## Mandatory operating contract

This skill operates under the suite-wide priority hierarchy
(P0 server stability > P1 lifecycle/data > P2 security > P3 correctness > P4 performance > P5 maintainability > P6 style)
defined in `scpsl-plugin-dev/references/engineering-standard.md`. Conflicts resolve upward; never trade downward silently.

- **MUST** verify every framework API claim against the pinned upstream (LabAPI 1.1.7 / EXILED 9.15.3)
  or have the responsible specialist verify it; anything unverifiable is written as `UNVERIFIED: <claim>`.
- **MUST NOT** accept subagent self-reports at face value: "done", "tested", and "reviewed" are claims
  requiring the evidence defined in `scpsl-agent-verification`.
- **MUST** keep one writer per file set at a time. Parallel agents get disjoint file ownership or do not run in parallel.
- If a missing fact would change framework, architecture, security, or gameplay behaviour, **STOP and ask the user**.
  Otherwise state the assumption in the task packet and proceed.

### Hard gates (this skill)
- No implementation delegation before the framework decision (LabAPI / EXILED / dual-target) and, for non-trivial
  work, a plan (`scpsl-planning`) exist in writing.
- No phase advances on a failed gate (planning → implementation → review → QA → release);
  the gate-failure protocol in `scpsl-plugin-dev/references/quality-gates.md` applies.
- The commander never edits plugin code directly except trivial integration fixes it then sends back through review.

### Definition of done (this skill)
Orchestration is done when every deliverable has: an owner, evidence, a review verdict, QA scenario results
(or an explicit unverified label), and a delivery note listing open gates — addressed to the user in one place.

## Delegation decision rules

Delegate when a task is: specialist-shaped (framework coding, review, QA, optimization, security,
docs, release), independently verifiable, and large enough that context separation helps.
Do NOT delegate when: the task is a one-line answer, the packet would contain the whole
conversation anyway, file ownership would collide, or the user asked a direct question.
Default subagent depth is 1 — subagents cannot reliably chain-delegate, so the commander
sequences phases itself instead of asking a coder to "also get it reviewed".

## The team (OpenCode `.opencode/agents/`)

| Agent | Delegate for | Never delegate to it |
|---|---|---|
| `scpsl-planner` | Specs, task breakdowns, acceptance criteria | Code, verdicts |
| `scpsl-labapi-coder` | LabAPI implementation | EXILED code, release sign-off |
| `scpsl-exiled-coder` | EXILED implementation | LabAPI code, release sign-off |
| `scpsl-reviewer` | Code/PR review, scanner triage | Fixes (it reports; a coder fixes) |
| `scpsl-tester` | Test execution, scenario evidence, bug reports | Production-code fixes |
| `scpsl-optimizer` | Measured performance work | Unmeasured "optimizations" |
| `scpsl-security-auditor` | Permission/privacy/input audits | Feature implementation |
| `scpsl-refactorer` | Behaviour-preserving cleanup/modernization | Behaviour changes disguised as cleanup |
| `scpsl-documenter` | README/config/command docs, changelog | Inventing undocumented behaviour |
| `scpsl-release-manager` | Gate checks, packaging, deployment, release notes | Waiving its own gates |

## Task packet protocol

Every delegation uses the packet in [assets/task-packet.md](assets/task-packet.md):
objective, framework + pins, inputs (spec/files), file ownership, skills to load, constraints,
acceptance criteria, and the exact evidence to return. A packet missing framework, ownership,
or acceptance criteria is sent back to the commander (itself) to fix — never forwarded vague
and "clarified later" by the subagent guessing.

Sequencing defaults: planner → (labapi|exiled)-coder → reviewer → tester → release-manager,
with security-auditor before release for anything touching permissions/data, optimizer only on
measured evidence, documenter alongside implementation (not after). Independent packets
(e.g. review of module A + tests of module B) may run in parallel; dependent ones never do.
See [references/delegation-protocol.md](references/delegation-protocol.md) and
[references/phase-gates.md](references/phase-gates.md).

## Integration and conflict resolution

- Integrate by evidence, not seniority: reviewer findings cite lines, tester cites scenarios,
  optimizer cites numbers. Conflicts (reviewer vs coder, tester vs spec) resolve against the
  priority hierarchy, then against the written spec, then by asking the user — in that order.
- A subagent reporting UNVERIFIED on a load-bearing claim blocks its phase. The commander
  either gets it verified (upstream source, build, server test) or downgrades the delivery.
- Partial results are reported as partial, with open gates named. The commander never
  rewrites a subagent's "unverified" into "done" in the summary. Handoff format:
  [assets/delivery-note.md](assets/delivery-note.md).

## References

- [references/delegation-protocol.md](references/delegation-protocol.md) — packet fields, parallelism, ownership
- [references/phase-gates.md](references/phase-gates.md) — phase transitions and failure handling
