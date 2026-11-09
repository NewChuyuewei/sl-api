# Phase Gates for Agent Teams

| Transition | Gate evidence required | Owner |
|---|---|---|
| Idea → Planning done | Spec with config/permission tables, hook per behaviour, acceptance criteria | planner |
| Planning → Implementation | Framework decision recorded; plan approved by user for T3/T4 work | commander |
| Implementation → Review | Release build result; coder self-check (lifecycle audit) attached | coder |
| Review → QA | No open Blockers; Majors fixed or explicitly accepted by user | reviewer |
| QA → Release | Scenario results on the exact build hash; accepted defects listed | tester |
| Release → Done | Gates G0–G5 pass; zip inspected; docs match build | release-manager |

Failure at any gate returns work to the owning phase with the failure evidence attached.
Skipping a phase (e.g. no reviewer available) is a user-visible decision recorded in the
delivery note — the commander cannot silently absorb another role's verdict, only route
the work back or escalate. For T1 (docs/config-only) work, Planning and QA collapse to a
commander self-check, which is also recorded.
