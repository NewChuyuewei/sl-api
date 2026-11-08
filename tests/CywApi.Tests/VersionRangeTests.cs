using System;
using CywApi.Core.Compatibility;
using Xunit;

namespace CywApi.Tests;

public sealed class VersionRangeTests
{
    [Fact]
    public void Below_Minimum_Is_Incompatible()
    {
        Assert.False(VersionRange.IsCompatible(new Version(14, 1, 0), new Version(14, 2, 0)));
    }

    [Fact]
    public void Equal_To_Minimum_Is_Compatible()
    {
        Assert.True(VersionRange.IsCompatible(new Version(14, 2, 0), new Version(14, 2, 0)));
    }

    [Fact]
    public void Above_Minimum_Is_Compatible()
    {
        Assert.True(VersionRange.IsCompatible(new Version(14, 2, 9), new Version(14, 2, 0)));
    }

    [Fact]
    public void Maximum_Is_Exclusive()
    {
        Assert.False(VersionRange.IsCompatible(new Version(15, 0, 0), new Version(14, 2, 0), new Version(15, 0, 0)));
        Assert.True(VersionRange.IsCompatible(new Version(14, 9, 9), new Version(14, 2, 0), new Version(15, 0, 0)));
    }

    [Fact]
    public void Null_Arguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => VersionRange.IsCompatible(null, new Version(1, 0)));
        Assert.Throws<ArgumentNullException>(() => VersionRange.IsCompatible(new Version(1, 0), null));
    }
}
