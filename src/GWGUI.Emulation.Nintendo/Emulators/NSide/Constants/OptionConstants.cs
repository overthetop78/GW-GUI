using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.NSide.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("higan_sfc_internal_resolution", "Emulation.Option.nside_sfc_balanced.higan_sfc_internal_resolution", null, "", "512x480", "512x480", [new("512x480", "512x480"), new("512x448", "512x448"), new("512x240", "512x240"), new("512x224", "512x224"), new("256x240", "256x240"), new("256x224", "256x224")], Tab: EmulationMachineTab.Video),
        new("higan_sfc_color_emulation", "Emulation.Option.nside_sfc_balanced.higan_sfc_color_emulation", null, "", "OFF", "OFF", [new("OFF", "Emulation.Option.nside_sfc_balanced.higan_sfc_color_emulation.Value.OFF"), new("ON", "ON")], Tab: EmulationMachineTab.Video),
        new("higan_sfc_blur_emulation", "Emulation.Option.nside_sfc_balanced.higan_sfc_blur_emulation", null, "", "OFF", "OFF", [new("OFF", "Emulation.Option.nside_sfc_balanced.higan_sfc_blur_emulation.Value.OFF"), new("ON", "ON")], Tab: EmulationMachineTab.Video),
        new("higan_sfc_scanline_emulation", "Emulation.Option.nside_sfc_balanced.higan_sfc_scanline_emulation", null, "", "OFF", "OFF", [new("OFF", "Emulation.Option.nside_sfc_balanced.higan_sfc_scanline_emulation.Value.OFF"), new("ON", "ON")], Tab: EmulationMachineTab.Video),
        new("higan_sfc_sgb_bios", "Emulation.Option.nside_sfc_balanced.higan_sfc_sgb_bios", null, "", "SGB1.sfc/", "SGB1.sfc/", [new("SGB1.sfc/", "Emulation.Option.nside_sfc_balanced.higan_sfc_sgb_bios.Value.SGB1.sfc/"), new("SGB2.sfc/", "Emulation.Option.nside_sfc_balanced.higan_sfc_sgb_bios.Value.SGB2.sfc/")], Tab: EmulationMachineTab.Rom, RequiresRestart: true)
    ];
}
