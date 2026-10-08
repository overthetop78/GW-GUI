namespace GWGUI.Emulation.Sega.Emulators.VeMUlator.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("enable_flash_write", "Emulation.Option.vemulator.enable_flash_write", null, null, "enabled", "enabled", [new("enabled", "Emulation.Option.vemulator.enable_flash_write.Value.enabled"), new("disabled", "Emulation.Option.vemulator.enable_flash_write.Value.disabled")], Tab: EmulationMachineTab.Storage, RequiresRestart: true),
    ];
}
