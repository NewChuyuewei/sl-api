---
name: scpsl-agent-implementation
description: Implementation protocol for SCP:SL coding agents — the build-verify loop, file ownership, self-review, and evidence return used by the LabAPI/EXILED coder and refactorer agents. Use when implementing or refactoring SCP:SL plugin code as an agent, before handing work to review or QA.
license: MIT
compatibility: Designed for OpenCode v2 subagent workflows (scpsl-labapi-coder, scpsl-exiled-coder, scpsl-refactorer). Targets LabAPI 1.1.7 / EXILED 9.15.3, net48, SCP:SL 14.2.x.
metadata:
  version: "1.0.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Agent Implementation Protocol (Coder Playbook)

A coding agent's job is a verified change, not a plausible diff. This protocol is the loop
every implementing agent (coders, refactorer) runs; the framework skills supply the APIs,
this skill supplies the discipline.

## Mandatory operating contract

This skill operates under the suite-wide priority hierarchy
(P0 server stability > P1 lifecycle/data > P2 security > P3 correctness > P4 performance > P5 maintainability > P6 style)
defined in `scpsl-plugin-dev/references/engineering-standard.md`. Conflicts resolve upward; never trade downward silently.

- **MUST** load the framework skill for the target (`scpsl-labapi` or `scpsl-exiled`) before writing code,
  and verify each API used against the pinned upstream. Unverifiable APIs are reported as `UNVERIFIED`, never guessed.
- **MUST NOT** edit files outside the task packet's ownership list, change invariants
  (config keys, permission nodes, commands, content IDs, Prefix) without commander approval, or mix frameworks in one assembly.
- **MUST** build in Release and run the self-review below before returning work. Unbuilt code is returned as UNVERIFIED.
- If the spec is ambiguous on framework, security, or gameplay behaviour, **STOP and return the question** — do not pick silently.

### Hard gates (this skill)
- Enable/disable symmetry is written in the same change as any registration (events, coroutines, patches, custom content).
- Every privileged command checks its permission node; every handler guards null/state/ownership first (framework skill patterns).
- No main-thread violations: MEC only for delays, no blocking/threaded game access, no IO in hot handlers.

### Definition of done (this skill)
Implementation is done when: Release build result is attached, self-review checklist passes line-by-line,
changed files stay inside ownership, and the return packet lists evidence + UNVERIFIED items + assumptions.

## The implementation loop

1. **Read the packet, then the spec, then the code.** Confirm framework, ownership, acceptance
   criteria. Explore the existing project shape before creating anything (mirror its layout and naming).
2. **Scaffold or extend** with the suite templates/scaffolder (`scpsl-plugin-dev`) for new projects;
   never hand-roll boilerplate that a template owns.
3. **Implement in dependency order**: config → Core/service logic → handlers → commands →
   presentation → cleanup. Cleanup is step 6 of the same change, not a later task.
4. **Build loop**: `dotnet build -c Release` (with the references properties from the framework
   template). Fix errors at the first failure; do not stack more code on a red build.
   Warnings about game references mean reference paths are wrong — fix paths, never downgrade pins.
5. **Self-review** (mini `scpsl-code-review` on your own diff): lifecycle lists diffed mechanically,
   hot handlers allocation-checked, permission nodes present, config defaults safe, no shipped
   game/framework DLLs introduced. Run `scpsl-code-review/scripts/scan_plugin.py` when available
   and triage every lead in the return packet.
6. **Return evidence** in the packet format: files changed (inside ownership), build command +
   result, scenario IDs suggested for QA, UNVERIFIED items, assumptions, blockers. Never write
   "done" without the build line.

## Refactor variant (scpsl-refactorer)

Same loop, stricter: invariant list from `scpsl-refactoring` governs; one move per commit-sized
step; characterization scenarios run before and after; behaviour deltas are defects, not
improvements, unless re-planned. If a move needs an invariant change, stop and return it as a
feature proposal.

## References

- [references/build-verify-loop.md](references/build-verify-loop.md) — build commands, failure triage, self-review checklist
