namespace GWGUI.Emulation.Atari;

internal static class AtariHatariContentErrors
{
    internal const string MultiplePrimaryContentUnsupported =
        "Hatari accepts one startup content path; use an M3U for floppy sets or one GEMDOS root for multiple partitions.";
    internal const string ContentTypeUnsupported = "This media type cannot be used as Hatari startup content.";
}
