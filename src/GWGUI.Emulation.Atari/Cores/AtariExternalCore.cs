using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariExternalCore : IAtariCore
{
    private readonly string _corePath;
    private readonly AtariExternalCoreInfo _info;
    private ExternalCoreLibrary? _library;
    private AtariExternalCoreExports? _exports;
    private AtariExternalHostCallbacks? _callbacks;
    private AtariLoadedContent? _content;
    private bool _nativeInitialized;
    private bool _gameLoaded;
    private bool _disposed;
    private readonly List<AtariMediaConfiguration> _mountedMedia = [];
    private readonly List<AtariSessionMedia> _sessionMedia = [];
    private string? _sessionDirectory;

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
    public double FramesPerSecond => _callbacks?.FramesPerSecond ?? default;
    public int SampleRate => _callbacks?.SampleRate ?? default;
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
            _sessionDirectory = absoluteSession;
            var systemDirectory = Path.Combine(absoluteSession, AtariConstants.SystemDirectoryName);
            AtariFirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory);
            _library = new ExternalCoreLibrary(_corePath);
            _exports = AtariCoreFunctions.ResolveExports(_library);
            _callbacks = new AtariExternalHostCallbacks(
                systemDirectory,
                Path.Combine(absoluteSession, AtariConstants.ContentDirectoryName),
                saveDirectory ?? Path.Combine(absoluteSession, AtariConstants.SavesDirectoryName),
                configuration.Options);
            AtariCoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;

            var media = configuration.Media.FirstOrDefault(item => item.IsInserted);
            AtariSessionMedia? preparedMedia = null;
            if (media is not null)
            {
                preparedMedia = AtariSessionMediaFunctions.Prepare(media, absoluteSession, _info.Extensions);
                _content = AtariContentFunctions.Create(preparedMedia.RuntimePath,
                    _info.NeedsFullPath, _info.Extensions);
            }
            if (!_exports.LoadGame(_content?.GameInfo ?? nint.Zero))
                throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                    AtariErrorMessages.ContentLoadFailed);
            _gameLoaded = true;
            if (media is not null && preparedMedia is not null)
            {
                AtariMediaRuntimeFunctions.Register(_mountedMedia, media);
                _sessionMedia.Add(preparedMedia);
            }
            _exports.GetSystemAvInfo(out var avInfo);
            _callbacks.ApplySystemAvInfo(avInfo);
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
    public void SetOption(string key, string value) => RequireCallbacks().SetOption(key, value);

    public void InsertMedia(AtariMediaConfiguration media)
    {
        if (Kind != AtariCoreKind.Hatari || media.Kind != AtariMediaKind.Floppy)
            throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        var preparedMedia = AtariSessionMediaFunctions.Prepare(media,
            _sessionDirectory ?? throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized),
            _info.Extensions);
        RequireCallbacks().DiskControl.Insert(preparedMedia.RuntimePath);
        AtariMediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
        _sessionMedia.Add(preparedMedia);
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (Kind != AtariCoreKind.Hatari) throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
        RequireCallbacks().DiskControl.Eject();
        AtariMediaRuntimeFunctions.MarkEjected(_mountedMedia, slot);
    }

    public void SelectDisk(int index)
    {
        if (Kind != AtariCoreKind.Hatari) throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
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
        if (Kind != AtariCoreKind.Hatari) throw new NotSupportedException(AtariErrorMessages.HatariFloppyRequired);
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
        _content?.Dispose();
        _content = null;
        _mountedMedia.Clear();
        _sessionMedia.Clear();
    }

    private AtariExternalCoreExports RequireExports() => _exports ??
        throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);

    private AtariExternalHostCallbacks RequireCallbacks() => _callbacks ??
        throw new InvalidOperationException(AtariErrorMessages.CoreNotInitialized);

    private void DisposeNativeResources()
    {
        Stop();
        if (_nativeInitialized && _exports is not null)
        {
            _exports.Deinitialize();
            _nativeInitialized = false;
        }
        _callbacks?.Dispose();
        _callbacks = null;
        _exports = null;
        _library?.Dispose();
        _library = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeNativeResources();
    }
}
