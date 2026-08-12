using GWGUI.MediaEngine.FileSystems.Fat;

namespace GWGUI.MediaEngine.Recognition.Msx;

/// <summary>Vérifie les champs du BPB utilisés pour reconnaître un secteur d'amorçage MSX-DOS.</summary>
internal static class MsxBootSectorProbe
{
    /// <summary>Préfixe OEM identifiant MSX-DOS.</summary>
    private const string OemPrefix = "MSX";
    /// <summary>Indique si les données contiennent un secteur d'amorçage MSX-DOS plausible.</summary>
    /// <param name="data">Données sectorielles commençant par le secteur d'amorçage.</param>
    /// <returns><see langword="true"/> lorsque la taille, l'OEM et les champs indispensables du BPB sont valides.</returns>
    public static bool LooksLikeMsx(ReadOnlySpan<byte> data)
    {
        if (data.Length < FatBpbLayout.SectorSize || data.Length % FatBpbLayout.SectorSize != 0) return false;
        var oem = System.Text.Encoding.ASCII.GetString(data.Slice(FatBpbLayout.OemOffset, FatBpbLayout.OemLength));
        return oem.StartsWith(OemPrefix, StringComparison.OrdinalIgnoreCase) && FatBpbGeometryDetector.TryDetect(data, data.Length, out _);
    }
}
