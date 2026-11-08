using System;
using System.Collections.Generic;
using System.Linq;

namespace CywApi.Modules;

/// <summary>
/// Deterministic module registry: modules enable in descending priority order (stable) and
/// disable in reverse order; a failing module is logged and does not stop the others.
/// </summary>
/// <remarks>Requirements: CYWS-HLR-005, CYWS-MOD-001, CYWS-MOD-002, CYWS-MOD-003, CYWS-MOD-004.</remarks>
public static class CywModuleManager
{
    private static readonly List<ICywModule> Registered = new();
    private static readonly List<ICywModule> EnabledModules = new();
    private static bool _started;

    /// <summary>Gets the number of currently enabled modules.</summary>
    public static int EnabledCount => EnabledModules.Count;

    /// <summary>Registers a module. Intended to be called before <see cref="EnableAll"/>.</summary>
    public static void Register(ICywModule module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        if (_started)
            throw new InvalidOperationException("Modules must be registered before EnableAll() is called.");

        foreach (var existing in Registered)
        {
            if (existing.GetType() == module.GetType())
                throw new InvalidOperationException($"Module {module.GetType().FullName} is already registered.");
        }

        Registered.Add(module);
    }

    /// <summary>Enables all registered modules (descending priority, stable within equal values).</summary>
    public static void EnableAll()
    {
        _started = true;

        foreach (var module in Registered.OrderByDescending(m => m.Priority))
        {
            try
            {
                module.OnEnable();
                EnabledModules.Add(module);
                CywLog.Debug($"[modules] enabled: {module.Name}");
            }
            catch (Exception ex)
            {
                CywLog.Error($"&c[modules] '{module.Name}' failed to enable: {ex}");
            }
        }

        CywLog.Info($"&7[modules] {EnabledCount}/{Registered.Count} enabled");
    }

    /// <summary>Disables all enabled modules in reverse enable order and clears the registry.</summary>
    public static void DisableAll()
    {
        for (var i = EnabledModules.Count - 1; i >= 0; i--)
        {
            var module = EnabledModules[i];
            try
            {
                module.OnDisable();
            }
            catch (Exception ex)
            {
                CywLog.Error($"&c[modules] '{module.Name}' failed to disable: {ex}");
            }
        }

        EnabledModules.Clear();
        Registered.Clear();
        _started = false;
    }
}
