using System.IO;
using System.Text;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariStateStoreTests
{
    [Fact]
    public async Task QuickAndNamedStatesUseMachineDirectoryWithMetadataAndCapture()
    {
        var root = CreateRoot();
        var machine = new StoreMachine(Configuration());
        var store = new AtariStateStore(root);
        var before = DateTimeOffset.UtcNow;
        try
        {
            var quick = await store.SaveQuickStateAsync(machine, AtariStateStoreTestConstants.CaptureBytes);
            var named = await store.SaveNamedStateAsync(machine, AtariStateStoreTestConstants.NamedState,
                AtariStateStoreTestConstants.CaptureBytes);
            var after = DateTimeOffset.UtcNow;

            var machineDirectory = AtariStateStoreFunctions.GetMachineDirectory(root, machine.Configuration.Id);
            Assert.True(File.Exists(Path.Combine(machineDirectory, quick.StateFileName)));
            Assert.True(File.Exists(Path.Combine(machineDirectory, quick.CaptureFileName!)));
            Assert.True(File.Exists(Path.Combine(machineDirectory, named.StateFileName)));
            Assert.InRange(named.CreatedAtUtc, before, after);
            Assert.Equal(AtariStoredStateKind.Quick, quick.Kind);
            Assert.Equal(AtariStoredStateKind.Named, named.Kind);
            Assert.Equal(AtariStateStoreTestConstants.ExpectedStateCount, store.List(machine.Configuration.Id).Count);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public async Task InvalidNamesAreRejected(string name)
    {
        var root = CreateRoot();
        try
        {
            var store = new AtariStateStore(root);
            await Assert.ThrowsAsync<ArgumentException>(() =>
                store.SaveNamedStateAsync(new StoreMachine(Configuration()), name).AsTask());
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    public static TheoryData<string> InvalidNames => new()
    {
        string.Empty,
        " ",
        AtariStateStoreConstants.CurrentDirectoryName,
        AtariStateStoreConstants.ParentDirectoryName,
        AtariStateStoreConstants.QuickStateName,
        AtariStateStoreTestConstants.PathTraversalName,
        new string(AtariStateStoreTestConstants.NameCharacter,
            AtariStateStoreConstants.MaximumStateNameLength + AtariStateStoreTestConstants.NextCount)
    };

    [Fact]
    public void InterruptedAtomicWriteKeepsPreviousFileAndRemovesTemporaryFile()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, AtariStateStoreTestConstants.AtomicFileName);
        File.WriteAllText(path, AtariStateStoreTestConstants.OriginalContents);
        try
        {
            Assert.Throws<IOException>(() => AtariStateStoreFunctions.WriteAtomically(path, stream =>
            {
                stream.Write(AtariStateStoreTestConstants.ReplacementBytes);
                throw new IOException(AtariStateStoreTestConstants.InterruptionMessage);
            }));
            Assert.Equal(AtariStateStoreTestConstants.OriginalContents, File.ReadAllText(path));
            Assert.False(File.Exists(path + AtariStateStoreConstants.TemporaryFileExtension));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task ConcurrentMetadataReadsNeverObservePartialDocuments()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, AtariStateStoreTestConstants.MetadataFileName);
        var first = Metadata(AtariStateStoreTestConstants.FirstMetadataName);
        var second = Metadata(AtariStateStoreTestConstants.SecondMetadataName);
        AtariStateStoreFunctions.WriteMetadataAtomically(path, first);
        try
        {
            var writes = Task.Run(() =>
            {
                for (var index = AtariStateStoreTestConstants.FirstIndex;
                     index < AtariStateStoreTestConstants.ConcurrentIterationCount; index++)
                    AtariStateStoreFunctions.WriteMetadataAtomically(path,
                        index % AtariStateStoreTestConstants.AlternatingDivisor == AtariStateStoreTestConstants.FirstIndex
                            ? first : second);
            });
            var reads = Task.Run(() =>
            {
                for (var index = AtariStateStoreTestConstants.FirstIndex;
                     index < AtariStateStoreTestConstants.ConcurrentIterationCount; index++)
                {
                    var metadata = AtariStateStoreFunctions.ReadMetadata(path);
                    Assert.Contains(metadata.Name,
                        new[] { AtariStateStoreTestConstants.FirstMetadataName,
                            AtariStateStoreTestConstants.SecondMetadataName });
                }
            });
            await Task.WhenAll(writes, reads);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task ConcurrentStateReadsNeverObservePartialContainers()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, AtariStateStoreTestConstants.StateFileName);
        var firstHeader = StateHeader(AtariStateStoreTestConstants.StateBytes);
        var secondHeader = StateHeader(AtariStateStoreTestConstants.SecondStateBytes);
        AtariStateFileFunctions.Write(path, firstHeader, AtariStateStoreTestConstants.StateBytes);
        try
        {
            var writes = Task.Run(() =>
            {
                for (var index = AtariStateStoreTestConstants.FirstIndex;
                     index < AtariStateStoreTestConstants.ConcurrentIterationCount; index++)
                {
                    var useFirst = index % AtariStateStoreTestConstants.AlternatingDivisor
                                   == AtariStateStoreTestConstants.FirstIndex;
                    AtariStateFileFunctions.Write(path, useFirst ? firstHeader : secondHeader,
                        useFirst ? AtariStateStoreTestConstants.StateBytes
                            : AtariStateStoreTestConstants.SecondStateBytes);
                }
            });
            var reads = Task.Run(() =>
            {
                for (var index = AtariStateStoreTestConstants.FirstIndex;
                     index < AtariStateStoreTestConstants.ConcurrentIterationCount; index++)
                {
                    var state = AtariStateFileFunctions.Read(path).State;
                    Assert.True(state.SequenceEqual(AtariStateStoreTestConstants.StateBytes)
                                || state.SequenceEqual(AtariStateStoreTestConstants.SecondStateBytes));
                }
            });
            await Task.WhenAll(writes, reads);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task RestoreUsesStoredStateAndDeletionRequiresConfirmationAndTargetsOneMachine()
    {
        var root = CreateRoot();
        var firstMachine = new StoreMachine(Configuration());
        var secondMachine = new StoreMachine(Configuration());
        var store = new AtariStateStore(root);
        try
        {
            await store.SaveNamedStateAsync(firstMachine, AtariStateStoreTestConstants.NamedState);
            await store.SaveNamedStateAsync(secondMachine, AtariStateStoreTestConstants.NamedState);
            await store.RestoreAsync(firstMachine, AtariStateStoreTestConstants.NamedState,
                AtariStoredStateKind.Named);

            Assert.Equal(AtariStateStoreTestConstants.StateBytes, firstMachine.LoadedState);
            Assert.False(store.DeleteMachineStates(firstMachine.Configuration.Id, confirmed: false));
            Assert.Single(store.List(firstMachine.Configuration.Id));
            Assert.True(store.DeleteMachineStates(firstMachine.Configuration.Id, confirmed: true));
            Assert.Empty(store.List(firstMachine.Configuration.Id));
            Assert.Single(store.List(secondMachine.Configuration.Id));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task StateContainerContainsHashesButNoFirmwareOrMediaPaths()
    {
        var root = CreateRoot();
        var configuration = Configuration(
            AtariStateStoreTestConstants.ProtectedFirmwarePath,
            AtariStateStoreTestConstants.ProtectedMediaPath);
        var machine = new StoreMachine(configuration);
        var store = new AtariStateStore(root);
        try
        {
            var metadata = await store.SaveQuickStateAsync(machine);
            var statePath = Path.Combine(
                AtariStateStoreFunctions.GetMachineDirectory(root, configuration.Id), metadata.StateFileName);
            var contents = Encoding.UTF8.GetString(File.ReadAllBytes(statePath));
            Assert.DoesNotContain(AtariStateStoreTestConstants.ProtectedFirmwarePath, contents,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(AtariStateStoreTestConstants.ProtectedMediaPath, contents,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    private static AtariStoredStateMetadata Metadata(string name) => new(name, AtariStoredStateKind.Named,
        AtariStateStoreTestConstants.MetadataDate, AtariStateStoreTestConstants.StateFileName, null,
        AtariCoreKind.Stella, AtariStateStoreTestConstants.CoreName, AtariStateStoreTestConstants.CoreVersion,
        AtariMachineModel.Atari2600, AtariStateStoreTestConstants.ConfigurationHash,
        AtariStateStoreTestConstants.ContentHash);

    private static AtariSavedStateHeader StateHeader(byte[] state) => new(
        AtariStateConstants.CurrentFormatVersion, AtariCoreKind.Stella, AtariStateStoreTestConstants.CoreName,
        AtariStateStoreTestConstants.CoreVersion, AtariStateStoreTestConstants.CoreHash,
        AtariMachineModel.Atari2600, AtariStateStoreTestConstants.ConfigurationHash,
        AtariStateStoreTestConstants.ContentHash, AtariSavedStateFunctions.HashBytes(state));

    private static AtariMachineConfiguration Configuration(string? firmwarePath = null, string? mediaPath = null) =>
        new(firmwarePath is null ? AtariMachineModel.Atari2600 : AtariMachineModel.Atari7800,
            firmwarePath is null ? [] : [new AtariFirmwareConfiguration(AtariFirmwareKind.Atari7800Bios,
                firmwarePath, IsRequired: false)],
            mediaPath is null ? [] : [new AtariMediaConfiguration(mediaPath, AtariMediaKind.Cartridge,
                EmulationMediaSlot.Cartridge0)]);

    private static string CreateRoot()
    {
        var path = Path.Combine(Path.GetTempPath(),
            AtariStateStoreTestConstants.RootPrefix + Guid.NewGuid().ToString(AtariStateStoreConstants.MachineIdentifierFormat));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private sealed class StoreMachine(AtariMachineConfiguration configuration) : IAtariMachine
    {
        public byte[]? LoadedState { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();
        public EmulationMachineState State => EmulationMachineState.Running;
        public AtariMachineConfiguration Configuration { get; } = configuration;
        public Exception? Fault => null;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> AvailableOptions => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => AtariStateStoreTestConstants.CoreName;
        public string CoreVersion => AtariStateStoreTestConstants.CoreVersion;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates => true;
        public bool IsAudioMuted => false;
        public float AudioVolume => AtariStateStoreTestConstants.AudioVolume;
        public AtariRuntimeStatus RuntimeStatus => throw new NotSupportedException();
        public event EventHandler<VideoFrame>? VideoFrameReady { add { } remove { } }
        public event EventHandler<AudioChunk>? AudioChunkReady { add { } remove { } }
        public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask PauseAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask ResumeAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask StopAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral) { }
        public void SetAudioMuted(bool muted) { }
        public void SetAudioVolume(float volume) { }
        public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) { }
        public ValueTask InsertMediaAsync(AtariMediaConfiguration media,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask EjectMediaAsync(EmulationMediaSlot slot,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var header = new AtariSavedStateHeader(AtariStateConstants.CurrentFormatVersion, Configuration.Core,
                CoreName, CoreVersion, AtariStateStoreTestConstants.CoreHash, Configuration.Model,
                AtariStateStoreTestConstants.ConfigurationHash, AtariStateStoreTestConstants.ContentHash,
                AtariSavedStateFunctions.HashBytes(AtariStateStoreTestConstants.StateBytes));
            AtariStateFileFunctions.Write(path, header, AtariStateStoreTestConstants.StateBytes);
            return ValueTask.CompletedTask;
        }
        public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadedState = AtariStateFileFunctions.Read(path).State;
            return ValueTask.CompletedTask;
        }
        public ValueTask SetOptionAsync(string key, string value,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

internal static class AtariStateStoreTestConstants
{
    internal const string NamedState = "Before boss";
    internal const string PathTraversalName = "..\\escape";
    internal const string AtomicFileName = "atomic.bin";
    internal const string MetadataFileName = "state.json";
    internal const string StateFileName = "state.gwats";
    internal const string OriginalContents = "original";
    internal const string InterruptionMessage = "interrupted";
    internal const string FirstMetadataName = "first";
    internal const string SecondMetadataName = "second";
    internal const string CoreName = "Stella";
    internal const string CoreVersion = "test";
    internal const string CoreHash = "core-hash";
    internal const string ConfigurationHash = "configuration-hash";
    internal const string ContentHash = "content-hash";
    internal const string ProtectedFirmwarePath = "C:\\protected\\firmware.bin";
    internal const string ProtectedMediaPath = "C:\\protected\\game.a26";
    internal const string RootPrefix = "gwgui-atari-state-store-";
    internal const char NameCharacter = 'a';
    internal const int ExpectedStateCount = 2;
    internal const int FirstIndex = 0;
    internal const int NextCount = 1;
    internal const int ConcurrentIterationCount = 100;
    internal const int AlternatingDivisor = 2;
    internal const float AudioVolume = 1.0f;
    internal static readonly DateTimeOffset MetadataDate = new(2026, 8, 16,
        12, 0, 0, TimeSpan.Zero);
    internal static readonly byte[] CaptureBytes = [1, 2, 3];
    internal static readonly byte[] StateBytes = [4, 5, 6];
    internal static readonly byte[] SecondStateBytes = [10, 11, 12, 13];
    internal static readonly byte[] ReplacementBytes = [7, 8, 9];
}
