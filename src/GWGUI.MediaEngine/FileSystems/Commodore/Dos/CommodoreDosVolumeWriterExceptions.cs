namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Construit les erreurs de création des volumes Commodore DOS.</summary>
public static class CommodoreDosVolumeWriterExceptions
{
    /// <summary>Indique un format cible inconnu.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"The Commodore DOS target format '{formatId}' is unsupported.");

    /// <summary>Indique une entrée incompatible avec le catalogue plat.</summary>
    public static InvalidDataException InvalidEntry(string path) => new($"The entry '{path}' cannot be stored in a Commodore DOS root directory.");

    /// <summary>Indique que le volume ne contient plus assez de blocs.</summary>
    public static InvalidDataException DiskFull() => new("The Commodore DOS volume has insufficient free blocks.");

    /// <summary>Indique qu'un fichier REL dépasse les six secteurs latéraux classiques.</summary>
    public static InvalidDataException RelativeFileTooLarge(string path) => new($"The REL file '{path}' exceeds the classic side-sector capacity.");
}
