namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal static readonly IReadOnlyDictionary<MachineModel, string> Models = new Dictionary<MachineModel, string>
    {
        [MachineModel.Spectrum16K] = "Spectrum 16K",
        [MachineModel.Spectrum48K] = "Spectrum 48K",
        [MachineModel.Spectrum48KNtsc] = "Spectrum 48K (NTSC)",
        [MachineModel.Spectrum128K] = "Spectrum 128K",
        [MachineModel.SpectrumPlus2] = "Spectrum +2",
        [MachineModel.SpectrumPlus2A] = "Spectrum +2A",
        [MachineModel.SpectrumPlus3] = "Spectrum +3",
        [MachineModel.SpectrumPlus3E] = "Spectrum +3e",
        [MachineModel.SpectrumSe] = "Spectrum SE",
        [MachineModel.TimexTc2048] = "Timex TC2048",
        [MachineModel.TimexTc2068] = "Timex TC2068",
        [MachineModel.TimexTs2068] = "Timex TS2068",
        [MachineModel.Pentagon128K] = "Pentagon 128K",
        [MachineModel.Pentagon512K] = "Pentagon 512K",
        [MachineModel.Pentagon1024K] = "Pentagon 1024",
        [MachineModel.Scorpion256K] = "Scorpion 256K"
    };
}
