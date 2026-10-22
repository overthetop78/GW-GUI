using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using SoftwareValues = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions;
using HardwareValues = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Functions.OptionValueFunctions;
using RearmedValues = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Functions.OptionValueFunctions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class BeetleNativeValueTests
{
    [Fact]
    public void NativeMappingsKeepTheNinetyTwoValuesCapturedBeforeReuseAndRejectUndefinedValues()
    {
        Verify<LineRender>(["aggressive", "default", "disabled"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<AnalogResponseCurve>(["cubic", "linear", "quadratic"], SoftwareValues.ToNative, HardwareValues.ToNative, RearmedValues.ToNative);
        Verify<CrosshairColor>(["black", "blue", "cyan", "green", "orange", "pink", "purple", "red", "white", "yellow"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<OverrideBios>(["disabled", "openbios", "ps1_rom", "psxonpsp"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<DitherMode>(["1x(native)", "disabled", "internal resolution"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<AspectRatio>(["16:9", "4:3", "corrected", "ntsc", "uncorrected"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<AnalogToggleCombo>(["l1+l2+r1+r2+start+select", "l1+r1+l3", "l1+r1+r3", "l1+r1+select", "l1+r1+start", "l2+r2+l3", "l2+r2+r3", "l2+r2+select", "l2+r2+start", "l3+r3"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<GpuOverclock>(["2x", "4x", "8x", "16x", "32x", "1x(native)"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<CdAccessMethod>(["async", "precache", "sync"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<Deinterlacer>(["bob", "bob_offset", "fastmad", "off", "weave"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<CpuDynarec>(["disabled", "execute", "run_interpreter"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<PgxpMode>(["disabled", "memory + CPU", "memory only"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<UseMednafenMemcard0Method>(["libretro", "mednafen"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<GunInputMode>(["lightgun", "touchscreen"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<InternalResolution>(["2x", "4x", "8x", "16x", "1x(native)"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<WidescreenHackAspectRatio>(["16:10", "16:9", "18:9", "19:9", "20:9", "21:9", "32:9"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<CoreTimingFps>(["auto_toggle", "force_interlaced", "force_progressive"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<GunCursor>(["cross", "dot", "off"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<CropOverscan>(["disabled", "smart", "static"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<DynarecInvalidate>(["dma", "full"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<Region>(["auto", "ntsc-j", "ntsc-u", "pal"], SoftwareValues.ToNative, HardwareValues.ToNative);
        Verify<AnalogToggle>(["disabled", "enabled", "enabled-analog"], SoftwareValues.ToNative, HardwareValues.ToNative);
    }

    private static void Verify<T>(string[] expected, params Func<T, string>[] conversions) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        Assert.Equal(expected.Length, values.Length);
        foreach (var conversion in conversions)
        {
            for (var index = 0; index < values.Length; index++)
                Assert.Equal(expected[index], conversion(values[index]));
            var invalid = (T)Enum.ToObject(typeof(T), int.MaxValue);
            Assert.False(Enum.IsDefined(invalid));
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => conversion(invalid));
            Assert.Equal("value", error.ParamName);
            Assert.Equal(invalid, error.ActualValue);
        }
    }
}

