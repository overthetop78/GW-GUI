using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

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
        formatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
        formatId.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase) ||
        formatId.Equals("imd", StringComparison.OrdinalIgnoreCase);
}
