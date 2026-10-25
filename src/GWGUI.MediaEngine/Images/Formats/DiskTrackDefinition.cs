using System.Globalization;
using GWGUI.MediaEngine.Images.Reading.Decoding.Definitions;

namespace GWGUI.MediaEngine.Images.Formats;

internal sealed record DiskTrackDefinition(string Selection, string Codec, IReadOnlyDictionary<string, string> Parameters)
{
    public int Number(string name, int fallback) => Parameters.TryGetValue(name, out var text)
        ? int.Parse(text, CultureInfo.InvariantCulture) : fallback;

    public bool Matches(int cylinder, int head) => Selection == "*" || Selection.Split(',').Any(range =>
    {
        var parts = range.Split('.');
        if (parts.Length > 1 && int.Parse(parts[1], CultureInfo.InvariantCulture) != head) return false;
        var bounds = parts[0].Split('-');
        var first = int.Parse(bounds[0], CultureInfo.InvariantCulture);
        var last = bounds.Length == 1 ? first : int.Parse(bounds[1], CultureInfo.InvariantCulture);
        return cylinder >= first && cylinder <= last;
    });
}

internal sealed record DiskDefinition(string Id, int Cylinders, int Heads, IReadOnlyList<DiskTrackDefinition> Tracks)
{
    public DiskTrackDefinition? Track(int cylinder, int head)
        => Tracks.LastOrDefault(track => track.Selection != "*" && track.Matches(cylinder, head))
           ?? Tracks.FirstOrDefault(track => track.Selection == "*");

    public IReadOnlyList<DefinedDiskSector> Layout()
    {
        var sectors = new List<DefinedDiskSector>();
        for (var cylinder = 0; cylinder < Cylinders; cylinder++)
        for (var head = 0; head < Heads; head++)
        {
            var track = Track(cylinder, head);
            if (track is null) continue;
            var sizes = new List<int>();
            var defaultSize = track.Codec switch
            {
                "apple2.gcr" => AppleIIGcrFormat.SectorSize,
                "datageneral" => DataGeneralFmFormat.SectorSize,
                "hp.mmfm" => HpMmfmFormat.SectorSize,
                "micropolis" => track.Number("img_bps", MicropolisMfmFormat.SectorSize),
                "northstar" when track.Parameters.GetValueOrDefault("mode") == "fm" => 256,
                _ => 512
            };
            var expression = track.Parameters.GetValueOrDefault("bps", defaultSize.ToString(CultureInfo.InvariantCulture));
            foreach (var term in expression.Split(','))
            {
                var parts = term.Split('*');
                var size = int.Parse(parts[0], CultureInfo.InvariantCulture);
                var count = parts.Length == 1 ? 1 : int.Parse(parts[1], CultureInfo.InvariantCulture);
                sizes.AddRange(Enumerable.Repeat(size, count));
            }
            var physicalNumbers = track.Codec == "apple2.gcr"
                ? track.Parameters["secs"].Split(',').Select(number => int.Parse(number, CultureInfo.InvariantCulture)).ToArray() : null;
            var countOfSectors = physicalNumbers?.Length ?? track.Number("secs", track.Codec == "datageneral" ? 8 : 0);
            var firstSector = track.Number("id", track.Codec is "ibm.fm" or "ibm.mfm" ? 1 : 0);
            for (var index = 0; index < countOfSectors; index++)
            {
                var size = sizes[index % sizes.Count];
                sectors.Add(new(sectors.Count, cylinder, head, physicalNumbers?[index] ?? firstSector + index,
                    size, track.Number("img_bps", size), track));
            }
        }
        return sectors;
    }
}

internal sealed record DefinedDiskSector(int LogicalBlock, int Cylinder, int Head, int Number,
    int Size, int ImageSize, DiskTrackDefinition Track);
