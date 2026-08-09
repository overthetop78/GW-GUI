namespace GWGUI.Domain.Conversion;

public sealed record ConversionSelection(string FormatId, IReadOnlySet<string> ExplicitExtensions);
public sealed record ConversionOutput(string FormatId, string Extension, string OutputPath, bool UsesImplicitExtension);
