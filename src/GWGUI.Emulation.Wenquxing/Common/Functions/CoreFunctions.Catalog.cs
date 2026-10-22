using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Wenquxing.Common.Functions;

/// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
internal static class EmulatorCatalogFunctions
{
    /// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur et ferme ses flux de lecture. Les chemins, identités et callbacks de validation restent fournis par le module ; aucune identité de module connue du SDK n’est nécessaire.</remarks>
    internal static IEmulationCoreDiagnostics Diagnostics => EmulationSdk.CoreDiagnostics;

}
