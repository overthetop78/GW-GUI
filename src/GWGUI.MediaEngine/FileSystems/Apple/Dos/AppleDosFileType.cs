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
