using System.Globalization;
using AltFlip = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AltFlip;
using AnalogAxisModifier = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AnalogAxisModifier;
using AnalogCombo = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AnalogCombo;
using Bios = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Bios;
using DisplayFpsV2 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.DisplayFpsV2;
using Dithering = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Dithering;
using FrameskipType = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.FrameskipType;
using Memcard1 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Memcard1;
using Memcard2 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Memcard2;
using Multitap = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Multitap;
using Region = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Region;
using ScreenCentering = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ScreenCentering;
using ShowBiosBootlogo = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ShowBiosBootlogo;
using ShowOverscan = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ShowOverscan;
using SpuInterpolation = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.SpuInterpolation;
using GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(Region value) => value switch
    {
        Region.Auto => OptionConstants.RegionAuto,
        Region.NTSC => OptionConstants.RegionNTSC,
        Region.PAL => OptionConstants.RegionPAL,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Bios value) => value switch
    {
        Bios.Auto => OptionConstants.BiosAuto,
        Bios.HLE => OptionConstants.BiosHLE,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ShowBiosBootlogo value) => value switch
    {
        ShowBiosBootlogo.Disabled => OptionConstants.ShowBiosBootlogoDisabled,
        ShowBiosBootlogo.Enabled => OptionConstants.ShowBiosBootlogoEnabled,
        ShowBiosBootlogo.EnabledNoPcsx => OptionConstants.ShowBiosBootlogoEnabledNoPcsx,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Memcard1 value) => value switch
    {
        Memcard1.Libretro => OptionConstants.Memcard1Libretro,
        Memcard1.Serial => OptionConstants.Memcard1Serial,
        Memcard1.Shared => OptionConstants.Memcard1Shared,
        Memcard1.None => OptionConstants.Memcard1None,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Memcard2 value) => value switch
    {
        Memcard2.Serial => OptionConstants.Memcard2Serial,
        Memcard2.Shared => OptionConstants.Memcard2Shared,
        Memcard2.None => OptionConstants.Memcard2None,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Dithering value) => value switch
    {
        Dithering.Disabled => OptionConstants.DitheringDisabled,
        Dithering.Enabled => OptionConstants.DitheringEnabled,
        Dithering.Force => OptionConstants.DitheringForce,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AutomaticToggleState value) => value switch
    {
        AutomaticToggleState.Auto => OptionConstants.GpuThreadRenderingAuto,
        AutomaticToggleState.Disabled => OptionConstants.GpuThreadRenderingDisabled,
        AutomaticToggleState.Enabled => OptionConstants.GpuThreadRenderingEnabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(FrameskipType value) => value switch
    {
        FrameskipType.Disabled => OptionConstants.FrameskipTypeDisabled,
        FrameskipType.Auto => OptionConstants.FrameskipTypeAuto,
        FrameskipType.AutoThreshold => OptionConstants.FrameskipTypeAutoThreshold,
        FrameskipType.FixedInterval => OptionConstants.FrameskipTypeFixedInterval,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DisplayFpsV2 value) => value switch
    {
        DisplayFpsV2.Disabled => OptionConstants.DisplayFpsV2Disabled,
        DisplayFpsV2.Enabled => OptionConstants.DisplayFpsV2Enabled,
        DisplayFpsV2.Extra => OptionConstants.DisplayFpsV2Extra,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AltFlip value) => value switch
    {
        AltFlip.Auto => OptionConstants.AltFlipAuto,
        AltFlip.Early => OptionConstants.AltFlipEarly,
        AltFlip.Late => OptionConstants.AltFlipLate,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ShowOverscan value) => value switch
    {
        ShowOverscan.Disabled => OptionConstants.ShowOverscanDisabled,
        ShowOverscan.Auto => OptionConstants.ShowOverscanAuto,
        ShowOverscan.Hack => OptionConstants.ShowOverscanHack,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ScreenCentering value) => value switch
    {
        ScreenCentering.Auto => OptionConstants.ScreenCenteringAuto,
        ScreenCentering.Game => OptionConstants.ScreenCenteringGame,
        ScreenCentering.Borderless => OptionConstants.ScreenCenteringBorderless,
        ScreenCentering.Manual => OptionConstants.ScreenCenteringManual,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SpuInterpolation value) => value switch
    {
        SpuInterpolation.Simple => OptionConstants.SpuInterpolationSimple,
        SpuInterpolation.Gaussian => OptionConstants.SpuInterpolationGaussian,
        SpuInterpolation.Cubic => OptionConstants.SpuInterpolationCubic,
        SpuInterpolation.Off => OptionConstants.SpuInterpolationOff,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogAxisModifier value) => value switch
    {
        AnalogAxisModifier.Circle => OptionConstants.AnalogAxisModifierCircle,
        AnalogAxisModifier.Square => OptionConstants.AnalogAxisModifierSquare,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogCombo value) => value switch
    {
        AnalogCombo.Disabled => OptionConstants.AnalogComboDisabled,
        AnalogCombo.L1PlusR1PlusSelect => OptionConstants.AnalogComboL1PlusR1PlusSelect,
        AnalogCombo.L1PlusR1PlusStart => OptionConstants.AnalogComboL1PlusR1PlusStart,
        AnalogCombo.L1PlusR1PlusL3 => OptionConstants.AnalogComboL1PlusR1PlusL3,
        AnalogCombo.L1PlusR1PlusR3 => OptionConstants.AnalogComboL1PlusR1PlusR3,
        AnalogCombo.L3PlusR3 => OptionConstants.AnalogComboL3PlusR3,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Multitap value) => value switch
    {
        Multitap.Disabled => OptionConstants.MultitapDisabled,
        Multitap.Port1 => OptionConstants.MultitapPort1,
        Multitap.Port2 => OptionConstants.MultitapPort2,
        Multitap.Ports1And2 => OptionConstants.MultitapPorts1And2,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogResponseCurve value) => value switch
    {
        AnalogResponseCurve.Linear => OptionConstants.NegconResponseLinear,
        AnalogResponseCurve.Quadratic => OptionConstants.NegconResponseQuadratic,
        AnalogResponseCurve.Cubic => OptionConstants.NegconResponseCubic,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CrosshairVisibility value) => value switch
    {
        CrosshairVisibility.Disabled => OptionConstants.Crosshair1Disabled,
        CrosshairVisibility.Blue => OptionConstants.Crosshair1Blue,
        CrosshairVisibility.Green => OptionConstants.Crosshair1Green,
        CrosshairVisibility.Red => OptionConstants.Crosshair1Red,
        CrosshairVisibility.White => OptionConstants.Crosshair1White,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
