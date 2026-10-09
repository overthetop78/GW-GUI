namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        return new EmulationConfigurationSummary(hardware.DisplayNameResourceKey,
        [
            $"CPU {hardware.DefaultCpu}",
            $"RAM {hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB",
            $"Core {configuration.Core}"
        ]);
    }
}
