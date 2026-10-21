namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Fonction matérielle occupée par un firmware dans la configuration.</summary>
public enum FirmwareCategory
{
    /// <summary>Firmware du système d’exploitation de la machine.</summary>
    OperatingSystem,
    /// <summary>Firmware de l’interpréteur BASIC.</summary>
    Basic,
    /// <summary>Firmware des fonctions de gestion des disques.</summary>
    DiscFilingSystem,
    /// <summary>Firmware système MOS sélectionné par le profil matériel qui le déclare.</summary>
    MasterMos
}

/// <summary>Niveau de correspondance d’un firmware avec le modèle et le backend choisis.</summary>
public enum FirmwareCompatibility
{
    /// <summary>Le firmware correspond aux capacités attendues.</summary>
    Compatible,
    /// <summary>La correspondance comporte les limitations décrites par les données de compatibilité.</summary>
    PartiallyCompatible,
    /// <summary>Le firmware ne correspond pas à la combinaison demandée.</summary>
    Incompatible
}

/// <summary>Résultat de la reconnaissance d’un fichier candidat de firmware.</summary>
public enum FirmwareDetectionStatus
{
    /// <summary>Le fichier a été associé à une identité connue du catalogue.</summary>
    Known,
    /// <summary>Le fichier est accessible mais son identité n’a pas été reconnue.</summary>
    Unknown,
    /// <summary>Le contenu du fichier n’a pas pu être lu pour le reconnaître.</summary>
    Unreadable
}

/// <summary>Mode de fourniture déclaré pour les données d’un firmware.</summary>
public enum FirmwareDistribution
{
    /// <summary>Fichier protégé fourni par l’utilisateur, distinct des ressources du module.</summary>
    UserSuppliedCopyrighted,
    /// <summary>Données protégées embarquées dans les ressources du cœur.</summary>
    EmbeddedCopyrighted,
    /// <summary>Aucun fichier externe n’est associé à cette déclaration.</summary>
    NoExternalFile
}

/// <summary>Origine de l’information utilisée pour décrire un firmware ou sa compatibilité.</summary>
public enum FirmwareEvidence
{
    /// <summary>Information issue des sources du backend identifié par B2.</summary>
    B2Sources
}

/// <summary>Algorithme d’empreinte utilisé pour reconnaître les fichiers de firmware du catalogue.</summary>
public enum FirmwareHashAlgorithm
{
    /// <summary>Empreinte MD5 calculée sur les octets du fichier candidat.</summary>
    Md5
}

/// <summary>Politique de préparation d’un firmware pour un modèle et un backend.</summary>
public enum FirmwareProvision
{
    /// <summary>Un fichier externe doit être fourni avant le démarrage.</summary>
    RequiredExternal,
    /// <summary>Un fichier externe peut être fourni mais n’est pas obligatoire.</summary>
    OptionalExternal,
    /// <summary>Le cœur fournit lui-même les données embarquées du firmware.</summary>
    Embedded,
    /// <summary>Les données embarquées du cœur peuvent être remplacées par un fichier fourni.</summary>
    EmbeddedReplaceable,
    /// <summary>Cette fonction de firmware n’est pas utilisée par la combinaison choisie.</summary>
    NotUsed
}
