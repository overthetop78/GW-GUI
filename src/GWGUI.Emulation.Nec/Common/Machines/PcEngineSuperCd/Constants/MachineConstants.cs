using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants;

internal static class MachineConstants
{
    internal const string Id = "PcEngineSuperCd";
    internal const string DisplayName = "PC Engine Super CD-ROM²";
    internal static Model Definition { get; } = PcEngineDuoMachineConstants.Definition with
    { Id = Id, DisplayName = DisplayName };
}
