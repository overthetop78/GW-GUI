namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection concise d’une configuration vers les informations présentées dans la liste des machines.</summary>
internal static class ConfigurationSummaryFunctions
{
    /// <summary>Associe le nom traduit du modèle à ses informations de processeur, mémoire et backend.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle et le cœur sont déjà sélectionnés.</param>
    /// <returns>Résumé avec une clé de nom de modèle et trois détails techniques ; la mémoire est convertie des octets en Kio.</returns>
    /// <remarks>Les valeurs matérielles proviennent du catalogue, sans démarrer un cœur ni ouvrir un média.</remarks>
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        return new EmulationConfigurationSummary(hardware.DisplayNameResourceKey, [$"CPU {hardware.DefaultCpu}", $"RAM {hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB", $"Core {configuration.Core}"]);
    }
}
