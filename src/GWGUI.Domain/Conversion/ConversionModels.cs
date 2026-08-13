namespace GWGUI.Domain.Conversion;

public sealed record ConversionSelection(string FormatId, IReadOnlySet<string> ExplicitExtensions);
public sealed record ConversionOutput
{
    public ConversionOutput(string formatId, string extension, string outputPath, bool usesImplicitExtension, ConversionFidelityLevel? fidelity = null)
    {
        FormatId = formatId;
        Extension = extension;
        OutputPath = outputPath;
        UsesImplicitExtension = usesImplicitExtension;
        Fidelity = fidelity ?? ConversionFidelity.ForRebuiltOutput(extension);
    }

    public string FormatId { get; init; }
    public string Extension { get; init; }
    public string OutputPath { get; init; }
    public bool UsesImplicitExtension { get; init; }
    public ConversionFidelityLevel Fidelity { get; init; }
    public bool PreservesOriginalProtection => ConversionFidelity.PreservesOriginalProtection(Fidelity);
}
