using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Services.Logging;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using ConversionOutput = global::GWGUI.MediaEngine.Images.Conversion.ConversionOutput;
using GWGUI.MediaEngine.Images.Formats.Detection;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Enums;
using System.IO;

namespace GWGUI.App.Services.DiskImages.Visualization;

internal sealed class VisualizerLoadingController(
    VisualizerTabSection visualizer,
    Func<AppSettings> getSettings,
    Func<ImageFormatDetector> getFormatDetector,
    Func<GwFormatCapabilities> getCapabilities,
    IGwCommandBuilder commandBuilder,
    IGreaseweazleRunner visualizationRunner,
    DiskImageCancellationScope cancellation,
    MediaImageExplorationService? mediaExploration,
    MediaVisualizationController mediaVisualization,
    ScpVisualizationController scpVisualization,
    Func<ExploredMediaImage?> getExploredMediaImage,
    Action<ExploredMediaImage?> setExploredMediaImage,
    Func<string, CancellationToken, Task<ExploredDiskImage>> analyze,
    Action<IImageDisquette> rememberReadImage,
    Action<ExploredDiskImage> applyScpDetection,
    Action<string> clearVisualizer,
    Func<bool> operationIsRunning)
{
    internal async Task LoadAsync(
        string path,
        string? displayFileName = null,
        ExploredDiskImage? exploredImage = null,
        MediaOpeningAnalysisResult? openingResult = null)
    {
        var visualization = cancellation.BeginVisualization();
        var cancellationToken = visualization.Token;
        if (openingResult?.Document.Representation.RepresentationKind == MediaRepresentationKind.Flux
            && openingResult.DiskExploration.ScpImage is { } loadedScpImage)
        {
            visualizer.Header.ApplyDetection(null, null, [], true);
            try
            {
                await scpVisualization.LoadAsync(path, loadedScpImage, displayFileName);
                cancellationToken.ThrowIfCancellationRequested();
                applyScpDetection(openingResult.DiskExploration);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                ErrorLog.Write(exception, $"Displaying loaded SCP image: {path}");
            }
            return;
        }

        scpVisualization.ResetForNonScp();
        var explored = exploredImage;
        try
        {
            if (openingResult is not null)
            {
                explored = openingResult.DiskExploration;
                rememberReadImage(explored);
                setExploredMediaImage(openingResult.MediaExploration);
            }
            else if (explored is null)
            {
                explored = await analyze(path, cancellationToken);
            }
            else
            {
                rememberReadImage(explored);
                if (mediaExploration is not null && !string.Equals(
                        getExploredMediaImage()?.Document.Source.PrimaryPath,
                        path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    setExploredMediaImage(await mediaExploration.ExploreAsync(
                        path,
                        explored.PrimaryFormatId,
                        cancellationToken));
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            explored = null;
        }

        if (cancellationToken.IsCancellationRequested) return;
        if (explored?.ScpImage is { } exploredScpImage)
        {
            visualizer.Header.ApplyDetection(null, null, [], true);
            try
            {
                await scpVisualization.LoadAsync(path, exploredScpImage, displayFileName);
                cancellationToken.ThrowIfCancellationRequested();
                applyScpDetection(explored);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                ErrorLog.Write(exception, $"Displaying explored SCP image: {path}");
            }
            return;
        }

        bool presented;
        try
        {
            presented = getExploredMediaImage() is { } mediaImage
                && await mediaVisualization.PresentAsync(mediaImage.Document, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (presented)
        {
            scpVisualization.ResetForNonScp();
            scpVisualization.HideProgress();
            return;
        }

        clearVisualizer(displayFileName ?? Path.GetFileName(path));
        if (operationIsRunning()) return;
        cancellationToken.ThrowIfCancellationRequested();

        var detection = getFormatDetector().Detect(path, new FileInfo(path).Length);
        if (!GwVisualizationPolicy.CanConvertToScp(path, detection, getCapabilities())) return;
        var settings = getSettings();
        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath)) return;
        var formatId = detection.Format?.Id ?? DiskImageFormatIds.RawScp;
        var temporaryPath = Path.Combine(
            Path.GetTempPath(),
            $"{DiskImageTemporaryFileConstants.VisualizerFilePrefix}{Guid.NewGuid().ToString(DiskImageTemporaryFileConstants.UniqueNameFormat)}{DiskImageFileExtensions.Scp}");
        string? stagedSourcePath = null;
        var gateEntered = false;
        try
        {
            await cancellation.EnterVisualizationConversionAsync(cancellationToken);
            gateEntered = true;
            cancellationToken.ThrowIfCancellationRequested();
            var conversionSourcePath = path;
            if (Path.GetExtension(path).Equals(DiskImageFileExtensions.Atr, StringComparison.OrdinalIgnoreCase))
            {
                stagedSourcePath = Path.Combine(
                    Path.GetTempPath(),
                    $"{DiskImageTemporaryFileConstants.VisualizerFilePrefix}{Guid.NewGuid().ToString(DiskImageTemporaryFileConstants.UniqueNameFormat)}{DiskImageFileExtensions.Img}");
                await GWGUI.MediaEngine.Images.Conversion.Atari.AtrPayloadWriter.WriteRawPayloadAsync(
                    path,
                    stagedSourcePath,
                    cancellationToken);
                conversionSourcePath = stagedSourcePath;
            }
            var command = commandBuilder.BuildConversion(
                settings.GwExecutablePath,
                conversionSourcePath,
                new ConversionOutput(formatId, DiskImageFileExtensions.Scp, temporaryPath, false));
            var result = await visualizationRunner.RunAsync(command, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!result.IsSuccess || !File.Exists(temporaryPath)) return;
            await scpVisualization.LoadAsync(temporaryPath, displayFileName ?? Path.GetFileName(path));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            if (gateEntered) cancellation.ExitVisualizationConversion();
            TryDelete(stagedSourcePath);
            TryDelete(temporaryPath);
        }
    }

    private static void TryDelete(string? path)
    {
        try { if (path is not null && File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
