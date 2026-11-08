using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CywApi.Core.Patching;
using HarmonyLib;

namespace CywApi.Patching;

/// <summary>
/// Applies and removes CYW's API managed Harmony patches (only [CywPatch]-marked classes).
/// One Harmony instance is shared framework-wide; failures are isolated per patch class.
/// </summary>
/// <remarks>Requirements: CYWS-HLR-004, CYWS-PAT-001, CYWS-PAT-002, CYWS-PAT-004, CYWS-PAT-005, CYWS-PAT-006.</remarks>
public static class CywPatcher
{
    /// <summary>Unique Harmony instance id owned by CYW's API. Never unpatch another id.</summary>
    public const string HarmonyId = "cywapi.framework";

    private static readonly HashSet<Assembly> AppliedAssemblies = new();
    private static Harmony? _harmony;

    /// <summary>Gets the number of patch methods applied in the current session.</summary>
    public static int AppliedCount { get; private set; }

    /// <summary>Gets the number of patch classes that failed to apply.</summary>
    public static int FailedCount { get; private set; }

    /// <summary>Gets the number of patch classes skipped via configuration.</summary>
    public static int SkippedCount { get; private set; }

    /// <summary>
    /// Applies every [CywPatch]-marked class found in the given assembly.
    /// A failing patch class is logged and does not stop the remaining ones.
    /// </summary>
    public static void Apply(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        if (AppliedAssemblies.Contains(assembly))
        {
            CywLog.Info($"&8[patcher] {assembly.GetName().Name} already applied — skipped");
            return;
        }

        AppliedAssemblies.Add(assembly);
        _harmony ??= new Harmony(HarmonyId);

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).ToArray();
        }

        var disabled = CywPlugin.Instance?.Config?.Patches?.Disabled;
        var appliedThisCall = 0;

        foreach (var type in types)
        {
            if (type == null || type.GetCustomAttribute<CywPatchAttribute>() == null)
                continue;

            if (CywPatchFilter.IsDisabled(type.FullName ?? type.Name, disabled))
            {
                SkippedCount++;
                CywLog.Info($"&8[patcher] skipped (disabled): {type.FullName}");
                continue;
            }

            try
            {
                var patched = _harmony.CreateClassProcessor(type).Patch();
                var count = patched == null ? 0 : patched.Count;
                AppliedCount += count;
                appliedThisCall += count;
                CywLog.Debug($"[patcher] applied {count} method(s) from {type.FullName}");
            }
            catch (Exception ex)
            {
                FailedCount++;
                CywLog.Error($"&c[patcher] failed to apply {type.FullName}: {ex.Message}");
            }
        }

        CywLog.Info($"&7[patcher] {assembly.GetName().Name}: +{appliedThisCall} applied, {FailedCount} failed, {SkippedCount} skipped");
    }

    /// <summary>Removes every patch previously applied through CYW's API and resets counters.</summary>
    public static void UnpatchAll()
    {
        try
        {
            _harmony?.UnpatchAll(HarmonyId);
        }
        catch (Exception ex)
        {
            CywLog.Error($"&c[patcher] unpatch failed: {ex.Message}");
        }

        _harmony = null;
        AppliedAssemblies.Clear();
        AppliedCount = 0;
        FailedCount = 0;
        SkippedCount = 0;
    }
}
