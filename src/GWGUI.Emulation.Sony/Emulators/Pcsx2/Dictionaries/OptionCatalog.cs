using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static class OptionCatalog
{
    // pcsx2_libretro.dll exposes these variables; LRPS2 is a different native core.
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.FastbootKey, OptionConstants.FastbootLabel, OptionConstants.FastbootHelp,
            null, "enabled", "enabled",
            [new("enabled", OptionValueResourceConstants.EnabledLabelResource),
             new("disabled", OptionValueResourceConstants.DisabledLabelResource)],
            Tab: EmulationMachineTab.Rom, RequiresRestart: true),
        new(OptionConstants.RendererKey, OptionConstants.RendererLabel, OptionConstants.RendererHelp,
            null, "Auto", "Auto",
            [new("Auto", "Emulation.Option.pcsx2.pcsx2_renderer.Value.0"),
             new("OpenGL", "Emulation.Option.pcsx2.pcsx2_renderer.Value.1"),
             new("D3D11", "Emulation.Option.pcsx2.pcsx2_renderer.Value.2"),
             new("D3D12", "Emulation.Option.pcsx2.pcsx2_renderer.Value.3"),
             new("Vulkan", "Emulation.Option.pcsx2.pcsx2_renderer.Value.4"),
             new("Software", OptionValueResourceConstants.SoftwareLabelResource),
             new("Null", OptionValueResourceConstants.DisabledLabelResource)],
            Tab: EmulationMachineTab.Video, RequiresRestart: true),
        new(OptionConstants.UpscaleMultiplierKey, OptionConstants.UpscaleMultiplierLabel,
            OptionConstants.UpscaleMultiplierHelp, null, "Native PS2", "Native PS2",
            [new("Native PS2", "1x"), new("2x Native ~720p", "2x"),
             new("3x Native ~1080p", "3x"), new("4x Native ~1440p 2K", "4x"),
             new("5x Native ~1620p 3K", "5x"), new("6x Native ~2160p 4K", "6x"),
             new("8x Native ~2880p 5K", "8x")], Tab: EmulationMachineTab.Video)
    ];
}
