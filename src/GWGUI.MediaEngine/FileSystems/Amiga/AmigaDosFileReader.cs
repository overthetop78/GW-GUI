using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Reconstruit les fichiers OFS et FFS depuis leurs blocs de données et d'extension.</summary>
public static class AmigaDosFileReader
{
    /// <summary>Lit le contenu déclaré et indique si toutes les structures nécessaires sont valides.</summary>
    public static AmigaDosFileData Read(SectorImage image, ReadOnlySpan<byte> header, int size, AmigaDosVariant variant, ICollection<string> warnings)
    {
        var output = new List<byte>(size);
        var metadata = header.ToArray();
        var extensionVisited = new HashSet<int>();
        var valid = true;
        while (true)
        {
            var highSequence = Math.Clamp(BigEndianInt32.Read(metadata, AmigaDosLayout.HighSequenceOffset), 0, AmigaDosLayout.RootHashTableEntryCount);
            for (var index = 0; index < highSequence && output.Count < size; index++)
            {
                var pointerOffset = AmigaDosLayout.DataPointersOffset + (AmigaDosLayout.RootHashTableEntryCount - 1 - index) * AmigaDosLayout.WordSize;
                var dataBlock = BigEndianInt32.Read(metadata, pointerOffset);
                if (dataBlock <= 0 || dataBlock >= image.BlockCount || !image.TryGetBlock(dataBlock, out var sector))
                {
                    warnings.Add(AmigaDosWarnings.MissingFileData(dataBlock));
                    valid = false;
                    continue;
                }
                var data = sector.Data.ToArray();
                if (variant.IsFastFileSystem())
                {
                    output.AddRange(data.Take(Math.Min(data.Length, size - output.Count)));
                    continue;
                }
                var observedType = BigEndianInt32.Read(data, AmigaDosLayout.PrimaryTypeOffset);
                if (observedType != AmigaDosLayout.OfsDataPrimaryType)
                {
                    warnings.Add(AmigaDosWarnings.UnexpectedOfsDataType(dataBlock, observedType));
                    valid = false;
                }
                if (!AmigaDosChecksum.IsValid(data))
                {
                    warnings.Add(AmigaDosWarnings.InvalidOfsDataChecksum(dataBlock));
                    valid = false;
                    continue;
                }
                var length = Math.Clamp(BigEndianInt32.Read(data, AmigaDosLayout.HashTableSizeOffset), 0, AmigaDosLayout.OfsDataMaximumLength);
                output.AddRange(data.Skip(AmigaDosLayout.OfsDataHeaderLength).Take(Math.Min(length, size - output.Count)));
            }
            var extension = BigEndianInt32.Read(metadata, AmigaDosLayout.ExtensionBlockOffset);
            if (extension == 0) break;
            if (extension < 0 || extension >= image.BlockCount || !extensionVisited.Add(extension) || !image.TryGetBlock(extension, out var extensionBlock))
            {
                warnings.Add(AmigaDosWarnings.InvalidExtension(extension));
                valid = false;
                break;
            }
            metadata = extensionBlock.Data.ToArray();
            if (BigEndianInt32.Read(metadata, AmigaDosLayout.PrimaryTypeOffset) != AmigaDosLayout.HeaderPrimaryType || !AmigaDosChecksum.IsValid(metadata))
            {
                warnings.Add(AmigaDosWarnings.InvalidExtensionChecksum(extension));
                valid = false;
                break;
            }
        }
        if (output.Count < size)
        {
            warnings.Add(AmigaDosWarnings.TruncatedFile(size, output.Count));
            valid = false;
        }
        return new(output.Take(size).ToArray(), valid);
    }
}
