namespace GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Constants;

internal static class Atari8BitModelConstants
{
    internal const string Atari400And800ModelId = "400/800 (OS B)";
    internal const string Atari800XlModelId = "800XL (64K)";
    internal const string Atari130XeModelId = "130XE (128K)";
    internal const string XlXeModelId = "Modern XL/XE(320K CS)";
    internal const string XlXe576KModelId = "Modern XL/XE(576K)";
    internal const string XlXe1088KModelId = "Modern XL/XE(1088K)";
    internal const string XegsModelId = "XEGS";

    internal const string Atari400DisplayNameResource = "Emulation.Atari.Model.400";
    internal const string Atari800DisplayNameResource = "Emulation.Atari.Model.800";
    internal const string Atari800XlDisplayNameResource = "Emulation.Atari.Model.800Xl";
    internal const string Atari130XeDisplayNameResource = "Emulation.Atari.Model.130Xe";
    internal const string XlXeDisplayNameResource = "Emulation.Atari.Model.XlXe";
    internal const string XegsDisplayNameResource = "Emulation.Atari.Model.Xegs";

    internal const long CpuFrequencyHz = 1_789_772;
    internal const long FortyEightKibibytes = 48 * 1024;
    internal const long SixtyFourKibibytes = 64 * 1024;
    internal const long OneHundredTwentyEightKibibytes = 128 * 1024;
    internal const long ThreeHundredTwentyKibibytes = 320 * 1024;

    internal const int OnePort = 1;
    internal const int TwoPorts = 2;
    internal const int FourPorts = 4;
}
