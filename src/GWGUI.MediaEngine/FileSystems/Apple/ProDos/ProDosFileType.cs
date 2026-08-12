namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Identifie les types de fichiers ProDOS affichés par le lecteur.</summary>
internal enum ProDosFileType : byte
{
    /// <summary>Fichier texte.</summary>
    Text = 0x04,
    /// <summary>Fichier binaire.</summary>
    Binary = 0x06,
    /// <summary>Répertoire.</summary>
    Directory = 0x0f,
    /// <summary>Programme BASIC.</summary>
    Basic = 0xfc,
    /// <summary>Variables BASIC.</summary>
    Variables = 0xfd,
    /// <summary>Fichier système.</summary>
    System = 0xff
}

/// <summary>Fournit les libellés des types de fichiers ProDOS.</summary>
internal static class ProDosFileTypeNames
{
    /// <summary>Retourne le libellé affiché du type.</summary>
    public static string Get(ProDosFileType type) => type switch { ProDosFileType.Text => "Texte", ProDosFileType.Binary => "Binaire", ProDosFileType.Directory => "Répertoire", ProDosFileType.Basic => "BASIC", ProDosFileType.Variables => "Variables", ProDosFileType.System => "Système", _ => $"ProDOS ${(byte)type:X2}" };
}
