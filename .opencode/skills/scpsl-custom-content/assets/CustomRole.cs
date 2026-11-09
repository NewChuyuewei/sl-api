using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;
using PlayerHandlers = Exiled.Events.Handlers.Player;

namespace __PLUGIN_NAME__.Roles;

/// <summary>
/// EXILED custom role template. Duplicate this class per role, give each copy a
/// unique, never-reused <see cref="Id"/>, and register all roles from the
/// plugin's OnEnabled via CustomRole.RegisterRoles().
/// Verify lifecycle member names (RoleAdded / RoleRemoved / ShowMessage) against
/// the pinned 9.15.3 CustomRole API before compiling.
/// </summary>
public sealed class ExampleRole : CustomRole
{
    /// <inheritdoc />
    public override uint Id { get; set; } = 100;

    /// <inheritdoc />
    public override string Name { get; set; } = "Example Role";

    /// <inheritdoc />
    public override string Description { get; set; } = "TODO: describe this role to the player receiving it.";

    /// <inheritdoc />
    public override string CustomInfo { get; set; } = "Example Role";

    /// <inheritdoc />
    public override int MaxHealth { get; set; } = 120;

    /// <inheritdoc />
    public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosConscript;

    /// <inheritdoc />
    public override List<string> Inventory { get; set; } = new() { "GunAK", "Medkit" };

    /// <inheritdoc />
    public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
    {
        { AmmoType.Nato762, 60 },
    };

    /// <inheritdoc />
    protected override void SubscribeEvents()
    {
        PlayerHandlers.Hurting += OnHurting;
        base.SubscribeEvents();
    }

    /// <inheritdoc />
    protected override void UnsubscribeEvents()
    {
        PlayerHandlers.Hurting -= OnHurting;
        base.UnsubscribeEvents();
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        // Role handlers receive events for ALL players — gate on ownership first.
        if (ev.Player is null || !Check(ev.Player))
            return;

        // TODO: role-specific behaviour.
    }
}
