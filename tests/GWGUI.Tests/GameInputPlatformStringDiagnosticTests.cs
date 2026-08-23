using GWGUI.App.Services.Input.GameInput;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputPlatformStringDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void ResolvesEveryKnownXboxPlatformString()
    {
        _ = GameInputControllerReader.GetConnectedDevices();
        var field = typeof(GameInputControllerReader).GetField("_gameInput", BindingFlags.NonPublic | BindingFlags.Static);
        var gameInput = field?.GetValue(null) as IGameInput ?? throw new InvalidOperationException("GameInput non initialisé.");
        var values = new[]
        {
            @"\\?\HID#VID_045E&PID_0B12&IG_00#b&df77f30&0&0000#{ec87f1e3-c13b-4100-b5f7-8b84d54260cb}",
            @"HID\VID_045E&PID_0B12&IG_00\B&DF77F30&0&0000",
            @"USB\VID_045E&PID_0B12&IG_00\00&00&000043BCAD8DED7E",
            @"USB\VID_045E&PID_02E6\000000000",
            "Xbox Wireless Adapter for Windows #2"
        };
        foreach (var value in values)
        {
            IGameInputDevice? device = null;
            var result = gameInput.FindDeviceFromPlatformString(value, out device);
            output.WriteLine($"value={value}");
            output.WriteLine($"hresult=0x{result:X8}");
            output.WriteLine($"deviceReturned={device is not null}");
            if (device is not null && device.GetDeviceInfo(out var pointer) >= 0 && pointer != IntPtr.Zero)
            {
                var info = Marshal.PtrToStructure<GameInputDeviceInfo>(pointer);
                output.WriteLine($"displayName={Marshal.PtrToStringUTF8(info.DisplayName)}");
                output.WriteLine($"vidPid={info.VendorId:X4}:{info.ProductId:X4}");
                output.WriteLine($"deviceId={info.DeviceId.ToHex()}");
                output.WriteLine($"deviceRootId={info.DeviceRootId.ToHex()}");
                output.WriteLine($"containerId={info.ContainerId}");
                output.WriteLine($"pnpPath={Marshal.PtrToStringUTF8(info.PnpPath)}");
                output.WriteLine($"status={device.GetDeviceStatus()}");
            }
            output.WriteLine("---");
        }
    }
}
