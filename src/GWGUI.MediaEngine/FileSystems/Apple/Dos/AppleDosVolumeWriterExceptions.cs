namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Construit les erreurs du créateur de volumes Apple DOS.</summary>
public static class AppleDosVolumeWriterExceptions
{
    /// <summary>Indique que le format cible n'est ni DOS 3.2 ni DOS 3.3.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"The Apple DOS target format '{formatId}' is unsupported.");

    /// <summary>Indique que le catalogue ou les secteurs disponibles ne suffisent pas.</summary>
    public static InvalidDataException DiskFull() => new("The Apple DOS volume does not have enough catalog or data sectors.");
}
