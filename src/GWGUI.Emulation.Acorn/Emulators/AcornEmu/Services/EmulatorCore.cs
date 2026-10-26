using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using NativeModelCatalog = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.ModelCatalog;
using BbcAudioConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.AudioConstants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using CassetteConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.CassetteConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Adaptation du matériel AcornEmu au contrat de cœur déjà possédé par la session Common.</summary>
/// <remarks>Le cœur reste sur le thread de la machine. Il ne crée ni fenêtre, ni processus, ni thread.
/// Les ROM sont lues depuis les chemins choisis par l'utilisateur ; aucune image n'est intégrée à l'assembly.</remarks>
internal sealed partial class EmulatorCore : IEmulatorCore, IEmulatorCassetteTransport
{
    private readonly AcornEmuModelDefinition _definition;
    private IAcornMachine? _machine;

    /// <summary>Sélectionne le constructeur et les sorties du modèle sans charger de ROM ni créer de session.</summary>
    internal EmulatorCore(MachineModel model) => _definition = NativeModelCatalog.Get(model);
    private bool _disposed;
    private long _sequence;
    private long _audioSequence, _audioFrames;
    private readonly Queue<AudioChunk> _audio = new();
    private string _videoOutput = EmulatorConstants.UhfOutput;
    private string? _cassettePath;
    private EmulationCassetteSettings _cassetteSettings = new();
    private EmulationCassettePcmSettings _cassettePcm = new();
    private IReadOnlyDictionary<string, string>? _mountedCassetteParameters;
    private Dictionary<string, string> _runtimeOptions = new(StringComparer.Ordinal);
    private string _mechanicalOperation = S.None;
    private string? _mechanicalEvent;
    private long _mechanicalOffset;
    public Emulator Emulator => Emulator.AcornEmu;
    public string CoreName => EmulatorConstants.Name;
    public string CoreVersion => EmulatorConstants.Version;
    public string CoreSha256 { get; } = CommonCoreHostFunctions.StateFiles.HashBytes(File.ReadAllBytes(typeof(EmulatorCore).Assembly.Location)).ToLowerInvariant();
    public double FramesPerSecond => _definition.FramesPerSecond;
    public int SampleRate => BbcAudioConstants.SampleRate;
    public bool SupportsSaveStates => false;
    public bool SupportsPointerCapture => false;
    public bool ShutdownRequested { get; private set; }
    public EmulationVideoStandard? Region => _definition.Region;
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    public int? HostProcessId => null;
    public int BufferedAudioFrames => _audio.Sum(chunk => chunk.FrameCount);
    public int MinimumAudioLatencyMilliseconds => 0;
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) { }
    public long AudioOverrunCount => 0;
    public long AudioUnderrunCount => 0;
    public int DiskCount => 0;
    public int CurrentDiskIndex => -1;
    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk { get; private set; }
    public IReadOnlyList<CoreOption> Options =>
    [
        new(EmulatorConstants.VideoOutput, "Emulation.Acorn.Video.Output", null, null, EmulatorConstants.UhfOutput, _videoOutput,
            [new(EmulatorConstants.UhfOutput, "UHF PAL", "Emulation.Acorn.Video.Output.Uhf"), new(EmulatorConstants.MonochromeOutput, "Composite", "Emulation.Acorn.Video.Output.Monochrome"), new(EmulatorConstants.ColourCompositeOutput, "Composite", "Emulation.Acorn.Video.Output.ColourComposite"), new(EmulatorConstants.RgbOutput, "RGB", "Emulation.Acorn.Video.Output.Rgb")], ResourceKey: "Emulation.Acorn.Video.Output", Tab: EmulationMachineTab.Video)
    ];
    public IReadOnlyList<string> Diagnostics => [];
    public IReadOnlyDictionary<int, bool> LedStates
    {
        get
        {
            var states = new Dictionary<int, bool> { [1] = CassetteActive, [0] = _machine?.FloppyActive(0) == true, [StorageConstants.SecondDriveLed] = _machine?.FloppyActive(1) == true };
            for (var unit = 0; unit < StorageConstants.HardDiskCount; unit++) states[StorageConstants.HardDiskLedStart + unit] = _machine?.HardDiskActive(unit) == true;
            return states;
        }
    }
    public IReadOnlySet<string> SupportedContentExtensions { get; } = new HashSet<string>(CassetteConstants.Extensions.Concat(StorageConstants.DoubleDensityExtensions).Concat(StorageConstants.HardDiskExtensions), StringComparer.OrdinalIgnoreCase);
    public EmulationCassetteState CassetteState => _machine?.CassetteTransport.CassetteState ?? EmulationCassetteState.Empty;
    public bool CassetteActive => _machine?.CassetteTransport.CassetteActive ?? false;
    public EmulationCassetteCommand? CassetteOperation => _machine?.CassetteTransport.CassetteOperation;
    public IReadOnlySet<EmulationCassetteCommand> CassetteCommands => _machine is null ? new HashSet<EmulationCassetteCommand>() : new HashSet<EmulationCassetteCommand>(_machine.CassetteTransport.CassetteCommands.Concat(_cassetteSettings.WriteProtected ? [] : new[] { EmulationCassetteCommand.Record }));
    public void ExecuteCassette(EmulationCassetteCommand command)
    {
        var machine = _machine ?? throw new InvalidOperationException();
        if (command != EmulationCassetteCommand.Record) SaveMediaChanges(EmulationMediaSlot.Cassette0);
        machine.CassetteTransport.ExecuteCassette(command);
        if (command == EmulationCassetteCommand.Stop) _mechanicalEvent = S.Stop;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (configuration.Model != _definition.Model) throw new ArgumentOutOfRangeException(nameof(configuration));
        _machine = _definition.Create(configuration);
        _cassetteSettings = EmulationCassetteSettings.Read(configuration.Options);
        _runtimeOptions = new(configuration.Options, StringComparer.Ordinal);
        _cassettePcm = EmulationCassettePcmSettings.Read(configuration.Options);
        _machine.ConfigureCassette(_cassetteSettings);
        ConfigureDisks(configuration);
        ConfigureHardDisks(configuration);
        ConfigureNetwork(configuration);
        foreach (var media in configuration.Media.Where(media => media.IsInserted)) InsertMedia(media);
        _videoOutput = configuration.Options.GetValueOrDefault(EmulatorConstants.VideoOutput, EmulatorConstants.UhfOutput);
        ShutdownRequested = false;
        _sequence = 0;
        _audioSequence = _audioFrames = 0;
        ClearAudio();
    }
    public void RunFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var machine = _machine ?? throw new InvalidOperationException();
        var image = machine.RunFrame();
        // Le matériel continue de tourner et de décoder chaque bit ; seule l'attente de présentation est supprimée.
        if (_cassetteSettings.Mode != EmulationCassettePlaybackMode.RealTime)
            for (var frame = 0; frame < 31 && CassetteActive; frame++) { machine.DrainAudio(); image = machine.RunFrame(); }
        // La coupure moteur du MOS termine un SAVE même lorsque le bouton REC reste armé sur le magnétophone.
        if (!CassetteActive && machine.CassetteHasChanges) SaveMediaChanges(EmulationMediaSlot.Cassette0);
        FlushStoppedDisks();
        FlushStoppedHardDisks();
        LatestVideoFrame = new VideoFrame(image, _definition.Width, _definition.Height, _definition.Width * _definition.BytesPerPixel, _definition.PixelFormat, _definition.AspectRatio, ++_sequence, TimeSpan.FromSeconds(_sequence / FramesPerSecond));
        var samples = machine.DrainAudio();
        var mixed = samples.Select(value => (double)value).ToArray();
        MixCassetteSound(mixed);
        MixFloppySound(mixed);
        MixHardDiskSound(mixed);
        samples = CommonCoreHostFunctions.MediaProcessing.ProtectMechanicalAudioMix(mixed);
        if (samples.Length != 0)
        {
            LatestAudioChunk = new AudioChunk(samples, SampleRate, samples.Length, ++_audioSequence, TimeSpan.FromSeconds((double)_audioFrames / SampleRate), EmulationAudioChannelLayout.Mono);
            _audioFrames += samples.Length;
            _audio.Enqueue(LatestAudioChunk);
        }
    }
    public void SetInput(EmulationInputSnapshot snapshot) => _machine?.SetInput(snapshot);
    public void SoftReset() { _network?.Reset(); _machine?.Reset(false); ClearAudio(); }
    public void HardReset() { _network?.Reset(); _machine?.Reset(true); ClearAudio(); }
    public void Stop() { try { SaveMediaChanges(EmulationMediaSlot.Cassette0); SaveDisks(); SaveHardDisks(); } finally { try { DisposeNetwork(); } finally { ShutdownRequested = true; ClearAudio(); } } }
    /// <summary>Transfère un bloc mono possédé au parcours audio Common déjà utilisé par les autres cœurs.</summary>
    public bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeue(out chunk);
    /// <summary>Détache les blocs non lus pour ne pas rejouer d'ancien son après arrêt ou reset.</summary>
    private void ClearAudio() { _audio.Clear(); LatestAudioChunk = null; }
    /// <summary>Mélange les seuls bruits de mécanisme à la présentation audio en temps réel, sans les enregistrer sur la bande.</summary>
    private void MixCassetteSound(double[] samples)
    {
        if (!_cassetteSettings.MechanicalSound || _cassetteSettings.Mode != EmulationCassettePlaybackMode.RealTime) return;
        var operation = _mechanicalEvent ?? (CassetteActive ? CassetteOperation?.ToString() : null) ?? S.None;
        if (operation == S.None && _mechanicalOperation is S.Play or S.Record or S.FastForward or S.Rewind) operation = _mechanicalEvent = S.Stop;
        if (operation != _mechanicalOperation) { _mechanicalOperation = operation; _mechanicalOffset = 0; }
        var noise = BbcMicroCassetteFunctions.Sound(operation, SampleRate, samples.Length, _mechanicalOffset, _cassetteSettings.MechanicalVolumePercent);
        for (var index = 0; index < samples.Length; index++) samples[index] += noise[index];
        _mechanicalOffset += samples.Length;
        if (_mechanicalOffset > SampleRate / 5) _mechanicalEvent = null;
    }
    public DiskStatus GetDiskStatus() => new(0, -1, true, []);
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => slot.Category switch
    {
        EmulationMediaCategory.FloppyDrive when slot.Index is >= 0 and < StorageConstants.DriveCount => _floppyImages[slot.Index]?.HasChanges == true,
        EmulationMediaCategory.HardDisk when slot.Index is >= 0 and < StorageConstants.HardDiskCount => _hardDisks[slot.Index]?.HasChanges == true,
        _ => slot == EmulationMediaSlot.Cassette0 && _machine?.CassetteHasChanges == true
    };
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (slot.Category == EmulationMediaCategory.FloppyDrive) { if (slot.Index is < 0 or >= 2) throw new ArgumentOutOfRangeException(nameof(slot)); SaveFloppy(slot.Index); return; }
        if (slot.Category == EmulationMediaCategory.HardDisk) { if (slot.Index is < 0 or >= StorageConstants.HardDiskCount) throw new ArgumentOutOfRangeException(nameof(slot)); SaveHardDisk(slot.Index); return; }
        if (!HasUnsavedMediaChanges(slot)) return;
        BbcMicroCassetteFunctions.Write(_cassettePath ?? throw new InvalidOperationException(), _machine!.RecordedCassette, _mountedCassetteParameters);
        _machine.MarkCassetteSaved();
        _machine.RefreshCassetteIndex(BbcMicroCassetteFunctions.Read(_cassettePath!, _mountedCassetteParameters));
    }
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => throw new NotSupportedException();
    /// <summary>Le chargement passe par Common et Emulation ; le cœur reçoit un signal, jamais un document MediaEngine.</summary>
    public void InsertMedia(MediaConfiguration media)
    {
        if (media.Slot.Category == EmulationMediaCategory.FloppyDrive) { InsertFloppy(media); return; }
        if (media.Slot.Category == EmulationMediaCategory.HardDisk) { InsertHardDisk(media); return; }
        if (media.Slot != EmulationMediaSlot.Cassette0 || media.Category != MediaCategory.Cassette || !SupportedContentExtensions.Contains(Path.GetExtension(media.Path))) throw new ArgumentException(nameof(media));
        var parameters = media.Parameters ?? _cassettePcm.ToParameters();
        var signal = BbcMicroCassetteFunctions.Read(media.Path, parameters);
        SaveMediaChanges(EmulationMediaSlot.Cassette0);
        (_machine ?? throw new InvalidOperationException()).InsertCassette(signal, !media.IsReadOnly && BbcMicroCassetteFunctions.RecordingExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase));
        _cassettePath = media.Path;
        _mountedCassetteParameters = parameters;
    }
    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (slot.Category == EmulationMediaCategory.FloppyDrive) { EjectFloppy(slot.Index); return; }
        if (slot.Category == EmulationMediaCategory.HardDisk) { EjectHardDisk(slot.Index); return; }
        if (slot != EmulationMediaSlot.Cassette0) throw new ArgumentOutOfRangeException(nameof(slot));
        SaveMediaChanges(slot);
        _machine?.EjectCassette(); _cassettePath = null; _mechanicalEvent = S.Eject;
    }
    public void SelectDisk(int index) => throw new NotSupportedException();
    public byte[] SaveState() => throw new NotSupportedException();
    public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
    public void SetOption(string key, string value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var machine = _machine ?? throw new InvalidOperationException();
        if (key == CassetteConstants.ConnectedOption)
        {
            var connected = bool.Parse(value);
            if (!connected) EjectMedia(EmulationMediaSlot.Cassette0);
            _runtimeOptions[key] = value;
            return;
        }
        if (key.StartsWith(EmulationCassetteSettings.Prefix, StringComparison.Ordinal))
        {
            var options = new Dictionary<string, string>(_runtimeOptions, StringComparer.Ordinal) { [key] = value };
            var settings = EmulationCassetteSettings.Read(options);
            var pcm = EmulationCassettePcmSettings.Read(options);
            if (settings.BaudRate is not (0 or 300 or 1200)) throw new ArgumentOutOfRangeException(nameof(value));
            SaveMediaChanges(EmulationMediaSlot.Cassette0);
            _runtimeOptions = options; _cassetteSettings = settings; _cassettePcm = pcm; machine.ConfigureCassette(settings);
            return;
        }
        if (ApplyMechanicalSoundOption(key, value)) return;
        machine.SetOption(key, value);
        _videoOutput = value;
    }

    /// <summary>Détache les données possédées ; aucune ressource empruntée à l'hôte n'est fermée.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        try { SaveMediaChanges(EmulationMediaSlot.Cassette0); SaveDisks(); SaveHardDisks(); ShutdownRequested = true; }
        finally
        {
            try { DisposeDisks(); }
            finally { try { DisposeHardDisks(); } finally { try { DisposeNetwork(); } finally { _machine = null; LatestVideoFrame = null; ClearAudio(); _disposed = true; } } }
        }
    }
}
