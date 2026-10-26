using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Acorn.Common.Constants;


public static class MediaConstants
{
    internal static string WinchesterImageExtension => CommonHost.MediaProcessing.FileExtensions["Dat"];
    internal static string WinchesterDescriptorExtension => CommonHost.MediaProcessing.FileExtensions["Dsc"];
    /// <summary>Identifiant MediaEngine du format d’image Acorn Winchester.</summary>
    internal const string WinchesterFormatId = "hdd.acorn-winchester";
    /// <summary>Identifiant du système de fichiers Acorn ADFS.</summary>
    internal const string AcornAdfsFileSystemId = "acorn-adfs";
    /// <summary>Séparateur des extensions dans les filtres de sélection de fichiers.</summary>
    internal const char SupportedExtensionSeparator = '|';
    /// <summary>Caractère précédant le nom d’une extension de fichier.</summary>
    internal const char ExtensionPrefix = '.';
    /// <summary>Séparateur de la liste d’extensions présentée à l’utilisateur.</summary>
    internal const string ExtensionListSeparator = ", ";
    /// <summary>Position de montage initiale d’un média dans son lecteur.</summary>
    public const int DefaultMountOrder = 0;
}

