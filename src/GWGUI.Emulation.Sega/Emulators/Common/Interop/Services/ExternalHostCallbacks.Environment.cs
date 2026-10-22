using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IEmulationCoreEnvironmentCommands, IEmulationCoreOptionCommands
{
    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            if (ExternalCoreEnvironmentFunctions.TryHandleLegacyQuery(command, data,
                    2, ExternalCoreInteropConstants.MessageInterfaceVersion, out var legacyQueryResult))
                return legacyQueryResult;
            if (ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(command, data,
                    this, this, out var legacyCallbackResult))
                return legacyCallbackResult;
            switch (command)
            {
                case SetContentInfoOverride:
                    return CaptureContentOverrides(data);
                case GetGameInfoExtended:
                    return ProvideExtendedGameInfo(data);
                case HardwareRenderConstants.SetHardwareRender:
                    return ConfigureHardwareRender(data);
                case HardwareRenderConstants.GetPreferredHardwareRender:
                    return ExternalCoreEnvironmentFunctions.WriteInteger(data, OpenGlHardwareRenderContext.OpenGlCoreContextType);
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

    bool IEmulationCoreEnvironmentCommands.ReadMessage(nint data)
    {
        return CaptureMessage(data, extended: false);
    }

    bool IEmulationCoreEnvironmentCommands.SetPixelFormat(nint data)
    {
        _pixelFormat = ExternalCoreEnvironmentFunctions.ReadLegacyPixelFormat(data,
    static value => new NotSupportedException(CoreExceptions.UnsupportedPixelFormat(value)));
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.GetDiskControlVersion(nint data)
    {
        _ = ExternalCoreEnvironmentFunctions.WriteInteger(data, 1);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureInputDescriptors(nint data)
    {
        return true;
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

    bool IEmulationCoreEnvironmentCommands.SetSupportNoGame(nint data)
    {
        SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.ReadExtendedMessage(nint data)
    {
        return CaptureMessage(data, extended: true);
    }

    bool IEmulationCoreEnvironmentCommands.SetGeometry(nint data)
    {
        return ApplyGeometry(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetSystemAvInfo(nint data)
    {
        return ApplySystemAvInfo(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetLogInterface(nint data)
    {
        return ExternalCoreEnvironmentFunctions.SetLogInterface(data, Log);
    }

    bool IEmulationCoreEnvironmentCommands.ProvideVfs(nint data)
    {
        return false;
    }

    bool IEmulationCoreEnvironmentCommands.SetLedInterface(nint data)
    {
        return ExternalCoreEnvironmentFunctions.SetLedInterface(data, Led);
    }

    bool IEmulationCoreEnvironmentCommands.SetRotation(nint data) => false;
    bool IEmulationCoreEnvironmentCommands.CapturePerformanceLevel(nint data) => false;
    bool IEmulationCoreEnvironmentCommands.SetRumbleInterface(nint data) => false;
    bool IEmulationCoreEnvironmentCommands.SetSensorInterface(nint data) => false;

    void IEmulationCoreOptionCommands.RegisterVersionOne(nint data)
    {
        RegisterVersionOneOptions(data);
    }

    void IEmulationCoreOptionCommands.RegisterVersionOneInternational(nint data)
    {
        RegisterVersionOneOptions(Marshal.ReadIntPtr(data));
    }

    void IEmulationCoreOptionCommands.RegisterVersionTwo(nint data)
    {
        RegisterVersionTwoOptions(data);
    }

    void IEmulationCoreOptionCommands.RegisterVersionTwoInternational(nint data)
    {
        RegisterVersionTwoOptions(Marshal.ReadIntPtr(data));
    }

    void IEmulationCoreOptionCommands.RegisterLegacyVariables(nint data)
    {
        RegisterLegacyOptions(data);
    }

    bool IEmulationCoreOptionCommands.ReturnValue(nint data)
    {
        return ReturnOption(data);
    }

    bool IEmulationCoreOptionCommands.GetAndClearUpdated(nint data)
    {
        _ = EmulationCoreOptionFunctions.GetAndClearUpdated(data, ref _optionsUpdated);
        return true;
    }

    bool IEmulationCoreOptionCommands.ApplyVisibility(nint data)
    {
        return ApplyOptionVisibility(data);
    }

    bool IEmulationCoreOptionCommands.CaptureDisplayUpdate(nint data)
    {
        return CaptureOptionsDisplayCallback(data);
    }

    bool IEmulationCoreOptionCommands.SetNativeValue(nint data) => false;

    private void RegisterLegacyOptions(nint data) => OptionCatalog = AdaptOptions(EmulationCoreOptionFunctions.ReadLegacy(data, _options, _optionVisibility));

    private void RegisterVersionOneOptions(nint data)
    {
        if (EmulationCoreOptionFunctions.ReadVersionOne(data, _options, _optionVisibility) is { } options)
            OptionCatalog = AdaptOptions(options);
    }

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
    => EmulationCoreOptionFunctions.ReturnNativeValue(data, _options, (_, value) => NativeString(value), unavailableResult: false);

    internal void ValidateConfiguredOptions() =>
        EmulationCoreOptionFunctions.ValidateConfiguredValues(
            _configuredOptionKeys, _options, OptionCatalog,
            static option => option.Key, static option => option.Values, static value => value.Value,
            static (key, value) => throw new InvalidDataException(CoreExceptions.InvalidOptionValue(value, key)));

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
        return CommonCoreHostFunctions.NativeResources.AddString(_nativeStrings, value, value).Pointer;
    }

}
