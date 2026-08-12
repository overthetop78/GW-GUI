using GWGUI.MediaEngine.Definitions;

using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;

namespace GWGUI.MediaEngine.SectorImages;

internal static class IsoScpSectorImagePolicyRegistry
{
    private static readonly IReadOnlyList<(Predicate<string> Matches, Func<string, IIsoScpSectorImagePolicy> Create)> Policies =
    [
        (format => format.StartsWith("atari.", StringComparison.OrdinalIgnoreCase),
            format => new Atari8BitIsoScpSectorImagePolicy(format)),
        (format => format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase),
            _ => new AtariStIsoScpSectorImagePolicy()),
        (format => format.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase),
            _ => new AmstradIsoScpSectorImagePolicy()),
        (format => format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase),
            _ => new IbmPcIsoScpSectorImagePolicy(true)),
        (format => format.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase),
            _ => new BbcIsoScpSectorImagePolicy()),
        (format => format.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase),
            _ => new EpsonQx10IsoScpSectorImagePolicy()),
        (format => format.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase),
            _ => new UcsdIsoScpSectorImagePolicy())
    ];

    public static IIsoScpSectorImagePolicy Resolve(string? formatId)
    {
        if (formatId is null) return new AutomaticIsoScpSectorImagePolicy();
        var registration = Policies.FirstOrDefault(item => item.Matches(formatId));
        return registration.Create is null ? new GenericIsoScpSectorImagePolicy() : registration.Create(formatId);
    }
}
