namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

/// <summary>Chronologie PAL et capacités de RAM du BBC Micro Model A.</summary>
internal static class BbcMicroModelAConstants
{
    internal const int CyclesPerLine = 128;
    internal const int LinesPerFrame = 312;
    internal const int CyclesPerFrame = CyclesPerLine * LinesPerFrame;
    /// <summary>Valeurs enregistrées pour la RAM d'origine et son extension à 32 Kio.</summary>
    internal const string StandardMemoryKib = "16";
    internal const string ExpandedMemoryKib = "32";
}
