using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace __PLUGIN_NAME__;

/// <summary>
/// Entry point of the __PLUGIN_NAME__ EXILED plugin.
/// Keep this class thin: lifecycle and wiring only. Feature logic belongs in
/// <see cref="EventHandlers"/> and service classes.
/// </summary>
public sealed class Plugin : Exiled.API.Features.Plugin<Config>
{
    /// <inheritdoc />
    public override string Name => "__PLUGIN_NAME__";

    /// <summary>
    /// Config/translation section key. This is serialized into every server
    /// owner's YAML — treat it as public API and never rename it after release.
    /// Use snake_case in real projects (e.g. "my_plugin").
    /// </summary>
    public override string Prefix => "__plugin_name__";

    /// <inheritdoc />
    public override string Author => "__AUTHOR__";

    /// <inheritdoc />
    public override Version Version => new(1, 0, 0);

    /// <inheritdoc />
    public override PluginPriority Priority => PluginPriority.Default;

    // RequiredExiledVersion intentionally not overridden: it defaults to the
    // EXILED version this plugin was compiled against (9.15.3) and the loader
    // enforces it. Do not set IgnoreRequiredVersionCheck in public releases.

    /// <summary>Gets the active plugin instance, or <c>null</c> while disabled.</summary>
    public static Plugin? Instance { get; private set; }

    private EventHandlers? _handlers;

    /// <inheritdoc />
    public override void OnEnabled()
    {
        Instance = this;
        _handlers = new EventHandlers(this);
        _handlers.Register();
        Log.Info($"{Name} v{Version} by {Author} has been enabled.");
        base.OnEnabled();
    }

    /// <inheritdoc />
    public override void OnDisabled()
    {
        _handlers?.Unregister();
        _handlers = null;
        Instance = null;
        base.OnDisabled();
    }

    /// <inheritdoc />
    public override void OnReloaded()
    {
        // Config/translation objects are refreshed by the loader on reload.
        // Re-read cached values here if the plugin snapshots config anywhere.
        base.OnReloaded();
    }
}
