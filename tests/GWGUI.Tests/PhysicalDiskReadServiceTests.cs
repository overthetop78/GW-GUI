using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.PhysicalDiskReading;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;
using System.IO;

namespace GWGUI.Tests;

public sealed class PhysicalDiskReadServiceTests
{
    [Fact]
    public async Task ServiceSavesScpDecodesEveryRegisteredCodecAndExploresAcquiredImage()
    {
        var capture = new GreaseweazleFluxCapture(
            Enumerable.Repeat(1_000_000u, 10).ToArray(),
            [1_000_000, 8_000_000],
            40_000_000,
            [0]);
        var device = new ReadDevice(capture);
        var first = new CountingDecoder("test.first", .2);
        var second = new CountingDecoder("test.second", .8);
        var service = new PhysicalDiskReadService(
            new PhysicalDiskFluxAcquisitionService(device),
            new ScpWriter(),
            new FluxDecoderRegistry([first, second]),
            DiskImageExplorer.CreateDefault());
        var options = new PhysicalDiskReadOptions(
            "COM11",
            GreaseweazleBusType.Shugart,
            0,
            [new PhysicalDiskTrackAddress(0, 0)],
            ScpDiskType.Amiga,
            Revolutions: 1);
        var progress = new RecordingProgress<PhysicalDiskReadOperationProgress>();
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-read-{Guid.NewGuid():N}.scp");

        try
        {
            var result = await service.ReadAsync(options, path, progress);

            Assert.True(File.Exists(path));
            Assert.Equal(path, result.OutputPath);
            Assert.Equal(path, result.Document.SourcePath);
            Assert.Same(result.Acquisition.Image, result.Document.ScpImage);
            Assert.Same(capture, result.Acquisition.RawCaptures[options.Tracks[0]]);
            var diagnostic = Assert.Single(result.TrackDiagnostics);
            Assert.Equal("test.second", diagnostic.Best.Result.DecoderId);
            Assert.Equal(["test.first", "test.second"], diagnostic.DecoderResults.Select(item => item.Result.DecoderId));
            Assert.Equal(1, first.CallCount);
            Assert.Equal(1, second.CallCount);
            Assert.Contains(progress.Values, item => item.Stage == PhysicalDiskReadStage.Acquiring);
            Assert.All(progress.Values.Where(item => item.Stage == PhysicalDiskReadStage.Acquiring),
                item => Assert.Equal(options.Tracks, item.Tracks));
            var acquired = Assert.Single(progress.Values, item =>
                item.Stage == PhysicalDiskReadStage.Acquiring && item.PisteAcquise is not null);
            Assert.Equal(0, acquired.PisteAcquise!.Cylindre);
            Assert.Equal(0, acquired.PisteAcquise.Face);
            Assert.NotEmpty(acquired.PisteAcquise.Revolutions);
            Assert.Equal(0, acquired.PisteAcquise.Revolutions[0].DebutIndex);
            Assert.Equal("Captured", acquired.PisteAcquise.Revolutions[0].Origine);
            Assert.Contains(progress.Values, item => item.Stage == PhysicalDiskReadStage.Saving);
            Assert.Contains(progress.Values, item => item.Stage == PhysicalDiskReadStage.Decoding);
            Assert.Contains(progress.Values, item => item.Stage == PhysicalDiskReadStage.Exploring);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private sealed class CountingDecoder(string id, double confidence) : IFluxDecoder
    {
        public string Id { get; } = id;

        public string DisplayName => Id;

        public int CallCount { get; private set; }

        public FluxDecodeResult Decode(FluxRevolution revolution)
        {
            CallCount++;
            return new(Id, DisplayName, confidence, 40, [], []);
        }
    }

    private sealed class RecordingProgress<T> : IProgress<T>
    {
        public List<T> Values { get; } = [];

        public void Report(T value) => Values.Add(value);
    }

    private sealed class ReadDevice(GreaseweazleFluxCapture capture) : IGreaseweazleReadDevice
    {
        public GreaseweazleFirmwareInfo? Firmware { get; private set; }

        public ValueTask<GreaseweazleFirmwareInfo> OpenAsync(string portName, CancellationToken cancellationToken = default)
        {
            Firmware = new(1, 6, 22, capture.SampleFrequency, 7, 1, 1, 4, 144, 224, 64, true);
            return ValueTask.FromResult(Firmware);
        }

        public ValueTask SetBusTypeAsync(GreaseweazleBusType busType, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask<GreaseweazleFluxCapture> ReadFluxAsync(int revolutions, uint tickLimit = 0, int retries = 5, CancellationToken cancellationToken = default) => ValueTask.FromResult(capture);

        public ValueTask ResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask CloseAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
