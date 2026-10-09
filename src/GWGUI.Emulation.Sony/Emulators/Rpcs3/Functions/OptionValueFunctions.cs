using System.Globalization;
using AnisotropicFilter = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.AnisotropicFilter;
using ClocksScale = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ClocksScale;
using DefaultResolution = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.DefaultResolution;
using Dns = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.Dns;
using DriverRecoveryTimeout = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.DriverRecoveryTimeout;
using DriverWakeupDelay = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.DriverWakeupDelay;
using EnterButton = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.EnterButton;
using FbAliasingBias = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.FbAliasingBias;
using FrameLimit = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.FrameLimit;
using FramePacing = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.FramePacing;
using Language = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.Language;
using LicenseArea = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.LicenseArea;
using LlvmThreads = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.LlvmThreads;
using MaxPreemptCount = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.MaxPreemptCount;
using Msaa = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.Msaa;
using PerfOverlay = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.PerfOverlay;
using PpuDecoder = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.PpuDecoder;
using PsnStatus = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.PsnStatus;
using Renderer = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.Renderer;
using RsxFifoAccuracy = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.RsxFifoAccuracy;
using ShaderCompilerThreads = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ShaderCompilerThreads;
using ShaderMode = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ShaderMode;
using ShaderQuality = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ShaderQuality;
using SleepTimersAccuracy = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.SleepTimersAccuracy;
using SpuBlockSize = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.SpuBlockSize;
using SpuDecoder = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.SpuDecoder;
using SpuXfloatAccuracy = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.SpuXfloatAccuracy;
using StretchToDisplay = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.StretchToDisplay;
using ThreadScheduler = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ThreadScheduler;
using VblankRate = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.VblankRate;
using ZcullAccuracy = GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums.ZcullAccuracy;
using GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(PpuDecoder value) => value switch
    {
        PpuDecoder.Llvm => OptionConstants.PpuDecoderLlvm,
        PpuDecoder.Interpreter => OptionConstants.PpuDecoderInterpreter,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SpuDecoder value) => value switch
    {
        SpuDecoder.Llvm => OptionConstants.SpuDecoderLlvm,
        SpuDecoder.Asmjit => OptionConstants.SpuDecoderAsmjit,
        SpuDecoder.Dynamic => OptionConstants.SpuDecoderDynamic,
        SpuDecoder.Interpreter => OptionConstants.SpuDecoderInterpreter,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SpuXfloatAccuracy value) => value switch
    {
        SpuXfloatAccuracy.Accurate => OptionConstants.SpuXfloatAccuracyAccurate,
        SpuXfloatAccuracy.Approximate => OptionConstants.SpuXfloatAccuracyApproximate,
        SpuXfloatAccuracy.Relaxed => OptionConstants.SpuXfloatAccuracyRelaxed,
        SpuXfloatAccuracy.Inaccurate => OptionConstants.SpuXfloatAccuracyInaccurate,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SpuBlockSize value) => value switch
    {
        SpuBlockSize.Safe => OptionConstants.SpuBlockSizeSafe,
        SpuBlockSize.Mega => OptionConstants.SpuBlockSizeMega,
        SpuBlockSize.Giga => OptionConstants.SpuBlockSizeGiga,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SleepTimersAccuracy value) => value switch
    {
        SleepTimersAccuracy.Auto => OptionConstants.SleepTimersAccuracyAuto,
        SleepTimersAccuracy.AsHost => OptionConstants.SleepTimersAccuracyAsHost,
        SleepTimersAccuracy.Usleep => OptionConstants.SleepTimersAccuracyUsleep,
        SleepTimersAccuracy.AllTimers => OptionConstants.SleepTimersAccuracyAllTimers,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ClocksScale value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Renderer value) => value switch
    {
        Renderer.Opengl => OptionConstants.RendererOpengl,
        Renderer.Null => OptionConstants.RendererNull,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(FrameLimit value) => value switch
    {
        FrameLimit.Auto => OptionConstants.FrameLimitAuto,
        FrameLimit.Ps3 => OptionConstants.FrameLimitPs3,
        FrameLimit.Off => OptionConstants.FrameLimitOff,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnisotropicFilter value) => value switch
    {
        AnisotropicFilter.Auto => OptionConstants.AnisotropicFilterAuto,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Msaa value) => value switch
    {
        Msaa.Auto => OptionConstants.MsaaAuto,
        Msaa.Disabled => OptionConstants.MsaaDisabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ZcullAccuracy value) => value switch
    {
        ZcullAccuracy.Precise => OptionConstants.ZcullAccuracyPrecise,
        ZcullAccuracy.Approximate => OptionConstants.ZcullAccuracyApproximate,
        ZcullAccuracy.Relaxed => OptionConstants.ZcullAccuracyRelaxed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ShaderQuality value) => value switch
    {
        ShaderQuality.Low => OptionConstants.ShaderQualityLow,
        ShaderQuality.High => OptionConstants.ShaderQualityHigh,
        ShaderQuality.Ultra => OptionConstants.ShaderQualityUltra,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DefaultResolution value) => value switch
    {
        DefaultResolution._720p => OptionConstants.DefaultResolution720p,
        DefaultResolution._1080p => OptionConstants.DefaultResolution1080p,
        DefaultResolution._480p => OptionConstants.DefaultResolution480p,
        DefaultResolution._576p => OptionConstants.DefaultResolution576p,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ShaderMode value) => value switch
    {
        ShaderMode.Async => OptionConstants.ShaderModeAsync,
        ShaderMode.AsyncInterpreter => OptionConstants.ShaderModeAsyncInterpreter,
        ShaderMode.AsyncRecompiler => OptionConstants.ShaderModeAsyncRecompiler,
        ShaderMode.Interpreter => OptionConstants.ShaderModeInterpreter,
        ShaderMode.Sync => OptionConstants.ShaderModeSync,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(StretchToDisplay value) => value switch
    {
        StretchToDisplay.Disabled => OptionConstants.StretchToDisplayDisabled,
        StretchToDisplay.Enabled => OptionConstants.StretchToDisplayEnabled,
        StretchToDisplay._1610 => OptionConstants.StretchToDisplay1610,
        StretchToDisplay._219 => OptionConstants.StretchToDisplay219,
        StretchToDisplay._329 => OptionConstants.StretchToDisplay329,
        StretchToDisplay._43 => OptionConstants.StretchToDisplay43,
        StretchToDisplay._54 => OptionConstants.StretchToDisplay54,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(RsxFifoAccuracy value) => value switch
    {
        RsxFifoAccuracy.Fast => OptionConstants.RsxFifoAccuracyFast,
        RsxFifoAccuracy.Atomic => OptionConstants.RsxFifoAccuracyAtomic,
        RsxFifoAccuracy.AtomicOrdered => OptionConstants.RsxFifoAccuracyAtomicOrdered,
        RsxFifoAccuracy.AsPs3 => OptionConstants.RsxFifoAccuracyAsPs3,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DriverWakeupDelay value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(VblankRate value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ThreadScheduler value) => value switch
    {
        ThreadScheduler.Os => OptionConstants.ThreadSchedulerOs,
        ThreadScheduler.Old => OptionConstants.ThreadSchedulerOld,
        ThreadScheduler.Alt => OptionConstants.ThreadSchedulerAlt,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(LlvmThreads value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ShaderCompilerThreads value) => value switch
    {
        ShaderCompilerThreads.Auto => OptionConstants.ShaderCompilerThreadsAuto,
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(MaxPreemptCount value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(PsnStatus value) => value switch
    {
        PsnStatus.Disabled => OptionConstants.PsnStatusDisabled,
        PsnStatus.Simulated => OptionConstants.PsnStatusSimulated,
        PsnStatus.Rpcn => OptionConstants.PsnStatusRpcn,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Dns value) => value switch
    {
        Dns._8Point8Point8Point8 => OptionConstants.Dns8Point8Point8Point8,
        Dns._1Point1Point1Point1 => OptionConstants.Dns1Point1Point1Point1,
        Dns._208Point67Point222Point222 => OptionConstants.Dns208Point67Point222Point222,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(FbAliasingBias value) => value switch
    {
        FbAliasingBias.Auto => OptionConstants.FbAliasingBiasAuto,
        FbAliasingBias.Color => OptionConstants.FbAliasingBiasColor,
        FbAliasingBias.Depth => OptionConstants.FbAliasingBiasDepth,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(PerfOverlay value) => value switch
    {
        PerfOverlay.Disabled => OptionConstants.PerfOverlayDisabled,
        PerfOverlay.Minimal => OptionConstants.PerfOverlayMinimal,
        PerfOverlay.Low => OptionConstants.PerfOverlayLow,
        PerfOverlay.Medium => OptionConstants.PerfOverlayMedium,
        PerfOverlay.High => OptionConstants.PerfOverlayHigh,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(FramePacing value) => value switch
    {
        FramePacing.Frontend => OptionConstants.FramePacingFrontend,
        FramePacing.Emulator => OptionConstants.FramePacingEmulator,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Language value) => value switch
    {
        Language.English => OptionConstants.LanguageEnglish,
        Language.Japanese => OptionConstants.LanguageJapanese,
        Language.French => OptionConstants.LanguageFrench,
        Language.Spanish => OptionConstants.LanguageSpanish,
        Language.German => OptionConstants.LanguageGerman,
        Language.Italian => OptionConstants.LanguageItalian,
        Language.Dutch => OptionConstants.LanguageDutch,
        Language.Portuguese => OptionConstants.LanguagePortuguese,
        Language.Russian => OptionConstants.LanguageRussian,
        Language.Korean => OptionConstants.LanguageKorean,
        Language.ChineseTrad => OptionConstants.LanguageChineseTrad,
        Language.ChineseSimp => OptionConstants.LanguageChineseSimp,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(LicenseArea value) => value switch
    {
        LicenseArea.Usa => OptionConstants.LicenseAreaUsa,
        LicenseArea.Eu => OptionConstants.LicenseAreaEu,
        LicenseArea.Jp => OptionConstants.LicenseAreaJp,
        LicenseArea.Hk => OptionConstants.LicenseAreaHk,
        LicenseArea.Kr => OptionConstants.LicenseAreaKr,
        LicenseArea.Cn => OptionConstants.LicenseAreaCn,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(EnterButton value) => value switch
    {
        EnterButton.Cross => OptionConstants.EnterButtonCross,
        EnterButton.Circle => OptionConstants.EnterButtonCircle,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DriverRecoveryTimeout value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
