namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        PcEngineMachineConstants.Definition,
        CoreGrafxMachineConstants.Definition,
        SuperGrafxMachineConstants.Definition,
        PcEngineDuoMachineConstants.Definition,
        TurboExpressMachineConstants.Definition,
        PcEngineLtMachineConstants.Definition,
        PcFxMachineConstants.Definition,
        LaserActiveMachineConstants.Definition,
        Machines.Pc8001.Constants.MachineConstants.Definition,
        Machines.Pc8801.Constants.MachineConstants.Definition,
        Machines.Pc8801.Dictionaries.ModelCatalog.MkII,
        Machines.Pc8801.Dictionaries.ModelCatalog.MkIISr,
        Machines.Pc9801.Constants.MachineConstants.Definition,
        Machines.Pc9821.Constants.MachineConstants.Definition,
        Machines.PcEngineCd.Constants.MachineConstants.Definition,
        Machines.PcEngineSuperCd.Constants.MachineConstants.Definition,
        Machines.PcEngineArcadeCard.Constants.MachineConstants.Definition
    ];

    internal static Model Get(string id) => All.FirstOrDefault(item =>
        string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
