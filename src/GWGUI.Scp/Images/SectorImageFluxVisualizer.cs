using GWGUI.Scp.Encoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class SectorImageFluxVisualizer(FluxEncoderRegistry? encoders = null)
{
    private readonly FluxEncoderRegistry _encoders = encoders ?? new FluxEncoderRegistry();

    public bool CanVisualize(SectorImage image) => EncoderIdFor(image) is not null;

    public ScpImage Create(SectorImage image, CancellationToken cancellationToken = default)
    {
        var encoderId = EncoderIdFor(image) ?? throw new NotSupportedException($"No track encoder is available for '{image.FormatId}'.");
        var tracks = new List<ScpTrack>();
        foreach (var group in image.AvailableBlocks
                     .Select(block => (Block: block, Address: VisualAddress(image, block.Address)))
                     .GroupBy(item => (item.Address.Cylinder, item.Address.Head))
                     .OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectors = CreateTrackSectors(image, group.OrderBy(item => item.Address.Number).ToArray());
            if (sectors.Count == 0) continue;
            var attributes = TrackAttributes(image, sectors.Count);
            var encoded = _encoders.Encode(encoderId,
                new TrackEncodeRequest(group.Key.Cylinder, group.Key.Head, sectors, attributes,
                    BitCellTicks(image, group.Key.Cylinder)));
            var trackNumber = checked((byte)(group.Key.Cylinder * 2 + group.Key.Head));
            tracks.Add(new(trackNumber, group.Key.Cylinder, group.Key.Head, [encoded.Revolution]));
        }
        if (tracks.Count == 0) throw new InvalidDataException("The sector image contains no track that can be visualized.");
        var start = tracks.Min(track => track.TrackNumber);
        var end = tracks.Max(track => track.TrackNumber);
        var heads = (byte)(tracks.Select(track => track.Head).Distinct().Count() == 1 ? tracks[0].Head + 1 : 0);
        var header = new ScpHeader(0, 0, 1, start, end, ScpFlags.IndexAligned | ScpFlags.Writable,
            0, heads, 0, 0);
        return new(header, tracks, true, image.Capacity);
    }

    internal static string? EncoderIdFor(SectorImage image)
    {
        var id = image.FormatId;
        if (id.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) return "amiga.mfm";
        if (id.StartsWith("commodore.1581", StringComparison.OrdinalIgnoreCase)) return "iso.mfm";
        if (id.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase)) return "commodore.gcr";
        if (id.Equals("apple2.rwts18", StringComparison.OrdinalIgnoreCase)) return "apple2.rwts18";
        if (id.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase) && image.BlockSize == 512 && image.Cylinders >= 80) return "applemac.gcr";
        if (id.StartsWith("apple2.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase)) return "apple2.gcr";
        if (id.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase) && image.Cylinders == 46 && image.Heads == 2) return "applelisa.fileware.gcr";
        if (id.StartsWith("applemac.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase)) return "applemac.gcr";
        if (id.StartsWith("mac.", StringComparison.OrdinalIgnoreCase)) return id.Equals("mac.1440", StringComparison.OrdinalIgnoreCase) ? "iso.mfm" : "applemac.gcr";
        if (id.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase)) return "dec.rx02";
        if (id.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase) || id.Equals("atari.90", StringComparison.OrdinalIgnoreCase)) return "iso.fm";
        if (id.StartsWith("acorn.adfs.", StringComparison.OrdinalIgnoreCase)) return "iso.mfm";
        if (id.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)
            || id.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase)
            || id.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("ucsd.", StringComparison.OrdinalIgnoreCase)
            || id.StartsWith("epson.", StringComparison.OrdinalIgnoreCase)
            || id is "imd" or "td0") return "iso.mfm";
        if (id.StartsWith("commodore900.", StringComparison.OrdinalIgnoreCase)) return "commodore900.gcr";
        return null;
    }

    private static IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if (image.FormatId.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase))
        {
            var sectors = new List<TrackSector>(items.Count * 2);
            foreach (var item in items)
            {
                if (item.Block.Data.Count < 512) continue;
                var first = (item.Address.Number - 1) * 2 + 1;
                sectors.Add(new(first, item.Block.Data.Take(256).ToArray(), SizeCode: 1));
                sectors.Add(new(first + 1, item.Block.Data.Skip(256).Take(256).ToArray(), SizeCode: 1));
            }
            return sectors;
        }
        if (image.FormatId.StartsWith("commodore.1581", StringComparison.OrdinalIgnoreCase))
            return items.Select(item => item.Block).GroupBy(block => block.LogicalBlock / 2).OrderBy(group => group.Key).Select(group =>
            {
                var halves = group.OrderBy(block => block.LogicalBlock).ToArray();
                var data = halves.SelectMany(block => block.Data).Take(512).ToArray();
                return new TrackSector(group.Key % 10 + 1, data, SizeCode: 2);
            }).Where(sector => sector.Data.Count == 512).ToArray();
        if ((image.FormatId.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase)
             || image.FormatId.Equals("apple3.sos", StringComparison.OrdinalIgnoreCase))
            && image.Cylinders < 80)
        {
            var sectors = new List<TrackSector>(items.Count * 2);
            foreach (var block in items.Select(item => item.Block))
            {
                if (block.Data.Count < 512) continue;
                sectors.Add(new(block.Address.Number * 2, block.Data.Take(256).ToArray()));
                sectors.Add(new(block.Address.Number * 2 + 1, block.Data.Skip(256).Take(256).ToArray()));
            }
            return sectors;
        }
        return items.Select(item => new TrackSector(item.Address.Number, item.Block.Data,
            SizeCode: SizeCode(item.Block.Data.Count), Attributes: TagAttributes(item.Block.Tag))).ToArray();
    }

    private static SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) && image.SectorsPerTrack == 1 && image.Cylinders > 80)
        {
            var sectorsPerTrack = image.FormatId.Equals("atari.130", StringComparison.OrdinalIgnoreCase) ? 26 : 18;
            var logical = address.Cylinder;
            return new(logical / sectorsPerTrack, 0, logical % sectorsPerTrack + 1);
        }
        if (image.FormatId.StartsWith("commodore.1581", StringComparison.OrdinalIgnoreCase))
        {
            var logical = address.Cylinder * image.SectorsPerTrack + address.Number;
            var physical = logical / 2;
            return new(physical / 20, physical % 20 / 10, physical % 10 + 1);
        }
        if (image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase) && image.Heads == 1 && image.Cylinders > 84)
            return new(address.Cylinder / 2, address.Cylinder % 2, address.Number);
        return address;
    }

    private static IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount)
    {
        if (image.FormatId.StartsWith("apple2.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int>
            {
                ["sectorsPerTrack"] = sectorCount,
                ["format"] = image.Cylinders >= 80 ? 0x24 : 0
            };
        if (image.FormatId.StartsWith("applemac.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = image.Heads == 1 ? 0x02 : 0x22 };
        if (image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = 0x12 };
        return null;
    }

    private static uint BitCellTicks(SectorImage image, int cylinder)
    {
        if (!image.FormatId.StartsWith("commodore900.", StringComparison.OrdinalIgnoreCase)) return 40;
        return cylinder switch { < 39 => 86, < 53 => 93, < 64 => 100, _ => 106 };
    }

    private static IReadOnlyDictionary<string, int>? TagAttributes(IReadOnlyList<byte>? tag)
    {
        if (tag is null || tag.Count == 0) return null;
        return tag.Select((value, index) => (Key: $"tag{index}", Value: (int)value)).ToDictionary(item => item.Key, item => item.Value);
    }

    private static byte? SizeCode(int size) => size switch { 128 => 0, 256 => 1, 512 => 2, 1024 => 3, 2048 => 4, 4096 => 5, 8192 => 6, 16384 => 7, _ => null };
}
