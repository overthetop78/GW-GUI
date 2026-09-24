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
    private void DrawWheelArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var center = At(bounds, .500, .285);
        var angle = Input.Wheel * 130d * Math.PI / 180d;
        if (Math.Abs(Input.Wheel) > .01f)
            dc.DrawLine(new Pen(PressedStrong(), 2), center,
                new Point(center.X + Math.Sin(angle) * Radius(bounds, .19),
                    center.Y - Math.Cos(angle) * Radius(bounds, .19)));
        var wheelDpad = At(bounds, .395, .285);
        DrawArtworkDirection(dc, new Rect(wheelDpad.X - 4, wheelDpad.Y - 18, 8, 14),
            Input.RacingButton(GameInputRacingWheelButtons.DPadUp, 6));
        DrawArtworkDirection(dc, new Rect(wheelDpad.X - 4, wheelDpad.Y + 4, 8, 14),
            Input.RacingButton(GameInputRacingWheelButtons.DPadDown, 7));
        DrawArtworkDirection(dc, new Rect(wheelDpad.X - 18, wheelDpad.Y - 4, 14, 8),
            Input.RacingButton(GameInputRacingWheelButtons.DPadLeft, 8));
        DrawArtworkDirection(dc, new Rect(wheelDpad.X + 4, wheelDpad.Y - 4, 14, 8),
            Input.RacingButton(GameInputRacingWheelButtons.DPadRight, 9));
        DrawArtworkHalo(dc, At(bounds, .565, .315), Radius(bounds, .024), Input.RacingButton(GameInputRacingWheelButtons.A, 0));
        DrawArtworkHalo(dc, At(bounds, .585, .285), Radius(bounds, .024), Input.RacingButton(GameInputRacingWheelButtons.B, 1));
        DrawArtworkHalo(dc, At(bounds, .545, .285), Radius(bounds, .024), Input.RacingButton(GameInputRacingWheelButtons.X, 2));
        DrawArtworkHalo(dc, At(bounds, .565, .255), Radius(bounds, .024), Input.RacingButton(GameInputRacingWheelButtons.Y, 3));
        DrawArtworkHalo(dc, At(bounds, .380, .335), Radius(bounds, .030), Input.RacingButton(GameInputRacingWheelButtons.PreviousGear, 4));
        DrawArtworkHalo(dc, At(bounds, .620, .335), Radius(bounds, .030), Input.RacingButton(GameInputRacingWheelButtons.NextGear, 5));
        DrawArtworkHalo(dc, At(bounds, .470, .350), Radius(bounds, .022), Input.RacingButton(GameInputRacingWheelButtons.View, 10));
        DrawArtworkHalo(dc, At(bounds, .530, .350), Radius(bounds, .022), Input.RacingButton(GameInputRacingWheelButtons.Menu, 11));
        DrawArtworkHalo(dc, At(bounds, .405, .380), Radius(bounds, .025), Input.RacingButton(GameInputRacingWheelButtons.LeftThumbstick, 12));
        DrawArtworkHalo(dc, At(bounds, .595, .380), Radius(bounds, .025), Input.RacingButton(GameInputRacingWheelButtons.RightThumbstick, 13));
        DrawPedalOverlay(dc, bounds, .345, .800, Input.Clutch);
        DrawPedalOverlay(dc, bounds, .500, .800, Input.Brake);
        DrawPedalOverlay(dc, bounds, .655, .800, Input.Throttle);
        if (Input.Handbrake > .01f) DrawButton(dc, bounds.Right - 18, bounds.Bottom - 18, "HB", true, 11);
        if (Input.PatternShifterGear != 0) DrawButton(dc, bounds.X + 18, bounds.Bottom - 18, Input.PatternShifterGear.ToString(), true, 11);
    }

    private void DrawFlightArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var stick = At(bounds, .745 + Input.FlightRoll * .025, .360 + Input.FlightPitch * .025);
        if (Math.Abs(Input.FlightRoll) > .01f || Math.Abs(Input.FlightPitch) > .01f)
            dc.DrawEllipse(PressedFill(), null, stick, Radius(bounds, .075), Radius(bounds, .075));
        DrawArtworkHalo(dc, At(bounds, .715, .190), Radius(bounds, .030), Input.FlightButton(GameInputFlightStickButtons.FirePrimary, 0));
        DrawArtworkHalo(dc, At(bounds, .700, .095), Radius(bounds, .030), Input.FlightButton(GameInputFlightStickButtons.FireSecondary, 1));
        DrawArtworkHalo(dc, At(bounds, .760, .085), Radius(bounds, .025), Input.FlightButton(GameInputFlightStickButtons.A, 2));
        DrawArtworkHalo(dc, At(bounds, .805, .115), Radius(bounds, .025), Input.FlightButton(GameInputFlightStickButtons.B, 3));
        DrawArtworkHalo(dc, At(bounds, .740, .125), Radius(bounds, .025), Input.FlightButton(GameInputFlightStickButtons.X, 4));
        DrawArtworkHalo(dc, At(bounds, .785, .155), Radius(bounds, .025), Input.FlightButton(GameInputFlightStickButtons.Y, 5));
        DrawArtworkHalo(dc, At(bounds, .360, .570), Radius(bounds, .026), Input.FlightButton(GameInputFlightStickButtons.View, 6));
        DrawArtworkHalo(dc, At(bounds, .430, .570), Radius(bounds, .026), Input.FlightButton(GameInputFlightStickButtons.Menu, 7));
        DrawArtworkHalo(dc, At(bounds, .360, .665), Radius(bounds, .032), Input.FlightButton(GameInputFlightStickButtons.LeftShoulder, 8));
        DrawArtworkHalo(dc, At(bounds, .475, .665), Radius(bounds, .032), Input.FlightButton(GameInputFlightStickButtons.RightShoulder, 9));
        var hat = At(bounds, .745, .075);
        DrawArtworkDirection(dc, new Rect(hat.X - 5, hat.Y - 18, 10, 15), Input.FlightHat(GameInputSwitchPosition.Up));
        DrawArtworkDirection(dc, new Rect(hat.X - 5, hat.Y + 3, 10, 15), Input.FlightHat(GameInputSwitchPosition.Down));
        DrawArtworkDirection(dc, new Rect(hat.X - 18, hat.Y - 5, 15, 10), Input.FlightHat(GameInputSwitchPosition.Left));
        DrawArtworkDirection(dc, new Rect(hat.X + 3, hat.Y - 5, 15, 10), Input.FlightHat(GameInputSwitchPosition.Right));
        DrawArtworkTrigger(dc, new Rect(bounds.X + bounds.Width * .250, bounds.Y + bounds.Height * .430,
            8, bounds.Height * .270), Input.FlightThrottle);
        if (Math.Abs(Input.FlightYaw) > .01f)
            DrawButton(dc, bounds.Right - 18, bounds.Bottom - 18, "Y", true, 11);
    }

    private void DrawArcadeArtworkOverlays(DrawingContext dc, Rect bounds)
    {
        var stick = At(bounds, .270 + Input.ArcadeX * .025, .370 + Input.ArcadeY * .025);
        if (Math.Abs(Input.ArcadeX) > .01f || Math.Abs(Input.ArcadeY) > .01f)
            dc.DrawEllipse(PressedFill(), null, stick, Radius(bounds, .070), Radius(bounds, .070));
        var buttons = new[]
        {
            GameInputArcadeStickButtons.Action1, GameInputArcadeStickButtons.Action2,
            GameInputArcadeStickButtons.Action3, GameInputArcadeStickButtons.Action4,
            GameInputArcadeStickButtons.Action5, GameInputArcadeStickButtons.Action6,
            GameInputArcadeStickButtons.Special1, GameInputArcadeStickButtons.Special2
        };
        var points = new[] { (.485,.385),(.585,.365),(.690,.360),(.790,.355),(.455,.535),(.565,.515),(.675,.510),(.780,.505) };
        for (var index = 0; index < buttons.Length; index++)
            DrawArtworkHalo(dc, At(bounds, points[index].Item1, points[index].Item2), Radius(bounds, .045),
                Input.ArcadeButton(buttons[index], index));
        DrawArtworkHalo(dc, At(bounds, .580, .170), Radius(bounds, .025), Input.ArcadeButton(GameInputArcadeStickButtons.View, 8));
        DrawArtworkHalo(dc, At(bounds, .630, .170), Radius(bounds, .025), Input.ArcadeButton(GameInputArcadeStickButtons.Menu, 9));
    }

    private void DrawRawButtonHalo(DrawingContext dc, Rect bounds, int index, double x, double y, double radius) =>
        DrawArtworkHalo(dc, At(bounds, x, y), Radius(bounds, radius), Input.RawButton(index));

    private void DrawRawButtonMarker(
        DrawingContext dc, Rect bounds, int index,
        double x, double y, double width, double height, double cornerRadius)
    {
        if (!Input.RawButton(index)) return;
        var center = At(bounds, x, y);
        var marker = new Rect(
            center.X - bounds.Width * width / 2,
            center.Y - bounds.Height * height / 2,
            bounds.Width * width,
            bounds.Height * height);
        var inset = new Rect(marker.X + 3, marker.Y + 3,
            Math.Max(1, marker.Width - 6), Math.Max(1, marker.Height - 6));
        dc.DrawRoundedRectangle(PressedFill(), null, inset,
            Math.Max(1, cornerRadius - 2), Math.Max(1, cornerRadius - 2));
    }

    private void DrawRawButtonBadge(
        DrawingContext dc, Rect bounds, int index, double x, double y, string label)
    {
        if (!Input.RawButton(index)) return;
        var center = At(bounds, x, y);
        var badge = new Rect(center.X - 13, center.Y - 9, 26, 18);
        dc.DrawRoundedRectangle(ColorBrush(Color.FromArgb(175, 20, 20, 20)), null, badge, 7, 7);
        DrawText(dc, label, center, 10, ColorBrush(Color.FromArgb(235, 255, 255, 255)));
    }

    private void DrawPedalOverlay(DrawingContext dc, Rect bounds, double x, double y, float value)
    {
        if (value <= .01f) return;
        var center = At(bounds, x, y);
        dc.DrawRoundedRectangle(PressedFill(), null,
            new Rect(center.X - Radius(bounds, .035), center.Y - Radius(bounds, .080),
                Radius(bounds, .070), Radius(bounds, .160)), 5, 5);
    }

    private void DrawArtworkStick(
        DrawingContext dc,
        Point center,
        float x,
        float y,
        bool pressed,
        double radius)
    {
        var movedAxis = Math.Abs(x) > .03f || Math.Abs(y) > .03f;
        if (!pressed && !movedAxis) return;
        if (pressed) DrawArtworkHalo(dc, center, radius, active: true);
        if (!movedAxis) return;

        var movementRadius = radius * .65d;
        var moved = new Point(
            center.X + Math.Clamp(x, -1f, 1f) * movementRadius,
            center.Y + Math.Clamp(y, -1f, 1f) * movementRadius);
        var indicatorRadius = Math.Max(6d, radius * .55d);
        dc.DrawEllipse(PressedFill(), new Pen(PressedStrong(), Math.Max(1.5d, radius * .08d)),
            moved, indicatorRadius, indicatorRadius);
    }

    private static void DrawArtworkHalo(DrawingContext dc, Point center, double radius, bool active)
    {
        if (!active) return;
        var contained = Math.Max(2d, radius * .82d);
        dc.DrawEllipse(PressedFill(), new Pen(PressedStrong(), Math.Max(1.25d, radius * .08d)),
            center, contained, contained);
    }

    private static void DrawArtworkDirection(DrawingContext dc, Rect bounds, bool active)
    {
        if (!active) return;
        var inset = new Rect(bounds.X + 2, bounds.Y + 2,
            Math.Max(1, bounds.Width - 4), Math.Max(1, bounds.Height - 4));
        var corner = Math.Max(3d, Math.Min(inset.Width, inset.Height) * .3d);
        dc.DrawRoundedRectangle(PressedFill(), new Pen(PressedStrong(), 1.5d),
            inset, corner, corner);
    }

    private static void DrawArtworkTrigger(DrawingContext dc, Rect bounds, float value)
    {
        if (value <= .01f) return;
        var amount = Math.Clamp(value, 0f, 1f);
        var centerY = bounds.Y + bounds.Height / 2d;
        var active = new Rect(
            bounds.X,
            centerY,
            bounds.Width,
            Math.Max(1d, bounds.Height * .5d * amount));
        dc.DrawRoundedRectangle(PressedFill(), null, active, 4, 4);
    }

    private static Brush PressedFill() =>
        ColorBrush(Color.FromArgb(130, 255, 255, 255));

    private static Brush PressedStrong() =>
        ColorBrush(Color.FromArgb(245, 255, 255, 255));
}
