using System.Globalization;
using ConsoleRegion = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.ConsoleRegion;
using ControllerPortsMultitapMode = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.ControllerPortsMultitapMode;
using CPUExecutionMode = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.CPUExecutionMode;
using CPUFastmemMode = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.CPUFastmemMode;
using DisplayAspectRatio = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.DisplayAspectRatio;
using DisplayCropMode = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.DisplayCropMode;
using GPUDownsampleMode = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.GPUDownsampleMode;
using GPUMSAA = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.GPUMSAA;
using GPURenderer = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.GPURenderer;
using GPUShaderPrecompile = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.GPUShaderPrecompile;
using GPUTextureFilter = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.GPUTextureFilter;
using LoggingLogLevel = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.LoggingLogLevel;
using MemoryCardsCard1Type = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.MemoryCardsCard1Type;
using MemoryCardsCard2Type = GWGUI.Emulation.Sony.Emulators.SwanStation.Enums.MemoryCardsCard2Type;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(ConsoleRegion value) => value switch
    {
        ConsoleRegion.Auto => OptionConstants.ConsoleRegionAuto,
        ConsoleRegion.NTSCJ => OptionConstants.ConsoleRegionNTSCJ,
        ConsoleRegion.NTSCU => OptionConstants.ConsoleRegionNTSCU,
        ConsoleRegion.PAL => OptionConstants.ConsoleRegionPAL,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CPUExecutionMode value) => value switch
    {
        CPUExecutionMode.Interpreter => OptionConstants.CPUExecutionModeInterpreter,
        CPUExecutionMode.CachedInterpreter => OptionConstants.CPUExecutionModeCachedInterpreter,
        CPUExecutionMode.Recompiler => OptionConstants.CPUExecutionModeRecompiler,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GPURenderer value) => value switch
    {
        GPURenderer.OpenGL => OptionConstants.GPURendererOpenGL,
        GPURenderer.Software => OptionConstants.GPURendererSoftware,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GPUMSAA value) => value switch
    {
        GPUMSAA._2Ssaa => OptionConstants.GPUMSAA2Ssaa,
        GPUMSAA._4Ssaa => OptionConstants.GPUMSAA4Ssaa,
        GPUMSAA._8Ssaa => OptionConstants.GPUMSAA8Ssaa,
        GPUMSAA._16Ssaa => OptionConstants.GPUMSAA16Ssaa,
        GPUMSAA._32Ssaa => OptionConstants.GPUMSAA32Ssaa,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GPUTextureFilter value) => value switch
    {
        GPUTextureFilter.Nearest => OptionConstants.GPUTextureFilterNearest,
        GPUTextureFilter.Bilinear => OptionConstants.GPUTextureFilterBilinear,
        GPUTextureFilter.BilinearBinAlpha => OptionConstants.GPUTextureFilterBilinearBinAlpha,
        GPUTextureFilter.JINC2 => OptionConstants.GPUTextureFilterJINC2,
        GPUTextureFilter.JINC2BinAlpha => OptionConstants.GPUTextureFilterJINC2BinAlpha,
        GPUTextureFilter.XBR => OptionConstants.GPUTextureFilterXBR,
        GPUTextureFilter.XBRBinAlpha => OptionConstants.GPUTextureFilterXBRBinAlpha,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DisplayAspectRatio value) => value switch
    {
        DisplayAspectRatio._43 => OptionConstants.DisplayAspectRatio43,
        DisplayAspectRatio._169 => OptionConstants.DisplayAspectRatio169,
        DisplayAspectRatio._199 => OptionConstants.DisplayAspectRatio199,
        DisplayAspectRatio._209 => OptionConstants.DisplayAspectRatio209,
        DisplayAspectRatio.Custom => OptionConstants.DisplayAspectRatioCustom,
        DisplayAspectRatio.Auto => OptionConstants.DisplayAspectRatioAuto,
        DisplayAspectRatio.Native => OptionConstants.DisplayAspectRatioNative,
        DisplayAspectRatio.PAR11 => OptionConstants.DisplayAspectRatioPAR11,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DisplayCropMode value) => value switch
    {
        DisplayCropMode.None => OptionConstants.DisplayCropModeNone,
        DisplayCropMode.Overscan => OptionConstants.DisplayCropModeOverscan,
        DisplayCropMode.Borders => OptionConstants.DisplayCropModeBorders,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GPUDownsampleMode value) => value switch
    {
        GPUDownsampleMode.Disabled => OptionConstants.GPUDownsampleModeDisabled,
        GPUDownsampleMode.Box => OptionConstants.GPUDownsampleModeBox,
        GPUDownsampleMode.Adaptive => OptionConstants.GPUDownsampleModeAdaptive,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(GPUShaderPrecompile value) => value switch
    {
        GPUShaderPrecompile.Lazy => OptionConstants.GPUShaderPrecompileLazy,
        GPUShaderPrecompile.Enabled => OptionConstants.GPUShaderPrecompileEnabled,
        GPUShaderPrecompile.Disabled => OptionConstants.GPUShaderPrecompileDisabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(MemoryCardsCard1Type value) => value switch
    {
        MemoryCardsCard1Type.Libretro => OptionConstants.MemoryCardsCard1TypeLibretro,
        MemoryCardsCard1Type.Shared => OptionConstants.MemoryCardsCard1TypeShared,
        MemoryCardsCard1Type.PerGame => OptionConstants.MemoryCardsCard1TypePerGame,
        MemoryCardsCard1Type.PerGameTitle => OptionConstants.MemoryCardsCard1TypePerGameTitle,
        MemoryCardsCard1Type.None => OptionConstants.MemoryCardsCard1TypeNone,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(MemoryCardsCard2Type value) => value switch
    {
        MemoryCardsCard2Type.None => OptionConstants.MemoryCardsCard2TypeNone,
        MemoryCardsCard2Type.Shared => OptionConstants.MemoryCardsCard2TypeShared,
        MemoryCardsCard2Type.PerGame => OptionConstants.MemoryCardsCard2TypePerGame,
        MemoryCardsCard2Type.PerGameTitle => OptionConstants.MemoryCardsCard2TypePerGameTitle,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ControllerPortsMultitapMode value) => value switch
    {
        ControllerPortsMultitapMode.Disabled => OptionConstants.ControllerPortsMultitapModeDisabled,
        ControllerPortsMultitapMode.Port1Only => OptionConstants.ControllerPortsMultitapModePort1Only,
        ControllerPortsMultitapMode.Port2Only => OptionConstants.ControllerPortsMultitapModePort2Only,
        ControllerPortsMultitapMode.BothPorts => OptionConstants.ControllerPortsMultitapModeBothPorts,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnalogResponseCurve value) => value switch
    {
        AnalogResponseCurve.Linear => OptionConstants.Controller1TwistResponseLinear,
        AnalogResponseCurve.Quadratic => OptionConstants.Controller1TwistResponseQuadratic,
        AnalogResponseCurve.Cubic => OptionConstants.Controller1TwistResponseCubic,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(LoggingLogLevel value) => value switch
    {
        LoggingLogLevel.None => OptionConstants.LoggingLogLevelNone,
        LoggingLogLevel.Error => OptionConstants.LoggingLogLevelError,
        LoggingLogLevel.Warning => OptionConstants.LoggingLogLevelWarning,
        LoggingLogLevel.Perf => OptionConstants.LoggingLogLevelPerf,
        LoggingLogLevel.Success => OptionConstants.LoggingLogLevelSuccess,
        LoggingLogLevel.Info => OptionConstants.LoggingLogLevelInfo,
        LoggingLogLevel.Dev => OptionConstants.LoggingLogLevelDev,
        LoggingLogLevel.Profile => OptionConstants.LoggingLogLevelProfile,
        LoggingLogLevel.Debug => OptionConstants.LoggingLogLevelDebug,
        LoggingLogLevel.Trace => OptionConstants.LoggingLogLevelTrace,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CPUFastmemMode value) => value switch
    {
        CPUFastmemMode.Disabled => OptionConstants.CPUFastmemModeDisabled,
        CPUFastmemMode.LUT => OptionConstants.CPUFastmemModeLUT,
        CPUFastmemMode.MMap => OptionConstants.CPUFastmemModeMMap,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
