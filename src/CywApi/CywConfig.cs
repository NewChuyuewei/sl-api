using System.Collections.Generic;

namespace CywApi;

/// <summary>
/// Root configuration (config.yml). Properties only — fields are not serialized by the loader.
/// A broken file falls back to these defaults with a loader warning; the framework must never
/// fail to enable because of its config.
/// </summary>
/// <remarks>Requirements: CYWS-CFG-001, CYWS-CFG-004.</remarks>
public sealed class CywConfig
{
    private LogSection _log = new();
    private PatchSection _patches = new();
    private CompatibilitySection _compatibility = new();

    /// <summary>Enables verbose debug logging.</summary>
    public bool Debug { get; set; } = false;

    /// <summary>Logging options. Never null — an explicit YAML null is replaced by defaults.</summary>
    public LogSection Log
    {
        get => _log;
        set => _log = value ?? new LogSection();
    }

    /// <summary>Patch manager options. Never null — an explicit YAML null is replaced by defaults.</summary>
    public PatchSection Patches
    {
        get => _patches;
        set => _patches = value ?? new PatchSection();
    }

    /// <summary>Compatibility-check options. Never null — an explicit YAML null is replaced by defaults.</summary>
    public CompatibilitySection Compatibility
    {
        get => _compatibility;
        set => _compatibility = value ?? new CompatibilitySection();
    }
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
    private List<string> _disabled = new();

    /// <summary>
    /// Patch classes to skip when applying. Each entry is the full name or the short name of a
    /// [CywPatch] class; a trailing ".*" matches a whole namespace, e.g. "CywApi.Patches.*".
    /// Never null — an explicit YAML null is replaced by an empty list.
    /// </summary>
    public List<string> Disabled
    {
        get => _disabled;
        set => _disabled = value ?? new List<string>();
    }
}

/// <summary>Compatibility-check options.</summary>
public sealed class CompatibilitySection
{
    /// <summary>Logs a warning when the game version is outside the supported range.</summary>
    public bool CheckGameVersion { get; set; } = true;
}
