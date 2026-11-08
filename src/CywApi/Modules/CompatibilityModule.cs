using System;
using CommandSystem.Commands.Shared;
using CywApi.Core.Compatibility;

namespace CywApi.Modules;

/// <summary>
/// Checks the game version against the supported range and injects the framework version
/// into the server's build-info output. Never aborts the framework on failure.
/// </summary>
public sealed class CompatibilityModule : ICywModule
{
    private const string BuildInfoLine = "CYW's API v" + CywVersion.Display;

    /// <inheritdoc/>
    public string Name => "Compatibility";

    /// <inheritdoc/>
    public int Priority => 200;

    /// <inheritdoc/>
    public void OnEnable()
    {
        if (CywPlugin.Instance?.Config?.Compatibility.CheckGameVersion != false)
            CheckGameVersion();

        InjectBuildInfo();
    }

    /// <inheritdoc/>
    public void OnDisable()
    {
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
