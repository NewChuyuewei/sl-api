# EXILED 9.15.3 — Custom Roles Pattern

Base type: `Exiled.CustomRoles.API.Features.CustomRole`
(assembly `Exiled.CustomRoles.dll`). Verify each member against the pinned API reference —
this API evolves faster than core EXILED.

## Minimal role skeleton

```csharp
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerHandlers = Exiled.Events.Handlers.Player;

public sealed class ExampleRole : CustomRole
{
    public override uint Id { get; set; } = 100;              // unique forever, see skill ID rules
    public override string Name { get; set; } = "Example Role";
    public override string Description { get; set; } = "TODO: what this role does.";
    public override string CustomInfo { get; set; } = "Example Role";
    public override int MaxHealth { get; set; } = 120;
    public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosConscript;

    public override List<string> Inventory { get; set; } = new() { "GunAK", "Medkit" };
    public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
    {
        { AmmoType.Nato762, 60 },
    };

    protected override void SubscribeEvents()
    {
        PlayerHandlers.Hurting += OnHurting;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        PlayerHandlers.Hurting -= OnHurting;
        base.UnsubscribeEvents();
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        // Mandatory: this handler fires for every player on the server.
        if (!Check(ev.Player))
            return;
        // Role-specific behaviour here.
    }

    protected override void RoleAdded(Player player)
    {
        // Per-player setup (effects, ability state, presentation) after assignment.
        base.RoleAdded(player);
    }
}
```

(`Check`, `TrackedPlayers`, and the exact lifecycle virtual names — `RoleAdded`/`RoleRemoved`,
`ShowMessage` — follow the pinned `CustomRole.cs`; confirm before compiling.)

## Registration

```csharp
// In the plugin's OnEnabled: registers every CustomRole subclass in this assembly.
CustomRole.RegisterRoles();

// In OnDisabled: symmetric cleanup with CustomRole.UnregisterRoles() (or the
// targeted overloads) so a reload cannot duplicate Ids.
// Give the role to a player with role.AddRole(player); remove it with
// role.RemoveRole(player); test ownership with role.Check(player).
```

Config-defined roles: `RegisterRolesFromSource(Config, …)` lets owners edit role definitions
in YAML. Prefer code-defined roles for behaviour-heavy content and config overrides for stats.

## Spawn logic

- Use `SpawnProperties` (dynamic/static spawn points, locker/room chances) for natural spawns,
  or assign explicitly (`role.AddRole(player)`-style API, e.g. from a command or a spawn-wave hook).
- Gate with min-players/chance in config; roll once per spawn opportunity, not per event tick.
- Always delay role application until the player is fully spawned (the API's internal add delay
  exists for a reason); never fight a concurrent base-game role change — hook the role-change
  event chain instead of racing it.

## Abilities

Attach `CustomAbility` instances via `CustomAbilities`. Active abilities
(`ActiveAbility`) follow a select/use lifecycle with built-in cooldown/duration members.
Store per-player ability state keyed by player (cleared in the role-removed hook); never put
mutable per-player state in static fields shared by all holders.
