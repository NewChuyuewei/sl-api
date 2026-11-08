namespace CywApi.Modules;

/// <summary>
/// A framework module: a self-contained piece of functionality with an explicit lifecycle.
/// Modules enable in descending <see cref="Priority"/> order and disable in reverse.
/// </summary>
public interface ICywModule
{
    /// <summary>Gets a unique, human-readable module name (used in logs).</summary>
    string Name { get; }

    /// <summary>Gets the enable-order priority — higher values enable earlier.</summary>
    int Priority { get; }

    /// <summary>Called when the module is enabled. Throwing must not stop other modules.</summary>
    void OnEnable();

    /// <summary>Called when the module is disabled. Must undo everything done in OnEnable.</summary>
    void OnDisable();
}
