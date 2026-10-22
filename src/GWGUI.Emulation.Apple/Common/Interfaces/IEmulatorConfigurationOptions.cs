namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Préparation des options propres à un backend avant la validation de sa configuration.</summary>
/// <remarks>Le dictionnaire de préparation appartient à l’appelant. Le backend y conserve ses valeurs
/// dépendant du modèle, des firmwares et des médias ; ce contrat ne démarre aucune session de cœur.</remarks>
internal interface IEmulatorConfigurationOptions : IEmulatorConfigurationValidation
{
    /// <summary>Complète les options de préparation à partir de la configuration courante.</summary>
    /// <param name="configuration">Configuration empruntée déterminant les valeurs initiales.</param>
    /// <param name="options">Dictionnaire modifiable de l’appelant, complété par les règles existantes du backend.</param>
    void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options);
}
