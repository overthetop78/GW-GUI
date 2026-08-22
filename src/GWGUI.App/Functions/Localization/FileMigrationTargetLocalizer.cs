using GWGUI.App.Localization.Extensions;
using System.IO;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.App.Functions.Localization;

public static class FileMigrationTargetLocalizer
{
    public static string GetDisplayName(string formatId) => LocExtension.Get(formatId switch
    {
        DiskImageFormatIds.AmigaDos => "Migration.Target.Amiga880",
        DiskImageFormatIds.AtariSt720 => "Migration.Target.Atari720",
        DiskImageFormatIds.Ibm720 => "Migration.Target.Ibm720",
        DiskImageFormatIds.Msx2Dd => "Migration.Target.Msx720",
        DiskImageFormatIds.AppleIIAppleDos140 => "Migration.Target.AppleDos140",
        DiskImageFormatIds.AppleIIProDos140 => "Migration.Target.ProDos140",
        DiskImageFormatIds.AppleIIProDos800 => "Migration.Target.ProDos800",
        DiskImageFormatIds.AppleIIISos => "Migration.Target.Sos800",
        DiskImageFormatIds.Commodore1541 => "Migration.Target.Commodore1541",
        DiskImageFormatIds.Commodore1571 => "Migration.Target.Commodore1571",
        DiskImageFormatIds.Commodore1581 => "Migration.Target.Commodore1581",
        _ => throw new InvalidDataException($"The migration target '{formatId}' has no localized display name.")
    });
}
