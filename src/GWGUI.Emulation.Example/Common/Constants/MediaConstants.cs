namespace GWGUI.Emulation.Example.Common.Constants;


/// <summary>Conventions de représentation des extensions de médias et de leur ordre de montage.</summary>
/// <remarks>Le séparateur technique est utilisé dans les listes reçues du cœur ; le séparateur lisible
/// sert à présenter les extensions dans les diagnostics. L’ordre de montage est une position logique,
/// indépendante du chemin du fichier et de son contenu.</remarks>
public static class MediaConstants
{
    /// <summary>Séparateur technique entre les extensions annoncées par un cœur.</summary>
    internal const char SupportedExtensionSeparator = '|';
    /// <summary>Caractère précédant le nom d’une extension de fichier.</summary>
    internal const char ExtensionPrefix = '.';
    /// <summary>Séparateur lisible entre les extensions présentées dans un diagnostic.</summary>
    internal const string ExtensionListSeparator = ", ";
    /// <summary>Position initiale d’un média lorsqu’aucun autre ordre de montage n’est indiqué.</summary>
    public const int DefaultMountOrder = 0;
}

