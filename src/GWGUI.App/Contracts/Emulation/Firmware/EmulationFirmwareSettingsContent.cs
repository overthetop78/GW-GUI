using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Firmware;

/// <summary>Données du sélecteur commun ; la marque et le rôle restent définis par l'appelant.</summary>
internal sealed record EmulationFirmwareSettingsContent(
    string Destination,
    string CurrentPath,
    Func<CancellationToken, Task<IReadOnlyList<EmulationFirmwareCandidate>>> Refresh,
    Func<Task> OpenFolder,
    Func<EmulationFirmwareCandidate, bool> CanUse,
    Action<Exception> ShowError,
    Func<EmulationFirmwareCandidate, string> DestinationLabel);
