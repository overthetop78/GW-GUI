namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Règles de planification et de diagnostic des sessions fictives.</summary>
internal static class MachineConstants
{
    /// <summary>Intervalle d’attente en pause, en millisecondes, pour vérifier les commandes et l’arrêt.</summary>
    internal const int PauseWaitMilliseconds = 100;
    /// <summary>Durée d’une impulsion de bascule du pointeur, en millisecondes.</summary>
    internal const int ControllerPointerPulseMilliseconds = 20;
    /// <summary>Nombre maximal de diagnostics récents ajoutés au contexte d’un échec.</summary>
    internal const int DiagnosticTailCount = 100;
    /// <summary>Nombre d’éléments d’une collection vide lors du contrôle des diagnostics.</summary>
    internal const int EmptyCount = 0;
    /// <summary>Absence de temps restant avant la prochaine échéance.</summary>
    internal const long NoRemainingTicks = 0;
    /// <summary>Fréquence indiquant l’absence d’un format audio exploitable.</summary>
    internal const int InvalidSampleRate = 0;
    /// <summary>Préfixe des threads de session, complété par le cœur et l’identité de l’instance.</summary>
    internal const string ThreadNamePrefix = "gwgui Example";
    /// <summary>Clé technique du contexte d’exception contenant les diagnostics du cœur.</summary>
    internal const string DiagnosticDataKey = "ExampleDiagnostics";
}
