namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var memoryKib = new Engine().SelectedAdapter(configuration) is { } adapter ? adapter.GetMemoryKib(configuration) : hardware.MainMemoryBytes / (decimal)SettingsConstants.Kibibyte;
        return new EmulationConfigurationSummary(hardware.DisplayNameResourceKey, [$"CPU {hardware.DefaultCpu}", $"RAM {memoryKib} KiB", $"Core {configuration.Core}"]);
    }
}
