using System;

namespace CywApi;

/// <summary>Framework-wide version constants and compatibility targets.</summary>
/// <remarks>Requirements: CYWS-HLR-001, CYWS-CMP-002.</remarks>
public static class CywVersion
{
    /// <summary>Current framework version (single source of truth for the plugin metadata).</summary>
    public static readonly Version Current = new(0, 1, 0);

    /// <summary>Current framework version as a three-part display string.</summary>
    public const string Display = "0.1.0";

    /// <summary>Minimum supported SCP:SL game version.</summary>
    public static readonly Version MinGameVersion = new(14, 2, 0);
}
