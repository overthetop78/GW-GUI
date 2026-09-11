using GWGUI.App.Enums.ViewModels.Visualization;

namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record MediaInspectorEntry(
    string Label,
    string Value,
    string? Unit = null,
    MediaInspectorEntryLevel Level = MediaInspectorEntryLevel.Information);
