namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Sépare l'identifiant de fragment et l'offset de partage d'une adresse indirecte FileCore.</summary>
public readonly record struct AcornFileCoreAddress(uint FragmentId, int ShareOffset)
{
    /// <summary>Décode une adresse indirecte.</summary>
    public static AcornFileCoreAddress Decode(int address) => new((uint)address >> AcornFileCoreLayout.FragmentIdShift, address & AcornFileCoreLayout.ShareOffsetMask);
}
