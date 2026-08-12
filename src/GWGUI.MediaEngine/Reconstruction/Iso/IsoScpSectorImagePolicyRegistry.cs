using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Associe les familles de formats ISO aux politiques de reconstruction correspondantes.</summary>
internal static class IsoScpSectorImagePolicyRegistry
{
    private static readonly IReadOnlyList<(Predicate<string> Matches, Func<string, IIsoScpSectorImagePolicy> Create)> Policies =
    [
        (format => format.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase),
            format => new Atari8BitIsoScpSectorImagePolicy(format)),
        (format => format.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase),
            _ => new AtariStIsoScpSectorImagePolicy()),
        (format => format.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase),
            _ => new AmstradIsoScpSectorImagePolicy()),
        (format => format.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) ||
                   format.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase),
            _ => new IbmPcIsoScpSectorImagePolicy(true)),
        (format => format.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase),
            _ => new BbcIsoScpSectorImagePolicy()),
        (format => format.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase),
            _ => new EpsonQx10IsoScpSectorImagePolicy()),
        (format => format.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase),
            _ => new UcsdIsoScpSectorImagePolicy())
    ];

    /// <summary>Résout la politique inscrite, la politique automatique pour un identifiant nul ou la politique générique pour un identifiant explicite inconnu.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour la détection automatique.</param>
    /// <returns>La politique ISO à utiliser pour décoder et construire l'image.</returns>
    public static IIsoScpSectorImagePolicy Resolve(string? formatId)
    {
        if (formatId is null) return new AutomaticIsoScpSectorImagePolicy();
        var registration = Policies.FirstOrDefault(item => item.Matches(formatId));
        return registration.Create is null ? new GenericIsoScpSectorImagePolicy() : registration.Create(formatId);
    }
}
