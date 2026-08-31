using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using GWGUI.Emulation.Enums;
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
        set
        {
            _state = value;
            _visualState = null;
            InvalidateVisual();
        }
    }

    internal ControllerVisualState? VisualState
    {
        get => _visualState;
        set
        {
            _visualState = value;
            InvalidateVisual();
        }
    }

    internal ControllerArtworkProfile? ArtworkProfile
    {
        get => _artworkProfile;
        set
        {
            _artworkProfile = value;
            HoveredVisualControl = null;
            InvalidateVisual();
        }
    }

    internal IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds
    {
        get => _visualCommandIds;
        set
        {
            _visualCommandIds = value;
            HoveredVisualControl = null;
            InvalidateVisual();
        }
    }

    internal EmulationControllerVisualControl? HoveredVisualControl { get; private set; }

    internal event Action<EmulationControllerVisualControl>? VisualZoneClicked;

    private ControllerVisualModel _model;
    private GameInputLiveState? _state;
    private ControllerVisualState? _visualState;
    private ControllerArtworkProfile? _artworkProfile;
    private IReadOnlyDictionary<EmulationControllerVisualControl, string>? _visualCommandIds;
    private ControllerVisualInput Input =>
        _visualState is null ? new ControllerVisualInput(_state) : new ControllerVisualInput(_visualState);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var (scale, offsetX, offsetY) = CalculateLayout();
        drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY));
        drawingContext.PushTransform(new ScaleTransform(scale, scale));
        if (ArtworkProfile is { } profile)
        {
            DrawArtworkProfile(drawingContext, profile);
            drawingContext.Pop();
            drawingContext.Pop();
            return;
        }
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

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseMove(e);
        UpdateHoveredVisualControl(e.GetPosition(this));
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        SetHoveredVisualControl(null);
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var zone = HitTestVisualZone(e.GetPosition(this));
        if (zone is null) return;
        e.Handled = true;
        Focus();
        VisualZoneClicked?.Invoke(zone.Control);
    }

    private void DrawArtworkProfile(DrawingContext drawingContext, ControllerArtworkProfile profile)
    {
        var bounds = FitArtworkProfileBounds(profile.Artwork);
        drawingContext.DrawImage(profile.Artwork, bounds);
        DrawArtworkProfileOverlays(drawingContext, profile, bounds);
    }

    private void UpdateHoveredVisualControl(Point position) =>
        SetHoveredVisualControl(HitTestVisualZone(position)?.Control);

    private void SetHoveredVisualControl(EmulationControllerVisualControl? control)
    {
        if (HoveredVisualControl == control) return;
        HoveredVisualControl = control;
        Cursor = control is null ? null : System.Windows.Input.Cursors.Hand;
        InvalidateVisual();
    }

    private ControllerVisualZone? HitTestVisualZone(Point position)
    {
        if (ArtworkProfile is not { } profile) return null;
        var (scale, offsetX, offsetY) = CalculateLayout();
        var point = new Point((position.X - offsetX) / scale, (position.Y - offsetY) / scale);
        var artworkBounds = FitArtworkProfileBounds(profile.Artwork);
        for (var index = profile.Zones.Count - 1; index >= 0; index--)
        {
            var zone = profile.Zones[index];
            if (!HasVisualCommand(zone.Control)) continue;
            var bounds = ZoneBounds(artworkBounds, zone);
            if (Contains(zone, bounds, point)) return zone;
        }
        return null;
    }

    private bool HasVisualCommand(EmulationControllerVisualControl control) =>
        _visualCommandIds?.ContainsKey(control) == true;

    private static Rect ZoneBounds(Rect artworkBounds, ControllerVisualZone zone) =>
        new(
            artworkBounds.X + artworkBounds.Width * zone.XPercent / 100d,
            artworkBounds.Y + artworkBounds.Height * zone.YPercent / 100d,
            artworkBounds.Width * zone.WidthPercent / 100d,
            artworkBounds.Height * zone.HeightPercent / 100d);

    private static bool Contains(ControllerVisualZone zone, Rect bounds, Point point)
    {
        if (!bounds.Contains(point)) return false;
        if (zone.Shape is ControllerVisualZoneShape.DirectionalPad
            or ControllerVisualZoneShape.JoystickDirection)
            return MatchesDirectionSector(zone.Control, bounds, point);
        if (zone.Shape != ControllerVisualZoneShape.Ellipse) return true;

        var radiusX = bounds.Width / 2d;
        var radiusY = bounds.Height / 2d;
        if (radiusX <= 0d || radiusY <= 0d) return false;
        var dx = (point.X - (bounds.X + radiusX)) / radiusX;
        var dy = (point.Y - (bounds.Y + radiusY)) / radiusY;
        return dx * dx + dy * dy <= 1d;
    }

    private static bool MatchesDirectionSector(
        EmulationControllerVisualControl control,
        Rect bounds,
        Point point)
    {
        var dx = point.X - (bounds.X + bounds.Width / 2d);
        var dy = point.Y - (bounds.Y + bounds.Height / 2d);
        if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon) return false;
        if (Math.Abs(dx) > Math.Abs(dy))
            return dx < 0d
                ? control == EmulationControllerVisualControl.DirectionLeft
                : control == EmulationControllerVisualControl.DirectionRight;
        return dy < 0d
            ? control == EmulationControllerVisualControl.DirectionUp
            : control == EmulationControllerVisualControl.DirectionDown;
    }

    private (double Scale, double OffsetX, double OffsetY) CalculateLayout()
    {
        var width = Math.Max(1, ActualWidth);
        var height = Math.Max(1, ActualHeight);
        var canvasHeight = ArtworkProfile is null ? 320d : 520d;
        var scale = Math.Min(width / 620d, height / canvasHeight);
        return (scale, (width - 620d * scale) / 2d, (height - canvasHeight * scale) / 2d);
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
