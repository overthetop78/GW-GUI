using System.Windows.Media;
using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Controllers;

internal sealed record EmulationControllerVisualChoiceView(string Key, string Value, ImageSource? Artwork)
{
    internal EmulationControllerChoice? ControllerChoice { get; init; }
    internal string? VisualId { get; init; }
}
