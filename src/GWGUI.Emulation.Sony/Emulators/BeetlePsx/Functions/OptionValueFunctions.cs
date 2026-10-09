using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(GpuOverclock value) => value switch
    {
        GpuOverclock._1xNative => OptionConstants.GpuOverclock1xNative,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture) + OptionNumericConstants.MultiplierSuffix,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CdAccessMethod value) => value switch
    {
        CdAccessMethod.Sync => OptionConstants.CdAccessMethodSync,
        CdAccessMethod.Async => OptionConstants.CdAccessMethodAsync,
        CdAccessMethod.Precache => OptionConstants.CdAccessMethodPrecache,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(OverrideBios value) => value switch
    {
        OverrideBios.Disabled => OptionConstants.OverrideBiosDisabled,
        OverrideBios.Psxonpsp => OptionConstants.OverrideBiosPsxonpsp,
        OverrideBios.Ps1Rom => OptionConstants.OverrideBiosPs1Rom,
        OverrideBios.Openbios => OptionConstants.OverrideBiosOpenbios,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Region value) => value switch
    {
        Region.Auto => OptionConstants.RegionAuto,
        Region.NtscJ => OptionConstants.RegionNtscJ,
        Region.NtscU => OptionConstants.RegionNtscU,
        Region.Pal => OptionConstants.RegionPal,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(InternalResolution value) => value switch
    {
        InternalResolution._1xNative => OptionConstants.InternalResolution1xNative,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture) + OptionNumericConstants.MultiplierSuffix,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DitherMode value) => value switch
    {
        DitherMode._1xNative => OptionConstants.DitherMode1xNative,
        DitherMode.InternalResolution => OptionConstants.DitherModeInternalResolution,
        DitherMode.Disabled => OptionConstants.DitherModeDisabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(PgxpMode value) => value switch
    {
        PgxpMode.Disabled => OptionConstants.PgxpModeDisabled,
        PgxpMode.MemoryOnly => OptionConstants.PgxpModeMemoryOnly,
        PgxpMode.MemoryPlusCPU => OptionConstants.PgxpModeMemoryPlusCPU,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogToggle value) => value switch
    {
        AnalogToggle.Disabled => OptionConstants.AnalogToggleDisabled,
        AnalogToggle.Enabled => OptionConstants.AnalogToggleEnabled,
        AnalogToggle.EnabledAnalog => OptionConstants.AnalogToggleEnabledAnalog,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogToggleCombo value) => value switch
    {
        AnalogToggleCombo.L1PlusL2PlusR1PlusR2PlusStartPlusSelect => OptionConstants.AnalogToggleComboL1PlusL2PlusR1PlusR2PlusStartPlusSelect,
        AnalogToggleCombo.L1PlusR1PlusSelect => OptionConstants.AnalogToggleComboL1PlusR1PlusSelect,
        AnalogToggleCombo.L1PlusR1PlusStart => OptionConstants.AnalogToggleComboL1PlusR1PlusStart,
        AnalogToggleCombo.L1PlusR1PlusL3 => OptionConstants.AnalogToggleComboL1PlusR1PlusL3,
        AnalogToggleCombo.L1PlusR1PlusR3 => OptionConstants.AnalogToggleComboL1PlusR1PlusR3,
        AnalogToggleCombo.L2PlusR2PlusSelect => OptionConstants.AnalogToggleComboL2PlusR2PlusSelect,
        AnalogToggleCombo.L2PlusR2PlusStart => OptionConstants.AnalogToggleComboL2PlusR2PlusStart,
        AnalogToggleCombo.L2PlusR2PlusL3 => OptionConstants.AnalogToggleComboL2PlusR2PlusL3,
        AnalogToggleCombo.L2PlusR2PlusR3 => OptionConstants.AnalogToggleComboL2PlusR2PlusR3,
        AnalogToggleCombo.L3PlusR3 => OptionConstants.AnalogToggleComboL3PlusR3,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GunInputMode value) => value switch
    {
        GunInputMode.Lightgun => OptionConstants.GunInputModeLightgun,
        GunInputMode.Touchscreen => OptionConstants.GunInputModeTouchscreen,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GunCursor value) => value switch
    {
        GunCursor.Off => OptionConstants.GunCursorOff,
        GunCursor.Cross => OptionConstants.GunCursorCross,
        GunCursor.Dot => OptionConstants.GunCursorDot,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CrosshairColor value) => value switch
    {
        CrosshairColor.Red => OptionConstants.CrosshairColorP1Red,
        CrosshairColor.Blue => OptionConstants.CrosshairColorP1Blue,
        CrosshairColor.Green => OptionConstants.CrosshairColorP1Green,
        CrosshairColor.Orange => OptionConstants.CrosshairColorP1Orange,
        CrosshairColor.Yellow => OptionConstants.CrosshairColorP1Yellow,
        CrosshairColor.Cyan => OptionConstants.CrosshairColorP1Cyan,
        CrosshairColor.Pink => OptionConstants.CrosshairColorP1Pink,
        CrosshairColor.Purple => OptionConstants.CrosshairColorP1Purple,
        CrosshairColor.Black => OptionConstants.CrosshairColorP1Black,
        CrosshairColor.White => OptionConstants.CrosshairColorP1White,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogResponseCurve value) => value switch
    {
        AnalogResponseCurve.Linear => OptionConstants.NegconResponseLinear,
        AnalogResponseCurve.Quadratic => OptionConstants.NegconResponseQuadratic,
        AnalogResponseCurve.Cubic => OptionConstants.NegconResponseCubic,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(UseMednafenMemcard0Method value) => value switch
    {
        UseMednafenMemcard0Method.Libretro => OptionConstants.UseMednafenMemcard0MethodLibretro,
        UseMednafenMemcard0Method.Mednafen => OptionConstants.UseMednafenMemcard0MethodMednafen,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(LineRender value) => value switch
    {
        LineRender.Default => OptionConstants.LineRenderDefault,
        LineRender.Aggressive => OptionConstants.LineRenderAggressive,
        LineRender.Disabled => OptionConstants.LineRenderDisabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(WidescreenHackAspectRatio value) => value switch
    {
        WidescreenHackAspectRatio._169 => OptionConstants.WidescreenHackAspectRatio169,
        WidescreenHackAspectRatio._1610 => OptionConstants.WidescreenHackAspectRatio1610,
        WidescreenHackAspectRatio._189 => OptionConstants.WidescreenHackAspectRatio189,
        WidescreenHackAspectRatio._199 => OptionConstants.WidescreenHackAspectRatio199,
        WidescreenHackAspectRatio._209 => OptionConstants.WidescreenHackAspectRatio209,
        WidescreenHackAspectRatio._219 => OptionConstants.WidescreenHackAspectRatio219,
        WidescreenHackAspectRatio._329 => OptionConstants.WidescreenHackAspectRatio329,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CpuDynarec value) => value switch
    {
        CpuDynarec.Disabled => OptionConstants.CpuDynarecDisabled,
        CpuDynarec.Execute => OptionConstants.CpuDynarecExecute,
        CpuDynarec.RunInterpreter => OptionConstants.CpuDynarecRunInterpreter,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DynarecInvalidate value) => value switch
    {
        DynarecInvalidate.Full => OptionConstants.DynarecInvalidateFull,
        DynarecInvalidate.Dma => OptionConstants.DynarecInvalidateDma,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DynarecOpCycles value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DynarecSpuSamples value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CoreTimingFps value) => value switch
    {
        CoreTimingFps.AutoToggle => OptionConstants.CoreTimingFpsAutoToggle,
        CoreTimingFps.ForceProgressive => OptionConstants.CoreTimingFpsForceProgressive,
        CoreTimingFps.ForceInterlaced => OptionConstants.CoreTimingFpsForceInterlaced,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Deinterlacer value) => value switch
    {
        Deinterlacer.Weave => OptionConstants.DeinterlacerWeave,
        Deinterlacer.Bob => OptionConstants.DeinterlacerBob,
        Deinterlacer.BobOffset => OptionConstants.DeinterlacerBobOffset,
        Deinterlacer.Fastmad => OptionConstants.DeinterlacerFastmad,
        Deinterlacer.Off => OptionConstants.DeinterlacerOff,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AspectRatio value) => value switch
    {
        AspectRatio.Corrected => OptionConstants.AspectRatioCorrected,
        AspectRatio.Uncorrected => OptionConstants.AspectRatioUncorrected,
        AspectRatio._43 => OptionConstants.AspectRatio43,
        AspectRatio.Ntsc => OptionConstants.AspectRatioNtsc,
        AspectRatio._169 => OptionConstants.AspectRatio169,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CropOverscan value) => value switch
    {
        CropOverscan.Disabled => OptionConstants.CropOverscanDisabled,
        CropOverscan.Static => OptionConstants.CropOverscanStatic,
        CropOverscan.Smart => OptionConstants.CropOverscanSmart,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
