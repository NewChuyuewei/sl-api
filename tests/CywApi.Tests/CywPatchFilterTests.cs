using CywApi.Core.Patching;
using Xunit;

namespace CywApi.Tests;

public sealed class CywPatchFilterTests
{
    [Theory]
    [InlineData("My.Ns.Patch", new[] { "My.Ns.Patch" }, true)]      // full name
    [InlineData("My.Ns.Patch", new[] { "Patch" }, true)]            // short name
    [InlineData("My.Ns.Patch", new[] { "My.Ns.*" }, true)]          // namespace wildcard
    [InlineData("My.Ns.Sub.Patch", new[] { "My.Ns.*" }, true)]      // nested namespace
    [InlineData("My.Ns.Patch", new[] { "My.Other.*" }, false)]      // unrelated namespace
    [InlineData("My.NsPatch", new[] { "My.Ns.*" }, false)]          // prefix must end on a dot
    [InlineData("My.Ns.Patch", new[] { "My.Ns.Patch2" }, false)]    // exact, not prefix
    [InlineData("My.Ns.Patch", new[] { "  My.Ns.Patch  " }, true)]  // trimmed
    [InlineData("My.Ns.Patch", new[] { "" }, false)]                // empty entry ignored
    public void IsDisabled_Matches(string typeName, string[] patterns, bool expected)
    {
        Assert.Equal(expected, CywPatchFilter.IsDisabled(typeName, patterns));
    }

    [Fact]
    public void IsDisabled_False_For_Empty_Inputs()
    {
        Assert.False(CywPatchFilter.IsDisabled("", new[] { "X" }));
        Assert.False(CywPatchFilter.IsDisabled(null, new[] { "X" }));
        Assert.False(CywPatchFilter.IsDisabled("My.Patch", null));
        Assert.False(CywPatchFilter.IsDisabled("My.Patch", new string[0]));
    }
}
