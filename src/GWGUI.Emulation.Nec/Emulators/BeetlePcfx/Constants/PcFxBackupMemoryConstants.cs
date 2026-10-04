namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

internal static class PcFxBackupMemoryConstants
{
    internal const uint SaveRamType = 0;
    internal const int FirstByteOffset = 0;
    internal const string TemporaryIdFormat = "N";
    internal const string TemporaryNameSeparator = ".";
    internal const string TemporarySuffix = ".tmp";
    internal const int BankSize = 0x8000;
    internal const int BankCount = 2;
    internal const string InternalFileName = "InternalBackup.srm";
    internal const string MemoryDirectoryName = "Memory";
    internal const string ExternalFileName = "FX-BMP.srm";
    internal const string ExternalExtension = ".srm";
    internal const string ModelName = "FX-BMP";
    internal const string ModelResourceKey = "Emulation.Nec.Controller.PcFxBackupMemory";
    internal const string DeviceResourceKey = "Emulation.Storage.MemoryCard.Device";
    internal const string ImageResourceKey = "Emulation.Storage.MemoryCard.Device";
    internal const string UseExistingResourceKey = "Common.Choose";
    internal const string CreateResourceKey = "Common.Create";
    internal const string AssociatedResourceKey = "Emulation.Storage.Media.Associated";
    internal const string FileNameResourceKey = "Read.FileName";
    internal const string DestinationFolderResourceKey = "Emulation.Storage.File.DestinationFolder";
    internal const string DeviceNameResourceKey = "Emulation.Device.Name";
    internal const string DeviceIdentifierResourceKey = "Emulation.Device.Name.Id";
    internal const string ModelFieldResourceKey = "Emulation.Machine.Model";
    internal const string UseActionResourceKey = "Emulation.Storage.MemoryCard.Use";
    internal const string ExistingHintResourceKey = "Emulation.Storage.MemoryCard.ExistingHint";
    internal const string ImageRequiredResourceKey = "Emulation.Storage.MemoryCard.ImageRequired";
    internal const string InvalidSizeResourceKey = "Emulation.Storage.MemoryCard.InvalidSize";
    internal const string InvalidFormatResourceKey = "Emulation.Storage.MemoryCard.InvalidFormat";
    internal const string EnabledOption = "storage.fxBmpEnabled";
    internal const string PathOption = "storage.fxBmpPath";
    internal const byte EmptyFileByte = 0;
}
