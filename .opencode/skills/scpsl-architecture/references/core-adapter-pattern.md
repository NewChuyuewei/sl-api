# Core / Adapter Pattern in Practice

## What belongs where

| Core (framework-free) | Adapter (LabAPI or EXILED project) |
|---|---|
| Damage/spawn/economy rules, chance rolls (seeded `Random` injected) | Event subscription and arg extraction |
| Cooldown and state-machine logic over IDs + primitives | Wrapper calls: roles, items, hints, broadcasts, CASSIE |
| Parsing/validation of command arguments (strings in, result out) | Permission checks against the framework, response presentation |
| Config *semantics* (clamping, derived values) | Config *classes* the framework serializes |
| DTO results: "give player X item Y, show text Z" as data | Executing DTO results against live game objects |

Litmus test: if a file needs `using LabApi…`, `using Exiled…`, `using UnityEngine`, or
`using PlayerRoles` to compile, it is adapter code — wherever it currently sits.

## Translation points

- **Inbound**: handler reads `ev.Player.UserId`, `ev.Player.Role`, item serial, damage amount →
  builds a Core request record. All null/state guards happen here, once.
- **Outbound**: Core returns a decision (allowed? modified amount? effects to apply as DTOs) →
  handler applies: mutate `ev` args where the event allows, else wrapper calls.
  Core never learns that `IsAllowed` exists; the adapter maps decision → `ev.IsAllowed`.
- **Enums**: Core defines `TeamKind`, `ItemKind`-style enums; one `GameMapping` static class
  per adapter converts to/from `RoleTypeId`/`ItemType`. A new game item then touches one file.

## Worked mini-example (damage modifier)

```text
EXILED handler OnHurting(ev):
  guard ev.Player/Attacker -> coreRequest(victimId, attackerTeam, weaponKind, baseDamage)
  decision = DamageRules.Evaluate(coreRequest, settings)
  ev.Amount = decision.Amount;  ev.IsAllowed = decision.Allowed

LabAPI handler OnPlayerHurting(args):  (exact args per LabAPI catalogue)
  same coreRequest built from wrapper values
  same Evaluate call
  args mapped back per LabAPI's arg contract
```

Both adapters share 100% of the rule and its unit tests; only extraction/mapping differs.
That is the entire point — protect it in review.
