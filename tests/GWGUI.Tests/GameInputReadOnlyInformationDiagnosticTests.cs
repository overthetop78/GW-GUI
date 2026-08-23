using GWGUI.App.Services.Input.GameInput;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputReadOnlyInformationDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void DumpsEveryReadOnlyInformationFunctionForControllers()
    {
        _ = GameInputControllerReader.GetConnectedDevices();
        var devicesField = typeof(GameInputControllerReader).GetField("Devices", BindingFlags.NonPublic | BindingFlags.Static)!;
        foreach (var pair in (IEnumerable)devicesField.GetValue(null)!)
        {
            var entry = pair.GetType().GetProperty("Value")!.GetValue(pair)!;
            if (!(bool)entry.GetType().GetProperty("IsController")!.GetValue(entry)!) continue;
            var id = (string)entry.GetType().GetProperty("Id")!.GetValue(entry)!;
            var name = (string)entry.GetType().GetProperty("Name")!.GetValue(entry)!;
            var device = (IGameInputDevice)entry.GetType().GetProperty("Device")!.GetValue(entry)!;
            output.WriteLine($"===== {id} | {name} =====");
            Dump(device);
        }
    }

    private void Dump(IGameInputDevice device)
    {
        var infoHr = device.GetDeviceInfo(out var infoPointer);
        output.WriteLine($"GetDeviceInfo.hresult=0x{infoHr:X8}");
        if (infoHr < 0 || infoPointer == IntPtr.Zero) return;
        var info = Marshal.PtrToStructure<GameInputDeviceInfo>(infoPointer);
        output.WriteLine($"GetDeviceStatus={device.GetDeviceStatus()}");
        output.WriteLine($"displayName={Marshal.PtrToStringUTF8(info.DisplayName)}");
        output.WriteLine($"pnpPath={Marshal.PtrToStringUTF8(info.PnpPath)}");
        output.WriteLine($"vidPid={info.VendorId:X4}:{info.ProductId:X4}");
        output.WriteLine($"deviceId={info.DeviceId.ToHex()}");
        output.WriteLine($"deviceRootId={info.DeviceRootId.ToHex()}");
        output.WriteLine($"containerId={info.ContainerId}");
        output.WriteLine($"supportedInput=0x{(uint)info.SupportedInput:X8}");
        output.WriteLine($"supportedRumbleMotors=0x{(uint)info.SupportedRumbleMotors:X8}");
        output.WriteLine($"supportedSystemButtons=0x{(uint)info.SupportedSystemButtons:X8}");
        output.WriteLine($"forceFeedbackMotorCount={info.ForceFeedbackMotorCount}");
        output.WriteLine($"inputReportCount={info.InputReportCount}");
        output.WriteLine($"outputReportCount={info.OutputReportCount}");

        var hapticHr = device.GetHapticInfo(out var haptic);
        output.WriteLine($"GetHapticInfo.hresult=0x{hapticHr:X8}");
        if (hapticHr >= 0)
        {
            var hapticPointer = Marshal.AllocHGlobal(Marshal.SizeOf<GameInputHapticInfo>());
            try
            {
                Marshal.StructureToPtr(haptic, hapticPointer, false);
                output.WriteLine($"haptic.audioEndpointId={Marshal.PtrToStringUni(hapticPointer) ?? string.Empty}");
            }
            finally { Marshal.FreeHGlobal(hapticPointer); }
            output.WriteLine($"haptic.locationCount={haptic.LocationCount}");
        }

        for (uint motor = 0; motor < info.ForceFeedbackMotorCount; motor++)
            output.WriteLine($"IsForceFeedbackMotorPoweredOn[{motor}]={device.IsForceFeedbackMotorPoweredOn(motor)}");

        foreach (var kind in new[] { GameInputKind.ArcadeStick, GameInputKind.FlightStick, GameInputKind.Gamepad, GameInputKind.RacingWheel })
        {
            var axisHr = device.GetExtraAxisCount(kind, out var axisCount);
            var buttonHr = device.GetExtraButtonCount(kind, out var buttonCount);
            output.WriteLine($"GetExtraAxisCount({kind})=0x{axisHr:X8}, count={axisCount}");
            output.WriteLine($"GetExtraButtonCount({kind})=0x{buttonHr:X8}, count={buttonCount}");
            if (axisHr >= 0 && axisCount > 0)
            {
                var indexes = new byte[axisCount];
                var handle = GCHandle.Alloc(indexes, GCHandleType.Pinned);
                try { output.WriteLine($"GetExtraAxisIndexes({kind})=0x{device.GetExtraAxisIndexes(kind, axisCount, handle.AddrOfPinnedObject()):X8}, indexes=[{string.Join(",", indexes)}]"); }
                finally { handle.Free(); }
            }
            if (buttonHr >= 0 && buttonCount > 0)
            {
                var indexes = new byte[buttonCount];
                var handle = GCHandle.Alloc(indexes, GCHandleType.Pinned);
                try { output.WriteLine($"GetExtraButtonIndexes({kind})=0x{device.GetExtraButtonIndexes(kind, buttonCount, handle.AddrOfPinnedObject()):X8}, indexes=[{string.Join(",", indexes)}]"); }
                finally { handle.Free(); }
            }
        }

        DumpReports(device, info.InputReportInfo, info.InputReportCount, "input");
        DumpReports(device, info.OutputReportInfo, info.OutputReportCount, "output");
    }

    private void DumpReports(IGameInputDevice device, IntPtr pointer, uint count, string source)
    {
        var size = Marshal.SizeOf<GameInputRawDeviceReportInfo>();
        for (var index = 0; index < count; index++)
        {
            var info = Marshal.PtrToStructure<GameInputRawDeviceReportInfo>(IntPtr.Add(pointer, index * size));
            output.WriteLine($"{source}Report[{index}].kind={info.Kind}, id={info.Id}, size={info.Size}");
            var hr = device.CreateRawDeviceReport(info.Id, info.Kind, out var report);
            output.WriteLine($"CreateRawDeviceReport({source},{info.Id}).hresult=0x{hr:X8}, returned={report is not null}");
            if (hr >= 0 && report is not null)
            {
                report.GetReportInfo(out var returnedInfo);
                output.WriteLine($"rawReport.info=kind:{returnedInfo.Kind},id:{returnedInfo.Id},size:{returnedInfo.Size}");
                output.WriteLine($"rawReport.dataSize={report.GetRawDataSize()}");
            }
        }
    }
}
