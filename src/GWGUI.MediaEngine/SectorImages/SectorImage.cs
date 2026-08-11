namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Représente une image sectorielle, sa géométrie et les blocs logiques effectivement disponibles.</summary>
public sealed class SectorImage
{
    private readonly IReadOnlyDictionary<int, SectorBlock> _blocks;

    private readonly long? _capacity;
    private readonly bool _allowVariableBlockSize;
    private readonly int? _logicalBlockCount;

    public SectorImage(string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack, IEnumerable<SectorBlock> blocks, bool allowVariableBlockSize = false, long? capacity = null, int? logicalBlockCount = null)
    {
        if (blockSize <= 0) throw SectorImageExceptions.InvalidDimension(nameof(blockSize), blockSize);
        if (cylinders <= 0) throw SectorImageExceptions.InvalidDimension(nameof(cylinders), cylinders);
        if (heads <= 0) throw SectorImageExceptions.InvalidDimension(nameof(heads), heads);
        if (sectorsPerTrack <= 0) throw SectorImageExceptions.InvalidDimension(nameof(sectorsPerTrack), sectorsPerTrack);
        if (logicalBlockCount is <= 0) throw SectorImageExceptions.InvalidDimension(nameof(logicalBlockCount), logicalBlockCount);
        FormatId = formatId;
        BlockSize = blockSize;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        _allowVariableBlockSize = allowVariableBlockSize;
        _capacity = capacity;
        _logicalBlockCount = logicalBlockCount;
        _blocks = blocks.GroupBy(block => block.LogicalBlock).ToDictionary(group => group.Key, group => group.First());
    }

    public string FormatId { get; }
    public int BlockSize { get; }
    public int Cylinders { get; }
    public int Heads { get; }
    public int SectorsPerTrack { get; }
    public int BlockCount => _logicalBlockCount ?? checked(Cylinders * Heads * SectorsPerTrack);
    public long Capacity => _capacity ?? (long)BlockCount * BlockSize;
    public IReadOnlyCollection<SectorBlock> AvailableBlocks => _blocks.Values.ToArray();
    public IReadOnlyList<int> MissingBlocks => Enumerable.Range(0, BlockCount).Where(block => !_blocks.ContainsKey(block)).ToArray();

    /// <summary>Recherche un bloc à partir de son indice logique.</summary>
    /// <param name="logicalBlock">Indice logique recherché, compté à partir de zéro.</param>
    /// <param name="block">Bloc trouvé lorsque la méthode retourne <see langword="true"/>.</param>
    /// <returns><see langword="true"/> si le bloc est disponible ; sinon <see langword="false"/>.</returns>
    public bool TryGetBlock(int logicalBlock, out SectorBlock block) => _blocks.TryGetValue(logicalBlock, out block!);

    /// <summary>Retourne les données d'un bloc logique disponible.</summary>
    /// <param name="logicalBlock">Indice logique du bloc, compté à partir de zéro.</param>
    /// <returns>Données du bloc exprimées en octets.</returns>
    /// <exception cref="InvalidDataException">Le bloc est absent, ou sa taille diffère de <see cref="BlockSize"/> lorsque les tailles variables ne sont pas autorisées.</exception>
    public ReadOnlyMemory<byte> GetBlock(int logicalBlock)
    {
        if (!_blocks.TryGetValue(logicalBlock, out var block)) throw SectorImageExceptions.MissingBlock(logicalBlock);
        if (!_allowVariableBlockSize && block.Data.Count != BlockSize) throw SectorImageExceptions.InvalidBlockSize(logicalBlock, block.Data.Count, BlockSize);
        return block.Data is byte[] bytes ? bytes : block.Data.ToArray();
    }

    /// <summary>Crée une nouvelle image avec l'identifiant indiqué en conservant exactement la géométrie, les blocs et les règles de capacité de l'image courante.</summary>
    /// <param name="formatId">Nouvel identifiant de format.</param>
    /// <returns>Nouvelle image dont seul l'identifiant change.</returns>
    public SectorImage WithFormatId(string formatId) => new(formatId, BlockSize, Cylinders, Heads, SectorsPerTrack, AvailableBlocks, _allowVariableBlockSize, _capacity, _logicalBlockCount);
}
