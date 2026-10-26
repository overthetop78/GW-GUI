namespace GWGUI.Emulation.Contracts;

/// <summary>Machine et chemin d’image externe relatif au dossier de l’application.</summary>
public sealed record EmulationMachineDefinition(string Id, string DisplayResourceKey, string? ImagePath = null);
