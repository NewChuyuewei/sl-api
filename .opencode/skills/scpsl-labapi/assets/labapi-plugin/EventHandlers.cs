using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;

namespace __PLUGIN_NAME__;

/// <summary>
/// All LabAPI event handlers for this plugin. Only the virtual methods that are
/// overridden here get subscribed by
/// <see cref="CustomHandlersManager.RegisterEventsHandler"/> — add a handler by
/// adding an override, remove one by deleting it. No manual += / -= bookkeeping.
/// </summary>
public sealed class EventHandlers : CustomEventsHandler
{
    private readonly Plugin _plugin;

    public EventHandlers(Plugin plugin) => _plugin = plugin;

    /// <summary>Example: greet players when they finish joining the server.</summary>
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        if (ev.Player is null)
            return;

        Logger.Debug($"{ev.Player.DisplayName} joined the server.", _plugin.Config.Debug);

        if (!string.IsNullOrWhiteSpace(_plugin.Config.JoinBroadcast))
            ev.Player.SendBroadcast(_plugin.Config.JoinBroadcast, _plugin.Config.JoinBroadcastSeconds);
    }
}
