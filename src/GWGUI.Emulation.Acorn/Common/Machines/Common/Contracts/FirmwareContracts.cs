namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path, bool IsRequired, bool IsOriginalFirmware = true);

/// <summary>Identité matérielle d'une image, indépendante du modèle et de l'émulateur qui pourra la charger.</summary>
/// <param name="Name">Nom technique reconnu par le contenu.</param>
/// <param name="Version">Version de l'image, lorsqu'elle est connue.</param>
/// <param name="Category">Rôle matériel ; il ne désigne pas un champ de configuration de backend.</param>
/// <param name="Combined">Image concaténée langage puis système, chacun de 16 Kio.</param>
internal sealed record FirmwareDefinition(string Name, string? Version, FirmwareCategory Category, bool Combined = false);


