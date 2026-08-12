using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Parcourt récursivement les entrées de répertoire FAT12.</summary>
internal static class Fat12DirectoryReader
{
    /// <summary>Profondeur maximale autorisée.</summary>
    public const int MaximumDepth = 64;

    /// <summary>Lit un répertoire et place les répertoires avant les fichiers.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(SectorImage image, FatSectorRange directory, FatSectorRange fat, Fat12Layout layout, List<string> warnings, int depth, string path)
    {
        if (depth > MaximumDepth)
        {
            warnings.Add(Fat12FileSystemExceptions.DepthLimit(path, depth));
            return [];
        }
        var entries = new List<FileSystemEntry>();
        for (var offset = 0; offset + FatDirectoryLayout.EntrySize <= directory.Bytes.Length; offset += FatDirectoryLayout.EntrySize)
        {
            var first = directory.Bytes[offset];
            if (first == FatDirectoryLayout.EndMarker) break;
            if (first == FatDirectoryLayout.DeletedMarker) continue;
            var attributes = (FatDirectoryAttributes)directory.Bytes[offset + FatDirectoryLayout.AttributesOffset];
            if ((attributes & FatDirectoryLayout.LongFileName) == FatDirectoryLayout.LongFileName || attributes.HasFlag(FatDirectoryAttributes.VolumeLabel)) continue;
            var name = FatDirectoryEntryReader.DecodeName(directory.Bytes.AsSpan(offset, FatDirectoryLayout.NameLength + FatDirectoryLayout.ExtensionLength));
            if (name is FatDirectoryLayout.CurrentDirectoryName or FatDirectoryLayout.ParentDirectoryName || name.Length == 0) continue;
            entries.Add(ReadEntry(image, directory, fat, layout, warnings, depth, path, offset, name, attributes));
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit les métadonnées puis le contenu ou les enfants d'une entrée unique.</summary>
    private static FileSystemEntry ReadEntry(SectorImage image, FatSectorRange directory, FatSectorRange fat, Fat12Layout layout, List<string> warnings, int depth, string path, int offset, string name, FatDirectoryAttributes attributes)
    {
        var cluster = BinaryPrimitives.ReadUInt16LittleEndian(directory.Bytes.AsSpan(offset + FatDirectoryLayout.FirstClusterOffset));
        var size = BinaryPrimitives.ReadUInt32LittleEndian(directory.Bytes.AsSpan(offset + FatDirectoryLayout.FileSizeOffset));
        var isDirectory = attributes.HasFlag(FatDirectoryAttributes.Directory);
        var chain = Fat12ClusterChainReader.Read(image, fat, layout, cluster, warnings, name);
        IReadOnlyList<FileSystemEntry> children = [];
        IReadOnlyList<byte>? content = null;
        var valid = directory.IsValid && chain.IsValid;
        if (isDirectory)
        {
            var sectors = new FatSectorRange(chain.Content.ToArray(), Enumerable.Repeat(chain.IsValid, Math.Max(1, chain.Content.Count / FatBootSectorLayout.SectorSize)).ToArray());
            children = Read(image, sectors, fat, layout, warnings, depth + 1, CombinePath(path, name));
        }
        else
        {
            var available = Math.Min((long)chain.Content.Count, size);
            content = chain.Content.Take(checked((int)available)).ToArray();
            if (size > chain.Content.Count)
            {
                warnings.Add(Fat12FileSystemExceptions.IncompleteContent(name, size, chain.Content.Count));
                valid = false;
            }
        }
        var modifiedDate = BinaryPrimitives.ReadUInt16LittleEndian(directory.Bytes.AsSpan(offset + FatDirectoryLayout.ModifiedDateOffset));
        var modifiedTime = BinaryPrimitives.ReadUInt16LittleEndian(directory.Bytes.AsSpan(offset + FatDirectoryLayout.ModifiedTimeOffset));
        return new(name, isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, size, FatDateTime.Decode(modifiedDate, modifiedTime), string.Empty, (uint)attributes, cluster, valid, children, content);
    }

    /// <summary>Concatène deux segments d'un chemin technique FAT.</summary>
    private static string CombinePath(string path, string name) => path.Length == 0 ? name : path + "/" + name;
}
