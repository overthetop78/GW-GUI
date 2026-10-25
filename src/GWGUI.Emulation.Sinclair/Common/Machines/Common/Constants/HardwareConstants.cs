namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Constants;

internal static class HardwareConstants
{
    internal const string CpuResource = "Emulation.Sinclair.Cpu.Z80";
    internal const long Kibibyte = 1024;
    internal const long ZxFrequencyHz = 3_250_000;
    internal const long SpectrumFrequencyHz = 3_500_000;
    internal const long SpectrumSeFrequencyHz = 3_528_000;
    internal const long Spectrum128FrequencyHz = 3_546_900;
    internal const long SpectrumNtscFrequencyHz = 3_527_500;
    internal const long PentagonFrequencyHz = 3_584_000;
    internal const long TimexNtscFrequencyHz = 3_528_000;
    internal const long Zx80MemoryBytes = Kibibyte;
    internal const long Memory16KBytes = 16 * Kibibyte;
    internal const long Memory48KBytes = 48 * Kibibyte;
    internal const long Memory128KBytes = 128 * Kibibyte;
    internal const long SpectrumSeMemoryBytes = 280 * Kibibyte;
    internal const long Memory256KBytes = 256 * Kibibyte;
    internal const long Memory512KBytes = 512 * Kibibyte;
    internal const long Memory1024KBytes = 1024 * Kibibyte;
    internal const int DisplayWidth = 256;
    internal const int DisplayHeight = 192;
}
