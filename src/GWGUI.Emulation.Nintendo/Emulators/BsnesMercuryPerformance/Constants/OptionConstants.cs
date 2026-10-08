using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesMercuryPerformance.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("bsnes_violate_accuracy", "Emulation.Option.bsnes_mercury_performance.bsnes_violate_accuracy", null, "", "disabled", "disabled", [new("disabled", "Emulation.Option.bsnes_mercury_performance.bsnes_violate_accuracy.Value.disabled"), new("enabled", "Emulation.Option.bsnes_mercury_performance.bsnes_violate_accuracy.Value.enabled")], Tab: EmulationMachineTab.General),
        new("bsnes_chip_hle", "Emulation.Option.bsnes_mercury_performance.bsnes_chip_hle", null, "", "LLE", "LLE", [new("LLE", "Emulation.Option.bsnes_mercury_performance.bsnes_chip_hle.Value.LLE"), new("HLE", "Emulation.Option.bsnes_mercury_performance.bsnes_chip_hle.Value.HLE")], Tab: EmulationMachineTab.Rom),
        new("bsnes_superfx_overclock", "Emulation.Option.bsnes_mercury_performance.bsnes_superfx_overclock", null, "", "100%", "100%", [new("100%", "100%"), new("150%", "150%"), new("200%", "200%"), new("300%", "300%"), new("400%", "400%"), new("500%", "500%"), new("1000%", "1000%")], Tab: EmulationMachineTab.Cpu),
        new("bsnes_region", "Emulation.Option.bsnes_mercury_performance.bsnes_region", null, "", "auto", "auto", [new("auto", "Emulation.Option.bsnes_mercury_performance.bsnes_region.Value.auto"), new("ntsc", "Emulation.Option.bsnes_mercury_performance.bsnes_region.Value.ntsc"), new("pal", "Emulation.Option.bsnes_mercury_performance.bsnes_region.Value.pal")], Tab: EmulationMachineTab.General),
        new("bsnes_aspect_ratio", "Emulation.Option.bsnes_mercury_performance.bsnes_aspect_ratio", null, "", "auto", "auto", [new("auto", "Emulation.Option.bsnes_mercury_performance.bsnes_aspect_ratio.Value.auto"), new("ntsc", "Emulation.Option.bsnes_mercury_performance.bsnes_aspect_ratio.Value.ntsc"), new("pal", "Emulation.Option.bsnes_mercury_performance.bsnes_aspect_ratio.Value.pal")], Tab: EmulationMachineTab.Video),
        new("bsnes_crop_overscan", "Emulation.Option.bsnes_mercury_performance.bsnes_crop_overscan", null, "", "disabled", "disabled", [new("disabled", "Emulation.Option.bsnes_mercury_performance.bsnes_crop_overscan.Value.disabled"), new("enabled", "Emulation.Option.bsnes_mercury_performance.bsnes_crop_overscan.Value.enabled")], Tab: EmulationMachineTab.Video),
        new("bsnes_gamma_ramp", "Emulation.Option.bsnes_mercury_performance.bsnes_gamma_ramp", null, "", "disabled", "disabled", [new("disabled", "Emulation.Option.bsnes_mercury_performance.bsnes_gamma_ramp.Value.disabled"), new("enabled", "Emulation.Option.bsnes_mercury_performance.bsnes_gamma_ramp.Value.enabled")], Tab: EmulationMachineTab.Video, RequiresRestart: true)
    ];
}
