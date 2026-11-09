---
name: scpsl-planning
description: Plans SCP Secret Laboratory plugin work — requirements, feature specifications, scope, milestones, task breakdown, estimation, risk, and acceptance criteria for LabAPI 1.1.7 or EXILED 9.15.3 plugins. Use when the user wants a plan, roadmap, feature spec, implementation plan, task list, or estimate before writing SCP:SL plugin code.
license: MIT
compatibility: Works with the scpsl-plugin-dev router skill. Targets LabAPI 1.1.7 / EXILED 9.15.3, net48, SCP:SL 14.2.x. No network or SDK required for planning artifacts.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Planning

Turn a vague plugin idea into an implementable, testable plan. Planning output must be
concrete enough that an agent (or human) can implement without re-deciding the design,
and small enough that the first version can ship and be tested on a real server.

Load `scpsl-plugin-dev` first if the framework (LabAPI / EXILED / dual-target) is undecided —
the plan must name the framework in its first section. Never plan against beta/dev APIs.

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
- No plan is delivered without: named framework + pins, config table, permission table, hook per behaviour, and acceptance criteria.
- Every API uncertainty is a named, time-boxed spike task. Estimates touching unverified APIs are labelled as ranges contingent on the spike.

### Stop conditions
- The one-sentence goal cannot be written, or two stakeholders' goals conflict: stop and clarify before estimating anything.

### Definition of done (this skill)
A plan is done when a second agent can scaffold and implement from it without a single design question, and QA can derive scenarios from it mechanically.

## Planning workflow

1. **Restate the goal in one sentence.** Who benefits (players, admins, server owners),
   what changes in a round, and how success is observed. If this sentence is fuzzy, ask
   clarifying questions before writing the plan — do not paper over ambiguity with scope.
2. **Fix the constraints.** Framework + pins (LabAPI 1.1.7 / EXILED 9.15.3 / dual-target),
   server type (public/private, player count), gameplay impact and transparently-modded
   implications, dependencies on other plugins, and any deadline/version target.
3. **Write the feature specification** using [assets/feature-spec.md](assets/feature-spec.md):
   behaviour rules, triggers (events/commands), config surface, permissions, player-facing
   presentation (hints/broadcasts/CASSIE), edge cases, and explicit non-goals.
4. **Break down tasks** using [assets/implementation-plan.md](assets/implementation-plan.md).
   Each task: one deliverable, files/projects touched, framework surface used
   (event/command/config/custom content/Harmony), and a done-check. Order tasks so the
   plugin loads and does *something testable* as early as possible
   (scaffold → config → one handler → presentation → commands → polish).
5. **Estimate and sequence into milestones.** M1 = loadable skeleton + core mechanic on a
   test server; M2 = complete feature + config/permissions; M3 = hardening (reload/round
   cycles, edge cases), docs, release. Call out the critical path (usually custom content
   or Harmony) explicitly. Use ranges, not fake precision; state what the estimate excludes.
6. **List risks and open questions** with an owner and a decision deadline per item:
   API uncertainty (verify in pinned docs), event-ordering vs other plugins, performance
   on full servers, balance, config migration. Convert API uncertainty into a named
   spike task ("verify X in LabAPI 1.1.7 source, 1h") instead of a plan-wide fudge factor.
7. **Define acceptance criteria** per feature using [assets/acceptance-criteria.md](assets/acceptance-criteria.md):
   observable in-game behaviour, reload/restart behaviour, denied-permission behaviour,
   and the exact test scenarios `scpsl-testing-qa` will execute. "Works on my server"
   is not an acceptance criterion.

## Planning rules for SCP:SL specifics

- **Every feature maps to a hook.** For each behaviour, name the event/command/timing
  mechanism it will use (or flag "hook unknown — spike"). Plans that say "when X happens"
  without naming the LabAPI/EXILED event are unfinished.
- **Config and permissions are planned, not improvised.** List every config key (name,
  default, meaning) and every permission node in the spec. Renaming them later breaks
  server owners — see the framework skills.
- **Lifecycle is a feature.** Round start/end, player disconnect, plugin reload/disable
  behaviour, and state cleanup appear as explicit tasks, never as an afterthought.
- **Dual-target costs double adapters.** If both frameworks must ship, the plan shows:
  shared Core work once, adapter work twice, test matrix twice. Do not estimate dual-target
  as "one plugin plus a little".
- **Custom content and Harmony inflate risk.** Custom roles/items (EXILED) and any Harmony
  patch get their own milestone, their own test scenarios, and a fallback
  (feature-flag off) in the plan.
- **Balance is a planned iteration.** Spawn chances, cooldowns, caps, and team impact ship
  as config with conservative defaults, plus a planned tuning pass after playtesting.
  Hard-coding balance numbers fails review.
- **Scope discipline.** Maintain a visible non-goals list and a "later" list. New ideas
  found mid-implementation enter the later list unless they block the milestone's
  acceptance criteria.

## Outputs (match the situation, don't over-produce)

| Situation | Deliverable |
|---|---|
| Single small feature | One filled `feature-spec.md` + task list (5-15 tasks) + acceptance criteria |
| New plugin | Spec + `implementation-plan.md` with milestones, risks, config/permission tables |
| Multi-plugin / dual-target | Above, plus Core/adapter split (hand off to `scpsl-architecture`) and per-framework test matrix |
| Estimate request | Milestone ranges with assumptions, exclusions, and the top 3 risks that move the number |

## Handoffs

- Framework choice / scaffolding → `scpsl-plugin-dev`, then `scpsl-labapi` or `scpsl-exiled`
- Solution structure, state, inter-plugin APIs → `scpsl-architecture`
- Test scenarios from acceptance criteria → `scpsl-testing-qa`
- Release sequencing → `scpsl-migration-release`

## References

- [references/requirements-workflow.md](references/requirements-workflow.md) — eliciting and fixing requirements
- [references/estimation-and-milestones.md](references/estimation-and-milestones.md) — task sizing, milestones, risk buffers
