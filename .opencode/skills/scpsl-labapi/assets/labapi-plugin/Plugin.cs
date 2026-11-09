using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins.Enums;

namespace __PLUGIN_NAME__;

/// <summary>
/// Entry point of the __PLUGIN_NAME__ LabAPI plugin.
/// Keep this class thin: configuration, lifecycle, and wiring only.
/// Feature logic belongs in <see cref="EventHandlers"/> and service classes.
/// </summary>
public sealed class Plugin : LabApi.Loader.Features.Plugins.Plugin<Config>
{
    /// <inheritdoc />
    public override string Name => "__PLUGIN_NAME__";

    /// <inheritdoc />
    public override string Description => "TODO: describe what this plugin does in one sentence.";

    /// <inheritdoc />
    public override string Author => "__AUTHOR__";

    /// <inheritdoc />
    public override Version Version { get; } = new(1, 0, 0);

    /// <inheritdoc />
    // Pinned to the LabAPI version this plugin was built against (1.1.7).
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    /// <inheritdoc />
    public override LoadPriority Priority => LoadPriority.Medium;

    /// <summary>Gets the active plugin instance, or <c>null</c> while disabled.</summary>
    public static Plugin? Instance { get; private set; }

    private EventHandlers? _handlers;

    /// <inheritdoc />
    public override void Enable()
    {
        Instance = this;
        _handlers = new EventHandlers(this);
        CustomHandlersManager.RegisterEventsHandler(_handlers);
        Logger.Info($"{Name} v{Version} by {Author} has been enabled.");
    }

    /// <inheritdoc />
    public override void Disable()
    {
        if (_handlers is not null)
            CustomHandlersManager.UnregisterEventsHandler(_handlers);

        _handlers = null;
        Instance = null;
        Logger.Info($"{Name} has been disabled.");
    }
}
