namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Décrit les types de fichiers UCSD.</summary>
public enum UcsdFileKind { Untyped, ExternalDisk, Code, Text, Info, Data, Graphics, Photo, SecureDirectory }

/// <summary>Fournit les libellés des types de fichiers UCSD.</summary>
public static class UcsdFileKindNames
{
    /// <summary>Retourne le libellé d'un type de fichier.</summary>
    public static string Get(UcsdFileKind kind) => kind switch
    {
        UcsdFileKind.ExternalDisk => "UCSD external disk file", UcsdFileKind.Code => "UCSD code file", UcsdFileKind.Text => "UCSD text file", UcsdFileKind.Info => "UCSD info file", UcsdFileKind.Data => "UCSD data file", UcsdFileKind.Graphics => "UCSD graphics file", UcsdFileKind.Photo => "UCSD photo file", UcsdFileKind.SecureDirectory => "UCSD secure directory", _ => "UCSD untyped file"
    };
}
