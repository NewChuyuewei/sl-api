using System;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;

namespace CywApi;

/// <summary>
/// CYW's API plugin entry point.
/// Loads before every other plugin (0-prefixed assembly name + <see cref="LoadPriority.Highest"/>).
/// </summary>
public sealed class CywPlugin : Plugin<CywConfig>
{
    /// <summary>Gets the single plugin instance.</summary>
    public static CywPlugin Instance { get; private set; } = null!;

    /// <inheritdoc/>
    public override string Name => "CywApi";

    /// <inheritdoc/>
    public override string Description => "CYW's API - an extension framework for LabAPI (independent implementation inspired by LabExtended).";

    /// <inheritdoc/>
    public override string Author => "CYW";

    /// <inheritdoc/>
    public override Version Version { get; } = new(0, 1, 0);

    /// <inheritdoc/>
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    /// <inheritdoc/>
    public override LoadPriority Priority => LoadPriority.Highest;

    /// <inheritdoc/>
    public override bool IsTransparent => true; // 先按非侵入式声明；替换 Player 等行为落地后再评审（见 plan.md D1）

    /// <inheritdoc/>
    public override void Enable()
    {
        Instance = this;

        Logger.Info($"CYW's API v{Version.ToString(3)} enabled (LabAPI {LabApiProperties.CurrentVersion}).");
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        Instance = null!;

        Logger.Info("CYW's API disabled.");
    }
}
