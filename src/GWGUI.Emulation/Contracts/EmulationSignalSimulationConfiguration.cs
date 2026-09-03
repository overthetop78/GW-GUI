using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSignalSimulationConfiguration(
    EmulationSignalConnection Connection = EmulationSignalConnection.None,
    int ConnectionIntensity = 0,
    EmulationSignalStandard Standard = EmulationSignalStandard.Automatic,
    int StandardIntensity = 0);
