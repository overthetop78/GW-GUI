using GWGUI.Emulation.Commodore.Emulators.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Factories;

internal sealed class PuaeMachineFactory : UaeMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.PUAE,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorDescriptionConstants.AmigaDescriptionResourceKey,
            SupportedMachines));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl,
        new(EmulatorConstants.ValidatedReleaseId, EmulatorConstants.ValidatedReleaseDisplayName,
            new(EmulatorConstants.DownloadUrl), EmulatorConstants.ReleasePublishedAt, true, true));
}
