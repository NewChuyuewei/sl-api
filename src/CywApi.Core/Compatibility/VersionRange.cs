using System;

namespace CywApi.Core.Compatibility;

/// <summary>
/// Inclusive-minimum / exclusive-maximum version range helper.
/// Pure logic — unit-tested off-server.
/// </summary>
/// <remarks>Requirements: CYWS-CMP-002.</remarks>
public static class VersionRange
{
    /// <summary>
    /// Returns true when <paramref name="current"/> is at least
    /// <paramref name="minInclusive"/> and (when given) below <paramref name="maxExclusive"/>.
    /// </summary>
    public static bool IsCompatible(Version? current, Version? minInclusive, Version? maxExclusive = null)
    {
        if (current == null)
            throw new ArgumentNullException(nameof(current));
        if (minInclusive == null)
            throw new ArgumentNullException(nameof(minInclusive));

        if (current < minInclusive)
            return false;

        if (maxExclusive != null && current >= maxExclusive)
            return false;

        return true;
    }
}
