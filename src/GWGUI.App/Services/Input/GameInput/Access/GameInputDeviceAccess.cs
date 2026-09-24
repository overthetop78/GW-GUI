using GWGUI.App.Contracts.Services.Input;
using GWGUI.App.Constants.Input.GameInput;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Input.Controllers;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

internal static partial class GameInputDeviceMonitor
{
    internal static void StartMonitoring() => Worker.Post(EnsureInitialized);

    internal static void StopMonitoring()
    {
        RawGameControllerFallback.StopMonitoring();
        if (!Worker.Stop(() =>
        {
            Shutdown();
            lock (Sync)
            {
                _initialized = false;
                InitializationFailed = false;
            }
        }, ShutdownTimeout))
            throw new TimeoutException(
                LocExtension.Get(ErrorDescriptionResourceKeys.GameInputWorkerStopTimeout));
    }

    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetailsCached()
    {
        GameInputDeviceDescriptor[] gameInput;
        lock (Sync)
            gameInput = Devices.Values.Where(entry => entry.IsController)
                .Select(entry => entry.Descriptor).ToArray();
        return InitializationFailed
            ? RawGameControllerFallback.MergeDescriptors(gameInput)
            : gameInput;
    }

    internal static IReadOnlyList<EmulationControllerState> ReadAll() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputDeviceDescriptor[] descriptors;
        EmulationControllerState[] gameInput;
        lock (Sync)
        {
            var entries = Devices.Values.Where(entry => entry.IsController)
                .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
            descriptors = entries.Select(entry => entry.Descriptor).ToArray();
            gameInput = entries.Select(Read).ToArray();
        }
        return gameInput.Concat(InitializationFailed
                ? RawGameControllerFallback.ReadAll(descriptors)
                : [])
            .Select(ControllerAnalogDeadZoneFunctions.ApplyConfigured).ToArray();
    });

    internal static IReadOnlyList<GameControllerDevice> GetConnectedDevices() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        return GetConnectedControllerDetailsCached()
            .Select(device => new GameControllerDevice(device.Id, device.ProductName)).ToArray();
    });

    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetails() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        return GetConnectedControllerDetailsCached();
    });

    internal static GameInputLiveState ReadDetailedState(string deviceId) => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputLiveState state;
        lock (Sync)
            if (Devices.TryGetValue(deviceId, out var entry) && entry.IsController)
                state = ReadDetailed(entry);
            else if (InitializationFailed)
                state = RawGameControllerFallback.TryReadDetailed(deviceId, out var fallback)
                    ? fallback : GameInputLiveState.Empty(deviceId);
            else state = GameInputLiveState.Empty(deviceId);
        return ControllerAnalogDeadZoneFunctions.ApplyConfigured(state);
    });

    internal static IReadOnlyList<GameInputLiveState> ReadAllDetailedStates() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputDeviceDescriptor[] gameInputDescriptors;
        var result = new List<GameInputLiveState>();
        lock (Sync)
        {
            var entries = Devices.Values.Where(entry => entry.IsController)
                .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
            gameInputDescriptors = entries.Select(entry => entry.Descriptor).ToArray();
            result.AddRange(entries.Select(ReadDetailed));
        }
        foreach (var descriptor in InitializationFailed
                     ? RawGameControllerFallback.MergeDescriptors(gameInputDescriptors)
                     : gameInputDescriptors)
        {
            if (result.Any(state => string.Equals(state.DeviceId, descriptor.Id,
                    StringComparison.OrdinalIgnoreCase))) continue;
            if (RawGameControllerFallback.TryReadDetailed(descriptor.Id, out var state))
                result.Add(state);
        }
        return result.Select(ControllerAnalogDeadZoneFunctions.ApplyConfigured).ToArray();
    });

    internal static string? GetControllerName(string deviceId)
    {
        lock (Sync)
            if (Devices.TryGetValue(deviceId, out var entry) && entry.IsController)
                return entry.Name;
        return InitializationFailed ? RawGameControllerFallback.GetName(deviceId) : null;
    }

    internal static void RefreshConnectedDevices() => Worker.Invoke(() =>
    {
        if (InitializationFailed)
        {
            if (!Shutdown()) return;
            lock (Sync)
            {
                _initialized = false;
                InitializationFailed = false;
            }
        }

        EnsureInitialized();
        if (_gameInput is null)
        {
            RawGameControllerFallback.RefreshOnUiThread();
            return;
        }

        var refreshTokens = new List<ulong>(ControllerRefreshFilters.Length);
        foreach (var filter in ControllerRefreshFilters)
        {
            var result = _gameInput.RegisterDeviceCallback(null, filter,
                GameInputDeviceStatus.Any, GameInputEnumerationKind.Blocking,
                new IntPtr(unchecked((long)(uint)filter)),
                Marshal.GetFunctionPointerForDelegate(DeviceCallback), out var refreshToken);
            if (result < 0)
            {
                foreach (var token in refreshTokens) SafeUnregister(token);
                GameInputDiagnostics.RecordRefreshRegistrationFailure(filter, result);
                return;
            }
            refreshTokens.Add(refreshToken);
        }

        DrainDeviceChanges();
        foreach (var token in refreshTokens)
            if (!SafeUnregister(token))
                GameInputDiagnostics.RecordRefreshUnregisterFailure(token);
    });

    internal static void SetFocusPolicyForDiagnostics(GameInputFocusPolicy policy) => Worker.Invoke(() =>
    {
        EnsureInitialized();
        _gameInput?.SetFocusPolicy(policy);
    });

    internal static bool SetRumble(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger) => Worker.Invoke(() => SetRumbleCore(
            deviceId, lowFrequency, highFrequency, leftTrigger, rightTrigger));

    private static unsafe bool SetRumbleCore(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger)
    {
        EnsureInitialized();
        lock (Sync)
        {
            if (!Devices.TryGetValue(deviceId, out var entry) || !entry.IsController) return false;
            var rumble = new GameInputRumbleParams
            {
                LowFrequency = Math.Clamp(lowFrequency, 0f, 1f),
                HighFrequency = Math.Clamp(highFrequency, 0f, 1f),
                LeftTrigger = Math.Clamp(leftTrigger, 0f, 1f),
                RightTrigger = Math.Clamp(rightTrigger, 0f, 1f)
            };
            try
            {
                entry.Device.SetRumbleState((IntPtr)(&rumble));
                return true;
            }
            catch (Exception exception) when (IsInteropFailure(exception))
            {
                return false;
            }
        }
    }

    internal static GameInputPhysicalState ReadPhysicalInput() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        lock (Sync)
            return GameInputPhysicalInputReader.Read(
                _gameInput, Devices.Values.ToArray(), PreviousMouse, InitializationFailed,
                Read, IsInteropFailure, Release);
    });
}
