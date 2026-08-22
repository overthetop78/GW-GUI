using GWGUI.Domain.Formats;
using GWGUI.App.Enums.ViewModels.Conversion;

namespace GWGUI.App.Contracts.ViewModels.Conversion;

public sealed record ConversionFormatPresentation(
    DiskFormat Format,
    bool IsCompatible,
    bool IsSelected,
    IReadOnlySet<string> ExplicitExtensions,
    ConversionFormatGroup Group,
    bool IsReconstructedFlux);
