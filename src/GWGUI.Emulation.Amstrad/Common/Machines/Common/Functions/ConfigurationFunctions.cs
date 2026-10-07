using CrocoDSProfile = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.EmulatorConstants;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var details = new List<string> { $"{ConfigurationSummaryFunctionsConstants.ProcessorSummaryPrefix}{model.RamKib}{ConfigurationSummaryFunctionsConstants.RamSummarySuffix}" };
        details.AddRange((configuration.Media ?? []).OrderBy(media => media.MountOrder)
            .Select(media => Path.GetFileName(media.Path)));
        return new EmulationConfigurationSummary(
            MachineConfigurationConstants.ResourcePrefix + model.Id, details);
    }
}

internal static class ConfigurationValidationFunctions
{
    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        if (!EmulatorCatalog.GetAll(model.Id).Any(item => item.Id == configuration.EmulatorId))
            throw new InvalidDataException(nameof(configuration.EmulatorId));
        if (configuration.EmulatorId == CrocoDSProfile.Id)
            Emulators.CrocoDS.Factories.CrocoDSMachineFactory.Validate(configuration);
        if (string.IsNullOrWhiteSpace(configuration.EmulatorId))
            throw new InvalidDataException(nameof(configuration.EmulatorId));
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException(nameof(configuration.SchemaVersion));
        foreach (var media in configuration.Media ?? [])
        {
            if (!File.Exists(media.Path)) throw new FileNotFoundException(null, media.Path);
            if (!Supports(model, media.Category))
                throw new InvalidDataException($"{model.Id}{ConfigurationSummaryFunctionsConstants.MediaCategorySeparator}{media.Category}");
        }
    }

    internal static bool Supports(Model model, MediaCategory category) => category switch
    {
        MediaCategory.Floppy => model.MaximumFloppyDriveCount > BufferConstants.EmptyCollectionCount,
        MediaCategory.Cassette => model.SupportsCassetteDrive,
        MediaCategory.Cartridge => model.SupportsCartridgeSlot,
        MediaCategory.Snapshot => true,
        _ => false
    };
}
