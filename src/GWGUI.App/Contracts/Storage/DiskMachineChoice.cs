namespace GWGUI.App.Contracts.Storage;

/// <summary>Machine présentée par le sélecteur avec son état de détection pour l'image courante.</summary>
public sealed record DiskMachineChoice(string DisplayName, bool IsDetected);
