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
