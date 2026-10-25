namespace GWGUI.Emulation.Philips.Emulators.O2em.Functions;

internal static class StorageDescriptionFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
        [new(ConfigurationFunctions.DefaultSlots(configuration.Model).Single(), EmulationMediaType.Cartridge, StorageSettingsConstants.CartridgeExtensions, RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true)];
}
