using GWGUI.Infrastructure.Commands;
using GWGUI.Infrastructure.Commands.Execution;
using ConversionOutput = global::GWGUI.MediaEngine.Images.Conversion.ConversionOutput;
using GWGUI.App.Parity;
using GWGUI.Infrastructure.Settings.Engines;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Parity;
using System.Diagnostics;
using System.IO;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Sequential;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.App.Functions.Localization;

namespace GWGUI.App.Services.Conversion;

public sealed class ConversionBatchExecutor(
    IGreaseweazleRunner runner,
    MediaImageReadingService mediaReader,
    MediaConversionService conversionService,
    SequentialMediaConversionService sequentialConversionService)
{
    private readonly MediaImageReadingService mediaReader = mediaReader ?? throw new ArgumentNullException(nameof(mediaReader));
    private readonly MediaConversionService conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
    private readonly SequentialMediaConversionService sequentialConversionService = sequentialConversionService ?? throw new ArgumentNullException(nameof(sequentialConversionService));

    public static bool IsInternal(ConversionOutput output) =>
        MediaEngineParityCatalog.Matrix.Rows.Any(row =>
            row.FormatId.Equals(output.FormatId, StringComparison.OrdinalIgnoreCase) &&
            row.TargetContainer.Equals(output.Extension, StringComparison.OrdinalIgnoreCase) &&
            row.IsValidatedFor(MediaParityOperation.Conversion));

    public static bool IsInternal(string sourcePath, ConversionOutput output) =>
        MediaEngineParityCatalog.Matrix.IsValidated(
            Path.GetExtension(sourcePath),
            output.FormatId,
            output.Extension,
            MediaParityOperation.Conversion);

    public async Task<GwBatchExecutionResult> RunAsync(
        string sourcePath,
        IReadOnlyList<(ConversionOutput Output, GwCommand Command)> items,
        IProgress<GwOutputLine>? progress = null,
        Action<GwBatchItem>? itemStarting = null,
        CancellationToken cancellationToken = default,
        OperationEngine engine = OperationEngine.Internal,
        bool acceptSequentialLosses = false)
    {
        var completed = new List<GwBatchItemResult>(items.Count);
        MediaImageDocument? sourceDocument = null;
        Action<MediaExplorationProgress>? conversionProgress = progress is null
            ? null
            : item => Report(progress, GwOutputStream.Standard, item.Detail);
        foreach (var (output, command) in items)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var item = new GwBatchItem(Path.GetFileName(output.OutputPath), command);
            itemStarting?.Invoke(item);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (engine == OperationEngine.GreaseweazleHostTools)
                {
                    Report(progress, GwOutputStream.Standard, LocExtension.Get("Conversion.EngineExternal", item.Label));
                    completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                    continue;
                }

                sourceDocument ??= await mediaReader.ReadAsync(
                    new MediaSourceDescriptor(sourcePath, []),
                    cancellationToken).ConfigureAwait(false);
                var isDirectMediaEngineDestination = conversionService.GetAvailableDestinations(sourceDocument).Any(destination =>
                    destination.FormatId.Equals(output.FormatId, StringComparison.OrdinalIgnoreCase)
                    && destination.Extension.Equals(output.Extension, StringComparison.OrdinalIgnoreCase));
                var isSequentialDestination = false;
                if (!isDirectMediaEngineDestination)
                {
                    var sequentialDestinations = await sequentialConversionService
                        .GetAvailableDestinationsAsync(sourceDocument, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    isSequentialDestination = sequentialDestinations.Any(destination =>
                        destination.FormatId.Equals(output.FormatId, StringComparison.OrdinalIgnoreCase)
                        && destination.Extension.Equals(output.Extension, StringComparison.OrdinalIgnoreCase));
                }
                if (!isDirectMediaEngineDestination && !isSequentialDestination)
                {
                    var line = Report(progress, GwOutputStream.Error, LocExtension.Get("Conversion.EngineInternalUnavailable", item.Label));
                    completed.Add(new(item, new(1, false, stopwatch.Elapsed, [line])));
                    continue;
                }
                Report(progress, GwOutputStream.Standard, LocExtension.Get("Conversion.EngineInternalStart", Path.GetFileName(sourcePath), item.Label));
                if (isSequentialDestination)
                {
                    await sequentialConversionService.ConvertAsync(
                        sourcePath,
                        output.OutputPath,
                        output.FormatId,
                        acceptLosses: acceptSequentialLosses,
                        progress: conversionProgress,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await conversionService.ConvertAsync(
                        new MediaConversionRequest(
                            sourceDocument,
                            output.OutputPath,
                            output.FormatId,
                            progress: conversionProgress),
                        cancellationToken).ConfigureAwait(false);
                }
                Report(progress, GwOutputStream.Standard, LocExtension.Get("Conversion.EngineInternalComplete", item.Label));
                completed.Add(new(item, new(0, false, stopwatch.Elapsed, [])));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completed.Add(new(item, new(-1, true, stopwatch.Elapsed, [])));
                break;
            }
            catch (Exception exception)
            {
                ErrorLog.WriteWithoutPublishing(exception, $"Converting image to {output.FormatId}");
                var line = Report(progress, GwOutputStream.Error, LocExtension.Get(
                    "Conversion.Failed",
                    item.Label,
                    ExceptionDescriptionFunctions.Describe(exception)));
                completed.Add(new(item, new(1, false, stopwatch.Elapsed, [line])));
            }
        }
        return new(completed, cancellationToken.IsCancellationRequested || completed.Any(result => result.Result.WasCancelled));
    }

    private static GwOutputLine Report(IProgress<GwOutputLine>? progress, GwOutputStream stream, string text)
    {
        var line = new GwOutputLine(DateTimeOffset.Now, stream, text);
        progress?.Report(line);
        return line;
    }
}
