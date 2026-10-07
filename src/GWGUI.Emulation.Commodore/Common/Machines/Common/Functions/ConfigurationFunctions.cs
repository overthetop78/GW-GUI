using System.IO;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) =>
        EmulatorCatalog.CreateAdapter(configuration.Core).Summarize(configuration);

    internal static EmulationConfigurationSummary CreateBasic(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var details = new List<string>
        {
            string.Join(MachineSettingsConstants.ProcessorSeparator, model.CpuModels.Select(CpuDisplayFunctions.DisplayName)),
            string.Format(CultureInfo.InvariantCulture, MachineSettingsConstants.RamKibFormat, model.RamKib),
            model.Chipset
        };
        details.AddRange((configuration.FirmwarePaths ?? new Dictionary<string, string>()).Values
            .Select(Path.GetFileName).OfType<string>());
        return new EmulationConfigurationSummary(
            MachineCatalog.All.Single(machine => machine.Id == model.Id).DisplayResourceKey, details);
    }
}

internal static class ConfigurationValidationFunctions
{
    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        var adapter = EmulatorCatalog.CreateAdapter(configuration.Core);
        if (!adapter.Definition.MachineIds.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var allowedSlots = adapter.GetFirmwareSlots(configuration);
        if (configuration.FirmwarePaths?.Keys.Any(field => !allowedSlots.Any(slot => slot.FieldId == field)) == true)
            throw new ArgumentException(nameof(configuration.FirmwarePaths));
        foreach (var path in (configuration.FirmwarePaths ?? new Dictionary<string, string>()).Values)
            if (!File.Exists(path)) throw new FileNotFoundException(null, path);
        adapter.ValidateConfiguration(configuration);
        foreach (var media in configuration.Media ?? [])
            if (!File.Exists(media.Path) && !Directory.Exists(media.Path))
                throw new FileNotFoundException(null, media.Path);
    }
}
