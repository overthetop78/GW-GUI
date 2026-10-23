using GWGUI.Emulation.Commodore.Emulators.ViceXPet.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Factories;
using PetModels = GWGUI.Emulation.Commodore.Common.Machines.Pet.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXPet.Factories;

internal sealed class MachineFactory : ViceMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.ViceXPet,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, ContentConstants.DescriptionResourceKey, EmulatorConstants.Models.Keys.ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    internal override string ModelOption => EmulatorConstants.ModelOption;
    internal override string ResourceSection => EmulatorConstants.ResourceSection;
    internal override IReadOnlyDictionary<string, (string Option, string Command)> Models => EmulatorConstants.Models;
    internal override IReadOnlyList<string> CartridgeExtensions => [];
    internal override IReadOnlyList<(FirmwareSlot Slot, string Resource)> RomDefinitions(MachineConfiguration configuration) =>
        configuration.Model == PetModels.SuperPet
            ? [
        (new(ContentConstants.KernalField, string.Empty, ContentConstants.KernalResourceKey), ContentConstants.KernalNativeResource),
        (new(ContentConstants.BasicField, string.Empty, ContentConstants.BasicResourceKey), ContentConstants.BasicNativeResource),
        (new(ContentConstants.EditorField, string.Empty, ContentConstants.EditorResourceKey), ContentConstants.EditorNativeResource),
        (new(ContentConstants.CharactersField, string.Empty, ContentConstants.CharactersResourceKey), ContentConstants.CharactersNativeResource),
        (new(ContentConstants.SuperPetAField, string.Empty, ContentConstants.SuperPetAResourceKey), ContentConstants.SuperPetANativeResource),
        (new(ContentConstants.SuperPetBField, string.Empty, ContentConstants.SuperPetBResourceKey), ContentConstants.SuperPetBNativeResource),
        (new(ContentConstants.SuperPetCField, string.Empty, ContentConstants.SuperPetCResourceKey), ContentConstants.SuperPetCNativeResource),
        (new(ContentConstants.SuperPetDField, string.Empty, ContentConstants.SuperPetDResourceKey), ContentConstants.SuperPetDNativeResource),
        (new(ContentConstants.SuperPetEField, string.Empty, ContentConstants.SuperPetEResourceKey), ContentConstants.SuperPetENativeResource),
        (new(ContentConstants.SuperPetFField, string.Empty, ContentConstants.SuperPetFResourceKey), ContentConstants.SuperPetFNativeResource)
              ]
            : [
        (new(ContentConstants.KernalField, string.Empty, ContentConstants.KernalResourceKey), ContentConstants.KernalNativeResource),
        (new(ContentConstants.BasicField, string.Empty, ContentConstants.BasicResourceKey), ContentConstants.BasicNativeResource),
        (new(ContentConstants.EditorField, string.Empty, ContentConstants.EditorResourceKey), ContentConstants.EditorNativeResource),
        (new(ContentConstants.CharactersField, string.Empty, ContentConstants.CharactersResourceKey), ContentConstants.CharactersNativeResource)
              ];
}
