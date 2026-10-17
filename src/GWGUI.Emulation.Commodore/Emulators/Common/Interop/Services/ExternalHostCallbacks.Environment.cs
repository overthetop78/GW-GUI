
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            switch (command)
            {
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
                    var value => throw new NotSupportedException(CoreExceptions.UnsupportedPixelFormat(value))
                    };
                    return true;
                case ExternalCoreApiConstants.GetCoreOptionsVersion:
                    if (data != nint.Zero) Marshal.WriteInt32(data, CoreOptionsVersion);
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
                    if (data != nint.Zero)
                        Marshal.WriteByte(data,
                            Interlocked.Exchange(ref _optionsUpdated, ExternalCoreInteropConstants.InactiveState) != ExternalCoreInteropConstants.InactiveState ? ExternalCoreInteropConstants.NativeBooleanTrue : ExternalCoreInteropConstants.NativeBooleanFalse);
                    return true;
                case ExternalCoreApiConstants.GetDiskControlVersion:
                    if (data != nint.Zero) Marshal.WriteInt32(data, DiskControlVersion);
                    return true;
                case ExternalCoreApiConstants.SetInputDescriptors:
                    return true;
                case ExternalCoreApiConstants.SetKeyboardCallback:
                    var keyboard = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data);
                    _keyboardEvent = keyboard.Callback == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.KeyboardEvent>(keyboard.Callback);
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
                    return false;
                case ExternalCoreApiConstants.GetLedInterface:
                    if (data == nint.Zero) return false;
                    Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
                    {
                        SetLedState = Marshal.GetFunctionPointerForDelegate(Led)
                    }, data, false);
                    return true;
                default:
                    if (_unknownEnvironmentCommands.Add(command)) AddDiagnostic(string.Format(UnsupportedEnvironmentCommandFormat, command));
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void RegisterLegacyOptions(nint data) => OptionCatalog = AdaptOptions(EmulationCoreOptionFunctions.ReadLegacy(data, _options, _optionVisibility));

    private void RegisterVersionTwoOptions(nint data)
    {
        if (EmulationCoreOptionFunctions.ReadVersionTwo(data, _options, _optionVisibility) is { } options)
            OptionCatalog = AdaptOptions(options);
    }

    private bool ApplyOptionVisibility(nint data)
    {
        if (data == nint.Zero) return false;
        var display = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionDisplay>(data);
        var key = display.Key == nint.Zero ? null : Marshal.PtrToStringUTF8(display.Key);
        if (string.IsNullOrWhiteSpace(key)) return false;
        _optionVisibility[key] = display.Visible;
        OptionCatalog = OptionCatalog.Select(option => option.Key.Equals(key, StringComparison.Ordinal) ? option with { IsVisible = display.Visible } : option).ToArray();
        return true;
    }

    private bool CaptureOptionsDisplayCallback(nint data)
    {
        if (data == nint.Zero) return false;
        var callback = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>(data).Callback;
        _updateOptionsDisplay = callback == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.UpdateCoreOptionsDisplay>(callback);
        return true;
    }

    private static IReadOnlyList<CoreOption> AdaptOptions(IReadOnlyList<EmulationOption> options) => options.Select(option =>
        new CoreOption(option.Key, option.Name, option.Description, option.Category, option.DefaultValue, option.CurrentValue,
            option.Values.Select(value => new CoreOptionValue(value.Value, value.Label)).ToArray(), option.IsVisible)).ToArray();

    private bool ReturnOption(nint data)
    {
        if (data == nint.Zero) return true;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is not null && _options.TryGetValue(key, out var value))
        {
            variable.Value = NativeString(value);
            Marshal.StructureToPtr(variable, data, false);
            return true;
        }

        if (data != nint.Zero)
        {
            variable.Value = nint.Zero;
            Marshal.StructureToPtr(variable, data, false);
        }
        return true;
    }

    internal void ValidateConfiguredOptions()
    {
        foreach (var key in _configuredOptionKeys)
        {
            if (key.Equals(_pathOption, StringComparison.Ordinal)) continue;
            var configuredValue = _options[key];
            var option = OptionCatalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == BufferConstants.EmptyCollectionCount) continue;
            if (!option.Values.Any(value => value.Value.Equals(configuredValue, StringComparison.Ordinal)))
                throw new InvalidDataException(CoreExceptions.InvalidOptionValue(configuredValue, key));
        }
    }

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == nint.Zero) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        var message = textPointer == nint.Zero ? null : Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic(string.Format(CoreMessageFormat, extended ? Extended : string.Empty, message));
        return true;
    }

    private bool CaptureControllerInfo(nint data)
    {
        if (!GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.TryCopyControllerPorts<IReadOnlyList<ControllerDevice>, ControllerDevice>(
            data, MaximumNativeControllerPorts, checked((int)MaximumNativeControllerDevices), static (name, id) => new(name, id), static devices => devices,
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
