namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Règles de démarrage automatique d’une cassette explicitement choisies par l’utilisateur.</summary>
internal static class CassetteBootFunctions
{
    /// <summary>Détermine si le contenu inséré demande une impulsion Retour après le démarrage du cœur.</summary>
    /// <param name="configuration">Configuration portant les montages et le choix de démarrage automatique.</param>
    /// <returns>True seulement pour une cassette insérée et une demande de démarrage automatique.</returns>
    internal static bool RequiresDelayedReturn(MachineConfiguration configuration) => configuration.Media.Any(media => media.Category == MediaCategory.Cassette && media.IsInserted
        && (media.CassetteAutoBoot || string.Equals(configuration.Options.GetValueOrDefault(SettingsConstants.CassetteAutoBoot), bool.TrueString, StringComparison.OrdinalIgnoreCase)));
}

/// <summary>Projection de la présence de bande et du moteur vers l’état public du transport.</summary>
internal static class CassetteStateFunctions
{
    /// <summary>Distingue un lecteur vide, une cassette arrêtée et une lecture effectivement active.</summary>
    /// <param name="mounted">Indique la présence d’une cassette insérée.</param>
    /// <param name="motorActive">Activité du lecteur annoncée par le cœur.</param>
    /// <returns>État du transport public correspondant à ces deux informations.</returns>
    internal static EmulationCassetteState From(bool mounted, bool motorActive) => !mounted ? EmulationCassetteState.Empty : motorActive ? EmulationCassetteState.Playing : EmulationCassetteState.Stopped;
}
