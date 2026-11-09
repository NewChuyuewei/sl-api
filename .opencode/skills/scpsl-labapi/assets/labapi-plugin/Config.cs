namespace __PLUGIN_NAME__;

/// <summary>
/// Plugin configuration, serialized to YAML by LabAPI (<c>config.yml</c>).
/// Only properties are serialized — fields are ignored. Every property needs a
/// safe default: if the server owner's YAML is broken, LabAPI falls back to a
/// fresh instance of this class and the plugin must still enable.
/// </summary>
public sealed class Config
{
    /// <summary>Whether verbose debug logging is written to the server console.</summary>
    public bool Debug { get; set; } = false;

    /// <summary>Broadcast shown to a player right after they join. Empty disables it.</summary>
    public string JoinBroadcast { get; set; } = "Welcome to the server!";

    /// <summary>How long the join broadcast stays on screen, in seconds.</summary>
    public ushort JoinBroadcastSeconds { get; set; } = 6;
}
