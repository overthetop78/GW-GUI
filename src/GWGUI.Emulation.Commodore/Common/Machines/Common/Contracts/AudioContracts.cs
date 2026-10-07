namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

public sealed record AudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = MachineSettingsConstants.DefaultAudioLatencyMilliseconds,
    int StereoSeparation = MachineSettingsConstants.FullStereoSeparation);
