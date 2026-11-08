namespace CywApi.Core;

/// <summary>
/// Marker type for the framework-agnostic core library.
/// This project must never reference LabAPI, game, Unity, or Mirror assemblies.
/// </summary>
public static class CywCore
{
    /// <summary>Gets the core library version.</summary>
    public const string Version = "0.1.0";
}
