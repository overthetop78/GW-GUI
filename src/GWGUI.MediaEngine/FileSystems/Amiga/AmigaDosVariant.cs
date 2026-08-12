namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Identifie les huit variantes de blocs AmigaDOS.</summary>
internal enum AmigaDosVariant : byte
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
