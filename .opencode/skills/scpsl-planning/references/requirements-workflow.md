# Requirements Workflow for SCP:SL Plugins

## Questions that must be answered before design

1. **Round moment**: when does this happen (join, spawn, role change, damage, item use,
   round end, admin action)? Name the candidate LabAPI/EXILED event for each answer.
2. **Actors**: which roles/teams/items trigger it, who is affected, who can opt out?
3. **Authority**: player-triggered, admin-triggered (command + permission node), or automatic?
4. **Visibility**: what do affected players, other players, spectators, and admins each see?
   (hint vs broadcast vs CASSIE vs console — they are not interchangeable)
5. **Persistence**: per-round only, per-player session, or cross-round (file/database)?
   Cross-round persistence needs an explicit storage decision in `scpsl-architecture`.
6. **Failure behaviour**: what happens on reload mid-round, disconnect mid-interaction,
   missing target, broken config? "Nothing breaks and the feature turns off" is the default answer.
7. **Coexistence**: which popular plugins might touch the same events (custom roles, spawn
   systems, damage modifiers)? Note ordering assumptions as risks, not facts.

## Writing behaviour rules

- Rules are numbered, testable statements: "When a Chaos Insurgent picks up item X, then Y
  within Z seconds, unless round is ending." Each rule names trigger, condition, effect, exception.
- Separate **rules** (gameplay truth) from **presentation** (how it is shown) from
  **tuning** (numbers in config). Tuning changes must never require a rule rewrite.
- Record rejected alternatives in one line each ("considered per-frame check; rejected —
  event exists"). Future agents will otherwise re-litigate settled decisions.

## The spec quality bar

A spec is done when: a second agent can scaffold the project from it alone; every config key
and permission node is named; every behaviour names its hook; acceptance scenarios can be
written from it without asking questions. Anything less goes back with questions, not guesses.
