namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Contient le nombre de blocs libres et la validité de leur bitmap.</summary>
internal sealed record ProDosBitmapResult
{
    /// <summary>Crée un résultat de lecture du bitmap.</summary>
    public ProDosBitmapResult(int freeBlocks, bool isValid)
    {
        FreeBlocks = freeBlocks;
        IsValid = isValid;
    }

    /// <summary>Nombre de blocs indiqués comme libres.</summary>
    public int FreeBlocks { get; }
    /// <summary>Indique si tous les blocs bitmap requis étaient valides.</summary>
    public bool IsValid { get; }
}
