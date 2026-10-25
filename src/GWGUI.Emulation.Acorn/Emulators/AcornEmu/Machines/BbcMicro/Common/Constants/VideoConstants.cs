namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants;

/// <summary>Dimensions du tampon du rendu BBC courant ; elles ne définissent pas toutes les machines Acorn.</summary>
internal static class VideoConstants
{
    internal const int Width = 640;
    internal const int Height = 256;
    /// <summary>Largeur du curseur ULA en octets vidéo, indexée par les trois bits supérieurs du contrôle.</summary>
    internal static IReadOnlyList<int> CursorWidths { get; } = Array.AsReadOnly<int>([0, 0, 1, 2, 1, 0, 2, 4]);
}
