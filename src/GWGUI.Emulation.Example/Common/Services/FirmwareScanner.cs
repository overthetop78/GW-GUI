using System.IO;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Parcours asynchrone du dossier de firmwares et signalement des contenus présents plusieurs fois.</summary>
/// <param name="firmwareRoot">Dossier de recherche emprunté ; les fichiers restent la propriété de l’utilisateur.</param>
/// <remarks>Les règles d’identification sont déléguées aux fonctions de scan. Le signalement des doublons
/// porte sur une empreinte commune, indépendamment du nom ou de l’emplacement du fichier.</remarks>
public sealed class FirmwareScanner(string firmwareRoot)
{
    // Un chemin absolu stabilise la recherche même si le répertoire courant du processus change.
    private readonly string _firmwareRoot = Path.GetFullPath(firmwareRoot);

    /// <summary>Inspecte les candidats puis complète leur indicateur de doublon.</summary>
    /// <param name="model">Modèle dont les contraintes de firmware sont recherchées.</param>
    /// <param name="region">Région matérielle facultative utilisée pour évaluer la compatibilité.</param>
    /// <param name="cancellationToken">Annulation de l’énumération et de chaque lecture de fichier.</param>
    /// <returns>Résultats dans l’ordre du parcours, avec les empreintes partagées signalées.</returns>
    /// <remarks>Les fichiers sans empreinte ne sont pas regroupés comme doublons.
    /// Chaque analyse possède et libère ses flux avant de rendre son résultat.</remarks>
    public async Task<IReadOnlyList<ScannedFirmware>> ScanAsync(MachineModel model, HardwareRegion? region = null, CancellationToken cancellationToken = default)
    {
        var candidates = await Task.Run(() => FirmwareScanFunctions.EnumerateCandidates(_firmwareRoot), cancellationToken).ConfigureAwait(false);
        var scanned = new List<ScannedFirmware>(candidates.Count);
        var inspections = EmulatorCatalog.CreateAdapters().OfType<IEmulatorFirmwareInspection>().ToArray();
        foreach (var path in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanned.Add(await FirmwareScanFunctions.ScanFileAsync(inspections, path, model, region, cancellationToken).ConfigureAwait(false));
        }
        var duplicateHashes = scanned.Where(item => item.Md5 is not null).GroupBy(item => item.Md5!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() >= 2).Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return scanned.Select(item => item with { IsDuplicate = item.Md5 is not null && duplicateHashes.Contains(item.Md5) }).ToArray();
    }
}
