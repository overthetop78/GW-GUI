using GWGUI.MediaEngine.Contracts.Explorer;

namespace GWGUI.App.ViewModels.Explorer;

/// <summary>Associates one explored media volume with the label displayed by the volume selector.</summary>
public sealed record ExplorerMediaVolumeChoice(
    ExploredMediaVolume Volume,
    string DisplayName);
