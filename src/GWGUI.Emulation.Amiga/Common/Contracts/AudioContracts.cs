namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record AudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = 50,
    string Interpolation = AudioConstants.Anti,
    string Filter = AudioConstants.Emulated,
    int StereoSeparation = 100);
