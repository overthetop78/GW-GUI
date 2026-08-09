using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record DiskImageMetadata(string SystemName, string? ProtectionName)
{
    public static DiskImageMetadata From(SectorImage image, IEnumerable<string>? detectedFormatIds = null)
    {
        var ids = new[] { image.FormatId }.Concat(detectedFormatIds ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var systems = ids.Select(DiskSystemCatalog.NameFor).Where(value => value != "—").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(
            systems.Length == 0 ? "—" : string.Join(" + ", systems),
            DiskProtectionCatalog.NameFor(ids));
    }
}
