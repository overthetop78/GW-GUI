namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Contient une lecture positionnelle de blocs UCSD.</summary>
/// <param name="Bytes">Octets positionnés.</param>
/// <param name="PresentBlocks">Présence et taille correcte de chaque bloc.</param>
/// <param name="MissingBlocks">Numéros des blocs absents ou tronqués.</param>
internal sealed record UcsdBlockReadResult(IReadOnlyList<byte> Bytes, IReadOnlyList<bool> PresentBlocks, IReadOnlyList<int> MissingBlocks)
{
    /// <summary>Indique si tous les blocs demandés sont valides.</summary>
    public bool IsValid => MissingBlocks.Count == 0;
}
