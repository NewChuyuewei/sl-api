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
/// <remarks>
/// <para>
/// Contract: every patch applied through this API is registered under the shared, reserved
/// Harmony id <see cref="HarmonyId"/> (<c>cywapi.framework</c>). Disabling the framework runs
/// <see cref="UnpatchAll"/>, which removes all of them together; no other component may patch
/// under this id. Per-assembly scoping of the id is deferred to the M7c ecosystem review.
/// </para>
/// <para>Requirements: CYWS-HLR-004, CYWS-PAT-001, CYWS-PAT-002, CYWS-PAT-004, CYWS-PAT-005, CYWS-PAT-006, CYWS-PAT-007, CYWS-PAT-008, CYWS-PAT-009.</para>
/// </remarks>
public static class CywPatcher
{
    /// <summary>
    /// Reserved Harmony instance id owned by CYW's API. All patches applied through this API
    /// share it and <see cref="UnpatchAll"/> removes exactly those; never patch or unpatch
    /// under another id.
    /// </summary>
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
    /// Applies every [CywPatch]-marked class found in the given assembly under the shared
    /// <see cref="HarmonyId"/>. A failing patch class is logged and does not stop the remaining
    /// ones; a failed type discovery leaves the assembly unmarked, so the call stays retryable.
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

        _harmony ??= new Harmony(HarmonyId);

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderExceptions = ex.LoaderExceptions;
            var loaderExceptionCount = loaderExceptions?.Length ?? 0;
            var firstLoaderMessage = loaderExceptionCount > 0 ? loaderExceptions![0]?.Message : null;

            // PAT-008: partial loads are diagnosable — count the loader exceptions and show the first.
            CywLog.Warn(loaderExceptionCount == 0
                ? $"&e[patcher] {assembly.GetName().Name}: partial type load; continuing with the types that did load"
                : $"&e[patcher] {assembly.GetName().Name}: partial type load — {loaderExceptionCount} loader exception(s); first: {firstLoaderMessage ?? "(no message)"}");

            types = ex.Types.Where(t => t != null).ToArray();
        }
        catch (Exception ex)
        {
            // PAT-007: discovery failed — keep the assembly unmarked so a later call can retry.
            CywLog.Error($"&c[patcher] {assembly.GetName().Name}: type discovery failed: {ex.Message}");
            return;
        }

        // PAT-007: mark as applied only after successful discovery (partial loads included).
        AppliedAssemblies.Add(assembly);

        var disabled = CywPlugin.Instance?.Config?.Patches?.Disabled;
        var appliedThisCall = 0;
        var failedThisCall = 0;
        var skippedThisCall = 0;

        foreach (var type in types)
        {
            if (type == null || type.GetCustomAttribute<CywPatchAttribute>() == null)
                continue;

            if (CywPatchFilter.IsDisabled(type.FullName ?? type.Name, disabled))
            {
                SkippedCount++;
                skippedThisCall++;
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
                failedThisCall++;
                CywLog.Error($"&c[patcher] failed to apply {type.FullName}: {ex.Message}");
            }
        }

        CywLog.Info($"&7[patcher] {assembly.GetName().Name}: +{appliedThisCall} applied, {failedThisCall} failed, {skippedThisCall} skipped");
    }

    /// <summary>
    /// Removes every patch registered under <see cref="HarmonyId"/> — i.e. all patches applied
    /// through CYW's API — and resets the patcher state and counters. Safe to call when nothing
    /// was applied; the disable path never throws. Successful runs log an info audit line with
    /// the affected method count when it can be determined.
    /// </summary>
    public static void UnpatchAll()
    {
        var affectedMethods = -1;
        try
        {
            affectedMethods = CountPatchedMethods();
        }
        catch (Exception ex)
        {
            CywLog.Debug($"[patcher] affected-method count unavailable: {ex.Message}");
        }

        try
        {
            _harmony?.UnpatchAll(HarmonyId);

            CywLog.Info(affectedMethods < 0
                ? $"&7[patcher] unpatch complete (id '{HarmonyId}')"
                : $"&7[patcher] unpatch complete: {affectedMethods} method(s) affected (id '{HarmonyId}')");
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

    /// <summary>
    /// Counts the methods currently carrying at least one patch owned by <see cref="HarmonyId"/>
    /// (best-effort audit data for <see cref="UnpatchAll"/>, gathered before the unpatch runs).
    /// </summary>
    private static int CountPatchedMethods()
    {
        var count = 0;

        foreach (var method in Harmony.GetAllPatchedMethods())
        {
            var patches = Harmony.GetPatchInfo(method);
            if (patches != null && patches.Owners.Contains(HarmonyId))
                count++;
        }

        return count;
    }
}
