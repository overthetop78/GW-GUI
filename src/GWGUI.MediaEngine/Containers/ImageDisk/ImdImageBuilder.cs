using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Construit une représentation ImageDisk depuis une image Epson reconstruite.</summary>
public static class ImdImageBuilder
{
    /// <summary>Crée une piste IMD par cylindre et face en conservant les secteurs absents et invalides.</summary>
    public static ImdImage BuildEpson(SectorImage image, ImdMode mode = ImdMode.Mfm250Kbps)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(image.FormatId);
        if (image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads) throw new InvalidDataException("The Epson image geometry does not match its format identifier.");
        var blocks = image.AvailableBlocks.ToDictionary(block => block.Address);
        var tracks = new List<ImdTrack>();
        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                if (track.Count == 0) continue;
                var sectors = new List<ImdSector>(track.Count);
                for (var index = 0; index < track.Count; index++)
                {
                    var address = new SectorAddress(cylinder, head, track.FirstSector + index);
                    if (!blocks.TryGetValue(address, out var block))
                    {
                        sectors.Add(new(checked((byte)cylinder), checked((byte)head), checked((byte)address.Number), track.SectorSize, ImdSectorRecordType.Unavailable, new byte[track.SectorSize]));
                        continue;
                    }
                    if (block.Data.Count != track.SectorSize) throw new InvalidDataException($"Epson sector {cylinder}:{head}:{address.Number} has an invalid size.");
                    var compressed = block.Data.All(value => value == block.Data[0]);
                    var type = SelectRecordType(compressed, block.IntegrityValid != false);
                    sectors.Add(new(checked((byte)cylinder), checked((byte)head), checked((byte)address.Number), track.SectorSize, type, block.Data.ToArray()));
                }
                tracks.Add(new(mode, checked((byte)cylinder), checked((byte)head), sectors));
            }
        }
        return new(ImdFormat.DefaultComment, tracks, image);
    }

    private static ImdSectorRecordType SelectRecordType(bool compressed, bool valid) => (compressed, valid) switch
    {
        (true, true) => ImdSectorRecordType.Compressed,
        (false, true) => ImdSectorRecordType.Normal,
        (true, false) => ImdSectorRecordType.CompressedWithError,
        _ => ImdSectorRecordType.NormalWithError
    };
}
