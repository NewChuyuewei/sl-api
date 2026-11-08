using System.Diagnostics;
using CywApi.Core.Colors;
using LabApi.Features.Console;

namespace CywApi;

/// <summary>
/// Framework logger: gates debug output, renders optional ANSI colors and can prepend the
/// source assembly name. All CYW's API code logs through this class.
/// </summary>
/// <remarks>Requirements: CYWS-HLR-003, CYWS-LOG-001, CYWS-LOG-003.</remarks>
public static class CywLog
{
    /// <summary>Writes a debug line — only when debugging is enabled in the config.</summary>
    public static void Debug(string message)
    {
        if (CywPlugin.Instance?.Config?.Debug != true)
            return;

        Logger.Debug(Format(message), true);
    }

    /// <summary>Writes an informational line.</summary>
    public static void Info(string message) => Logger.Info(Format(message));

    /// <summary>Writes a warning line.</summary>
    public static void Warn(string message) => Logger.Warn(Format(message));

    /// <summary>Writes an error line.</summary>
    public static void Error(string message) => Logger.Error(Format(message));

    /// <summary>
    /// Formats a message according to the config: color codes are converted to ANSI escapes
    /// (or stripped when colors are off), and the source assembly name can be prepended.
    /// </summary>
    public static string Format(string message)
    {
        var config = CywPlugin.Instance?.Config;
        var text = message ?? string.Empty;

        if (config != null && config.Log.UseColors)
            text = AnsiColorParser.ToAnsi(text) ?? string.Empty;
        else
            text = AnsiColorParser.StripCodes(text) ?? string.Empty;

        if (config != null && config.Log.PrependSource)
        {
            var source = ResolveSourceAssemblyName();
            if (!string.IsNullOrEmpty(source))
                text = "[" + source + "] " + text;
        }

        return text;
    }

    private static string? ResolveSourceAssemblyName()
    {
        var trace = new StackTrace(false);
        for (var i = 0; i < trace.FrameCount; i++)
        {
            var method = trace.GetFrame(i)?.GetMethod();
            var assembly = method?.DeclaringType?.Assembly;
            if (assembly == null || assembly == typeof(CywLog).Assembly)
                continue;

            return assembly.GetName().Name;
        }

        return null;
    }
}
