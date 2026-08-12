namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Identifie les familles techniques sondées dans une capture SCP.</summary>
internal enum ScpFormatFamily
{
    /// <summary>Famille sectorielle commune ISO FM/MFM.</summary>
    Iso,
    /// <summary>Famille spécialisée Amiga MFM.</summary>
    Amiga,
    /// <summary>Famille spécialisée Commodore GCR.</summary>
    Commodore,
    /// <summary>Famille spécialisée Apple GCR.</summary>
    Apple,
    /// <summary>Famille spécialisée DEC RX02.</summary>
    Dec
}
