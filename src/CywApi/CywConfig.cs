using System.Collections.Generic;

namespace CywApi;

/// <summary>
/// Root configuration (config.yml). Properties only — fields are not serialized by the loader.
/// A broken file falls back to these defaults with a loader warning; the framework must never
/// fail to enable because of its config.
/// </summary>
public sealed class CywConfig
{
    /// <summary>Enables verbose debug logging.</summary>
    public bool Debug { get; set; } = false;

    /// <summary>Logging options.</summary>
    public LogSection Log { get; set; } = new();

    /// <summary>Patch manager options.</summary>
    public PatchSection Patches { get; set; } = new();

    /// <summary>Compatibility-check options.</summary>
    public CompatibilitySection Compatibility { get; set; } = new();
}

/// <summary>Logging options.</summary>
public sealed class LogSection
{
    /// <summary>Renders &amp;-style color codes in framework log messages as ANSI colors.</summary>
    public bool UseColors { get; set; } = true;

    /// <summary>Prepends the calling assembly name to every framework log line.</summary>
    public bool PrependSource { get; set; } = false;
}

/// <summary>Patch manager options.</summary>
public sealed class PatchSection
{
    /// <summary>
    /// Patch classes to skip when applying. Each entry is the full name or the short name of a
    /// [CywPatch] class; a trailing ".*" matches a whole namespace, e.g. "CywApi.Patches.*".
    /// </summary>
    public List<string> Disabled { get; set; } = new();
}

/// <summary>Compatibility-check options.</summary>
public sealed class CompatibilitySection
{
    /// <summary>Logs a warning when the game version is outside the supported range.</summary>
    public bool CheckGameVersion { get; set; } = true;
}
