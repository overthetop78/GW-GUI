using System.Diagnostics;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Planification et nettoyage partagés par les deux machines fictives.</summary>
internal static class MachineFunctions
{
    /// <summary>Construit un nom de thread distinguant le cœur et l’instance de machine.</summary>
    /// <param name="id">Identité de la configuration.</param>
    /// <param name="core">Émulateur auquel la session est confiée.</param>
    /// <returns>Nom technique destiné aux diagnostics de threads.</returns>
    internal static string ThreadName(Guid id, Emulator core) => $"{MachineConstants.ThreadNamePrefix} {core} {id:N}";

    /// <summary>Calcule l’échéance suivante à partir de la fréquence bornée du cœur.</summary>
    /// <param name="current">Échéance précédente en ticks de Stopwatch.</param>
    /// <param name="framesPerSecond">Cadence demandée, en images par seconde.</param>
    /// <returns>Échéance suivante dans la même base de temps que current.</returns>
    internal static long NextFrameTimestamp(long current, double framesPerSecond) => current + (long)(Stopwatch.Frequency / Math.Clamp(framesPerSecond, VideoConstants.MinimumFramesPerSecond, VideoConstants.MaximumFramesPerSecond));

    /// <summary>Relâche toutes les entrées du cœur emprunté.</summary>
    /// <param name="core">Cœur possédé par la session, encore utilisable pendant cet appel.</param>
    internal static void ReleaseInput(IEmulatorCore core) => core.SetInput(EmulationInputSnapshot.Empty);

    /// <summary>Tente le relâchement pendant le nettoyage, sans masquer l’erreur initiale du cœur.</summary>
    /// <param name="core">Cœur en cours d’arrêt dont la destruction reste à son propriétaire.</param>
    internal static void TryReleaseInput(IEmulatorCore core)
    {
        try { ReleaseInput(core); }
        catch (Exception) { }
    }

    /// <summary>Nettoie le seul dossier temporaire généré après la destruction effective du cœur.</summary>
    /// <param name="sessionDirectory">Chemin du dossier GUID créé et possédé par la session.</param>
    /// <remarks>Le service commun contrôle les conventions du dossier ; les contenus utilisateur restent empruntés.</remarks>
    internal static void DeleteSessionDirectory(string sessionDirectory) => EmulationSdk.SessionDirectories.DeleteGeneratedSessionDirectory(sessionDirectory);
}
