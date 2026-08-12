namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Construit les erreurs communes aux builders d'images sectorielles.</summary>
internal static class SectorImageBuilderExceptions
{
    /// <summary>Crée l'erreur signalant une longueur incompatible avec la capacité d'une géométrie.</summary>
    public static InvalidDataException InvalidLength(string geometry, int observed, int expected) => new($"{geometry} contains {observed} bytes; expected exactly {expected} bytes.");
    /// <summary>Crée l'erreur signalant un numéro logique extérieur à la géométrie dense.</summary>
    public static InvalidDataException InvalidLogicalBlock(string geometry, int logicalBlock, int blockCount) => new($"{geometry} logical block {logicalBlock} is outside 0 through {blockCount - 1}.");
}
