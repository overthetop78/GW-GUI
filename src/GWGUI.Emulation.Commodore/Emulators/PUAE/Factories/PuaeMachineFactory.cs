using GWGUI.Emulation.Commodore.Emulators.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Factories;

internal sealed class PuaeMachineFactory : UaeMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.PUAE,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorDescriptionConstants.AmigaDescriptionResourceKey,
            MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl,
        new(EmulatorConstants.ValidatedReleaseId, EmulatorConstants.ValidatedReleaseDisplayName,
            new(EmulatorConstants.DownloadUrl), new(2026, 7, 31, 1, 0, 0, TimeSpan.Zero), true, true));
}
