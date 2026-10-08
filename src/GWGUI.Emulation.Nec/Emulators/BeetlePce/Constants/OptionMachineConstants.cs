using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using Cd = GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants;
using SuperCd = GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants;
using ArcadeCard = GWGUI.Emulation.Nec.Common.Machines.PcEngineArcadeCard.Constants.MachineConstants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> CompactDisc { get; } = new HashSet<string>(StringComparer.Ordinal)
        { PcEngineDuoMachineConstants.Id, Cd.Id, SuperCd.Id, ArcadeCard.Id };
}
