using GWGUI.App.Services.Conversion;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;

namespace GWGUI.Tests;

public sealed class EngineSelectionTests
{
    [Fact]
    public void MigrationMovesLegacyPhysicalEngineChoicesToGlobalSettings()
    {
        var settings = new AppSettings { SchemaVersion = 7 };
        settings.Read.EnabledOptions.Add("internal-reader");
        settings.Write.EnabledOptions.Add("internal-writer");

        SettingsMigrator.Migrate(settings);

        Assert.Equal(OperationEngine.Internal, settings.Engines.PhysicalRead);
        Assert.Equal(OperationEngine.Internal, settings.Engines.PhysicalWrite);
        Assert.Equal(OperationEngine.Internal, settings.Engines.Conversion);
        Assert.Equal(OperationEngine.Internal, settings.Engines.ExplorerRead);
        Assert.DoesNotContain("internal-reader", settings.Read.EnabledOptions);
        Assert.DoesNotContain("internal-writer", settings.Write.EnabledOptions);
    }

    [Fact]
    public async Task ExternalConversionUsesHostToolsEvenWhenInternalConversionExists()
    {
        var runner = new RecordingRunner();
        var progress = new RecordingProgress();
        var output = new ConversionOutput("amiga.amigados", ".adf", "output.adf", false);
        var command = new GwCommand("gw.exe", "convert", ["source.scp", "output.adf"]);

        var result = await new ConversionBatchExecutor(runner).RunAsync(
            "source.scp",
            [(output, command)],
            progress,
            engine: OperationEngine.GreaseweazleHostTools);

        Assert.Equal(1, runner.CallCount);
        Assert.Equal(1, result.SuccessfulCount);
        Assert.Contains(progress.Lines, line => line.Text.Contains("Host Tools", StringComparison.Ordinal));
    }

    [Fact]
    public async Task InternalConversionDoesNotSilentlyFallBackToHostTools()
    {
        var runner = new RecordingRunner();
        var output = new ConversionOutput("unsupported.format", ".bin", "output.bin", false);
        var command = new GwCommand("gw.exe", "convert", ["source.scp", "output.bin"]);

        var result = await new ConversionBatchExecutor(runner).RunAsync(
            "source.scp",
            [(output, command)],
            engine: OperationEngine.Internal);

        Assert.Equal(0, runner.CallCount);
        Assert.Single(result.FailedLabels);
    }

    [Fact]
    public void InternalConversionFailureDescriptionPreservesSectorDetails()
    {
        var error = new InvalidDataException(
            "Unable to reconstruct the disk image.",
            new InvalidDataException("Track 12, side 1, sector 4 is unreadable."));

        var description = ConversionBatchExecutor.DescribeFailure(error);

        Assert.Contains("Unable to reconstruct the disk image.", description, StringComparison.Ordinal);
        Assert.Contains("Track 12, side 1, sector 4 is unreadable.", description, StringComparison.Ordinal);
    }

    private sealed class RecordingRunner : IGreaseweazleRunner
    {
        public int CallCount { get; private set; }
        public bool IsRunning => false;

        public Task<GwExecutionResult> RunAsync(
            GwCommand command,
            IProgress<GwOutputLine>? progress = null,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new GwExecutionResult(0, false, TimeSpan.Zero, []));
        }
    }

    private sealed class RecordingProgress : IProgress<GwOutputLine>
    {
        public List<GwOutputLine> Lines { get; } = [];

        public void Report(GwOutputLine value) => Lines.Add(value);
    }
}
