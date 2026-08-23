using GWGUI.App.Services.Input.GameInput;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Options;

public sealed partial class ControllerVisualizer
{
    private static readonly Geometry ClassicPlayStationBody = Geometry.Parse("M105,104 C135,68 201,62 250,83 C286,98 334,98 370,83 C419,62 485,68 515,104 C539,133 550,224 519,266 C493,301 451,281 426,242 L392,190 C372,159 347,148 310,148 C273,148 248,159 228,190 L194,242 C169,281 127,301 101,266 C70,224 81,133 105,104 Z");

    private void DrawPlayStationClassic(DrawingContext dc, bool analog)
    {
        var input = Input;
        var shell = analog ? ColorBrush(Color.FromRgb(34, 34, 37)) : ColorBrush(Color.FromRgb(181, 181, 174));
        var ink = analog ? ColorBrush(Color.FromRgb(197, 197, 199)) : ColorBrush(Color.FromRgb(58, 58, 61));
        dc.DrawGeometry(shell, new Pen(ink, 3), ClassicPlayStationBody);
        dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromArgb(70, 255, 255, 255)), 2),
            Geometry.Parse("M112,112 C142,83 199,79 247,96 C285,109 335,109 373,96 C421,79 478,83 508,112"));
        DrawDarkDpad(dc, 158, 144);
        DrawButton(dc, 460, 181, "×", input.Button(GameInputGamepadButtons.A, 0), 19,
            ColorBrush(Color.FromRgb(40, 41, 44)));
        DrawButton(dc, 501, 140, "○", input.Button(GameInputGamepadButtons.B, 1), 19,
            ColorBrush(Color.FromRgb(40, 41, 44)));
        DrawButton(dc, 419, 140, "□", input.Button(GameInputGamepadButtons.X, 2), 19,
            ColorBrush(Color.FromRgb(40, 41, 44)));
        DrawButton(dc, 460, 99, "△", input.Button(GameInputGamepadButtons.Y, 3), 19,
            ColorBrush(Color.FromRgb(40, 41, 44)));
        DrawButton(dc, 274, 142, "SELECT", input.Button(GameInputGamepadButtons.View, 6), 11,
            ColorBrush(Color.FromRgb(45, 45, 48)));
        DrawButton(dc, 346, 142, "START", input.Button(GameInputGamepadButtons.Menu, 7), 11,
            ColorBrush(Color.FromRgb(45, 45, 48)));
        if (analog)
        {
            DrawDarkStick(dc, 258, 217, input.LeftX, input.LeftY,
                input.Button(GameInputGamepadButtons.LeftThumbstick, 8));
            DrawDarkStick(dc, 362, 217, input.RightX, input.RightY,
                input.Button(GameInputGamepadButtons.RightThumbstick, 9));
            DrawButton(dc, 310, 181, "ANALOG", input.RawButton(16), 12,
                ColorBrush(Color.FromRgb(42, 42, 45)));
            dc.DrawEllipse(ColorBrush(Color.FromRgb(205, 45, 39)), null, new Point(310, 202), 3, 3);
        }
        DrawShoulder(dc, new Rect(116, 69, 140, 25), "L1",
            input.Button(GameInputGamepadButtons.LeftShoulder, 4));
        DrawShoulder(dc, new Rect(364, 69, 140, 25), "R1",
            input.Button(GameInputGamepadButtons.RightShoulder, 5));
        DrawText(dc, "SONY", new Point(310, 112), 12, ink);
    }

    private void DrawMasterSystem(DrawingContext dc)
    {
        var input = Input;
        dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(34, 35, 37)),
            new Pen(ColorBrush(Color.FromRgb(8, 8, 9)), 4), new Rect(72, 82, 476, 174), 17, 17);
        dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(74, 75, 78)),
            new Pen(ColorBrush(Color.FromRgb(150, 151, 153)), 2), new Rect(93, 101, 434, 134), 9, 9);
        dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(28, 29, 31)), null,
            new Rect(107, 113, 198, 111), 6, 6);
        DrawDarkDpad(dc, 174, 169);
        DrawButton(dc, 416, 168, "1", input.RawButton(0), 27,
            ColorBrush(Color.FromRgb(170, 41, 43)));
        DrawButton(dc, 481, 168, "2", input.RawButton(1), 27,
            ColorBrush(Color.FromRgb(170, 41, 43)));
        DrawText(dc, "SEGA", new Point(310, 122), 14, ColorBrush(Color.FromRgb(221, 222, 224)));
        DrawText(dc, "CONTROL PAD", new Point(448, 216), 10, ColorBrush(Color.FromRgb(221, 222, 224)));
    }

    private void DrawMegaDrive(DrawingContext dc, bool sixButtons)
    {
        var input = Input;
        var body = Geometry.Parse("M75,115 C92,75 143,60 222,63 L398,63 C477,60 528,75 545,115 C566,164 550,229 503,251 C466,269 431,253 406,225 C383,199 352,189 310,189 C268,189 237,199 214,225 C189,253 154,269 117,251 C70,229 54,164 75,115 Z");
        dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(62, 62, 66), Color.FromRgb(15, 15, 17), 90), new Pen(ColorBrush(Color.FromRgb(8, 8, 9)), 5), body);
        dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromArgb(95, 255, 255, 255)), 2),
            Geometry.Parse("M92,119 C112,87 159,78 226,80 L394,80 C461,78 508,87 528,119"));
        dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(22, 22, 24)), null, new Rect(114, 92, 392, 124), 54, 54);
        dc.DrawRoundedRectangle(ColorBrush(Color.FromRgb(47, 47, 51)), new Pen(ColorBrush(Color.FromRgb(5, 5, 6)), 2), new Rect(266, 91, 88, 34), 16, 16);
        DrawText(dc, "SEGA", new Point(310, 108), 15, ColorBrush(Color.FromRgb(85, 123, 218)));
        DrawDarkDpad(dc, 167, 157);

        if (sixButtons)
        {
            var topLabels = new[] { "X", "Y", "Z" };
            var bottomLabels = new[] { "A", "B", "C" };
            for (var index = 0; index < 3; index++)
            {
                DrawButton(dc, 398 + index * 49, 130, topLabels[index], input.RawButton(index), 16,
                    ColorBrush(Color.FromRgb(66, 66, 72)));
                DrawButton(dc, 384 + index * 54, 179, bottomLabels[index], input.RawButton(index + 3), 22,
                    ColorBrush(Color.FromRgb(44, 44, 48)));
            }
            DrawButton(dc, 310, 154, "MODE", input.RawButton(7), 10,
                ColorBrush(Color.FromRgb(35, 35, 38)));
            DrawButton(dc, 310, 184, "START", input.RawButton(6), 18,
                ColorBrush(Color.FromRgb(35, 35, 38)));
        }
        else
        {
            DrawButton(dc, 402, 169, "A", input.RawButton(0), 24,
                ColorBrush(Color.FromRgb(45, 45, 48)));
            DrawButton(dc, 463, 151, "B", input.RawButton(1), 27,
                ColorBrush(Color.FromRgb(45, 45, 48)));
            DrawButton(dc, 520, 127, "C", input.RawButton(2), 24,
                ColorBrush(Color.FromRgb(45, 45, 48)));
            DrawButton(dc, 310, 171, "START", input.RawButton(3), 20,
                ColorBrush(Color.FromRgb(35, 35, 38)));
        }
    }

    private void DrawSaturn(DrawingContext dc)
    {
        var input = Input;
        var saturnBody = Geometry.Parse("M75,124 C93,80 152,68 226,76 C275,81 345,81 394,76 C468,68 527,80 545,124 C564,171 543,229 497,251 C454,271 417,246 389,218 C368,197 341,190 310,190 C279,190 252,197 231,218 C203,246 166,271 123,251 C77,229 56,171 75,124 Z");
        dc.DrawGeometry(ColorBrush(Color.FromRgb(83, 84, 88)),
            new Pen(ColorBrush(Color.FromRgb(24, 24, 26)), 4), saturnBody);
        dc.DrawGeometry(null, new Pen(ColorBrush(Color.FromArgb(80, 255, 255, 255)), 2),
            Geometry.Parse("M91,124 C116,93 163,88 225,94 C276,99 344,99 395,94 C457,88 504,93 529,124"));
        DrawShoulder(dc, new Rect(99, 64, 122, 30), "L",
            input.Button(GameInputGamepadButtons.LeftShoulder, 7));
        DrawShoulder(dc, new Rect(399, 64, 122, 30), "R",
            input.Button(GameInputGamepadButtons.RightShoulder, 8));
        DrawDarkDpad(dc, 160, 169, disc: true);
        var labels = new[] { "X", "Y", "Z", "A", "B", "C" };
        for (var index = 0; index < labels.Length; index++)
        {
            var row = index < 3 ? 0 : 1;
            var column = index % 3;
            var idle = row == 0 ? ColorBrush(Color.FromRgb(66, 67, 70)) :
                new[] { ColorBrush(Color.FromRgb(57, 119, 195)), ColorBrush(Color.FromRgb(230, 195, 51)), ColorBrush(Color.FromRgb(208, 53, 49)) }[column];
            DrawButton(dc, 389 + column * 52, 132 + row * 57, labels[index],
                input.RawButton(index), row == 0 ? 17 : 21, idle);
        }
        DrawButton(dc, 306, 176, "▶", input.RawButton(6), 14,
            ColorBrush(Color.FromRgb(38, 39, 42)));
        DrawText(dc, "SEGA SATURN", new Point(310, 111), 12,
            ColorBrush(Color.FromRgb(214, 215, 217)));
    }

    private void DrawDreamcast(DrawingContext dc)
    {
        var input = Input;
        var body = Geometry.Parse("M92,110 C124,73 203,70 260,92 C291,104 329,104 360,92 C417,70 496,73 528,110 C555,142 554,236 515,273 C487,299 450,282 424,242 L389,190 C372,165 349,151 310,151 C271,151 248,165 231,190 L196,242 C170,282 133,299 105,273 C66,236 65,142 92,110 Z");
        dc.DrawGeometry(Brushes.WhiteSmoke, Stroke(), body);
        DrawDarkStick(dc, 170, 131, input.LeftX, input.LeftY,
            input.Button(GameInputGamepadButtons.LeftThumbstick, 8));
        DrawDarkDpad(dc, 175, 217);
        dc.DrawRoundedRectangle(Control(), Stroke(), new Rect(245, 100, 130, 88), 7, 7);
        DrawText(dc, "VMU", new Point(310, 144), 15);
        DrawButton(dc, 447, 177, "A", input.Button(GameInputGamepadButtons.A, 0), 19, Brushes.Green);
        DrawButton(dc, 487, 137, "B", input.Button(GameInputGamepadButtons.B, 1), 19, Brushes.Red);
        DrawButton(dc, 407, 137, "X", input.Button(GameInputGamepadButtons.X, 2), 19, Brushes.Blue);
        DrawButton(dc, 447, 97, "Y", input.Button(GameInputGamepadButtons.Y, 3), 19, Brushes.Goldenrod);
        DrawButton(dc, 310, 214, "▶", input.Button(GameInputGamepadButtons.Menu, 7), 14);
        DrawTrigger(dc, 145, 71, "L", input.LeftTrigger);
        DrawTrigger(dc, 475, 71, "R", input.RightTrigger);
    }
}
