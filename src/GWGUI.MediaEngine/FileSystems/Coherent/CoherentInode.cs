namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Représente les champs d'un inode COHERENT utilisés pendant l'exploration.</summary>
internal sealed record CoherentInode
{
    /// <summary>Crée un inode en copiant ses pointeurs.</summary>
    public CoherentInode(ushort mode, uint size, IEnumerable<int> blocks, uint modified) { Mode = mode; Size = size; Blocks = Array.AsReadOnly(blocks.ToArray()); Modified = modified; }
    /// <summary>Mode et droits.</summary>
    public ushort Mode { get; }
    /// <summary>Taille logique.</summary>
    public uint Size { get; }
    /// <summary>Pointeurs directs et indirects.</summary>
    public IReadOnlyList<int> Blocks { get; }
    /// <summary>Date Unix de modification.</summary>
    public uint Modified { get; }
}
