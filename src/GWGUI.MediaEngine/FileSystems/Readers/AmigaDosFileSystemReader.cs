using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les volumes AmigaDOS OFS et FFS ainsi que leurs variantes.</summary>
public sealed class AmigaDosFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur AmigaDOS.</summary>
    public string Id => Definitions.FileSystemIds.AmigaDos;
    /// <summary>Formats d'images sectorielles pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DiskImageFormatIds.AmigaDos,
        DiskImageFormatIds.AmigaDosHighDensity
    };

    /// <summary>Indique si l'image contient un volume AmigaDOS plausible.</summary>
    /// <param name="image">Image sectorielle à examiner.</param>
    /// <returns><see langword="true"/> si un volume AmigaDOS est reconnu.</returns>
    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != AmigaDosLayout.BlockSize || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 4) return false;
        if (HasDosSignature(boot.Data)) return true;

        // Some protected/demo disks replace the normal AmigaDOS boot block with
        // custom boot code while keeping a perfectly ordinary AmigaDOS volume.
        // Accept those only when the conventional root block is structurally
        // valid and checksums correctly, to avoid mistaking arbitrary MFM data
        // for a filesystem.
        var conventionalRoot = image.BlockCount / 2;
        if (!IsRootBlock(image, conventionalRoot) || !image.TryGetBlock(conventionalRoot, out var root)) return false;
        return ChecksumValid(root.Data is byte[] bytes ? bytes : root.Data.ToArray());
    }

    /// <summary>Lit le volume AmigaDOS contenu dans l'image.</summary>
    /// <param name="image">Image sectorielle à lire.</param>
    /// <returns>Volume et entrées reconstruits.</returns>
    /// <exception cref="InvalidDataException">Le boot, la racine ou un bloc indispensable est invalide.</exception>
    public FileSystemVolume Read(SectorImage image)
    {
        if (image.TryGetBlock(0, out var bootBlock) && HasDosPrefix(bootBlock.Data) && bootBlock.Data[3] > (byte)AmigaDosLayout.MaximumVariant) throw AmigaDosExceptions.UnsupportedBootVariant(bootBlock.Data[3]);
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a supported AmigaDOS boot block.");
        var warnings = new List<string>();
        var boot = image.GetBlock(0).Span;
        var variant = HasDosSignature(boot) ? (AmigaDosVariant)boot[3] : AmigaDosVariant.Ofs;
        var rootPointer = ReadInt32(boot, AmigaDosLayout.BootRootPointerOffset);
        var conventionalRootBlock = image.BlockCount / 2;
        var rootBlock = IsRootBlock(image, rootPointer) ? rootPointer : conventionalRootBlock;
        var root = ReadRequiredBlock(image, rootBlock, "root block");
        if (ReadInt32(root, AmigaDosLayout.PrimaryTypeOffset) != AmigaDosLayout.HeaderPrimaryType || ReadInt32(root, AmigaDosLayout.SecondaryTypeOffset) != AmigaDosLayout.RootSecondaryType) throw AmigaDosExceptions.InvalidRootBlock(rootBlock);
        if (!ChecksumValid(root)) warnings.Add($"Root block {rootBlock} has an invalid checksum.");
        var hashSize = Math.Clamp(ReadInt32(root, AmigaDosLayout.HashTableSizeOffset), 0, AmigaDosLayout.RootHashTableEntryCount);
        if (hashSize == 0) hashSize = AmigaDosLayout.RootHashTableEntryCount;
        var visited = new HashSet<int> { rootBlock };
        var entries = ReadDirectory(image, root, hashSize, variant, visited, warnings, 0);
        var freeBlocks = CountFreeBlocks(image, root, warnings);
        var fileSystem = Definitions.FileSystemDisplayNames.AmigaDos(variant);
        return new(ReadBString(root, AmigaDosLayout.OrdinaryNameOffset, AmigaDosLayout.OrdinaryNameMaximumLength), fileSystem, image.Capacity, (long)freeBlocks * AmigaDosLayout.BlockSize, ReadDate(root, AmigaDosLayout.DateOffset), ReadDate(root, AmigaDosLayout.VolumeModifiedDateOffset), entries, warnings);
    }

    /// <summary>Lit récursivement les entrées d'un répertoire AmigaDOS.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, ReadOnlySpan<byte> directory, int hashSize, AmigaDosVariant variant, HashSet<int> visited, List<string> warnings, int depth)
    {
        if (depth > AmigaDosLayout.MaximumDirectoryDepth) { warnings.Add(AmigaDosExceptions.DirectoryDepthExceeded(depth)); return []; }
        var entries = new List<FileSystemEntry>();
        for (var index = 0; index < hashSize; index++)
        {
            var blockNumber = ReadInt32(directory, AmigaDosLayout.DataPointersOffset + index * 4);
            var chain = new HashSet<int>();
            while (blockNumber != 0)
            {
                if (blockNumber < 0 || blockNumber >= image.BlockCount || !chain.Add(blockNumber))
                {
                    warnings.Add($"Invalid or cyclic directory chain at block {blockNumber}.");
                    break;
                }
                if (!image.TryGetBlock(blockNumber, out var sector))
                {
                    warnings.Add($"Directory entry block {blockNumber} is missing.");
                    break;
                }
                var block = sector.Data.ToArray().AsSpan();
                var next = ReadInt32(block, AmigaDosLayout.HashChainOffset);
                if (!visited.Add(blockNumber)) { blockNumber = next; continue; }
                var type = ReadInt32(block, AmigaDosLayout.SecondaryTypeOffset);
                var name = ReadEntryName(block, variant);
                var kind = type switch { AmigaDosLayout.DirectorySecondaryType => FileSystemEntryKind.Directory, AmigaDosLayout.FileSecondaryType => FileSystemEntryKind.File, AmigaDosLayout.HardLinkSecondaryType or AmigaDosLayout.DirectoryLinkSecondaryType or AmigaDosLayout.FileLinkSecondaryType => FileSystemEntryKind.Link, _ => FileSystemEntryKind.Unknown };
                var children = kind == FileSystemEntryKind.Directory
                    ? ReadDirectory(image, block, AmigaDosLayout.RootHashTableEntryCount, variant, visited, warnings, depth + 1)
                    : Array.Empty<FileSystemEntry>();
                IReadOnlyList<byte>? content = null;
                var size = kind == FileSystemEntryKind.File ? ReadUInt32(block, AmigaDosLayout.FileSizeOffset) : 0;
                if (kind == FileSystemEntryKind.File)
                {
                    try { content = ReadFile(image, block, checked((int)size), ((byte)variant & 1) != 0, warnings); }
                    catch (Exception exception) when (exception is InvalidDataException or OverflowException) { warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception)); }
                }
                entries.Add(new(name, kind, size, ReadDate(block, AmigaDosLayout.DateOffset), ReadBString(block, AmigaDosLayout.LongNameOffset, AmigaDosLayout.CommentMaximumLength), ReadUInt32(block, AmigaDosLayout.ProtectionOffset), blockNumber,
                    ChecksumValid(block), children, content));
                blockNumber = next;
            }
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Reconstruit le contenu d'un fichier depuis ses blocs de données et d'extension.</summary>
    private static IReadOnlyList<byte> ReadFile(SectorImage image, ReadOnlySpan<byte> header, int size, bool fastFileSystem, List<string> warnings)
    {
        var output = new List<byte>(size);
        var metadata = header.ToArray();
        var extensionVisited = new HashSet<int>();
        while (true)
        {
            var highSequence = Math.Clamp(ReadInt32(metadata, AmigaDosLayout.HighSequenceOffset), 0, AmigaDosLayout.RootHashTableEntryCount);
            for (var index = 0; index < highSequence && output.Count < size; index++)
            {
                var dataBlock = ReadInt32(metadata, AmigaDosLayout.DataPointersOffset + (AmigaDosLayout.RootHashTableEntryCount - 1 - index) * 4);
                if (dataBlock <= 0 || dataBlock >= image.BlockCount || !image.TryGetBlock(dataBlock, out var sector))
                {
                    warnings.Add($"File data block {dataBlock} is missing.");
                    continue;
                }
                var data = sector.Data.ToArray();
                if (fastFileSystem) output.AddRange(data.Take(Math.Min(data.Length, size - output.Count)));
                else
                {
                    var length = Math.Clamp(ReadInt32(data, AmigaDosLayout.HashTableSizeOffset), 0, AmigaDosLayout.OfsDataMaximumLength);
                    if (ReadInt32(data, AmigaDosLayout.PrimaryTypeOffset) != AmigaDosLayout.OfsDataPrimaryType) warnings.Add($"OFS data block {dataBlock} has an unexpected type.");
                    output.AddRange(data.Skip(AmigaDosLayout.OfsDataHeaderLength).Take(Math.Min(length, size - output.Count)));
                }
            }
            var extension = ReadInt32(metadata, AmigaDosLayout.ExtensionBlockOffset);
            if (extension == 0) break;
            if (extension < 0 || extension >= image.BlockCount || !extensionVisited.Add(extension) || !image.TryGetBlock(extension, out var extensionBlock))
                throw AmigaDosExceptions.InvalidExtensionBlock(extension);
            metadata = extensionBlock.Data.ToArray();
        }
        return output.Take(size).ToArray();
    }

    /// <summary>Compte les blocs libres décrits par les bitmaps de la racine.</summary>
    private static int CountFreeBlocks(SectorImage image, ReadOnlySpan<byte> root, List<string> warnings)
    {
        var count = 0;
        for (var pointer = 0; pointer < AmigaDosLayout.MaximumBitmapPointerCount; pointer++)
        {
            var bitmapBlock = ReadInt32(root, AmigaDosLayout.BitmapPointersOffset + pointer * 4);
            if (bitmapBlock == 0) break;
            if (!image.TryGetBlock(bitmapBlock, out var sector)) { warnings.Add($"Bitmap block {bitmapBlock} is missing."); continue; }
            var bitmap = sector.Data.ToArray().AsSpan();
            if (!ChecksumValid(bitmap)) warnings.Add($"Bitmap block {bitmapBlock} has an invalid checksum.");
            for (var offset = 4; offset < AmigaDosLayout.BlockSize; offset += 4) count += System.Numerics.BitOperations.PopCount(ReadUInt32(bitmap, offset));
        }
        return Math.Min(count, image.BlockCount);
    }

    /// <summary>Lit le nom ordinaire ou long d'une entrée selon la variante.</summary>
    private static string ReadEntryName(ReadOnlySpan<byte> block, AmigaDosVariant variant)
    {
        var ordinary = ReadBString(block, AmigaDosLayout.OrdinaryNameOffset, AmigaDosLayout.OrdinaryNameMaximumLength);
        if (ordinary.Length > 0 || variant < AmigaDosVariant.OfsLongNames) return ordinary;
        return ReadBString(block, AmigaDosLayout.LongNameOffset, AmigaDosLayout.LongNameMaximumLength);
    }

    /// <summary>Décode une chaîne préfixée par sa longueur.</summary>
    private static string ReadBString(ReadOnlySpan<byte> block, int offset, int maximum)
    {
        if (offset < 0 || offset >= block.Length) return string.Empty;
        var length = Math.Min(block[offset], Math.Min(maximum, block.Length - offset - 1));
        return System.Text.Encoding.Latin1.GetString(block.Slice(offset + 1, length)).TrimEnd('\0');
    }

    /// <summary>Décode une date AmigaDOS ou retourne <see langword="null"/> si elle est invalide.</summary>
    private static DateTimeOffset? ReadDate(ReadOnlySpan<byte> block, int offset)
    {
        var days = ReadInt32(block, offset); var minutes = ReadInt32(block, offset + 4); var ticks = ReadInt32(block, offset + 8);
        if (days < 0 || minutes < 0 || minutes >= AmigaDosLayout.MinutesPerDay || ticks < 0 || ticks >= 60 * AmigaDosLayout.TicksPerSecond) return null;
        try { return AmigaDosLayout.Epoch.AddDays(days).AddMinutes(minutes).AddMilliseconds(ticks * AmigaDosLayout.TickDurationMilliseconds); } catch { return null; }
    }

    /// <summary>Retourne un bloc obligatoire ou signale son absence.</summary>
    private static ReadOnlySpan<byte> ReadRequiredBlock(SectorImage image, int blockNumber, string description)
    {
        if (!image.TryGetBlock(blockNumber, out var block)) throw AmigaDosExceptions.MissingBlock(description, blockNumber);
        return block.Data.ToArray();
    }

    /// <summary>Vérifie la structure primaire et secondaire d'un bloc racine.</summary>
    private static bool IsRootBlock(SectorImage image, int blockNumber)
    {
        if (blockNumber <= 0 || blockNumber >= image.BlockCount || !image.TryGetBlock(blockNumber, out var block) || block.Data.Count != AmigaDosLayout.BlockSize)
            return false;
        var data = block.Data is byte[] bytes ? bytes.AsSpan() : block.Data.ToArray().AsSpan();
        return ReadInt32(data, AmigaDosLayout.PrimaryTypeOffset) == AmigaDosLayout.HeaderPrimaryType && ReadInt32(data, AmigaDosLayout.SecondaryTypeOffset) == AmigaDosLayout.RootSecondaryType;
    }

    /// <summary>Vérifie le checksum additif d'un bloc AmigaDOS.</summary>
    private static bool ChecksumValid(ReadOnlySpan<byte> block)
    {
        if (block.Length != AmigaDosLayout.BlockSize) return false;
        uint sum = 0; for (var offset = 0; offset < block.Length; offset += 4) sum = unchecked(sum + ReadUInt32(block, offset));
        return sum == 0;
    }

    /// <summary>Vérifie la signature DOS et la variante d'une liste d'octets.</summary>
    private static bool HasDosSignature(IReadOnlyList<byte> boot) => HasDosPrefix(boot) && boot[3] <= (byte)AmigaDosLayout.MaximumVariant;

    /// <summary>Vérifie la signature DOS et la variante d'une plage d'octets.</summary>
    private static bool HasDosSignature(ReadOnlySpan<byte> boot) => HasDosPrefix(boot) && boot[3] <= (byte)AmigaDosLayout.MaximumVariant;

    /// <summary>Vérifie le préfixe DOS d'une liste d'octets.</summary>
    private static bool HasDosPrefix(IReadOnlyList<byte> boot) => boot.Count >= 4 && boot[0] == AmigaDosLayout.DosSignatureD && boot[1] == AmigaDosLayout.DosSignatureO && boot[2] == AmigaDosLayout.DosSignatureS;

    /// <summary>Vérifie le préfixe DOS d'une plage d'octets.</summary>
    private static bool HasDosPrefix(ReadOnlySpan<byte> boot) => boot.Length >= 4 && boot[0] == AmigaDosLayout.DosSignatureD && boot[1] == AmigaDosLayout.DosSignatureO && boot[2] == AmigaDosLayout.DosSignatureS;

    /// <summary>Lit un entier signé 32 bits en ordre big-endian.</summary>
    private static int ReadInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32BigEndian(data.Slice(offset, 4));
    /// <summary>Lit un entier non signé 32 bits en ordre big-endian.</summary>
    private static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
}
