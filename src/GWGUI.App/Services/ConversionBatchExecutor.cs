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
using GWGUI.MediaEngine.Conversion.Epson;
using GWGUI.MediaEngine.Conversion.Dec;
using GWGUI.MediaEngine.Conversion.Ucsd;
using GWGUI.MediaEngine.Conversion.Hfe;
using GWGUI.MediaEngine.Conversion.Flux;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.App.Services;

public sealed class ConversionBatchExecutor(
    IGreaseweazleRunner runner,
    AmigaAdfConversionService? amigaAdf = null,
    IbmRawConversionService? ibmRaw = null,
    MsxRawConversionService? msxRaw = null,
    AcornAdfConversionService? acornAdf = null,
    BbcDfsConversionService? bbcDfs = null,
    AppleNibbleConversionService? appleNibble = null,
    AtariStConversionService? atariSt = null,
    D81ConversionService? d81 = null,
    AtrConversionService? atr = null,
    CommodoreDosConversionService? commodoreDos = null,
    AppleSectorConversionService? appleSector = null,
    AmstradDskConversionService? amstradDsk = null,
    EpsonQx10ConversionService? epsonQx10 = null,
    DecRx02ConversionService? decRx02 = null,
    UcsdImgConversionService? ucsdImg = null,
    CoherentConversionService? coherent = null,
    MacintoshConversionService? macintosh = null,
    LisaConversionService? lisa = null,
    HfeConversionService? hfe = null,
    FluxContainerConversionService? flux = null,
    SectorImageScpFileConversionService? scp = null)
{
    private readonly AmigaAdfConversionService _amigaAdf = amigaAdf ?? MediaEngineFactory.CreateAmigaAdfConversionService();
    private readonly IbmRawConversionService _ibmRaw = ibmRaw ?? MediaEngineFactory.CreateIbmRawConversionService();
    private readonly MsxRawConversionService _msxRaw = msxRaw ?? MediaEngineFactory.CreateMsxRawConversionService();
    private readonly AcornAdfConversionService _acornAdf = acornAdf ?? MediaEngineFactory.CreateAcornAdfConversionService();
    private readonly BbcDfsConversionService _bbcDfs = bbcDfs ?? MediaEngineFactory.CreateBbcDfsConversionService();
    private readonly AppleNibbleConversionService _appleNibble = appleNibble ?? MediaEngineFactory.CreateAppleNibbleConversionService();
    private readonly AtariStConversionService _atariSt = atariSt ?? MediaEngineFactory.CreateAtariStConversionService();
    private readonly D81ConversionService _d81 = d81 ?? MediaEngineFactory.CreateD81ConversionService();
    private readonly AtrConversionService _atr = atr ?? MediaEngineFactory.CreateAtrConversionService();
    private readonly CommodoreDosConversionService _commodoreDos = commodoreDos ?? MediaEngineFactory.CreateCommodoreDosConversionService();
    private readonly AppleSectorConversionService _appleSector = appleSector ?? MediaEngineFactory.CreateAppleSectorConversionService();
    private readonly AmstradDskConversionService _amstradDsk = amstradDsk ?? MediaEngineFactory.CreateAmstradDskConversionService();
    private readonly EpsonQx10ConversionService _epsonQx10 = epsonQx10 ?? MediaEngineFactory.CreateEpsonQx10ConversionService();
    private readonly DecRx02ConversionService _decRx02 = decRx02 ?? MediaEngineFactory.CreateDecRx02ConversionService();
    private readonly UcsdImgConversionService _ucsdImg = ucsdImg ?? MediaEngineFactory.CreateUcsdImgConversionService();
    private readonly CoherentConversionService _coherent = coherent ?? MediaEngineFactory.CreateCoherentConversionService();
    private readonly MacintoshConversionService _macintosh = macintosh ?? MediaEngineFactory.CreateMacintoshConversionService();
    private readonly LisaConversionService _lisa = lisa ?? MediaEngineFactory.CreateLisaConversionService();
    private readonly HfeConversionService _hfe = hfe ?? MediaEngineFactory.CreateHfeConversionService();
    private readonly FluxContainerConversionService _flux = flux ?? MediaEngineFactory.CreateFluxContainerConversionService();
    private readonly SectorImageScpFileConversionService _scp = scp ?? MediaEngineFactory.CreateSectorImageScpFileConversionService();

    public static bool IsInternal(ConversionOutput output) =>
        SectorImageScpFileConversionService.CanCreate(output.FormatId, output.Extension) || AmigaAdfConversionService.CanCreate(output.FormatId, output.Extension) || AcornAdfConversionService.CanCreate(output.FormatId, output.Extension) || BbcDfsConversionService.CanCreate(output.FormatId, output.Extension) || IbmRawConversionService.CanCreate(output.FormatId, output.Extension) || MsxRawConversionService.CanCreate(output.FormatId, output.Extension) || AppleSectorConversionService.CanCreate(output.FormatId, output.Extension) || AppleNibbleConversionService.CanCreate(output.FormatId, output.Extension) || MacintoshConversionService.CanCreate(output.FormatId, output.Extension) || LisaConversionService.CanCreate(output.FormatId, output.Extension) || HfeConversionService.CanCreate(output.FormatId, output.Extension) || AtariStConversionService.CanCreate(output.FormatId, output.Extension) || D81ConversionService.CanCreate(output.FormatId, output.Extension) || AtrConversionService.CanCreate(output.FormatId, output.Extension) || CommodoreDosConversionService.CanCreate(output.FormatId, output.Extension) || CoherentConversionService.CanCreate(output.FormatId, output.Extension) || AmstradDskConversionService.CanCreate(output.FormatId, output.Extension) || EpsonQx10ConversionService.CanCreate(output.FormatId, output.Extension) || DecRx02ConversionService.CanCreate(output.FormatId, output.Extension) || UcsdImgConversionService.CanCreate(output.FormatId, output.Extension);

    public static bool IsInternal(string sourcePath, ConversionOutput output) =>
        FluxContainerConversionService.CanConvert(sourcePath, output.FormatId, output.Extension) ||
        IsInternal(output);

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
            if (!IsInternal(sourcePath, output))
            {
                completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                continue;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (FluxContainerConversionService.CanConvert(sourcePath, output.FormatId, output.Extension))
                {
                    try
                    {
                        await _flux.ConvertAsync(sourcePath, output.OutputPath, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (NotSupportedException) when (!output.PreservesOriginalProtection)
                    {
                        completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                        continue;
                    }
                }
                else if (SectorImageScpFileConversionService.CanCreate(output.FormatId, output.Extension))
                    await _scp.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                else if (AmigaAdfConversionService.CanCreate(output.FormatId, output.Extension))
                    await _amigaAdf.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AcornAdfConversionService.CanCreate(output.FormatId, output.Extension))
                    await _acornAdf.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (BbcDfsConversionService.CanCreate(output.FormatId, output.Extension))
                    await _bbcDfs.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (IbmRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _ibmRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (MsxRawConversionService.CanCreate(output.FormatId, output.Extension))
                    await _msxRaw.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (MacintoshConversionService.CanCreate(output.FormatId, output.Extension))
                    await _macintosh.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (LisaConversionService.CanCreate(output.FormatId, output.Extension))
                    await _lisa.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (HfeConversionService.CanCreate(output.FormatId, output.Extension))
                {
                    try { await _hfe.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false); }
                    catch (NotSupportedException)
                    {
                        completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                        continue;
                    }
                }
                else if (AtariStConversionService.CanCreate(output.FormatId, output.Extension))
                    await _atariSt.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (D81ConversionService.CanCreate(output.FormatId, output.Extension))
                    await _d81.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (AtrConversionService.CanCreate(output.FormatId, output.Extension))
                    await _atr.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (CommodoreDosConversionService.CanCreate(output.FormatId, output.Extension))
                    await _commodoreDos.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (CoherentConversionService.CanCreate(output.FormatId, output.Extension))
                    await _coherent.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                else if (AmstradDskConversionService.CanCreate(output.FormatId, output.Extension))
                    await _amstradDsk.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (EpsonQx10ConversionService.CanCreate(output.FormatId, output.Extension))
                    await _epsonQx10.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else if (DecRx02ConversionService.CanCreate(output.FormatId, output.Extension))
                    await _decRx02.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                else if (UcsdImgConversionService.CanCreate(output.FormatId, output.Extension))
                    await _ucsdImg.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                else if (AppleSectorConversionService.CanCreate(output.FormatId, output.Extension))
                    await _appleSector.ConvertAsync(sourcePath, output.OutputPath, output.FormatId, cancellationToken).ConfigureAwait(false);
                else
                {
                    try
                    {
                        await _appleNibble.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                    }
                    catch (InvalidDataException) when (AppleNibbleConversionService.IsCatalogAliasTarget(output.FormatId, output.Extension))
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
