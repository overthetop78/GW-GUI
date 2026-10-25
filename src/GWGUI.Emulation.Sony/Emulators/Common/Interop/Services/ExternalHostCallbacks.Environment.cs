using CoreAudioBufferStatusCallback = GWGUI.Emulation.Interop.ExternalCoreApi.AudioBufferStatusCallback;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Common.Exceptions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IEmulationCoreEnvironmentCommands
{
    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleAudioBufferCommand(command, data, ref _audioBufferStatusCallback, ref _minimumAudioLatencyMilliseconds, out var audioResult)) return audioResult;
            if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleLegacyQuery(command, data,
                    2, ExternalCoreInteropConstants.MessageInterfaceVersion, out var legacyQueryResult))
                return legacyQueryResult;
            if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleLegacyEnvironmentCommand(command, data,
                    this, true, out var legacyEnvironmentResult))
                return legacyEnvironmentResult;
            switch (command)
            {
                case MultichannelAudioConstants.GetBatchCallback when !_legacyCallbacks:
                    if (data == nint.Zero) return false;
                    Marshal.StructureToPtr(new MultichannelAudioApi.Callback
                    {
                        Int16 = Marshal.GetFunctionPointerForDelegate(MultichannelAudioBatch),
                        Float = nint.Zero
                    }, data, false);
                    return true;
                case ExternalCoreApiConstants.GetRumbleInterface when !_pspCallbacks:
                    return ((IEmulationCoreEnvironmentCommands)this).SetRumbleInterface(data);
                case ExternalCoreApiConstants.Shutdown:
                    _shutdownRequested = true;
                    return true;
                case ExternalCoreApiConstants.SetHardwareRender:
                    return HardwareRender.Configure(data);
                case ExternalCoreApiConstants.GetPreferredHardwareRender:
                    return CommonCoreHostFunctions.EnvironmentProcessing.WriteInteger(data, OpenGlHardwareRenderContext.OpenGlCoreContextType);
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
                    _ = CommonCoreHostFunctions.OptionProcessing.GetAndClearUpdated(data, ref _optionsUpdated);
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
        _pixelFormat = CommonCoreHostFunctions.EnvironmentProcessing.ReadLegacyPixelFormat(data,
            value => new NotSupportedException(ErrorMessages.ForEmulator(_emulatorId, CoreErrorResourceKeys.UnsupportedPixelFormat, value)));
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
        return CommonCoreHostFunctions.EnvironmentProcessing.SetLogInterface(data, Log);
    }

    bool IEmulationCoreEnvironmentCommands.SetLedInterface(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.SetLedInterface(data, Led);
    }

    bool IEmulationCoreEnvironmentCommands.SetRumbleInterface(nint data)
    {
        return !_pspCallbacks && Feedback.Register(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetSensorInterface(nint data) => false;

    bool IEmulationCoreEnvironmentCommands.WriteSystemDirectory(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.WritePointer(data, NativeString(SystemDirectory));
    }

    bool IEmulationCoreEnvironmentCommands.WriteContentDirectory(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.WritePointer(data, NativeString(ContentDirectory));
    }

    bool IEmulationCoreEnvironmentCommands.WriteSaveDirectory(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.WritePointer(data, NativeString(SaveDirectory));
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
        return _virtualFileSystem?.Provide(data) ?? false;
    }

    bool IEmulationCoreEnvironmentCommands.SetKeyboardCallback(nint data)
    {
        var keyboard = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data);
        _keyboardEvent = CommonCoreHostFunctions.EnvironmentProcessing.OptionalDelegate<ExternalCoreApi.KeyboardEvent>(keyboard.Callback);
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
        _ = CommonCoreHostFunctions.EnvironmentProcessing.WriteInteger(data, 1);
        return true;
    }

    private void RegisterLegacyOptions(nint data) => OptionCatalog = CoreOptionMetadataFunctions.ReadLegacy(data, _options, _optionVisibility);

    private void RegisterVersionTwoOptions(nint options)
    {
        if (CoreOptionMetadataFunctions.ReadVersionTwo(options, _options, _optionVisibility) is { } catalog) OptionCatalog = catalog;
    }

    private bool ApplyOptionVisibility(nint data)
    {
        if (!CommonCoreHostFunctions.OptionProcessing.ApplyNativeVisibility(data, _optionVisibility, OptionCatalog,
                static option => option.Key, static (option, visible) => option with { IsVisible = visible },
                out var catalog)) return false;
        OptionCatalog = catalog;
        return true;
    }

    private bool CaptureOptionsDisplayCallback(nint data)
    {
        if (!CommonCoreHostFunctions.OptionProcessing.TryGetDisplayCallback(data, out var callback)) return false;
        _updateOptionsDisplay = callback;
        return true;
    }



    private bool ReturnOption(nint data)
    => CommonCoreHostFunctions.OptionProcessing.ReturnNativeValue(data, _options, (_, value) => NativeString(value));

    internal void ValidateConfiguredOptions() =>
        CommonCoreHostFunctions.OptionProcessing.ValidateConfiguredValues(
            _configuredOptionKeys, _options, OptionCatalog,
            static option => option.Key, static option => option.Values, static value => value.Value,
            (key, value) => throw new InvalidDataException(ErrorMessages.ForEmulator(_emulatorId, CoreErrorResourceKeys.InvalidOptionValue, value, key)));

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == 0) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        CommonCoreHostFunctions.EnvironmentProcessing.CopyDiagnostic(textPointer, message => AddDiagnostic($"[message{(extended ? ExternalHostCallbacksConstants.Extended : string.Empty)}] {message}"));
        return true;
    }

    private bool CaptureControllerInfo(nint data)
    {
        if (!CommonCoreHostFunctions.EnvironmentProcessing.TryCopyControllerPorts<IReadOnlyList<ControllerDevice>, ControllerDevice>(
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

    /// <summary>Callback natif emprunté, conservé uniquement pendant la session du cœur.</summary>
    private CoreAudioBufferStatusCallback? _audioBufferStatusCallback;
    private int _minimumAudioLatencyMilliseconds;
    /// <summary>Instantané de la sortie physique transmis par Common avant le calcul de la frame.</summary>
    internal AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <summary>Latence minimale négociée avec le cœur, en millisecondes.</summary>
    internal int MinimumAudioLatencyMilliseconds => _minimumAudioLatencyMilliseconds;
    /// <summary>Notifie le délégué du cœur avec le calcul commun de remplissage et de sous-alimentation.</summary>
    internal void NotifyAudioBufferStatus() => CommonCoreHostFunctions.EnvironmentProcessing.PublishAudioBufferStatus(_audioBufferStatusCallback, OutputAudioStatus, FramesPerSecond);
    /// <summary>Retire les références empruntées avant le déchargement du code natif.</summary>
    private void ClearAudioBufferFeedback()
    {
        _audioBufferStatusCallback = null;
        OutputAudioStatus = null;
        _minimumAudioLatencyMilliseconds = 0;
    }
}
