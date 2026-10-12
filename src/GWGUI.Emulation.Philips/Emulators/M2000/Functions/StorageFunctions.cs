namespace GWGUI.Emulation.Philips.Emulators.M2000.Functions;

internal static class StorageDescriptionFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
        [new(ConfigurationFunctions.DefaultSlots(configuration.Model).Single(), EmulationMediaType.Cassette, StorageSettingsConstants.CassetteExtensions, RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cassettes, IsPermanent: true)];
}
