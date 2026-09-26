namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;

public sealed record AudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = AudioConstants.DefaultLatencyMilliseconds);
