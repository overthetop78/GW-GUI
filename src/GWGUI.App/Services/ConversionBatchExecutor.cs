using System.Diagnostics;
using System.IO;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Conversion;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Conversion.Amiga;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Conversion.Msx;
using GWGUI.MediaEngine.Conversion.Acorn;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Conversion.Commodore;
using GWGUI.MediaEngine.Conversion.Amstrad;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.App.Services;

public sealed class ConversionBatchExecutor(
    IGreaseweazleRunner runner,
    AmigaAdfConversionService? amigaAdf = null,
    IbmRawConversionService? ibmRaw = null,
    MsxRawConversionService? msxRaw = null,
    AcornAdfConversionService? acornAdf = null,
    BbcDfsConversionService? bbcDfs = null,
    AppleRwts18ConversionService? appleRwts18 = null,
    AtariStConversionService? atariSt = null,
    D81ConversionService? d81 = null,
    AtrConversionService? atr = null,
    CommodoreDosConversionService? commodoreDos = null,
    AppleSectorConversionService? appleSector = null,
    AmstradDskConversionService? amstradDsk = null)
{
    private readonly AmigaAdfConversionService _amigaAdf = amigaAdf ?? MediaEngineFactory.CreateAmigaAdfConversionService();
    private readonly IbmRawConversionService _ibmRaw = ibmRaw ?? MediaEngineFactory.CreateIbmRawConversionService();
    private readonly MsxRawConversionService _msxRaw = msxRaw ?? MediaEngineFactory.CreateMsxRawConversionService();
    private readonly AcornAdfConversionService _acornAdf = acornAdf ?? MediaEngineFactory.CreateAcornAdfConversionService();
    private readonly BbcDfsConversionService _bbcDfs = bbcDfs ?? MediaEngineFactory.CreateBbcDfsConversionService();
    private readonly AppleRwts18ConversionService _appleRwts18 = appleRwts18 ?? MediaEngineFactory.CreateAppleRwts18ConversionService();
    private readonly AtariStConversionService _atariSt = atariSt ?? MediaEngineFactory.CreateAtariStConversionService();
    private readonly D81ConversionService _d81 = d81 ?? MediaEngineFactory.CreateD81ConversionService();
    private readonly AtrConversionService _atr = atr ?? MediaEngineFactory.CreateAtrConversionService();
    private readonly CommodoreDosConversionService _commodoreDos = commodoreDos ?? MediaEngineFactory.CreateCommodoreDosConversionService();
    private readonly AppleSectorConversionService _appleSector = appleSector ?? MediaEngineFactory.CreateAppleSectorConversionService();
    private readonly AmstradDskConversionService _amstradDsk = amstradDsk ?? MediaEngineFactory.CreateAmstradDskConversionService();

    public static bool IsInternal(ConversionOutput output) =>
        AmigaAdfConversionService.CanCreate(output.FormatId, output.Extension) || AcornAdfConversionService.CanCreate(output.FormatId, output.Extension) || BbcDfsConversionService.CanCreate(output.FormatId, output.Extension) || IbmRawConversionService.CanCreate(output.FormatId, output.Extension) || MsxRawConversionService.CanCreate(output.FormatId, output.Extension) || AppleSectorConversionService.CanCreate(output.FormatId, output.Extension) || AppleRwts18ConversionService.CanCreate(output.FormatId, output.Extension) || AtariStConversionService.CanCreate(output.FormatId, output.Extension) || D81ConversionService.CanCreate(output.FormatId, output.Extension) || AtrConversionService.CanCreate(output.FormatId, output.Extension) || CommodoreDosConversionService.CanCreate(output.FormatId, output.Extension) || AmstradDskConversionService.CanCreate(output.FormatId, output.Extension);

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
                else if (AcornAdfConversionService.CanCreate(output.FormatId, output.Extension))
                    await _acornAdf.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (BbcDfsConversionService.CanCreate(output.FormatId, output.Extension))
                    await _bbcDfs.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (IbmRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _ibmRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (MsxRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _msxRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AtariStConversionService.CanCreate(output.FormatId, output.Extension))
                    await _atariSt.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (D81ConversionService.CanCreate(output.FormatId, output.Extension))
                    await _d81.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AtrConversionService.CanCreate(output.FormatId, output.Extension))
                    await _atr.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (CommodoreDosConversionService.CanCreate(output.FormatId, output.Extension))
                    await _commodoreDos.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AmstradDskConversionService.CanCreate(output.FormatId, output.Extension))
                    await _amstradDsk.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AppleSectorConversionService.CanCreate(output.FormatId, output.Extension))
                    await _appleSector.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else
                {
                    try
                    {
                        await _appleRwts18.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                    }
                    catch (InvalidDataException) when (AppleRwts18ConversionService.IsCatalogAliasTarget(output.FormatId, output.Extension))
                    {
                        completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                        continue;
                    }
                }
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
