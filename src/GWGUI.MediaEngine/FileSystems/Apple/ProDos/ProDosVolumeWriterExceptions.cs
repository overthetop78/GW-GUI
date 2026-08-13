namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Construit les erreurs du créateur de volumes ProDOS.</summary>
public static class ProDosVolumeWriterExceptions
{
    /// <summary>Indique que la cible n'est pas une géométrie ProDOS prise en charge.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"The ProDOS target format '{formatId}' is unsupported.");

    /// <summary>Indique que le volume ne peut plus allouer de bloc ou d'entrée.</summary>
    public static InvalidDataException DiskFull() => new("The ProDOS volume does not have enough blocks or directory entries.");
}
