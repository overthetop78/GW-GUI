namespace GWGUI.Emulation.Atari;

internal static class AtariHatariStorageErrors
{
    internal const string StorageMissing = "The configured Atari storage path does not exist.";
    internal const string StorageTypeInvalid = "The configured Atari storage type is not supported by Hatari.";
    internal const string StorageExtensionInvalid = "The Atari hard-disk image extension does not match its storage bus.";
    internal const string GemdosRequiresDirectory = "A GEMDOS drive must reference a host directory, not a .GEM file.";
    internal const string StorageNotSupportedByModel = "The selected Atari model does not support this storage bus.";
    internal const string MultiplePrimaryStorageUnsupported =
        "Hatari accepts only one primary hard-disk or GEMDOS content path at startup.";
    internal const string MountPointInvalid = "The GEMDOS mount point must be a letter from C to Z.";
    internal const string MarkerAlreadyExists = "The temporary GEMDOS marker path already exists.";
}
