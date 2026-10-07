using GWGUI.Emulation.Commodore.Emulators.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.PUAE2021.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE2021.Factories;

internal sealed class PuaeMachineFactory : UaeMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.PUAE2021,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorDescriptionConstants.AmigaDescriptionResourceKey,
            MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
}
