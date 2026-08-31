using System.Windows.Media;

namespace GWGUI.App.Contracts.Input;

internal sealed record ControllerArtworkProfile(
    string VisualId,
    ImageSource Artwork,
    IReadOnlyList<ControllerVisualZone> Zones);
