using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using PlayerHandlers = Exiled.Events.Handlers.Player;

namespace __PLUGIN_NAME__.Items;

/// <summary>
/// EXILED custom item template. Duplicate per item with a unique, never-reused
/// <see cref="Id"/>, and register from the plugin's OnEnabled via
/// CustomItem.RegisterItems(). Verify members against the pinned 9.15.3
/// CustomItem API before compiling.
/// </summary>
public sealed class ExampleItem : CustomItem
{
    /// <inheritdoc />
    public override uint Id { get; set; } = 100;

    /// <inheritdoc />
    public override string Name { get; set; } = "Example Item";

    /// <inheritdoc />
    public override string Description { get; set; } = "TODO: describe this item to the player receiving it.";

    /// <inheritdoc />
    public override float Weight { get; set; } = 1f;

    /// <inheritdoc />
    public override ItemType Type { get; set; } = ItemType.Medkit;

    /// <inheritdoc />
    public override Vector3 Scale { get; set; } = Vector3.one;

    /// <inheritdoc />
    public override Exiled.API.Features.Spawn.SpawnProperties? SpawnProperties { get; set; }

    /// <inheritdoc />
    protected override void SubscribeEvents()
    {
        PlayerHandlers.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    /// <inheritdoc />
    protected override void UnsubscribeEvents()
    {
        PlayerHandlers.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        // Item handlers receive events for ALL items — gate on ownership first.
        if (ev.Item is null || !Check(ev.Item))
            return;

        // TODO: custom behaviour.
    }
}
