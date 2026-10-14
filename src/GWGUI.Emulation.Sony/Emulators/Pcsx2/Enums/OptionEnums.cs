namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;

internal enum RendererBackend
{
    Automatic,
    OpenGl,
    Direct3D11,
    Direct3D12,
    Vulkan,
    ParallelGs,
    SoftwareHardwarePresentation,
    SoftwarePresentation,
}

internal enum NativeScalingMode
{
    Automatic,
    Disabled,
    Normal,
    Aggressive,
}

internal enum ParallelSupersampling
{
    Native,
    Samples2,
    Samples4Sparse,
    Samples4Ordered,
    Samples8,
    Samples16,
    Samples16Ordered,
    Samples32Ordered,
}

internal enum ParallelScanout
{
    Scale2,
    Scale4,
    Scale4Antialiased,
    Scale4FullField,
    Scale4AntialiasedFullField,
    Disabled,
}

internal enum DeinterlacingMode
{
    Automatic,
    Off,
    WeaveTopFirst,
    WeaveBottomFirst,
    BobTopFirst,
    BobBottomFirst,
    BlendTopFirst,
    BlendBottomFirst,
    AdaptiveTopFirst,
    AdaptiveBottomFirst,
}

internal enum HardwareDownloadMode
{
    Accurate,
    DisableReadbacks,
    Unsynchronized,
    Disabled,
}

internal enum TextureFilteringMode
{
    Nearest,
    ForcedBilinear,
    ConsoleBilinear,
    ForcedBilinearExceptSprites,
}

internal enum TrilinearFilteringMode
{
    Automatic,
    Disabled,
    Console,
    Forced,
}

internal enum DitheringMode
{
    Disabled,
    Scaled,
    Unscaled,
    Force32Bit,
}

internal enum BlendingAccuracy
{
    Minimum,
    Basic,
    Medium,
    High,
    Full,
    Maximum,
}

internal enum CpuExecutionProvider
{
    Recompiler,
    Interpreter,
}

internal enum CycleSkipping
{
    Disabled,
    Mild,
    Moderate,
    Maximum,
}

internal enum FrameratePatch
{
    Disabled,
    Enabled,
    PalToNtsc,
}

internal enum CpuSpriteLevel
{
    Sprites,
    SpritesAndTriangles,
    BlendedSpritesAndTriangles,
}

internal enum ClutRendering
{
    Disabled,
    Normal,
    Aggressive,
}

internal enum AutoFlushMode
{
    Disabled,
    Sprites,
    AllPrimitives,
}

internal enum RenderTargetTextureMode
{
    Disabled,
    InsideTarget,
    MergeTargets,
}

internal enum HalfPixelOffset
{
    Disabled,
    NormalVertex,
    SpecialTexture,
    AggressiveTexture,
    AlignToNative,
}

internal enum SpriteRounding
{
    Disabled,
    Half,
    Full,
}

internal enum AnalogAxisInversion
{
    Disabled,
    Horizontal,
    Vertical,
    Both,
}

