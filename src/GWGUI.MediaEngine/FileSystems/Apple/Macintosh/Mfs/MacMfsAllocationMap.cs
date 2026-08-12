namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Décode et parcourt la carte d'allocation MFS composée d'entrées de 12 bits.</summary>
internal sealed class MacMfsAllocationMap
{
    /// <summary>Entrées 12 bits décodées de la carte.</summary>
    private readonly ushort[] entries;

    /// <summary>Crée une carte à partir de ses entrées décodées.</summary>
    private MacMfsAllocationMap(ushort[] entries) => this.entries = entries;

    /// <summary>Décode toutes les entrées, y compris la dernière lorsque leur nombre est impair.</summary>
    public static MacMfsAllocationMap Decode(ReadOnlySpan<byte> packed, int count)
    {
        if (count < 0 || count > MacMfsFileSystemLayout.MaximumAllocationCount) throw MacFileSystemExceptions.TruncatedAllocationMap(count, packed.Length, 0);
        var requiredLength = (count * MacMfsFileSystemLayout.BitsPerAllocationEntry + 7) / 8;
        if (packed.Length < requiredLength) throw MacFileSystemExceptions.TruncatedAllocationMap(count, packed.Length, requiredLength);
        var result = new ushort[count];
        for (var index = 0; index < count; index++)
        {
            var pairOffset = index / 2 * MacMfsFileSystemLayout.PackedPairLength;
            result[index] = index % 2 == 0
                ? (ushort)((packed[pairOffset] << MacMfsFileSystemLayout.HalfByteShift | packed[pairOffset + 1] >> MacMfsFileSystemLayout.HalfByteShift) & MacMfsFileSystemLayout.AllocationValueMask)
                : (ushort)(((packed[pairOffset + 1] & MacMfsFileSystemLayout.LowNibbleMask) << MacMfsFileSystemLayout.ByteShift | packed[pairOffset + 2]) & MacMfsFileSystemLayout.AllocationValueMask);
        }
        return new(result);
    }

    /// <summary>Retourne une entrée décodée.</summary>
    public ushort this[int index] => entries[index];

    /// <summary>Nombre d'entrées décodées.</summary>
    public int Count => entries.Length;

    /// <summary>Parcourt une chaîne et distingue cycle, indice hors carte et fin prématurée.</summary>
    public MacMfsAllocationChain Traverse(int firstCluster, int requiredClusterCount)
    {
        var clusters = new List<int>();
        var visited = new HashSet<int>();
        var current = firstCluster;
        var cycle = false;
        var outOfRange = false;
        while (current >= MacMfsFileSystemLayout.FirstUsableCluster && current < MacMfsFileSystemLayout.EndOfChain && clusters.Count < requiredClusterCount)
        {
            if (!visited.Add(current)) { cycle = true; break; }
            var index = current - MacMfsFileSystemLayout.FirstUsableCluster;
            if (index < 0 || index >= entries.Length) { outOfRange = true; break; }
            clusters.Add(current);
            current = entries[index];
        }
        var premature = clusters.Count < requiredClusterCount && !cycle && !outOfRange;
        return new(clusters.AsReadOnly(), !cycle && !outOfRange && !premature, cycle, outOfRange, premature);
    }
}
