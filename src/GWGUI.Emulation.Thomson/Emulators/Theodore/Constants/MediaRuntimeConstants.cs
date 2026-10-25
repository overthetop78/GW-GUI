using System.Collections.Frozen;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;

internal static class MediaRuntimeConstants
{
    internal const string Floppy = ".fd";
    internal const string SapFloppy = ".sap";
    internal const string Tape = ".k7";
    internal const string MoCartridge = ".m5";
    internal const string ToCartridge = ".m7";
    internal const string RomCartridge = ".rom";
    internal static readonly IReadOnlySet<string> CartridgeExtensions = new[] { MoCartridge, ToCartridge, RomCartridge }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    internal static readonly IReadOnlySet<string> Extensions = new[] { Floppy, SapFloppy, Tape, MoCartridge, ToCartridge, RomCartridge }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
