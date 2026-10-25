namespace GWGUI.Emulation.Contracts;

public sealed record EmulationStorageDialogPresentation(
    string DeviceResourceKey,
    string ImageResourceKey,
    string UseExistingResourceKey,
    string CreateResourceKey,
    string AssociatedResourceKey,
    string FileNameResourceKey,
    string DestinationFolderResourceKey,
    string DeviceNameResourceKey,
    string DeviceIdentifierResourceKey,
    string ModelFieldResourceKey,
    string UseActionResourceKey,
    string ExistingHintResourceKey,
    string ImageRequiredResourceKey,
    string InvalidSizeResourceKey,
    string InvalidFormatResourceKey);
