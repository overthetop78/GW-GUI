namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Décrit une disposition physique de répertoire et d'allocations CP/M.</summary>
internal sealed record CpmLayout
{
    /// <summary>Crée et valide une disposition CP/M.</summary>
    public CpmLayout(int directoryOffset, int allocationOrigin, int directoryEntries, int allocationBlockSize, int directoryBlocks, bool wideAllocations)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(directoryOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(allocationOrigin);
        ArgumentOutOfRangeException.ThrowIfLessThan(directoryEntries, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(allocationBlockSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(directoryBlocks, 1);
        DirectoryOffset = directoryOffset;
        AllocationOrigin = allocationOrigin;
        DirectoryEntries = directoryEntries;
        AllocationBlockSize = allocationBlockSize;
        DirectoryBlocks = directoryBlocks;
        WideAllocations = wideAllocations;
    }

    /// <summary>Offset du répertoire.</summary>
    public int DirectoryOffset { get; init; }
    /// <summary>Origine des blocs d'allocation.</summary>
    public int AllocationOrigin { get; init; }
    /// <summary>Nombre d'entrées de répertoire.</summary>
    public int DirectoryEntries { get; }
    /// <summary>Taille d'un bloc d'allocation.</summary>
    public int AllocationBlockSize { get; }
    /// <summary>Nombre de blocs réservés au répertoire.</summary>
    public int DirectoryBlocks { get; }
    /// <summary>Indique si les allocations occupent deux octets.</summary>
    public bool WideAllocations { get; }
}
