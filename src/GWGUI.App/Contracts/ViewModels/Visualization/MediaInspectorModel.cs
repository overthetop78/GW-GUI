namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record MediaInspectorModel(
    string Title,
    string? SelectedElement,
    IReadOnlyList<MediaInspectorSection> Sections);
