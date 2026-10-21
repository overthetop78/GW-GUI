namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;


/// <summary>Bornes, durées et identifiants utilisés par le propriétaire Common de la machine.</summary>
/// <remarks>Les cadences sont exprimées en images par seconde, l’attente de pause en millisecondes.
/// Les diagnostics et le nom de thread identifient la session sans modifier les contrats du cœur.</remarks>
internal static class MachineConstants
{
    /// <summary>Cadence minimale retenue pour produire un intervalle de calcul valide.</summary>
    internal const double MinimumFramesPerSecond = 1;
    /// <summary>Cadence maximale retenue pour borner le rythme de la boucle.</summary>
    internal const double MaximumFramesPerSecond = 1000;
    /// <summary>Durée d’attente en millisecondes utilisée lorsque la machine est en pause.</summary>
    internal const int PauseWaitMilliseconds = 100;
    /// <summary>Nombre maximal de derniers messages conservés dans le détail d’un diagnostic.</summary>
    internal const int DiagnosticTailCount = 100;
    /// <summary>Quantité indiquant l’absence d’éléments à traiter.</summary>
    internal const int EmptyCount = 0;
    /// <summary>Seuil nul utilisé lorsqu’aucun tick d’attente ne reste avant le calcul suivant.</summary>
    internal const long NoRemainingTicks = 0;
    /// <summary>Seuil nul : une fréquence audio exploitable doit être strictement positive.</summary>
    internal const int InvalidSampleRate = 0;
    /// <summary>Préfixe du nom de thread permettant d’identifier le travail de la machine dans les diagnostics.</summary>
    internal const string ThreadNamePrefix = "gwgui Example";
    /// <summary>Clé des détails de diagnostic du module ajoutés aux données d’une exception.</summary>
    internal const string DiagnosticDataKey = "ExampleDiagnostics";
}
