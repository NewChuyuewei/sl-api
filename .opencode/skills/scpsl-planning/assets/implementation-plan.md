# Implementation Plan — <Plugin/Feature Name>

## Goal and framework decision
<One sentence + framework chosen (LabAPI / EXILED / dual-target) and why. Link the decision
from scpsl-plugin-dev's decision matrix.>

## Milestones
- [ ] M1 — Skeleton that loads: scaffold, config, one end-to-end behaviour, clean enable/disable logs
- [ ] M2 — Feature complete: all spec rules, commands/permissions, presentation
- [ ] M3 — Hardened release: edge cases, reload/round cycles, performance sanity, docs, release zip

## Task breakdown
| # | Task | Files/projects | Framework surface | Done-check |
|---|---|---|---|---|
| 1 | Scaffold project + config class |  | Plugin/Config | Builds; enables on test server |
| 2 |  |  | Event: |  |

## Risks and spikes
| Risk | Spike/fallback | Re-estimate trigger |
|---|---|---|
| API behaviour unverified | Verify in pinned docs/source (time-boxed) | Spike result |

## Test and release sequencing
- Acceptance scenarios → scpsl-testing-qa test plan: <link>
- Release steps → scpsl-migration-release checklist: <link>
