namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;

public sealed record AudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = 50,
    string Interpolation = SettingsDescriptionFunctionsConstants.Anti,
    string Filter = SettingsDescriptionFunctionsConstants.Emulated,
    int StereoSeparation = 100);
