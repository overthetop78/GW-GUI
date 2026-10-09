namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

internal static class BbcMicroModelConstants
{
    internal const string ModelAId = nameof(MachineModel.BbcMicroModelA);
    internal const string ModelBId = nameof(MachineModel.BbcMicroModelB);
    internal const string Master128Id = nameof(MachineModel.BbcMaster128);
    internal const string ModelADisplayNameResource = "Emulation.Acorn.Model.BbcMicroModelA";
    internal const string ModelBDisplayNameResource = "Emulation.Acorn.Model.BbcMicroModelB";
    internal const string Master128DisplayNameResource = "Emulation.Acorn.Model.BbcMaster128";
    internal const long CpuFrequencyHz = 2_000_000;
    internal const long SixteenKibibytes = 16 * 1024;
    internal const long ThirtyTwoKibibytes = 32 * 1024;
    internal const long OneHundredTwentyEightKibibytes = 128 * 1024;
    internal const int OnePort = 1;
    internal const int TwoPorts = 2;
}
