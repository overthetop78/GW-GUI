using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Constants;

internal static class HatariContentErrors
{
    internal const string MultiplePrimaryContentUnsupported =
        "Hatari accepts one startup content path; use an M3U for floppy sets or one GEMDOS root for multiple partitions.";
    internal const string ContentTypeUnsupported = "This media type cannot be used as Hatari startup content.";
}
