namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Identifie les huit variantes de blocs AmigaDOS.</summary>
public enum AmigaDosVariant : byte
{
    /// <summary>Old File System.</summary>
    Ofs = 0,
    /// <summary>Fast File System.</summary>
    Ffs = 1,
    /// <summary>Old File System avec noms internationaux.</summary>
    OfsInternational = 2,
    /// <summary>Fast File System avec noms internationaux.</summary>
    FfsInternational = 3,
    /// <summary>Old File System avec cache de répertoire.</summary>
    OfsDirectoryCache = 4,
    /// <summary>Fast File System avec cache de répertoire.</summary>
    FfsDirectoryCache = 5,
    /// <summary>Old File System avec noms longs.</summary>
    OfsLongNames = 6,
    /// <summary>Fast File System avec noms longs.</summary>
    FfsLongNames = 7
}

/// <summary>Expose les règles techniques associées aux variantes AmigaDOS.</summary>
public static class AmigaDosVariantExtensions
{
    /// <summary>Indique si la variante utilise Fast File System.</summary>
    public static bool IsFastFileSystem(this AmigaDosVariant variant) => ((byte)variant & 1) != 0;
    /// <summary>Indique si la variante accepte les noms longs.</summary>
    public static bool SupportsLongNames(this AmigaDosVariant variant) => variant >= AmigaDosVariant.OfsLongNames;
    /// <summary>Retourne l'identifiant technique central du système de fichiers.</summary>
    public static string FileSystemId(this AmigaDosVariant variant) => variant switch
    {
        AmigaDosVariant.Ofs => Definitions.FileSystemIds.AmigaDosOfs,
        AmigaDosVariant.Ffs => Definitions.FileSystemIds.AmigaDosFfs,
        AmigaDosVariant.OfsInternational => Definitions.FileSystemIds.AmigaDosOfsInternational,
        AmigaDosVariant.FfsInternational => Definitions.FileSystemIds.AmigaDosFfsInternational,
        AmigaDosVariant.OfsDirectoryCache => Definitions.FileSystemIds.AmigaDosOfsDirectoryCache,
        AmigaDosVariant.FfsDirectoryCache => Definitions.FileSystemIds.AmigaDosFfsDirectoryCache,
        AmigaDosVariant.OfsLongNames => Definitions.FileSystemIds.AmigaDosOfsLongNames,
        AmigaDosVariant.FfsLongNames => Definitions.FileSystemIds.AmigaDosFfsLongNames,
        _ => Definitions.FileSystemIds.AmigaDos
    };
}
