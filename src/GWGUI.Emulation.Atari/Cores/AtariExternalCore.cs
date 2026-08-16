using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariExternalCore : IAtariCore
{
    private readonly string _corePath;
    private AtariExternalCoreInfo _info;
    private ExternalCoreLibrary? _library;
    private AtariExternalCoreExports? _exports;
    private AtariExternalHostCallbacks? _callbacks;
    private AtariLoadedContent? _content;
    private bool _nativeInitialized;
    private bool _gameLoaded;
    private bool _supportsSaveStates;
    private bool _disposed;
    private readonly List<AtariMediaConfiguration> _mountedMedia = [];
    private readonly List<AtariSessionMedia> _sessionMedia = [];
    private string? _sessionDirectory;
    private AtariHatariContent? _hatariContent;
    private AtariMachineConfiguration? _configuration;
    private AtariPreparedCartridge? _cartridge;
    private AtariPreparedJaguarCd? _jaguarCd;

    internal AtariExternalCore(string absoluteCorePath, AtariCoreKind kind)
    {
        _corePath = Path.GetFullPath(absoluteCorePath);
        Kind = kind;
        _info = AtariExternalCoreProbe.Inspect(absoluteCorePath, kind);
        CoreSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(_corePath))).ToLowerInvariant();
    }

    public AtariCoreKind Kind { get; }
    public VideoFrame? LatestVideoFrame => _callbacks?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _callbacks?.LatestAudioChunk;
    public IReadOnlyList<AtariCoreOption> Options => _callbacks?.Options ?? [];
    public IReadOnlyList<string> Diagnostics => _callbacks?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _callbacks?.LedStates ?? new Dictionary<int, bool>();
    public string CoreName => _info.LibraryName;
    public string CoreVersion => _info.LibraryVersion;
    public string CoreSha256 { get; }
    public IReadOnlySet<string> SupportedContentExtensions => _info.Extensions;
    public bool SupportsSaveStates => _supportsSaveStates;
    public double FramesPerSecond => _callbacks?.FramesPerSecond ?? default;
    public int SampleRate => _callbacks?.SampleRate ?? default;
    public AtariRuntimeRegion? Region { get; private set; }
    public int BufferedAudioFrames => _callbacks?.BufferedAudioFrames ?? default;
    public long AudioOverrunCount => _callbacks?.AudioOverrunCount ?? default;
    public long AudioUnderrunCount => _callbacks?.AudioUnderrunCount ?? default;
    public AtariHostProcessState HostProcessState => AtariHostProcessState.InProcess;
    public int? HostProcessId => null;
    internal IReadOnlyList<AtariMediaConfiguration> MountedMedia => _mountedMedia;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        chunk = null;
        return _callbacks?.TryDequeueAudio(out chunk) == true;
    }

    public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_library is not null)
            throw new InvalidOperationException(AtariErrorMessages.CoreAlreadyInitialized);

        try
        {
            var absoluteSession = Path.GetFullPath(sessionDirectory);
            _configuration = configuration;
            _sessionDirectory = absoluteSession;
            var systemDirectory = Path.Combine(absoluteSession, AtariConstants.SystemDirectoryName);
            var media = configuration.Media.Where(item => item.IsInserted)
                .OrderBy(item => item.MountOrder)
                .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            AtariSessionMedia? preparedMedia = null;
            Atari800PreparedMedia? atari800Media = null;
            _library = new ExternalCoreLibrary(_corePath);
            _exports = AtariCoreFunctions.ResolveExports(_library);
            _callbacks = new AtariExternalHostCallbacks(
                systemDirectory,
                Path.Combine(absoluteSession, AtariConstants.ContentDirectoryName),
                saveDirectory ?? Path.Combine(absoluteSession, AtariConstants.SavesDirectoryName),
                Path.Combine(absoluteSession, AtariConstants.AssetsDirectoryName),
                configuration.Options);
            AtariCoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;
            _info = AtariCoreFunctions.ReadInitializedInfo(_exports, Kind);
            _callbacks.ValidateConfiguredOptions();

            AtariFirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory);
            if (Kind == AtariCoreKind.Hatari)
            {
                _hatariContent = AtariHatariContentFunctions.Prepare(configuration, absoluteSession, _info.Extensions);
                media = _hatariContent?.Configuration;
            }
            else if (Kind == AtariCoreKind.Atari800 && media is not null)
            {
                atari800Media = Atari800MediaFunctions.Prepare(configuration, media, absoluteSession,
                    _info.Extensions);
                preparedMedia = atari800Media.SessionMedia;
            }
            else if (configuration.Model == AtariMachineModel.JaguarCd
                     && media?.Kind == AtariMediaKind.CompactDisc)
            {
                _jaguarCd = AtariJaguarCdFunctions.Prepare(configuration, media,
                    _info.NeedsFullPath, _info.Extensions);
            }
            else if (AtariCartridgeFunctions.Supports(Kind) && media is not null)
            {
                _cartridge = AtariCartridgeFunctions.Prepare(configuration, media, Kind,
                    _info.NeedsFullPath, _info.Extensions);
                AtariCartridgeFunctions.ValidateNoUnsupportedMetadata(media);
            }
            else if (media is not null)
            {
                preparedMedia = AtariSessionMediaFunctions.Prepare(media, absoluteSession, _info.Extensions);
            }
            IReadOnlyDictionary<string, string> runtimeOptions = Kind == AtariCoreKind.Atari800
                ? Atari800MediaFunctions.ApplyOptions(configuration, atari800Media)
                : AtariHatariStorageFunctions.ApplyWriteProtection(configuration.Options, _hatariContent?.Storage);
            if (_cartridge is not null)
                runtimeOptions = AtariCartridgeFunctions.ApplyOptions(runtimeOptions, _cartridge.Configuration, Kind);
            foreach (var option in runtimeOptions)
                if (!configuration.Options.TryGetValue(option.Key, out var configuredValue)
                    || !string.Equals(configuredValue, option.Value, StringComparison.Ordinal))
                    _callbacks.SetOption(option.Key, option.Value);

            if (media is not null)
            {
                var runtimePath = _hatariContent?.RuntimePath ?? atari800Media?.RuntimePath ??
                    _cartridge?.RuntimePath ?? _jaguarCd?.RuntimePath ?? preparedMedia!.RuntimePath;
                _content = AtariContentFunctions.Create(runtimePath,
                    _jaguarCd?.NeedsFullPath ?? _info.NeedsFullPath, _info.Extensions);
            }
            AtariCoreLifecycleFunctions.Load(_exports, _callbacks, configuration,
                _content?.GameInfo ?? nint.Zero);
            _gameLoaded = true;
            _supportsSaveStates = AtariStateFunctions.IsAvailable(_exports);
            Region = AtariRuntimeFunctions.Region(_exports.GetRegion());
            if (atari800Media?.ContentType is Atari800ContentType.Floppy or Atari800ContentType.Cassette &&
                !_callbacks.DiskControl.IsAvailable)
                throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                    Atari800MediaErrors.MediaControlRequired);
            if (media is not null)
            {
                AtariMediaRuntimeFunctions.Register(_mountedMedia, media);
                if (_hatariContent?.SessionMedia is { } hatariSessionMedia)
                    _sessionMedia.Add(hatariSessionMedia);
                else if (preparedMedia is not null)
                    _sessionMedia.Add(preparedMedia);
            }
        }
        catch
        {
            DisposeNativeResources();
            throw;
        }
    }

    public void RunFrame() => RequireExports().Run();
    public void HardReset() => RequireExports().Reset();
    public void SetInput(EmulationInputSnapshot snapshot) => RequireCallbacks().Input = snapshot;
    public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);
        AtariControllerPortFunctions.ConfigurePort(RequireExports(), RequireCallbacks(), configuration, port, peripheral);
    }
    public void SetOption(string key, string value) => RequireCallbacks().SetOption(key, value);

    public void InsertMedia(AtariMediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);
        AtariJaguarCdFunctions.RejectForStandardJaguar(configuration.Model, media);
        if (media.Kind == AtariMediaKind.CompactDisc)
        {
            ReplaceJaguarCd(media);
            return;
        }
        if (AtariCartridgeFunctions.Supports(Kind))
        {
            ReplaceCartridge(media);
            return;
        }
        if (Kind == AtariCoreKind.Atari800 && media.Kind == AtariMediaKind.Cartridge)
            throw new NotSupportedException(Atari800MediaErrors.DynamicCartridgeUnsupported);
        if ((Kind != AtariCoreKind.Hatari && Kind != AtariCoreKind.Atari800) ||
            media.Kind is not (AtariMediaKind.Floppy or AtariMediaKind.Cassette))
            throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        var sessionDirectory = _sessionDirectory ??
            throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);
        AtariSessionMedia preparedMedia;
        if (Kind == AtariCoreKind.Atari800)
        {
            var prepared = Atari800MediaFunctions.Prepare(
                _configuration ?? throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized),
                media, sessionDirectory, _info.Extensions);
            preparedMedia = prepared.SessionMedia ??
                throw new NotSupportedException(Atari800MediaErrors.DynamicCartridgeUnsupported);
        }
        else
        {
            preparedMedia = AtariSessionMediaFunctions.Prepare(media, sessionDirectory, _info.Extensions);
        }
        RequireCallbacks().DiskControl.Insert(preparedMedia.RuntimePath);
        AtariMediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
        _sessionMedia.Add(preparedMedia);
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (slot == EmulationMediaSlot.Cd0 && Kind == AtariCoreKind.VirtualJaguar)
            throw new NotSupportedException(AtariJaguarCdErrors.EjectionUnsupported);
        if (AtariCartridgeFunctions.Supports(Kind))
            throw new NotSupportedException(AtariCartridgeErrors.EjectionUnsupported);
        if (Kind != AtariCoreKind.Hatari && Kind != AtariCoreKind.Atari800)
            throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        RequireCallbacks().DiskControl.Eject();
        AtariMediaRuntimeFunctions.MarkEjected(_mountedMedia, slot);
    }

    public void SelectDisk(int index)
    {
        if (Kind != AtariCoreKind.Hatari && Kind != AtariCoreKind.Atari800)
            throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        RequireCallbacks().DiskControl.Select(index);
    }

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        var media = _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot) ??
            throw new InvalidOperationException(AtariSessionMediaErrors.ExplicitSaveRequired);
        AtariSessionMediaFunctions.Save(media);
    }

    public AtariDiskStatus GetDiskStatus()
    {
        if (Kind != AtariCoreKind.Hatari && Kind != AtariCoreKind.Atari800)
            throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        return RequireCallbacks().DiskControl.GetStatus();
    }

    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) =>
        _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot)?.RequiresExplicitSave == true;

    public byte[] SaveState()
    {
        var exports = RequireExports();
        var size = exports.GetSerializedSize();
        if (size == nuint.Zero || size > AtariConstants.MaximumStateSize)
            throw new AtariEmulationException(AtariErrorKind.State, AtariErrorCode.StateInvalid,
                AtariErrorMessages.StateSizeInvalid);
        var state = GC.AllocateUninitializedArray<byte>(checked((int)size));
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!exports.Serialize(buffer, size))
                throw new AtariEmulationException(AtariErrorKind.State, AtariErrorCode.StateInvalid,
                    AtariErrorMessages.StateSaveFailed);
            Marshal.Copy(buffer, state, AtariConstants.FirstBufferIndex, state.Length);
            return state;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty || state.Length > AtariConstants.MaximumStateSize)
            throw new AtariEmulationException(AtariErrorKind.State, AtariErrorCode.StateInvalid,
                AtariErrorMessages.StateSizeInvalid);
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, AtariConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!RequireExports().Unserialize(buffer, (nuint)bytes.Length))
                throw new AtariEmulationException(AtariErrorKind.State, AtariErrorCode.StateIncompatible,
                    AtariErrorMessages.StateLoadFailed);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Stop()
    {
        if (_gameLoaded && _exports is not null)
        {
            _exports.UnloadGame();
            _gameLoaded = false;
        }
        _supportsSaveStates = false;
        _content?.Dispose();
        _content = null;
        AtariHatariContentFunctions.Cleanup(_hatariContent);
        _hatariContent = null;
        _cartridge = null;
        _jaguarCd = null;
        _mountedMedia.Clear();
        _sessionMedia.Clear();
        _configuration = null;
    }

    private void ReplaceCartridge(AtariMediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);
        var exports = RequireExports();
        var prepared = AtariCartridgeFunctions.Prepare(configuration, media, Kind,
            _info.NeedsFullPath, _info.Extensions);
        AtariCartridgeFunctions.ValidateNoUnsupportedMetadata(media);
        foreach (var option in AtariCartridgeFunctions.GetMediaOptions(media, Kind))
            RequireCallbacks().SetOption(option.Key, option.Value);
        var candidate = AtariContentFunctions.Create(prepared.RuntimePath,
            prepared.NeedsFullPath, _info.Extensions);
        var previousContent = _content;
        if (_gameLoaded)
        {
            exports.UnloadGame();
            _gameLoaded = false;
        }

        if (!exports.LoadGame(candidate.GameInfo))
        {
            candidate.Dispose();
            if (previousContent is null || !exports.LoadGame(previousContent.GameInfo))
                throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                    AtariCartridgeErrors.RollbackFailed);
            _gameLoaded = true;
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                AtariCartridgeErrors.ReplacementFailed);
        }

        _gameLoaded = true;
        _supportsSaveStates = AtariStateFunctions.IsAvailable(exports);
        previousContent?.Dispose();
        _content = candidate;
        _cartridge = prepared;
        _jaguarCd = null;
        AtariMediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
    }

    private void ReplaceJaguarCd(AtariMediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);
        var prepared = AtariJaguarCdFunctions.Prepare(configuration, media,
            _info.NeedsFullPath, _info.Extensions);
        var candidate = AtariContentFunctions.Create(prepared.RuntimePath,
            prepared.NeedsFullPath, _info.Extensions);
        var exports = RequireExports();
        var previousContent = _content;
        if (_gameLoaded)
        {
            exports.UnloadGame();
            _gameLoaded = false;
        }
        if (!exports.LoadGame(candidate.GameInfo))
        {
            candidate.Dispose();
            if (previousContent is not null && exports.LoadGame(previousContent.GameInfo))
                _gameLoaded = true;
            throw AtariJaguarCdFunctions.Unsupported(AtariErrorMessages.ContentLoadFailed);
        }
        _gameLoaded = true;
        _supportsSaveStates = AtariStateFunctions.IsAvailable(exports);
        previousContent?.Dispose();
        _content = candidate;
        _jaguarCd = prepared;
        _cartridge = null;
        AtariMediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
    }

    private AtariExternalCoreExports RequireExports() => _exports ??
        throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);

    private AtariExternalHostCallbacks RequireCallbacks() => _callbacks ??
        throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);

    private void DisposeNativeResources()
    {
        var exports = _exports;
        var callbacks = _callbacks;
        var library = _library;
        AtariCoreLifecycleFunctions.Cleanup(exports, _gameLoaded, _nativeInitialized,
            () => callbacks?.Dispose(), () => library?.Dispose());
        _gameLoaded = false;
        _supportsSaveStates = false;
        _nativeInitialized = false;
        _content?.Dispose();
        _content = null;
        AtariHatariContentFunctions.Cleanup(_hatariContent);
        _hatariContent = null;
        _cartridge = null;
        _jaguarCd = null;
        _mountedMedia.Clear();
        _sessionMedia.Clear();
        _configuration = null;
        _callbacks = null;
        _exports = null;
        _library = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeNativeResources();
    }
}
