namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

/// <summary>Capacité d'une banque ROM BBC ; distincte de la quantité de RAM et des chemins natifs des émulateurs.</summary>
internal static class BbcMicroFirmwareConstants
{
    internal const int BankSize = 16_384;
    /// <summary>96 glyphes de dix lignes ; la ROM interne SAA505x ne contient pas les codes de contrôle.</summary>
    internal const int CharacterRomSize = 960;
}
