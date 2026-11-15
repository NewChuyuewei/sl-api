using System.Diagnostics;
using CywApi.Core.Colors;
using CywApi.Core.Logging;
using LabApi.Features.Console;

namespace CywApi;

/// <summary>
/// Framework logger: gates debug output, renders optional ANSI colors and can prepend the
/// source assembly name. All CYW's API code logs through this class.
/// </summary>
/// <remarks>Requirements: CYWS-HLR-003, CYWS-LOG-001, CYWS-LOG-003, CYWS-LOG-005.</remarks>
public static class CywLog
{
    /// <summary>Writes a debug line — only when debugging is enabled in the config.</summary>
    public static void Debug(string message)
    {
        if (CywPlugin.Instance?.Config?.Debug != true)
            return;

        Logger.Debug(Format(message));
    }

    /// <summary>Writes an informational line.</summary>
    public static void Info(string message) => Logger.Info(Format(message));

    /// <summary>Writes a warning line.</summary>
    public static void Warn(string message) => Logger.Warn(Format(message));

    /// <summary>Writes an error line.</summary>
    public static void Error(string message) => Logger.Error(Format(message));

    /// <summary>
    /// Escapes every control character in an untrusted data string — including LF and CR —
    /// into a visible representation before it is embedded into a log message, so the value
    /// can never inject terminal control sequences or forge log lines.
    /// </summary>
    /// <remarks>
    /// Log messages passed to this class are rendered as formatting-capable text
    /// (<c>&amp;x</c> color codes are converted to ANSI escapes); every string that originates
    /// from players, plugin code, or configuration data MUST be passed through this method
    /// before being embedded into a log message. Null and empty input are returned unchanged.
    /// </remarks>
    /// <param name="value">The untrusted/data string to escape; may be null.</param>
    /// <returns>The escaped text, or the input unchanged when null, empty, or control-free.</returns>
    public static string? EscapeData(string? value) => ControlCharFilter.EscapeData(value);

    /// <summary>
    /// Formats a message according to the config: raw control characters are neutralized first
    /// (CYWS-LOG-005 choke-point, before color handling so generated ANSI is unaffected), color
    /// codes are then converted to ANSI escapes (or stripped when colors are off), and the
    /// source assembly name can be prepended.
    /// </summary>
    public static string Format(string message)
    {
        var config = CywPlugin.Instance?.Config;
        var text = ControlCharFilter.Neutralize(message ?? string.Empty) ?? string.Empty;

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
