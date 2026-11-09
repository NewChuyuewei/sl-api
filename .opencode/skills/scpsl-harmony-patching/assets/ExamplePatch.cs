using System;
using HarmonyLib;

namespace __PLUGIN_NAME__.Patches;

/// <summary>
/// Harmony patch template — copy, rename, and retarget. Read the
/// scpsl-harmony-patching skill gate before shipping any patch.
///
/// JUSTIFICATION (required — fill in before merging):
/// No LabAPI 1.1.7 / EXILED 9.15.3 event or wrapper API can ______________
/// because ______________. Closest event checked: ______________, which is
/// insufficient because ______________. Verified against SCP:SL 14.2.x,
/// target method signature confirmed in the referenced game assembly.
/// </summary>
[HarmonyPatch(typeof(object), nameof(ToString))] // TODO: retarget to the real game type/method.
internal static class ExamplePatch
{
    // Postfix by default (least invasive). Change to a Prefix returning false
    // only when skipping the original is the documented point of the patch.
    private static void Postfix(object __instance, ref string __result)
    {
        try
        {
            // TODO: minimal logic. No allocations, no blocking, no LINQ on hot paths.
            // Delegate anything non-trivial to a service class.
        }
        catch (Exception ex)
        {
            // Fail open: log once with context and leave vanilla behaviour intact.
            // Replace with the host framework logger (LabApi Logger / EXILED Log).
            System.Diagnostics.Debug.WriteLine($"[__PLUGIN_NAME__] ExamplePatch failed open: {ex.Message}");
        }
    }
}
