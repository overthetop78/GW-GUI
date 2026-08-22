using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;
using System.IO;

namespace GWGUI.Tests;

public sealed class AtariSavedStateTests
{
    public static TheoryData<AtariEmulator, AtariMachineModel> CoreModels => new()
    {
        { AtariEmulator.Hatari, AtariMachineModel.St },
        { AtariEmulator.Atari800, AtariMachineModel.Atari800 },
        { AtariEmulator.Stella, AtariMachineModel.Atari2600 },
        { AtariEmulator.ProSystem, AtariMachineModel.Atari7800 },
        { AtariEmulator.BeetleLynx, AtariMachineModel.Lynx },
        { AtariEmulator.VirtualJaguar, AtariMachineModel.Jaguar }
    };

    [Theory]
    [MemberData(nameof(CoreModels))]
    public async Task StateRoundTripUsesContainerForEveryCore(AtariEmulator kind, AtariMachineModel model)
    {
        var paths = CreatePaths();
        var core = new StateCore(kind);
        await using var machine = CreateMachine(core, model, paths.Session);
        try
        {
            await machine.StartAsync();
            await machine.SaveStateAsync(paths.State);
            await machine.LoadStateAsync(paths.State);

            Assert.Equal(AtariSavedStateTestConstants.Payload, core.LoadedState);
            var saved = AtariStateFileFunctions.Read(paths.State);
            Assert.Equal(kind, saved.Header.Core);
            Assert.Equal(model, saved.Header.Model);
            Assert.Equal(AtariSavedStateTestConstants.CoreVersion, saved.Header.CoreVersion);
            Assert.Equal(AtariSavedStateTestConstants.Payload, saved.State);
        }
        finally
        {
            await machine.StopAsync();
            DeleteFile(paths.State);
        }
    }

    [Theory]
    [MemberData(nameof(CoreModels))]
    public async Task NativeLoadFailureIsPreciseForEveryCore(AtariEmulator kind, AtariMachineModel model)
    {
        var paths = CreatePaths();
        var core = new StateCore(kind) { RejectLoad = true };
        await using var machine = CreateMachine(core, model, paths.Session);
        try
        {
            await machine.StartAsync();
            await machine.SaveStateAsync(paths.State);

            var error = await Assert.ThrowsAsync<AtariEmulationException>(
                () => machine.LoadStateAsync(paths.State).AsTask());
            Assert.Equal(AtariErrorCategory.State, error.Category);
            Assert.Equal(AtariErrorCode.StateIncompatible, error.Code);
            Assert.Equal(AtariErrorMessages.StateLoadFailed, error.Message);
        }
        finally
        {
            DeleteFile(paths.State);
        }
    }

    [Fact]
    public void FileReaderRejectsTruncationAndCorruptedPayload()
    {
        var paths = CreatePaths();
        try
        {
            File.WriteAllBytes(paths.State, AtariSavedStateTestConstants.TruncatedBytes);
            var truncated = Assert.Throws<AtariEmulationException>(() => AtariStateFileFunctions.Read(paths.State));
            Assert.Equal(AtariStateConstants.TruncatedFileError, truncated.Message);

            var core = new StateCore(AtariEmulator.Stella);
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600);
            var header = AtariSavedStateFunctions.CreateHeader(configuration, core,
                AtariSavedStateTestConstants.Payload);
            AtariStateFileFunctions.Write(paths.State, header, AtariSavedStateTestConstants.Payload);
            var bytes = File.ReadAllBytes(paths.State);
            bytes[^AtariSavedStateTestConstants.LastElementOffset] ^= AtariSavedStateTestConstants.CorruptionMask;
            File.WriteAllBytes(paths.State, bytes);
            var corrupted = Assert.Throws<AtariEmulationException>(() => AtariStateFileFunctions.Read(paths.State));
            Assert.Equal(AtariStateConstants.CorruptedPayloadError, corrupted.Message);
        }
        finally
        {
            DeleteFile(paths.State);
        }
    }

    [Fact]
    public void ValidationDistinguishesCoreModelConfigurationAndContent()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atari-state-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var contentPath = Path.Combine(directory, AtariSavedStateTestConstants.ContentFileName);
        File.WriteAllBytes(contentPath, AtariSavedStateTestConstants.FirstContent);
        try
        {
            var core = new StateCore(AtariEmulator.Stella);
            var configuration = Configuration(contentPath, AtariSavedStateTestConstants.FirstOptionValue);
            var header = AtariSavedStateFunctions.CreateHeader(configuration, core,
                AtariSavedStateTestConstants.Payload);

            AssertMismatch(header with { Core = AtariEmulator.ProSystem }, configuration, core,
                AtariStateConstants.CoreMismatchError);
            AssertMismatch(header with { Model = AtariMachineModel.Atari7800 }, configuration, core,
                AtariStateConstants.ModelMismatchError);
            AssertMismatch(header,
                Configuration(contentPath, AtariSavedStateTestConstants.SecondOptionValue), core,
                AtariStateConstants.ConfigurationMismatchError);

            File.WriteAllBytes(contentPath, AtariSavedStateTestConstants.SecondContent);
            AssertMismatch(header, configuration, core, AtariStateConstants.ContentMismatchError);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task UnavailableStatesAreRejectedBeforeCallingCore()
    {
        var paths = CreatePaths();
        var core = new StateCore(AtariEmulator.Stella) { SupportsSaveStates = false };
        await using var machine = CreateMachine(core, AtariMachineModel.Atari2600, paths.Session);
        try
        {
            await machine.StartAsync();
            var error = await Assert.ThrowsAsync<AtariEmulationException>(
                () => machine.SaveStateAsync(paths.State).AsTask());
            Assert.Equal(AtariErrorMessages.StateUnavailable, error.Message);
            Assert.Equal(AtariSavedStateTestConstants.NoCalls, core.SaveCalls);
        }
        finally
        {
            await machine.StopAsync();
            DeleteFile(paths.State);
        }
    }

    [Fact]
    public async Task StateValidationUsesMediaChangedWhileTheMachineIsRunning()
    {
        var paths = CreatePaths();
        var mediaPath = Path.Combine(paths.Session, "state-disk.st");
        Directory.CreateDirectory(paths.Session);
        await File.WriteAllBytesAsync(mediaPath, AtariSavedStateTestConstants.FirstContent);
        var core = new StateCore(AtariEmulator.Hatari);
        await using var machine = CreateMachine(core, AtariMachineModel.St, paths.Session);
        try
        {
            var media = new AtariMediaConfiguration(mediaPath, AtariMediaCategory.Floppy,
                EmulationMediaSlot.Floppy0);
            await machine.StartAsync();
            await machine.InsertMediaAsync(media);
            await machine.SaveStateAsync(paths.State);
            await machine.LoadStateAsync(paths.State);

            await machine.EjectMediaAsync(EmulationMediaSlot.Floppy0);
            var error = await Assert.ThrowsAsync<AtariEmulationException>(
                () => machine.LoadStateAsync(paths.State).AsTask());
            Assert.Equal(AtariStateConstants.ContentMismatchError, error.Message);
        }
        finally
        {
            await machine.StopAsync();
            DeleteFile(paths.State);
            DeleteFile(mediaPath);
        }
    }

    [Fact]
    public void FileContainerPreservesVariablePayloadSizesExactly()
    {
        var paths = CreatePaths();
        var core = new StateCore(AtariEmulator.Stella);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600);
        try
        {
            foreach (var payload in AtariSavedStateTestConstants.VariablePayloads)
            {
                var header = AtariSavedStateFunctions.CreateHeader(configuration, core, payload);
                AtariStateFileFunctions.Write(paths.State, header, payload);
                Assert.Equal(payload, AtariStateFileFunctions.Read(paths.State).State);
            }
        }
        finally
        {
            DeleteFile(paths.State);
        }
    }

    private static AtariMachine CreateMachine(StateCore core, AtariMachineModel model, string session) =>
        new(Guid.NewGuid(), new AtariMachineConfiguration(model), core, session);

    private static AtariMachineConfiguration Configuration(string contentPath, string optionValue) => new(
        AtariMachineModel.Atari2600,
        media:
        [
            new AtariMediaConfiguration(contentPath, AtariMediaCategory.Cartridge, EmulationMediaSlot.Cartridge0)
        ],
        options: new Dictionary<string, string>
        {
            [AtariSavedStateTestConstants.OptionKey] = optionValue
        });

    private static void AssertMismatch(AtariSavedStateHeader header, AtariMachineConfiguration configuration,
        StateCore core, string message)
    {
        var error = Assert.Throws<AtariEmulationException>(
            () => AtariSavedStateFunctions.Validate(header, configuration, core));
        Assert.Equal(AtariErrorCode.StateIncompatible, error.Code);
        Assert.Equal(message, error.Message);
    }

    private static (string Session, string State) CreatePaths()
    {
        var root = Path.Combine(Path.GetTempPath(), $"atari-state-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return (Path.Combine(root, AtariSavedStateTestConstants.SessionDirectoryName),
            Path.Combine(root, AtariSavedStateTestConstants.StateFileName));
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
    }

    private sealed class StateCore(AtariEmulator kind) : IAtariCore
    {
        public AtariEmulator Emulator { get; } = kind;
        public byte[]? LoadedState { get; private set; }
        public bool RejectLoad { get; init; }
        public int SaveCalls { get; private set; }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => Emulator.ToString();
        public string CoreVersion => AtariSavedStateTestConstants.CoreVersion;
        public string CoreSha256 => AtariSavedStateTestConstants.CoreSha256;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates { get; init; } = true;
        public double FramesPerSecond => AtariSavedStateTestConstants.FramesPerSecond;
        public int SampleRate => AtariSavedStateTestConstants.SampleRate;
        public AtariRuntimeRegion? Region => AtariRuntimeRegion.Ntsc;
        public int BufferedAudioFrames => AtariSavedStateTestConstants.NoCalls;
        public long AudioOverrunCount => AtariSavedStateTestConstants.NoCalls;
        public long AudioUnderrunCount => AtariSavedStateTestConstants.NoCalls;
        public AtariHostProcessState HostProcessState => AtariHostProcessState.InProcess;
        public int? HostProcessId => null;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null) { }
        public void RunFrame() { }
        public void HardReset() { }
        public void Stop() { }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, AtariPeripheralCategory peripheral) { }
        public void InsertMedia(AtariMediaConfiguration media) { }
        public void EjectMedia(EmulationMediaSlot slot) { }
        public void SelectDisk(int index) { }
        public void SaveMediaChanges(EmulationMediaSlot slot) { }
        public AtariDiskStatus GetDiskStatus() => new(AtariSavedStateTestConstants.NoCalls,
            AtariSavedStateTestConstants.NoCalls, true, []);
        public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
        public byte[] SaveState()
        {
            SaveCalls++;
            return AtariSavedStateTestConstants.Payload.ToArray();
        }
        public void LoadState(ReadOnlySpan<byte> state)
        {
            if (RejectLoad)
                throw new AtariEmulationException(AtariErrorCategory.State, AtariErrorCode.StateIncompatible,
                    AtariErrorMessages.StateLoadFailed);
            LoadedState = state.ToArray();
        }
        public void SetOption(string key, string value) { }
        public void Dispose() { }
    }
}

internal static class AtariSavedStateTestConstants
{
    internal const string SessionDirectoryName = "session";
    internal const string StateFileName = "state.gwats";
    internal const string ContentFileName = "game.a26";
    internal const string OptionKey = "difficulty";
    internal const string FirstOptionValue = "a";
    internal const string SecondOptionValue = "b";
    internal const string CoreVersion = "1.0-test";
    internal const string CoreSha256 = "0123456789abcdef";
    internal const int SampleRate = 44100;
    internal const double FramesPerSecond = 60.0;
    internal const int NoCalls = 0;
    internal const int LastElementOffset = 1;
    internal const byte CorruptionMask = byte.MaxValue;
    internal static readonly byte[] Payload = [1, 2, 3, 4];
    internal static readonly byte[] TruncatedBytes = [1, 2, 3];
    internal static readonly byte[] FirstContent = [10, 20];
    internal static readonly byte[] SecondContent = [30, 40];
    internal static readonly IReadOnlyList<byte[]> VariablePayloads =
    [
        [1],
        [1, 2, 3, 4, 5, 6, 7, 8]
    ];
}
