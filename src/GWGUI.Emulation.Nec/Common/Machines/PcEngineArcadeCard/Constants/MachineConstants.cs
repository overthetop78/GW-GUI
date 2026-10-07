using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineArcadeCard.Constants;

internal static class MachineConstants
{
    internal const string Id = "PcEngineArcadeCard";
    internal const string DisplayName = "PC Engine Arcade Card";
    internal static Model Definition { get; } = PcEngineDuoMachineConstants.Definition with
    { Id = Id, DisplayName = DisplayName };
}
