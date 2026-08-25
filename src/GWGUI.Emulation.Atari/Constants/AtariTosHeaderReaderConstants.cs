namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariTosHeaderReaderConstants
{
    internal const string EmuTOS = "EmuTOS";
    internal const string KAOS = "KAOS";
    internal const string KAOSTOS = "KAOS - TOS";
    internal const string Value09016Version090913 = @"[^0-9]{0,16}(?<version>[0-9]+(?:\.[0-9]+){1,3})";
    internal const string Version = "version";
    internal const string Value09Version009090209 = @"(?<![0-9])(?<version>0\.[0-9]+(?:\.[0-9]+){0,2})(?![0-9])";
}
