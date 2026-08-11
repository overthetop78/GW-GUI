namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Représente une image sectorielle, sa géométrie et les blocs logiques effectivement disponibles.</summary>
public sealed class SectorImage
{
    /// <summary>Blocs disponibles indexés par leur numéro logique.</summary>
    private readonly IReadOnlyDictionary<int, SectorBlock> _blocks;

    /// <summary>Capacité explicite de l'image, en octets, ou <see langword="null"/> pour la déduire.</summary>
    private readonly long? _capacity;
    /// <summary>Indique si les blocs peuvent avoir une taille différente de <see cref="BlockSize"/>.</summary>
    private readonly bool _allowVariableBlockSize;
    /// <summary>Nombre logique explicite de blocs, ou <see langword="null"/> pour le déduire de la géométrie.</summary>
    private readonly int? _logicalBlockCount;

    /// <summary>Crée une image sectorielle validée à partir de sa géométrie et de ses blocs disponibles.</summary>
    /// <param name="formatId">Identifiant technique non vide du format.</param>
    /// <param name="blockSize">Taille nominale d'un bloc, en octets.</param>
    /// <param name="cylinders">Nombre strictement positif de cylindres.</param>
    /// <param name="heads">Nombre strictement positif de têtes.</param>
    /// <param name="sectorsPerTrack">Nombre strictement positif de secteurs par piste.</param>
    /// <param name="blocks">Blocs disponibles à copier dans l'image.</param>
    /// <param name="allowVariableBlockSize">Autorise des tailles de blocs différentes de <paramref name="blockSize"/>.</param>
    /// <param name="capacity">Capacité facultative de l'image, en octets.</param>
    /// <param name="logicalBlockCount">Nombre logique facultatif de blocs.</param>
    /// <exception cref="ArgumentException"><paramref name="formatId"/> est nul, vide ou blanc.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Une dimension, la capacité ou le nombre logique de blocs n'est pas strictement positif.</exception>
    /// <exception cref="InvalidDataException">Les blocs contiennent un doublon, un numéro hors limites ou dépassent la capacité annoncée.</exception>
    public SectorImage(string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack, IEnumerable<SectorBlock> blocks, bool allowVariableBlockSize = false, long? capacity = null, int? logicalBlockCount = null)
    {
        if (string.IsNullOrWhiteSpace(formatId)) throw SectorImageExceptions.InvalidFormatId(nameof(formatId), formatId);
        if (blockSize <= 0) throw SectorImageExceptions.InvalidDimension(nameof(blockSize), blockSize, "un entier strictement positif");
        if (cylinders <= 0) throw SectorImageExceptions.InvalidDimension(nameof(cylinders), cylinders, "un entier strictement positif");
        if (heads <= 0) throw SectorImageExceptions.InvalidDimension(nameof(heads), heads, "un entier strictement positif");
        if (sectorsPerTrack <= 0) throw SectorImageExceptions.InvalidDimension(nameof(sectorsPerTrack), sectorsPerTrack, "un entier strictement positif");
        if (capacity is <= 0) throw SectorImageExceptions.InvalidDimension(nameof(capacity), capacity, "un entier strictement positif");
        if (logicalBlockCount is <= 0) throw SectorImageExceptions.InvalidDimension(nameof(logicalBlockCount), logicalBlockCount, "un entier strictement positif");
        FormatId = formatId;
        BlockSize = blockSize;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        _allowVariableBlockSize = allowVariableBlockSize;
        _capacity = capacity;
        _logicalBlockCount = logicalBlockCount;
        var copiedBlocks = blocks.Select(block => block with
        {
            Data = Array.AsReadOnly(block.Data.ToArray()),
            Tag = block.Tag is null ? null : Array.AsReadOnly(block.Tag.ToArray())
        }).ToArray();
        var blockGroups = copiedBlocks.GroupBy(block => block.LogicalBlock).ToArray();
        var duplicate = blockGroups.FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw SectorImageExceptions.InvalidPropertyValue(nameof(SectorBlock.LogicalBlock), duplicate.Count(), "une valeur unique", duplicate.Key);
        var effectiveLogicalBlockCount = logicalBlockCount ?? checked(cylinders * heads * sectorsPerTrack);
        var outOfRange = blockGroups.FirstOrDefault(group => group.Key < 0 || group.Key >= effectiveLogicalBlockCount);
        if (outOfRange is not null) throw SectorImageExceptions.InvalidPropertyValue(nameof(SectorBlock.LogicalBlock), outOfRange.Key, $"une valeur comprise entre 0 et {effectiveLogicalBlockCount - 1}", outOfRange.Key);
        var describedByteCount = blockGroups.Sum(group => (long)group.First().Data.Count);
        if (capacity is { } declaredCapacity && declaredCapacity < describedByteCount) throw SectorImageExceptions.InvalidPropertyValue(nameof(Capacity), declaredCapacity, $"au moins {describedByteCount} octets");
        _blocks = blockGroups.ToDictionary(group => group.Key, group => group.First());
    }

    /// <summary>Identifiant technique du format.</summary>
    public string FormatId { get; }
    /// <summary>Taille nominale d'un bloc, en octets.</summary>
    public int BlockSize { get; }
    /// <summary>Nombre de cylindres.</summary>
    public int Cylinders { get; }
    /// <summary>Nombre de têtes.</summary>
    public int Heads { get; }
    /// <summary>Nombre de secteurs par piste.</summary>
    public int SectorsPerTrack { get; }
    /// <summary>Nombre total de blocs logiques annoncé par l'image.</summary>
    public int BlockCount => _logicalBlockCount ?? checked(Cylinders * Heads * SectorsPerTrack);
    /// <summary>Capacité totale de l'image, en octets.</summary>
    public long Capacity => _capacity ?? (long)BlockCount * BlockSize;
    /// <summary>Copie de la collection des blocs effectivement disponibles.</summary>
    public IReadOnlyCollection<SectorBlock> AvailableBlocks => _blocks.Values.ToArray();
    /// <summary>Numéros logiques des blocs absents, triés par ordre croissant.</summary>
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
