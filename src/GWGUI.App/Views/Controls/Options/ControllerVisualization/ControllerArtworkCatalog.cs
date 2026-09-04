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

    private static readonly IReadOnlyDictionary<string, ProfileDefinition> ProfileDefinitions =
        new Dictionary<string, ProfileDefinition>(StringComparer.Ordinal)
        {
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
