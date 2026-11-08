using System.Reflection;
using CywApi.Patching;

namespace CywApi.Modules;

/// <summary>Applies [CywPatch] classes from the framework assembly and removes them on disable.</summary>
public sealed class PatchingModule : ICywModule
{
    /// <inheritdoc/>
    public string Name => "Patching";

    /// <inheritdoc/>
    public int Priority => 100;

    /// <inheritdoc/>
    public void OnEnable() => CywPatcher.Apply(Assembly.GetExecutingAssembly());

    /// <inheritdoc/>
    public void OnDisable() => CywPatcher.UnpatchAll();
}
