namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Décrit les tailles exactes d'un format ATR Atari 8-bit catalogué.</summary>
internal sealed record AtrFormatProfile(string FormatId, int SectorSize, int SectorCount)
{
    /// <summary>Longueur exacte de la charge utile, secteurs d'amorçage compris.</summary>
    public int PayloadLength => AtrLayout.GetBootAreaLength(SectorSize) + (SectorCount - (SectorSize == AtrLayout.SingleDensitySectorSize ? 0 : AtrLayout.BootSectorCount)) * SectorSize;
}
