namespace GWGUI.Emulation.Atari.Common.Machines.AtariJaguar.Constants;

internal static class AtariJaguarModelConstants
{
    internal const string JaguarModelId = "jaguar";
    internal const string JaguarCdModelId = "jaguar-cd";
    internal const string JaguarDisplayNameResource = "Emulation.Atari.Model.Jaguar";
    internal const string JaguarCdDisplayNameResource = "Emulation.Atari.Model.JaguarCd";
    internal const long CpuFrequencyHz = 13_295_000;
    internal const long MainMemoryBytes = 2 * 1024 * 1024;
    internal const int TwoPorts = 2;
}
