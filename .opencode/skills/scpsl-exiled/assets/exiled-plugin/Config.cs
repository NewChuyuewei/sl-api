using System.ComponentModel;
using Exiled.API.Interfaces;

namespace __PLUGIN_NAME__;

/// <summary>
/// Plugin configuration (<see cref="IConfig"/>). Serialized into the EXILED
/// <c>Configs/&lt;port&gt;-config.yml</c> under the plugin's prefix; property
/// names become snake_case keys and <see cref="DescriptionAttribute"/> texts
/// become the YAML comments. Every property needs a safe default.
/// </summary>
public sealed class Config : IConfig
{
    /// <inheritdoc />
    // Read by the EXILED loader to decide whether this plugin runs at all.
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool Debug { get; set; } = false;

    /// <summary>Hint shown to a player right after they are verified.</summary>
    [Description("Hint shown to a player right after they are verified. Empty disables it.")]
    public string JoinHint { get; set; } = "Welcome to the server!";

    /// <summary>How long the join hint stays on screen, in seconds.</summary>
    [Description("How long the join hint stays on screen, in seconds.")]
    public float JoinHintSeconds { get; set; } = 5f;
}
