namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariDiskControlErrors
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
