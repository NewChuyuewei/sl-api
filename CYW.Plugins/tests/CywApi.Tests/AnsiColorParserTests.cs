using CywApi.Core.Colors;
using Xunit;

namespace CywApi.Tests;

[Trait("Req", "CYWS-LOG-002")]
public sealed class AnsiColorParserTests
{
    [Fact]
    public void ToAnsi_Converts_Known_Codes()
    {
        Assert.Equal("\u001b[31mx\u001b[0m", AnsiColorParser.ToAnsi("&4x&r"));
    }

    [Fact]
    public void ToAnsi_Is_Case_Insensitive()
    {
        Assert.Equal("\u001b[91m", AnsiColorParser.ToAnsi("&C"));
    }

    [Fact]
    public void ToAnsi_Escapes_Double_Ampersand()
    {
        Assert.Equal("a&b", AnsiColorParser.ToAnsi("a&&b"));
    }

    [Fact]
    public void ToAnsi_Leaves_Unknown_Codes_Untouched()
    {
        Assert.Equal("&x &", AnsiColorParser.ToAnsi("&x &"));
    }

    [Fact]
    public void ToAnsi_Returns_Input_When_No_Code_Char()
    {
        Assert.Equal("plain text", AnsiColorParser.ToAnsi("plain text"));
    }

    [Fact]
    public void StripCodes_Removes_Codes_And_Keeps_Literal_Ampersand()
    {
        Assert.Equal("hello", AnsiColorParser.StripCodes("&chello&r"));
        Assert.Equal("a&b", AnsiColorParser.StripCodes("a&&b"));
    }

    [Fact]
    public void StripCodes_Leaves_Unknown_Codes_Untouched()
    {
        Assert.Equal("&x", AnsiColorParser.StripCodes("&x"));
    }

    [Fact]
    [Trait("Req", "CYWS-LOG-004")]
    public void ToAnsi_Returns_Null_For_Null_Input()
    {
        Assert.Null(AnsiColorParser.ToAnsi(null));
    }

    [Fact]
    [Trait("Req", "CYWS-LOG-004")]
    public void StripCodes_Returns_Null_For_Null_Input()
    {
        Assert.Null(AnsiColorParser.StripCodes(null));
    }

    [Fact]
    [Trait("Req", "CYWS-LOG-004")]
    public void ToAnsi_Returns_Empty_For_Empty_Input()
    {
        Assert.Equal(string.Empty, AnsiColorParser.ToAnsi(string.Empty));
    }
}
