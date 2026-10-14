using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static class OptionCatalog
{
    // The official pcsx2_libretro.dll now identifies itself as LRPS2.
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
             new("paraLLEl-GS", "paraLLEl-GS"),
             new("Software (HW)", "Emulation.Option.Sony.Value.SoftwareHardwarePresentation"),
             new("Software (SW)", OptionValueResourceConstants.SoftwareLabelResource)],
            Tab: EmulationMachineTab.Video, RequiresRestart: true),
        new(OptionConstants.UpscaleMultiplierKey, OptionConstants.UpscaleMultiplierLabel,
            OptionConstants.UpscaleMultiplierHelp, null, "1x (Native)", "1x (Native)",
            [new("1x (Native)", "1×"), new("2x", "2×"), new("4x", "4×"), new("8x", "8×")],
            Tab: EmulationMachineTab.Video, RequiresRestart: true)
    ];

    internal static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LegacyValues { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [OptionConstants.RendererKey] = new Dictionary<string, string>(StringComparer.Ordinal) { ["Software"] = "Software (HW)" },
            [OptionConstants.UpscaleMultiplierKey] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Native PS2"] = "1x (Native)",
                ["2x Native ~720p"] = "2x",
                ["4x Native ~1440p 2K"] = "4x",
                ["8x Native ~2880p 5K"] = "8x"
            }
        };
}
