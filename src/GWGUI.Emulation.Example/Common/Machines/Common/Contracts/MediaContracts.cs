namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Description immuable d’un contenu monté, conservée même après son éjection.</summary>
/// <param name="Path">Chemin du fichier ou dossier utilisateur ; le module ne le possède pas.</param>
/// <param name="Category">Type de contenu attendu par le lecteur.</param>
/// <param name="Slot">Emplacement commun auquel le contenu est associé.</param>
/// <param name="Label">Nom facultatif présenté à l’utilisateur.</param>
/// <param name="IsReadOnly">Interdit les modifications du contenu par le cœur.</param>
/// <param name="IsInserted">Indique si le contenu est actuellement inséré.</param>
/// <param name="CassetteAutoBoot">Demande une impulsion de démarrage après insertion de ce contenu cassette.</param>
public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = false, bool IsInserted = true, bool CassetteAutoBoot = false);
