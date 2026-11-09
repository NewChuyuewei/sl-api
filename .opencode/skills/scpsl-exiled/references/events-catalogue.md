# EXILED 9.15.3 — Events Catalogue (orientation, not exhaustive)

## Where events live

- Subscribe: `Exiled.Events.Handlers.<Domain>.<Event> += handler`
  (e.g. `Exiled.Events.Handlers.Player.Verified += OnVerified`).
- Args: `Exiled.Events.EventArgs.<Domain>.<Event>EventArgs`
  (e.g. `Exiled.Events.EventArgs.Player.VerifiedEventArgs`).
- Confirm every signature in the pinned API reference (https://exiled-team.github.io/EXILED)
  or the 9.15.3 `Exiled.Events` source — event sets grow between minor releases.

## Domains and representative events

| Handler class | Representative events |
|---|---|
| `Player` | `Verified`, `PreAuthenticating`, `Joined`-equivalents via `Verified`, `Left`, `Destroying`, `ChangingRole`, `Spawned`, `Hurting`, `Dying`, `Died`, `ChangingItem`, `UsingItem`, `PickingUpItem`, `DroppingItem`, `Shooting`, `ReloadingWeapon`, `ReceivingEffect`, `Escaping`, `UnlockingGenerator`, pocket-dimension events |
| `Server` | `WaitingForPlayers`, `RoundStarted`, `RestartingRound`, `RoundEnded`, `RespawningTeam`, `ReloadedConfigs`-style reload hooks |
| `Map` | `ExplodingGrenade`, `GeneratorActivating`, decontamination, door/LCZ events |
| `Warhead` | `Starting`, `Stopping`, `Detonated` lifecycle |
| `Item` | `ChangingAmmo`, `ChangingAttachments`, `ReceivingPreference` |
| `Scp914` | `Activating`, `ChangingKnobSetting`, `UpgradingPlayer`, `UpgradingPickup` |
| Per-SCP (`Scp049`, `Scp079`, `Scp096`, `Scp106`, `Scp173`, `Scp939`, …) | Ability/trigger events, e.g. `Scp096.AddingTarget`, `Scp106.Teleporting` |

## Cancellation semantics

- `-ing` args (`ChangingRoleEventArgs`, `DyingEventArgs`, `HurtingEventArgs`, …) usually carry
  `IsAllowed`; setting it to `false` cancels. **Read the incoming value first** — a previous
  plugin's veto must survive your handler unless your plugin's job is to override it.
- Many `-ing` args are also the mutation surface (`ev.NewRole`, `ev.Items`, damage/attacker
  members). Mutate args instead of forcing state after the fact.
- Past-tense args (`DiedEventArgs`, `SpawnedEventArgs`, …) are notifications only.

## Registration hygiene (audit checklist)

For every `+=` there must be exactly one `-=` with the same handler class, event, and method:

```text
Register()    Player.Verified      += OnVerified;     ✔
Unregister()  Player.Verified      -= OnVerified;     ✔
```

Upstream's `Exiled.Example` itself shipped an `+=` in its unregister method — proof this bug
class is easy to write. After writing Register/Unregister, diff the two lists mechanically.
Also: create handler instances in `OnEnabled` (not field initializers that survive reloads),
null them in `OnDisabled`, and never subscribe static lambdas you cannot unsubscribe.

9.15-era note: some EXILED internals and examples reference LabAPI args
(`LabApi.Events.Arguments.*`) where EXILED re-exports LabAPI-fired events. Follow the exact
using directives of the pinned example for the event you copy — do not "fix" them to EXILED
namespaces by guesswork.
