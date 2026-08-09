namespace GWGUI.Domain.Formats;

public sealed record DiskProtection(string Id, string Machine, IReadOnlySet<string> FormatIds, string DisplayName);

public sealed class DiskClassificationCatalog
{
    public DiskClassificationCatalog(IEnumerable<DiskFormat> formats)
    {
        Formats = formats.Where(format => format.Id != "raw.scp").ToArray();
        Machines = Formats.Select(format => format.Family).Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase).ToArray();
        Protections =
        [
            new DiskProtection("apple2.rwts18", "Apple II",
                new HashSet<string>(["apple2.appledos.140"], StringComparer.OrdinalIgnoreCase),
                "Brøderbund RWTS18")
        ];
    }

    public IReadOnlyList<DiskFormat> Formats { get; }
    public IReadOnlyList<string> Machines { get; }
    public IReadOnlyList<DiskProtection> Protections { get; }

    public DiskFormat? ResolveFormat(string? detectedId)
    {
        if (string.IsNullOrWhiteSpace(detectedId)) return null;
        var exact = Formats.FirstOrDefault(format => format.Id.Equals(detectedId, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact;
        var alias = detectedId.ToLowerInvariant() switch
        {
            "apple2.dos33" or "apple2.rwts18" => "apple2.appledos.140",
            "apple2.dos32" => "apple2.appledos.113",
            "apple2.prodos" => "apple2.prodos.140",
            "applemac.mfs" => "mac.400",
            "applemac.hfs" => "mac.800",
            _ => null
        };
        return alias is null ? null : Formats.FirstOrDefault(format => format.Id.Equals(alias, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<DiskFormat> FormatsFor(string? machine) => Formats
        .Where(format => format.Family.Equals(machine, StringComparison.OrdinalIgnoreCase)).ToArray();

    public IReadOnlyList<DiskProtection> ProtectionsFor(string? machine, string? formatId) => Protections
        .Where(protection => protection.Machine.Equals(machine, StringComparison.OrdinalIgnoreCase)
            && formatId is not null && protection.FormatIds.Contains(formatId)).ToArray();
}
