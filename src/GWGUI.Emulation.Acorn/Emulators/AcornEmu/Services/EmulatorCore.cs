using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using NativeModelCatalog = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.ModelCatalog;
using BbcAudioConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.AudioConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Adaptation du matériel AcornEmu au contrat de cœur déjà possédé par la session Common.</summary>
/// <remarks>Le cœur reste sur le thread de la machine. Il ne crée ni fenêtre, ni processus, ni thread.
/// Les ROM sont lues depuis les chemins choisis par l'utilisateur ; aucune image n'est intégrée à l'assembly.</remarks>
internal sealed class EmulatorCore : IEmulatorCore
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
    public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool>();
    public IReadOnlySet<string> SupportedContentExtensions { get; } = new HashSet<string>();

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (configuration.Model != _definition.Model) throw new ArgumentOutOfRangeException(nameof(configuration));
        _machine = _definition.Create(configuration);
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
        LatestVideoFrame = new VideoFrame(image, _definition.Width, _definition.Height, _definition.Width * _definition.BytesPerPixel, _definition.PixelFormat, _definition.AspectRatio, ++_sequence, TimeSpan.FromSeconds(_sequence / FramesPerSecond));
        var samples = machine.DrainAudio();
        if (samples.Length != 0)
        {
            LatestAudioChunk = new AudioChunk(samples, SampleRate, samples.Length, ++_audioSequence, TimeSpan.FromSeconds((double)_audioFrames / SampleRate), EmulationAudioChannelLayout.Mono);
            _audioFrames += samples.Length;
            _audio.Enqueue(LatestAudioChunk);
        }
    }
    public void SetInput(EmulationInputSnapshot snapshot) => _machine?.SetInput(snapshot);
    public void SoftReset() { _machine?.Reset(false); ClearAudio(); }
    public void HardReset() { _machine?.Reset(true); ClearAudio(); }
    public void Stop() { ShutdownRequested = true; ClearAudio(); }
    /// <summary>Transfère un bloc mono possédé au parcours audio Common déjà utilisé par les autres cœurs.</summary>
    public bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeue(out chunk);
    /// <summary>Détache les blocs non lus pour ne pas rejouer d'ancien son après arrêt ou reset.</summary>
    private void ClearAudio() { _audio.Clear(); LatestAudioChunk = null; }
    public DiskStatus GetDiskStatus() => new(0, -1, true, []);
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    public void SaveMediaChanges(EmulationMediaSlot slot) { }
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => throw new NotSupportedException();
    public void InsertMedia(MediaConfiguration media) => throw new NotSupportedException();
    public void EjectMedia(EmulationMediaSlot slot) => throw new NotSupportedException();
    public void SelectDisk(int index) => throw new NotSupportedException();
    public byte[] SaveState() => throw new NotSupportedException();
    public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
    public void SetOption(string key, string value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var machine = _machine ?? throw new InvalidOperationException();
        machine.SetOption(key, value);
        _videoOutput = value;
    }

    /// <summary>Détache les données possédées ; aucune ressource empruntée à l'hôte n'est fermée.</summary>
    public void Dispose()
    {
        try { ShutdownRequested = true; }
        finally { _machine = null; LatestVideoFrame = null; ClearAudio(); _disposed = true; }
    }
}
