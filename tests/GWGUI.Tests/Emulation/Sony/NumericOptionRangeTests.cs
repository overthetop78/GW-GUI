using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants;
using GWGUI.Emulation.Sony.Emulators.BeetlePsx.Dictionaries;
using HwOptions = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Dictionaries.OptionCatalog;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class NumericOptionRangeTests
{
    [Fact]
    public void BothBeetleVariantsExposeCpuFrequencyAndCropAsSliders()
    {
        foreach (var options in new[] { OptionCatalog.All, HwOptions.All })
        {
            var frequency = Assert.Single(options, option => option.Key.EndsWith("cpu_freq_scale"));
            Assert.Equal(50, frequency.NumericRange!.Minimum);
            Assert.Equal(750, frequency.NumericRange.Maximum);
            Assert.Equal(10, frequency.NumericRange.Step);
            var crop = Assert.Single(options, option => option.Key.EndsWith("image_crop"));
            Assert.Equal(0, crop.NumericRange!.Minimum);
            Assert.Equal(20, crop.NumericRange.Maximum);
            Assert.Equal(1, crop.NumericRange.Step);
            Assert.Empty(frequency.Values);
            Assert.Empty(crop.Values);
        }
    }

    [Theory]
    [InlineData("100", "100%")]
    [InlineData("115", "120%")]
    [InlineData("999", "750%")]
    [InlineData("not-a-number", "100%")]
    public void SliderNumbersAreSnappedClampedAndConvertedToNativePercent(string input, string expected)
    {
        var configuration = Configuration(OptionConstants.CpuFreqScaleKey, input);
        var native = CoreSettingsFunctions.Configure(Definition(), configuration);
        Assert.Equal(expected, native.Options![OptionConstants.CpuFreqScaleKey]);
        var field = CoreSettingsFunctions.Blocks(Definition(), native).SelectMany(block => block.Fields)
            .Single(field => field.Id == OptionConstants.CpuFreqScaleKey);
        Assert.Equal(EmulationSettingsEditor.Slider, field.Editor);
        Assert.Equal(expected.TrimEnd('%'), field.Value);
    }

    [Theory]
    [InlineData("0", "disabled")]
    [InlineData("12", "12px")]
    public void CropZeroUsesTheNativeDisabledMode(string input, string expected)
    {
        var native = CoreSettingsFunctions.Configure(Definition(), Configuration(OptionConstants.ImageCropKey, input));
        Assert.Equal(expected, native.Options![OptionConstants.ImageCropKey]);
    }

    [Fact]
    public void LeftAndRightCardsShareOneRangeButHaveIndependentDefaults()
    {
        var left = OptionCatalog.All.Single(option => option.Key == OptionConstants.MemcardLeftIndexKey);
        var right = OptionCatalog.All.Single(option => option.Key == OptionConstants.MemcardRightIndexKey);
        Assert.Same(left.NumericRange, right.NumericRange);
        Assert.Equal("0", left.DefaultValue);
        Assert.Equal("1", right.DefaultValue);
    }

    [Theory]
    [InlineData("1", "+1px")]
    [InlineData("-1", "-1px")]
    [InlineData("0", "disabled")]
    public void ImageOffsetPreservesTheNativeSign(string input, string expected)
    {
        var native = CoreSettingsFunctions.Configure(Definition(), Configuration(OptionConstants.ImageOffsetKey, input));
        Assert.Equal(expected, native.Options![OptionConstants.ImageOffsetKey]);
    }

    private static MachineConfiguration Configuration(string key, string value) =>
        new(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation,
            "test", Options: new Dictionary<string, string> { [key] = value });

    private static CoreDefinition Definition() =>
        GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants.Definition
            with { Options = OptionCatalog.All };
}
