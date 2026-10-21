using System.IO;

namespace GWGUI.Emulation.Functions;

/// <summary>Parcours communs des dossiers de firmware, accessibles à tout module indépendant.</summary>
/// <remarks>Les opérations viennent des parcours Apple et Wenquxing existants. L’identification, la compatibilité et la destination de chaque firmware restent définies par l’adaptateur.</remarks>
public static class EmulationFirmwareFunctions
{
    /// <summary>Crée le dossier si nécessaire puis inspecte ses fichiers dans l’ordre ordinal insensible à la casse.</summary>
    /// <remarks>Le parcours est plat et séquentiel. L’inspecteur reçoit le jeton d’annulation ; ses erreurs sont propagées sans transformer un firmware illisible en candidat compatible.</remarks>
    public static async ValueTask<IReadOnlyList<TCandidate>> ScanDirectoryAsync<TCandidate>(string directory,
        Func<string, CancellationToken, ValueTask<TCandidate>> inspect, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        var candidates = new List<TCandidate>();
        foreach (var path in Directory.EnumerateFiles(directory).Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            candidates.Add(await inspect(path, cancellationToken).ConfigureAwait(false));
        }
        return candidates;
    }

    /// <summary>Parcourt récursivement les fichiers triés, en excluant les points de réanalyse, puis appelle l’inspecteur pour chacun.</summary>
    /// <remarks>L’inspecteur peut ajouter plusieurs candidats à sa collection existante. Le SDK ne crée ni seconde liste ni règle de compatibilité.</remarks>
    public static void ScanRecursive(string directory, Action<string> inspect, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        foreach (var path in Directory.EnumerateFiles(directory, "*", new EnumerationOptions
                 { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint })
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            inspect(path);
        }
    }
}
