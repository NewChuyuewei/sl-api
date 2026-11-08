using System;
using System.Collections.Generic;

namespace CywApi.Core.Patching;

/// <summary>
/// Evaluates patch-disable patterns against patch class names.
/// Pure logic — unit-tested off-server.
/// </summary>
/// <remarks>Requirements: CYWS-PAT-003.</remarks>
public static class CywPatchFilter
{
    /// <summary>
    /// Returns true when <paramref name="typeFullName"/> should be skipped.
    /// A pattern matches the full name ("My.Ns.Patch") or the short name ("Patch");
    /// a pattern ending in ".*" matches every type below that namespace.
    /// </summary>
    public static bool IsDisabled(string? typeFullName, IReadOnlyList<string>? patterns)
    {
        if (typeFullName == null || typeFullName.Length == 0 || patterns == null || patterns.Count == 0)
            return false;

        var shortName = typeFullName;
        var lastDot = typeFullName.LastIndexOf('.');
        if (lastDot >= 0)
            shortName = typeFullName.Substring(lastDot + 1);

        for (var i = 0; i < patterns.Count; i++)
        {
            var pattern = patterns[i]?.Trim() ?? string.Empty;
            if (pattern.Length == 0)
                continue;

            if (pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                var prefix = pattern.Substring(0, pattern.Length - 2);
                if (typeFullName.Length > prefix.Length + 1 &&
                    typeFullName.StartsWith(prefix, StringComparison.Ordinal) &&
                    typeFullName[prefix.Length] == '.')
                    return true;
            }
            else if (string.Equals(pattern, typeFullName, StringComparison.Ordinal) ||
                     string.Equals(pattern, shortName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
