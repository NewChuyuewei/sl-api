using Exiled.Events.EventArgs.Player;
using PlayerHandlers = Exiled.Events.Handlers.Player;

namespace __PLUGIN_NAME__;

/// <summary>
/// All EXILED event subscriptions for this plugin.
/// Invariant: every <c>+=</c> in <see cref="Register"/> has exactly one matching
/// <c>-=</c> in <see cref="Unregister"/> (same handler class, event, and method).
/// Audit the two methods against each other before every release — a missed
/// unsubscribe means handlers fire twice after a plugin reload.
/// </summary>
public sealed class EventHandlers
{
    private readonly Plugin _plugin;

    public EventHandlers(Plugin plugin) => _plugin = plugin;

    /// <summary>Subscribes all handlers. Called once from <c>OnEnabled</c>.</summary>
    public void Register()
    {
        PlayerHandlers.Verified += OnVerified;
    }

    /// <summary>Unsubscribes all handlers. Called once from <c>OnDisabled</c>.</summary>
    public void Unregister()
    {
        PlayerHandlers.Verified -= OnVerified;
    }

    /// <summary>Example: greet players once they are verified (authenticated and spawned in).</summary>
    private void OnVerified(VerifiedEventArgs ev)
    {
        if (ev.Player is null)
            return;

        if (!string.IsNullOrWhiteSpace(_plugin.Config.JoinHint))
            ev.Player.ShowHint(_plugin.Config.JoinHint, _plugin.Config.JoinHintSeconds);
    }
}
