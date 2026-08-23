using GWGUI.App.Services.Input.GameInput;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Options;

public sealed partial class ControllerVisualizer
{
    private void DrawWheel(DrawingContext dc)
    {
        var input = Input;
        var stroke = Stroke(6);
        dc.DrawRoundedRectangle(Card(), Stroke(), new Rect(105, 70, 410, 158), 24, 24);
        dc.DrawEllipse(Control(), stroke, new Point(310, 143), 104, 104);
        dc.DrawEllipse(Card(), stroke, new Point(310, 143), 38, 38);
        dc.PushTransform(new RotateTransform(input.Wheel * 130, 310, 143));
        dc.DrawLine(stroke, new Point(310, 105), new Point(310, 48));
        dc.DrawLine(stroke, new Point(282, 166), new Point(230, 209));
        dc.DrawLine(stroke, new Point(338, 166), new Point(390, 209));
        dc.Pop();
        DrawDirectionalPad(dc, 155, 145,
            input.RacingButton(GameInputRacingWheelButtons.DPadLeft, 8),
            input.RacingButton(GameInputRacingWheelButtons.DPadRight, 9),
            input.RacingButton(GameInputRacingWheelButtons.DPadUp, 6),
            input.RacingButton(GameInputRacingWheelButtons.DPadDown, 7));
        DrawButton(dc, 459, 170, "A",
            input.RacingButton(GameInputRacingWheelButtons.A, 0), 15);
        DrawButton(dc, 489, 140, "B",
            input.RacingButton(GameInputRacingWheelButtons.B, 1), 15);
        DrawButton(dc, 429, 140, "X",
            input.RacingButton(GameInputRacingWheelButtons.X, 2), 15);
        DrawButton(dc, 459, 110, "Y",
            input.RacingButton(GameInputRacingWheelButtons.Y, 3), 15);
        DrawButton(dc, 257, 143, "−",
            input.RacingButton(GameInputRacingWheelButtons.PreviousGear, 4), 13);
        DrawButton(dc, 363, 143, "+",
            input.RacingButton(GameInputRacingWheelButtons.NextGear, 5), 13);
        DrawButton(dc, 275, 218, "◫",
            input.RacingButton(GameInputRacingWheelButtons.View, 12), 11);
        DrawButton(dc, 345, 218, "☰",
            input.RacingButton(GameInputRacingWheelButtons.Menu, 13), 11);
        DrawButton(dc, 257, 177, "L3",
            input.RacingButton(GameInputRacingWheelButtons.LeftThumbstick, 14), 10);
        DrawButton(dc, 363, 177, "R3",
            input.RacingButton(GameInputRacingWheelButtons.RightThumbstick, 15), 10);
        DrawText(dc, $"G {input.PatternShifterGear}", new Point(310, 246), 12);
        DrawPedal(dc, 185, "ACC", input.Throttle);
        DrawPedal(dc, 275, "BRK", input.Brake);
        DrawPedal(dc, 365, "CLU", input.Clutch);
        DrawPedal(dc, 455, "HB", input.Handbrake);
    }

    private void DrawFlightStick(DrawingContext dc)
    {
        var input = Input;
        dc.DrawRoundedRectangle(Card(), Stroke(), new Rect(155, 240, 310, 58), 25, 25);
        var gripX = 310 + input.FlightRoll * 55;
        var gripY = 96 + input.FlightPitch * 45;
        dc.DrawLine(Stroke(18), new Point(310, 245), new Point(gripX, gripY));
        dc.DrawRoundedRectangle(Active(input.FlightButton(GameInputFlightStickButtons.FirePrimary, 0)), Stroke(),
            new Rect(gripX - 35, gripY - 30, 70, 65), 20, 20);
        DrawButton(dc, gripX + 25, gripY - 20, "1",
            input.FlightButton(GameInputFlightStickButtons.FirePrimary, 0), 15);
        DrawButton(dc, 230, 267, "2",
            input.FlightButton(GameInputFlightStickButtons.FireSecondary, 1), 18);
        DrawButton(dc, 285, 267, "A",
            input.FlightButton(GameInputFlightStickButtons.A, 2), 18);
        DrawButton(dc, 340, 267, "B",
            input.FlightButton(GameInputFlightStickButtons.B, 3), 18);
        DrawButton(dc, 395, 267, "X",
            input.FlightButton(GameInputFlightStickButtons.X, 4), 18);
        DrawButton(dc, 445, 267, "Y",
            input.FlightButton(GameInputFlightStickButtons.Y, 5), 18);
        DrawButton(dc, 175, 257, "◫",
            input.FlightButton(GameInputFlightStickButtons.View, 6), 10);
        DrawButton(dc, 175, 281, "☰",
            input.FlightButton(GameInputFlightStickButtons.Menu, 7), 10);
        DrawButton(dc, 272, 224, "LB",
            input.FlightButton(GameInputFlightStickButtons.LeftShoulder, 8), 11);
        DrawButton(dc, 348, 224, "RB",
            input.FlightButton(GameInputFlightStickButtons.RightShoulder, 9), 11);
        DrawButton(dc, gripX, gripY - 18, "↑", input.FlightHat(GameInputSwitchPosition.Up), 8);
        DrawButton(dc, gripX, gripY + 18, "↓", input.FlightHat(GameInputSwitchPosition.Down), 8);
        DrawButton(dc, gripX - 18, gripY, "←", input.FlightHat(GameInputSwitchPosition.Left), 8);
        DrawButton(dc, gripX + 18, gripY, "→", input.FlightHat(GameInputSwitchPosition.Right), 8);
        dc.DrawEllipse(Control(), Stroke(), new Point(115, 174), 35, 35);
        dc.PushTransform(new RotateTransform(input.FlightYaw * 100, 115, 174));
        dc.DrawLine(Stroke(5), new Point(115, 174), new Point(115, 143));
        dc.Pop();
        DrawText(dc, "YAW", new Point(115, 218), 11);
        var throttle = new Rect(490, 100, 32, 150);
        dc.DrawRoundedRectangle(Control(), Stroke(), throttle, 8, 8);
        dc.DrawEllipse(Accent(), Stroke(), new Point(506, throttle.Bottom - input.FlightThrottle * throttle.Height), 18, 12);
    }

    private void DrawArcadeStick(DrawingContext dc)
    {
        var input = Input;
        dc.DrawRoundedRectangle(Card(), Stroke(), new Rect(55, 68, 510, 224), 18, 18);
        dc.DrawLine(Stroke(12), new Point(180, 230),
            new Point(180 + input.ArcadeX * 45, 145 + input.ArcadeY * 45));
        dc.DrawEllipse(Active(input.ArcadeButton(GameInputArcadeStickButtons.Action1, 0)), Stroke(),
            new Point(180 + input.ArcadeX * 45, 125 + input.ArcadeY * 45), 28, 28);
        var arcadeButtons = new[]
        {
            GameInputArcadeStickButtons.Action1, GameInputArcadeStickButtons.Action2,
            GameInputArcadeStickButtons.Action3, GameInputArcadeStickButtons.Action4,
            GameInputArcadeStickButtons.Action5, GameInputArcadeStickButtons.Action6,
            GameInputArcadeStickButtons.Special1, GameInputArcadeStickButtons.Special2
        };
        for (var index = 0; index < arcadeButtons.Length; index++)
            DrawButton(dc, 350 + (index % 4) * 48, 135 + (index / 4) * 58,
                (index + 1).ToString(), input.ArcadeButton(arcadeButtons[index], index), 21);
        DrawButton(dc, 288, 101, "◫",
            input.ArcadeButton(GameInputArcadeStickButtons.View, 8), 11);
        DrawButton(dc, 330, 101, "☰",
            input.ArcadeButton(GameInputArcadeStickButtons.Menu, 9), 11);
    }
}
