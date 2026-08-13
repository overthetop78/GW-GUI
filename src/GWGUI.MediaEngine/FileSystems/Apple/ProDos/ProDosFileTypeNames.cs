namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Fournit les libellés des types de fichiers ProDOS.</summary>
internal static class ProDosFileTypeNames
{
    /// <summary>Retourne le libellé affiché du type.</summary>
    public static string Get(ProDosFileType type) => type switch { ProDosFileType.Text => "Texte", ProDosFileType.Binary => "Binaire", ProDosFileType.Directory => "Répertoire", ProDosFileType.Basic => "BASIC", ProDosFileType.Variables => "Variables", ProDosFileType.System => "Système", _ => $"ProDOS ${(byte)type:X2}" };
}
