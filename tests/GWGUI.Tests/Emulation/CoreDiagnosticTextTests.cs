using GWGUI.Emulation;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreDiagnosticTextTests
{
    [Theory]
    [InlineData(null, false, null)]
    [InlineData(null, true, null)]
    [InlineData("", false, null)]
    [InlineData("", true, null)]
    [InlineData(" \t\r\n", false, null)]
    [InlineData(" \t\r\n", true, " \t\r\n")]
    [InlineData("\u2003", false, null)]
    [InlineData("\u2003", true, "\u2003")]
    [InlineData("été 日本語 🎮", false, "été 日本語 🎮")]
    [InlineData("été 日本語 🎮", true, "été 日本語 🎮")]
    public void CopiesAndPublishesOnlyTextAcceptedByTheExistingPolicy(string? text, bool allowWhitespace, string? expected)
    {
        var pointer = text is null ? nint.Zero : Marshal.StringToCoTaskMemUTF8(text);
        var published = new List<string>();
        try
        {
            var result = EmulationSdk.EnvironmentProcessing.CopyDiagnostic(pointer, published.Add, allowWhitespace);
            Assert.Equal(expected, result);
            if (expected is null) Assert.Empty(published);
            else Assert.Equal(new[] { expected }, published);
        }
        finally { if (pointer != nint.Zero) Marshal.FreeCoTaskMem(pointer); }
    }

    [Fact]
    public void CopiedTextSurvivesReleaseOfBorrowedMemory()
    {
        var pointer = Marshal.StringToCoTaskMemUTF8("message é 日本語");
        string? copied;
        string? published = null;
        try { copied = EmulationSdk.EnvironmentProcessing.CopyDiagnostic(pointer, text => published = text); }
        finally { Marshal.FreeCoTaskMem(pointer); }
        Assert.Equal("message é 日本語", copied);
        Assert.Same(copied, published);
    }

    [Fact]
    public void DoesNotPublishFilteredText()
    {
        var pointer = Marshal.StringToCoTaskMemUTF8("\t");
        try
        {
            Assert.Null(EmulationSdk.EnvironmentProcessing.CopyDiagnostic(pointer,
                _ => throw new InvalidOperationException("must not publish")));
        }
        finally { Marshal.FreeCoTaskMem(pointer); }
    }

    [Fact]
    public void PublisherFailurePropagatesWithoutPublishingAgain()
    {
        var pointer = Marshal.StringToCoTaskMemUTF8("message");
        var expected = new InvalidOperationException("publisher");
        var count = 0;
        try
        {
            var actual = Assert.Throws<InvalidOperationException>(() =>
                EmulationSdk.EnvironmentProcessing.CopyDiagnostic(pointer, _ => { count++; throw expected; }));
            Assert.Same(expected, actual);
            Assert.Equal(1, count);
        }
        finally { Marshal.FreeCoTaskMem(pointer); }
    }
}
