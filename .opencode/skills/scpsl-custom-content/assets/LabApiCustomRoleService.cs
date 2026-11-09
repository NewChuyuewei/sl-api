using System.Collections.Generic;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace __PLUGIN_NAME__.Services;

/// <summary>
/// Minimal custom-role registry for LabAPI, which has no built-in custom role
/// framework. Owns assignment state and guarantees cleanup; event handlers in
/// the plugin call into this service and gate behaviour on
/// <see cref="TryGetRole"/>.
/// Keyed by UserId (stable) — never key long-lived state by wrapper reference
/// alone, and never keep entries across rounds.
/// </summary>
public sealed class CustomRoleService
{
    /// <summary>Framework-agnostic description of one custom role holder.</summary>
    public sealed class RoleState
    {
        public required int RoleId { get; init; }
        public required string RoleName { get; init; }
        public required RoleTypeId BaseRole { get; init; }
    }

    private readonly Dictionary<string, RoleState> _holders = new();

    /// <summary>Records that <paramref name="player"/> now holds a custom role.</summary>
    public void Assign(Player player, RoleState state)
    {
        if (player?.UserId is null)
            return;

        _holders[player.UserId] = state;
        player.SendHint($"You are now: {state.RoleName}", 8f);
    }

    /// <summary>Returns the custom role held by <paramref name="player"/>, if any.</summary>
    public bool TryGetRole(Player? player, out RoleState state)
    {
        state = null!;
        return player?.UserId is not null && _holders.TryGetValue(player.UserId, out state!);
    }

    /// <summary>Removes any custom role record for <paramref name="player"/>.</summary>
    public void Remove(Player? player)
    {
        if (player?.UserId is not null)
            _holders.Remove(player.UserId);
    }

    /// <summary>
    /// Drops all records. Call on round end and from the plugin's Disable().
    /// Add per-player teardown (coroutines, effects, hints) here so there is
    /// exactly one place that undoes <see cref="Assign"/>.
    /// </summary>
    public void ClearAll() => _holders.Clear();
}
