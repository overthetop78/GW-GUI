using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit en lecture seule le système de fichiers COHERENT de type V7 utilisé par le Commodore 900.</summary>
public sealed class CoherentFileSystemReader : IFileSystemReader
{
    /// <summary>Obtient l'identifiant du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.Coherent;
    /// <summary>Obtient les formats dont le catalogue peut être lu.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.Commodore900Coherent };

    /// <summary>Indique si l'image utilise le format et la taille de bloc COHERENT attendus.</summary>
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == CoherentSuperblockLayout.BlockSize;

    /// <summary>Lit le volume, ses métadonnées et son arborescence.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        var bytes = Flatten(image);
        var fileSystemBlocks = CoherentSuperblockProbe.ReadValidatedFileSystemBlockCount(bytes);
        var inodeZoneEnd = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(CoherentSuperblockLayout.InodeZoneEndOffset, sizeof(ushort)));
        if (inodeZoneEnd < 3 || inodeZoneEnd > fileSystemBlocks) throw CoherentFileSystemExceptions.InvalidInodeZone(inodeZoneEnd, fileSystemBlocks);

        var warnings = new List<string>();
        var visited = new HashSet<ushort>();
        var entries = ReadDirectory(bytes, CoherentSuperblockLayout.RootInodeNumber, visited, warnings);
        var volumeName = DecodeFixed(bytes.AsSpan(CoherentSuperblockLayout.VolumeNameOffset, CoherentSuperblockLayout.NameLength));
        if (volumeName is CoherentSuperblockLayout.PlaceholderName or CoherentSuperblockLayout.DefaultVolumeName) volumeName = string.Empty;
        var freeBytes = (long)CoherentCanonicalBinary.ReadUInt32(bytes.AsSpan(CoherentSuperblockLayout.FreeBlockCountOffset, CoherentCanonicalBinary.UInt32Length)) * CoherentSuperblockLayout.BlockSize;
        var modified = DecodeTime(CoherentCanonicalBinary.ReadUInt32(bytes.AsSpan(CoherentSuperblockLayout.ModifiedTimeOffset, CoherentCanonicalBinary.UInt32Length)));
        return new(volumeName, Definitions.FileSystemDisplayNames.CoherentCommodore900, (long)fileSystemBlocks * CoherentSuperblockLayout.BlockSize, Math.Clamp(freeBytes, 0, (long)fileSystemBlocks * CoherentSuperblockLayout.BlockSize), null, modified, entries, warnings);
    }

    /// <summary>Lit récursivement un répertoire en empêchant les cycles d'inodes.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(byte[] image, ushort inodeNumber, HashSet<ushort> visited, List<string> warnings)
    {
        if (!visited.Add(inodeNumber)) return [];
        var inode = ReadInode(image, inodeNumber);
        var data = ReadFileData(image, inode, warnings, $"inode {inodeNumber}");
        var result = new List<FileSystemEntry>();
        for (var offset = 0; offset + CoherentSuperblockLayout.DirectoryEntrySize <= data.Length; offset += CoherentSuperblockLayout.DirectoryEntrySize)
        {
            var childNumber = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
            if (childNumber == 0) continue;
            var name = DecodeFixed(data.AsSpan(offset + sizeof(ushort), CoherentSuperblockLayout.DirectoryNameLength));
            if (name.Length == 0 || name is "." or "..") continue;
            try
            {
                var child = ReadInode(image, childNumber);
                var directory = (child.Mode & CoherentSuperblockLayout.TypeMask) == CoherentSuperblockLayout.DirectoryMode;
                var content = directory ? null : ReadFileData(image, child, warnings, name);
                var children = directory ? ReadDirectory(image, childNumber, visited, warnings) : [];
                result.Add(new(name, directory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                    child.Size, DecodeTime(child.Modified), $"COHERENT inode {childNumber}", (uint)(child.Mode & CoherentSuperblockLayout.ProtectionMask),
                    childNumber, true, children, content));
            }
            catch (InvalidDataException exception)
            {
                warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception));
            }
        }
        return result.OrderByDescending(entry => entry.Kind == FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit l'inode demandé et ses treize pointeurs de blocs.</summary>
    private static CoherentInode ReadInode(byte[] image, ushort number)
    {
        if (number == 0) throw CoherentFileSystemExceptions.NullInode();
        var offset = CoherentSuperblockLayout.BlockSize * 2 + (number - 1) * CoherentSuperblockLayout.InodeSize;
        if (offset < 0 || offset + CoherentSuperblockLayout.InodeSize > image.Length) throw CoherentFileSystemExceptions.InodeOutsideImage(number, image.Length);
        var value = image.AsSpan(offset, CoherentSuperblockLayout.InodeSize);
        var pointers = new int[CoherentSuperblockLayout.InodePointerCount];
        for (var index = 0; index < pointers.Length; index++)
        {
            var item = value.Slice(CoherentSuperblockLayout.InodePointersOffset + index * CoherentSuperblockLayout.InodePointerSize, CoherentSuperblockLayout.InodePointerSize);
            pointers[index] = item[1] | item[2] << BitPrimitives.BitsPerByte | item[0] << 16;
        }
        return new(BinaryPrimitives.ReadUInt16LittleEndian(value[CoherentSuperblockLayout.InodeModeOffset..]), CoherentCanonicalBinary.ReadUInt32(value.Slice(CoherentSuperblockLayout.InodeSizeOffset, CoherentCanonicalBinary.UInt32Length)), pointers, CoherentCanonicalBinary.ReadUInt32(value.Slice(CoherentSuperblockLayout.InodeModifiedOffset, CoherentCanonicalBinary.UInt32Length)));
    }

    /// <summary>Reconstruit le contenu d'un inode depuis ses blocs directs et indirects.</summary>
    private static byte[] ReadFileData(byte[] image, CoherentInode inode, List<string> warnings, string name)
    {
        if (inode.Size > int.MaxValue) throw CoherentFileSystemExceptions.FileTooLarge(inode.Size);
        // Les nœuds de périphérique stockent leur identifiant dans i_data plutôt que des
        // adresses de blocs. Leur taille nulle interdit de les parcourir comme des fichiers.
        if (inode.Size == 0) return [];
        var requiredBlocks = checked(((int)inode.Size + CoherentSuperblockLayout.BlockSize - 1) / CoherentSuperblockLayout.BlockSize);
        var blocks = new List<int>(requiredBlocks);
        for (var index = 0; index < CoherentSuperblockLayout.DirectPointerCount && blocks.Count < requiredBlocks; index++) blocks.Add(inode.Blocks[index]);
        AddIndirect(image, inode.Blocks[10], 1, blocks, requiredBlocks, warnings, name);
        AddIndirect(image, inode.Blocks[11], 2, blocks, requiredBlocks, warnings, name);
        AddIndirect(image, inode.Blocks[12], 3, blocks, requiredBlocks, warnings, name);
        var result = new byte[checked((int)inode.Size)];
        var destination = 0;
        foreach (var block in blocks)
        {
            if (destination >= result.Length) break;
            var count = Math.Min(CoherentSuperblockLayout.BlockSize, result.Length - destination);
            if (block == 0) { destination += count; continue; }
            var source = block * CoherentSuperblockLayout.BlockSize;
            if (block <= 0 || source + CoherentSuperblockLayout.BlockSize > image.Length) { warnings.Add($"{name}: COHERENT block {block} is outside the image."); continue; }
            image.AsSpan(source, count).CopyTo(result.AsSpan(destination));
            destination += count;
        }
        if (destination < result.Length) warnings.Add($"{name}: {result.Length - destination} byte(s) could not be read.");
        return result;
    }

    /// <summary>Ajoute les blocs référencés par un pointeur indirect du niveau indiqué.</summary>
    private static void AddIndirect(byte[] image, int block, int depth, List<int> result, int requiredBlocks, List<string> warnings, string name)
    {
        if (result.Count >= requiredBlocks) return;
        if (block == 0)
        {
            var capacity = 1;
            for (var index = 0; index < depth; index++) capacity *= CoherentSuperblockLayout.BlockSize / CoherentCanonicalBinary.UInt32Length;
            while (capacity-- > 0 && result.Count < requiredBlocks) result.Add(0);
            return;
        }
        var longOffset = (long)block * CoherentSuperblockLayout.BlockSize;
        if (block <= 0 || longOffset < 0 || longOffset + CoherentSuperblockLayout.BlockSize > image.Length) { warnings.Add(CoherentFileSystemExceptions.IndirectBlockOutsideImage(name, block, depth)); return; }
        var offset = (int)longOffset;
        for (var index = 0; index < CoherentSuperblockLayout.BlockSize && result.Count < requiredBlocks; index += CoherentCanonicalBinary.UInt32Length)
        {
            var rawChild = CoherentCanonicalBinary.ReadUInt32(image.AsSpan(offset + index, CoherentCanonicalBinary.UInt32Length));
            if (rawChild > image.Length / CoherentSuperblockLayout.BlockSize) { warnings.Add(CoherentFileSystemExceptions.IndirectBlockOutsideImage(name, rawChild, depth)); continue; }
            var child = (int)rawChild;
            if (depth == 1) result.Add(child); else AddIndirect(image, child, depth - 1, result, requiredBlocks, warnings, name);
        }
    }

    /// <summary>Décode un champ ASCII fixe en retirant son remplissage terminal.</summary>
    private static string DecodeFixed(ReadOnlySpan<byte> bytes) => System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ', '\n', '\r');
    /// <summary>Convertit une date Unix et ignore les valeurs hors plage.</summary>
    private static DateTimeOffset? DecodeTime(uint seconds)
    {
        try { return seconds == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(seconds); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    /// <summary>Replace les blocs logiques disponibles dans une image contiguë.</summary>
    private static byte[] Flatten(SectorImage image)
    {
        var bytes = new byte[checked(image.BlockCount * image.BlockSize)];
        for (var block = 0; block < image.BlockCount; block++)
            if (image.TryGetBlock(block, out var sector) && sector.Data.Count == image.BlockSize)
                sector.Data.ToArray().CopyTo(bytes, block * image.BlockSize);
        return bytes;
    }

}
