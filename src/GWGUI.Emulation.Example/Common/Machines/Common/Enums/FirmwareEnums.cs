namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Rôles de firmware décrits par les emplacements du module.</summary>
public enum FirmwareCategory
{
    /// <summary>Logiciel système de la machine.</summary>
    OperatingSystem,
    /// <summary>Interpréteur de commandes optionnel.</summary>
    Basic,
    /// <summary>Logiciel du contrôleur de stockage.</summary>
    StorageController,
}

/// <summary>Résultat de la comparaison du candidat au profil attendu.</summary>
public enum FirmwareCompatibility
{
    /// <summary>Le candidat satisfait les contraintes annoncées.</summary>
    Compatible,
    /// <summary>Le candidat ne couvre qu’une partie des modèles ou régions.</summary>
    PartiallyCompatible,
    /// <summary>Le candidat ne satisfait pas les contraintes.</summary>
    Incompatible,
}

/// <summary>Résultat de lecture et de reconnaissance d’un candidat.</summary>
public enum FirmwareDetectionStatus
{
    /// <summary>Le candidat correspond à une définition connue.</summary>
    Known,
    /// <summary>Le candidat est lisible mais son identité n’est pas connue.</summary>
    Unknown,
    /// <summary>Le candidat n’a pas pu être lu.</summary>
    Unreadable,
}

/// <summary>Origine et possibilité de fournir un firmware avec le module.</summary>
public enum FirmwareDistribution
{
    /// <summary>Fichier fourni et conservé par l’utilisateur.</summary>
    UserSuppliedCopyrighted,
    /// <summary>Contenu embarqué soumis à ses conditions de distribution.</summary>
    EmbeddedCopyrighted,
    /// <summary>Aucun fichier externe nécessaire à la simulation.</summary>
    NoExternalFile,
}

/// <summary>Origine des règles documentant l’identité d’un firmware.</summary>
public enum FirmwareEvidence
{
    /// <summary>Déclaration du profil de machine dans le module.</summary>
    ModuleDefinition,
}

/// <summary>Algorithmes d’identité du catalogue de firmwares existant.</summary>
public enum FirmwareHashAlgorithm
{
    /// <summary>Empreinte MD5 permettant d’identifier un candidat du catalogue.</summary>
    Md5,
}

/// <summary>Politique de fourniture d’un rôle de firmware.</summary>
public enum FirmwareProvision
{
    /// <summary>Le rôle nécessite un fichier fourni à la configuration.</summary>
    RequiredExternal,
    /// <summary>Le rôle accepte un fichier facultatif.</summary>
    OptionalExternal,
    /// <summary>Le cœur fournit lui-même ce rôle.</summary>
    Embedded,
    /// <summary>Le cœur fournit le rôle mais accepte son remplacement.</summary>
    EmbeddedReplaceable,
    /// <summary>Le profil n’utilise pas ce rôle.</summary>
    NotUsed,
}
