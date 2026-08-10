namespace GWGUI.MediaEngine.SectorImages;

internal static class EpsonQx10GeometryCatalog
{
    public static EpsonQx10Geometry Resolve(string formatId) => formatId.ToLowerInvariant() switch
    {
        "epson.qx10.320" => EpsonQx10Geometry.Uniform(40, 2, new(1, 16, 256)),
        "epson.qx10.400" => EpsonQx10Geometry.Uniform(40, 2, new(1, 5, 1024)),
        "epson.qx10.booter" => new(15, 1, (cylinder, _) =>
            cylinder == 0 ? new(1, 16, 256) : new(1, 17, 256)),
        "epson.qx10.399" => new(40, 2, (cylinder, head) =>
            cylinder == 0 && head == 0 ? new(1, 16, 256) : new(1, 10, 512)),
        "epson.qx10.logo" => new(40, 2, (cylinder, _) => cylinder switch
        {
            0 or 1 or 4 => new(1, 16, 256),
            5 or 6 => new(2, 10, 512),
            3 or 7 => default,
            _ => new(1, 10, 512)
        }),
        _ => new(40, 2, (cylinder, _) =>
            cylinder <= 1 ? new(1, 16, 256) : new(1, 10, 512))
    };
}

internal readonly record struct EpsonQx10TrackGeometry(int FirstSector, int Count, int SectorSize);

internal sealed record EpsonQx10Geometry(
    int Cylinders,
    int Heads,
    Func<int, int, EpsonQx10TrackGeometry> Track)
{
    public IEnumerable<EpsonQx10TrackGeometry> AllTracks
    {
        get
        {
            for (var cylinder = 0; cylinder < Cylinders; cylinder++)
                for (var head = 0; head < Heads; head++)
                    yield return Track(cylinder, head);
        }
    }

    public static EpsonQx10Geometry Uniform(
        int cylinders,
        int heads,
        EpsonQx10TrackGeometry track) =>
        new(cylinders, heads, (_, _) => track);
}
