namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Application des valeurs d’éditeurs propres à l’adaptateur sélectionné.</summary>
/// <remarks>Common prépare les valeurs communes avant de demander les adaptations du backend et de valider le résultat.</remarks>
internal interface IEmulatorSettingsApplication
{
    /// <summary>Adapte la configuration aux réglages pris en charge par le backend.</summary>
    /// <param name="configuration">Configuration de départ empruntée, dont les autres données sont conservées.</param>
    /// <param name="values">Valeurs proposées par l’hôte, indexées par les identifiants des champs.</param>
    /// <returns>Configuration résultant des règles existantes de l’adaptateur, avant sa validation Common finale.</returns>
    MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values);
}
