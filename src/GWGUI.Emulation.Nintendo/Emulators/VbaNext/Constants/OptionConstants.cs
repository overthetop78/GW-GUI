using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.VbaNext.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("vbanext_bios", "Emulation.Option.vba_next.vbanext_bios", "Emulation.Option.vba_next.vbanext_bios.Help", "", "enabled", "enabled", [new("enabled", "Emulation.Option.vba_next.vbanext_bios.Value.enabled"), new("disabled", "Emulation.Option.vba_next.vbanext_bios.Value.disabled")], Tab: EmulationMachineTab.Rom, RequiresRestart: true),
        new("vbanext_rtc", "Emulation.Option.vba_next.vbanext_rtc", "Emulation.Option.vba_next.vbanext_rtc.Help", "", "auto", "auto", [new("auto", "Emulation.Option.vba_next.vbanext_rtc.Value.auto"), new("enabled", "Emulation.Option.vba_next.vbanext_rtc.Value.enabled"), new("disabled", "Emulation.Option.vba_next.vbanext_rtc.Value.disabled")], Tab: EmulationMachineTab.General, RequiresRestart: true),
        new("vbanext_turboenable", "Emulation.Option.vba_next.vbanext_turboenable", "Emulation.Option.vba_next.vbanext_turboenable.Help", "", "disabled", "disabled", [new("disabled", "Emulation.Option.vba_next.vbanext_turboenable.Value.disabled"), new("enabled", "Emulation.Option.vba_next.vbanext_turboenable.Value.enabled")], Tab: EmulationMachineTab.Controllers),
        new("vbanext_turbodelay", "Emulation.Option.vba_next.vbanext_turbodelay", "Emulation.Option.vba_next.vbanext_turbodelay.Help", "", "2", "2", [new("1", "1"), new("2", "2"), new("3", "3"), new("4", "4"), new("5", "5"), new("6", "6"), new("7", "7"), new("8", "8"), new("9", "9"), new("10", "10"), new("11", "11"), new("12", "12"), new("13", "13"), new("14", "14"), new("15", "15")], Tab: EmulationMachineTab.Controllers)
    ];
}
