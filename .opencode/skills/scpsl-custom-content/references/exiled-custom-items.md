# EXILED 9.15.3 — Custom Items Pattern

Base type: `Exiled.CustomItems.API.Features.CustomItem` (assembly `Exiled.CustomItems.dll`);
firearms use the weapon-oriented base classes in the same API. Verify members against the
pinned reference — names here follow the 9.15.x API shape.

## Minimal item skeleton

```csharp
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using PlayerHandlers = Exiled.Events.Handlers.Player;

public sealed class ExampleItem : CustomItem
{
    public override uint Id { get; set; } = 100;              // unique forever
    public override string Name { get; set; } = "Example Item";
    public override string Description { get; set; } = "TODO: what this item does.";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.Medkit;   // base game item it builds on
    public override Vector3 Scale { get; set; } = Vector3.one;
    public override Exiled.API.Features.Spawn.SpawnProperties? SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        PlayerHandlers.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        PlayerHandlers.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        // Mandatory: fires for every item use on the server.
        if (!Check(ev.Item))
            return;
        // Custom behaviour here.
    }
}
```

## Rules that prevent the common disasters

- **Always `Check` first.** Handlers receive all players' items. `Check` matches the item/pickup
  against this content's tracked serials (`TrackedSerials` in the pinned API).
- **Track by serial, never by "player has ItemType X".** The base `Type` is shared with the
  vanilla item; only serials distinguish your custom instance after trades/drops.
- **Register once, unregister on disable:** `CustomItem.RegisterItems()` in `OnEnabled`
  (assembly-wide) or targeted overloads; symmetric `CustomItem.UnregisterItems()` in
  `OnDisabled`. Give/spawn via `Give(player)` / `Spawn(position)` on the custom item,
  and test ownership with `Check(item)` / `Check(pickup)` / `Check(player)`.
  Duplicate `Id` registrations are rejected/logged — a reload leak looks exactly like that.
- **Spawn via the API** (`SpawnProperties` for map spawns; give/spawn helpers for commands and
  role inventories — custom items are referenced by **name** in `CustomRole.Inventory`).
- **Handle the full item lifecycle**: use, drop, pickup, destroy, SCP-914 upgrade
  (`UpgradingPickup`-style hooks exist in the API) — decide explicitly what happens in each,
  even if the answer is "becomes vanilla / is destroyed", and implement that.
- Weapons: prefer the firearm base so ammo/attachment behaviour stays sane; changing damage
  belongs in the hurt/shoot event chain with a `Check`, not in Harmony, for standard cases.
