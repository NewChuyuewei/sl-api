using System;

namespace CywApi.Patching;

/// <summary>
/// Marks a class as a CYW's API managed Harmony patch class. Only marked classes are
/// discovered and applied by <see cref="CywPatcher"/>; the patch methods themselves
/// keep using standard Harmony attributes ([HarmonyPatch], [HarmonyPrefix], ...).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CywPatchAttribute : Attribute
{
    /// <summary>
    /// Optional note describing why this patch exists (which event or API is missing).
    /// Reviewers must be able to re-run the Harmony gate from this note.
    /// </summary>
    public string? Reason { get; set; }
}
