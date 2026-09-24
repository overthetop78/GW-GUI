using GWGUI.MediaEngine.Contracts;

namespace GWGUI.App.ViewModels.Explorer;

/// <summary>Identifies one optical session in the Explorer without representing it as a file.</summary>
public sealed record ExplorerOpticalSessionChoice(int SessionNumber, string DisplayName);

/// <summary>Identifies one optical track in the Explorer without representing it as a file.</summary>
public sealed record ExplorerOpticalTrackChoice(OpticalTrackDescriptor Track, string DisplayName);
