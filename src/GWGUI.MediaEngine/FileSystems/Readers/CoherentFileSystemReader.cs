using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit en lecture seule le système de fichiers COHERENT de type V7 utilisé par le Commodore 900.</summary>
public sealed class CoherentFileSystemReader : IFileSystemReader
{
    /// <summary>Taille d'un inode COHERENT en octets.</summary>
    private const int InodeSize = 64;
    /// <summary>Mode identifiant un répertoire.</summary>
    private const ushort DirectoryMode = 0x4000;
    /// <summary>Masque isolant le type d'un inode.</summary>
    private const ushort TypeMask = 0xf000;

    /// <summary>Obtient l'identifiant du lecteur.</summary>
    public string Id => "coherent";
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
        if (inodeZoneEnd < 3 || inodeZoneEnd > fileSystemBlocks) throw new InvalidDataException("The COHERENT inode zone is invalid.");

        var warnings = new List<string>();
        var visited = new HashSet<ushort>();
        var entries = ReadDirectory(bytes, 2, visited, warnings);
        var volumeName = DecodeFixed(bytes.AsSpan(CoherentSuperblockLayout.VolumeNameOffset, CoherentSuperblockLayout.NameLength));
        if (volumeName is CoherentSuperblockLayout.PlaceholderName or CoherentSuperblockLayout.DefaultVolumeName) volumeName = string.Empty;
        var freeBytes = (long)CoherentCanonicalBinary.ReadUInt32(bytes.AsSpan(CoherentSuperblockLayout.FreeBlockCountOffset, CoherentCanonicalBinary.UInt32Length)) * CoherentSuperblockLayout.BlockSize;
        var modified = DecodeTime(CoherentCanonicalBinary.ReadUInt32(bytes.AsSpan(CoherentSuperblockLayout.ModifiedTimeOffset, CoherentCanonicalBinary.UInt32Length)));
        return new(volumeName, "COHERENT (Commodore 900)", (long)fileSystemBlocks * CoherentSuperblockLayout.BlockSize, Math.Clamp(freeBytes, 0, (long)fileSystemBlocks * CoherentSuperblockLayout.BlockSize), null, modified, entries, warnings);
    }

    /// <summary>Lit récursivement un répertoire en empêchant les cycles d'inodes.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(byte[] image, ushort inodeNumber, HashSet<ushort> visited, List<string> warnings)
    {
        if (!visited.Add(inodeNumber)) return [];
        var inode = ReadInode(image, inodeNumber);
        var data = ReadFileData(image, inode, warnings, $"inode {inodeNumber}");
        var result = new List<FileSystemEntry>();
        for (var offset = 0; offset + 16 <= data.Length; offset += 16)
        {
            var childNumber = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
            if (childNumber == 0) continue;
            var name = DecodeFixed(data.AsSpan(offset + 2, 14));
            if (name.Length == 0 || name is "." or "..") continue;
            try
            {
                var child = ReadInode(image, childNumber);
                var directory = (child.Mode & TypeMask) == DirectoryMode;
                var content = directory ? null : ReadFileData(image, child, warnings, name);
                var children = directory ? ReadDirectory(image, childNumber, visited, warnings) : [];
                result.Add(new(name, directory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                    child.Size, DecodeTime(child.Modified), $"COHERENT inode {childNumber}", (uint)(child.Mode & 0x0fff),
                    childNumber, true, children, content));
            }
            catch (InvalidDataException exception)
            {
                warnings.Add($"{name}: {exception.Message}");
            }
        }
        return result.OrderByDescending(entry => entry.Kind == FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit l'inode demandé et ses treize pointeurs de blocs.</summary>
    private static Inode ReadInode(byte[] image, ushort number)
    {
        if (number == 0) throw new InvalidDataException("Invalid COHERENT inode 0.");
        var offset = CoherentSuperblockLayout.BlockSize * 2 + (number - 1) * InodeSize;
        if (offset < 0 || offset + InodeSize > image.Length) throw new InvalidDataException($"COHERENT inode {number} is outside the image.");
        var value = image.AsSpan(offset, InodeSize);
        var pointers = new int[13];
        for (var index = 0; index < pointers.Length; index++)
        {
            var item = value.Slice(12 + index * 3, 3);
            pointers[index] = item[1] | item[2] << BitPrimitives.BitsPerByte | item[0] << 16;
        }
        return new(BinaryPrimitives.ReadUInt16LittleEndian(value), CoherentCanonicalBinary.ReadUInt32(value.Slice(8, CoherentCanonicalBinary.UInt32Length)), pointers, CoherentCanonicalBinary.ReadUInt32(value.Slice(56, CoherentCanonicalBinary.UInt32Length)));
    }

    /// <summary>Reconstruit le contenu d'un inode depuis ses blocs directs et indirects.</summary>
    private static byte[] ReadFileData(byte[] image, Inode inode, List<string> warnings, string name)
    {
        if (inode.Size > int.MaxValue) throw new InvalidDataException("The file is too large.");
        // Les nœuds de périphérique stockent leur identifiant dans i_data plutôt que des
        // adresses de blocs. Leur taille nulle interdit de les parcourir comme des fichiers.
        if (inode.Size == 0) return [];
        var requiredBlocks = checked(((int)inode.Size + CoherentSuperblockLayout.BlockSize - 1) / CoherentSuperblockLayout.BlockSize);
        var blocks = new List<int>(requiredBlocks);
        for (var index = 0; index < 10 && blocks.Count < requiredBlocks; index++) blocks.Add(inode.Blocks[index]);
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
        if (block <= 0 || longOffset < 0 || longOffset + CoherentSuperblockLayout.BlockSize > image.Length) { warnings.Add($"{name}: indirect COHERENT block {block} is outside the image."); return; }
        var offset = (int)longOffset;
        for (var index = 0; index < CoherentSuperblockLayout.BlockSize && result.Count < requiredBlocks; index += CoherentCanonicalBinary.UInt32Length)
        {
            var rawChild = CoherentCanonicalBinary.ReadUInt32(image.AsSpan(offset + index, CoherentCanonicalBinary.UInt32Length));
            if (rawChild > image.Length / CoherentSuperblockLayout.BlockSize) { warnings.Add($"{name}: indirect COHERENT block {rawChild} is outside the image."); continue; }
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

    /// <summary>Regroupe les champs d'inode utilisés pendant l'exploration.</summary>
    /// <param name="Mode">Mode et droits de l'inode.</param>
    /// <param name="Size">Taille logique en octets.</param>
    /// <param name="Blocks">Pointeurs directs et indirects.</param>
    /// <param name="Modified">Date de modification Unix.</param>
    private sealed record Inode(ushort Mode, uint Size, IReadOnlyList<int> Blocks, uint Modified);
}
