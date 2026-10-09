using System.Globalization;
using AnisotropicFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AnisotropicFiltering;
using AspectRatio = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AspectRatio;
using AudioBufferMs = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AudioBufferMs;
using AxisDeadzone = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AxisDeadzone;
using AxisScale = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AxisScale;
using BlendingAccuracy = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.BlendingAccuracy;
using CasMode = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.CasMode;
using FrameLimiter = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.FrameLimiter;
using HwDownloadMode = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.HwDownloadMode;
using Lightgun = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Lightgun;
using Multitap = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Multitap;
using Renderer = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Renderer;
using TextureFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.TextureFiltering;
using TrilinearFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.TrilinearFiltering;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(Renderer value) => value switch
    {
        Renderer.Opengl => OptionConstants.RendererOpengl,
        Renderer.Software => OptionConstants.RendererSoftware,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HwDownloadMode value) => value switch
    {
        HwDownloadMode.Accurate => OptionConstants.HwDownloadModeAccurate,
        HwDownloadMode.Unsynchronized => OptionConstants.HwDownloadModeUnsynchronized,
        HwDownloadMode.Disabled => OptionConstants.HwDownloadModeDisabled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(BlendingAccuracy value) => value switch
    {
        BlendingAccuracy.Minimum => OptionConstants.BlendingAccuracyMinimum,
        BlendingAccuracy.Basic => OptionConstants.BlendingAccuracyBasic,
        BlendingAccuracy.Medium => OptionConstants.BlendingAccuracyMedium,
        BlendingAccuracy.High => OptionConstants.BlendingAccuracyHigh,
        BlendingAccuracy.Full => OptionConstants.BlendingAccuracyFull,
        BlendingAccuracy.Maximum => OptionConstants.BlendingAccuracyMaximum,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(TextureFiltering value) => value switch
    {
        TextureFiltering.Nearest => OptionConstants.TextureFilteringNearest,
        TextureFiltering.BilinearPs2 => OptionConstants.TextureFilteringBilinearPs2,
        TextureFiltering.BilinearForced => OptionConstants.TextureFilteringBilinearForced,
        TextureFiltering.BilinearForcedSprite => OptionConstants.TextureFilteringBilinearForcedSprite,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(TrilinearFiltering value) => value switch
    {
        TrilinearFiltering.Auto => OptionConstants.TrilinearFilteringAuto,
        TrilinearFiltering.Off => OptionConstants.TrilinearFilteringOff,
        TrilinearFiltering.Ps2 => OptionConstants.TrilinearFilteringPs2,
        TrilinearFiltering.Forced => OptionConstants.TrilinearFilteringForced,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AnisotropicFiltering value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(CasMode value) => value switch
    {
        CasMode.Disabled => OptionConstants.CasModeDisabled,
        CasMode.Sharpen => OptionConstants.CasModeSharpen,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AudioBufferMs value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(FrameLimiter value) => value switch
    {
        FrameLimiter.Frontend => OptionConstants.FrameLimiterFrontend,
        FrameLimiter.Internal => OptionConstants.FrameLimiterInternal,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AspectRatio value) => value switch
    {
        AspectRatio.Auto => OptionConstants.AspectRatioAuto,
        AspectRatio._43 => OptionConstants.AspectRatio43,
        AspectRatio._169 => OptionConstants.AspectRatio169,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Multitap value) => value switch
    {
        Multitap.Disabled => OptionConstants.MultitapDisabled,
        Multitap.Port1 => OptionConstants.MultitapPort1,
        Multitap.Port2 => OptionConstants.MultitapPort2,
        Multitap.Both => OptionConstants.MultitapBoth,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Lightgun value) => value switch
    {
        Lightgun.Disabled => OptionConstants.LightgunDisabled,
        Lightgun.Usb1 => OptionConstants.LightgunUsb1,
        Lightgun.Usb2 => OptionConstants.LightgunUsb2,
        Lightgun.Both => OptionConstants.LightgunBoth,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AxisScale value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(AxisDeadzone value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
