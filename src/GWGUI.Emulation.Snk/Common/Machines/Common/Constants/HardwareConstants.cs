namespace GWGUI.Emulation.Snk.Common.Machines.Common.Constants;

internal static class HardwareConstants
{
    internal const string CartridgeCpuResource = "Emulation.Snk.Cpu.M68000";
    internal const string PocketCpuResource = "Emulation.Snk.Cpu.Tlcs900H";
    internal const long Kibibyte = 1024;
    internal const long CartridgeFrequencyHz = 12_000_000;
    internal const long PocketFrequencyHz = 6_144_000;
    internal const long CartridgeMemoryBytes = 64 * Kibibyte;
    internal const long CdMemoryBytes = 2048 * Kibibyte;
    internal const long PocketMemoryBytes = 16 * Kibibyte;
    internal const int CartridgeDisplayWidth = 320;
    internal const int CartridgeDisplayHeight = 224;
    internal const int PocketDisplayWidth = 160;
    internal const int PocketDisplayHeight = 152;
}
