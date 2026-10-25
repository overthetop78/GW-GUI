
namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Exceptions;

internal static class StStorageErrors
{
    internal static string StorageMissing => ErrorMessages.ContentFileMissing;
    internal static string StorageTypeInvalid => ErrorMessages.StorageExtensionInvalid;
    internal static string StorageExtensionInvalid => ErrorMessages.StorageExtensionInvalid;
    internal static string GemdosRequiresDirectory => ErrorMessages.StorageExtensionInvalid;
    internal static string StorageNotSupportedByModel => ErrorMessages.IncompatibleMedia;
    internal static string MultiplePrimaryStorageUnsupported => ErrorMessages.IncompatibleMedia;
    internal static string MountPointInvalid => ErrorMessages.OptionValueInvalidFormat;
    internal static string MarkerAlreadyExists => ErrorMessages.ContentLoadFailed;
}
