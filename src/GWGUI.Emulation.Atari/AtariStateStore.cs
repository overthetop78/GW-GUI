namespace GWGUI.Emulation.Atari;

public sealed class AtariStateStore(string stateRoot)
{
    private readonly string _stateRoot = Path.GetFullPath(
        string.IsNullOrWhiteSpace(stateRoot) ? throw new ArgumentException(nameof(stateRoot)) : stateRoot);

    public ValueTask<AtariStoredStateMetadata> SaveQuickStateAsync(IAtariMachine machine,
        byte[]? capture = null, CancellationToken cancellationToken = default) =>
        SaveAsync(machine, AtariStateStoreConstants.QuickStateName, AtariStoredStateKind.Quick,
            capture, cancellationToken);

    public ValueTask<AtariStoredStateMetadata> SaveNamedStateAsync(IAtariMachine machine, string name,
        byte[]? capture = null, CancellationToken cancellationToken = default) =>
        SaveAsync(machine, AtariStateStoreFunctions.ValidateStateName(name), AtariStoredStateKind.Named,
            capture, cancellationToken);

    public async ValueTask RestoreAsync(IAtariMachine machine, string name, AtariStoredStateKind kind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(machine);
        var machineDirectory = AtariStateStoreFunctions.GetMachineDirectory(_stateRoot, machine.Configuration.Id);
        var stem = AtariStateStoreFunctions.GetFileStem(kind, name);
        await machine.LoadStateAsync(AtariStateStoreFunctions.StatePath(machineDirectory, stem), cancellationToken)
            .ConfigureAwait(false);
    }

    public IReadOnlyList<AtariStoredStateMetadata> List(Guid configurationId)
    {
        var machineDirectory = AtariStateStoreFunctions.GetMachineDirectory(_stateRoot, configurationId);
        if (!Directory.Exists(machineDirectory)) return [];
        return Directory.EnumerateFiles(machineDirectory,
                AtariStateStoreConstants.MetadataSearchPattern, SearchOption.TopDirectoryOnly)
            .Select(AtariStateStoreFunctions.ReadMetadata)
            .OrderByDescending(metadata => metadata.CreatedAtUtc)
            .ThenBy(metadata => metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public bool DeleteMachineStates(Guid configurationId, bool confirmed)
    {
        if (!confirmed) return false;
        var machineDirectory = AtariStateStoreFunctions.GetMachineDirectory(_stateRoot, configurationId);
        if (!Directory.Exists(machineDirectory)) return false;
        Directory.Delete(machineDirectory, recursive: true);
        return true;
    }

    private async ValueTask<AtariStoredStateMetadata> SaveAsync(IAtariMachine machine, string name,
        AtariStoredStateKind kind, byte[]? capture, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(machine);
        cancellationToken.ThrowIfCancellationRequested();
        var machineDirectory = AtariStateStoreFunctions.GetMachineDirectory(_stateRoot, machine.Configuration.Id);
        var stem = AtariStateStoreFunctions.GetFileStem(kind, name);
        var statePath = AtariStateStoreFunctions.StatePath(machineDirectory, stem);
        await machine.SaveStateAsync(statePath, cancellationToken).ConfigureAwait(false);
        var state = AtariStateFileFunctions.Read(statePath);
        string? captureFileName = null;
        if (capture is { Length: > AtariStateConstants.EmptyLength })
        {
            var capturePath = AtariStateStoreFunctions.CapturePath(machineDirectory, stem);
            AtariStateStoreFunctions.WriteBytesAtomically(capturePath, capture);
            captureFileName = Path.GetFileName(capturePath);
        }
        else
        {
            var capturePath = AtariStateStoreFunctions.CapturePath(machineDirectory, stem);
            if (File.Exists(capturePath)) File.Delete(capturePath);
        }
        var metadata = new AtariStoredStateMetadata(name, kind, DateTimeOffset.UtcNow,
            Path.GetFileName(statePath), captureFileName, state.Header.Core, state.Header.CoreName,
            state.Header.CoreVersion, state.Header.Model, state.Header.ConfigurationSha256,
            state.Header.ContentSha256);
        AtariStateStoreFunctions.WriteMetadataAtomically(
            AtariStateStoreFunctions.MetadataPath(machineDirectory, stem), metadata);
        return metadata;
    }
}
