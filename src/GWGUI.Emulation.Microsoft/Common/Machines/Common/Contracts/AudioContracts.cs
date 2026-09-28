namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;

public sealed record AudioConfiguration(
    string? OutputDeviceId = null,
    int LatencyMilliseconds = AudioConstants.DefaultLatencyMilliseconds);
