namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Identifie les organisations de stockage d'une entrée ProDOS.</summary>
internal enum ProDosStorageType : byte
{
    /// <summary>Entrée inactive.</summary>
    Inactive = 0,
    /// <summary>Fichier contenu dans un bloc unique.</summary>
    Seedling = 1,
    /// <summary>Fichier utilisant un bloc d'index.</summary>
    Sapling = 2,
    /// <summary>Fichier utilisant un index maître et des index enfants.</summary>
    Tree = 3,
    /// <summary>Sous-répertoire.</summary>
    Subdirectory = 0x0d,
    /// <summary>En-tête de volume.</summary>
    VolumeHeader = 0x0f
}
