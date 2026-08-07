namespace GWGUI.Scp.SectorImages;

public sealed record SectorAddress(int Cylinder, int Head, int Number);

public sealed record SectorBlock(
    int LogicalBlock,
    SectorAddress Address,
    IReadOnlyList<byte> Data,
    bool? IntegrityValid = true,
    int Revolution = 0);

public sealed class SectorImage
{
    private readonly IReadOnlyDictionary<int, SectorBlock> _blocks;

    public SectorImage(string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack, IEnumerable<SectorBlock> blocks)
    {
        if (blockSize <= 0 || cylinders <= 0 || heads <= 0 || sectorsPerTrack <= 0) throw new ArgumentOutOfRangeException(nameof(blockSize));
        FormatId = formatId;
        BlockSize = blockSize;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        _blocks = blocks.GroupBy(block => block.LogicalBlock).ToDictionary(group => group.Key, group => group.First());
    }

    public string FormatId { get; }
    public int BlockSize { get; }
    public int Cylinders { get; }
    public int Heads { get; }
    public int SectorsPerTrack { get; }
    public int BlockCount => checked(Cylinders * Heads * SectorsPerTrack);
    public long Capacity => (long)BlockCount * BlockSize;
    public IReadOnlyCollection<SectorBlock> AvailableBlocks => _blocks.Values.ToArray();
    public IReadOnlyList<int> MissingBlocks => Enumerable.Range(0, BlockCount).Where(block => !_blocks.ContainsKey(block)).ToArray();

    public bool TryGetBlock(int logicalBlock, out SectorBlock block) => _blocks.TryGetValue(logicalBlock, out block!);

    public ReadOnlyMemory<byte> GetBlock(int logicalBlock)
    {
        if (!_blocks.TryGetValue(logicalBlock, out var block)) throw new InvalidDataException($"Logical block {logicalBlock} is missing.");
        if (block.Data.Count != BlockSize) throw new InvalidDataException($"Logical block {logicalBlock} has an invalid size.");
        return block.Data is byte[] bytes ? bytes : block.Data.ToArray();
    }
}
