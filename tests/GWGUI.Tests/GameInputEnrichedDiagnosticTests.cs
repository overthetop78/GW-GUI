using GWGUI.App.Services.Input.GameInput;
using Xunit.Abstractions;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputEnrichedDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void DumpsEnrichedConnectedControllerDescriptorsAndStates()
    {
        var legacyStates = GameInputControllerReader.ReadAll();
        output.WriteLine($"LegacyStateCount={legacyStates.Count}");
        foreach (var legacy in legacyStates) output.WriteLine($"LegacyState={legacy.DeviceId}|buttons=0x{legacy.Buttons:X8}|axes={legacy.LeftX},{legacy.LeftY},{legacy.RightX},{legacy.RightY},{legacy.LeftTrigger},{legacy.RightTrigger}|raw={string.Join(",", legacy.Controls.Select(pair => $"{pair.Key}:{pair.Value:0.000}"))}");
        output.WriteLine($"LegacyDiagnostic={GameInputControllerReader.LastReadDiagnostic}");
        foreach (var device in GameInputControllerReader.GetConnectedControllerDetails())
        {
            output.WriteLine($"===== {device.Id} =====");
            output.WriteLine($"ProductName={device.ProductName}");
            output.WriteLine($"GameInputDisplayName={device.GameInputDisplayName}");
            output.WriteLine($"VidPid={device.VidPid}");
            output.WriteLine($"Family={device.Family}");
            output.WriteLine($"Usage={device.Usage.Page:X4}:{device.Usage.Id:X4}");
            output.WriteLine($"SupportedInput={device.SupportedInput}");
            output.WriteLine($"RumbleMotors={device.RumbleMotors}");
            output.WriteLine($"SystemButtons={device.SystemButtons}");
            output.WriteLine($"SuggestedVisualModel={device.SuggestedVisualModel}");
            output.WriteLine($"IsExactVisualModelMatch={device.IsExactVisualModelMatch}");
            output.WriteLine($"Controls={string.Join(" | ", device.Controls.Select(control =>
                $"{control.Type}[{control.Index}]={control.Label}"))}");
            output.WriteLine($"ForceFeedbackMotors={device.ForceFeedbackMotors.Count}");
            output.WriteLine($"Reports=input:{device.InputReports.Count},output:{device.OutputReports.Count}");
            output.WriteLine($"Haptics={device.HasHaptics}");
            output.WriteLine($"PnP={device.PnpPath}");
            output.WriteLine($"WindowsIdentityChain={string.Join(" || ", device.WindowsIdentityChain)}");

            var state = GameInputControllerReader.ReadDetailedState(device.Id);
            output.WriteLine($"State.Diagnostic={GameInputControllerReader.LastDetailedReadDiagnostic}");
            output.WriteLine($"State.Timestamp={state.Timestamp}");
            output.WriteLine($"State.InputKind={state.InputKind}");
            output.WriteLine($"State.RawReport={Convert.ToHexString(state.RawReport.ToArray())}");
            output.WriteLine($"State.Controls={string.Join(" | ", state.Controls.Select(control =>
                $"{control.Type}[{control.Index}]={control.Value:0.000}/{control.SwitchPosition}"))}");
            output.WriteLine($"State.Gamepad={state.Gamepad}");
            output.WriteLine($"State.RacingWheel={state.RacingWheel}");
            output.WriteLine($"State.FlightStick={state.FlightStick}");
            output.WriteLine($"State.ArcadeStick={state.ArcadeStick}");
        }
    }
}
