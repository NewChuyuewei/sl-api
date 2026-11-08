using Xunit;

namespace CywApi.Tests;

public sealed class CoreSmokeTests
{
    [Fact]
    public void CoreLibrary_IsReferenced_AndVersionMatches()
    {
        Assert.Equal("0.1.0", CywApi.Core.CywCore.Version);
    }
}
