using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(RendererBackend value) => value switch
    {
        RendererBackend.Automatic => "Auto",
        RendererBackend.OpenGl => "OpenGL",
        RendererBackend.Direct3D11 => "D3D11",
        RendererBackend.Direct3D12 => "D3D12",
        RendererBackend.Vulkan => "Vulkan",
        RendererBackend.ParallelGs => "paraLLEl-GS",
        RendererBackend.SoftwareHardwarePresentation => "Software (HW)",
        RendererBackend.SoftwarePresentation => "Software (SW)",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(NativeScalingMode value) => value switch
    {
        NativeScalingMode.Automatic => "Automatic",
        NativeScalingMode.Disabled => "disabled",
        NativeScalingMode.Normal => "Normal",
        NativeScalingMode.Aggressive => "Aggressive",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(ParallelSupersampling value) => value switch
    {
        ParallelSupersampling.Native => "Native",
        ParallelSupersampling.Samples2 => "2x SSAA",
        ParallelSupersampling.Samples4Sparse => "4x SSAA (sparse grid)",
        ParallelSupersampling.Samples4Ordered => "4x SSAA (ordered, can high-res)",
        ParallelSupersampling.Samples8 => "8x SSAA (can high-res)",
        ParallelSupersampling.Samples16 => "16x SSAA (can high-res)",
        ParallelSupersampling.Samples16Ordered => "16x SSAA (ordered, can high-res 4x)",
        ParallelSupersampling.Samples32Ordered => "32x SSAA (ordered, can high-res 4x full)",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(ParallelScanout value) => value switch
    {
        ParallelScanout.Scale2 => "enabled",
        ParallelScanout.Scale4 => "enabled (4x)",
        ParallelScanout.Scale4Antialiased => "enabled (4x, anti-aliased)",
        ParallelScanout.Scale4FullField => "enabled (4x, full field height)",
        ParallelScanout.Scale4AntialiasedFullField => "enabled (4x, anti-aliased, full field height)",
        ParallelScanout.Disabled => "disabled",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(DeinterlacingMode value) => value switch
    {
        DeinterlacingMode.Automatic => "Automatic",
        DeinterlacingMode.Off => "Off",
        DeinterlacingMode.WeaveTopFirst => "Weave TFF",
        DeinterlacingMode.WeaveBottomFirst => "Weave BFF",
        DeinterlacingMode.BobTopFirst => "Bob TFF",
        DeinterlacingMode.BobBottomFirst => "Bob BFF",
        DeinterlacingMode.BlendTopFirst => "Blend TFF",
        DeinterlacingMode.BlendBottomFirst => "Blend BFF",
        DeinterlacingMode.AdaptiveTopFirst => "Adaptive TFF",
        DeinterlacingMode.AdaptiveBottomFirst => "Adaptive BFF",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(HardwareDownloadMode value) => value switch
    {
        HardwareDownloadMode.Accurate => "Accurate",
        HardwareDownloadMode.DisableReadbacks => "Disable Readbacks",
        HardwareDownloadMode.Unsynchronized => "Unsynchronized",
        HardwareDownloadMode.Disabled => "Disabled",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(TextureFilteringMode value) => value switch
    {
        TextureFilteringMode.Nearest => "Nearest",
        TextureFilteringMode.ForcedBilinear => "Bilinear (Forced)",
        TextureFilteringMode.ConsoleBilinear => "Bilinear (PS2)",
        TextureFilteringMode.ForcedBilinearExceptSprites => "Bilinear (Forced excluding sprite)",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(TrilinearFilteringMode value) => value switch
    {
        TrilinearFilteringMode.Automatic => "Automatic",
        TrilinearFilteringMode.Disabled => "disabled",
        TrilinearFilteringMode.Console => "Trilinear (PS2)",
        TrilinearFilteringMode.Forced => "Trilinear (Forced)",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(DitheringMode value) => value switch
    {
        DitheringMode.Disabled => "disabled",
        DitheringMode.Scaled => "Scaled",
        DitheringMode.Unscaled => "Unscaled",
        DitheringMode.Force32Bit => "Force 32bit",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(BlendingAccuracy value) => value switch
    {
        BlendingAccuracy.Minimum => "Minimum",
        BlendingAccuracy.Basic => "Basic",
        BlendingAccuracy.Medium => "Medium",
        BlendingAccuracy.High => "High",
        BlendingAccuracy.Full => "Full",
        BlendingAccuracy.Maximum => "Maximum",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(CpuExecutionProvider value) => value switch
    {
        CpuExecutionProvider.Recompiler => "Recompiler",
        CpuExecutionProvider.Interpreter => "Interpreter",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(CycleSkipping value) => value switch
    {
        CycleSkipping.Disabled => "disabled",
        CycleSkipping.Mild => "Mild Underclock",
        CycleSkipping.Moderate => "Moderate Underclock",
        CycleSkipping.Maximum => "Maximum Underclock",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(FrameratePatch value) => value switch
    {
        FrameratePatch.Disabled => "disabled",
        FrameratePatch.Enabled => "enabled",
        FrameratePatch.PalToNtsc => "60fps PAL-to-NTSC",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(CpuSpriteLevel value) => value switch
    {
        CpuSpriteLevel.Sprites => "Sprites Only",
        CpuSpriteLevel.SpritesAndTriangles => "Sprites/Triangles",
        CpuSpriteLevel.BlendedSpritesAndTriangles => "Blended Sprites/Triangles",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(ClutRendering value) => value switch
    {
        ClutRendering.Disabled => "disabled",
        ClutRendering.Normal => "Normal",
        ClutRendering.Aggressive => "Aggressive",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(AutoFlushMode value) => value switch
    {
        AutoFlushMode.Disabled => "disabled",
        AutoFlushMode.Sprites => "Sprites Only",
        AutoFlushMode.AllPrimitives => "All Primitives",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(RenderTargetTextureMode value) => value switch
    {
        RenderTargetTextureMode.Disabled => "disabled",
        RenderTargetTextureMode.InsideTarget => "Inside Target",
        RenderTargetTextureMode.MergeTargets => "Merge Targets",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(HalfPixelOffset value) => value switch
    {
        HalfPixelOffset.Disabled => "disabled",
        HalfPixelOffset.NormalVertex => "Normal (Vertex)",
        HalfPixelOffset.SpecialTexture => "Special (Texture)",
        HalfPixelOffset.AggressiveTexture => "Special (Texture - Aggressive)",
        HalfPixelOffset.AlignToNative => "Align to Native",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(SpriteRounding value) => value switch
    {
        SpriteRounding.Disabled => "disabled",
        SpriteRounding.Half => "Half",
        SpriteRounding.Full => "Full",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(AnalogAxisInversion value) => value switch
    {
        AnalogAxisInversion.Disabled => "disabled",
        AnalogAxisInversion.Horizontal => "x_axis",
        AnalogAxisInversion.Vertical => "y_axis",
        AnalogAxisInversion.Both => "all",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

}
