using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Adaptation du matériel AcornEmu au contrat de cœur déjà possédé par la session Common.</summary>
/// <remarks>Le cœur reste sur le thread de la machine. Il ne crée ni fenêtre, ni processus, ni thread.
/// Les ROM sont lues depuis les chemins choisis par l'utilisateur ; aucune image n'est intégrée à l'assembly.</remarks>
internal sealed class EmulatorCore : IEmulatorCore
{
    private ModelAMachine? _machine;
    private bool _disposed;
    private long _sequence;
    public Emulator Emulator => Emulator.AcornEmu;
    public string CoreName => EmulatorConstants.Name;
    public string CoreVersion => EmulatorConstants.Version;
    public string CoreSha256 { get; } = CommonCoreHostFunctions.StateFiles.HashBytes(File.ReadAllBytes(typeof(EmulatorCore).Assembly.Location)).ToLowerInvariant();
    public double FramesPerSecond => (double)HardwareConstants.CpuFrequency / HardwareConstants.CyclesPerFrame;
    public int SampleRate => 44100;
    public bool SupportsSaveStates => false;
    public bool SupportsPointerCapture => false;
    public bool ShutdownRequested { get; private set; }
    public EmulationVideoStandard? Region => EmulationVideoStandard.Pal;
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    public int? HostProcessId => null;
    public int BufferedAudioFrames => 0;
    public int MinimumAudioLatencyMilliseconds => 0;
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) { }
    public long AudioOverrunCount => 0;
    public long AudioUnderrunCount => 0;
    public int DiskCount => 0;
    public int CurrentDiskIndex => -1;
    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk => null;
    public IReadOnlyList<CoreOption> Options => [];
    public IReadOnlyList<string> Diagnostics => [];
    public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool>();
    public IReadOnlySet<string> SupportedContentExtensions { get; } = new HashSet<string>();

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (configuration.Model != MachineModel.BbcMicroModelA) throw new ArgumentOutOfRangeException(nameof(configuration));
        var mos = FirmwareFunctions.Read(configuration, EmulatorConstants.SystemFirmware);
        var basic = FirmwareFunctions.Read(configuration, EmulatorConstants.BasicFirmware);
        _machine = new ModelAMachine(mos, basic);
        ShutdownRequested = false;
        _sequence = 0;
    }
    public void RunFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var machine = _machine ?? throw new InvalidOperationException();
        var image = machine.RunFrame();
        LatestVideoFrame = new VideoFrame(image, HardwareConstants.Width, HardwareConstants.Height, HardwareConstants.Width * 4, EmulationPixelFormat.Xrgb8888, 4f / 3, ++_sequence, TimeSpan.FromSeconds(_sequence / FramesPerSecond));
    }
    public void SetInput(EmulationInputSnapshot snapshot) => _machine?.SetInput(snapshot);
    public void SoftReset() => _machine?.Reset(false);
    public void HardReset() => _machine?.Reset(true);
    public void Stop() => ShutdownRequested = true;
    public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
    public DiskStatus GetDiskStatus() => new(0, -1, true, []);
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    public void SaveMediaChanges(EmulationMediaSlot slot) { }
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => throw new NotSupportedException();
    public void InsertMedia(MediaConfiguration media) => throw new NotSupportedException();
    public void EjectMedia(EmulationMediaSlot slot) => throw new NotSupportedException();
    public void SelectDisk(int index) => throw new NotSupportedException();
    public byte[] SaveState() => throw new NotSupportedException();
    public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
    public void SetOption(string key, string value) => throw new NotSupportedException();

    /// <summary>Détache les données possédées ; aucune ressource empruntée à l'hôte n'est fermée.</summary>
    public void Dispose()
    {
        try { ShutdownRequested = true; }
        finally { _machine = null; LatestVideoFrame = null; _disposed = true; }
    }
}
