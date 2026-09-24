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
    private void DrawXboxArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var input = Input;
        var rematch = Model == ControllerVisualModel.XboxRematchCore;
        var leftStick = rematch ? (.255, .405) : (.258, .298);
        var rightStick = rematch ? (.615, .555) : (.612, .502);
        DrawArtworkStick(dc, At(bounds, leftStick.Item1, leftStick.Item2), input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8), Radius(bounds, .08));
        DrawArtworkStick(dc, At(bounds, rightStick.Item1, rightStick.Item2), input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9), Radius(bounds, .08));
        if (rematch)
        {
            DrawFaceButtons(dc, bounds, (.730, .455), (.790, .360), (.665, .360), (.730, .270));
            DrawArtworkHalo(dc, At(bounds, .430, .335), Radius(bounds, .032), input.Button(GameInputGamepadButtons.View, 6));
            DrawArtworkHalo(dc, At(bounds, .565, .335), Radius(bounds, .032), input.Button(GameInputGamepadButtons.Menu, 7));
            DrawArtworkHalo(dc, At(bounds, .500, .410), Radius(bounds, .027), input.SystemButton(GameInputSystemButtons.Share));
            DrawArtworkHalo(dc, At(bounds, .500, .255), Radius(bounds, .050), input.SystemButton(GameInputSystemButtons.Guide));
            DrawArtworkDpad(dc, bounds, .390, .555, .040);
            DrawShoulderAndTriggerOverlays(dc, bounds, .275, .725, .210);
        }
        else
        {
            DrawFaceButtons(dc, bounds, (.736, .388), (.795, .302), (.674, .300), (.739, .202));
            DrawArtworkHalo(dc, At(bounds, .434, .297), Radius(bounds, .032), input.Button(GameInputGamepadButtons.View, 6));
            DrawArtworkHalo(dc, At(bounds, .565, .297), Radius(bounds, .032), input.Button(GameInputGamepadButtons.Menu, 7));
            DrawArtworkHalo(dc, At(bounds, .500, .370), Radius(bounds, .027), input.SystemButton(GameInputSystemButtons.Share));
            DrawArtworkHalo(dc, At(bounds, .500, .173), Radius(bounds, .050), input.SystemButton(GameInputSystemButtons.Guide));
            DrawArtworkDpad(dc, bounds, .382, .502, .040);
            DrawShoulderAndTriggerOverlays(dc, bounds, .275, .725, .150);
        }

        DrawArtworkHalo(dc, At(bounds, .350, .650), Radius(bounds, .032),
            input.Button(GameInputGamepadButtons.PaddleLeft1, 14));
        DrawArtworkHalo(dc, At(bounds, .405, .715), Radius(bounds, .032),
            input.Button(GameInputGamepadButtons.PaddleLeft2, 15));
        DrawArtworkHalo(dc, At(bounds, .650, .650), Radius(bounds, .032),
            input.Button(GameInputGamepadButtons.PaddleRight1, 16));
        DrawArtworkHalo(dc, At(bounds, .595, .715), Radius(bounds, .032),
            input.Button(GameInputGamepadButtons.PaddleRight2, 17));
    }

    private void DrawXbox360ArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var input = Input;
        DrawArtworkStick(dc, At(bounds, .249, .278), input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8), Radius(bounds, .078));
        DrawArtworkStick(dc, At(bounds, .610, .497), input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9), Radius(bounds, .078));
        DrawArtworkDpad(dc, bounds, .363, .498, .045);
        DrawFaceButtons(dc, bounds, (.735, .388), (.799, .289), (.670, .285), (.735, .179));
        DrawArtworkHalo(dc, At(bounds, .402, .288), Radius(bounds, .027),
            input.Button(GameInputGamepadButtons.View, 6));
        DrawArtworkHalo(dc, At(bounds, .586, .287), Radius(bounds, .027),
            input.Button(GameInputGamepadButtons.Menu, 7));
        DrawArtworkHalo(dc, At(bounds, .499, .290), Radius(bounds, .052),
            input.SystemButton(GameInputSystemButtons.Guide));
        DrawShoulderAndTriggerOverlays(dc, bounds, .270, .730, .080);
    }
    private void DrawGenericArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var input = Input;
        DrawArtworkStick(dc, At(bounds, .350, .550), input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8), Radius(bounds, .07));
        DrawArtworkStick(dc, At(bounds, .625, .550), input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9), Radius(bounds, .07));
        DrawFaceButtons(dc, bounds, (.765, .445), (.835, .350), (.695, .350), (.765, .255));
        DrawArtworkDpad(dc, bounds, .250, .350, .040);
        DrawArtworkHalo(dc, At(bounds, .420, .350), Radius(bounds, .025), Input.Button(GameInputGamepadButtons.View, 6));
        DrawArtworkHalo(dc, At(bounds, .565, .350), Radius(bounds, .025), Input.Button(GameInputGamepadButtons.Menu, 7));
        DrawShoulderAndTriggerOverlays(dc, bounds, .260, .740, .130);
    }
    private void DrawPlayStationArtworkOverlays(DrawingContext dc, Rect bounds, bool analog)
    {
        var input = Input;
        var modern = Model is ControllerVisualModel.PlayStation4 or ControllerVisualModel.PlayStation5;
        var dpad = modern ? (.210, .305) : (.205, .410);
        var faceA = modern ? (.790, .405) : (.770, .510);
        var faceB = modern ? (.855, .310) : (.850, .410);
        var faceX = modern ? (.725, .310) : (.690, .410);
        var faceY = modern ? (.790, .215) : (.770, .310);
        DrawFaceButtons(dc, bounds, faceA, faceB, faceX, faceY);
        DrawArtworkDpad(dc, bounds, dpad.Item1, dpad.Item2, .040);
        if (modern)
        {
            DrawArtworkHalo(dc, At(bounds, .295, .135), Radius(bounds, .025), input.Button(GameInputGamepadButtons.View, 6));
            DrawArtworkHalo(dc, At(bounds, .705, .135), Radius(bounds, .025), input.Button(GameInputGamepadButtons.Menu, 7));
            DrawArtworkHalo(dc, At(bounds, .500, .525), Radius(bounds, .030), input.SystemButton(GameInputSystemButtons.Guide));
        }
        else
        {
            DrawArtworkHalo(dc, At(bounds, .430, .505), Radius(bounds, .025), input.Button(GameInputGamepadButtons.View, 6));
            DrawArtworkHalo(dc, At(bounds, .570, .505), Radius(bounds, .025), input.Button(GameInputGamepadButtons.Menu, 7));
        }
        if (analog)
        {
            var stickY = modern ? .515 : .610;
            DrawArtworkStick(dc, At(bounds, .375, stickY), input.LeftX, input.LeftY,
                input.Button(GameInputGamepadButtons.LeftThumbstick, 8), Radius(bounds, .070));
            DrawArtworkStick(dc, At(bounds, .625, stickY), input.RightX, input.RightY,
                input.Button(GameInputGamepadButtons.RightThumbstick, 9), Radius(bounds, .070));
        }
        DrawShoulderAndTriggerOverlays(dc, bounds, .220, .780, .145);
    }
    private void DrawFaceButtons(
        DrawingContext dc, Rect bounds,
        (double X, double Y) a, (double X, double Y) b,
        (double X, double Y) x, (double X, double Y) y)
    {
        var radius = Radius(bounds, .045);
        DrawArtworkHalo(dc, At(bounds, a.X, a.Y), radius, Input.Button(GameInputGamepadButtons.A, 0));
        DrawArtworkHalo(dc, At(bounds, b.X, b.Y), radius, Input.Button(GameInputGamepadButtons.B, 1));
        DrawArtworkHalo(dc, At(bounds, x.X, x.Y), radius, Input.Button(GameInputGamepadButtons.X, 2));
        DrawArtworkHalo(dc, At(bounds, y.X, y.Y), radius, Input.Button(GameInputGamepadButtons.Y, 3));
    }

    private void DrawShoulderAndTriggerOverlays(DrawingContext dc, Rect bounds, double leftX, double rightX, double shoulderY)
    {
        DrawArtworkHalo(dc, At(bounds, leftX, shoulderY), Radius(bounds, .050),
            Input.Button(GameInputGamepadButtons.LeftShoulder, 4));
        DrawArtworkHalo(dc, At(bounds, rightX, shoulderY), Radius(bounds, .050),
            Input.Button(GameInputGamepadButtons.RightShoulder, 5));
        DrawArtworkTrigger(dc, new Rect(
            bounds.X + bounds.Width * (leftX - .08), bounds.Y + bounds.Height * .075,
            bounds.Width * .16, 7), Input.LeftTrigger);
        DrawArtworkTrigger(dc, new Rect(
            bounds.X + bounds.Width * (rightX - .08), bounds.Y + bounds.Height * .075,
            bounds.Width * .16, 7), Input.RightTrigger);
    }

    private void DrawArtworkDpad(
        DrawingContext dc,
        Rect bounds,
        double x,
        double y,
        double arm,
        int horizontalAxis = 0,
        int verticalAxis = 1,
        int upButton = 10,
        int downButton = 12,
        int leftButton = 13,
        int rightButton = 11,
        double buttonWidth = .034,
        double buttonLength = .075)
    {
        var center = At(bounds, x, y);
        var offset = Radius(bounds, arm);
        var dx = offset;
        var dy = offset;
        var w = Math.Max(8, bounds.Width * buttonWidth);
        var h = Math.Max(8, bounds.Height * buttonLength);
        DrawArtworkDirection(dc, new Rect(center.X - w / 2, center.Y - dy - h / 2, w, h),
            Input.Direction(GameInputSwitchPosition.Up, GameInputGamepadButtons.DPadUp, upButton, horizontalAxis, verticalAxis));
        DrawArtworkDirection(dc, new Rect(center.X - w / 2, center.Y + dy - h / 2, w, h),
            Input.Direction(GameInputSwitchPosition.Down, GameInputGamepadButtons.DPadDown, downButton, horizontalAxis, verticalAxis));
        DrawArtworkDirection(dc, new Rect(center.X - dx - h / 2, center.Y - w / 2, h, w),
            Input.Direction(GameInputSwitchPosition.Left, GameInputGamepadButtons.DPadLeft, leftButton, horizontalAxis, verticalAxis));
        DrawArtworkDirection(dc, new Rect(center.X + dx - h / 2, center.Y - w / 2, h, w),
            Input.Direction(GameInputSwitchPosition.Right, GameInputGamepadButtons.DPadRight, rightButton, horizontalAxis, verticalAxis));
    }

}
