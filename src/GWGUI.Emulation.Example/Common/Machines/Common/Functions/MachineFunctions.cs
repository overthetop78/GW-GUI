using GWGUI.Emulation;
using System.IO;
using System.Diagnostics;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Opérations auxiliaires du cycle de machine : identification, cadence, relâchement des entrées et nettoyage de session.</summary>
internal static class MachineFunctions
{
    /// <summary>Compose le nom de thread à partir du préfixe de module, du backend et de l’identité de machine.</summary>
    /// <param name="id">Identité de configuration, écrite au format GUID sans séparateurs.</param>
    /// <param name="core">Backend dont le nom complète l’identification de la session.</param>
    /// <returns>Nom de diagnostic ; aucun thread n’est créé par cette méthode.</returns>
    internal static string ThreadName(Guid id, Emulator core) => $"{MachineConstants.ThreadNamePrefix} {core} {id:N}";

    /// <summary>Calcule l’échéance suivante dans l’unité de ticks du chronomètre haute résolution.</summary>
    /// <param name="current">Échéance de départ, exprimée dans la même unité que Stopwatch.GetTimestamp.</param>
    /// <param name="framesPerSecond">Cadence en images par seconde, bornée aux limites du module.</param>
    /// <returns>Échéance augmentée de l’intervalle entier d’une frame.</returns>
    internal static long NextFrameTimestamp(long current, double framesPerSecond) => current + (long)(Stopwatch.Frequency / Math.Clamp(framesPerSecond, MachineConstants.MinimumFramesPerSecond, MachineConstants.MaximumFramesPerSecond));

    /// <summary>Présente un instantané vide au cœur pour relâcher les entrées précédemment actives.</summary>
    /// <param name="core">Cœur emprunté, dont la durée de vie reste gérée par la machine.</param>
    internal static void ReleaseInput(IEmulatorCore core) => core.SetInput(EmulationInputSnapshot.Empty);

    /// <summary>Tente de relâcher les entrées sans interrompre le parcours de fin si le cœur refuse l’appel.</summary>
    /// <param name="core">Cœur emprunté ; il n’est pas détruit par cette méthode.</param>
    /// <remarks>Les exceptions de SetInput sont ignorées ici, contrairement à ReleaseInput qui les propage.</remarks>
    internal static void TryReleaseInput(IEmulatorCore core)
    {
        try { ReleaseInput(core); }
        catch (Exception) { }
    }

    /// <summary>Confie le retrait d’un dossier de session généré au service commun de nettoyage.</summary>
    /// <param name="sessionDirectory">Dossier possédé dont le nom est un GUID, distinct des données utilisateur empruntées.</param>
    /// <remarks>À appeler dans le finally du propriétaire après arrêt et destruction effectifs du cœur.
    /// Le service commun applique ses contrôles de nom et ses règles de retrait des attributs de lecture seule.</remarks>
    internal static void DeleteSessionDirectory(string sessionDirectory)
        => EmulationSdk.SessionDirectories.DeleteGeneratedSessionDirectory(sessionDirectory);

}
