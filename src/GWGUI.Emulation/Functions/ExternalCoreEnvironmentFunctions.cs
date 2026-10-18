using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Functions;
internal static class ExternalCoreEnvironmentFunctions
{
    internal static string? CopyDiagnostic(nint textPointer, Action<string> publish, bool allowWhitespace = false)
    {
        var text = textPointer == nint.Zero ? null : Marshal.PtrToStringUTF8(textPointer);
        if (allowWhitespace ? string.IsNullOrEmpty(text) : string.IsNullOrWhiteSpace(text)) return null;
        publish(text);
        return text;
    }

    internal static bool TryHandleCallbackCommand(uint command, nint data,
        IEmulationCoreEnvironmentCommands callbacks, out bool result)
    {
        switch (command)
        {
            case ExternalCoreApiConstants.SetRotation:
                result = callbacks.SetRotation(data);
                break;
            case ExternalCoreApiConstants.SetPerformanceLevel:
                result = callbacks.CapturePerformanceLevel(data);
                break;
            case ExternalCoreApiConstants.SetPixelFormat:
                result = callbacks.SetPixelFormat(data);
                break;
            case ExternalCoreApiConstants.SetSystemAvInfo:
                result = callbacks.SetSystemAvInfo(data);
                break;
            case ExternalCoreApiConstants.SetGeometry:
                result = callbacks.SetGeometry(data);
                break;
            case ExternalCoreApiConstants.SetMessage:
                result = callbacks.ReadMessage(data);
                break;
            case ExternalCoreApiConstants.SetMessageExtended:
                result = callbacks.ReadExtendedMessage(data);
                break;
            case ExternalCoreApiConstants.GetLogInterface:
                result = callbacks.SetLogInterface(data);
                break;
            case ExternalCoreApiConstants.GetLedInterface:
                result = callbacks.SetLedInterface(data);
                break;
            case ExternalCoreApiConstants.GetRumbleInterface:
                result = callbacks.SetRumbleInterface(data);
                break;
            case ExternalCoreApiConstants.GetSensorInterface:
                result = callbacks.SetSensorInterface(data);
                break;
            case ExternalCoreApiConstants.GetSystemDirectory:
                result = callbacks.WriteSystemDirectory(data);
                break;
            case ExternalCoreApiConstants.GetContentDirectory:
                result = callbacks.WriteContentDirectory(data);
                break;
            case ExternalCoreApiConstants.GetSaveDirectory:
                result = callbacks.WriteSaveDirectory(data);
                break;
            case ExternalCoreApiConstants.SetSupportNoGame:
                result = callbacks.SetSupportNoGame(data);
                break;
            case ExternalCoreApiConstants.SetInputDescriptors:
                result = callbacks.CaptureInputDescriptors(data);
                break;
            case ExternalCoreApiConstants.SetControllerInfo:
                result = callbacks.CaptureControllerInfo(data);
                break;
            case ExternalCoreApiConstants.SetMemoryMaps:
                result = callbacks.CaptureMemoryMaps(data);
                break;
            case ExternalCoreApiConstants.SetSupportAchievements:
                result = callbacks.SetSupportAchievements(data);
                break;
            case ExternalCoreApiConstants.GetVfsInterface:
                result = callbacks.ProvideVfs(data);
                break;
            case ExternalCoreApiConstants.SetKeyboardCallback:
                result = callbacks.SetKeyboardCallback(data);
                break;
            case ExternalCoreApiConstants.SetDiskControl:
                result = data != nint.Zero && callbacks.CaptureDiskControl(data);
                break;
            case ExternalCoreApiConstants.SetDiskControlExtended:
                result = data != nint.Zero && callbacks.CaptureExtendedDiskControl(data);
                break;
            case ExternalCoreApiConstants.GetDiskControlVersion:
                result = callbacks.GetDiskControlVersion(data);
                break;
            default:
                result = false;
                return false;
        }
        return true;
    }

    internal static void InstallCallbacks(ExternalCoreExports exports, IEmulationCoreCallbacks callbacks)
    {
        exports.SetEnvironment(callbacks.Environment);
        exports.SetVideo(callbacks.Video);
        exports.SetAudioSample(callbacks.AudioSample);
        exports.SetAudioBatch(callbacks.AudioBatch);
        exports.SetInputPoll(callbacks.InputPoll);
        exports.SetInputState(callbacks.InputState);
    }


    internal static bool TryHandleStatelessCommand(uint command, nint data, uint coreOptionsVersion,
        uint messageInterfaceVersion, ulong inputCapabilities, out bool result)
    {
        switch (command)
        {
            case ExternalCoreApiConstants.GetOverscan:
            case ExternalCoreApiConstants.GetFastForwarding:
                result = WriteBoolean(data, false);
                break;
            case ExternalCoreApiConstants.GetCanDuplicateFrames:
            case ExternalCoreApiConstants.GetInputBitmasks:
                result = WriteBoolean(data, true);
                break;
            case ExternalCoreApiConstants.GetCoreOptionsVersion:
                result = WriteInteger(data, coreOptionsVersion);
                break;
            case ExternalCoreApiConstants.GetMessageInterfaceVersion:
                result = WriteInteger(data, messageInterfaceVersion);
                break;
            case ExternalCoreApiConstants.GetPerformanceInterface:
                result = false;
                break;
            case ExternalCoreApiConstants.GetInputDeviceCapabilities:
                result = WriteUnsignedLong(data, inputCapabilities);
                break;
            case ExternalCoreApiConstants.GetLanguage:
                result = WriteInteger(data, CurrentLanguage());
                break;
            default:
                result = false;
                return false;
        }
        return true;
    }


    internal static bool TryHandleOptionCommand(uint command, nint data,
        IEmulationCoreOptionCommands options, out bool result)
    {
        switch (command)
        {
            case ExternalCoreApiConstants.GetVariable:
                result = options.ReturnValue(data);
                break;
            case ExternalCoreApiConstants.SetVariables:
                options.RegisterLegacyVariables(data);
                result = data != nint.Zero;
                break;
            case ExternalCoreApiConstants.GetVariableUpdate:
                result = options.GetAndClearUpdated(data);
                break;
            case ExternalCoreApiConstants.SetCoreOptions:
                options.RegisterVersionOne(data);
                result = data != nint.Zero;
                break;
            case ExternalCoreApiConstants.SetCoreOptionsInternational:
                options.RegisterVersionOneInternational(data);
                result = data != nint.Zero;
                break;
            case ExternalCoreApiConstants.SetCoreOptionsV2:
                options.RegisterVersionTwo(data);
                result = data != nint.Zero;
                break;
            case ExternalCoreApiConstants.SetCoreOptionsV2International:
                options.RegisterVersionTwoInternational(data);
                result = data != nint.Zero;
                break;
            case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                result = options.ApplyVisibility(data);
                break;
            case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                result = options.CaptureDisplayUpdate(data);
                break;
            case ExternalCoreApiConstants.SetVariable:
                result = options.SetNativeValue(data);
                break;
            default:
                result = false;
                return false;
        }
        return true;
    }

    internal static bool WritePointer(nint destination, nint value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteIntPtr(destination, value);
        return true;
    }

    internal static bool WriteBoolean(nint destination, bool value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteByte(destination, value ? (byte)1 : (byte)0);
        return true;
    }

    internal static bool WriteInteger(nint destination, int value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt32(destination, value);
        return true;
    }

    internal static bool WriteInteger(nint destination, uint value) => WriteInteger(destination, checked((int)value));

    internal static bool WriteUnsignedLong(nint destination, ulong value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt64(destination, unchecked((long)value));
        return true;
    }

    internal static bool SetPixelFormat(nint data, ref EmulationPixelFormat pixelFormat)
    {
        if (data == nint.Zero) return false;
        switch (Marshal.ReadInt32(data))
        {
            case 1:
                pixelFormat = EmulationPixelFormat.Xrgb8888;
                return true;
            case 2:
                pixelFormat = EmulationPixelFormat.Rgb565;
                return true;
            case 0:
                pixelFormat = EmulationPixelFormat.Rgb1555;
                return true;
            default:
                return false;
        }
    }

    internal static T? OptionalDelegate<T>(nint pointer) where T : Delegate
        => pointer == 0 ? null : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static T RequiredDelegate<T>(nint pointer, Func<Exception> createError) where T : Delegate
    {
        if (pointer == 0) throw createError();
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }

    internal static bool SetLogInterface(nint data, ExternalCoreApi.LogCallback log)
        => SetLogInterface<ExternalCoreApi.LogCallback>(data, log);

    internal static bool SetLogInterface<TLog>(nint data, TLog log) where TLog : Delegate
        => InstallInterface(data, () => new ExternalCoreApi.LogInterface
        {
            Log = Marshal.GetFunctionPointerForDelegate(log)
        });

    internal static bool SetLedInterface(nint data, ExternalCoreApi.SetLedState setState)
        => InstallInterface(data, () => new ExternalCoreApi.LedInterface
        {
            SetLedState = Marshal.GetFunctionPointerForDelegate(setState)
        });

    internal static bool SetRumbleInterface(nint data, ExternalCoreApi.SetRumbleState setState)
        => InstallInterface(data, () => new ExternalCoreApi.RumbleInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(setState)
        });

    internal static bool SetSensorInterface(nint data, ExternalCoreApi.SetSensorState setState, ExternalCoreApi.GetSensorInput getInput)
        => InstallInterface(data, () => new ExternalCoreApi.SensorInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(setState),
            GetInput = Marshal.GetFunctionPointerForDelegate(getInput)
        });

    private static bool InstallInterface<T>(nint data, Func<T> create) where T : struct
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(create(), data, false);
        return true;
    }

    internal static IReadOnlyList<T> CopyInputDescriptors<T>(nint data, int maximumCount, Func<uint, uint, uint, uint, string, T> create)
    {
        if (data == nint.Zero)
            return[];
        var result = new List<T>();
        var nativeSize = Marshal.SizeOf<ExternalCoreApi.InputDescriptor>();
        for (var index = 0; index < maximumCount; index++)
        {
            var native = Marshal.PtrToStructure<ExternalCoreApi.InputDescriptor>(data + index * nativeSize);
            if (native.Description == nint.Zero)
                return result;
            result.Add(create(native.Port, native.Device, native.Index, native.Id, Marshal.PtrToStringUTF8(native.Description) ?? string.Empty));
        }

        return result;
    }

    internal static IReadOnlyList<TPort> CopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices, Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort)
    {
        TryCopyControllerPorts(data, maximumPorts, maximumDevices, createDevice, createPort,
            rejectOverflow: false, skipEmptyNames: false, out var ports);
        return ports;
    }

    internal static bool TryCopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices,
        Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort,
        bool rejectOverflow, bool skipEmptyNames, out IReadOnlyList<TPort> result)
    {
        result = [];
        if (data == nint.Zero)
            return true;
        var ports = new List<TPort>();
        result = ports;
        var portSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var deviceSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        for (var portIndex = 0; portIndex < maximumPorts; portIndex++)
        {
            var port = Marshal.PtrToStructure<ExternalCoreApi.ControllerInfo>(data + portIndex * portSize);
            if (port.Types == nint.Zero || port.Count == 0)
                return true;
            if (rejectOverflow && port.Count > maximumDevices)
                return false;
            var devices = new List<TDevice>();
            var count = Math.Min(port.Count, (uint)maximumDevices);
            for (var deviceIndex = 0; deviceIndex < count; deviceIndex++)
            {
                var device = Marshal.PtrToStructure<ExternalCoreApi.ControllerDescription>(port.Types + checked((int)deviceIndex) * deviceSize);
                var name = Marshal.PtrToStringUTF8(device.Description) ?? string.Empty;
                if (!skipEmptyNames || !string.IsNullOrWhiteSpace(name))
                    devices.Add(createDevice(name, device.Id));
            }

            ports.Add(createPort(devices));
        }

        return true;
    }

    internal static IReadOnlyList<T> CopyMemoryMap<T>(nint data, int maximumCount, Func<ulong, nint, nuint, nuint, nuint, nuint, nuint, string?, T> create)
    {
        if (data == nint.Zero)
            return[];
        var map = Marshal.PtrToStructure<ExternalCoreApi.MemoryMap>(data);
        if (map.Descriptors == nint.Zero || map.Count == 0)
            return[];
        var result = new List<T>();
        var count = Math.Min(map.Count, (uint)maximumCount);
        var size = Marshal.SizeOf<ExternalCoreApi.MemoryDescriptor>();
        for (var index = 0; index < count; index++)
        {
            var item = Marshal.PtrToStructure<ExternalCoreApi.MemoryDescriptor>(map.Descriptors + checked((int)index) * size);
            result.Add(create(item.Flags, item.Pointer, item.Offset, item.Start, item.Select, item.Disconnect, item.Length, Marshal.PtrToStringUTF8(item.AddressSpace)));
        }

        return result;
    }

    internal static uint CurrentLanguage() => (uint)(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        "ja" => Language.Japanese,
        "fr" => Language.French,
        "es" => Language.Spanish,
        "de" => Language.German,
        "it" => Language.Italian,
        "nl" => Language.Dutch,
        "pt" when string.Equals(CultureInfo.CurrentUICulture.Name, "pt-BR", StringComparison.OrdinalIgnoreCase) => Language.PortugueseBrazil,
        "pt" => Language.PortuguesePortugal,
        "ru" => Language.Russian,
        "ko" => Language.Korean,
        "zh" when CultureInfo.CurrentUICulture.Name.Contains("Hant", StringComparison.OrdinalIgnoreCase) => Language.ChineseTraditional,
        "zh" => Language.ChineseSimplified,
        "pl" => Language.Polish,
        "vi" => Language.Vietnamese,
        "ar" => Language.Arabic,
        "el" => Language.Greek,
        "tr" => Language.Turkish,
        "he" => Language.Hebrew,
        "fi" => Language.Finnish,
        "id" => Language.Indonesian,
        "sv" => Language.Swedish,
        "uk" => Language.Ukrainian,
        "cs" => Language.Czech,
        "hu" => Language.Hungarian,
        "nb" => Language.Norwegian,
        "th" => Language.Thai,
        _ => Language.English
    });
    private enum Language : uint
    {
        English = 0,
        Japanese = 1,
        French = 2,
        Spanish = 3,
        German = 4,
        Italian = 5,
        Dutch = 6,
        PortugueseBrazil = 7,
        PortuguesePortugal = 8,
        Russian = 9,
        Korean = 10,
        ChineseTraditional = 11,
        ChineseSimplified = 12,
        Polish = 14,
        Vietnamese = 15,
        Arabic = 16,
        Greek = 17,
        Turkish = 18,
        Hebrew = 21,
        Finnish = 23,
        Indonesian = 24,
        Swedish = 25,
        Ukrainian = 26,
        Czech = 27,
        Hungarian = 31,
        Norwegian = 34,
        Thai = 36
    }

    internal static string CopyNativeLogTemplate(nint format, string argumentMarker, string conversionCharacters)
    {
        var source = Marshal.PtrToStringUTF8(format)?.Trim();
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        var result = new StringBuilder(source.Length);
        for (var index = 0; index < source.Length; index++)
        {
            var character = source[index];
            if (character != '%')
            {
                result.Append(character);
                continue;
            }

            if (index + 1 < source.Length && source[index + 1] == '%')
            {
                result.Append('%');
                index++;
                continue;
            }

            result.Append(argumentMarker);
            while (index + 1 < source.Length)
            {
                index++;
                if (conversionCharacters.Contains(source[index], StringComparison.Ordinal))
                    break;
            }
        }

        return result.ToString();
    }


    internal static void SelectDisk(int index, Func<uint> getImageIndex, Func<bool> getEjectState,
        Func<bool, bool> setEjectState, Func<uint, bool> setImageIndex,
        Func<Exception> ejectError, Func<Exception> selectError, Func<Exception> insertError)
    {
        var previousIndex = getImageIndex();
        var wasEjected = getEjectState();
        if (!wasEjected && !setEjectState(true)) throw ejectError();
        try
        {
            if (!setImageIndex((uint)index)) throw selectError();
            if (!setEjectState(false)) throw insertError();
        }
        catch
        {
            if (previousIndex != uint.MaxValue) setImageIndex(previousIndex);
            if (!wasEjected) setEjectState(false);
            throw;
        }
    }

    internal static string? ReadDiskText<T>(T? getter, int index, int bufferSize)
        where T : Delegate
    {
        if (getter is null || index < 0)
            return null;
        var buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            var success = getter switch
            {
                ExternalCoreApi.GetImagePath path => path((uint)index, buffer, checked((nuint)bufferSize)),
                ExternalCoreApi.GetImageLabel label => label((uint)index, buffer, checked((nuint)bufferSize)),
                _ => false
            };
            return success ? Marshal.PtrToStringUTF8(buffer) : null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
