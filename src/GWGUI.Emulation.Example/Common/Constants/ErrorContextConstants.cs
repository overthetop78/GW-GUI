namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Clés invariantes des valeurs structurées jointes aux diagnostics du module.</summary>
/// <remarks>Le code d’erreur identifie le problème ; le contexte transporte ses données particulières.
/// Les clés sont indépendantes de la langue du message et permettent au récepteur de retrouver les valeurs
/// sans analyser le texte destiné à l’utilisateur.</remarks>
internal static class ErrorContextConstants
{
    /// <summary>Chemin du fichier ou du dossier concerné par le diagnostic.</summary>
    internal const string Path = "path";
    /// <summary>Version de document, de cœur ou de protocole concernée par le contrôle.</summary>
    internal const string Version = "version";
    /// <summary>Valeur attendue par un contrôle de compatibilité.</summary>
    internal const string Expected = "expected";
    /// <summary>Valeur effectivement observée par le contrôle.</summary>
    internal const string Actual = "actual";
    /// <summary>Extension du fichier refusé ou analysé.</summary>
    internal const string Extension = "extension";
    /// <summary>Extensions admises pour le contenu ou le lecteur concerné.</summary>
    internal const string SupportedExtensions = "supportedExtensions";
}

