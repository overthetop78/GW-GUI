namespace GWGUI.Domain.Formats;

public sealed record ImageExtension(string Extension, string DisplayName, bool IsDefault = false);

public enum FloppyFormFactor
{
    Unknown,
    ThreeInch,
    ThreeAndHalfInch,
    FiveAndQuarterInch,
    EightInch
}

public enum FloppyDensity
{
    Unknown,
    DoubleDensity,
    HighDensity,
    ExtendedDensity
}

public sealed record DiskFormat(
    string Id,
    string Family,
    string DisplayName,
    IReadOnlyList<ImageExtension> Extensions,
    bool IsCommon = true,
    IReadOnlySet<string>? CompatibleSourceExtensions = null,
    string? Tag = null,
    FloppyFormFactor FormFactor = FloppyFormFactor.Unknown,
    bool SupportsPhysicalRead = true,
    bool SupportsPhysicalWrite = true,
    FloppyDensity Density = FloppyDensity.Unknown);
