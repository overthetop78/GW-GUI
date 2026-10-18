using System.Globalization;
using GWGUI.MediaFileSystems.Functions;

namespace GWGUI.Tests.Media;

public sealed class MediaMetadataIntegerTests
{
    [Theory]
    [InlineData("0", true, true, 0)]
    [InlineData("42", true, true, 42)]
    [InlineData("2147483647", true, true, int.MaxValue)]
    [InlineData("-2147483648", false, true, int.MinValue)]
    [InlineData("-1", false, true, -1)]
    [InlineData("+42", false, true, 42)]
    [InlineData(" 42\t", false, true, 42)]
    [InlineData("2147483648", false, false, 0)]
    [InlineData("-2147483649", false, false, 0)]
    [InlineData("1,000", false, false, 0)]
    [InlineData("1.0", false, false, 0)]
    [InlineData("0x10", false, false, 0)]
    [InlineData("", false, false, 0)]
    [InlineData(null, false, false, 0)]
    public void KeepsStrictMediaAndPhysicalIntegerPoliciesSeparate(
        string? text, bool strictSuccess, bool physicalSuccess, int expected)
    {
        var metadata = new Dictionary<string, string> { ["position"] = text! };
        Assert.Equal(strictSuccess, MediaMetadataFunctions.TryReadInt(metadata, "position", out var strict));
        Assert.Equal(strictSuccess ? expected : 0, strict);
        Assert.Equal(physicalSuccess, MediaMetadataFunctions.TryReadInt(metadata, "position", NumberStyles.Integer, out var physical));
        Assert.Equal(physicalSuccess ? expected : 0, physical);
        Assert.Equal(physicalSuccess ? expected : 0, MediaMetadataFunctions.ReadIntOrDefault(metadata, "position"));
        Assert.Equal(text, metadata["position"]);
    }

    [Fact]
    public void MissingMetadataResetsOutputAndUsesTheOriginalPhysicalDefault()
    {
        var metadata = new Dictionary<string, string>();
        var value = 123;
        Assert.False(MediaMetadataFunctions.TryReadInt(metadata, "missing", out value));
        Assert.Equal(0, value);
        value = 123;
        Assert.False(MediaMetadataFunctions.TryReadInt(metadata, "missing", NumberStyles.Integer, out value));
        Assert.Equal(0, value);
        Assert.Equal(0, MediaMetadataFunctions.ReadIntOrDefault(metadata, "missing"));
        Assert.Empty(metadata);
    }
}
