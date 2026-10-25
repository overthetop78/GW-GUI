using Pc8001 = GWGUI.Emulation.Nec.Common.Machines.Pc8001.Constants.MachineConstants;
using Pc8801 = GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants.MachineConstants;

namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;

internal static class ModelConstants
{
    internal const string BasicModeOption = "q88_basic_mode";
    internal const string NBasic = "N";
    internal const string N88V1S = "N88 V1S";
    internal const string N88V1H = "N88 V1H";
    internal const string N88V2 = "N88 V2";

    internal static string BasicModeFor(string model) => model switch
    {
        Pc8001.Id => NBasic,
        Pc8801.Id => N88V1S,
        Pc8801.MkIIId => N88V1H,
        Pc8801.MkIISrId => N88V2,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
