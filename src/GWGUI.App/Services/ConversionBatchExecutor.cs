using System.Diagnostics;
using System.IO;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Conversion;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Conversion.Amiga;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Conversion.Msx;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.App.Services;

public sealed class ConversionBatchExecutor(
    IGreaseweazleRunner runner,
    AmigaAdfConversionService? amigaAdf = null,
    IbmRawConversionService? ibmRaw = null,
    MsxRawConversionService? msxRaw = null,
    AppleRwts18ConversionService? appleRwts18 = null,
    AtariStConversionService? atariSt = null)
{
    private readonly AmigaAdfConversionService _amigaAdf = amigaAdf ?? MediaEngineFactory.CreateAmigaAdfConversionService();
    private readonly IbmRawConversionService _ibmRaw = ibmRaw ?? MediaEngineFactory.CreateIbmRawConversionService();
    private readonly MsxRawConversionService _msxRaw = msxRaw ?? MediaEngineFactory.CreateMsxRawConversionService();
    private readonly AppleRwts18ConversionService _appleRwts18 = appleRwts18 ?? MediaEngineFactory.CreateAppleRwts18ConversionService();
    private readonly AtariStConversionService _atariSt = atariSt ?? MediaEngineFactory.CreateAtariStConversionService();

    public static bool IsInternal(ConversionOutput output) =>
        AmigaAdfConversionService.CanCreate(output.FormatId, output.Extension) || IbmRawConversionService.CanCreate(output.FormatId, output.Extension) || MsxRawConversionService.CanCreate(output.FormatId, output.Extension) || AppleRwts18ConversionService.CanCreate(output.FormatId, output.Extension) || AtariStConversionService.CanCreate(output.FormatId, output.Extension);

    public async Task<GwBatchExecutionResult> RunAsync(
        string sourcePath,
        IReadOnlyList<(ConversionOutput Output, GwCommand Command)> items,
        IProgress<GwOutputLine>? progress = null,
        Action<GwBatchItem>? itemStarting = null,
        CancellationToken cancellationToken = default)
    {
        var completed = new List<GwBatchItemResult>(items.Count);
        foreach (var (output, command) in items)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var item = new GwBatchItem(Path.GetFileName(output.OutputPath), command);
            itemStarting?.Invoke(item);
            if (!IsInternal(output))
            {
                completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                continue;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (AmigaAdfConversionService.CanCreate(output.FormatId, output.Extension))
                    await _amigaAdf.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (IbmRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _ibmRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (MsxRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _msxRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AtariStConversionService.CanCreate(output.FormatId, output.Extension))
                    await _atariSt.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else
                    await _appleRwts18.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                completed.Add(new(item, new(0, false, stopwatch.Elapsed, [])));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completed.Add(new(item, new(-1, true, stopwatch.Elapsed, [])));
                break;
            }
            catch (Exception exception)
            {
                ErrorLog.Write(exception, $"Converting image to {output.FormatId}");
                completed.Add(new(item, new(1, false, stopwatch.Elapsed, [])));
            }
        }
        return new(completed, cancellationToken.IsCancellationRequested || completed.Any(result => result.Result.WasCancelled));
    }
}
