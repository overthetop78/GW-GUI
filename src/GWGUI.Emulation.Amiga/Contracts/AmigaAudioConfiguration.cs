namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaAudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = 50,
    string Interpolation = AmigaAudioConfigurationConstants.Anti,
    string Filter = AmigaAudioConfigurationConstants.Emulated,
    int StereoSeparation = 100);
