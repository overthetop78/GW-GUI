using GWGUI.MediaEngine.Constants;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string CompactDiscDriveEnabledOption = "storage.compactDiscDriveEnabled";
    internal const string HardDiskEnabledOptionPrefix = "storage.hardDiskEnabled";
    internal const string CompactDiscDriveLabel = "CD";
    internal const string CdModelResource = "Emulation.Sony.Storage.CD";
    internal const int MemoryCardCount = 2;
    internal const int MemoryCardSize = 128 * 1024;
    internal const int Ps2MemoryCardSize = 8 * 1024 * 1024 / 512 * 528;
    internal const string Ps2MemoryCardExtension = ".ps2";
    internal const string MemoryStickModelResource = "Emulation.Sony.Storage.MemoryStick";
    internal static readonly string MemoryStickSubdirectory = System.IO.Path.Combine("Memory Stick", "Sony", "PSP");
    internal const string UmdModelResource = "Emulation.Sony.Storage.UMD";
    internal const string MemoryCardSlotPrefix = "MC";
    internal const string MemoryCardModel = "Memory Card";
    internal const string MemoryCardSlotResource = "Emulation.Sony.Storage.MemoryCardSlot";
    internal const string MemoryCardModelResource = "Emulation.Sony.Storage.MemoryCard";
    internal const string MemoryCardPathOptionPrefix = "storage.memoryCardPath";
    internal const string MemoryCardEnabledOptionPrefix = "storage.memoryCardEnabled";
    internal const string MemoryCardFilePrefix = "MemoryCard";
    internal const string MemoryCardExtension = ".mcr";
    internal const string AlternativeMemoryCardExtension = ".mcd";
    internal const string MemoryCardDeviceResource = "Emulation.Storage.MemoryCard.Device";
    internal const string ChooseResource = "Common.Choose";
    internal const string CreateResource = "Common.Create";
    internal const string AssociatedResource = "Emulation.Storage.Media.Associated";
    internal const string FileNameResource = "Read.FileName";
    internal const string FolderResource = "Emulation.Storage.File.DestinationFolder";
    internal const string DeviceNameResource = "Emulation.Device.Name";
    internal const string DeviceIdResource = "Emulation.Device.Name.Id";
    internal const string ModelResource = "Emulation.Machine.Model";
    internal const string UseCardResource = "Emulation.Storage.MemoryCard.Use";
    internal const string ExistingCardHintResource = "Emulation.Storage.MemoryCard.ExistingHint";
    internal const string CardRequiredResource = "Emulation.Storage.MemoryCard.ImageRequired";
    internal const string InvalidCardSizeResource = "Emulation.Storage.MemoryCard.InvalidSize";
    internal const string InvalidCardFormatResource = "Emulation.Storage.MemoryCard.InvalidFormat";

    internal static readonly IReadOnlyList<string> PlayStationExtensions =
        [DiskImageFileExtensions.Cue, DiskImageFileExtensions.Bin,
         DiskImageFileExtensions.Chd, DiskImageFileExtensions.Iso,
         DiskImageFileExtensions.Ccd, DiskImageFileExtensions.Mds];

    internal static readonly IReadOnlyList<string> PspExtensions = [DiskImageFileExtensions.Iso, DiskImageFileExtensions.Chd];
}
