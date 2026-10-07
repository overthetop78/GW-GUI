using GWGUI.Emulation.Commodore.Emulators.ViceX128.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Factories;

namespace GWGUI.Emulation.Commodore.Emulators.ViceX128.Factories;

internal sealed class MachineFactory : ViceMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.ViceX128,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, ContentConstants.DescriptionResourceKey,
            EmulatorConstants.Models.Keys.ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    internal override string ModelOption => EmulatorConstants.ModelOption;
    internal override string ResourceSection => EmulatorConstants.ResourceSection;
    internal override IReadOnlyDictionary<string, (string Option, string Command)> Models => EmulatorConstants.Models;
    internal override IReadOnlyList<string> CartridgeExtensions => [];
    internal override IReadOnlyList<(FirmwareSlot Slot, string Resource)> RomDefinitions(MachineConfiguration configuration) =>
    [
        (new(ContentConstants.KernalInternationalField, ContentConstants.KernalInternationalResourceKey), ContentConstants.KernalInternationalNativeResource),
        (new(ContentConstants.BasicLowField, ContentConstants.BasicLowResourceKey), ContentConstants.BasicLowNativeResource),
        (new(ContentConstants.BasicHighField, ContentConstants.BasicHighResourceKey), ContentConstants.BasicHighNativeResource),
        (new(ContentConstants.CharactersInternationalField, ContentConstants.CharactersInternationalResourceKey), ContentConstants.CharactersInternationalNativeResource),
        (new(ContentConstants.C64KernalField, ContentConstants.C64KernalResourceKey), ContentConstants.C64KernalNativeResource),
        (new(ContentConstants.C64BasicField, ContentConstants.C64BasicResourceKey), ContentConstants.C64BasicNativeResource)
    ];
}
