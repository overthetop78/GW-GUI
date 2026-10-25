namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;

internal static class HardwareConstants
{
    internal const string PdCpuResource = "Emulation.Epoch.Cpu.PD777";
    internal const string ScvCpuResource = "Emulation.Epoch.Cpu.UPD7801";
    internal const long ScvCpuFrequencyHz = 4_000_000;
    internal const long ScvInternalMemoryBytes = 128;
    internal const long ScvVideoMemoryBytes = 8192;
    internal const int PdDisplayWidth = 375, PdDisplayHeight = 240;
    internal const int ScvDisplayWidth = 198, ScvDisplayHeight = 225;
    internal const int YenoDisplayWidth = 186, YenoDisplayHeight = 248;
}
