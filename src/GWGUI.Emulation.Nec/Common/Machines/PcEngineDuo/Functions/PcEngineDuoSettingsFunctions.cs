using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Functions;

internal static class PcEngineDuoSettingsFunctions
{
    internal static EmulationSettingsBlock CdRamBlock() =>
        new(PcEngineDuoMachineConstants.CdMemoryBlockId, EmulationMachineTab.Ram,
            PcEngineDuoMachineConstants.CdRamResourceKey,
            [new EmulationSettingsField(SettingsConstants.Model + PcEngineDuoMachineConstants.CdRamSuffix,
                EmulationMachineTab.Ram, PcEngineDuoMachineConstants.CdMemoryBlockId,
                PcEngineDuoMachineConstants.CdRamResourceKey, EmulationSettingsEditor.Information,
                PcEngineDuoMachineConstants.CdRamDisplay)],
            SettingsDescriptionFunctionsConstants.IconMemory,
            SettingsDescriptionFunctionsConstants.OneColumn);
}
