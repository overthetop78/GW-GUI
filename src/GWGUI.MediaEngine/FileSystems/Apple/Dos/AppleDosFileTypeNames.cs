namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Fournit les libellés affichés des types Apple DOS.</summary>
internal static class AppleDosFileTypeNames
{
    /// <summary>Retourne le libellé affiché d'un type Apple DOS.</summary>
    public static string Get(AppleDosFileType type) => type switch { AppleDosFileType.Text => "Text", AppleDosFileType.IntegerBasic => "Integer BASIC", AppleDosFileType.ApplesoftBasic => "Applesoft BASIC", AppleDosFileType.Binary => "Binary", AppleDosFileType.S => "S", AppleDosFileType.Relocatable => "Relocatable", AppleDosFileType.A => "A", AppleDosFileType.B => "B", _ => "File" };
}
