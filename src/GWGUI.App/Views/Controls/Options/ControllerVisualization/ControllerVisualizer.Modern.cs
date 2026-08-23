using GWGUI.App.Services.Input.GameInput;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Options;

public sealed partial class ControllerVisualizer
{
    private static readonly Geometry XboxBody = Geometry.Parse("M104,91 C115,62 145,43 185,40 C218,37 248,49 269,61 C292,74 328,74 351,61 C372,49 402,37 435,40 C475,43 505,62 516,91 C535,139 557,234 530,277 C516,300 492,310 472,299 C456,290 443,268 431,245 L397,181 C383,156 358,144 326,144 L294,144 C262,144 237,156 223,181 L189,245 C177,268 164,290 148,299 C128,310 104,300 90,277 C63,234 85,139 104,91 Z");
    private static readonly Geometry PlayStationBody = Geometry.Parse("M112,94 C145,54 207,52 255,73 C286,86 334,86 365,73 C413,52 475,54 508,94 C535,128 548,230 515,274 C492,304 456,290 433,252 L391,183 C374,156 349,145 310,145 C271,145 246,156 229,183 L187,252 C164,290 128,304 105,274 C72,230 85,128 112,94 Z");

    private void DrawXbox(DrawingContext dc, bool series, bool rematchCore = false)
    {
        var input = Input;
        var shell = new LinearGradientBrush(
            series ? Color.FromRgb(48, 50, 53) : Color.FromRgb(42, 45, 48),
            Color.FromRgb(10, 11, 13), 90);
        shell.Freeze();
        var edge = new Pen(ColorBrush(Color.FromRgb(205, 208, 211)), 4);
        edge.LineJoin = PenLineJoin.Round;
        dc.DrawGeometry(shell, edge, XboxBody);
        if (rematchCore)
        {
            dc.DrawLine(new Pen(ColorBrush(Color.FromRgb(44, 46, 49)), 8),
                new Point(310, 42), new Point(310, 4));
            dc.DrawEllipse(input.SystemButton(GameInputSystemButtons.Guide) ? Accent() : ColorBrush(Color.FromRgb(34, 143, 161)), null,
                new Point(310, 104), 21, 21);
            DrawText(dc, "TB", new Point(310, 104), 10,
                ColorBrush(Color.FromRgb(235, 240, 241)));
        }

        // Épaisseur supérieure et gâchettes, visibles comme sur la manette réelle.
        dc.DrawGeometry(ColorBrush(Color.FromRgb(21, 23, 26)),
            new Pen(ColorBrush(Color.FromRgb(122, 126, 132)), 2),
            Geometry.Parse("M119,89 C143,58 184,52 226,62 L268,75 L352,75 L394,62 C436,52 477,58 501,89 L477,101 C451,78 421,75 390,84 L352,95 L268,95 L230,84 C199,75 169,78 143,101 Z"));
        // Les commandes supérieures sont intégrées au bord de la coque au lieu de flotter en gros blocs.
        var muted = ColorBrush(Color.FromRgb(150, 153, 158));
        dc.DrawRoundedRectangle(
            input.TriggerPressed(left: true) ? Accent() : ColorBrush(Color.FromRgb(55, 58, 62)),
            null, new Rect(151, 70, 50, 4), 2, 2);
        dc.DrawRoundedRectangle(
            input.TriggerPressed(left: false) ? Accent() : ColorBrush(Color.FromRgb(55, 58, 62)),
            null, new Rect(419, 70, 50, 4), 2, 2);
        DrawText(dc, "LT", new Point(176, 82), 10,
            input.TriggerPressed(left: true) ? Accent() : muted);
        DrawText(dc, "LB", new Point(226, 91), 10,
            input.Button(GameInputGamepadButtons.LeftShoulder, 4) ? Accent() : muted);
        DrawText(dc, "RB", new Point(394, 91), 10,
            input.Button(GameInputGamepadButtons.RightShoulder, 5) ? Accent() : muted);
        DrawText(dc, "RT", new Point(444, 82), 10,
            input.TriggerPressed(left: false) ? Accent() : muted);

        // Reflets et textures des poignées.
        dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromArgb(70, 255, 255, 255)), 2),
            Geometry.Parse("M108,107 C92,158 81,227 100,270 C112,295 132,299 149,284"));
        dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromArgb(70, 255, 255, 255)), 2),
            Geometry.Parse("M512,107 C528,158 539,227 520,270 C508,295 488,299 471,284"));
        for (var dot = 0; dot < 6; dot++)
        {
            dc.DrawEllipse(ColorBrush(Color.FromArgb(70, 220, 220, 220)), null,
                new Point(118 + dot * 7, 249 + dot * 5), 1.3, 1.3);
            dc.DrawEllipse(ColorBrush(Color.FromArgb(70, 220, 220, 220)), null,
                new Point(502 - dot * 7, 249 + dot * 5), 1.3, 1.3);
        }

        DrawXboxStick(dc, 188, 139, input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8));
        DrawXboxStick(dc, 354, 211, input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9));

        if (series) DrawXboxDiscDpad(dc, 210, 217);
        else DrawDarkDpad(dc, 210, 217);

        DrawXboxFaceButton(dc, 444, 172, "A", input.Button(GameInputGamepadButtons.A, 0), Color.FromRgb(57, 173, 82));
        DrawXboxFaceButton(dc, 484, 132, "B", input.Button(GameInputGamepadButtons.B, 1), Color.FromRgb(220, 66, 62));
        DrawXboxFaceButton(dc, 404, 132, "X", input.Button(GameInputGamepadButtons.X, 2), Color.FromRgb(55, 137, 220));
        DrawXboxFaceButton(dc, 444, 92, "Y", input.Button(GameInputGamepadButtons.Y, 3), Color.FromRgb(230, 190, 49));

        DrawButton(dc, 279, 137, "▣", input.Button(GameInputGamepadButtons.View, 6), 13,
            ColorBrush(Color.FromRgb(31, 33, 36)));
        DrawButton(dc, 341, 137, "☰", input.Button(GameInputGamepadButtons.Menu, 7), 13,
            ColorBrush(Color.FromRgb(31, 33, 36)));
        if (series)
            DrawButton(dc, 310, 174, "↥",
                input.SystemButton(GameInputSystemButtons.Share) ||
                input.LabelButton(GameInputLabel.Share) || input.RawButton(14), 12,
                ColorBrush(Color.FromRgb(31, 33, 36)));

        var extraButtons = new[]
        {
            ("C", input.Button(GameInputGamepadButtons.C, 16), 246d),
            ("Z", input.Button(GameInputGamepadButtons.Z, 17), 274d),
            ("P1", input.Button(GameInputGamepadButtons.PaddleLeft1, 18), 302d),
            ("P2", input.Button(GameInputGamepadButtons.PaddleLeft2, 19), 330d),
            ("P3", input.Button(GameInputGamepadButtons.PaddleRight1, 20), 358d),
            ("P4", input.Button(GameInputGamepadButtons.PaddleRight2, 21), 386d)
        };
        foreach (var (label, active, x) in extraButtons)
            if (active) DrawButton(dc, x, 276, label, true, 10,
                ColorBrush(Color.FromRgb(31, 33, 36)));

        // Bouton Xbox : cercle sombre et quatre branches courbes du glyphe.
        if (!rematchCore)
        {
        dc.DrawEllipse(input.SystemButton(GameInputSystemButtons.Guide) ? Accent() : ColorBrush(Color.FromRgb(25, 27, 30)),
            new Pen(ColorBrush(Color.FromRgb(178, 181, 185)), 2), new Point(310, 104), 22, 22);
        var logoPen = new Pen(ColorBrush(Color.FromRgb(226, 228, 229)), 4)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        dc.DrawGeometry(null, logoPen, Geometry.Parse("M296,91 C302,94 307,99 310,104"));
        dc.DrawGeometry(null, logoPen, Geometry.Parse("M324,91 C318,94 313,99 310,104"));
        dc.DrawGeometry(null, logoPen, Geometry.Parse("M296,118 C301,113 306,108 310,104"));
        dc.DrawGeometry(null, logoPen, Geometry.Parse("M324,118 C319,113 314,108 310,104"));
        }
    }

    private void DrawXboxStick(DrawingContext dc, double x, double y, float axisX, float axisY, bool pressed)
    {
        dc.DrawEllipse(ColorBrush(Color.FromRgb(12, 13, 15)),
            new Pen(ColorBrush(Color.FromRgb(112, 116, 121)), 4), new Point(x, y), 39, 39);
        dc.DrawEllipse(pressed ? Accent() : ColorBrush(Color.FromRgb(38, 40, 43)),
            new Pen(ColorBrush(Color.FromRgb(178, 181, 184)), 2),
            new Point(x + axisX * 16, y + axisY * 16), 25, 25);
        dc.DrawEllipse(null, new Pen(ColorBrush(Color.FromArgb(90, 255, 255, 255)), 1),
            new Point(x + axisX * 16, y + axisY * 16), 18, 18);
    }

    private void DrawXboxDiscDpad(DrawingContext dc, double x, double y)
    {
        dc.DrawEllipse(ColorBrush(Color.FromRgb(17, 18, 20)),
            new Pen(ColorBrush(Color.FromRgb(112, 116, 121)), 4), new Point(x, y), 50, 50);
        var input = Input;
        var directions = new[]
        {
            (new Rect(x - 13, y - 41, 26, 34), input.Direction(GameInputSwitchPosition.Up, GameInputGamepadButtons.DPadUp, 10)),
            (new Rect(x - 13, y + 7, 26, 34), input.Direction(GameInputSwitchPosition.Down, GameInputGamepadButtons.DPadDown, 12)),
            (new Rect(x - 41, y - 13, 34, 26), input.Direction(GameInputSwitchPosition.Left, GameInputGamepadButtons.DPadLeft, 13)),
            (new Rect(x + 7, y - 13, 34, 26), input.Direction(GameInputSwitchPosition.Right, GameInputGamepadButtons.DPadRight, 11))
        };
        foreach (var (rect, active) in directions)
            dc.DrawRoundedRectangle(active ? Accent() : ColorBrush(Color.FromRgb(61, 64, 68)),
                null, rect, 4, 4);
        dc.DrawEllipse(ColorBrush(Color.FromRgb(61, 64, 68)), null, new Point(x, y), 15, 15);
    }

    private void DrawXboxFaceButton(
        DrawingContext dc, double x, double y, string label, bool pressed, Color color)
    {
        var fill = pressed ? Accent() : ColorBrush(Color.FromRgb(27, 29, 32));
        dc.DrawEllipse(fill, new Pen(ColorBrush(Color.FromRgb(171, 174, 178)), 2),
            new Point(x, y), 20, 20);
        DrawText(dc, label, new Point(x, y), 15, ColorBrush(color));
    }

    private void DrawPlayStation(DrawingContext dc, bool dualSense)
    {
        var input = Input;
        var outer = dualSense ? ColorBrush(Color.FromRgb(232, 234, 235)) : ColorBrush(Color.FromRgb(34, 35, 39));
        var center = dualSense ? ColorBrush(Color.FromRgb(20, 21, 24)) : ColorBrush(Color.FromRgb(42, 43, 48));
        var outline = new Pen(dualSense ? ColorBrush(Color.FromRgb(92, 95, 101)) : ColorBrush(Color.FromRgb(178, 180, 185)), 3);
        dc.DrawGeometry(outer, outline, PlayStationBody);

        if (dualSense)
        {
            dc.DrawGeometry(center, null,
                Geometry.Parse("M220,65 C250,78 279,85 310,85 C341,85 370,78 400,65 L384,183 C372,222 344,246 310,246 C276,246 248,222 236,183 Z"));
            dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromRgb(40, 103, 191)), 2),
                Geometry.Parse("M231,78 C253,90 280,96 310,96 C340,96 367,90 389,78"));
        }

        dc.DrawRoundedRectangle(dualSense ? ColorBrush(Color.FromRgb(31, 32, 36)) : ColorBrush(Color.FromRgb(25, 26, 29)),
            new Pen(ColorBrush(Color.FromRgb(117, 120, 126)), 2),
            dualSense ? new Rect(242, 80, 136, 71) : new Rect(234, 72, 152, 68), 14, 14);
        dc.DrawLine(new Pen(ColorBrush(Color.FromArgb(80, 255, 255, 255)), 1),
            new Point(255, 94), new Point(365, 94));

        dc.DrawRoundedRectangle(input.LeftTrigger > .04f ? Accent() : ColorBrush(Color.FromRgb(29, 31, 34)), outline, new Rect(124, 47, 111, 27), 11, 11);
        dc.DrawRoundedRectangle(input.RightTrigger > .04f ? Accent() : ColorBrush(Color.FromRgb(29, 31, 34)), outline, new Rect(385, 47, 111, 27), 11, 11);
        DrawText(dc, "L2", new Point(180, 60), 11);
        DrawText(dc, "R2", new Point(440, 60), 11);
        dc.DrawRoundedRectangle(input.Button(GameInputGamepadButtons.LeftShoulder, 4) ? Accent() : ColorBrush(Color.FromRgb(29, 31, 34)), outline,
            new Rect(132, 70, 122, 25), 10, 10);
        dc.DrawRoundedRectangle(input.Button(GameInputGamepadButtons.RightShoulder, 5) ? Accent() : ColorBrush(Color.FromRgb(29, 31, 34)), outline,
            new Rect(366, 70, 122, 25), 10, 10);
        DrawText(dc, "L1", new Point(193, 82), 11);
        DrawText(dc, "R1", new Point(427, 82), 11);

        DrawDarkDpad(dc, 157, 142);
        DrawDarkStick(dc, 257, 216, input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8));
        DrawDarkStick(dc, 363, 216, input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9));

        var symbolColor = dualSense ? ColorBrush(Color.FromRgb(115, 119, 127)) : ColorBrush(Color.FromRgb(210, 212, 217));
        DrawPlayStationFaceButton(dc, 461, 180, "×", input.Button(GameInputGamepadButtons.A, 0), symbolColor);
        DrawPlayStationFaceButton(dc, 501, 140, "○", input.Button(GameInputGamepadButtons.B, 1), symbolColor);
        DrawPlayStationFaceButton(dc, 421, 140, "□", input.Button(GameInputGamepadButtons.X, 2), symbolColor);
        DrawPlayStationFaceButton(dc, 461, 100, "△", input.Button(GameInputGamepadButtons.Y, 3), symbolColor);

        DrawButton(dc, 208, 112, dualSense ? "↗" : "SH",
            input.Button(GameInputGamepadButtons.View, 6), 12,
            ColorBrush(Color.FromRgb(35, 36, 40)));
        DrawButton(dc, 412, 112, dualSense ? "≡" : "OP",
            input.Button(GameInputGamepadButtons.Menu, 7), 12,
            ColorBrush(Color.FromRgb(35, 36, 40)));
        dc.DrawEllipse(ColorBrush(Color.FromRgb(26, 27, 31)),
            new Pen(ColorBrush(Color.FromRgb(130, 133, 139)), 2), new Point(310, 174), 18, 18);
        DrawText(dc, "PS", new Point(310, 174), 10, ColorBrush(Color.FromRgb(200, 202, 205)));

        if (dualSense)
        {
            dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(35, 37, 41)), null,
                new Rect(268, 260, 84, 8), 4, 4);
            dc.DrawEllipse(ColorBrush(Color.FromRgb(33, 92, 184)), null, new Point(310, 269), 3, 3);
        }
    }

    private void DrawPlayStationFaceButton(
        DrawingContext dc, double x, double y, string symbol, bool pressed, Brush symbolBrush)
    {
        dc.DrawEllipse(pressed ? Accent() : ColorBrush(Color.FromRgb(29, 30, 34)),
            new Pen(ColorBrush(Color.FromRgb(125, 128, 134)), 2), new Point(x, y), 19, 19);
        DrawText(dc, symbol, new Point(x, y), 15, symbolBrush);
    }

    private void DrawGenericGamepad(DrawingContext dc)
    {
        var input = Input;
        dc.DrawGeometry(Card(), Stroke(), PlayStationBody);
        DrawDpad(dc, 165, 145);
        DrawStick(dc, 250, 210, input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8));
        DrawStick(dc, 355, 210, input.RightX, input.RightY,
            input.Button(GameInputGamepadButtons.RightThumbstick, 9));
        for (var index = 0; index < 4; index++)
        {
            var positions = new[] { new Point(455, 177), new Point(495, 137), new Point(415, 137), new Point(455, 97) };
            var standardButtons = new[]
            {
                GameInputGamepadButtons.A, GameInputGamepadButtons.B,
                GameInputGamepadButtons.X, GameInputGamepadButtons.Y
            };
            DrawButton(dc, positions[index].X, positions[index].Y, (index + 1).ToString(),
                input.Button(standardButtons[index], index));
        }
        DrawTrigger(dc, 155, 45, "L2", input.LeftTrigger);
        DrawTrigger(dc, 465, 45, "R2", input.RightTrigger);
        DrawShoulder(dc, new Rect(115, 66, 145, 28), "L1",
            input.Button(GameInputGamepadButtons.LeftShoulder, 4));
        DrawShoulder(dc, new Rect(360, 66, 145, 28), "R1",
            input.Button(GameInputGamepadButtons.RightShoulder, 5));
    }
}
