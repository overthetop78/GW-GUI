namespace GWGUI.MediaEngine.Recognition.Msx;

/// <summary>Vérifie les champs du BPB utilisés pour reconnaître un secteur d'amorçage MSX-DOS.</summary>
internal static class MsxBootSectorProbe
{
    /// <summary>Indique si les données contiennent un secteur d'amorçage MSX-DOS plausible.</summary>
    /// <param name="data">Données sectorielles commençant par le secteur d'amorçage.</param>
    /// <returns><see langword="true"/> lorsque la taille, l'OEM et les champs indispensables du BPB sont valides.</returns>
    public static bool LooksLikeMsx(ReadOnlySpan<byte> data)
    {
        if (data.Length < 512 || data.Length % 512 != 0) return false;
        var oem = System.Text.Encoding.ASCII.GetString(data.Slice(3, 8));
        return oem.StartsWith("MSX", StringComparison.OrdinalIgnoreCase) && data[11] == 0 && data[12] == 2 && data[13] > 0 && data[16] > 0;
    }
}
