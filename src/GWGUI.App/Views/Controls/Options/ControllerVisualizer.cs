using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Options;

public sealed partial class ControllerVisualizer : FrameworkElement
{
    internal ControllerVisualModel Model
    {
        get => _model;
        set { _model = value; InvalidateVisual(); }
    }

    internal GameInputLiveState? State
    {
        get => _state;
        set { _state = value; InvalidateVisual(); }
    }

    private ControllerVisualModel _model;
    private GameInputLiveState? _state;
    private ControllerVisualInput Input => new(_state);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var width = Math.Max(1, ActualWidth);
        var height = Math.Max(1, ActualHeight);
        var scale = Math.Min(width / 620d, height / 320d);
        var offsetX = (width - 620d * scale) / 2d;
        var offsetY = (height - 320d * scale) / 2d;
        drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY));
        drawingContext.PushTransform(new ScaleTransform(scale, scale));
        if (DrawArtworkController(drawingContext))
        {
            drawingContext.Pop();
            drawingContext.Pop();
            return;
        }
        switch (Model)
        {
            case ControllerVisualModel.XboxSeries: DrawXbox(drawingContext, series: true); break;
            case ControllerVisualModel.XboxOne: DrawXbox(drawingContext, series: false); break;
            case ControllerVisualModel.XboxRematchCore: DrawXbox(drawingContext, series: false, rematchCore: true); break;
            case ControllerVisualModel.PlayStation4: DrawPlayStation(drawingContext, dualSense: false); break;
            case ControllerVisualModel.PlayStation5: DrawPlayStation(drawingContext, dualSense: true); break;
            case ControllerVisualModel.PlayStation1: DrawPlayStationClassic(drawingContext, analog: false); break;
            case ControllerVisualModel.PlayStation2: DrawPlayStationClassic(drawingContext, analog: true); break;
            case ControllerVisualModel.Dreamcast: DrawDreamcast(drawingContext); break;
            case ControllerVisualModel.MasterSystem: DrawMasterSystem(drawingContext); break;
            case ControllerVisualModel.MegaDrive3: DrawMegaDrive(drawingContext, sixButtons: false); break;
            case ControllerVisualModel.MegaDrive6: DrawMegaDrive(drawingContext, sixButtons: true); break;
            case ControllerVisualModel.Saturn: DrawSaturn(drawingContext); break;
            case ControllerVisualModel.RacingWheel: DrawWheel(drawingContext); break;
            case ControllerVisualModel.FlightStick: DrawFlightStick(drawingContext); break;
            case ControllerVisualModel.ArcadeStick: DrawArcadeStick(drawingContext); break;
            default: DrawGenericGamepad(drawingContext); break;
        }
        drawingContext.Pop();
        drawingContext.Pop();
    }

    internal ControllerVisualSnapshot GetSnapshotForTest()
    {
        var input = Input;
        return new ControllerVisualSnapshot(
            input.LeftX, input.LeftY, input.RightX, input.RightY,
            input.LeftTrigger, input.RightTrigger,
            input.Wheel, input.Throttle, input.Brake, input.Clutch,
            input.Button(GameInputGamepadButtons.A, 0),
            input.Direction(GameInputSwitchPosition.Up, GameInputGamepadButtons.DPadUp, 10));
    }

    private void DrawDpad(DrawingContext dc, double x, double y)
    {
        var input = Input;
        DrawDirectionalPad(dc, x, y,
            input.Direction(GameInputSwitchPosition.Left, GameInputGamepadButtons.DPadLeft, 13),
            input.Direction(GameInputSwitchPosition.Right, GameInputGamepadButtons.DPadRight, 11),
            input.Direction(GameInputSwitchPosition.Up, GameInputGamepadButtons.DPadUp, 10),
            input.Direction(GameInputSwitchPosition.Down, GameInputGamepadButtons.DPadDown, 12));
    }

    private void DrawDirectionalPad(
        DrawingContext dc, double x, double y, bool left, bool right, bool up, bool down)
    {
        var stroke = Stroke();
        dc.DrawRoundedRectangle(Active(left), stroke, new Rect(x - 52, y - 16, 38, 32), 6, 6);
        dc.DrawRoundedRectangle(Active(right), stroke, new Rect(x + 14, y - 16, 38, 32), 6, 6);
        dc.DrawRoundedRectangle(Active(up), stroke, new Rect(x - 16, y - 52, 32, 38), 6, 6);
        dc.DrawRoundedRectangle(Active(down), stroke, new Rect(x - 16, y + 14, 32, 38), 6, 6);
        dc.DrawRectangle(Control(), null, new Rect(x - 16, y - 16, 32, 32));
    }

    private void DrawDiscDpad(DrawingContext dc, double x, double y)
    {
        dc.DrawEllipse(Control(), Stroke(), new Point(x, y), 53, 53);
        DrawDpad(dc, x, y);
    }

    private void DrawStick(DrawingContext dc, double x, double y, float axisX, float axisY, bool pressed)
    {
        var stroke = Stroke();
        dc.DrawEllipse(Control(), stroke, new Point(x, y), 38, 38);
        dc.DrawEllipse(Active(pressed), stroke, new Point(x + axisX * 16, y + axisY * 16), 23, 23);
    }

    private void DrawDarkDpad(DrawingContext dc, double x, double y, bool disc = false)
    {
        var input = Input;
        var idle = ColorBrush(Color.FromRgb(43, 45, 49));
        var edge = new Pen(ColorBrush(Color.FromRgb(132, 135, 141)), 2);
        if (disc)
            dc.DrawEllipse(ColorBrush(Color.FromRgb(20, 21, 24)), edge, new Point(x, y), 51, 51);
        var left = input.Direction(GameInputSwitchPosition.Left, GameInputGamepadButtons.DPadLeft, 13);
        var right = input.Direction(GameInputSwitchPosition.Right, GameInputGamepadButtons.DPadRight, 11);
        var up = input.Direction(GameInputSwitchPosition.Up, GameInputGamepadButtons.DPadUp, 10);
        var down = input.Direction(GameInputSwitchPosition.Down, GameInputGamepadButtons.DPadDown, 12);
        dc.DrawRoundedRectangle(left ? Accent() : idle, edge, new Rect(x - 50, y - 14, 37, 28), 5, 5);
        dc.DrawRoundedRectangle(right ? Accent() : idle, edge, new Rect(x + 13, y - 14, 37, 28), 5, 5);
        dc.DrawRoundedRectangle(up ? Accent() : idle, edge, new Rect(x - 14, y - 50, 28, 37), 5, 5);
        dc.DrawRoundedRectangle(down ? Accent() : idle, edge, new Rect(x - 14, y + 13, 28, 37), 5, 5);
        dc.DrawRectangle(idle, null, new Rect(x - 14, y - 14, 28, 28));
    }

    private void DrawDarkStick(DrawingContext dc, double x, double y, float axisX, float axisY, bool pressed)
    {
        var edge = new Pen(ColorBrush(Color.FromRgb(128, 131, 137)), 3);
        dc.DrawEllipse(ColorBrush(Color.FromRgb(17, 18, 21)), edge, new Point(x, y), 38, 38);
        dc.DrawEllipse(pressed ? Accent() : ColorBrush(Color.FromRgb(45, 47, 51)), edge,
            new Point(x + axisX * 16, y + axisY * 16), 23, 23);
        dc.DrawEllipse(null, new Pen(ColorBrush(Color.FromArgb(80, 255, 255, 255)), 1),
            new Point(x + axisX * 16, y + axisY * 16), 16, 16);
    }

    private void DrawShoulder(DrawingContext dc, Rect rect, string label, bool pressed)
    {
        dc.DrawRoundedRectangle(Active(pressed), Stroke(), rect, 10, 10);
        DrawText(dc, label, new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), 12);
    }

    private void DrawTrigger(DrawingContext dc, double x, double y, string label, float value)
    {
        var rect = new Rect(x - 45, y - 16, 90, 28);
        dc.DrawRoundedRectangle(Control(), Stroke(), rect, 9, 9);
        if (value > 0)
            dc.DrawRoundedRectangle(Accent(), null, new Rect(rect.X, rect.Y, rect.Width * Math.Clamp(value, 0, 1), rect.Height), 9, 9);
        DrawText(dc, label, new Point(x, y - 2), 12);
    }

    private void DrawPedal(DrawingContext dc, double x, string label, float value)
    {
        var rect = new Rect(x - 32, 254, 64, 45);
        dc.DrawRoundedRectangle(Control(), Stroke(), rect, 8, 8);
        var amount = Math.Clamp(value, 0, 1);
        if (amount > 0)
            dc.DrawRectangle(Accent(), null, new Rect(rect.X, rect.Bottom - rect.Height * amount, rect.Width, rect.Height * amount));
        DrawText(dc, label, new Point(x, 276), 11);
    }

    private void DrawButton(DrawingContext dc, double x, double y, string label, bool pressed, double radius = 20, Brush? idle = null)
    {
        dc.DrawEllipse(pressed ? Accent() : idle ?? Control(), Stroke(), new Point(x, y), radius, radius);
        DrawText(dc, label, new Point(x, y), Math.Min(14, radius * .7));
    }

    private void DrawText(DrawingContext dc, string value, Point center, double size, Brush? brush = null)
    {
        var text = new FormattedText(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), size, brush ?? Text(), VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
    }


    private static Brush ColorBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private Brush Text() => (Brush)(TryFindResource("TextBrush") ?? Brushes.White);
    private Brush Card() => (Brush)(TryFindResource("CardBrush") ?? Brushes.Transparent);
    private Brush Control() => (Brush)(TryFindResource("ControlBrush") ?? Brushes.DimGray);
    private Brush Accent() => (Brush)(TryFindResource("AccentBrush") ?? Brushes.DodgerBlue);
    private Brush Active(bool active) => active ? Accent() : Control();
    private Pen Stroke(double thickness = 3) => new(Text(), thickness);
}
