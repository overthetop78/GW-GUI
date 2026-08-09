namespace GWGUI.Scp.SectorImages;

internal static class IsoScpSectorImagePolicyRegistry
{
    private static readonly IReadOnlyList<(Predicate<string> Matches, Func<string, IIsoScpSectorImagePolicy> Create)> Policies =
    [
        (format => format.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
                   format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase),
            format => new AtariIsoScpSectorImagePolicy(format)),
        (format => format.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase),
            _ => new AmstradIsoScpSectorImagePolicy()),
        (format => format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("mac.1440", StringComparison.OrdinalIgnoreCase),
            _ => new IbmPcIsoScpSectorImagePolicy(true)),
        (format => format.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase),
            _ => new BbcIsoScpSectorImagePolicy()),
        (format => format.StartsWith("epson.qx10.", StringComparison.OrdinalIgnoreCase),
            _ => new EpsonQx10IsoScpSectorImagePolicy()),
        (format => format.Equals("ucsd.ibm.mfm", StringComparison.OrdinalIgnoreCase),
            _ => new UcsdIsoScpSectorImagePolicy())
    ];

    public static IIsoScpSectorImagePolicy Resolve(string? formatId)
    {
        if (formatId is null) return new AutomaticIsoScpSectorImagePolicy();
        var registration = Policies.FirstOrDefault(item => item.Matches(formatId));
        return registration.Create is null ? new GenericIsoScpSectorImagePolicy() : registration.Create(formatId);
    }
}
