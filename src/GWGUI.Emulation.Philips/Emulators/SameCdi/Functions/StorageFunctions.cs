using GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;
namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Functions;

internal static class StorageDescriptionFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
        [new(ConfigurationFunctions.DefaultSlots(configuration.Model).Single(), EmulationMediaType.CompactDisc,
            GWGUI.Emulation.Philips.Emulators.SameCdi.Constants.StorageConstants.AcceptedExtensions, RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.CompactDiscs, IsPermanent: true)];
}
