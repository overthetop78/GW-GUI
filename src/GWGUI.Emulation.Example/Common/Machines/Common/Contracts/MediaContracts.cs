namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Montage persistant d’un média dans un lecteur logique de la machine.</summary>
/// <param name="Path">Chemin du contenu fourni par l’appelant ; le record n’ouvre pas ce fichier.</param>
/// <param name="Category">Catégorie de média utilisée pour vérifier sa compatibilité avec le modèle.</param>
/// <param name="Slot">Lecteur logique auquel le média est affecté.</param>
/// <param name="Label">Libellé facultatif de présentation, distinct du chemin et de l’identité du lecteur.</param>
/// <param name="IsReadOnly">Demande de protection du média contre les écritures de la session.</param>
/// <param name="IsInserted">État initial d’insertion ; false conserve la description d’un média éjecté.</param>
/// <remarks>Common valide les montages et les projette vers le contrat du cœur.
/// Le fichier reste possédé par son appelant ; retirer ce montage ne demande pas sa suppression.</remarks>
public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = false, bool IsInserted = true);
