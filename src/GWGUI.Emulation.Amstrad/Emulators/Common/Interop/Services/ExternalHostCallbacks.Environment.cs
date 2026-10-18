using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

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
                    _ = ExternalCoreEnvironmentFunctions.WriteBoolean(data, true);
                    return true;
                case ExternalCoreApiConstants.SetMessage:
                    return CaptureMessage(data, extended: false);
                case ExternalCoreApiConstants.SetPixelFormat:
                    _pixelFormat = Marshal.ReadInt32(data) switch
                    {
                        ExternalCoreInteropConstants.PixelFormatXrgb8888 => EmulationPixelFormat.Xrgb8888,
                        ExternalCoreInteropConstants.PixelFormatRgb565 => EmulationPixelFormat.Rgb565,
                    var value => throw new NotSupportedException(CommonExceptions.UnsupportedPixelFormat(value))
                    };
                    return true;
                case ExternalCoreApiConstants.GetCoreOptionsVersion:
                    _ = ExternalCoreEnvironmentFunctions.WriteInteger(data, ExternalCoreInteropConstants.CoreOptionsInterfaceVersion);
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
                    _ = ExternalCoreEnvironmentFunctions.WriteInteger(data, ExternalCoreInteropConstants.DiskControlInterfaceVersion);
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
                    return ExternalCoreEnvironmentFunctions.WriteInteger(data, ExternalCoreInteropConstants.MessageInterfaceVersion);
                case ExternalCoreApiConstants.SetMessageExtended:
                    return CaptureMessage(data, extended: true);
                case ExternalCoreApiConstants.SetFastForwardingOverride:
                    return false;
                case ExternalCoreApiConstants.SetGeometry:
                    return ApplyGeometry(data);
                case ExternalCoreApiConstants.SetSystemAvInfo:
                    return ApplySystemAvInfo(data);
                case ExternalCoreApiConstants.GetLogInterface:
                    return ExternalCoreEnvironmentFunctions.SetLogInterface(data, Log);
                case ExternalCoreApiConstants.GetVfsInterface:
                    return false;
                case ExternalCoreApiConstants.GetLedInterface:
                    return ExternalCoreEnvironmentFunctions.SetLedInterface(data, Led);
                default:
                    if (_unknownEnvironmentCommands.Add(command)) AddDiagnostic($"{ExternalHostCallbacksConstants.UnsupportedEnvironmentCommand}{command}");
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

    private static IReadOnlyList<CoreOption> AdaptOptions(IReadOnlyList<EmulationOption> options) => options.Select(option =>
        new CoreOption(option.Key, option.Name, option.Description, option.Category, option.DefaultValue, option.CurrentValue,
            option.Values.Select(value => new CoreOptionValue(value.Value, value.Label)).ToArray(), option.IsVisible)).ToArray();

    private bool ReturnOption(nint data)
    => EmulationCoreOptionFunctions.ReturnNativeValue(data, _options, (_, value) => NativeString(value));

    internal void ValidateConfiguredOptions()
    {
        foreach (var key in _configuredOptionKeys)
        {
            var configuredValue = _options[key];
            var option = OptionCatalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == BufferConstants.EmptyCollectionCount) continue;
            if (!option.Values.Any(value => value.Value.Equals(configuredValue, StringComparison.Ordinal)))
                throw new InvalidDataException(CommonExceptions.InvalidOptionValue(configuredValue, key));
        }
    }

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == nint.Zero) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        ExternalCoreEnvironmentFunctions.CopyDiagnostic(textPointer, message => AddDiagnostic($"{ExternalHostCallbacksConstants.MessageDiagnosticPrefix}{(extended ? ExternalHostCallbacksConstants.Extended : string.Empty)}{ExternalHostCallbacksConstants.DiagnosticLevelSuffix}{message}"));
        return true;
    }

    private bool CaptureControllerInfo(nint data)
    {
        if (!GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.TryCopyControllerPorts<IReadOnlyList<ControllerDevice>, ControllerDevice>(
            data, MaximumControllerInfoPorts, MaximumControllerTypesPerPort, static (name, id) => new(name, id), static devices => devices,
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
