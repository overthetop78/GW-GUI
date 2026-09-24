using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using GWGUI.Emulation.Enums;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Options;

public sealed partial class ControllerVisualizer
{
    private void DrawMasterSystemArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkDpad(dc, bounds, .255, .600, .055, buttonWidth: .048, buttonLength: .15);
        DrawRawButtonHalo(dc, bounds, 0, .675, .665, .055);
        DrawRawButtonHalo(dc, bounds, 1, .850, .665, .055);
    }

    private void DrawNintendo64ArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkDpad(dc, bounds, .277, .374, .060,
            horizontalAxis: 3, verticalAxis: 4,
            upButton: -1, downButton: -1, leftButton: -1, rightButton: -1,
            buttonWidth: .043, buttonLength: .080);
        DrawArtworkStick(dc, At(bounds, .496, .563), Input.LeftX, Input.LeftY,
            false, Radius(bounds, .052));
        DrawRawButtonHalo(dc, bounds, 5, .669, .454, .038);
        DrawRawButtonHalo(dc, bounds, 4, .620, .384, .038);
        DrawRawButtonHalo(dc, bounds, 9, .497, .382, .040);
        DrawRawButtonHalo(dc, bounds, 0, .726, .265, .031);
        DrawRawButtonHalo(dc, bounds, 1, .768, .326, .031);
        DrawRawButtonHalo(dc, bounds, 2, .727, .389, .031);
        DrawRawButtonHalo(dc, bounds, 3, .685, .326, .031);
        DrawRawButtonMarker(dc, bounds, 6, .249, .202, .085, .045, 7);
        DrawRawButtonMarker(dc, bounds, 7, .751, .202, .085, .045, 7);
        DrawRawButtonBadge(dc, bounds, 8, .500, .690, "Z");
    }
    private void DrawNintendoEntertainmentSystemArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkDpad(dc, bounds, .237, .562, .114, horizontalAxis: 0, verticalAxis: 1,
            buttonWidth: .046, buttonLength: .145);
        DrawRawButtonHalo(dc, bounds, 0, .665, .674, .101);
        DrawRawButtonHalo(dc, bounds, 1, .772, .674, .101);
        DrawRawButtonMarker(dc, bounds, 8, .405, .689, .075, .083, 8);
        DrawRawButtonMarker(dc, bounds, 9, .504, .689, .075, .083, 8);
    }

    private void DrawSuperNintendoArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkDpad(dc, bounds, .214, .535, .085, horizontalAxis: 1, verticalAxis: 0,
            buttonWidth: .060, buttonLength: .090);
        DrawRawButtonHalo(dc, bounds, 0, .781, .436, .055);
        DrawRawButtonHalo(dc, bounds, 1, .872, .532, .055);
        DrawRawButtonHalo(dc, bounds, 3, .690, .535, .055);
        DrawRawButtonHalo(dc, bounds, 2, .784, .630, .055);
        DrawRawButtonMarker(dc, bounds, 8, .419, .574, .070, .077, 10);
        DrawRawButtonMarker(dc, bounds, 9, .524, .574, .070, .077, 10);
        DrawRawButtonMarker(dc, bounds, 4, .212, .230, .180, .063, 10);
        DrawRawButtonMarker(dc, bounds, 5, .790, .230, .180, .063, 10);
    }

    private void DrawMegaDriveArtworkOverlays(DrawingContext dc, Rect bounds, bool sixButtons)
    {
        DrawArtworkDpad(dc, bounds, .226, .523, .082, horizontalAxis: 1, verticalAxis: 0,
            buttonWidth: .058, buttonLength: .140);
        if (sixButtons)
        {
            DrawRawButtonHalo(dc, bounds, 2, .680, .599, .071);
            DrawRawButtonHalo(dc, bounds, 1, .782, .568, .076);
            DrawRawButtonHalo(dc, bounds, 5, .878, .535, .076);
            DrawRawButtonHalo(dc, bounds, 3, .671, .463, .055);
            DrawRawButtonHalo(dc, bounds, 0, .752, .421, .055);
            DrawRawButtonHalo(dc, bounds, 4, .831, .405, .055);
            DrawRawButtonMarker(dc, bounds, 9, .500, .469, .081, .056, 10);
            DrawRawButtonMarker(dc, bounds, 8, .500, .590, .081, .056, 10);
        }
        else
        {
            DrawRawButtonHalo(dc, bounds, 0, .655, .630, .052);
            DrawRawButtonHalo(dc, bounds, 1, .755, .570, .052);
            DrawRawButtonHalo(dc, bounds, 2, .850, .520, .052);
            DrawRawButtonMarker(dc, bounds, 3, .500, .500, .070, .042, 6);
        }
    }

    private void DrawSaturnArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkDpad(dc, bounds, .245, .520, .045);
        var points = new[] { (.660,.635), (.755,.565), (.850,.510), (.630,.475), (.715,.420), (.800,.380), (.500,.580), (.230,.170), (.770,.170) };
        for (var index = 0; index < points.Length; index++)
            DrawRawButtonHalo(dc, bounds, index, points[index].Item1, points[index].Item2, index is 7 or 8 ? .050 : .042);
    }

    private void DrawDreamcastArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        DrawArtworkStick(dc, At(bounds, .250, .275), Input.LeftX, Input.LeftY,
            Input.Button(GameInputGamepadButtons.LeftThumbstick, 8), Radius(bounds, .065));
        DrawArtworkDpad(dc, bounds, .270, .530, .045);
        DrawFaceButtons(dc, bounds, (.700, .470), (.800, .390), (.625, .390), (.700, .300));
        DrawArtworkHalo(dc, At(bounds, .500, .680), Radius(bounds, .035), Input.Button(GameInputGamepadButtons.Menu, 7));
        DrawShoulderAndTriggerOverlays(dc, bounds, .200, .800, .150);
    }

}
