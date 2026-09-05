using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationSignalSimulationConfiguration(
    EmulationSignalConnection Connection = EmulationSignalConnection.None,
    int ConnectionIntensity = 0,
    EmulationSignalStandard Standard = EmulationSignalStandard.Automatic,
    int StandardIntensity = 0);
