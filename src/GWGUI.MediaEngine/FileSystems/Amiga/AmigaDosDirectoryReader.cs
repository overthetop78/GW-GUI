using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Parcourt les tables de hachage et sous-répertoires AmigaDOS.</summary>
public static class AmigaDosDirectoryReader
{
    /// <summary>Lit les entrées d'un répertoire dans l'ordre répertoire puis nom sans casse.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(SectorImage image, ReadOnlySpan<byte> directory, int hashSize, AmigaDosVariant variant, HashSet<int> visited, List<string> warnings, int depth)
    {
        if (depth > AmigaDosLayout.MaximumDirectoryDepth)
        {
            warnings.Add(AmigaDosWarnings.DirectoryDepthExceeded(depth));
            return [];
        }
        var entries = new List<FileSystemEntry>();
        for (var index = 0; index < hashSize; index++)
        {
            var blockNumber = BigEndianInt32.Read(directory, AmigaDosLayout.DataPointersOffset + index * AmigaDosLayout.WordSize);
            var chain = new HashSet<int>();
            while (blockNumber != 0)
            {
                if (blockNumber < 0 || blockNumber >= image.BlockCount || !chain.Add(blockNumber))
                {
                    warnings.Add(AmigaDosWarnings.InvalidDirectoryChain(blockNumber));
                    break;
                }
                if (!image.TryGetBlock(blockNumber, out var sector))
                {
                    warnings.Add(AmigaDosWarnings.MissingDirectoryEntry(blockNumber));
                    break;
                }
                var block = sector.Data.ToArray();
                var next = BigEndianInt32.Read(block, AmigaDosLayout.HashChainOffset);
                if (!visited.Add(blockNumber))
                {
                    blockNumber = next;
                    continue;
                }
                var entryType = AmigaDosEntryTypeExtensions.FromRaw(BigEndianInt32.Read(block, AmigaDosLayout.SecondaryTypeOffset));
                var kind = entryType.ToCommonKind();
                var name = AmigaDosNameCodec.ReadEntryName(block, variant);
                var children = kind == FileSystemEntryKind.Directory ? Read(image, block, AmigaDosLayout.RootHashTableEntryCount, variant, visited, warnings, depth + 1) : [];
                var size = kind == FileSystemEntryKind.File ? BigEndianInt32.ReadUnsigned(block, AmigaDosLayout.FileSizeOffset) : 0;
                IReadOnlyList<byte>? content = null;
                var metadataValid = AmigaDosChecksum.IsValid(block);
                if (kind == FileSystemEntryKind.File)
                {
                    try
                    {
                        var file = AmigaDosFileReader.Read(image, block, checked((int)size), variant, warnings);
                        content = file.Content;
                        metadataValid &= file.IsValid;
                    }
                    catch (Exception exception) when (exception is InvalidDataException or OverflowException or ArgumentOutOfRangeException)
                    {
                        warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception));
                        metadataValid = false;
                    }
                }
                entries.Add(new(name, kind, size, AmigaDosTime.Read(block, AmigaDosLayout.DateOffset), AmigaDosNameCodec.Read(block, AmigaDosLayout.LongNameOffset, AmigaDosLayout.CommentMaximumLength), BigEndianInt32.ReadUnsigned(block, AmigaDosLayout.ProtectionOffset), blockNumber, metadataValid, children, content));
                blockNumber = next;
            }
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
