namespace GWGUI.Emulation.Atari.Common.Constants;

public static class MediaConstants
{
    public const int DefaultMountOrder = 0;
}

internal static class CartridgeConstants
{
    internal const string StellaRegionOptionKey = "stella_console";
    internal const string JaguarRegionOptionKey = "virtualjaguar_pal";
    internal const string AutomaticRegionValue = "auto";
    internal const string NtscRegionValue = "ntsc";
    internal const string PalRegionValue = "pal";
    internal const string SecamRegionValue = "secam";
    internal const string EnabledValue = "enabled";
    internal const string DisabledValue = "disabled";

    internal static readonly IReadOnlySet<Emulator> CartridgeCores =
        new HashSet<Emulator>
        {
            Emulator.Stella,
            Emulator.ProSystem,
            Emulator.BeetleLynx,
            Emulator.VirtualJaguar
        };

    internal static readonly IReadOnlyDictionary<Emulator, IReadOnlySet<string>> Extensions =
        new Dictionary<Emulator, IReadOnlySet<string>>
        {
            [Emulator.Stella] = Values("a26", "bin"),
            [Emulator.ProSystem] = Values("a78", "bin", "cdf"),
            [Emulator.BeetleLynx] = Values("lnx", "lyx", "bll", "o"),
            [Emulator.VirtualJaguar] = Values("j64", "jag", "rom", "abs", "cof", "bin", "prg")
        };

    private static IReadOnlySet<string> Values(params string[] extensions) =>
        new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
}

internal static class CartridgeErrors
{
    internal const string UnsupportedCore = "The selected Atari core does not use the shared cartridge controller.";
    internal const string CartridgeRequired = "The selected Atari machine requires cartridge media.";
    internal const string ExtensionUnsupported = "The cartridge extension is not supported by the selected Atari core.";
    internal const string FileUnreadable = "The Atari cartridge cannot be opened for reading.";
    internal const string ReplacementFailed = "The Atari core rejected the replacement cartridge.";
    internal const string RollbackFailed = "The Atari core rejected both the replacement and the previous cartridge.";
    internal const string EjectionUnsupported = "This Atari core cannot remain powered on without a cartridge.";
    internal const string RegionUnsupported = "The selected Atari core does not expose cartridge region selection.";
    internal const string SecamUnsupported = "The selected Atari core does not expose SECAM cartridge timing.";
}

internal static class CompatibilityConstants
{
    internal const int NoControllerPort = 0;
    internal const int OneControllerPort = 1;
    internal const int TwoControllerPorts = 2;
    internal const int FourControllerPorts = 4;
    internal const int EmptyCollectionCount = 0;
    internal const int SingleChoiceCount = 1;

    internal const string ForcedByModelResource = "Emulation.Atari.Unavailable.ForcedByModel";
    internal const string NoFpuResource = "Emulation.Atari.Unavailable.NoFpu";
    internal const string NoAlternateMemoryResource = "Emulation.Atari.Unavailable.NoAlternateMemory";
    internal const string NoFirmwareResource = "Emulation.Atari.Unavailable.NoFirmware";
    internal const string NoKeyboardResource = "Emulation.Atari.Unavailable.NoKeyboard";
    internal const string NoMouseResource = "Emulation.Atari.Unavailable.NoMouse";
    internal const string NoStorageResource = "Emulation.Atari.Unavailable.NoStorage";
    internal const string JaguarStandardNoCdResource = "Emulation.Atari.Unavailable.JaguarStandardNoCd";

    internal const string ForcedValueSeparator = ",";
    internal const string CoreManagedValue = "core-managed";
}

internal static class ContentConstants
{
    internal const string ExtensionSeparator = ", ";
}

internal static class DiskControlConstants
{
    internal const int InterfaceVersion = 1;
    internal const int TextBufferSize = 4096;
    internal const int NoImageIndex = -1;
    internal const int FirstImageIndex = 0;
    internal const uint FirstNativeImageIndex = 0;
    internal const uint NoNativeImageIndex = uint.MaxValue;
    internal const uint PreviousImageOffset = 1;
}

internal static class DiskControlErrors
{
    internal const string Unavailable = "The Atari core has not provided disk control.";
    internal const string Incomplete = "The Atari core provided incomplete disk control.";
    internal const string EjectFailed = "The Atari media drive could not be ejected.";
    internal const string SelectFailed = "The Atari core could not select the requested disk.";
    internal const string InsertFailed = "The Atari media drive could not insert the requested image.";
    internal const string CreateSlotFailed = "The Atari core could not create a media slot.";
    internal const string ReplaceFailed = "The Atari core refused the selected media image.";
    internal const string MediaMissing = "The Atari media image was not found.";
}

internal static class ScpMediaFunctionsConstants
{
    internal const string AnAtariSCPImageMustBeMountedAsFloppyMedia = "An Atari SCP image must be mounted as floppy media.";
}

internal static class SessionMediaConstants
{
    internal const string SessionDirectoryName = "Floppies";
    internal const string PlaylistExtension = ".m3u";
    internal const string PlaylistCommentPrefix = "#";
    internal const string RuntimeFileNameFormat = "{0:D3}-{1}";
    internal const string RuntimePlaylistFileName = "session.m3u";
    internal const string SessionInstanceNameFormat = "{0}-{1}";
    internal const string UniqueNameFormat = "N";
    internal const int FirstMediaIndex = 0;
    internal const int RuntimeFileNumberOffset = 1;
}

internal static class SessionMediaErrors
{
    internal const string PlaylistEntryMissing = "A multidisk playlist references a missing image.";
    internal const string PlaylistEmpty = "The multidisk playlist contains no disk image.";
    internal const string ExplicitSaveRequired = "Session media changes require an explicit save operation.";
}
