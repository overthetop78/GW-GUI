namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Définit les durées de cellule des zones d'encodage Commodore 900.</summary>
internal static class Commodore900Encoding
{
    /// <summary>Durée d'une cellule dans la première zone, en ticks.</summary>
    public const uint Zone1BitCellTicks = 86;
    /// <summary>Durée d'une cellule dans la deuxième zone, en ticks.</summary>
    public const uint Zone2BitCellTicks = 93;
    /// <summary>Durée d'une cellule dans la troisième zone, en ticks.</summary>
    public const uint Zone3BitCellTicks = 100;
    /// <summary>Durée d'une cellule dans la quatrième zone, en ticks.</summary>
    public const uint Zone4BitCellTicks = 106;
}
