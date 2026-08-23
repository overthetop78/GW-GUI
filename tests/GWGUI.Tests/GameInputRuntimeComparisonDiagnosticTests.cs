using GWGUI.App.Services.Input.GameInput;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputRuntimeComparisonDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void EnumeratesEveryInstalledGameInputRuntimeSeparately()
    {
        foreach (var path in RuntimePaths()) Enumerate(path);
    }

    [Fact]
    public void EnumeratesOnlySystem32RedistRuntime() =>
        Enumerate(Path.Combine(Environment.SystemDirectory, "GameInputRedist.dll"));

    [Fact]
    public void EnumeratesOnlyRegisteredRedistRuntime() =>
        Enumerate(RegisteredRuntimePath());

    [Fact]
    public void EnumeratesSystemRuntimeByEveryControllerKind()
    {
        var path = Path.Combine(Environment.SystemDirectory, "GameInputRedist.dll");
        foreach (var kind in new[]
        {
            GameInputKind.Gamepad,
            GameInputKind.Controller,
            GameInputKind.RawDeviceReport,
            GameInputKind.Gamepad | GameInputKind.Controller
        }) Enumerate(path, kind);
    }

    [Fact]
    public void EnumeratesRegisteredRuntimeByEveryControllerKind()
    {
        var path = RegisteredRuntimePath();
        foreach (var kind in new[]
        {
            GameInputKind.Gamepad,
            GameInputKind.Controller,
            GameInputKind.ControllerAxis,
            GameInputKind.ControllerButton,
            GameInputKind.RawDeviceReport,
            GameInputKind.Gamepad | GameInputKind.Controller
        }) Enumerate(path, kind);
    }

    [Fact]
    public void RuntimeComparisonDoesNotDetachTheSharedControllerReader()
    {
        var before = GameInputControllerReader.GetConnectedControllerDetails()
            .Select(device => device.Id).Order().ToArray();

        Enumerate(RegisteredRuntimePath(), GameInputKind.Gamepad | GameInputKind.Controller);
        GameInputControllerReader.RefreshConnectedDevices();

        var after = GameInputControllerReader.GetConnectedControllerDetails()
            .Select(device => device.Id).Order().ToArray();
        Assert.Equal(before, after);
    }

    private void Enumerate(string path, GameInputKind? requestedKinds = null)
    {
        var version = FileVersionInfo.GetVersionInfo(path).FileVersion ?? "unknown";
        var module = LoadLibrary(path);
        Assert.NotEqual(IntPtr.Zero, module);
        var address = GetProcAddress(module, "GameInputInitialize");
        output.WriteLine($"RUNTIME | {path} | version={version} | initialize=0x{address.ToInt64():X}");
        if (address == IntPtr.Zero) return;

        var initialize = Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(address);
        var interfaceId = GameInputNative.InterfaceId;
        var result = initialize(in interfaceId, out var pointer);
        output.WriteLine($"INITIALIZE | hresult=0x{result:X8} | pointer=0x{pointer.ToInt64():X}");
        if (result < 0 || pointer == IntPtr.Zero) return;

        IGameInput? gameInput = null;
        ulong token = 0;
        var devices = new List<string>();
        GameInputDeviceCallback callback = (_, _, device, _, current, previous) =>
        {
            if (device.GetDeviceInfo(out var infoPointer) < 0 || infoPointer == IntPtr.Zero) return;
            var info = Marshal.PtrToStructure<GameInputDeviceInfo>(infoPointer);
            var name = Marshal.PtrToStringUTF8(info.DisplayName) ?? string.Empty;
            var pnp = Marshal.PtrToStringUTF8(info.PnpPath) ?? string.Empty;
            lock (devices) devices.Add($"{info.VendorId:X4}:{info.ProductId:X4} | {name} | status={current}/{previous} | kinds=0x{(uint)info.SupportedInput:X8} | {pnp}");
        };
        try
        {
            gameInput = (IGameInput)Marshal.GetObjectForIUnknown(pointer);
            var allKinds = requestedKinds ?? (GameInputKind.RawDeviceReport | GameInputKind.Controller |
                GameInputKind.Keyboard | GameInputKind.Mouse |
                GameInputKind.ArcadeStick | GameInputKind.FlightStick |
                GameInputKind.Gamepad | GameInputKind.RacingWheel);
            output.WriteLine($"FILTER | {allKinds} | 0x{(uint)allKinds:X8}");
            result = gameInput.RegisterDeviceCallback(null, allKinds, GameInputDeviceStatus.Any,
                GameInputEnumerationKind.Blocking, IntPtr.Zero,
                Marshal.GetFunctionPointerForDelegate(callback), out token);
            output.WriteLine($"REGISTER | hresult=0x{result:X8} | token=0x{token:X16}");
            Thread.Sleep(3000);
            lock (devices)
            {
                output.WriteLine($"DEVICES={devices.Count}");
                foreach (var device in devices) output.WriteLine(device);
            }
        }
        finally
        {
            if (gameInput is not null && token != 0)
                output.WriteLine($"UNREGISTER={gameInput.UnregisterCallback(token)}");
            if (gameInput is not null && Marshal.IsComObject(gameInput))
                Marshal.ReleaseComObject(gameInput);
            Marshal.Release(pointer);
            GC.KeepAlive(callback);
        }
    }

    private static string RegisteredRuntimePath()
    {
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Microsoft\GameInput");
        return Path.Combine(Assert.IsType<string>(key?.GetValue("RedistDir")), "GameInputRedist.dll");
    }

    private static IReadOnlyList<string> RuntimePaths()
    {
        var paths = new List<string>
        {
            Path.Combine(Environment.SystemDirectory, "GameInput.dll"),
            Path.Combine(Environment.SystemDirectory, "GameInputRedist.dll")
        };
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Microsoft\GameInput");
        if (key?.GetValue("RedistDir") is string directory)
            paths.Add(Path.Combine(directory, "GameInputRedist.dll"));
        return paths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int InitializeDelegate(in Guid interfaceId, out IntPtr gameInput);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string fileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr module, string name);
}
