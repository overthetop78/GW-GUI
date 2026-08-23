using Windows.Gaming.Input;
using Xunit.Abstractions;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class WindowsGamingInputDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void DumpsEveryWindowsGamingInputControllerCollection()
    {
        WpfTestHost.Run(() =>
        {
            output.WriteLine($"RawGameControllers={RawGameController.RawGameControllers.Count}");
            foreach (var controller in RawGameController.RawGameControllers)
            {
                output.WriteLine($"RAW | {controller.DisplayName} | {controller.HardwareVendorId:X4}:{controller.HardwareProductId:X4} | id={controller.NonRoamableId} | axes={controller.AxisCount} | buttons={controller.ButtonCount} | switches={controller.SwitchCount}");
                output.WriteLine($"RAW.Gamepad={Gamepad.FromGameController(controller) is not null} | Arcade={ArcadeStick.FromGameController(controller) is not null} | Flight={FlightStick.FromGameController(controller) is not null} | Wheel={RacingWheel.FromGameController(controller) is not null}");
            }

            output.WriteLine($"Gamepads={Gamepad.Gamepads.Count}");
            foreach (var gamepad in Gamepad.Gamepads) DumpController("GAMEPAD", gamepad);
            output.WriteLine($"ArcadeSticks={ArcadeStick.ArcadeSticks.Count}");
            foreach (var stick in ArcadeStick.ArcadeSticks) DumpController("ARCADE", stick);
            output.WriteLine($"FlightSticks={FlightStick.FlightSticks.Count}");
            foreach (var stick in FlightStick.FlightSticks) DumpController("FLIGHT", stick);
            output.WriteLine($"RacingWheels={RacingWheel.RacingWheels.Count}");
            foreach (var wheel in RacingWheel.RacingWheels) DumpController("WHEEL", wheel);
        });
    }

    private void DumpController(string kind, IGameController controller)
    {
        var raw = RawGameController.FromGameController(controller);
        output.WriteLine($"{kind} | {raw?.DisplayName} | {raw?.HardwareVendorId:X4}:{raw?.HardwareProductId:X4} | id={raw?.NonRoamableId}");
    }
}
