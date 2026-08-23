using GWGUI.App.Services.Input.GameInput;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class HidReportDecoderTests
{
    [Fact]
    public void NativeHidStructuresMatchWindowsLayout()
    {
        Assert.Equal(64, Marshal.SizeOf<HidNative.HidCaps>());
        Assert.Equal(72, Marshal.SizeOf<HidNative.HidButtonCaps>());
        Assert.Equal(72, Marshal.SizeOf<HidNative.HidValueCaps>());
    }

    [Theory]
    [InlineData(0x7F, 8, true, 127)]
    [InlineData(0x80, 8, true, -128)]
    [InlineData(0xFF, 8, true, -1)]
    [InlineData(0xFFFF, 16, true, -1)]
    [InlineData(0xFF, 8, false, 255)]
    public void SignExtendUsesTheHidLogicalBitWidth(
        uint raw,
        ushort bitSize,
        bool signed,
        int expected)
    {
        Assert.Equal(expected, HidReportDecoder.SignExtend(raw, bitSize, signed));
    }

    [Theory]
    [InlineData(-32768, -32768, 32767, 0f)]
    [InlineData(32767, -32768, 32767, 1f)]
    [InlineData(0, 0, 255, 0f)]
    [InlineData(255, 0, 255, 1f)]
    public void NormalizeUsesTheDeclaredLogicalRange(
        int value,
        int minimum,
        int maximum,
        float expected)
    {
        Assert.Equal(expected, HidReportDecoder.Normalize(value, minimum, maximum), 4);
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(7, 0, 8)]
    [InlineData(1, 1, 1)]
    [InlineData(8, 1, 8)]
    [InlineData(9, 1, 0)]
    public void DecodeHatAcceptsZeroAndOneBasedHidRanges(
        int value,
        int logicalMinimum,
        int expected)
    {
        Assert.Equal(expected, (int)HidReportDecoder.DecodeHat(value, logicalMinimum));
    }
}
