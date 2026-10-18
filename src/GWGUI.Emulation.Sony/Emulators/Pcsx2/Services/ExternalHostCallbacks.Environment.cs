using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using ExternalCoreInteropConstants = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalCoreInteropConstants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

internal sealed partial class ExternalHostCallbacks
{
    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            switch (command)
            {
                case ExternalCoreApiConstants.GetRumbleInterface:
                    return Feedback.Register(data);
                case ExternalCoreApiConstants.Shutdown:
                    _shutdownRequested = true;
                    return true;
                case ExternalCoreApiConstants.SetHardwareRender:
                    return HardwareRender.Configure(data);
                case ExternalCoreApiConstants.GetPreferredHardwareRender:
                    if (data == nint.Zero) return false;
                    Marshal.WriteInt32(data, OpenGlHardwareRenderContext.OpenGlCoreContextType);
                    return true;
                case ExternalCoreApiConstants.GetSystemDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SystemDirectory));
                    return true;
                case ExternalCoreApiConstants.GetContentDirectory:
                    Marshal.WriteIntPtr(data, NativeString(ContentDirectory));
                    return true;
                case ExternalCoreApiConstants.GetSaveDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SaveDirectory));
                    return true;
                case ExternalCoreApiConstants.GetCanDuplicateFrames:
                case ExternalCoreApiConstants.GetInputBitmasks:
                    if (data != nint.Zero)
                        Marshal.WriteByte(data, ExternalCoreInteropConstants.NativeBooleanTrue);
                    return true;
                case ExternalCoreApiConstants.SetMessage:
                    return CaptureMessage(data, extended: false);
                case ExternalCoreApiConstants.SetPixelFormat:
                    _pixelFormat = Marshal.ReadInt32(data) switch
                    {
                        ExternalCoreInteropConstants.PixelFormatXrgb8888 => EmulationPixelFormat.Xrgb8888,
                        ExternalCoreInteropConstants.PixelFormatRgb565 => EmulationPixelFormat.Rgb565,
                    var value => throw new NotSupportedException(Pcsx2Exceptions.UnsupportedPixelFormat(value))
                    };
                    return true;
                case ExternalCoreApiConstants.GetCoreOptionsVersion:
                    if (data != 0) Marshal.WriteInt32(data, 2);
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsV2:
                    RegisterVersionTwoOptions(data);
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsV2International:
                    RegisterVersionTwoOptions(Marshal.ReadIntPtr(data));
                    return true;
                case ExternalCoreApiConstants.SetVariables:
                    RegisterLegacyOptions(data);
                    return true;
                case ExternalCoreApiConstants.GetVariable:
                    return ReturnOption(data);
                case ExternalCoreApiConstants.GetVariableUpdate:
                    _ = EmulationCoreOptionFunctions.GetAndClearUpdated(data, ref _optionsUpdated);
                    return true;
                case ExternalCoreApiConstants.GetDiskControlVersion:
                    if (data != 0) Marshal.WriteInt32(data, 1);
                    return true;
                case ExternalCoreApiConstants.SetInputDescriptors:
                    return true;
                case ExternalCoreApiConstants.SetKeyboardCallback:
                    var keyboard = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data);
                    _keyboardEvent = keyboard.Callback == 0 ? null : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.KeyboardEvent>(keyboard.Callback);
                    return true;
                case ExternalCoreApiConstants.SetDiskControl:
                    DiskControl.Capture(data);
                    return true;
                case ExternalCoreApiConstants.SetDiskControlExtended:
                    DiskControl.CaptureExtended(data);
                    return true;
                case ExternalCoreApiConstants.SetControllerInfo:
                    return CaptureControllerInfo(data);
                case ExternalCoreApiConstants.SetMemoryMaps:
                case ExternalCoreApiConstants.SetSupportAchievements:
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                    return ApplyOptionVisibility(data);
                case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                    return CaptureOptionsDisplayCallback(data);
                case ExternalCoreApiConstants.SetSupportNoGame:
                    SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
                    return true;
                case ExternalCoreApiConstants.GetMessageInterfaceVersion:
                    if (data != nint.Zero)
                        Marshal.WriteInt32(data, ExternalCoreInteropConstants.MessageInterfaceVersion);
                    return data != nint.Zero;
                case ExternalCoreApiConstants.SetMessageExtended:
                    return CaptureMessage(data, extended: true);
                case ExternalCoreApiConstants.SetFastForwardingOverride:
                    return false;
                case ExternalCoreApiConstants.SetGeometry:
                    return ApplyGeometry(data);
                case ExternalCoreApiConstants.SetSystemAvInfo:
                    return ApplySystemAvInfo(data);
                case ExternalCoreApiConstants.GetLogInterface:
                    Marshal.StructureToPtr(new ExternalCoreApi.LogInterface
                    {
                        Log = Marshal.GetFunctionPointerForDelegate(Log)
                    }, data, false);
                    return true;
                case ExternalCoreApiConstants.GetVfsInterface:
                    return _virtualFileSystem.Provide(data);
                case ExternalCoreApiConstants.GetLedInterface:
                    if (data == 0) return false;
                    Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
                    {
                        SetLedState = Marshal.GetFunctionPointerForDelegate(Led)
                    }, data, false);
                    return true;
                default:
                    if (_unknownEnvironmentCommands.Add(command)) AddDiagnostic($"Unsupported environment command: {command}");
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void RegisterLegacyOptions(nint data) => OptionCatalog = CoreOptionMetadataFunctions.ReadLegacy(data, _options, _optionVisibility);

    private void RegisterVersionTwoOptions(nint options)
    {
        if (CoreOptionMetadataFunctions.ReadVersionTwo(options, _options, _optionVisibility) is { } catalog) OptionCatalog = catalog;
    }

    private bool ApplyOptionVisibility(nint data)
    {
        if (!EmulationCoreOptionFunctions.ApplyNativeVisibility(data, _optionVisibility, OptionCatalog,
                static option => option.Key, static (option, visible) => option with { IsVisible = visible },
                out var catalog)) return false;
        OptionCatalog = catalog;
        return true;
    }

    private bool CaptureOptionsDisplayCallback(nint data)
    {
        if (!EmulationCoreOptionFunctions.TryGetDisplayCallback(data, out var callback)) return false;
        _updateOptionsDisplay = callback;
        return true;
    }



    private bool ReturnOption(nint data)
    => EmulationCoreOptionFunctions.ReturnNativeValue(data, _options, (_, value) => NativeString(value));

    internal void ValidateConfiguredOptions()
    {
        foreach (var key in _configuredOptionKeys)
        {
            var configuredValue = _options[key];
            var option = OptionCatalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == 0) continue;
            if (!option.Values.Any(value => value.Value.Equals(configuredValue, StringComparison.Ordinal)))
                throw new InvalidDataException(Pcsx2Exceptions.InvalidOptionValue(configuredValue, key));
        }
    }

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == 0) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        var message = textPointer == 0 ? null : Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic($"[message{(extended ? ExternalHostCallbacksConstants.Extended : string.Empty)}] {message}");
        return true;
    }

    private bool CaptureControllerInfo(nint data)
    {
        if (!GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.TryCopyControllerPorts<IReadOnlyList<ControllerDevice>, ControllerDevice>(
            data, 16, 64, static (name, id) => new(name, id), static devices => devices,
            rejectOverflow: true, skipEmptyNames: true, out var ports)) return false;
        ControllerPorts = ports;
        return true;
    }

    private nint NativeString(string value)
    {
        if (_nativeStrings.TryGetValue(value, out var existing)) return existing.Pointer;
        return ExternalCoreUtf8String.AddTo(_nativeStrings, value, value).Pointer;
    }

}
