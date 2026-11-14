using System;
using CywApi.Modules;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;

namespace CywApi;

/// <summary>
/// CYW's API plugin entry point.
/// Loads before every other plugin (0-prefixed assembly name + <see cref="LoadPriority.Highest"/>).
/// </summary>
/// <remarks>Requirements: CYWS-HLR-001, CYWS-HLR-002.</remarks>
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
    public override Version Version { get; } = CywVersion.Current;

    /// <inheritdoc/>
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    /// <inheritdoc/>
    public override LoadPriority Priority => LoadPriority.Highest;

    /// <inheritdoc/>
    public override bool IsTransparent => true; // 零侵入补丁阶段保持 true；侵入性补丁（M2 D4 玩家替换等）落地前必须复评并预期切换为 false（CYWS-CMP-004 门禁）

    /// <inheritdoc/>
    public override void Enable()
    {
        Instance = this;

        CywLog.Info($"&6CYW's API &fv{CywVersion.Display} &7starting (LabAPI {LabApiProperties.CurrentVersion})...");

        CywModuleManager.Register(new CompatibilityModule());
        CywModuleManager.Register(new PatchingModule());
        CywModuleManager.EnableAll();

        CywLog.Info($"&aCYW's API v{CywVersion.Display} enabled.");
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        CywModuleManager.DisableAll();

        CywLog.Info("CYW's API disabled.");
        Instance = null!;
    }
}
