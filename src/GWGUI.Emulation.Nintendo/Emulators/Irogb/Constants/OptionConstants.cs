using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Irogb.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("irogb_bios", "Emulation.Option.irogb.irogb_bios", null, "", "auto", "auto", [new("auto", "Emulation.Option.irogb.irogb_bios.Value.auto"), new("dmg", "Emulation.Option.irogb.irogb_bios.Value.dmg"), new("cgb", "Emulation.Option.irogb.irogb_bios.Value.cgb"), new("none", "Emulation.Option.irogb.irogb_bios.Value.none")], Tab: EmulationMachineTab.Rom),
        new("irogb_monochrome_dmg", "Emulation.Option.irogb.irogb_monochrome_dmg", null, "", "disabled", "disabled", [new("enabled", "Emulation.Option.irogb.irogb_monochrome_dmg.Value.enabled"), new("disabled", "Emulation.Option.irogb.irogb_monochrome_dmg.Value.disabled")], Tab: EmulationMachineTab.Video)
    ];
}
