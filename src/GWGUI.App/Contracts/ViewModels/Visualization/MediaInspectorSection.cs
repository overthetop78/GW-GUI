namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record MediaInspectorSection(
    string Title,
    string Icon,
    IReadOnlyList<MediaInspectorEntry> Entries);
