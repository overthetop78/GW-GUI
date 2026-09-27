using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Exceptions;

internal static class HatariStorageErrors
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
