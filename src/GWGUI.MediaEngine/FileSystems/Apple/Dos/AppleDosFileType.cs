namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Identifie les types de fichiers Apple DOS.</summary>
internal enum AppleDosFileType : byte
{
    /// <summary>Fichier texte.</summary>
    Text = 0,
    /// <summary>Programme Integer BASIC.</summary>
    IntegerBasic = 1,
    /// <summary>Programme Applesoft BASIC.</summary>
    ApplesoftBasic = 2,
    /// <summary>Fichier binaire.</summary>
    Binary = 4,
    /// <summary>Type S.</summary>
    S = 8,
    /// <summary>Fichier relogeable.</summary>
    Relocatable = 16,
    /// <summary>Type A.</summary>
    A = 32,
    /// <summary>Type B.</summary>
    B = 64
}

/// <summary>Fournit les libellés affichés des types Apple DOS.</summary>
internal static class AppleDosFileTypeNames
{
    /// <summary>Retourne le libellé affiché d'un type Apple DOS.</summary>
    public static string Get(AppleDosFileType type) => type switch { AppleDosFileType.Text => "Text", AppleDosFileType.IntegerBasic => "Integer BASIC", AppleDosFileType.ApplesoftBasic => "Applesoft BASIC", AppleDosFileType.Binary => "Binary", AppleDosFileType.S => "S", AppleDosFileType.Relocatable => "Relocatable", AppleDosFileType.A => "A", AppleDosFileType.B => "B", _ => "File" };
}
