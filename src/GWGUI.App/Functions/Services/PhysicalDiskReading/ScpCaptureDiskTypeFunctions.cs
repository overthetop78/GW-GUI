using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Functions.Services.PhysicalDiskReading;

public static class ScpCaptureDiskTypeFunctions
{
    public static ScpDiskType Resolve(string density) => density.ToUpperInvariant() switch
    {
        "HD" or "ED" => ScpDiskType.Other1440,
        _ => ScpDiskType.Other720
    };
}
