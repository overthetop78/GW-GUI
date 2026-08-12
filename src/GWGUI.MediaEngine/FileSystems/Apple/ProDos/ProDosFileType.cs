namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Identifie les types de fichiers ProDOS affichés par le lecteur.</summary>
internal enum ProDosFileType : byte { Text = 0x04, Binary = 0x06, Directory = 0x0f, Basic = 0xfc, Variables = 0xfd, System = 0xff }

/// <summary>Fournit les libellés des types de fichiers ProDOS.</summary>
internal static class ProDosFileTypeNames
{
    /// <summary>Retourne le libellé affiché du type.</summary>
    public static string Get(ProDosFileType type) => type switch { ProDosFileType.Text => "Text", ProDosFileType.Binary => "Binary", ProDosFileType.Directory => "Directory", ProDosFileType.Basic => "BASIC", ProDosFileType.Variables => "Variables", ProDosFileType.System => "System", _ => $"ProDOS ${(byte)type:X2}" };
}
