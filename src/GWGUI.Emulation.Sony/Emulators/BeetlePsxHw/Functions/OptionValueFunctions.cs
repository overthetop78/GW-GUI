using System.Globalization;
using BlackSetup = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.BlackSetup;
using ColorFormat = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.ColorFormat;
using Depth = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.Depth;
using Filter = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.Filter;
using HdCachingMethod = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdCachingMethod;
using HdDumpMode = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdDumpMode;
using HdReplacementMode = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdReplacementMode;
using HdrOverbright = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdrOverbright;
using HdrSdrGamma = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdrSdrGamma;
using HdrShoulder = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.HdrShoulder;
using Msaa = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.Msaa;
using Renderer = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.Renderer;
using SrcPrimaries = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.SrcPrimaries;
using TextureDirectory = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.TextureDirectory;
using VideoCable = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Enums.VideoCable;
using GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(GpuOverclock value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(CdAccessMethod value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(OverrideBios value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(Region value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(InternalResolution value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(Renderer value) => value switch
    {
        Renderer.HardwareGl => OptionConstants.RendererHardwareGl,
        Renderer.Software => OptionConstants.RendererSoftware,
        Renderer.Hardware => OptionConstants.RendererHardware,
        Renderer.HardwareVk => OptionConstants.RendererHardwareVk,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(ColorFormat value) => value switch
    {
        ColorFormat._24Bit => OptionConstants.ColorFormat24bit,
        ColorFormat._30bitHdr => OptionConstants.ColorFormat30bitHdr,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdrShoulder value) => value switch
    {
        HdrShoulder.Reinhard => OptionConstants.HdrShoulderReinhard,
        HdrShoulder.Aces => OptionConstants.HdrShoulderAces,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(SrcPrimaries value) => value switch
    {
        SrcPrimaries.Off => OptionConstants.SrcPrimariesOff,
        SrcPrimaries.Auto => OptionConstants.SrcPrimariesAuto,
        SrcPrimaries.Smptec => OptionConstants.SrcPrimariesSmptec,
        SrcPrimaries.Ebu => OptionConstants.SrcPrimariesEbu,
        SrcPrimaries.Ntsc1953 => OptionConstants.SrcPrimariesNtsc1953,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(BlackSetup value) => value switch
    {
        BlackSetup.Off => OptionConstants.BlackSetupOff,
        BlackSetup.Lifted => OptionConstants.BlackSetupLifted,
        BlackSetup.Crushed => OptionConstants.BlackSetupCrushed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(VideoCable value) => value switch
    {
        VideoCable.Off => OptionConstants.VideoCableOff,
        VideoCable.Rgb => OptionConstants.VideoCableRgb,
        VideoCable.Svideo => OptionConstants.VideoCableSvideo,
        VideoCable.Composite => OptionConstants.VideoCableComposite,
        VideoCable.Rf => OptionConstants.VideoCableRf,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdrSdrGamma value) => value switch
    {
        HdrSdrGamma.Bt1886 => OptionConstants.HdrSdrGammaBt1886,
        HdrSdrGamma.Gamma22 => OptionConstants.HdrSdrGammaGamma22,
        HdrSdrGamma.Srgb => OptionConstants.HdrSdrGammaSrgb,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdrOverbright value) => value switch
    {
        HdrOverbright.Clamped => OptionConstants.HdrOverbrightClamped,
        HdrOverbright.Hot => OptionConstants.HdrOverbrightHot,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Depth value) => value switch
    {
        Depth._16bppNative => OptionConstants.Depth16bppNative,
        Depth._32bpp => OptionConstants.Depth32bpp,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(DitherMode value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(Filter value) => value switch
    {
        Filter.Nearest => OptionConstants.FilterNearest,
        Filter.SABR => OptionConstants.FilterSABR,
        Filter.XBR => OptionConstants.FilterXBR,
        Filter.Bilinear => OptionConstants.FilterBilinear,
        Filter._3Point => OptionConstants.Filter3Point,
        Filter.JINC2 => OptionConstants.FilterJINC2,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(TextureFilterExclusion value) => value switch
    {
        TextureFilterExclusion.Disabled => OptionConstants.FilterExcludeSpriteDisabled,
        TextureFilterExclusion.Opaque => OptionConstants.FilterExcludeSpriteOpaque,
        TextureFilterExclusion.All => OptionConstants.FilterExcludeSpriteAll,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(Msaa value) => value switch
    {
        _ when Enum.IsDefined(value) => ((decimal)value).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture) + OptionNumericConstants.MultiplierSuffix,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdDumpMode value) => value switch
    {
        HdDumpMode.UploadRect => OptionConstants.HdDumpModeUploadRect,
        HdDumpMode.PageAligned => OptionConstants.HdDumpModePageAligned,
        HdDumpMode.Both => OptionConstants.HdDumpModeBoth,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(TextureDirectory value) => value switch
    {
        TextureDirectory.Content => OptionConstants.TextureDirectoryContent,
        TextureDirectory.System => OptionConstants.TextureDirectorySystem,
        TextureDirectory.Save => OptionConstants.TextureDirectorySave,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdReplacementMode value) => value switch
    {
        HdReplacementMode.UploadRect => OptionConstants.HdReplacementModeUploadRect,
        HdReplacementMode.PageAligned => OptionConstants.HdReplacementModePageAligned,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(HdCachingMethod value) => value switch
    {
        HdCachingMethod.Eager => OptionConstants.HdCachingMethodEager,
        HdCachingMethod.Lazy => OptionConstants.HdCachingMethodLazy,
        HdCachingMethod.LazySync => OptionConstants.HdCachingMethodLazySync,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string ToNative(PgxpMode value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(AnalogToggle value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(AnalogToggleCombo value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(GunInputMode value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(GunCursor value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(CrosshairColor value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(AnalogResponseCurve value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(UseMednafenMemcard0Method value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(LineRender value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(WidescreenHackAspectRatio value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(CpuDynarec value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(DynarecInvalidate value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(CoreTimingFps value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(Deinterlacer value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(AspectRatio value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);

    internal static string ToNative(CropOverscan value) => global::GWGUI.Emulation.Sony.Emulators.BeetlePsx.Functions.OptionValueFunctions.ToNative(value);
}
