using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static partial class OptionCatalog
{
    private static IReadOnlyList<CoreOption> HardwareFixesOptions { get; } =
    [
        Define(OptionConstants.EnableHwHacksKey, OptionConstants.EnableHwHacksLabel, OptionConstants.EnableHwHacksHelp,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.CpuSpriteSizeKey, OptionConstants.CpuSpriteSizeLabel, null,
            "0", EmulationMachineTab.Video,
            [
                new("0", OptionValueResourceConstants.DisabledLabelResource),
                new("1", "64 px"),
                new("2", "128 px"),
                new("3", "192 px"),
                new("4", "256 px"),
                new("5", "320 px"),
                new("6", "384 px"),
                new("7", "448 px"),
                new("8", "512 px"),
                new("9", "576 px"),
                new("10", "640 px"),
            ]),
        Define(OptionConstants.CpuSpriteLevelKey, OptionConstants.CpuSpriteLevelLabel, null,
            "Sprites Only", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(CpuSpriteLevel.Sprites), "Emulation.Option.Sony.Value.SpritesOnly"),
                new(OptionValueFunctions.ToNative(CpuSpriteLevel.SpritesAndTriangles), "Emulation.Option.Sony.Value.SpritesTriangles"),
                new(OptionValueFunctions.ToNative(CpuSpriteLevel.BlendedSpritesAndTriangles), "Emulation.Option.Sony.Value.BlendedSpritesTriangles"),
            ]),
        Define(OptionConstants.SoftwareClutRenderKey, OptionConstants.SoftwareClutRenderLabel, null,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(ClutRendering.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(ClutRendering.Normal), "Emulation.Option.Sony.Value.Normal"),
                new(OptionValueFunctions.ToNative(ClutRendering.Aggressive), "Emulation.Option.Sony.Value.Aggressive"),
            ]),
        Define(OptionConstants.AutoFlushKey, OptionConstants.AutoFlushLabel, null,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(AutoFlushMode.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(AutoFlushMode.Sprites), "Emulation.Option.Sony.Value.SpritesOnly"),
                new(OptionValueFunctions.ToNative(AutoFlushMode.AllPrimitives), "Emulation.Option.Sony.Value.AllPrimitives"),
            ]),
        Define(OptionConstants.TextureInsideRtKey, OptionConstants.TextureInsideRtLabel, null,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(RenderTargetTextureMode.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(RenderTargetTextureMode.InsideTarget), "Emulation.Option.Sony.Value.InsideTarget"),
                new(OptionValueFunctions.ToNative(RenderTargetTextureMode.MergeTargets), "Emulation.Option.Sony.Value.MergeTargets"),
            ]),
        Define(OptionConstants.DisableDepthConversionKey, OptionConstants.DisableDepthConversionLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.FramebufferConversionKey, OptionConstants.FramebufferConversionLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.DisablePartialInvalidationKey, OptionConstants.DisablePartialInvalidationLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.GpuPaletteConversionKey, OptionConstants.GpuPaletteConversionLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.PreloadFrameDataKey, OptionConstants.PreloadFrameDataLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.HalfPixelOffsetKey, OptionConstants.HalfPixelOffsetLabel, null,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(HalfPixelOffset.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(HalfPixelOffset.NormalVertex), "Emulation.Option.Sony.Value.NormalVertex"),
                new(OptionValueFunctions.ToNative(HalfPixelOffset.SpecialTexture), "Emulation.Option.Sony.Value.SpecialTexture"),
                new(OptionValueFunctions.ToNative(HalfPixelOffset.AggressiveTexture), "Emulation.Option.Sony.Value.SpecialTextureAggressive"),
                new(OptionValueFunctions.ToNative(HalfPixelOffset.AlignToNative), "Emulation.Option.Sony.Value.AlignToNative"),
            ]),
        Define(OptionConstants.RoundSpriteKey, OptionConstants.RoundSpriteLabel, null,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(SpriteRounding.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(SpriteRounding.Half), "Emulation.Option.Sony.Value.Half"),
                new(OptionValueFunctions.ToNative(SpriteRounding.Full), "Emulation.Option.Sony.Value.Full"),
            ]),
        Define(OptionConstants.AlignSpriteKey, OptionConstants.AlignSpriteLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.MergeSpriteKey, OptionConstants.MergeSpriteLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.UnscaledPaletteDrawKey, OptionConstants.UnscaledPaletteDrawLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
        Define(OptionConstants.ForceSpritePositionKey, OptionConstants.ForceSpritePositionLabel, null,
            "disabled", EmulationMachineTab.Video,
            ToggleChoices),
    ];
}
