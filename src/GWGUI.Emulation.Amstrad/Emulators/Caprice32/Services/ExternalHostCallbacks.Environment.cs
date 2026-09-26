using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Exceptions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

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
                    var value => throw new NotSupportedException(Caprice32Exceptions.UnsupportedPixelFormat(value))
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
                    if (data != nint.Zero)
                        Marshal.WriteByte(data,
                            Interlocked.Exchange(ref _optionsUpdated,
                                ExternalCoreInteropConstants.InactiveState)
                            != ExternalCoreInteropConstants.InactiveState
                                ? ExternalCoreInteropConstants.NativeBooleanTrue
                                : ExternalCoreInteropConstants.NativeBooleanFalse);
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
                    SupportsNoGame = data != nint.Zero
                        && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
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

    private void RegisterLegacyOptions(nint data)
    {
        var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
        var catalog = new List<CoreOption>();
        for (var current = data; current != 0; current += size)
        {
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(current);
            if (variable.Key == 0) break;
            var key = Marshal.PtrToStringUTF8(variable.Key)!;
            var definition = Marshal.PtrToStringUTF8(variable.Value);
            var parts = definition?.Split(';', 2) ?? [];
            var name = parts.ElementAtOrDefault(0)?.Trim() ?? key;
            var values = parts.ElementAtOrDefault(1)?.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => new CoreOptionValue(value, value)).ToArray() ?? [];
            var defaultValue = values.FirstOrDefault()?.Value ?? string.Empty;
            if (!_options.ContainsKey(key) && defaultValue.Length > 0) _options[key] = defaultValue;
            catalog.Add(new CoreOption(key, name, null, null, defaultValue, defaultValue, values,
                !_optionVisibility.TryGetValue(key, out var visible) || visible));
        }
        OptionCatalog = catalog;
    }

    private void RegisterVersionTwoOptions(nint options)
    {
        if (options == 0) return;
        var definitions = Marshal.ReadIntPtr(options, IntPtr.Size);
        var definitionSize = (CoreOptionPointerFieldsBeforeValues
            + MaximumCoreOptionValues * CoreOptionValueFieldCount
            + CoreOptionTerminatorFieldCount) * IntPtr.Size;
        var valuesOffset = CoreOptionPointerFieldsBeforeValues * IntPtr.Size;
        var defaultOffset = valuesOffset
            + MaximumCoreOptionValues * CoreOptionValueFieldCount * IntPtr.Size;
        var catalog = new List<CoreOption>();

        for (var optionIndex = 0; optionIndex < MaximumCoreOptionDefinitions; optionIndex++)
        {
            var definition = definitions + optionIndex * definitionSize;
            var keyPointer = Marshal.ReadIntPtr(definition);
            if (keyPointer == 0) break;
            var key = Marshal.PtrToStringUTF8(keyPointer)!;
            var name = StringAt(definition, IntPtr.Size) ?? key;
            var description = StringAt(
                definition, CoreOptionDescriptionPointerIndex * IntPtr.Size);
            var category = StringAt(
                definition, CoreOptionCategoryPointerIndex * IntPtr.Size);
            var defaultValue = StringAt(definition, defaultOffset) ?? string.Empty;
            var values = new List<CoreOptionValue>();
            for (var valueIndex = 0; valueIndex < MaximumCoreOptionValues; valueIndex++)
            {
                var valueOffset = valuesOffset
                    + valueIndex * CoreOptionValueFieldCount * IntPtr.Size;
                var value = StringAt(definition, valueOffset);
                if (value is null) break;
                values.Add(new CoreOptionValue(value, StringAt(definition, valueOffset + IntPtr.Size) ?? value));
            }
            catalog.Add(new CoreOption(key, name, description, category, defaultValue, defaultValue, values,
                !_optionVisibility.TryGetValue(key, out var visible) || visible));
            if (!_options.ContainsKey(key) && defaultValue.Length > 0) _options[key] = defaultValue;
        }
        OptionCatalog = catalog;
    }

    private bool ApplyOptionVisibility(nint data)
    {
        if (data == 0) return false;
        var display = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionDisplay>(data);
        var key = display.Key == 0 ? null : Marshal.PtrToStringUTF8(display.Key);
        if (string.IsNullOrWhiteSpace(key)) return false;
        _optionVisibility[key] = display.Visible;
        OptionCatalog = OptionCatalog.Select(option => option.Key.Equals(key, StringComparison.Ordinal)
            ? option with { IsVisible = display.Visible }
            : option).ToArray();
        return true;
    }

    private bool CaptureOptionsDisplayCallback(nint data)
    {
        if (data == 0) return false;
        var callback = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>(data).Callback;
        _updateOptionsDisplay = callback == 0
            ? null
            : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.UpdateCoreOptionsDisplay>(callback);
        return true;
    }

    private static string? StringAt(nint structure, int offset)
    {
        var pointer = Marshal.ReadIntPtr(structure, offset);
        return pointer == 0 ? null : Marshal.PtrToStringUTF8(pointer);
    }

    private bool ReturnOption(nint data)
    {
        if (data == 0) return true;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is not null && _options.TryGetValue(key, out var value))
        {
            variable.Value = NativeString(value);
            Marshal.StructureToPtr(variable, data, false);
            return true;
        }

        if (data != 0)
        {
            variable.Value = 0;
            Marshal.StructureToPtr(variable, data, false);
        }
        return true;
    }

    internal void ValidateConfiguredOptions()
    {
        foreach (var key in _configuredOptionKeys)
        {
            var configuredValue = _options[key];
            var option = OptionCatalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == 0) continue;
            if (!option.Values.Any(value => value.Value.Equals(configuredValue, StringComparison.Ordinal)))
                throw new InvalidDataException(Caprice32Exceptions.InvalidOptionValue(configuredValue, key));
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
        if (data == 0)
        {
            ControllerPorts = [];
            return true;
        }
        var ports = new List<IReadOnlyList<ControllerDevice>>();
        var infoSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var descriptionSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        for (var port = 0; port < 16; port++)
        {
            var info = Marshal.PtrToStructure<ExternalCoreApi.ControllerInfo>(data + port * infoSize);
            if (info.Types == 0 || info.Count == 0) break;
            if (info.Count > 64) return false;
            var devices = new List<ControllerDevice>(checked((int)info.Count));
            for (var index = 0; index < info.Count; index++)
            {
                var description = Marshal.PtrToStructure<ExternalCoreApi.ControllerDescription>(
                    info.Types + checked((int)index) * descriptionSize);
                var name = description.Description == 0 ? null : Marshal.PtrToStringUTF8(description.Description);
                if (!string.IsNullOrWhiteSpace(name)) devices.Add(new ControllerDevice(name, description.Id));
            }
            ports.Add(devices);
        }
        ControllerPorts = ports;
        return true;
    }

    private nint NativeString(string value)
    {
        if (_nativeStrings.TryGetValue(value, out var existing)) return existing;
        var pointer = Marshal.StringToCoTaskMemUTF8(value);
        _nativeStrings.Add(value, pointer);
        return pointer;
    }

}
