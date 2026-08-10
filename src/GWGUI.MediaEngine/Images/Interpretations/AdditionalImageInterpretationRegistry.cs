using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class AdditionalImageInterpretationRegistry(FileSystemRegistry fileSystems)
{
    private readonly IReadOnlyList<IAdditionalImageInterpretationPolicy> policies =
    [
        new IbmAdditionalImageInterpretationPolicy(fileSystems),
        new MsxAdditionalImageInterpretationPolicy(),
        new CompatibleFormatInterpretationPolicy()
    ];

    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (!IsIsoCompatible(image.FormatId)) yield break;
        foreach (var policy in policies)
        foreach (var interpretation in policy.Create(image))
            yield return interpretation;
    }

    private static bool IsIsoCompatible(string formatId) =>
        formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) ||
        formatId.Equals(DiskImageFormatIds.Imd, StringComparison.OrdinalIgnoreCase);
}
