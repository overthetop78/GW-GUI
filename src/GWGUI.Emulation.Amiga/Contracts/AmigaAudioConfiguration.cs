namespace GWGUI.Emulation.Amiga;

public sealed record AmigaAudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = 50,
    string Interpolation = "anti",
    string Filter = "emulated",
    int StereoSeparation = 100);
