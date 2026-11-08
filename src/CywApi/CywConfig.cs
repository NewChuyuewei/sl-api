namespace CywApi;

/// <summary>
/// Root configuration (config.yml). Properties only — fields are not serialized by the loader.
/// </summary>
public sealed class CywConfig
{
    /// <summary>Enables verbose debug logging.</summary>
    public bool Debug { get; set; } = false;
}
