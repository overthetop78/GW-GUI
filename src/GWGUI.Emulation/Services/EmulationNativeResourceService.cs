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
}
