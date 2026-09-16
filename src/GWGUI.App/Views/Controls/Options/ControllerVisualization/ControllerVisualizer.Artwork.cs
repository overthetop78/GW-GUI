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
    private bool DrawArtworkController(DrawingContext dc)
    {
        if (!ControllerVisualization.ControllerArtworkCatalog.TryGet(Model, out var artwork)) return false;

        var bounds = FitArtworkBounds(artwork);
        dc.DrawImage(artwork, bounds);
        switch (Model)
        {
            case ControllerVisualModel.XboxSeries:
            case ControllerVisualModel.XboxOne:
            case ControllerVisualModel.XboxRematchCore:
                DrawXboxArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.Xbox360:
            case ControllerVisualModel.Xbox360White:
                DrawXbox360ArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.GenericGamepad:
                DrawGenericArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.PlayStation4:
            case ControllerVisualModel.PlayStation5:
                DrawPlayStationArtworkOverlays(dc, bounds, analog: true);
                break;
            case ControllerVisualModel.PlayStation1:
                DrawPlayStationArtworkOverlays(dc, bounds, analog: false);
                break;
            case ControllerVisualModel.PlayStation2:
                DrawPlayStationArtworkOverlays(dc, bounds, analog: true);
                break;
            case ControllerVisualModel.MasterSystem:
                DrawMasterSystemArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.NintendoEntertainmentSystem:
                DrawNintendoEntertainmentSystemArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.Nintendo64:
                DrawNintendo64ArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.SuperNintendo:
                DrawSuperNintendoArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.MegaDrive3:
                DrawMegaDriveArtworkOverlays(dc, bounds, sixButtons: false);
                break;
            case ControllerVisualModel.MegaDrive6:
                DrawMegaDriveArtworkOverlays(dc, bounds, sixButtons: true);
                break;
            case ControllerVisualModel.Saturn:
                DrawSaturnArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.Dreamcast:
                DrawDreamcastArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.RacingWheel:
                DrawWheelArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.FlightStick:
                DrawFlightArtworkOverlays(dc, bounds);
                break;
            case ControllerVisualModel.ArcadeStick:
                DrawArcadeArtworkOverlays(dc, bounds);
                break;
        }
        return true;
    }

    private void DrawArtworkProfileOverlays(
        DrawingContext dc,
        ControllerArtworkProfile profile,
        Rect artworkBounds)
    {
        var joystickZone = profile.Zones.FirstOrDefault(zone =>
            zone.Shape == ControllerVisualZoneShape.JoystickDirection && HasVisualCommand(zone.Control));
        if (joystickZone is not null)
            DrawArtworkProfileJoystick(dc, ZoneBounds(artworkBounds, joystickZone));

        foreach (var zone in profile.Zones)
        {
            if (zone.Shape == ControllerVisualZoneShape.JoystickDirection
                || !HasVisualCommand(zone.Control)) continue;
            var hovered = HoveredVisualControl == zone.Control;
            var value = Math.Max(VisualZoneValue(zone.Control), hovered ? 1f : 0f);
            if (value <= .01f) continue;

            var bounds = ZoneBounds(artworkBounds, zone);
            switch (zone.Shape)
            {
                case ControllerVisualZoneShape.Ellipse:
                    DrawArtworkHalo(
                        dc,
                        new Point(bounds.X + bounds.Width / 2d, bounds.Y + bounds.Height / 2d),
                        Math.Min(bounds.Width, bounds.Height) / 2d,
                        active: true);
                    break;
                case ControllerVisualZoneShape.DirectionalPad:
                    DrawArtworkDirection(dc, DirectionZoneBounds(bounds, zone.Control), active: true);
                    break;
                default:
                    DrawArtworkDirection(dc, bounds, active: true);
                    break;
            }
        }
    }

    private void DrawArtworkProfileJoystick(DrawingContext dc, Rect bounds)
    {
        var left = Math.Max(VisualZoneValue(EmulationControllerVisualControl.DirectionLeft),
            HoveredVisualControl == EmulationControllerVisualControl.DirectionLeft ? 1f : 0f);
        var right = Math.Max(VisualZoneValue(EmulationControllerVisualControl.DirectionRight),
            HoveredVisualControl == EmulationControllerVisualControl.DirectionRight ? 1f : 0f);
        var up = Math.Max(VisualZoneValue(EmulationControllerVisualControl.DirectionUp),
            HoveredVisualControl == EmulationControllerVisualControl.DirectionUp ? 1f : 0f);
        var down = Math.Max(VisualZoneValue(EmulationControllerVisualControl.DirectionDown),
            HoveredVisualControl == EmulationControllerVisualControl.DirectionDown ? 1f : 0f);
        var x = Math.Clamp(right - left, -1f, 1f);
        var y = Math.Clamp(down - up, -1f, 1f);
        if (Math.Abs(x) <= .01f && Math.Abs(y) <= .01f) return;

        var size = Math.Min(bounds.Width, bounds.Height);
        var movement = size * .2d;
        var center = new Point(
            bounds.X + bounds.Width / 2d + x * movement,
            bounds.Y + bounds.Height / 2d + y * movement);
        var radius = Math.Max(6d, size * .2d);
        dc.DrawEllipse(PressedFill(), new Pen(PressedStrong(), Math.Max(1.5d, size * .025d)),
            center, radius, radius);
    }

    private float VisualZoneValue(EmulationControllerVisualControl control)
    {
        if (_visualCommandIds is null
            || !_visualCommandIds.TryGetValue(control, out var commandId))
            return 0f;
        return Math.Clamp(
            Math.Abs(_visualState?.EmulatedCommandValue(commandId) ?? 0f),
            0f,
            1f);
    }

    private static Rect DirectionZoneBounds(
        Rect bounds,
        EmulationControllerVisualControl control)
    {
        var horizontalWidth = bounds.Width * .44d;
        var horizontalHeight = bounds.Height * .18d;
        var verticalWidth = bounds.Width * .18d;
        var verticalHeight = bounds.Height * .44d;
        return control switch
        {
            EmulationControllerVisualControl.DirectionUp => new Rect(
                bounds.X + (bounds.Width - verticalWidth) / 2d,
                bounds.Y,
                verticalWidth,
                verticalHeight),
            EmulationControllerVisualControl.DirectionDown => new Rect(
                bounds.X + (bounds.Width - verticalWidth) / 2d,
                bounds.Bottom - verticalHeight,
                verticalWidth,
                verticalHeight),
            EmulationControllerVisualControl.DirectionLeft => new Rect(
                bounds.X,
                bounds.Y + (bounds.Height - horizontalHeight) / 2d,
                horizontalWidth,
                horizontalHeight),
            EmulationControllerVisualControl.DirectionRight => new Rect(
                bounds.Right - horizontalWidth,
                bounds.Y + (bounds.Height - horizontalHeight) / 2d,
                horizontalWidth,
                horizontalHeight),
            _ => bounds
        };
    }


    private static Rect FitArtworkBounds(ImageSource artwork)
    {
        var sourceWidth = artwork is BitmapSource bitmap ? bitmap.PixelWidth : artwork.Width;
        var sourceHeight = artwork is BitmapSource source ? source.PixelHeight : artwork.Height;
        var scale = Math.Min(540d / sourceWidth, 316d / sourceHeight);
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new Rect((620d - width) / 2d, (320d - height) / 2d, width, height);
    }

    private static Rect FitArtworkProfileBounds(ImageSource artwork)
    {
        var sourceWidth = artwork is BitmapSource bitmap ? bitmap.PixelWidth : artwork.Width;
        var sourceHeight = artwork is BitmapSource source ? source.PixelHeight : artwork.Height;
        var scale = Math.Min(580d / sourceWidth, 480d / sourceHeight);
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new Rect((620d - width) / 2d, (520d - height) / 2d, width, height);
    }

    private static Point At(Rect bounds, double x, double y) =>
        new(bounds.X + x * bounds.Width, bounds.Y + y * bounds.Height);

    private static double Radius(Rect bounds, double normalized) =>
        normalized * Math.Min(bounds.Width, bounds.Height);

}
