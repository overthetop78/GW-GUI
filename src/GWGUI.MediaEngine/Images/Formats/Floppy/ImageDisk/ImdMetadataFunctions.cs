using System.Text.Json;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;

/// <summary>Preserves and restores ImageDisk structure through common media document metadata.</summary>
internal static class ImdMetadataFunctions
{
    public static IReadOnlyDictionary<string, string> Create(ImdImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var structure = new
        {
            image.Comment,
            Tracks = image.Tracks.Select(track => new
            {
                Mode = (byte)track.Mode,
                track.Cylinder,
                track.Head,
                Sectors = track.Sectors.Select(sector => new
                {
                    sector.Cylinder,
                    sector.Head,
                    sector.Number,
                    sector.Size,
                    RecordType = (byte)sector.RecordType
                })
            })
        };
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ImdMetadataKeys.Structure] = JsonSerializer.Serialize(structure)
        };
    }

    public static ImdImage? Restore(
        SectorImage image,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!metadata.TryGetValue(ImdMetadataKeys.Structure, out var serialized)) return null;

        try
        {
            using var document = JsonDocument.Parse(serialized);
            var root = document.RootElement;
            var comment = root.GetProperty("Comment").GetString() ?? string.Empty;
            var blocks = image.AvailableBlocks.ToDictionary(block => block.LogicalBlock);
            var tracks = new List<ImdTrack>();
            var logicalBlock = 0;
            foreach (var trackElement in root.GetProperty("Tracks").EnumerateArray())
            {
                var mode = (ImdMode)trackElement.GetProperty("Mode").GetByte();
                if (!Enum.IsDefined(mode)) throw new InvalidDataException("The ImageDisk metadata contains an invalid track mode.");
                var sectors = new List<ImdSector>();
                foreach (var sectorElement in trackElement.GetProperty("Sectors").EnumerateArray())
                {
                    var recordType = (ImdSectorRecordType)sectorElement.GetProperty("RecordType").GetByte();
                    if (!Enum.IsDefined(recordType)) throw new InvalidDataException("The ImageDisk metadata contains an invalid sector record type.");
                    var size = sectorElement.GetProperty("Size").GetInt32();
                    if (size <= 0) throw new InvalidDataException("The ImageDisk metadata contains an invalid sector size.");
                    IReadOnlyList<byte> data;
                    if (recordType.HasData())
                    {
                        if (!blocks.TryGetValue(logicalBlock, out var block) || block.Data.Count != size)
                            throw new InvalidDataException($"The ImageDisk metadata does not match logical block '{logicalBlock}'.");
                        data = block.Data.ToArray();
                    }
                    else
                    {
                        data = new byte[size];
                    }

                    sectors.Add(new ImdSector(
                        sectorElement.GetProperty("Cylinder").GetByte(),
                        sectorElement.GetProperty("Head").GetByte(),
                        sectorElement.GetProperty("Number").GetByte(),
                        size,
                        recordType,
                        data));
                    logicalBlock++;
                }

                tracks.Add(new ImdTrack(
                    mode,
                    trackElement.GetProperty("Cylinder").GetByte(),
                    trackElement.GetProperty("Head").GetByte(),
                    sectors));
            }

            if (logicalBlock != image.BlockCount)
                throw new InvalidDataException("The ImageDisk metadata block count does not match the sector image.");
            return new ImdImage(comment, tracks, image);
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
        {
            throw new InvalidDataException("The ImageDisk metadata is invalid.", exception);
        }
    }
}
