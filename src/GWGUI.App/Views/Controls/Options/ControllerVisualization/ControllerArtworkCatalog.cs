using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static class ControllerArtworkCatalog
{
    private static readonly IReadOnlyDictionary<ControllerVisualModel, string> ModelResources =
        new Dictionary<ControllerVisualModel, string>
        {
            [ControllerVisualModel.GenericGamepad] = "generic-gamepad.png",
            [ControllerVisualModel.XboxSeries] = "xbox-series.png",
            [ControllerVisualModel.XboxOne] = "xbox-one.png",
            [ControllerVisualModel.Xbox360] = "xbox-360-black.png",
            [ControllerVisualModel.Xbox360White] = "xbox-360-white.png",
            [ControllerVisualModel.XboxRematchCore] = "xbox-rematch-core.png",
            [ControllerVisualModel.PlayStation4] = "playstation-4.png",
            [ControllerVisualModel.PlayStation5] = "playstation-5.png",
            [ControllerVisualModel.MasterSystem] = "master-system.png",
            [ControllerVisualModel.NintendoEntertainmentSystem] = "nintendo-entertainment-system.png",
            [ControllerVisualModel.Nintendo64] = "nintendo-64.png",
            [ControllerVisualModel.SuperNintendo] = "super-nintendo.png",
            [ControllerVisualModel.MegaDrive3] = "mega-drive-3.png",
            [ControllerVisualModel.MegaDrive6] = "mega-drive-6.png",
            [ControllerVisualModel.PlayStation1] = "playstation-1.png",
            [ControllerVisualModel.PlayStation2] = "playstation-2.png",
            [ControllerVisualModel.Saturn] = "saturn.png",
            [ControllerVisualModel.Dreamcast] = "dreamcast.png",
            [ControllerVisualModel.RacingWheel] = "racing-wheel.png",
            [ControllerVisualModel.FlightStick] = "flight-stick.png",
            [ControllerVisualModel.ArcadeStick] = "arcade-stick.png"
        };

    private static readonly IReadOnlyList<ControllerVisualZone> NecCoreGrafxTurboPadZones =
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.7d, 62.8d, 9.1d, 11.8d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.8d, 62.8d, 9.1d, 11.8d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.4d, 66.1d, 8.1d, 5.7d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.2d, 66.1d, 8.1d, 5.7d)
    ];

    private static readonly IReadOnlyDictionary<string, ProfileDefinition> ProfileDefinitions =
        new Dictionary<string, ProfileDefinition>(StringComparer.Ordinal)
        {
            [EmulationControllerVisualIds.NecPcEnginePad] = new("nec-pc-engine-pad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.4d, 50.0d, 19.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.4d, 50.0d, 19.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.4d, 50.0d, 19.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.4d, 50.0d, 19.0d, 27.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.2d, 62.7d, 10.1d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.8d, 62.7d, 10.1d, 13.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.5d, 65.6d, 8.1d, 5.8d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.3d, 65.6d, 8.1d, 5.8d)
            ]),
            [EmulationControllerVisualIds.NecPcEngineTurboPad] = new("nec-pc-engine-turbopad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.4d, 62.3d, 8.8d, 13.2d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.9d, 62.3d, 8.8d, 13.2d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.2d, 66.0d, 8.0d, 5.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.9d, 66.0d, 8.0d, 5.1d)
            ]),
            [EmulationControllerVisualIds.NecTurboGrafxTurboPad] = new("nec-turbografx-turbopad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.1d, 51.2d, 17.9d, 31.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.1d, 51.2d, 17.9d, 31.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.1d, 51.2d, 17.9d, 31.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.1d, 51.2d, 17.9d, 31.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.8d, 65.1d, 8.3d, 17.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 81.3d, 65.1d, 8.3d, 17.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.0d, 70.0d, 7.6d, 7.8d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.0d, 70.0d, 7.6d, 7.8d)
            ]),
            [EmulationControllerVisualIds.NecCoreGrafxTurboPad] =
                new("nec-coregrafx-turbopad.png", NecCoreGrafxTurboPadZones),
            [EmulationControllerVisualIds.NecCoreGrafxIiTurboPad] =
                new("nec-coregrafx-ii-turbopad.png", NecCoreGrafxTurboPadZones),
            [EmulationControllerVisualIds.NecDuoRTurboPad] =
                new("nec-duor-turbopad.png", NecCoreGrafxTurboPadZones),
            [EmulationControllerVisualIds.NecPcEngineTurboPadIi] = new("nec-pc-engine-turbopad-ii.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.5d, 62.1d, 8.6d, 14.1d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.0d, 62.1d, 8.6d, 14.1d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.0d, 66.3d, 8.3d, 6.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 66.3d, 8.3d, 6.3d)
            ]),
            [EmulationControllerVisualIds.NecPcEngineTurboStick] = new("nec-pc-engine-turbostick.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.7d, 58.4d, 10.1d, 13.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 49.8d, 29.2d, 5.5d, 6.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.4d, 46.2d, 10.7d, 13.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 74.1d, 29.2d, 5.2d, 6.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 48.5d, 52.8d, 5.0d, 5.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 55.6d, 43.2d, 5.0d, 5.6d)
            ]),
            [EmulationControllerVisualIds.NecAvenuePad3] = new("nec-avenue-pad-3.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.3d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.5d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.7d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.5d, 65.6d, 8.6d, 5.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.7d, 65.6d, 8.0d, 5.7d)
            ]),
            [EmulationControllerVisualIds.NecAvenuePad6] = new("nec-avenue-pad-6.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 65.5d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 74.3d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83.1d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65.5d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.3d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 83.1d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.5d, 65.6d, 8.6d, 5.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.7d, 65.6d, 8.0d, 5.7d)
            ]),
            [EmulationControllerVisualIds.NecArcadePad6] = new("nec-arcade-pad-6.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 75.5d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 84.7d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 84.7d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.1d, 73.5d, 7.2d, 6.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 73.5d, 7.2d, 6.7d)
            ]),
            [EmulationControllerVisualIds.NecTurboGrafxTurboStick] = new("nec-turbografx-turbostick.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.7d, 58.4d, 10.1d, 13.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.4d, 46.2d, 10.7d, 13.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 48.5d, 52.8d, 5.0d, 5.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 55.6d, 43.2d, 5.0d, 5.6d)
            ]),
            [EmulationControllerVisualIds.NecDuoPad] = new("nec-duopad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.6d, 65.4d, 8.0d, 16.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 81.4d, 65.4d, 8.0d, 16.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 70.1d, 7.7d, 6.9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 70.1d, 7.7d, 6.9d)
            ]),
            [EmulationControllerVisualIds.NecCordlessPad] = new("nec-cordless-pad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.8d, 61.9d, 9.5d, 13.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.8d, 61.9d, 9.5d, 13.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.4d, 65.2d, 9.1d, 6.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 65.2d, 9.1d, 6.3d)
            ]),
            [EmulationControllerVisualIds.NecTurboExpressControls] = new("nec-turboexpress-controls.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.3d, 62.3d, 9.0d, 6.3d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.1d, 57.6d, 9.0d, 6.3d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.0d, 84.3d, 8.3d, 3.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.2d, 84.3d, 8.3d, 3.3d)
            ]),
            [EmulationControllerVisualIds.NecPcEngineLtControls] = new("nec-pc-engine-lt-controls.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 65.3d, 69.1d, 6.8d, 6.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 75.4d, 66.2d, 6.8d, 6.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.8d, 77.2d, 6.5d, 3.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.3d, 77.2d, 6.5d, 3.0d)
            ]),
            [EmulationControllerVisualIds.NecPcFxPad] = new("nec-pc-fx-pad.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 68.9d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 77.9d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 86.8d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77.9d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 86.8d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.3d, 69.6d, 6.6d, 5.8d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 54.1d, 69.6d, 6.6d, 5.8d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 40.2d, 52.5d, 5.0d, 5.6d)
            ]),
            [EmulationControllerVisualIds.NecPcEngineMouse] = new("nec-pc-engine-mouse.png",
            [
                new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 23.3d, 18.4d, 25.1d, 27.1d),
                new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 51.9d, 18.4d, 25.1d, 27.1d)
            ]),
            [EmulationControllerVisualIds.NecPcFxMouse] = new("nec-pc-fx-mouse.png",
            [
                new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 21.4d, 18.0d, 27.5d, 27.5d),
                new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 18.0d, 27.5d, 27.5d)
            ]),
            [EmulationControllerVisualIds.XboxDuke] = new("xbox-duke.png",
            [
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 14.8d, 29.2d, 13.4d, 17.5d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 62.3d, 53.6d, 12.4d, 18.0d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 73.6d, 44.4d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 79.3d, 35.1d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67.7d, 35.1d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 73.5d, 25.8d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 83.9d, 44.5d, 5.6d, 8.3d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83.9d, 27.1d, 5.6d, 8.3d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 41.2d, 64.0d, 4.6d, 5.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 52.8d, 64.0d, 4.6d, 5.5d)
            ]),
            [EmulationControllerVisualIds.XboxControllerS] = new("xbox-controller-s.png",
            [
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 22.9d, 27.0d, 11.0d, 15.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 59.1d, 51.7d, 11.0d, 15.0d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 67.1d, 38.9d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.3d, 30.2d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 62.0d, 30.2d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 67.2d, 21.4d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 75.8d, 45.3d, 4.2d, 6.2d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 78.9d, 38.0d, 4.2d, 6.2d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 42.2d, 45.0d, 4.5d, 5.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 53.3d, 45.0d, 4.5d, 5.5d)
            ]),
            [EmulationControllerVisualIds.Xbox360TransformingDPad] = new("xbox-360-transforming-dpad.png",
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 44.7d, 20.0d, 10.6d, 17.0d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 18.4d, 17.9d, 10.5d, 17.3d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.1d, 39.9d, 11.0d, 18.4d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 72.1d, 33.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 79.3d, 23.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 23.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 73.0d, 11.4d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 37.8d, 24.0d, 5.0d, 8.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 58.1d, 24.0d, 5.0d, 8.1d)
            ]),
            [EmulationControllerVisualIds.Xbox360WirelessRacingWheel] = new("xbox-360-wireless-racing-wheel.png",
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 46.0d, 43.0d, 8.4d, 10.7d),
                new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.Ellipse, 10.9d, 1.1d, 78.9d, 86.9d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 47.6d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.0d, 43.5d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.1d, 43.5d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 39.2d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 35.0d, 45.8d, 3.5d, 5.2d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 61.1d, 45.8d, 3.5d, 5.2d)
            ]),
            [EmulationControllerVisualIds.Xbox360WirelessSpeedWheel] = new("xbox-360-wireless-speed-wheel.png",
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 46.4d, 62.6d, 7.4d, 10.9d),
                new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.Ellipse, 4.2d, 2.4d, 91.5d, 94.5d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 34.0d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 86.9d, 27.6d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 78.0d, 27.6d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 21.1d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 41.9d, 65.8d, 3.1d, 5.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 54.8d, 65.8d, 3.1d, 5.0d)
            ]),
            [EmulationControllerVisualIds.Xbox360BigButtonPad] = new("xbox-360-big-button-pad.png",
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 44.3d, 81.8d, 11.4d, 8.6d),
                new(EmulationControllerVisualControl.Buzzer, ControllerVisualZoneShape.Ellipse, 27.3d, 2.7d, 45.5d, 26.8d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 32.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 42.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 52.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 62.1d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 39.3d, 72.7d, 6.1d, 5.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 53.5d, 72.7d, 6.1d, 5.3d)
            ]),
            [EmulationControllerVisualIds.XboxDvdMoviePlaybackKit] = new("xbox-dvd-movie-playback-kit.png",
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 33.0d, 4.1d, 8.1d, 10.5d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 34.6d, 16.5d, 6.6d, 5.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 34.6d, 30.0d, 6.6d, 5.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 29.5d, 22.0d, 5.9d, 6.7d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 40.3d, 22.0d, 5.9d, 6.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 34.5d, 22.1d, 6.8d, 7.5d),
                new(EmulationControllerVisualControl.Rewind, ControllerVisualZoneShape.Ellipse, 29.9d, 37.5d, 4.8d, 5.3d),
                new(EmulationControllerVisualControl.FastForward, ControllerVisualZoneShape.Ellipse, 40.9d, 37.5d, 4.8d, 5.3d),
                new(EmulationControllerVisualControl.PlayPause, ControllerVisualZoneShape.Ellipse, 34.4d, 36.5d, 6.6d, 6.5d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.Ellipse, 34.5d, 44.0d, 6.5d, 5.5d),
                new(EmulationControllerVisualControl.Stop, ControllerVisualZoneShape.Ellipse, 34.5d, 51.2d, 6.5d, 5.5d)
            ]),
            [EmulationControllerVisualIds.Xbox360Chatpad] = new("xbox-360-chatpad.png",
            [
                new(EmulationControllerVisualControl.Keyboard, ControllerVisualZoneShape.RoundedRectangle, 15.0d, 28.0d, 70.0d, 43.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 25.8d, 69.7d, 4.6d, 8.3d)
            ]),
            [EmulationControllerVisualIds.XboxChatpad] = new("xbox-chatpad.png",
            [
                new(EmulationControllerVisualControl.Keyboard, ControllerVisualZoneShape.RoundedRectangle, 17.0d, 28.0d, 66.0d, 51.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.2d, 80.2d, 5.8d, 8.6d)
            ]),
            [EmulationControllerVisualIds.SonyPlayStationController] = new("playstation-1.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.8d, 26.4d, 18.7d, 28.1d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.8d, 26.4d, 18.7d, 28.1d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.8d, 26.4d, 18.7d, 28.1d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.8d, 26.4d, 18.7d, 28.1d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.0d, 46.0d, 7.1d, 12.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.2d, 35.7d, 7.1d, 12.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.2d, 35.7d, 7.1d, 12.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 73.7d, 25.1d, 7.1d, 12.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 17.0d, 14.2d, 10.8d, 7.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 72.8d, 14.2d, 10.8d, 7.0d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 17.0d, 9.0d, 10.8d, 5.0d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72.8d, 9.0d, 10.8d, 5.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.7d, 48.2d, 4.4d, 5.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 54.5d, 48.2d, 4.4d, 5.6d)
            ]),
            [EmulationControllerVisualIds.SonyDualShock2] = new("playstation-2.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 30.7d, 48.1d, 14.4d, 24.9d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.0d, 48.1d, 14.4d, 24.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 40.1d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 30.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.6d, 30.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 20.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 15.5d, 13.1d, 12.7d, 8.3d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 13.1d, 12.7d, 8.3d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 15.5d, 8.4d, 12.7d, 5.0d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 8.4d, 12.7d, 5.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.0d, 41.0d, 4.2d, 4.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 58.3d, 41.0d, 4.2d, 4.6d)
            ]),
            [EmulationControllerVisualIds.SonyDualShock4] = new("playstation-4.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 24.0d, 36.0d, 17.5d, 25.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 58.5d, 36.0d, 17.5d, 25.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 33.8d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.0d, 22.6d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 70.0d, 22.6d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 11.5d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.TouchPad, ControllerVisualZoneShape.RoundedRectangle, 34.0d, 8.0d, 32.0d, 23.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 28.3d, 14.5d, 3.0d, 9.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 68.7d, 14.5d, 3.0d, 9.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.0d, 43.0d, 6.0d, 9.5d)
            ]),
            [EmulationControllerVisualIds.SonyDualSense] = new("playstation-5.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 27.0d, 43.0d, 15.0d, 22.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 58.0d, 43.0d, 15.0d, 22.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 36.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.2d, 26.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.8d, 26.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 16.2d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.TouchPad, ControllerVisualZoneShape.RoundedRectangle, 33.0d, 7.2d, 34.0d, 29.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 26.4d, 14.0d, 3.0d, 10.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 71.0d, 14.0d, 3.0d, 10.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.0d, 49.0d, 6.0d, 9.5d)
            ]),
            [EmulationControllerVisualIds.NintendoNesPad] = new("nintendo-entertainment-system.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 16.5d, 37.0d, 14.6d, 45.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 16.5d, 37.0d, 14.6d, 45.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 16.5d, 37.0d, 14.6d, 45.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 16.5d, 37.0d, 14.6d, 45.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 63.1d, 57.8d, 8.0d, 24.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 73.0d, 57.8d, 8.0d, 24.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.5d, 65.0d, 7.0d, 9.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 47.2d, 65.0d, 7.0d, 9.0d)
            ]),
            [EmulationControllerVisualIds.NintendoFamicomPad1] = new("famicom-controller-i.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.1d, 37.0d, 18.0d, 40.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.1d, 37.0d, 18.0d, 40.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.1d, 37.0d, 18.0d, 40.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.1d, 37.0d, 18.0d, 40.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 63.7d, 57.6d, 9.0d, 20.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 57.6d, 9.0d, 20.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 36.3d, 60.8d, 9.0d, 10.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 47.4d, 60.8d, 9.0d, 10.0d)
            ]),
            [EmulationControllerVisualIds.NintendoSuperNesPad] = new("super-nes-controller.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.0d, 39.0d, 19.5d, 27.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.0d, 39.0d, 19.5d, 27.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.0d, 39.0d, 19.5d, 27.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.0d, 39.0d, 19.5d, 27.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.0d, 58.5d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.2d, 46.7d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.2d, 47.8d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 74.3d, 37.0d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.4d, 53.5d, 9.5d, 8.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 49.4d, 53.5d, 9.5d, 8.5d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 11.4d, 18.5d, 19.2d, 8.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 69.8d, 18.5d, 19.2d, 8.0d)
            ]),
            [EmulationControllerVisualIds.NintendoSuperFamicomPad] = new("super-nintendo.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.7d, 38.5d, 18.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.7d, 38.5d, 18.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.7d, 38.5d, 18.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.7d, 38.5d, 18.0d, 28.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.7d, 58.8d, 8.2d, 12.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 84.1d, 46.5d, 8.2d, 12.5d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 64.9d, 46.8d, 8.2d, 12.5d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.1d, 36.5d, 8.2d, 12.5d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 38.0d, 52.6d, 8.0d, 10.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 48.6d, 52.6d, 8.0d, 10.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 12.2d, 18.8d, 18.7d, 8.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 69.0d, 18.8d, 18.7d, 8.0d)
            ]),
            [EmulationControllerVisualIds.QuickShot] = new("quickshot.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 43.8d, 4.3d, 13.4d, 28.0d)
            ]),
            [EmulationControllerVisualIds.QuickShotDeluxe] = new("quickshot-deluxe.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 37.4d, 6.7d, 24.4d, 14.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 15.6d, 7.5d, 14.4d, 11.7d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 69.3d, 7.6d, 14.6d, 11.5d)
            ]),
            [EmulationControllerVisualIds.QuickShotIiTurbo] = new("quickshot-ii-turbo.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 15.6d, 20.0d, 41.4d)
            ]),
            [EmulationControllerVisualIds.CompetitionPro5000] = new("competition-pro-5000.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 5.8d, 3.3d, 30.9d, 23.1d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63.9d, 3.3d, 31.0d, 23.3d)
            ]),
            [EmulationControllerVisualIds.ZipstikSuperPro] = new("zipstik-super-pro.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 7.4d, 5.1d, 21.9d, 16.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 71.0d, 5.1d, 21.7d, 16.8d)
            ]),
            [EmulationControllerVisualIds.KonixSpeedkingLeftHand] = new("konix-speedking-left-hand.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 81.0d, 43.0d, 13.0d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 77.0d, 56.0d, 13.0d, 10.0d)
            ]),
            [EmulationControllerVisualIds.KonixSpeedkingRightHand] = new("konix-speedking-right-hand.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 27.1d, 12.0d, 44.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 27.1d, 12.0d, 44.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 27.1d, 12.0d, 44.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 27.1d, 12.0d, 44.0d, 23.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 6.0d, 43.0d, 13.0d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 10.0d, 56.0d, 13.0d, 10.0d)
            ]),
            [EmulationControllerVisualIds.KonixSpeedkingAnalog] = new("konix-speedking-analog.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 12.9d, 74.0d, 14.3d, 15.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 73.4d, 74.0d, 14.0d, 15.0d)
            ]),
            [EmulationControllerVisualIds.SuncomTac2] = new("suncom-tac-2.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 13.6d, 67.4d, 16.4d, 18.3d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.4d, 67.4d, 16.8d, 18.3d)
            ]),
            [EmulationControllerVisualIds.PowerplayCruiser] = new("powerplay-cruiser.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 14.8d, 64.9d, 19.2d, 17.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 67.8d, 65.0d, 18.9d, 17.8d)
            ]),
            [EmulationControllerVisualIds.SuzoTheArcadeTurbo] = new("suzo-the-arcade-turbo.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 43.1d, 30.0d, 13.2d, 13.4d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 81.2d, 20.0d, 9.3d)
            ]),
            [EmulationControllerVisualIds.CommodoreCd32] = new("commodore-cd32.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 5.9d, 24.0d, 13.7d, 34.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 5.9d, 24.0d, 13.7d, 34.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 5.9d, 24.0d, 13.7d, 34.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 5.9d, 24.0d, 13.7d, 34.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 80.2d, 47.8d, 6.6d, 16.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 88.8d, 44.6d, 6.7d, 16.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 78.7d, 26.5d, 6.5d, 16.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 87.2d, 23.4d, 6.6d, 16.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 10.9d, 0.0d, 13.0d, 2.7d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 76.1d, 0.0d, 11.0d, 2.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 59.0d, 67.5d, 9.8d, 7.1d)
            ]),
            [EmulationControllerVisualIds.CompetitionProCd32] = new("competition-pro-cd32.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.2d, 63.4d, 7.7d, 15.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 84.5d, 53.7d, 7.7d, 14.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 69.5d, 43.0d, 7.4d, 15.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 80.0d, 33.6d, 7.7d, 14.5d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 6.5d, 4.5d, 19.0d, 22.5d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 74.5d, 4.5d, 19.0d, 22.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 39.2d, 63.1d, 6.1d, 8.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 48.5d, 63.1d, 6.1d, 8.1d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 55.2d, 23.0d, 6.6d, 5.3d)
            ]),
            [EmulationControllerVisualIds.AtariCx40] = new("atari-cx40.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 15.8d, 14.4d, 14.5d, 14.4d)
            ]),
            [EmulationControllerVisualIds.Atari5200Controller] = new("atari-5200-controller.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 23.9d, 17.1d, 51.9d, 24.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 23.9d, 17.1d, 51.9d, 24.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 23.9d, 17.1d, 51.9d, 24.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 23.9d, 17.1d, 51.9d, 24.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 13.4d, 10.6d, 3.6d, 7.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 82.3d, 10.6d, 3.3d, 7.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 13.4d, 19.2d, 3.6d, 7.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 82.3d, 19.2d, 3.3d, 7.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 25.1d, 8.4d, 13.5d, 4.6d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 43.5d, 8.4d, 13.6d, 4.6d),
                new(EmulationControllerVisualControl.Reset, ControllerVisualZoneShape.RoundedRectangle, 61.5d, 8.4d, 13.6d, 4.6d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 27.9d, 57.3d, 13.0d, 5.4d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 43.0d, 57.3d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 60.6d, 57.3d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 27.9d, 63.3d, 13.0d, 5.4d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 43.0d, 63.3d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 60.6d, 63.3d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 27.9d, 70.8d, 13.0d, 5.4d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 43.0d, 70.8d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 60.6d, 70.8d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 27.9d, 78.3d, 13.0d, 5.4d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 43.0d, 78.3d, 13.2d, 5.4d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 60.6d, 78.3d, 13.2d, 5.4d)
            ]),
            [EmulationControllerVisualIds.Atari7800ProLineCx24] = new("atari-7800-pro-line-cx24.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 26.2d, 11.9d, 6.8d, 16.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 66.2d, 11.7d, 6.5d, 17.0d)
            ]),
            [EmulationControllerVisualIds.Atari7800ControlPadEurope] = new("atari-7800-control-pad-europe.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 46.4d, 51.4d, 10.3d, 16.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 64.8d, 51.4d, 10.3d, 16.0d)
            ]),
            [EmulationControllerVisualIds.AtariJaguarController] = new("atari-jaguar-controller.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 14.6d, 10.7d, 10.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 65.6d, 23.5d, 10.5d, 10.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 59.2d, 32.7d, 11.1d, 10.4d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 42.4d, 32.6d, 5.1d, 7.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 49.0d, 32.6d, 4.8d, 7.4d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 55.1d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 55.1d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 55.1d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 63.6d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 63.6d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 63.6d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 72.3d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 72.3d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 72.3d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 80.7d, 7.8d, 4.1d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 80.7d, 7.6d, 4.1d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 80.7d, 7.7d, 4.1d)
            ]),
            [EmulationControllerVisualIds.AtariJaguarProController] = new("atari-jaguar-pro-controller.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 22.0d, 25.4d, 14.8d, 17.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 22.0d, 25.4d, 14.8d, 17.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 22.0d, 25.4d, 14.8d, 17.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 22.0d, 25.4d, 14.8d, 17.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 73.2d, 26.0d, 6.7d, 7.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 66.7d, 31.7d, 6.4d, 7.1d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 61.3d, 38.2d, 6.2d, 7.1d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 42.7d, 36.2d, 4.3d, 4.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 48.2d, 36.2d, 4.5d, 4.9d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 36.6d, 53.7d, 5.8d, 2.5d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 46.5d, 53.7d, 5.6d, 2.5d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 56.1d, 53.7d, 5.8d, 2.5d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 36.6d, 60.5d, 5.8d, 2.5d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 46.5d, 60.5d, 5.6d, 2.5d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 56.1d, 60.5d, 5.8d, 2.5d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 36.6d, 67.4d, 5.8d, 2.6d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 46.5d, 67.4d, 5.6d, 2.6d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 56.1d, 67.4d, 5.8d, 2.6d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 36.6d, 74.4d, 5.8d, 2.5d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 46.5d, 74.4d, 5.6d, 2.5d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 56.1d, 74.4d, 5.8d, 2.5d)
            ]),
            [EmulationControllerVisualIds.MasterSystem] = new("master-system.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 13d, 39d, 23d, 35d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 13d, 39d, 23d, 35d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 13d, 39d, 23d, 35d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 13d, 39d, 23d, 35d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 61d, 55d, 12d, 19d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 78d, 55d, 12d, 19d)
            ]),
            [EmulationControllerVisualIds.MegaDrive3] = new("mega-drive-3.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 62d, 52d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 71d, 45d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 80d, 39d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 46d, 47d, 9d, 7d)
            ]),
            [EmulationControllerVisualIds.MegaDrive6] = new("mega-drive-6.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 64d, 54d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 75d, 52d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 84d, 49d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 64d, 41d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 71d, 37d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 79d, 36d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 46d, 45d, 9d, 7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 46d, 57d, 9d, 7d)
            ]),
            [EmulationControllerVisualIds.Saturn] = new("saturn.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 62d, 58d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 71d, 51d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 80d, 46d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 59d, 43d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 67d, 36d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 76d, 33d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 45d, 56d, 10d, 8d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 15d, 12d, 16d, 12d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 68d, 12d, 17d, 12d)
            ]),
            [EmulationControllerVisualIds.Dreamcast] = new("dreamcast.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 73d, 39d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 80d, 32d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 67d, 32d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 73d, 25d, 8d, 9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 47d, 61d, 6d, 7d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 18d, 8d, 11d, 10d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72d, 8d, 13d, 10d)
            ]),
            [EmulationControllerVisualIds.ArcadeStick] = new("arcade-stick.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 8d, 26d, 35d, 53d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 8d, 26d, 35d, 53d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 8d, 26d, 35d, 53d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 8d, 26d, 35d, 53d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 48d, 25d, 16d, 22d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65d, 25d, 16d, 22d)
            ]),
            [EmulationControllerVisualIds.FlightStick] = new("flight-stick.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 55d, 16d, 30d, 43d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 55d, 16d, 30d, 43d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 55d, 16d, 30d, 43d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 55d, 16d, 30d, 43d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69d, 14d, 12d, 13d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 79d, 15d, 10d, 12d)
            ]),
            [EmulationControllerVisualIds.RacingWheel] = new("racing-wheel.png",
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 27d, 25d, 18d, 16d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 27d, 25d, 18d, 16d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 27d, 25d, 18d, 16d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 27d, 25d, 18d, 16d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68d, 29d, 10d, 10d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 62d, 37d, 10d, 10d)
            ])
        };

    private static readonly Dictionary<ControllerVisualModel, ImageSource> ModelCache = [];
    private static readonly Dictionary<string, ControllerArtworkProfile> ProfileCache =
        new(StringComparer.Ordinal);

    internal static bool TryGet(ControllerVisualModel model, out ImageSource artwork)
    {
        if (ModelCache.TryGetValue(model, out artwork!)) return true;
        if (!ModelResources.TryGetValue(model, out var fileName))
        {
            artwork = null!;
            return false;
        }

        artwork = Load(fileName);
        ModelCache[model] = artwork;
        return true;
    }

    internal static bool TryGetProfile(string visualId, out ControllerArtworkProfile profile)
    {
        if (ProfileCache.TryGetValue(visualId, out profile!)) return true;
        if (!ProfileDefinitions.TryGetValue(visualId, out var definition))
        {
            profile = null!;
            return false;
        }

        profile = new ControllerArtworkProfile(visualId, Load(definition.FileName), definition.Zones);
        ProfileCache[visualId] = profile;
        return true;
    }

    internal static IReadOnlyList<ControllerArtworkProfile> AvailableProfiles(
        IReadOnlyList<string>? compatibleVisualIds)
    {
        if (compatibleVisualIds is null || compatibleVisualIds.Count == 0) return [];

        var profiles = new List<ControllerArtworkProfile>(compatibleVisualIds.Count);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var visualId in compatibleVisualIds)
            if (visited.Add(visualId) && TryGetProfile(visualId, out var profile))
                profiles.Add(profile);
        return profiles;
    }

    private static ImageSource Load(string fileName)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(
            $"pack://application:,,,/gwgui.app;component/Assets/Controllers/{fileName}",
            UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private sealed record ProfileDefinition(
        string FileName,
        IReadOnlyList<ControllerVisualZone> Zones);
}
