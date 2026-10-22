using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat de ressources aux allocations existantes, sans modifier leur durée de vie.</summary>
internal sealed class EmulationNativeResourceService : IEmulationNativeResources
{
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
}
