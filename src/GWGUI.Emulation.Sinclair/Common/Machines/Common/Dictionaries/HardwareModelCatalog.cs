namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => new(HardwareConstants.CpuResource, Frequency(model), Memory(model), HardwareConstants.DisplayWidth, HardwareConstants.DisplayHeight);

    private static long Frequency(MachineModel model) => model switch
    {
        MachineModel.Zx80 or MachineModel.Zx81 => HardwareConstants.ZxFrequencyHz,
        MachineModel.Spectrum48KNtsc => HardwareConstants.SpectrumNtscFrequencyHz,
        MachineModel.Spectrum128K or MachineModel.SpectrumPlus2 or MachineModel.SpectrumPlus2A or MachineModel.SpectrumPlus3 or MachineModel.SpectrumPlus3E => HardwareConstants.Spectrum128FrequencyHz,
        MachineModel.Pentagon128K or MachineModel.Pentagon512K or MachineModel.Pentagon1024K => HardwareConstants.PentagonFrequencyHz,
        MachineModel.TimexTs2068 => HardwareConstants.TimexNtscFrequencyHz,
        MachineModel.SpectrumSe => HardwareConstants.SpectrumSeFrequencyHz,
        MachineModel.Spectrum16K or MachineModel.Spectrum48K or MachineModel.TimexTc2048 or MachineModel.TimexTc2068 or MachineModel.Scorpion256K => HardwareConstants.SpectrumFrequencyHz,
        _ => throw new ArgumentOutOfRangeException(nameof(model))};

    private static long Memory(MachineModel model) => model switch
    {
        MachineModel.Zx80 => HardwareConstants.Zx80MemoryBytes,
        MachineModel.Zx81 or MachineModel.Spectrum16K => HardwareConstants.Memory16KBytes,
        MachineModel.Spectrum48K or MachineModel.Spectrum48KNtsc or MachineModel.TimexTc2048 or MachineModel.TimexTc2068 or MachineModel.TimexTs2068 => HardwareConstants.Memory48KBytes,
        MachineModel.Spectrum128K or MachineModel.SpectrumPlus2 or MachineModel.SpectrumPlus2A or MachineModel.SpectrumPlus3 or MachineModel.SpectrumPlus3E or MachineModel.Pentagon128K => HardwareConstants.Memory128KBytes,
        MachineModel.SpectrumSe => HardwareConstants.SpectrumSeMemoryBytes,
        MachineModel.Scorpion256K => HardwareConstants.Memory256KBytes,
        MachineModel.Pentagon512K => HardwareConstants.Memory512KBytes,
        MachineModel.Pentagon1024K => HardwareConstants.Memory1024KBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
}
