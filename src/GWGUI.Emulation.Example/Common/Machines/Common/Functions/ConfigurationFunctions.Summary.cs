using System.Globalization;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Résumé compact du profil et de son émulateur, sans duplication de libellés traduits.</summary>
internal static class ConfigurationSummaryFunctions
{
    /// <summary>Présente les données matérielles invariantes du profil choisi.</summary>
    /// <param name="configuration">Configuration dont le profil et le cœur doivent être résumés.</param>
    /// <returns>Clé de nom de machine et valeurs techniques de CPU, fréquence, RAM et cœur.</returns>
    /// <remarks>CPU, RAM, unités et identités de produits sont des données invariantes. Les libellés
    /// d’éditeurs restent fournis par les ressources communes de l’application.</remarks>
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        return new(hardware.DisplayNameResourceKey,
            [$"CPU {hardware.DefaultCpu} · {hardware.DefaultCpuFrequencyHz.ToString(CultureInfo.InvariantCulture)} Hz",
             $"RAM {hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture)} B", configuration.Core.ToString()]);
    }
}
