using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        const string startup = "Emulation.Thomson.Start", printer = "Emulation.Thomson.Printer";
        return[
            new(
            startup,
            EmulationMachineTab.General,
            startup,
            [
            Toggle(configuration, RuntimeOptionConstants.AutoRun, startup, "Emulation.Thomson.Start.AutoRun", false),
            Toggle(configuration, RuntimeOptionConstants.UseGameHash, startup, "Emulation.Thomson.Start.UseGameHash", true),
            Toggle(configuration, RuntimeOptionConstants.ShowStartHint, startup, "Emulation.Thomson.Start.ShowHint", true)
        ],
            IsCompact: true
        ),
            new(printer, EmulationMachineTab.General, printer, [Toggle(configuration, RuntimeOptionConstants.PrinterEmulation, printer, "Emulation.Thomson.Printer.SaveOutput", false)], IsCompact: true)
        ];
    }
    private static EmulationSettingsField Toggle(MachineConfiguration configuration, string key, string block, string label, bool enabled) => new(
        key,
        EmulationMachineTab.General,
        block,
        label,
        EmulationSettingsEditor.Toggle,
        configuration.Options.GetValueOrDefault(key, enabled ? RuntimeOptionConstants.Enabled : RuntimeOptionConstants.Disabled),
        [new(RuntimeOptionConstants.Enabled, "Emulation.Value.Enabled"), new(RuntimeOptionConstants.Disabled, "Emulation.Value.Disabled")],
        EnabledValue: RuntimeOptionConstants.Enabled,
        DisabledValue: RuntimeOptionConstants.Disabled
    );
}
