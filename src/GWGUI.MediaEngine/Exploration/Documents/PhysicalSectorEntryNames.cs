namespace GWGUI.MediaEngine.Exploration.Documents;

/// <summary>Construit les noms techniques des pistes et secteurs physiques.</summary>
internal static class PhysicalSectorEntryNames
{
    /// <summary>Extension binaire des contenus sectoriels.</summary>
    public const string BinaryExtension = ".bin";
    /// <summary>Nom technique d'une piste identifiée par son cylindre et sa face.</summary>
    /// <param name="cylinder">Numéro de cylindre.</param>
    /// <param name="head">Numéro de face.</param>
    /// <returns>Nom au format Txx Hxx.</returns>
    public static string Track(int cylinder, int head) => $"T{cylinder:D2} H{head:D2}";
    /// <summary>Nom technique d'un secteur identifié par son numéro physique.</summary>
    /// <param name="number">Numéro physique du secteur.</param>
    /// <returns>Nom au format Sxx.bin.</returns>
    public static string Sector(int number) => $"S{number:D2}{BinaryExtension}";
}
