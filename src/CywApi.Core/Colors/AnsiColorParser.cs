using System.Collections.Generic;
using System.Text;

namespace CywApi.Core.Colors;

/// <summary>
/// Converts simple "&amp;x" color codes into ANSI escape sequences so framework logs can be
/// colored in the game console. Pure logic — unit-tested off-server.
/// </summary>
public static class AnsiColorParser
{
    /// <summary>Character that introduces a color code.</summary>
    public const char CodeChar = '&';

    private static readonly Dictionary<char, string> EscapeMap = new()
    {
        ['0'] = "\u001b[30m", // black
        ['1'] = "\u001b[34m", // dark blue
        ['2'] = "\u001b[32m", // dark green
        ['3'] = "\u001b[36m", // dark cyan
        ['4'] = "\u001b[31m", // dark red
        ['5'] = "\u001b[35m", // dark magenta
        ['6'] = "\u001b[33m", // gold
        ['7'] = "\u001b[37m", // gray
        ['8'] = "\u001b[90m", // dark gray
        ['9'] = "\u001b[94m", // blue
        ['a'] = "\u001b[92m", // green
        ['b'] = "\u001b[96m", // cyan
        ['c'] = "\u001b[91m", // red
        ['d'] = "\u001b[95m", // magenta
        ['e'] = "\u001b[93m", // yellow
        ['f'] = "\u001b[97m", // white
        ['r'] = "\u001b[0m",  // reset
    };

    /// <summary>
    /// Converts known color codes to ANSI escapes; "&amp;&amp;" becomes a literal '&amp;'.
    /// Unknown codes pass through unchanged.
    /// </summary>
    public static string ToAnsi(string input)
    {
        if (string.IsNullOrEmpty(input) || input.IndexOf(CodeChar) < 0)
            return input;

        var sb = new StringBuilder(input.Length + 16);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == CodeChar && i + 1 < input.Length)
            {
                var next = input[i + 1];
                if (next == CodeChar)
                {
                    sb.Append(CodeChar);
                    i++;
                    continue;
                }

                if (EscapeMap.TryGetValue(char.ToLowerInvariant(next), out var ansi))
                {
                    sb.Append(ansi);
                    i++;
                    continue;
                }
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Removes known color codes without inserting escapes
    /// ("&amp;&amp;" still becomes a literal '&amp;').
    /// </summary>
    public static string StripCodes(string input)
    {
        if (string.IsNullOrEmpty(input) || input.IndexOf(CodeChar) < 0)
            return input;

        var sb = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == CodeChar && i + 1 < input.Length)
            {
                var next = input[i + 1];
                if (next == CodeChar)
                {
                    sb.Append(CodeChar);
                    i++;
                    continue;
                }

                if (EscapeMap.ContainsKey(char.ToLowerInvariant(next)))
                {
                    i++;
                    continue;
                }
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
