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

internal sealed partial class ExternalHostCallbacks : IEmulationCoreEnvironmentCommands
{
    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            if (ExternalCoreEnvironmentFunctions.TryHandleLegacyQuery(command, data,
                    2, ExternalCoreInteropConstants.MessageInterfaceVersion, out var legacyQueryResult))
                return legacyQueryResult;
            if (ExternalCoreEnvironmentFunctions.TryHandleLegacyEnvironmentCommand(command, data,
                    this, true, out var legacyEnvironmentResult))
                return legacyEnvironmentResult;
            switch (command)
            {
                case ExternalCoreApiConstants.GetRumbleInterface:
                    return ((IEmulationCoreEnvironmentCommands)this).SetRumbleInterface(data);
                case ExternalCoreApiConstants.Shutdown:
                    _shutdownRequested = true;
                    return true;
                case ExternalCoreApiConstants.SetHardwareRender:
                    return HardwareRender.Configure(data);
                case ExternalCoreApiConstants.GetPreferredHardwareRender:
                    return ExternalCoreEnvironmentFunctions.WriteInteger(data, OpenGlHardwareRenderContext.OpenGlCoreContextType);
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
                case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                    return ApplyOptionVisibility(data);
                case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                    return CaptureOptionsDisplayCallback(data);
                case ExternalCoreApiConstants.SetFastForwardingOverride:
                    return false;
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

    bool IEmulationCoreEnvironmentCommands.SetRotation(nint data) => false;

    bool IEmulationCoreEnvironmentCommands.CapturePerformanceLevel(nint data) => false;

    bool IEmulationCoreEnvironmentCommands.SetPixelFormat(nint data)
    {
        _pixelFormat = ExternalCoreEnvironmentFunctions.ReadLegacyPixelFormat(data,
            static value => new NotSupportedException(Pcsx2Exceptions.UnsupportedPixelFormat(value)));
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.SetSystemAvInfo(nint data)
    {
        return ApplySystemAvInfo(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetGeometry(nint data)
    {
        return ApplyGeometry(data);
    }

    bool IEmulationCoreEnvironmentCommands.ReadMessage(nint data)
    {
        return CaptureMessage(data, extended: false);
    }

    bool IEmulationCoreEnvironmentCommands.ReadExtendedMessage(nint data)
    {
        return CaptureMessage(data, extended: true);
    }

    bool IEmulationCoreEnvironmentCommands.SetLogInterface(nint data)
    {
        return ExternalCoreEnvironmentFunctions.SetLogInterface(data, Log);
    }

    bool IEmulationCoreEnvironmentCommands.SetLedInterface(nint data)
    {
        return ExternalCoreEnvironmentFunctions.SetLedInterface(data, Led);
    }

    bool IEmulationCoreEnvironmentCommands.SetRumbleInterface(nint data)
    {
        return Feedback.Register(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetSensorInterface(nint data) => false;

    bool IEmulationCoreEnvironmentCommands.WriteSystemDirectory(nint data)
    {
        return ExternalCoreEnvironmentFunctions.WritePointer(data, NativeString(SystemDirectory));
    }

    bool IEmulationCoreEnvironmentCommands.WriteContentDirectory(nint data)
    {
        return ExternalCoreEnvironmentFunctions.WritePointer(data, NativeString(ContentDirectory));
    }

    bool IEmulationCoreEnvironmentCommands.WriteSaveDirectory(nint data)
    {
        return ExternalCoreEnvironmentFunctions.WritePointer(data, NativeString(SaveDirectory));
    }

    bool IEmulationCoreEnvironmentCommands.SetSupportNoGame(nint data)
    {
        SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureInputDescriptors(nint data)
    {
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureControllerInfo(nint data)
    {
        return CaptureControllerInfo(data);
    }

    bool IEmulationCoreEnvironmentCommands.CaptureMemoryMaps(nint data)
    {
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.SetSupportAchievements(nint data)
    {
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.ProvideVfs(nint data)
    {
        return _virtualFileSystem.Provide(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetKeyboardCallback(nint data)
    {
        var keyboard = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data);
        _keyboardEvent = GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.KeyboardEvent>(keyboard.Callback);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureDiskControl(nint data)
    {
        DiskControl.Capture(data);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureExtendedDiskControl(nint data)
    {
        DiskControl.CaptureExtended(data);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.GetDiskControlVersion(nint data)
    {
        _ = ExternalCoreEnvironmentFunctions.WriteInteger(data, 1);
        return true;
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

    internal void ValidateConfiguredOptions() =>
        EmulationCoreOptionFunctions.ValidateConfiguredValues(
            _configuredOptionKeys, _options, OptionCatalog,
            static option => option.Key, static option => option.Values, static value => value.Value,
            static (key, value) => throw new InvalidDataException(Pcsx2Exceptions.InvalidOptionValue(value, key)));

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == 0) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        ExternalCoreEnvironmentFunctions.CopyDiagnostic(textPointer, message => AddDiagnostic($"[message{(extended ? ExternalHostCallbacksConstants.Extended : string.Empty)}] {message}"));
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
