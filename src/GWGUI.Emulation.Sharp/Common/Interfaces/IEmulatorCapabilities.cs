namespace GWGUI.Emulation.Sharp.Common.Interfaces;

/// <summary>Capacités annoncées par l’adaptateur pour la présentation et l’exécution de ses machines.</summary>
/// <remarks>Le contrat de pointeur tient compte de la configuration et de ses périphériques.
/// Ces capacités ne possèdent ni sortie physique ni session ; Common les consulte avant de créer le runtime.</remarks>
internal interface IEmulatorCapabilities : IEmulatorPointerCapabilities
{
    /// <summary>Indique si le cœur peut rester initialisé sans contenu inséré.</summary>
    /// <remarks>La fabrique transmet la même capacité à la machine pour les éjections et redémarrages de médias.</remarks>
    bool SupportsNoContent { get; }
}
