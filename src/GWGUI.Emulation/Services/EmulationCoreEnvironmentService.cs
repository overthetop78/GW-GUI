using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat d’environnement aux fonctions existantes sans reproduire leurs traitements.</summary>
internal sealed class EmulationCoreEnvironmentService : IEmulationCoreEnvironmentProcessing
{
    /// <inheritdoc/>
    public bool ApplyStructure<T>(nint data, Action<T> apply) where T : struct
        => ExternalCoreEnvironmentFunctions.ApplyStructure<T>(data, apply);

    /// <inheritdoc/>
    public void ApplySystemAvInfo(ExternalCoreApi.SystemAvInfo info, Action<ExternalCoreApi.Geometry> applyGeometry, Action<double> setFramesPerSecond, Action<int> setSampleRate)
        => ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(info, applyGeometry, setFramesPerSecond, setSampleRate);

    /// <inheritdoc/>
    public IReadOnlyList<TPort> CopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices, Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort)
        => ExternalCoreEnvironmentFunctions.CopyControllerPorts<TPort, TDevice>(data, maximumPorts, maximumDevices, createDevice, createPort);

    /// <inheritdoc/>
    public string? CopyDiagnostic(nint textPointer, Action<string> publish, bool allowWhitespace = false)
        => ExternalCoreEnvironmentFunctions.CopyDiagnostic(textPointer, publish, allowWhitespace);

    /// <inheritdoc/>
    public IReadOnlyList<T> CopyInputDescriptors<T>(nint data, int maximumCount, Func<uint, uint, uint, uint, string, T> create)
        => ExternalCoreEnvironmentFunctions.CopyInputDescriptors<T>(data, maximumCount, create);

    /// <inheritdoc/>
    public IReadOnlyList<T> CopyMemoryMap<T>(nint data, int maximumCount, Func<ulong, nint, nuint, nuint, nuint, nuint, nuint, string?, T> create)
        => ExternalCoreEnvironmentFunctions.CopyMemoryMap<T>(data, maximumCount, create);

    /// <inheritdoc/>
    public string CopyNativeLogTemplate(nint format, string argumentMarker, string conversionCharacters)
        => ExternalCoreEnvironmentFunctions.CopyNativeLogTemplate(format, argumentMarker, conversionCharacters);

    /// <inheritdoc/>
    public uint CurrentLanguage()
        => ExternalCoreEnvironmentFunctions.CurrentLanguage();

    /// <inheritdoc/>
    public bool HandleRemainingCommand(uint command, nint data, IEmulationCoreOptionCommands options, ISet<uint> unknownCommands, ICollection<string> diagnostics, Func<uint, string> createUnknownDiagnostic)
        => ExternalCoreEnvironmentFunctions.HandleRemainingCommand(command, data, options, unknownCommands, diagnostics, createUnknownDiagnostic);

    /// <inheritdoc/>
    public void InstallCallbacks(ExternalCoreExports exports, IEmulationCoreCallbacks callbacks)
        => ExternalCoreEnvironmentFunctions.InstallCallbacks(exports, callbacks);

    /// <inheritdoc/>
    public T? OptionalDelegate<T, TSource>(TSource source, Func<TSource, nint> readPointer) where T : Delegate
        => ExternalCoreEnvironmentFunctions.OptionalDelegate<T, TSource>(source, readPointer);

    /// <inheritdoc/>
    public T? OptionalDelegate<T>(nint pointer) where T : Delegate
        => ExternalCoreEnvironmentFunctions.OptionalDelegate<T>(pointer);

    /// <inheritdoc/>
    public string? ReadDiskText<T>(T? getter, int index, int bufferSize) where T : Delegate
        => ExternalCoreEnvironmentFunctions.ReadDiskText<T>(getter, index, bufferSize);

    /// <inheritdoc/>
    public EmulationPixelFormat ReadLegacyPixelFormat(nint data, Func<int, Exception> createUnsupportedError)
        => ExternalCoreEnvironmentFunctions.ReadLegacyPixelFormat(data, createUnsupportedError);

    /// <inheritdoc/>
    public T RequiredDelegate<T>(nint pointer, Func<Exception> createError) where T : Delegate
        => ExternalCoreEnvironmentFunctions.RequiredDelegate<T>(pointer, createError);

    /// <inheritdoc/>
    public void SelectDisk(int index, Func<uint> getImageIndex, Func<bool> getEjectState,
        Func<bool, bool> setEjectState, Func<uint, bool> setImageIndex,
        Func<Exception> ejectError, Func<Exception> selectError, Func<Exception> insertError)
        => ExternalCoreEnvironmentFunctions.SelectDisk(index, getImageIndex, getEjectState, setEjectState, setImageIndex, ejectError, selectError, insertError);

    /// <inheritdoc/>
    public bool SetLedInterface(nint data, ExternalCoreApi.SetLedState setState)
        => ExternalCoreEnvironmentFunctions.SetLedInterface(data, setState);

    /// <inheritdoc/>
    public bool SetLogInterface(nint data, ExternalCoreApi.LogCallback log)
        => ExternalCoreEnvironmentFunctions.SetLogInterface(data, log);

    /// <inheritdoc/>
    public bool SetLogInterface<TLog>(nint data, TLog log) where TLog : Delegate
        => ExternalCoreEnvironmentFunctions.SetLogInterface<TLog>(data, log);

    /// <inheritdoc/>
    public bool SetPixelFormat(nint data, ref EmulationPixelFormat pixelFormat)
        => ExternalCoreEnvironmentFunctions.SetPixelFormat(data, ref pixelFormat);

    /// <inheritdoc/>
    public bool SetRumbleInterface(nint data, ExternalCoreApi.SetRumbleState setState)
        => ExternalCoreEnvironmentFunctions.SetRumbleInterface(data, setState);

    /// <inheritdoc/>
    public bool SetSensorInterface(nint data, ExternalCoreApi.SetSensorState setState, ExternalCoreApi.GetSensorInput getInput)
        => ExternalCoreEnvironmentFunctions.SetSensorInterface(data, setState, getInput);

    /// <inheritdoc/>
    public bool TryCopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices,
        Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort,
        bool rejectOverflow, bool skipEmptyNames, out IReadOnlyList<TPort> result)
        => ExternalCoreEnvironmentFunctions.TryCopyControllerPorts<TPort, TDevice>(data, maximumPorts, maximumDevices, createDevice, createPort, rejectOverflow, skipEmptyNames, out result);

    /// <inheritdoc/>
    public bool TryHandleCallbackCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, data, callbacks, out result);

    /// <inheritdoc/>
    public bool TryHandleLegacyCallbackCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, IEmulationCoreOptionCommands options, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(command, data, callbacks, options, out result);

    /// <inheritdoc/>
    public bool TryHandleLegacyEnvironmentCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, bool captureNullDiskControls, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleLegacyEnvironmentCommand(command, data, callbacks, captureNullDiskControls, out result);

    /// <inheritdoc/>
    public bool TryHandleLegacyQuery(uint command, nint data, uint coreOptionsVersion, uint messageInterfaceVersion, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleLegacyQuery(command, data, coreOptionsVersion, messageInterfaceVersion, out result);

    /// <inheritdoc/>
    public bool TryHandleOptionCommand(uint command, nint data, IEmulationCoreOptionCommands options, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(command, data, options, out result);

    /// <inheritdoc/>
    public bool TryHandleStatelessCommand(uint command, nint data, uint coreOptionsVersion, uint messageInterfaceVersion, ulong inputCapabilities, out bool result)
        => ExternalCoreEnvironmentFunctions.TryHandleStatelessCommand(command, data, coreOptionsVersion, messageInterfaceVersion, inputCapabilities, out result);

    /// <inheritdoc/>
    public bool WriteBoolean(nint destination, bool value)
        => ExternalCoreEnvironmentFunctions.WriteBoolean(destination, value);

    /// <inheritdoc/>
    public bool WriteInteger(nint destination, int value)
        => ExternalCoreEnvironmentFunctions.WriteInteger(destination, value);

    /// <inheritdoc/>
    public bool WriteInteger(nint destination, uint value)
        => ExternalCoreEnvironmentFunctions.WriteInteger(destination, value);

    /// <inheritdoc/>
    public bool WritePointer(nint destination, nint value)
        => ExternalCoreEnvironmentFunctions.WritePointer(destination, value);

    /// <inheritdoc/>
    public bool WriteUnsignedLong(nint destination, ulong value)
        => ExternalCoreEnvironmentFunctions.WriteUnsignedLong(destination, value);
}
