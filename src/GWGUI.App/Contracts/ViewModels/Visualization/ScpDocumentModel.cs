using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record ScpDocumentModel(
    ScpImage Image,
    string FileName,
    string Summary,
    IReadOnlySet<int> Heads);
