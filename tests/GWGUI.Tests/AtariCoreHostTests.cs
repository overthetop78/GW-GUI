using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

[SupportedOSPlatform("windows")]
[Trait("Category", "LocalAssets")]
public sealed class AtariCoreHostTests
{
    private const int BlockedHostTimeoutMilliseconds = 250;
    private const int ConnectionFailureTimeoutMilliseconds = 250;
    private const int ClosedPipeProbeTimeoutMilliseconds = 100;
    private const int FirstFrameWidth = 4;
    private const int FirstFrameHeight = 3;
    private const int SecondFrameWidth = 256;
    private const int SecondFrameHeight = 256;
    private const int BytesPerTestPixel = 4;
    private const int TestSampleRate = 44_100;
    private const int ConcurrentRequestCount = 8;
    [Fact]
    public void ProtocolHeaders_RoundTripAndRejectAnotherVersion()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteRequestHeader(writer, AtariHostCommand.RunFrame);
        stream.Position = AtariConstants.FirstBufferIndex;
        using var reader = new BinaryReader(stream);

        Assert.Equal(AtariHostCommand.RunFrame, AtariCoreHostFunctions.ReadRequestHeader(reader));
        Assert.Throws<InvalidDataException>(() =>
            AtariCoreHostFunctions.ValidateProtocolVersion(AtariCoreHostConstants.ProtocolVersion + 1));
    }

    [Fact]
    public void StructuredAtariError_RoundTripsWithoutWpfType()
    {
        var source = new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
            AtariErrorMessages.ContentExtensionUnsupported,
            new Dictionary<string, string> { [AtariConstants.ExtensionContextKey] = "bad" });
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteError(writer, source);
        stream.Position = AtariConstants.FirstBufferIndex;
        using var reader = new BinaryReader(stream);

        var result = AtariCoreHostFunctions.ReadError(reader);

        Assert.Equal(source.Kind, result.Kind);
        Assert.Equal(source.Code, result.Code);
        Assert.Equal("bad", result.Context[AtariConstants.ExtensionContextKey]);
        Assert.DoesNotContain("System.Windows", System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void ProcessCore_UsesPrivateResourcesAndCleansEverythingAfterDoubleDispose()
    {
        var session = CreateSessionDirectory();
        var core = CreateCore();
        try
        {
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), session);
            var processId = Assert.IsType<int>(core.HostProcessId);
            var videoMapName = Assert.IsType<string>(core.VideoMapName);
            var pipeName = Assert.IsType<string>(core.PipeName);

            core.RunFrame();
            Assert.NotNull(core.LatestVideoFrame);
            Assert.Equal(core.LatestVideoFrame!.Pitch * core.LatestVideoFrame.Height,
                core.LatestVideoFrame.Pixels.Length);
            var state = core.SaveState();
            core.LoadState(state);
            var option = Assert.Single(core.Options.Take(AtariConstants.SingleAudioFrameCount));
            core.SetOption(option.Key, option.CurrentValue);
            core.Dispose();
            core.Dispose();

            AssertProcessExited(processId);
            Assert.Throws<FileNotFoundException>(() => MemoryMappedFile.OpenExisting(videoMapName));
            using var pipeProbe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            Assert.Throws<TimeoutException>(() => pipeProbe.Connect(ClosedPipeProbeTimeoutMilliseconds));
        }
        finally
        {
            core.Dispose();
            Directory.Delete(session, recursive: true);
        }
    }

    [Fact]
    public void ProtocolPayloads_RoundTripConfigurationInputMediaStateAndStatusWithoutWpfAssembly()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari800);
        var configurationJson = System.Text.Json.JsonSerializer.Serialize(configuration,
            AtariCoreHostFunctions.JsonOptions);
        var restoredConfiguration = System.Text.Json.JsonSerializer.Deserialize<AtariMachineConfiguration>(
            configurationJson, AtariCoreHostFunctions.JsonOptions);
        Assert.Equal(configuration.Model, restoredConfiguration?.Model);

        var media = new AtariMediaConfiguration("game.rom", AtariMediaKind.Cartridge,
            GWGUI.Emulation.EmulationMediaSlot.Cartridge0, IsReadOnly: true);
        var mediaJson = System.Text.Json.JsonSerializer.Serialize(media, AtariCoreHostFunctions.JsonOptions);
        Assert.Equal(media, System.Text.Json.JsonSerializer.Deserialize<AtariMediaConfiguration>(mediaJson,
            AtariCoreHostFunctions.JsonOptions));

        var diskStatus = new AtariDiskStatus(2, 1, false,
        [
            new AtariDiskImageStatus(0, "first.st", "First"),
            new AtariDiskImageStatus(1, "second.st", "Second")
        ]);
        using var diskStatusStream = new MemoryStream();
        using (var diskStatusWriter = new BinaryWriter(diskStatusStream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteDiskStatus(diskStatusWriter, diskStatus);
        diskStatusStream.Position = AtariConstants.FirstBufferIndex;
        using var diskStatusReader = new BinaryReader(diskStatusStream);
        var restoredDiskStatus = AtariCoreHostFunctions.ReadDiskStatus(diskStatusReader);
        Assert.Equal(diskStatus.ImageCount, restoredDiskStatus.ImageCount);
        Assert.Equal(diskStatus.CurrentIndex, restoredDiskStatus.CurrentIndex);
        Assert.Equal(diskStatus.IsEjected, restoredDiskStatus.IsEjected);
        Assert.Equal(diskStatus.Images, restoredDiskStatus.Images);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            AtariCoreHostFunctions.WriteInput(writer, GWGUI.Emulation.EmulationInputSnapshot.Empty);
            AtariCoreHostFunctions.WriteBytes(writer, [AtariConstants.NativeBooleanTrue]);
            AtariCoreHostFunctions.WriteResponseHeader(writer, AtariHostResponseStatus.Success);
        }
        stream.Position = AtariConstants.FirstBufferIndex;
        using var reader = new BinaryReader(stream);
        var restoredInput = AtariCoreHostFunctions.ReadInput(reader);
        Assert.Empty(restoredInput.Keys);
        Assert.Equal(GWGUI.Emulation.EmulationInputSnapshot.Empty.Pointer, restoredInput.Pointer);
        Assert.Equal(GWGUI.Emulation.EmulationInputSnapshot.Empty.Controllers, restoredInput.Controllers);
        Assert.Equal(new byte[] { AtariConstants.NativeBooleanTrue }, AtariCoreHostFunctions.ReadBytes(reader));
        Assert.Equal(AtariHostResponseStatus.Success, AtariCoreHostFunctions.ReadResponseHeader(reader));

        var references = typeof(AtariCoreHost).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();
        Assert.DoesNotContain("PresentationFramework", references);
        Assert.DoesNotContain("PresentationCore", references);
    }

    [Fact]
    public void SharedVideoAndAudio_RoundTripChangingFrameDimensionsWithoutCorruption()
    {
        var mapName = AtariCoreHostFunctions.CreateVideoMapName();
        using var video = new AtariSharedVideoWriter(mapName);
        MemoryMappedFile? memory = null;
        MemoryMappedViewAccessor? map = null;
        string? activeName = null;
        var first = CreateFrame(FirstFrameWidth, FirstFrameHeight, AtariConstants.NativeBooleanTrue);
        var second = CreateFrame(SecondFrameWidth, SecondFrameHeight, byte.MaxValue);

        AssertFrame(first, RoundTripFrame(first, video, ref memory, ref map, ref activeName));
        var firstMapName = activeName;
        var firstCapacity = video.SlotCapacity;
        AssertFrame(second, RoundTripFrame(second, video, ref memory, ref map, ref activeName));

        Assert.NotEqual(firstMapName, activeName);
        Assert.True(video.SlotCapacity > firstCapacity);
        var secondMapName = activeName;
        AssertFrame(first, RoundTripFrame(first, video, ref memory, ref map, ref activeName));
        Assert.NotEqual(secondMapName, activeName);
        Assert.Equal(firstCapacity, video.SlotCapacity);

        var samples = new short[] { short.MinValue, -1, 0, 1, short.MaxValue, 42 };
        var chunks = new[]
        {
            new GWGUI.Emulation.AudioChunk(samples, TestSampleRate,
                samples.Length / AtariConstants.StereoChannelCount, AtariConstants.SingleAudioFrameCount,
                TimeSpan.Zero)
        };
        using var audioStream = new MemoryStream();
        using (var writer = new BinaryWriter(audioStream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteAudio(writer, chunks);
        audioStream.Position = AtariConstants.FirstBufferIndex;
        using var audioReader = new BinaryReader(audioStream);
        var restored = Assert.Single(AtariCoreHostFunctions.ReadAudio(audioReader));
        Assert.Equal(samples, restored.InterleavedStereo.ToArray());
        map?.Dispose();
        memory?.Dispose();
    }

    [Fact]
    public void TwoMachines_HaveDistinctHostsAndOneCrashDoesNotStopTheOther()
    {
        var firstSession = CreateSessionDirectory();
        var secondSession = CreateSessionDirectory();
        using var first = CreateCore();
        using var second = CreateCore();
        try
        {
            first.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), firstSession);
            second.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), secondSession);
            var firstProcessId = Assert.IsType<int>(first.HostProcessId);
            var secondProcessId = Assert.IsType<int>(second.HostProcessId);
            Assert.NotEqual(firstProcessId, secondProcessId);
            Assert.NotEqual(first.PipeName, second.PipeName);
            Assert.NotEqual(first.VideoMapName, second.VideoMapName);

            Process.GetProcessById(firstProcessId).Kill(entireProcessTree: true);
            Assert.Throws<InvalidOperationException>(() => first.RunFrame());
            second.RunFrame();

            Assert.False(Process.GetProcessById(secondProcessId).HasExited);
        }
        finally
        {
            first.Dispose();
            second.Dispose();
            Directory.Delete(firstSession, recursive: true);
            Directory.Delete(secondSession, recursive: true);
        }
    }

    [Fact]
    public void BlockedHost_TimesOutAndOnlyThatHostIsTerminated()
    {
        var blockedSession = CreateSessionDirectory();
        var healthySession = CreateSessionDirectory();
        using var blocked = CreateCore(TimeSpan.FromMilliseconds(BlockedHostTimeoutMilliseconds));
        using var healthy = CreateCore();
        try
        {
            blocked.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), blockedSession);
            healthy.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), healthySession);
            var blockedProcessId = Assert.IsType<int>(blocked.HostProcessId);
            var healthyProcessId = Assert.IsType<int>(healthy.HostProcessId);
            using var blockedProcess = Process.GetProcessById(blockedProcessId);
            Assert.Equal(AtariCoreHostConstants.NativeOperationSuccess,
                NativeMethods.SuspendProcess(blockedProcess.Handle));

            var error = Assert.Throws<InvalidOperationException>(() => blocked.RunFrame());
            healthy.RunFrame();

            Assert.Contains(AtariCoreHostErrors.ResponseTimeout, error.Message);
            AssertProcessExited(blockedProcessId);
            Assert.False(Process.GetProcessById(healthyProcessId).HasExited);
        }
        finally
        {
            blocked.Dispose();
            healthy.Dispose();
            Directory.Delete(blockedSession, recursive: true);
            Directory.Delete(healthySession, recursive: true);
        }
    }

    [Fact]
    public void Cancellation_StopsOnlyTheAssociatedHost()
    {
        var session = CreateSessionDirectory();
        using var cancellation = new CancellationTokenSource();
        using var core = CreateCore(cancellationToken: cancellation.Token);
        try
        {
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), session);
            var processId = Assert.IsType<int>(core.HostProcessId);

            cancellation.Cancel();
            var error = Assert.Throws<InvalidOperationException>(() => core.RunFrame());

            Assert.Contains(AtariCoreHostErrors.RequestCancelled, error.Message);
            AssertProcessExited(processId);
        }
        finally
        {
            core.Dispose();
            Directory.Delete(session, recursive: true);
        }
    }

    [Fact]
    public async Task ConcurrentRequests_AreSerializedAndEveryResponseRemainsReadable()
    {
        var session = CreateSessionDirectory();
        using var core = CreateCore();
        try
        {
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), session);

            await Task.WhenAll(Enumerable.Range(AtariConstants.FirstBufferIndex, ConcurrentRequestCount)
                .Select(_ => Task.Run(core.RunFrame)));

            Assert.NotNull(core.LatestVideoFrame);
            Assert.False(Process.GetProcessById(Assert.IsType<int>(core.HostProcessId)).HasExited);
        }
        finally
        {
            core.Dispose();
            Directory.Delete(session, recursive: true);
        }
    }

    [Fact]
    public void ConnectionFailure_ReleasesPipeMappingAndStartedProcess()
    {
        var session = CreateSessionDirectory();
        var commandInterpreter = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        using var core = new AtariProcessCore(commandInterpreter,
            Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll"), AtariCoreKind.Atari800,
            connectionTimeout: TimeSpan.FromMilliseconds(ConnectionFailureTimeoutMilliseconds));
        try
        {
            Assert.ThrowsAny<Exception>(() =>
                core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), session));
            var videoMapName = Assert.IsType<string>(core.VideoMapName);

            Assert.Throws<FileNotFoundException>(() => MemoryMappedFile.OpenExisting(videoMapName));
            if (core.HostProcessId is { } processId) AssertProcessExited(processId);
        }
        finally
        {
            core.Dispose();
            Directory.Delete(session, recursive: true);
        }
    }

    private static AtariProcessCore CreateCore(TimeSpan? responseTimeout = null,
        CancellationToken cancellationToken = default) => new(FindAppExecutable(),
        Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll"), AtariCoreKind.Atari800,
        responseTimeout, cancellationToken);

    private static GWGUI.Emulation.VideoFrame CreateFrame(int width, int height, byte value)
    {
        var pitch = width * BytesPerTestPixel;
        return new GWGUI.Emulation.VideoFrame(Enumerable.Repeat(value, pitch * height).ToArray(), width, height,
            pitch, GWGUI.Emulation.EmulationPixelFormat.Xrgb8888, default,
            AtariConstants.SingleAudioFrameCount, TimeSpan.Zero);
    }

    private static GWGUI.Emulation.VideoFrame? RoundTripFrame(GWGUI.Emulation.VideoFrame frame,
        AtariSharedVideoWriter video, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? map,
        ref string? activeName)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteResizableSharedFrame(writer, frame, video);
        stream.Position = AtariConstants.FirstBufferIndex;
        using var reader = new BinaryReader(stream);
        return AtariCoreHostFunctions.ReadResizableSharedFrame(reader, ref memory, ref map, ref activeName);
    }

    private static void AssertFrame(GWGUI.Emulation.VideoFrame expected, GWGUI.Emulation.VideoFrame? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Width, actual!.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Pitch, actual.Pitch);
        Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
    }

    private static string CreateSessionDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-atari-host-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static string FindAppExecutable() => Path.Combine(FindRepositoryRoot(), "src", "GWGUI.App", "bin",
        "Debug", "net10.0-windows10.0.19041.0", "gwgui.app.exe");

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln"))) current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException();
    }

    private static void AssertProcessExited(int processId)
    {
        Assert.Throws<ArgumentException>(() => Process.GetProcessById(processId));
    }

    private static class NativeMethods
    {
        [DllImport("ntdll.dll", EntryPoint = "NtSuspendProcess")]
        internal static extern int SuspendProcess(nint processHandle);
    }
}
