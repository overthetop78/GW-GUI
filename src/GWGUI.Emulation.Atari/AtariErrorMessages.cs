namespace GWGUI.Emulation.Atari;

internal static class AtariErrorMessages
{
    internal const string UnsupportedSchema = "The Atari configuration schema is not supported.";
    internal const string EmptyFirmwarePath = "An Atari firmware path cannot be empty.";
    internal const string DuplicateFirmware = "An Atari firmware role cannot be configured more than once.";
    internal const string IncompatibleFirmware = "The Atari firmware is not compatible with the selected machine.";
    internal const string EmptyMediaPath = "An Atari media path cannot be empty.";
    internal const string DuplicateMediaSlot = "An Atari media slot cannot be configured more than once.";
    internal const string IncompatibleMedia = "The Atari media is not compatible with the selected machine.";
    internal const string InvalidControllerPort = "The Atari controller port is outside the supported range.";
    internal const string DuplicateControllerPort = "An Atari controller port cannot be configured more than once.";
}
