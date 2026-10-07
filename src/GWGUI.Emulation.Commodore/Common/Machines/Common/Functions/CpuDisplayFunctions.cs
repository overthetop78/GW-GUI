using System.Globalization;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class CpuDisplayFunctions
{
    internal static string DisplayName(CpuModel cpu) => cpu switch
    {
        CpuModel.ZilogZ80 => MachineSettingsConstants.ZilogDisplayName,
        CpuModel.Mos6502 or CpuModel.Mos6509 or CpuModel.Mos6510 or CpuModel.Mos7501 or CpuModel.Mos8502 =>
            string.Concat(MachineSettingsConstants.MosDisplayPrefix, ((int)cpu).ToString(CultureInfo.InvariantCulture)),
        CpuModel.Wdc65816 => string.Concat(MachineSettingsConstants.WdcDisplayPrefix, ((int)cpu).ToString(CultureInfo.InvariantCulture)),
        _ => string.Concat(MachineSettingsConstants.ProcessorDisplayPrefix, ((int)cpu).ToString(CultureInfo.InvariantCulture))
    };
}
