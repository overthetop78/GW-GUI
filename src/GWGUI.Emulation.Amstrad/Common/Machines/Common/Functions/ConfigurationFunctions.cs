using System.IO;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var details = new List<string> { $"Z80A · {model.RamKib} KiB RAM" };
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
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException(nameof(configuration.SchemaVersion));
        foreach (var media in configuration.Media ?? [])
        {
            if (!File.Exists(media.Path)) throw new FileNotFoundException(null, media.Path);
            if (!Supports(model, media.Category))
                throw new InvalidDataException($"{model.Id}:{media.Category}");
        }
    }

    internal static bool Supports(Model model, MediaCategory category) => category switch
    {
        MediaCategory.Floppy => model.MaximumFloppyDriveCount > 0,
        MediaCategory.Cassette => model.SupportsCassetteDrive,
        MediaCategory.Cartridge => model.SupportsCartridgeSlot,
        MediaCategory.Snapshot => true,
        _ => false
    };
}
