using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Contracts.ViewModels.Operations;
using GWGUI.App.Enums.ViewModels.Conversion;
using GWGUI.App.Enums.ViewModels.Operations;
using GWGUI.App.Functions.ViewModels.Conversion;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.Presenters.Operations;
using GWGUI.App.Services.Operations;
using GWGUI.App.Views.Dialogs.Conversion;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Read;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.Domain.Write;
using GWGUI.Emulation.Common;
using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class OperationExecutionTests : CoreTestBase
{
    [Fact]
    public async Task RunnerCapturesUnicodeStandardErrorAndExitCode()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; Write-Output 'café 漢字'; [Console]::Error.WriteLine('échec Ω'); exit 7"]);
        var result = await runner.RunAsync(command);
        Assert.Equal(7, result.ExitCode);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Standard && line.Text.Contains("café 漢字"));
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Error && line.Text.Contains("échec Ω"));
    }

    [Fact]
    public async Task BatchExecutorContinuesAfterFailuresAndKeepsAnExactSummary()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(2, false, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();
        var started = new List<string>();

        var result = await new GwBatchExecutor(runner).RunAsync(items, itemStarting: item => started.Add(item.Label));

        Assert.False(result.WasCancelled);
        Assert.Equal(2, result.SuccessfulCount);
        Assert.Equal(["two"], result.FailedLabels);
        Assert.Equal(["one", "two", "three"], started);
        Assert.Equal(3, runner.Commands.Count);
    }

    [Fact]
    public async Task BatchExecutorStopsImmediatelyAfterACommandReportsCancellation()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(-1, true, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();

        var result = await new GwBatchExecutor(runner).RunAsync(items);

        Assert.True(result.WasCancelled);
        Assert.Equal(1, result.SuccessfulCount);
        Assert.Empty(result.FailedLabels);
        Assert.Equal(2, runner.Commands.Count);
    }

    [Fact]
    public async Task OperationCoordinatorOwnsCancellationAndRejectsConcurrentWork()
    {
        var coordinator = new OperationCoordinator();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operation = coordinator.RunAsync(async token =>
        {
            started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return 1;
        });
        await started.Task;

        Assert.True(coordinator.IsRunning);
        var concurrent = await coordinator.RunAsync(_ => Task.FromResult(2));
        Assert.IsType<InvalidOperationException>(concurrent.Error);
        coordinator.RequestCancellation();
        var outcome = await operation;

        Assert.IsType<TaskCanceledException>(outcome.Error);
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public async Task OperationCoordinatorReturnsSuccessAndFailureAsExplicitOutcomes()
    {
        var coordinator = new OperationCoordinator();

        var success = await coordinator.RunAsync(_ => Task.FromResult(42));
        var failure = await coordinator.RunAsync<int>(_ => throw new IOException("broken"));

        Assert.True(success.HasResult);
        Assert.Equal(42, success.Result);
        Assert.Null(success.Error);
        Assert.False(failure.HasResult);
        Assert.Equal("broken", Assert.IsType<IOException>(failure.Error).Message);
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public async Task OperationCoordinatorSignalsCompletionOnlyAfterCancelledWorkHasStopped()
    {
        var coordinator = new OperationCoordinator();
        var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var running = coordinator.RunAsync<int>(async cancellationToken =>
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            finally
            {
                stopped.SetResult();
            }
            return 0;
        });
        await Task.Yield();

        coordinator.RequestCancellation();
        await coordinator.WaitForCompletionAsync();

        Assert.True(stopped.Task.IsCompleted);
        Assert.True((await running).Error is OperationCanceledException);
    }

    [Fact]
    public void OperationResultPresenterDistinguishesSingleSuccessFailureAndCancellation()
    {
        var presenter = new OperationResultPresenter();
        static OperationOutcome<GwExecutionResult> Outcome(int code, bool cancelled = false) =>
            new(true, new GwExecutionResult(code, cancelled, TimeSpan.FromSeconds(2), []), null);

        var success = presenter.Present(Outcome(0));
        var failure = presenter.Present(Outcome(3));
        var cancelled = presenter.Present(Outcome(-1, true));

        Assert.Equal(OperationResultState.Success, success.State);
        Assert.Equal(OperationResultState.Error, failure.State);
        Assert.Equal(OperationResultState.Cancelled, cancelled.State);
        Assert.Equal(["Operation.Succeeded", "Operation.Finished"], success.Messages.Select(message => message.ResourceKey));
        Assert.Equal([0, "0:00:02"], success.Messages[1].Arguments);
        Assert.All(success.Messages, message => Assert.True(message.StartOnNewLine));
    }

    [Fact]
    public void OperationResultPresenterBuildsExactPartialBatchSummary()
    {
        var command = new GwCommand("gw.exe", "convert", []);
        var items = new[]
        {
            new GwBatchItemResult(new GwBatchItem("disk.ima", command), new GwExecutionResult(0, false, TimeSpan.Zero, [])),
            new GwBatchItemResult(new GwBatchItem("disk.img", command), new GwExecutionResult(2, false, TimeSpan.Zero, []))
        };

        var presentation = new OperationResultPresenter().Present(new OperationOutcome<GwBatchExecutionResult>(true, new(items, false), null));

        Assert.Equal(OperationResultState.Error, presentation.State);
        Assert.Collection(presentation.Messages,
            summary => { Assert.Equal("Conversion.Summary", summary.ResourceKey); Assert.Equal([1, 1], summary.Arguments); Assert.True(summary.StartOnNewLine); },
            failures => { Assert.Equal("Conversion.Failures", failures.ResourceKey); Assert.Equal(["disk.img"], failures.Arguments); Assert.False(failures.StartOnNewLine); });
    }

    [Fact]
    public void OperationResultPresenterTurnsThrownExceptionsIntoLocalizedErrors()
    {
        var presentation = new OperationResultPresenter().Present(new OperationOutcome<GwExecutionResult>(false, null, new IOException("broken")));

        Assert.Equal(OperationResultState.Error, presentation.State);
        var message = Assert.Single(presentation.Messages);
        Assert.Equal("Error.Unexpected", message.ResourceKey);
        var detail = Assert.IsType<string>(Assert.Single(message.Arguments));
        Assert.DoesNotContain("broken", detail, StringComparison.Ordinal);
        Assert.False(message.StartOnNewLine);
    }

    [Fact]
    public void ConversionConflictResolverAppliesSkipOverwriteAndNumberChoices()
    {
        var untouched = new ConversionOutput("ibm.720", ".ima", "plain.ima", true);
        var overwrite = new ConversionOutput("ibm.720", ".img", "replace.img", false);
        var skip = new ConversionOutput("atarist.720", ".st", "skip.st", true);
        var number = new ConversionOutput("amiga.amigados", ".adf", "number.adf", true);
        var conflicts = new[] { overwrite, skip, number };
        var decisions = new[]
        {
            new ConversionConflictDecision(overwrite, ConversionConflictChoice.Overwrite),
            new ConversionConflictDecision(skip, ConversionConflictChoice.Skip),
            new ConversionConflictDecision(number, ConversionConflictChoice.Number)
        };

        var resolved = ConversionConflictResolutionFunctions.Apply([untouched, overwrite, skip, number], conflicts, decisions, path => "next-" + path);

        Assert.Equal([untouched, overwrite, number with { OutputPath = "next-number.adf" }], resolved);
    }

    [Fact]
    public async Task RunnerRejectsASecondConcurrentCommand()
    {
        var runner = new GreaseweazleRunner();
        using var cancellation = new CancellationTokenSource();
        var first = runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "Start-Sleep -Seconds 20"]), cancellationToken: cancellation.Token);
        Assert.True(SpinWait.SpinUntil(() => runner.IsRunning, TimeSpan.FromSeconds(2)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "exit 0"])));
        cancellation.Cancel();
        Assert.True((await first).WasCancelled);
        Assert.False(runner.IsRunning);
    }

    [Fact]
    public async Task RunnerReassemblesAFragmentedUtf8Line()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; [Console]::Out.Write('frag'); Start-Sleep -Milliseconds 50; [Console]::Out.WriteLine('menté')"]);
        var result = await runner.RunAsync(command);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Text == "fragmenté");
    }

    [Fact]
    public void ConversionCompatibilityUsesTheDetectedGeometryForSectorImages()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.ima", 737280);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".ima", detection);
        Assert.Equal(["raw.scp", "ibm.720"], outputs.Select(output => output.Id));
    }

    [Fact]
    public void ConversionCompatibilityKeepsAllDecodableFormatsForRawFlux()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.scp", 1234);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".scp", detection);
        Assert.Contains(outputs, output => output.Id == "amiga.amigados");
        Assert.Contains(outputs, output => output.Id == "atarist.720");
        Assert.Contains(outputs, output => output.Id == "ibm.720");
    }

    [Fact]
    public void ConversionFormatPresenterPinsSelectionsAndReturnsUncheckedItemsToTheirNaturalGroup()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var rare = catalog.Formats.First(format => format.Id != "raw.scp" && !format.IsCommon);
        var selected = new HashSet<string> { "ibm.720", rare.Id };
        var extensions = new Dictionary<string, HashSet<string>> { ["ibm.720"] = [".img"] };
        var presenter = new ConversionFormatPresenter();

        var pinned = presenter.Build(catalog, null, null, selected, extensions);

        Assert.Equal(2, pinned.TakeWhile(item => item.Group == ConversionFormatGroup.Selected).Count());
        Assert.All(pinned.Take(2), item => Assert.True(item.IsSelected));
        Assert.True(pinned.Single(item => item.Format.Id == "ibm.720").ExplicitExtensions.SetEquals([".img"]));

        var unselected = presenter.Build(catalog, null, null, new HashSet<string>(), extensions);
        Assert.Equal(ConversionFormatGroup.Common, unselected.Single(item => item.Format.Id == "ibm.720").Group);
        Assert.Equal(ConversionFormatGroup.Rare, unselected.Single(item => item.Format.Id == rare.Id).Group);
        Assert.Equal(unselected.OrderBy(item => item.Group).ThenBy(item => item.Format.DisplayName, StringComparer.CurrentCulture), unselected);
    }

    [Fact]
    public void ConversionFormatPresenterDisablesSelectionsThatDoNotMatchDetectedSectorGeometry()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.ima", 737280);
        var selected = new HashSet<string> { "ibm.720", "atarist.720" };

        var items = new ConversionFormatPresenter().Build(catalog, ".ima", detection, selected, new Dictionary<string, HashSet<string>>());

        Assert.True(items.Single(item => item.Format.Id == "ibm.720").IsSelected);
        var incompatible = items.Single(item => item.Format.Id == "atarist.720");
        Assert.False(incompatible.IsCompatible);
        Assert.False(incompatible.IsSelected);
        Assert.NotEqual(ConversionFormatGroup.Selected, incompatible.Group);
        var scp = items.Single(item => item.Format.Id == "raw.scp");
        Assert.True(scp.IsCompatible);
        Assert.True(scp.IsReconstructedFlux);
    }

    [Fact]
    public void RawScpReadNeverAddsAStaleKnownFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, "acorn.adfs.800", []));
        Assert.DoesNotContain("--format", command.Arguments);
        Assert.Equal(["disk.scp"], command.Arguments);
    }

    [Fact]
    public void KnownFormatReadRequiresAndAddsItsFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, "amiga.amigados", []));
        Assert.Equal(["--format", "amiga.amigados", "disk.adf"], command.Arguments);
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, null, [])));
    }

    [Fact]
    public void ADriveArgumentIsOnlyUsedWhenSeveralDrivesAreConfigured()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var second = new DriveSettings { ControllerUsbId = "GW-1", Selection = "B" };
        Assert.Null(HardwareRoutingPolicy.DriveArgument([first], first));
        Assert.Equal("B", HardwareRoutingPolicy.DriveArgument([first, second], second));
    }

    [Fact]
    public void OneDriveOnEachControllerDoesNotEmitAnUnnecessaryDriveArgument()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var second = new DriveSettings { ControllerUsbId = "GW-2", Selection = "B" };

        Assert.Null(HardwareRoutingPolicy.DriveArgument([first, second], first));
        Assert.Null(HardwareRoutingPolicy.DriveArgument([first, second], second));
    }

    [Fact]
    public void DeviceArgumentIsOnlyUsedWhenSeveralConfiguredControllersAreAvailable()
    {
        var firstController = new ControllerSettings { UsbId = "GW-1", LastPort = "COM3", IsAvailable = true };
        var secondController = new ControllerSettings { UsbId = "GW-2", LastPort = "COM5", IsAvailable = true };
        var firstDrive = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var secondDrive = new DriveSettings { ControllerUsbId = "GW-2", Selection = "A" };

        Assert.Null(HardwareRoutingPolicy.DeviceArgument([firstController], [firstDrive], firstDrive));
        Assert.Equal("COM5", HardwareRoutingPolicy.DeviceArgument([firstController, secondController], [firstDrive, secondDrive], secondDrive));

        secondController.IsAvailable = false;
        Assert.Null(HardwareRoutingPolicy.DeviceArgument([firstController, secondController], [firstDrive, secondDrive], firstDrive));
    }

    [Fact]
    public void AutomaticDriveSelectionAssignsHiddenAAndBPerController()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "legacy" };
        var second = new DriveSettings { ControllerUsbId = "GW-1", Selection = "legacy" };
        var other = new DriveSettings { ControllerUsbId = "GW-2", Selection = "legacy" };
        var drives = new List<DriveSettings> { first, second, other };

        HardwareRoutingPolicy.AssignAutomaticDriveSelections(drives, "GW-1");

        Assert.Equal("A", first.Selection);
        Assert.Equal("B", second.Selection);
        Assert.Equal("legacy", other.Selection);
    }

    [Theory]
    [InlineData("A", 0)]
    [InlineData("Z", 25)]
    [InlineData("AA", 26)]
    [InlineData("AB", 27)]
    public void AlphabeticSequenceInputParsesLikeItsDisplayedValue(string text, long expected)
    {
        Assert.True(SequenceFormatter.TryParse(text, SequenceKind.Alphabetic, out var value));
        Assert.Equal(expected, value);
        Assert.Equal(text, SequenceFormatter.Format(value, SequenceKind.Alphabetic, 1));
    }

    [Fact]
    public void RawContainerIdsAreNeverSentAsGwFormatArguments()
    {
        var write = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.scp", "raw.scp", []));
        Assert.Equal(["disk.scp"], write.Arguments);
        var convert = ConversionCommandBuilder.Build("gw.exe", "disk.scp", new ConversionOutput("raw.hfe", ".hfe", "disk.hfe", true));
        Assert.Equal(["disk.scp", "disk.hfe"], convert.Arguments);
        Assert.Equal("raw.gcr", GwFormatArgument.FromCatalogId("raw.gcr"));
    }
}
