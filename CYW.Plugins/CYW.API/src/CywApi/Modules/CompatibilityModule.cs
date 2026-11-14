using System;
using CommandSystem.Commands.Shared;
using CywApi.Core.Compatibility;
using LabApi.Events.Handlers;

namespace CywApi.Modules;

/// <summary>
/// Checks the game version against the supported range and injects the framework version
/// into the server's build-info output. Never aborts the framework on failure.
/// </summary>
/// <remarks>
/// Requirements: CYWS-HLR-006, CYWS-CMP-001, CYWS-CMP-002, CYWS-CMP-003.
/// The injection performed during <see cref="OnEnable"/> is overwritten by LabAPI's
/// <c>PluginLoader.AddPluginsToBuildInfo()</c>, so the framework line is re-injected when
/// <see cref="ServerEvents.PluginsEnabled"/> fires — that event is raised after the overwrite
/// (PluginLoader.Initialize: LoadAllPlugins → AddPluginsToBuildInfo → OnPluginsEnabled).
/// </remarks>
public sealed class CompatibilityModule : ICywModule
{
    private static readonly string BuildInfoLine = "CYW's API v" + CywVersion.Display;

    /// <inheritdoc/>
    public string Name => "Compatibility";

    /// <inheritdoc/>
    public int Priority => 200;

    /// <inheritdoc/>
    public void OnEnable()
    {
        var config = CywPlugin.Instance?.Config;
        if (config == null)
            CywLog.Debug("[compat] config unavailable — using defaults");

        if (config?.Compatibility.CheckGameVersion != false)
            CheckGameVersion();

        InjectBuildInfo();

        // CMP-003: re-inject after LabAPI overwrites BuildInfoCommand.ModDescription with the
        // plugin list; registered last so a throwing OnEnable cannot leak the subscription.
        ServerEvents.PluginsEnabled += OnPluginsEnabled;
    }

    /// <inheritdoc/>
    public void OnDisable()
    {
        ServerEvents.PluginsEnabled -= OnPluginsEnabled;
    }

    /// <summary>
    /// Re-injects the framework line once LabAPI has overwritten the build-info description
    /// with the list of enabled plugins, so the line appears in the final <c>buildinfo</c> output.
    /// </summary>
    private static void OnPluginsEnabled()
    {
        CywLog.Debug("[compat] plugins enabled — re-checking build info");
        InjectBuildInfo();
    }

    private static void CheckGameVersion()
    {
        try
        {
            var current = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

            if (VersionRange.IsCompatible(current, CywVersion.MinGameVersion))
            {
                CywLog.Info($"&7[compat] game version &f{GameCore.Version.VersionString} &7is supported");
            }
            else
            {
                CywLog.Warn($"&e[compat] game version {GameCore.Version.VersionString} is outside the supported range (>= {CywVersion.MinGameVersion}). CYW's API may misbehave; update with care.");
            }
        }
        catch (Exception ex)
        {
            CywLog.Warn($"&e[compat] game version check failed: {ex.Message}");
        }
    }

    private static void InjectBuildInfo()
    {
        try
        {
            var existing = BuildInfoCommand.ModDescription;
            if (existing != null && existing.Contains(BuildInfoLine))
                return; // idempotent across plugin reloads

            BuildInfoCommand.ModDescription =
                string.IsNullOrEmpty(existing) ? BuildInfoLine : existing + "\n" + BuildInfoLine;

            CywLog.Debug("[compat] build info updated");
        }
        catch (Exception ex)
        {
            CywLog.Warn($"&e[compat] failed to update build info: {ex.Message}");
        }
    }
}
