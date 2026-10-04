using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using static GWGUI.Emulation.Nec.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Functions;

internal static class PcEngineDuoSettingsFunctions
{
    internal static EmulationSettingsBlock CdRamBlock() =>
        Block(PcEngineDuoMachineConstants.CdMemoryBlockId, EmulationMachineTab.Ram,
            PcEngineDuoMachineConstants.CdRamResourceKey,
            SettingsDescriptionFunctionsConstants.IconMemory,
            SettingsDescriptionFunctionsConstants.OneColumn,
            Information(SettingsConstants.Model + PcEngineDuoMachineConstants.CdRamSuffix,
                EmulationMachineTab.Ram, PcEngineDuoMachineConstants.CdMemoryBlockId,
                PcEngineDuoMachineConstants.CdRamResourceKey,
                PcEngineDuoMachineConstants.CdRamDisplay));
}
