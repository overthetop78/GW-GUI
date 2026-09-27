using GWGUI.Emulation.Atari.Emulators.Atari800.Constants;
using GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;
using GWGUI.Emulation.Atari.Emulators.Atari800.Enums;
using GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

namespace GWGUI.Emulation.Atari.Emulators.Atari800.Exceptions;

internal static class Atari800MediaErrors
{
    internal static string UnsupportedMediaCategory => ErrorMessages.ContentExtensionUnsupported;
    internal static string InvalidExtension => ErrorMessages.ContentExtensionUnsupported;
    internal static string ComputerMediaOn5200 => ErrorMessages.IncompatibleMedia;
    internal static string ConsoleMediaOnComputer => ErrorMessages.IncompatibleMedia;
    internal static string CartridgeTypeInvalid => ErrorMessages.OptionValueInvalidFormat;
    internal static string DynamicCartridgeUnsupported => ErrorMessages.DynamicMediaUnsupported;
    internal static string MediaControlRequired => ErrorMessages.DynamicMediaUnsupported;
}
