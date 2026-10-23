namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Choix d’un fichier de firmware pour un rôle de la machine.</summary>
/// <param name="Category">Rôle auquel le fichier est affecté.</param>
/// <param name="Path">Chemin du fichier utilisateur emprunté.</param>
/// <param name="IsRequired">Indique si le profil exige ce fichier.</param>
/// <param name="IsOriginalFirmware">Indique si le candidat conserve son contenu original.</param>
/// <param name="FieldId">Identifiant du champ lorsqu’un même rôle possède plusieurs emplacements.</param>
public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path, bool IsRequired, bool IsOriginalFirmware = true, string? FieldId = null);

/// <summary>Empreinte du catalogue utilisée pour reconnaître un candidat.</summary>
/// <param name="Algorithm">Algorithme appliqué aux octets du candidat.</param>
/// <param name="Value">Représentation textuelle de l’empreinte.</param>
/// <param name="Region">Région précisée par cette empreinte, ou null pour toutes les régions.</param>
public sealed record FirmwareFingerprint(FirmwareHashAlgorithm Algorithm, string Value, HardwareRegion? Region = null);

/// <summary>Métadonnées de reconnaissance d’un rôle de firmware.</summary>
/// <param name="Id">Identifiant stable de la définition.</param>
/// <param name="Category">Rôle reconnu, ou null si non déterminé.</param>
/// <param name="Version">Version reconnue du contenu.</param>
/// <param name="ExpectedFileName">Nom de fichier attendu lorsqu’il est imposé.</param>
/// <param name="ExpectedSizeBytes">Taille attendue en octets, ou null sans contrainte.</param>
/// <param name="Provision">Politique de fourniture du rôle.</param>
/// <param name="Distribution">Origine du contenu et conditions de fourniture.</param>
/// <param name="Evidence">Origine des données de reconnaissance.</param>
/// <param name="Models">Profils compatibles avec la définition.</param>
/// <param name="Regions">Régions compatibles.</param>
/// <param name="Fingerprints">Empreintes connues du contenu.</param>
public sealed record FirmwareDefinition(string Id, FirmwareCategory? Category, string? Version, string? ExpectedFileName, long? ExpectedSizeBytes, FirmwareProvision Provision, FirmwareDistribution Distribution, FirmwareEvidence Evidence, IReadOnlyList<MachineModel> Models, IReadOnlyList<HardwareRegion> Regions, IReadOnlyList<FirmwareFingerprint> Fingerprints)
{
    /// <summary>Indique que le démarrage exige un fichier externe pour ce rôle.</summary>
    public bool RequiresExternalFile => Provision == FirmwareProvision.RequiredExternal;
    /// <summary>Indique que les règles de distribution autorisent la fourniture du contenu par le module.</summary>
    public bool CanBePackaged => Distribution != FirmwareDistribution.UserSuppliedCopyrighted;
}
