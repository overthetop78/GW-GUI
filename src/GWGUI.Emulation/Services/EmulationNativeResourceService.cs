using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat de ressources aux allocations existantes, sans modifier leur durée de vie.</summary>
internal sealed class EmulationNativeResourceService : IEmulationNativeResources
{
    public IEmulationHardwareRenderContext CreateHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback, Func<string> configurationInvalid) => new EmulationOpenGlHardwareRenderContext(callback, configurationInvalid);

    /// <inheritdoc/>
    public IEmulationDiskControl CreateDiskControl(IEmulationDiskControlPolicy policy, Func<string, IEmulationLoadedContent>? createContent = null, Func<string, bool>? mediaExists = null)
        => createContent is null ? new ExternalCoreDiskControl(policy, mediaExists) : new ExternalRichDiskControl(policy, createContent, mediaExists);

    /// <inheritdoc/>
    public IEmulationNativeString CreateString(string value, int minimumByteCapacity = 0) => new ExternalCoreUtf8String(value, minimumByteCapacity);

    /// <inheritdoc/>
    public IEmulationNativeString CreateString(string value, bool useWindowsAnsi) => new ExternalCoreUtf8String(value, useWindowsAnsi);

    /// <inheritdoc/>
    public IEmulationNativeString AddString(IDictionary<string, IEmulationNativeString> owner, string key, string value) => ExternalCoreUtf8String.AddTo(owner, key, value);

    /// <inheritdoc/>
    public IEmulationLoadedContent CreateLoadedContent(string absolutePath, bool needsFullPath, bool useWindowsAnsiPath) => ExternalCoreLoadedContent.Create(absolutePath, needsFullPath, useWindowsAnsiPath);

    /// <inheritdoc/>
    public IEmulationVirtualFileSystem CreateVirtualFileSystem(Action<string, long>? readObserver = null, Func<string, (string Path, bool IsReadOnly)>? pathResolver = null) => new ExternalCoreVirtualFileSystem(readObserver, pathResolver);

    /// <inheritdoc/>
    public IEmulationCoreLibrary CreateLibrary(string absolutePath) => new ExternalCoreLibrary(absolutePath);

    /// <inheritdoc/>
    public void ValidateLibraryPath(string absolutePath, Func<Exception> requireAbsolute, Func<string, Exception> missingFile) => ExternalCoreLibrary.ValidatePath(absolutePath, requireAbsolute, missingFile);

    /// <inheritdoc/>
    public T InspectLibrary<T>(string absolutePath, uint expectedApiVersion, Action<IEmulationCoreLibrary> validateExports, Func<uint, Exception> unsupportedApi,
        Func<ExternalCoreApi.SystemInfo, T> readInfo, Func<EntryPointNotFoundException, Exception> missingExport, Func<BadImageFormatException, Exception> invalidLibrary)
        => ExternalCoreLibrary.Inspect(absolutePath, expectedApiVersion, validateExports, unsupportedApi, readInfo, missingExport, invalidLibrary);

    /// <inheritdoc/>
    public TInfo ReadInfo<TExpected, TInfo>(ExternalCoreApi.SystemInfo nativeInfo, TExpected expected, Func<TExpected, string> expectedNameFor, Func<nint, IReadOnlySet<string>> parseExtensions,
        Func<string, string, Exception> identityMismatch, Func<TExpected, string, string, IReadOnlySet<string>, bool, bool, TInfo> create)
        => ExternalCoreLibrary.ReadInfo(nativeInfo, expected, expectedNameFor, parseExtensions, identityMismatch, create);

    /// <inheritdoc/>
    public HashSet<string> ParseExtensions(nint value) => ExternalCoreLibrary.ParseExtensions(value);

    /// <inheritdoc/>
    public IEmulationCoreOptionHost<TOption> CreateCoreOptionHost<TOption, TValue>(IReadOnlyDictionary<string, string> configured, Func<TOption, string> keyFor, Func<TOption, string> defaultFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor, Func<TOption, string, TOption> withCurrent, Func<TOption, bool, TOption> withVisibility,
        Func<TOption, string, bool, TOption> initialize, Func<TOption, TOption>? present = null) where TOption : class
        => new EmulationCoreOptionHost<TOption, TValue>(configured, keyFor, defaultFor, choicesFor, valueFor, withCurrent, withVisibility, initialize, present);

    /// <inheritdoc/>
    public IEmulationVariableOptionHost<TOption> CreateVariableOptionHost<TOption>(IReadOnlyDictionary<string, string> values,
        Func<string, string, string, string, IReadOnlyList<string>, TOption> createOption, Func<TOption, string, bool> accepts, Func<TOption, string, TOption> withCurrent)
        => new EmulationLegacyOptionHost<TOption>(values, createOption, accepts, withCurrent);

    /// <inheritdoc/>
    public IEmulationAudioCallbackHost CreateAudioCallbackHost() => new ExternalCoreAudioCallbackHost();

    /// <inheritdoc/>
    public IEmulationVideoCallbackHost CreateVideoCallbackHost() => new ExternalCoreVideoCallbackHost();

    /// <inheritdoc/>
    public IEmulationEnvironmentCallbackHost CreateEnvironmentCallbackHost(IEmulationVideoCallbackHost video, IEmulationAudioCallbackHost audio, IReadOnlyDictionary<string, string> options,
        string systemDirectory, string contentDirectory, string saveDirectory, string assetsDirectory, Func<IReadOnlyDictionary<string, string>, IEmulationVariableOptionHost<EmulationEnabledOption>> createOptions)
        => new ExternalCoreEnvironmentCallbackHost(video, audio, options, systemDirectory, contentDirectory, saveDirectory, assetsDirectory, createOptions);
}
