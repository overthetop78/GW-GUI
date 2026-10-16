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
    [Theory]
    [InlineData("pcee2", "pcsx2_audio_buffer_ms", "100", "50|75|100|150|200", "50 ms|75 ms|100 ms|150 ms|200 ms")]
    [InlineData("pcee2", "pcsx2_axis_scale", "133", "100|115|133|150", "100%|115%|Emulation.Option.Sony.Value.133PercentDefault|150%")]
    [InlineData("pcee2", "pcsx2_axis_deadzone", "0", "0|5|10|15|20|30", "Emulation.Option.Sony.Value.0PercentDefault|5%|10%|15%|20%|30%")]
    [InlineData("pcee2", "pcsx2_anisotropic_filtering", "0", "0|2|4|8|16", "Emulation.Option.Sony.Value.Disabled|2x|4x|8x|16x")]
    [InlineData("beetle_psx_hw", "beetle_psx_hw_hd_cache_vram_budget", "3072", "256|512|1024|1536|2048|3072|4096|6144|8192",
        "256 MB|512 MB|1 GB|1.5 GB|2 GB|3 GB|4 GB|6 GB|8 GB")]
    [InlineData("beetle_psx_hw", "beetle_psx_hw_hd_cache_ram_budget", "2048", "256|512|1024|2048|4096|8192", "256 MB|512 MB|1 GB|2 GB|4 GB|8 GB")]
    [InlineData("beetle_psx_hw", "beetle_psx_hw_phase_error", "0", "0|5|10|25|45",
        "Emulation.Option.Sony.Value.Disabled|Emulation.Option.Sony.Value.5Degrees|Emulation.Option.Sony.Value.10Degrees|" +
        "Emulation.Option.Sony.Value.25Degrees|Emulation.Option.Sony.Value.45Degrees")]
    [InlineData("beetle_psx_hw", "beetle_psx_hw_dynarec_op_cycles", "2", "2|1",
        "Emulation.Option.Sony.Value.2Default|Emulation.Option.Sony.Value.1AccurateFixesParasiteEve2EtcPoint")]
    [InlineData("beetle_psx_hw", "beetle_psx_hw_dynarec_spu_samples", "1", "1|4|16", "Emulation.Option.Sony.Value.1Default|4|16")]
    [InlineData("beetle_psx", "beetle_psx_dynarec_op_cycles", "2", "2|1",
        "Emulation.Option.Sony.Value.2Default|Emulation.Option.Sony.Value.1AccurateFixesParasiteEve2EtcPoint")]
    [InlineData("beetle_psx", "beetle_psx_dynarec_spu_samples", "1", "1|4|16", "Emulation.Option.Sony.Value.1Default|4|16")]
    [InlineData("rpcs3", "rpcs3_clocks_scale", "100", "50|75|100|150|200|300", "50%|75%|100%|150%|200%|300%")]
    [InlineData("rpcs3", "rpcs3_driver_wakeup_delay", "0", "0|20|50|100|200|400|800", "Emulation.Option.Sony.Value.0Default|20|50|100|200|400|800")]
    [InlineData("rpcs3", "rpcs3_llvm_threads", "0", "0|1|2|3|4|6|8", "Emulation.Option.Sony.Value.Automatic|1|2|3|4|6|8")]
    [InlineData("rpcs3", "rpcs3_max_preempt_count", "0", "0|10|20|50|100|200|400", "Emulation.Option.Sony.Value.0Disabled|10|20|50|100|200|400")]
    [InlineData("rpcs3", "rpcs3_driver_recovery_timeout", "1000", "0|1000|2000|5000|10000",
        "Emulation.Option.Sony.Value.Disabled|Emulation.Option.Sony.Value.1Second|Emulation.Option.Sony.Value.2Seconds|" +
        "Emulation.Option.Sony.Value.5Seconds|Emulation.Option.Sony.Value.10Seconds")]
    public void NumericChoicesRetainNativeValuesDefaultsAndLabels(string adapter, string key, string expectedDefault, string expectedValues, string expectedLabels)
    {
        var options = adapter switch
        {
            "pcee2" => GWGUI.Emulation.Sony.Emulators.Pcee2.Dictionaries.OptionCatalog.All,
            "rpcs3" => GWGUI.Emulation.Sony.Emulators.Rpcs3.Dictionaries.OptionCatalog.All,
            "beetle_psx" => OptionCatalog.All,
            "beetle_psx_hw" => HwOptions.All,
            _ => throw new ArgumentOutOfRangeException(nameof(adapter))
        };
        var option = Assert.Single(options, option => option.Key == key);
        Assert.Equal(expectedDefault, option.DefaultValue);
        Assert.Equal(expectedValues.Split('|'), option.Values.Select(value => value.Value).ToArray());
        Assert.Equal(expectedLabels.Split('|'), option.Values.Select(value => value.Label).ToArray());
    }

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
        var field = CoreSettingsFunctions.Blocks(Definition(), native).SelectMany(block => block.Fields).Single(field => field.Id == OptionConstants.CpuFreqScaleKey);
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

    [Theory]
    [InlineData("12", "12%")]
    [InlineData("12%", "12%")]
    [InlineData("999", "50%")]
    [InlineData("not-a-number", "15%")]
    public void Pcsx2ConvertsTheSelectedPortWithoutChangingTheOtherPort(string value, string expected)
    {
        var configuration = new MachineConfiguration(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation2, "pcsx2",
            Options: new Dictionary<string, string> { ["pcsx2_axis_deadzone1"] = value, ["pcsx2_axis_deadzone2"] = "27%" });
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions.Pcsx2OptionFunctions.ToNative(configuration);
        Assert.Equal(expected, native.Options!["pcsx2_axis_deadzone1"]);
        Assert.Equal("27%", native.Options["pcsx2_axis_deadzone2"]);
        Assert.Equal(value, configuration.Options!["pcsx2_axis_deadzone1"]);
    }

    [Theory]
    [InlineData("0", "disabled")]
    [InlineData("disabled", "disabled")]
    [InlineData("14", "15%")]
    [InlineData("999", "100%")]
    public void Pcsx2VibrationZeroIsDisabledAndOtherLevelsUseNativeSteps(string value, string expected)
    {
        var configuration = new MachineConfiguration(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation2, "pcsx2",
            Options: new Dictionary<string, string> { ["pcsx2_enable_rumble1"] = value });
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions.Pcsx2OptionFunctions.ToNative(configuration);
        Assert.Equal(expected, native.Options!["pcsx2_enable_rumble1"]);
        Assert.False(native.Options.ContainsKey("pcsx2_enable_rumble2"));
    }

    private static MachineConfiguration Configuration(string key, string value) => new(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation, "test", Options: new Dictionary<string, string> { [key] = value });

    private static CoreDefinition Definition() => GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants.Definition with { Options = OptionCatalog.All };
}
