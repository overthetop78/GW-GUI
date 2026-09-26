using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Exceptions;

internal static class HatariContentErrors
{
    internal static string MultiplePrimaryContentUnsupported => ErrorMessages.ContentExtensionUnsupported;
    internal static string ContentTypeUnsupported => ErrorMessages.ContentExtensionUnsupported;
}
