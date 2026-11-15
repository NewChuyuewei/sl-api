using CywApi.Core.Logging;
using Xunit;

namespace CywApi.Tests;

[Trait("Req", "CYWS-LOG-005")]
public sealed class ControlCharFilterTests
{
    // ---- Neutralize: C0 controls become Unicode Control Pictures (U+2400 + code point) ----

    [Fact]
    public void Neutralize_Converts_Esc_To_Control_Picture()
    {
        Assert.Equal("before\u241bafter", ControlCharFilter.Neutralize("before\u001bafter"));
    }

    [Fact]
    public void Neutralize_Converts_Cr_To_Control_Picture()
    {
        Assert.Equal("a\u240db", ControlCharFilter.Neutralize("a\rb"));
    }

    [Fact]
    public void Neutralize_Converts_Bel_And_Vt_To_Control_Pictures()
    {
        Assert.Equal("a\u2407b\u240bc", ControlCharFilter.Neutralize("a\u0007b\u000bc"));
    }

    [Fact]
    public void Neutralize_Converts_C0_Boundaries()
    {
        Assert.Equal("\u2400\u241f", ControlCharFilter.Neutralize("\u0000\u001f"));
    }

    [Fact]
    public void Neutralize_Converts_Del_To_Symbol_For_Delete()
    {
        Assert.Equal("a\u2421b", ControlCharFilter.Neutralize("a\u007fb"));
    }

    [Fact]
    public void Neutralize_Converts_C1_To_Escaped_Text()
    {
        // 0x9B is a C1 control (single-byte CSI) — it must never survive as a raw char.
        Assert.Equal("\\u009B31m", ControlCharFilter.Neutralize("\u009b31m"));
    }

    [Fact]
    public void Neutralize_Preserves_Lf()
    {
        Assert.Equal("line1\nline2", ControlCharFilter.Neutralize("line1\nline2"));
    }

    [Fact]
    public void Neutralize_Preserves_Lf_While_Neutralizing_Other_Controls()
    {
        Assert.Equal("a\u241b\nb\u240dc", ControlCharFilter.Neutralize("a\u001b\nb\rc"));
    }

    [Fact]
    public void Neutralize_Keeps_Cjk_Text_Around_Controls()
    {
        Assert.Equal("中文\u2407文本", ControlCharFilter.Neutralize("中文\u0007文本"));
    }

    [Fact]
    public void Neutralize_Returns_Same_Reference_When_No_Control_Chars()
    {
        const string text = "plain ASCII + 中文字符 text";
        Assert.Same(text, ControlCharFilter.Neutralize(text));
    }

    [Fact]
    public void Neutralize_Leaves_Ampersand_Color_Text_Untouched()
    {
        // "&x" conversion to ANSI is a separate stage; this filter must not interact with it.
        const string text = "&6gold &xunknown &&";
        Assert.Same(text, ControlCharFilter.Neutralize(text));
    }

    [Fact]
    public void Neutralize_Returns_Null_And_Empty_Unchanged()
    {
        Assert.Null(ControlCharFilter.Neutralize(null));
        Assert.Same(string.Empty, ControlCharFilter.Neutralize(string.Empty));
    }

    // ---- EscapeData: everything Neutralize does, plus LF (U+240A) and CR (U+240D) ----

    [Fact]
    public void EscapeData_Escapes_Lf()
    {
        Assert.Equal("a\u240ab", ControlCharFilter.EscapeData("a\nb"));
    }

    [Fact]
    public void EscapeData_Escapes_Cr()
    {
        Assert.Equal("a\u240db", ControlCharFilter.EscapeData("a\rb"));
    }

    [Fact]
    public void EscapeData_Escapes_Lf_And_Cr_Together()
    {
        Assert.Equal("a\u240ab\u240dc", ControlCharFilter.EscapeData("a\nb\rc"));
    }

    [Fact]
    public void EscapeData_Uses_Same_Visible_Forms_As_Neutralize()
    {
        Assert.Equal("x\u241by\\u009Bz", ControlCharFilter.EscapeData("x\u001by\u009bz"));
    }

    [Fact]
    public void EscapeData_Returns_Same_Reference_When_No_Control_Chars()
    {
        const string text = "data-field value with no controls";
        Assert.Same(text, ControlCharFilter.EscapeData(text));
    }

    [Fact]
    public void EscapeData_Returns_Null_And_Empty_Unchanged()
    {
        Assert.Null(ControlCharFilter.EscapeData(null));
        Assert.Same(string.Empty, ControlCharFilter.EscapeData(string.Empty));
    }

    [Fact]
    public void Filters_Keep_Non_Control_Edge_Chars()
    {
        // 0x7E '~', 0x20 space and U+00A0 are not control characters.
        const string text = "~\u00a0";
        Assert.Same(text, ControlCharFilter.Neutralize(text));
        Assert.Same(text, ControlCharFilter.EscapeData(text));
    }
}
