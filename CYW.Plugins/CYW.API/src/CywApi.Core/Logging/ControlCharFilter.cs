using System.Globalization;
using System.Text;

namespace CywApi.Core.Logging;

/// <summary>
/// Renders control characters in log-bound strings as visible text, so an untrusted string
/// can never inject terminal control sequences or forge log lines into the server console.
/// Pure logic — unit-tested off-server.
/// </summary>
/// <remarks>Requirements: CYWS-LOG-005.</remarks>
public static class ControlCharFilter
{
    /// <summary>
    /// Neutralizes raw control characters in a message that is about to be logged: C0 controls
    /// (0x00-0x1F) become Unicode Control Pictures (U+2400 + code point), DEL (0x7F) becomes
    /// U+2421, and C1 controls (0x80-0x9F) become the literal text "\uXXXX" (uppercase hex).
    /// LF (0x0A) is preserved so multi-line diagnostics still work.
    /// </summary>
    /// <param name="input">Raw message text; may be null.</param>
    /// <returns>
    /// The neutralized text; the input reference is returned unchanged when it is null, empty,
    /// or already free of control characters (no allocation).
    /// </returns>
    public static string? Neutralize(string? input) => Filter(input, escapeAllControls: false);

    /// <summary>
    /// Escapes every control character in an untrusted/data string — including LF (0x0A,
    /// rendered as U+240A) and CR (0x0D, rendered as U+240D) — so the value can be embedded
    /// safely into a log message without enabling terminal control or log-line forgery.
    /// </summary>
    /// <param name="input">Untrusted/data string; may be null.</param>
    /// <returns>
    /// The escaped text; the input reference is returned unchanged when it is null, empty,
    /// or already free of control characters (no allocation).
    /// </returns>
    public static string? EscapeData(string? input) => Filter(input, escapeAllControls: true);

    private static string? Filter(string? input, bool escapeAllControls)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var value = input!;
        var first = IndexOfFirstControl(value, escapeAllControls);
        if (first < 0)
            return input;

        var sb = new StringBuilder(value.Length + 16);
        sb.Append(value, 0, first);

        for (var i = first; i < value.Length; i++)
        {
            var c = value[i];
            if (IsControl(c, escapeAllControls))
                AppendVisible(sb, c);
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static int IndexOfFirstControl(string value, bool escapeAllControls)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (IsControl(value[i], escapeAllControls))
                return i;
        }

        return -1;
    }

    private static bool IsControl(char c, bool escapeAllControls)
    {
        if (c < 0x20)
            return escapeAllControls || c != '\n';

        return c == 0x7F || (c >= 0x80 && c <= 0x9F);
    }

    private static void AppendVisible(StringBuilder sb, char c)
    {
        if (c < 0x20)
        {
            sb.Append((char)(0x2400 + c));
            return;
        }

        if (c == 0x7F)
        {
            sb.Append('\u2421');
            return;
        }

        // C1 control (0x80-0x9F): spell it out as escaped text — the raw form can start a
        // terminal control sequence (e.g. 0x9B is the single-byte CSI introducer).
        sb.Append("\\u");
        sb.Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
    }
}
