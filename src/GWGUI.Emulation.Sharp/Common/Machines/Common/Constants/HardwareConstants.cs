namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Constants;

internal static class HardwareConstants
{
    internal const string X68000CpuResource = "Emulation.Sharp.Cpu.Hd68Hc000";
    internal const string XviCpuResource = "Emulation.Sharp.Cpu.M68000";
    internal const string X68030CpuResource = "Emulation.Sharp.Cpu.M68Ec030";
    internal const string X1CpuResource = "Emulation.Sharp.Cpu.Z80A";
    internal const long X68000FrequencyHz = 10_000_000;
    internal const long XviFrequencyHz = 16_000_000;
    internal const long X68030FrequencyHz = 25_000_000;
    internal const long X1FrequencyHz = 4_000_000;
    internal const long Kibibyte = 1024;
    internal const long Mebibyte = Kibibyte * Kibibyte;
    internal const long X68000MemoryBytes = Mebibyte;
    internal const long XviMemoryBytes = 2 * Mebibyte;
    internal const long X68030MemoryBytes = 4 * Mebibyte;
    internal const long X68000DisplayMemoryBytes = Mebibyte;
    internal const long X1MemoryBytes = 64 * Kibibyte;
    internal const long X1BitmapMemoryBytes = 48 * Kibibyte;
    internal const long X1TurboBitmapMemoryBytes = 96 * Kibibyte;
}
