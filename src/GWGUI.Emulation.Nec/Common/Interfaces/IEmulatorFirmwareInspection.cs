namespace GWGUI.Emulation.Nec.Common.Interfaces;

/// <summary>Identification d’un firmware depuis les métadonnées déjà observées par Common.</summary>
/// <remarks>L’adaptateur consulte ses règles sans recevoir de flux ni posséder le fichier.
/// Il complète l’identité et les modèles compatibles en conservant les observations du candidat.</remarks>
internal interface IEmulatorFirmwareInspection
{
    /// <summary>Recherche une identité connue pour les données observées du candidat.</summary>
    /// <param name="firmware">Chemin, taille, empreintes et métadonnées déjà observés par Common.</param>
    /// <returns>Résultat complété si le candidat est reconnu, sinon null afin de conserver le résultat initial.</returns>
    ScannedFirmware? IdentifyFirmware(ScannedFirmware firmware);
}
