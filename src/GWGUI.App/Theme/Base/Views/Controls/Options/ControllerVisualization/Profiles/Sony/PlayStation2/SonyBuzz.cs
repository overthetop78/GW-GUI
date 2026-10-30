using GWGUI.App.Contracts.Input;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBuzz()
    {
        var zones = new List<ControllerVisualZone>();
        double[] redX = [3.12, 28.32, 55.86, 82.42];
        double[] answerX = [4.88, 30.08, 57.81, 84.18];
        string[] colors = ["Blue", "Orange", "Green", "Yellow"];
        double[] answerY = [47.51, 59.28, 70.14, 80.09];
        for (var player = 0; player < 4; player++)
        {
            zones.Add(new(Enum.Parse<EmulationControllerVisualControl>($"Buzz{player + 1}Red"),
                ControllerVisualZoneShape.Ellipse, redX[player], 2.71, 14.65, 30.77));
            for (var button = 0; button < colors.Length; button++)
                zones.Add(new(Enum.Parse<EmulationControllerVisualControl>($"Buzz{player + 1}{colors[button]}"),
                    ControllerVisualZoneShape.RoundedRectangle, answerX[player], answerY[button], 10.94, button < 2 ? 6.79 : 6.33));
        }
        return new(ControllerArtworkFileNames.SonyBuzz, zones);
    }
}
