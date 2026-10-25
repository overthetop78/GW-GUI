namespace GWGUI.Emulation.Contracts;

/// <summary>Observations d'un fichier candidat et métadonnées reconnues, sans possession du fichier.</summary>
/// <typeparam name="TDefinition">Définition de catalogue propre au module, conservée avec son type et son contenu.</typeparam>
/// <param name="Path">Chemin du candidat fourni par l'appelant.</param>
/// <param name="SizeBytes">Taille observée en octets, ou null si la lecture a échoué.</param>
/// <param name="Md5">Empreinte MD5 observée, ou null si elle n'a pas été calculée.</param>
/// <param name="Detection">État de lecture et de reconnaissance du candidat.</param>
/// <param name="Definition">Définition reconnue du catalogue du module, ou null sans définition.</param>
/// <param name="Compatibility">Compatibilité établie avec le profil demandé ; Unknown si aucun profil n'a été évalué.</param>
/// <param name="IsDuplicate">Indique si un autre candidat du parcours possède la même empreinte.</param>
/// <param name="ReadError">Diagnostic de lecture, ou null lorsque la lecture a réussi.</param>
/// <param name="Sha256">Empreinte SHA-256 observée, ou null si elle n'a pas été calculée.</param>
/// <param name="LastWriteTimeUtc">Date UTC de dernière modification observée, ou null si elle n'a pas été relevée.</param>
/// <param name="Type">Identifiant de catégorie reconnu par le module, ou null sans catégorie annoncée.</param>
/// <param name="IsOfficial">Indique si la règle de reconnaissance classe le contenu comme officiel.</param>
/// <param name="Name">Nom reconnu par l'inspecteur, ou null sans nom annoncé.</param>
/// <param name="Version">Version reconnue par l'inspecteur, ou null sans version annoncée.</param>
/// <param name="CompatibleModels">Identifiants des profils compatibles annoncés ; null est normalisé en liste vide.</param>
/// <param name="IsVerified">Indique si les données de reconnaissance sont vérifiées suivant le catalogue du module.</param>
/// <remarks>Le SDK transporte les observations et conserve la définition de catalogue sans connaître le module
/// qui l'a créée. Les collections et la définition restent empruntées. Un inspecteur retourne une copie de record
/// pour compléter l'identité sans relire le fichier ni perdre les empreintes, la date ou les données du catalogue.
/// Le résultat ne possède aucun flux, handle ou ressource de session et n'installe aucun firmware.</remarks>
public sealed record EmulationFirmwareObservation<TDefinition>(string Path, long? SizeBytes, string? Md5, EmulationFirmwareDetectionStatus Detection, TDefinition? Definition, EmulationFirmwareCompatibility Compatibility, bool IsDuplicate, string? ReadError, string? Sha256 = null, DateTime? LastWriteTimeUtc = null, string? Type = null, bool IsOfficial = false, string? Name = null, string? Version = null, IReadOnlyList<string>? CompatibleModels = null, bool IsVerified = true) where TDefinition : class
{
    /// <summary>Profils annoncés par la reconnaissance ; la collection empruntée reste inchangée.</summary>
    public IReadOnlyList<string> CompatibleModels { get; init; } = CompatibleModels ?? [];

    /// <summary>Indique que le candidat est reconnu ; cette valeur suit l'état de détection unique.</summary>
    public bool IsKnown => Detection == EmulationFirmwareDetectionStatus.Known;
}
