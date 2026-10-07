using GWGUI.Emulation.Commodore.Emulators.ViceX64Sc.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Factories;

namespace GWGUI.Emulation.Commodore.Emulators.ViceX64Sc.Factories;

internal sealed class MachineFactory : ViceMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.ViceX64Sc,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, ContentConstants.DescriptionResourceKey,
            EmulatorConstants.Models.Keys.ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    internal override string ModelOption => EmulatorConstants.ModelOption;
    internal override string ResourceSection => EmulatorConstants.ResourceSection;
    internal override IReadOnlyDictionary<string, (string Option, string Command)> Models => EmulatorConstants.Models;
    internal override IReadOnlyList<string> CartridgeExtensions => ContentConstants.CartridgeExtensions;
    internal override IReadOnlyList<(FirmwareSlot Slot, string Resource)> RomDefinitions(MachineConfiguration configuration) =>
    [
        (new(ContentConstants.KernalField, ContentConstants.KernalResourceKey), ContentConstants.KernalNativeResource),
        (new(ContentConstants.BasicField, ContentConstants.BasicResourceKey), ContentConstants.BasicNativeResource),
        (new(ContentConstants.CharactersField, ContentConstants.CharactersResourceKey), ContentConstants.CharactersNativeResource)
    ];
}
