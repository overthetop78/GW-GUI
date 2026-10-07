using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly CoreDefinition _definition;
    private ExternalCoreLibrary? _library;
    private ExternalHostCallbacks? _host;
    private ExternalCoreApi.VoidCall? _deinitialize;
    private ExternalCoreApi.VoidCall? _unloadGame;
    private ExternalCoreApi.VoidCall? _run;
    private ExternalCoreApi.VoidCall? _reset;
    private ExternalCoreApi.GetSerializedSize? _getSerializedSize;
    private ExternalCoreApi.Serialize? _serialize;
    private ExternalCoreApi.Serialize? _unserialize;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;
    private nint _contentBuffer;
    private bool _gameLoaded;
    private bool _initialized;

    internal ExternalCore(string corePath, CoreDefinition definition)
    {
        _corePath = corePath;
        _definition = definition;
    }

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public IReadOnlyList<CoreOption> Options => _host?.OptionCatalog ?? [];
    public IReadOnlyList<string> Diagnostics => _host?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _host?.LedStates
        ?? new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond => _host?.FramesPerSecond ?? VideoConstants.DefaultFramesPerSecond;
    public int SampleRate => _host?.SampleRate ?? AudioConstants.DefaultSampleRate;
    public int DiskCount => _host?.DiskControl.ImageCount ?? BufferConstants.EmptyCollectionCount;
    public int CurrentDiskIndex => _host?.DiskControl.CurrentIndex ?? ProcessCoreConstants.NoSelectedDiskIndex;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        if (_host is not null) return _host.TryDequeueAudio(out chunk);
        chunk = null;
        return false;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory,
        string? saveDirectory = null)
    {
        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(CommonExceptions.MediaNotFound(), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = Convert.ToHexString(SHA256.HashData(stream));

        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        saveDirectory = Path.GetFullPath(saveDirectory
            ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, Path.GetFileName(_corePath));
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = PrepareContentPath(media, contentDirectory);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory,
            configuration.Options ?? new Dictionary<string, string>());

        try
        {
            _library = new ExternalCoreLibrary(isolatedCorePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(
                ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(CommonExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var info);
            var libraryName = Marshal.PtrToStringUTF8(info.LibraryName);
            if (!string.Equals(libraryName, _definition.LibraryName,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(CommonExceptions.LibraryIdentityMismatch(libraryName,
                    _definition.LibraryName));
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = (Marshal.PtrToStringUTF8(info.ValidExtensions) ?? string.Empty)
                .Split(MediaConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            ValidateExtension(contentPath);

            Export<ExternalCoreApi.SetEnvironment>(ExternalCoreConstants.RetroSetEnvironment)(_host.Environment);
            Export<ExternalCoreApi.SetVideo>(ExternalCoreConstants.RetroSetVideoRefresh)(_host.Video);
            Export<ExternalCoreApi.SetAudioSample>(ExternalCoreConstants.RetroSetAudioSample)(_host.AudioSample);
            Export<ExternalCoreApi.SetAudioBatch>(ExternalCoreConstants.RetroSetAudioSampleBatch)(_host.AudioBatch);
            Export<ExternalCoreApi.SetInputPoll>(ExternalCoreConstants.RetroSetInputPoll)(_host.InputPoll);
            Export<ExternalCoreApi.SetInputState>(ExternalCoreConstants.RetroSetInputState)(_host.InputState);
            _deinitialize = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroDeinit);
            _unloadGame = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroUnloadGame);
            _run = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroRun);
            _reset = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroReset);
            _getSerializedSize = Export<ExternalCoreApi.GetSerializedSize>(ExternalCoreConstants.RetroSerializeSize);
            _serialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroSerialize);
            _unserialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroUnserialize);
            Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroInit)();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(
                ExternalCoreConstants.RetroSetControllerPortDevice);
            for (var port = BufferConstants.FirstCollectionIndex; port < ModelCatalog.Get(configuration.Model).ControllerPortCount; port++)
                setController((uint)port, ExternalCoreConstants.JoypadDevice);

            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(CommonExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(nint.Zero);
            }
            else _gameLoaded = LoadGame(loadGame, contentPath, info.NeedFullPath);
            if (!_gameLoaded) throw new InvalidOperationException(CommonExceptions.ContentRefused());
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(
        MachineConfiguration configuration) => (configuration.Media ?? [])
        .Where(item => item.IsInserted).OrderBy(item => item.MountOrder).ToArray();

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media,
        string contentDirectory)
    {
        if (media.Count == BufferConstants.EmptyCollectionCount) return null;
        if (media.Count == BufferConstants.SingleItemCount || media.Any(item => item.Category != MediaCategory.Floppy))
            return Path.GetFullPath(media[BufferConstants.FirstCollectionIndex].Path);
        if (media.Count > ExternalCoreConstants.MaximumPlaylistEntries)
            throw new ArgumentOutOfRangeException(nameof(media),
                CommonExceptions.PlaylistLimitExceeded());
        var playlist = Path.Combine(contentDirectory, ExternalCoreConstants.PlaylistName);
        File.WriteAllLines(playlist, media.Select(item => Path.GetFullPath(item.Path)),
            new System.Text.UTF8Encoding(false));
        return playlist;
    }

    public void RunFrame() => (_run
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))();
    public void HardReset()
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized());
        var sessionDirectory = _sessionDirectory
            ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized());
        var saveDirectory = _saveDirectory;
        Dispose();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => (_reset
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertMedia(string path) => (_host
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))
        .DiskControl.Insert(path);
    public void EjectMedia() => (_host
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))
        .DiskControl.Select(index);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize
            ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(CommonExceptions.InvalidStateSize(size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(CommonExceptions.StateSaveFailed());
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, state.Length);
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException(CommonExceptions.StateEmpty(), nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(CommonExceptions.StateRestoreFailed());
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void SetOption(string key, string value) => (_host
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotInitialized()))
        .SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private void ValidateExtension(string? path)
    {
        if (path is null) return;
        var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
        if (!SupportedContentExtensions.Contains(extension))
            throw new InvalidDataException(CommonExceptions.UnsupportedContentExtension(extension));
    }

    private bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path, bool needFullPath)
    {
        using var nativePath = new ExternalCoreUtf8String(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            var bytes = needFullPath ? null : File.ReadAllBytes(path);
            if (bytes is not null)
            {
                _contentBuffer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, _contentBuffer, bytes.Length);
            }
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
                { Path = nativePath.Pointer, Data = _contentBuffer, Size = (nuint)(bytes?.Length ?? BufferConstants.EmptyCollectionCount) },
                game, false);
            return loadGame(game);
        }
        finally
        {
            Marshal.FreeHGlobal(game);
        }
    }

    private T Export<T>(string name) where T : Delegate => (_library
        ?? throw new InvalidOperationException(CommonExceptions.CoreNotLoaded())).Resolve<T>(name);

    private static string ResolveCorePath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException(CommonExceptions.CorePathNotAbsolute(), nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException(CommonExceptions.CoreNotFound(), path);
        return path;
    }

    public void Dispose()
    {
        try { Stop(); }
        finally
        {
            try
            {
                if (_initialized) _deinitialize?.Invoke();
            }
            finally
            {
                _initialized = false;
                _deinitialize = null;
                _unloadGame = null;
                _run = null;
                _reset = null;
                _getSerializedSize = null;
                _serialize = null;
                _unserialize = null;
                try { _host?.Dispose(); }
                finally
                {
                    _host = null;
                    try { _library?.Dispose(); }
                    finally
                    {
                        _library = null;
                        if (_contentBuffer != nint.Zero) Marshal.FreeHGlobal(_contentBuffer);
                        _contentBuffer = nint.Zero;
                    }
                }
            }
        }
    }
}
