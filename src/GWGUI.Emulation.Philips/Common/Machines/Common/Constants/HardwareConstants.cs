namespace GWGUI.Emulation.Philips.Common.Machines.Common.Constants;

internal static class HardwareConstants
{
    internal const string CdiCpuResource = "Emulation.Philips.Cpu.Scc68070";
    internal const string P2000CpuResource = "Emulation.Philips.Cpu.Z80";
    internal const string VideopacCpuResource = "Emulation.Philips.Cpu.I8048";
    internal const long P2000CpuFrequencyHz = 2_500_000;
    internal const long OdysseyCpuFrequencyHz = 5_369_318;
    internal const long VideopacCpuFrequencyHz = 5_911_492;
    internal const long P2000RamBytes = 32 * 1024, P2000VideoMemoryBytes = 4 * 1024;
    internal const long CdiRamBytes = 512 * 1024, CdiPlaneRamBytes = 2 * 512 * 1024;
    internal const long VideopacRamBytes = 64 + 128;
    internal const int P2000OutputWidth = 480, P2000OutputHeight = 480;
    internal const int VideopacOutputWidth = 340, VideopacOutputHeight = 250;
}
