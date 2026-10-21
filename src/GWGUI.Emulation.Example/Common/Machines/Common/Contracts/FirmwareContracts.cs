namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Firmware sélectionné pour une fonction matérielle de la machine.</summary>
/// <param name="Category">Emplacement fonctionnel du firmware, comme le système ou l’interpréteur BASIC.</param>
/// <param name="Path">Chemin du fichier fourni ; le contrat ne l’ouvre pas et ne le prend pas en propriété.</param>
/// <param name="IsRequired">Indique si ce firmware est obligatoire pour la configuration.</param>
/// <param name="IsOriginalFirmware">Distingue un firmware d’origine d’une autre implémentation compatible.</param>
/// <remarks>Le modèle déclare les catégories acceptées. La validation et les fonctions de préparation vérifient
/// la sélection avant de présenter les fichiers au cœur ; la destruction d’une configuration ne supprime pas ces fichiers.</remarks>
public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path, bool IsRequired, bool IsOriginalFirmware = true);


