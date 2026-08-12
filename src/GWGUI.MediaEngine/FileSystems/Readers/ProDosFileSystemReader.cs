using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.FileSystems.ProDos;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les volumes et fichiers ProDOS et SOS.</summary>
public sealed class ProDosFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.ProDos;
    /// <summary>Formats sectoriels pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleIIProDos, DiskImageFormatIds.AppleIIProDos140,
            DiskImageFormatIds.AppleIIProDos800, DiskImageFormatIds.AppleIIISos };

    /// <summary>Indique si l'image contient un en-tête de volume ProDOS valide.</summary>
    public bool CanRead(SectorImage image)
    {
        return image.BlockSize == ProDosVolumeHeader.BlockSize && image.TryGetBlock(ProDosVolumeHeader.BlockNumber, out var root) && root.Data.Count == ProDosVolumeHeader.BlockSize && ProDosVolumeHeader.IsValid(root.Data.ToArray());
    }

    /// <summary>Lit le volume ProDOS ou SOS.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) { var observed = image.TryGetBlock(ProDosFileSystemLayout.RootBlock, out var candidate) && candidate.Data.Count > ProDosFileSystemLayout.HeaderOffset ? candidate.Data[ProDosFileSystemLayout.HeaderOffset] : (byte)0; throw ProDosFileSystemExceptions.UnsupportedVolume(ProDosFileSystemLayout.RootBlock, observed); }
        var root = image.GetBlock(ProDosFileSystemLayout.RootBlock).Span;
        var name = ReadName(root, ProDosFileSystemLayout.HeaderOffset);
        var bitmap = ReadU16(root, ProDosFileSystemLayout.BitmapBlockOffset);
        var total = ReadU16(root, ProDosFileSystemLayout.TotalBlocksOffset);
        var warnings = new List<string>();
        var entries = ReadDirectory(image, ProDosFileSystemLayout.RootBlock, warnings, new HashSet<int>(), 0);
        var free = CountFreeBlocks(image, bitmap, Math.Min(total, image.BlockCount), warnings);
        var system = Definitions.FileSystemDisplayNames.ProDos(image.FormatId);
        return new(name, system, (long)Math.Min(total, image.BlockCount) * ProDosFileSystemLayout.BlockSize, (long)free * ProDosFileSystemLayout.BlockSize, ReadDate(root, ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.CreatedDateOffset), null, entries, warnings);
    }

    /// <summary>Lit récursivement une chaîne de blocs de répertoire.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, int firstBlock, List<string> warnings, HashSet<int> globalVisited, int depth)
    {
        if (depth > ProDosFileSystemLayout.MaximumDirectoryDepth) { warnings.Add(ProDosFileSystemExceptions.DirectoryDepthExceeded(depth, firstBlock)); return []; }
        var entries = new List<FileSystemEntry>(); var blockNumber = firstBlock; var chain = new HashSet<int>(); var first = true;
        while (blockNumber != 0)
        {
            if (!chain.Add(blockNumber) || !globalVisited.Add(blockNumber) || !image.TryGetBlock(blockNumber, out var block)) { warnings.Add(ProDosFileSystemExceptions.InvalidDirectoryBlock(blockNumber)); break; }
            var bytes = block.Data.ToArray();
            var start = first ? ProDosFileSystemLayout.FirstVolumeEntryOffset : ProDosFileSystemLayout.HeaderOffset;
            for (var offset = start; offset + ProDosFileSystemLayout.EntrySize <= ProDosFileSystemLayout.BlockSize; offset += ProDosFileSystemLayout.EntrySize)
            {
                var storage = bytes[offset] >> ProDosFileSystemLayout.StorageTypeShift;
                var nameLength = bytes[offset] & ProDosFileSystemLayout.NameLengthMask;
                if (storage == 0 || nameLength == 0 || nameLength > ProDosFileSystemLayout.MaximumNameLength) continue;
                var entryName = System.Text.Encoding.ASCII.GetString(bytes, offset + ProDosFileSystemLayout.NameOffset, nameLength);
                var key = ReadU16(bytes, offset + ProDosFileSystemLayout.KeyBlockOffset);
                var eof = bytes[offset + ProDosFileSystemLayout.EndOfFileOffset] | bytes[offset + ProDosFileSystemLayout.EndOfFileOffset + 1] << BitPrimitives.BitsPerByte | bytes[offset + ProDosFileSystemLayout.EndOfFileOffset + 2] << 16;
                var fileType = (ProDosFileType)bytes[offset + ProDosFileSystemLayout.FileTypeOffset];
                if (storage == ProDosFileSystemLayout.SubdirectoryStorageType)
                {
                    var children = ReadDirectory(image, key, warnings, globalVisited, depth + 1);
                    entries.Add(new(entryName, FileSystemEntryKind.Directory, 0, ReadDate(bytes, offset + ProDosFileSystemLayout.ModifiedDateOffset), ProDosFileTypeNames.Get(fileType), bytes[offset + ProDosFileSystemLayout.AccessOffset], key, true, children));
                }
                else if (storage is >= ProDosFileSystemLayout.SeedlingStorageType and <= ProDosFileSystemLayout.TreeStorageType)
                {
                    var content = ReadFile(image, storage, key, eof, warnings, entryName);
                    entries.Add(new(entryName, FileSystemEntryKind.File, eof, ReadDate(bytes, offset + ProDosFileSystemLayout.ModifiedDateOffset), ProDosFileTypeNames.Get(fileType), bytes[offset + ProDosFileSystemLayout.AccessOffset], key, true, [], content));
                }
            }
            blockNumber = ReadU16(bytes, ProDosFileSystemLayout.NextBlockOffset); first = false;
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Reconstruit un fichier seedling, sapling ou tree.</summary>
    private static IReadOnlyList<byte> ReadFile(SectorImage image, int storage, int key, int length, List<string> warnings, string name)
    {
        var blocks = new List<int>();
        if (storage == ProDosFileSystemLayout.SeedlingStorageType) blocks.Add(key);
        else if (storage == ProDosFileSystemLayout.SaplingStorageType) ReadIndex(image, key, blocks, warnings, name);
        else
        {
            if (!image.TryGetBlock(key, out var master)) warnings.Add(ProDosFileSystemExceptions.MissingMasterIndexBlock(name, key));
            else for (var index = 0; index < ProDosFileSystemLayout.IndexPointerCount; index++) { var child = Pointer(master.Data, index); if (child != 0) ReadIndex(image, child, blocks, warnings, name); }
        }
        using var output = new MemoryStream();
        foreach (var blockNumber in blocks)
        {
            if (!image.TryGetBlock(blockNumber, out var block)) { warnings.Add(ProDosFileSystemExceptions.MissingDataBlock(name, blockNumber)); output.Write(new byte[ProDosFileSystemLayout.BlockSize]); }
            else output.Write(block.Data.ToArray());
            if (output.Length >= length) break;
        }
        return output.ToArray().Take(length).ToArray();
    }

    /// <summary>Lit les pointeurs contenus dans un bloc d'index.</summary>
    private static void ReadIndex(SectorImage image, int blockNumber, List<int> output, List<string> warnings, string name)
    {
        if (!image.TryGetBlock(blockNumber, out var index)) { warnings.Add(ProDosFileSystemExceptions.MissingIndexBlock(name, blockNumber)); return; }
        for (var entry = 0; entry < ProDosFileSystemLayout.IndexPointerCount; entry++) { var pointer = Pointer(index.Data, entry); if (pointer != 0) output.Add(pointer); }
    }

    /// <summary>Lit un pointeur d'index séparé en octets bas et hauts.</summary>
    private static int Pointer(IReadOnlyList<byte> block, int index) => block[index] | block[index + ProDosFileSystemLayout.IndexHighBytesOffset] << BitPrimitives.BitsPerByte;
    /// <summary>Compte les blocs libres dans le bitmap.</summary>
    private static int CountFreeBlocks(SectorImage image, int bitmapStart, int total, List<string> warnings)
    {
        var free = 0;
        for (var block = 0; block < total; block++)
        {
            var mapBlock = bitmapStart + block / ProDosFileSystemLayout.BlocksPerBitmapBlock;
            if (!image.TryGetBlock(mapBlock, out var bitmap)) { if (block % ProDosFileSystemLayout.BlocksPerBitmapBlock == 0) warnings.Add($"Bitmap block {mapBlock} is missing."); continue; }
            var bit = block % ProDosFileSystemLayout.BlocksPerBitmapBlock; if ((bitmap.Data[bit / BitPrimitives.BitsPerByte] & (0x80 >> (bit & 7))) != 0) free++;
        }
        return free;
    }

    /// <summary>Lit un entier 16 bits little-endian.</summary>
    private static int ReadU16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
    /// <summary>Lit un nom ProDOS.</summary>
    private static string ReadName(ReadOnlySpan<byte> data, int offset) { var len = data[offset] & ProDosFileSystemLayout.NameLengthMask; return System.Text.Encoding.ASCII.GetString(data.Slice(offset + ProDosFileSystemLayout.NameOffset, len)); }
    /// <summary>Décode une date ProDOS.</summary>
    private static DateTimeOffset? ReadDate(ReadOnlySpan<byte> data, int offset) { if (offset + 4 > data.Length) return null; var date = ReadU16(data, offset); var time = ReadU16(data, offset + 2); try { var year = 1900 + (date >> 9); if (year < 1940) year += 100; return new DateTimeOffset(year, (date >> 5) & 15, date & 31, time >> BitPrimitives.BitsPerByte, time & 0x3f, 0, TimeSpan.Zero); } catch { return null; } }
}
