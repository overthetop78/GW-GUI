using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants;

internal static class MachineConstants
{
    internal const string Id = "PcEngineCd";
    internal const string DisplayName = "PC Engine CD / TurboGrafx-CD";
    internal static Model Definition { get; } = PcEngineDuoMachineConstants.Definition with
    { Id = Id, DisplayName = DisplayName };
}
