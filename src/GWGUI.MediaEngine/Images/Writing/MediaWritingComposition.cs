using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Acorn;
using GWGUI.MediaEngine.Images.Conversion.Apple;
using GWGUI.MediaEngine.Images.Conversion.Atari;
using GWGUI.MediaEngine.Images.Conversion.Commodore;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Images.Formats.Floppy.BbcDfs;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.CpcDsk;
using GWGUI.MediaEngine.Images.Formats.Floppy.D81;
using GWGUI.MediaEngine.Images.Formats.Floppy.DiskCopy;
using GWGUI.MediaEngine.Images.Formats.Floppy.Hfe;
using GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Msa;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Rx02;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Images.Formats.Floppy.TeleDisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.TwoImg;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Raw;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Chd;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Qcow2;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vdi;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vhd;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vhdx;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vmdk;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Images.Formats.Optical.Iso;
using GWGUI.MediaEngine.Images.Formats.Tape.AtariCas;
using GWGUI.MediaEngine.Images.Formats.Tape.CommodoreTap;
using GWGUI.MediaEngine.Images.Formats.Tape.MsxCas;
using GWGUI.MediaEngine.Images.Formats.Tape.Simh;
using GWGUI.MediaEngine.Images.Formats.Tape.SpectrumTap;
using GWGUI.MediaEngine.Images.Formats.Tape.Tzx;
using GWGUI.MediaEngine.Images.Formats.Tape.Uef;
using GWGUI.MediaEngine.Images.Formats.Tape.Wav;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Writing;

namespace GWGUI.MediaEngine.Images.Writing;

/// <summary>Provides the registered media image writers and their writing service.</summary>
public sealed class MediaWritingComposition
{
    private static readonly BuiltInImageFormatCatalog Formats = new();

    private MediaWritingComposition(IReadOnlyList<IMediaImageWriter> writers)
    {
        Writers = writers;
        Registry = new MediaImageWriterRegistry(writers);
        WritingService = new MediaImageWritingService(Registry);
    }

    public IReadOnlyList<IMediaImageWriter> Writers { get; }

    public MediaImageWriterRegistry Registry { get; }

    public MediaImageWritingService WritingService { get; }

    public static MediaWritingComposition CreateDefault()
    {
        var acornAdf = new AcornAdfWriter();
        var amigaAdf = new AmigaAdfWriter();
        var bbcDfs = new BbcDfsImageWriter();
        var ibmRaw = new IbmRawImageWriter();
        var msxRaw = new MsxRawImageWriter();
        var appleRaw = new AppleRawImageWriter();
        var twoImg = new TwoImgWriter();
        var appleNibble = new AppleDiskImageWriter();
        var macintoshRaw = new MacintoshRawImageWriter();
        var diskCopy = new DiskCopyWriter();
        var atariSt = new AtariStWriter(new LinearSectorImageWriter());
        var msa = new MsaWriter();
        var atr = new AtrWriter();
        var d81 = new D81Writer(new LinearSectorImageWriter());
        var commodoreDos = new CommodoreDosContainerWriter();
        var coherent = new CoherentRawImageWriter();
        var amstrad = new CpcDskWriter();
        var epsonRaw = new EpsonQx10RawImageWriter();
        var imd = new ImdWriter();
        var decRx02 = new DecRx02Writer();
        var linear = new LinearSectorImageWriter();
        var td0 = new Td0Writer();
        var scp = new ScpWriter();
        var hfe = new HfeWriter();
        var hardDiskRaw = new RawHardDiskWriter();
        var hardDiskQcow2 = new Qcow2Writer();
        var hardDiskVhd = new VhdWriter();
        var hardDiskVhdx = new VhdxWriter();
        var hardDiskVdi = new VdiWriter();
        var hardDiskVmdk = new VmdkWriter();
        var hardDiskChd = new ChdHardDiskWriter();
        var opticalIso = new IsoWriter();
        var opticalBinCue = new BinCueWriter();
        var tapeWav = new WavTapeWriter();
        var tapeAtariCas = new AtariCasWriter();
        var tapeTzx = new TzxWriter();
        var tapeSpectrumTap = new SpectrumTapWriter();
        var tapeCommodoreTap = new CommodoreTapWriter();
        var tapeMsxCas = new MsxCasWriter();
        var tapeUef = new UefWriter();
        var tapeSimh = new SimhTapeWriter();

        IMediaImageWriter[] writers =
        [
            Sector(MediaImageWriterIds.AcornAdf, [DiskImageFileExtensions.Adf], AcornAdfConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(acornAdf.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.AmigaAdf, [DiskImageFileExtensions.Adf], AmigaAdfConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(amigaAdf.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.BbcDfs, [DiskImageFileExtensions.Ssd, DiskImageFileExtensions.Dsd], BbcDfsConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteSingleAsync(bbcDfs.WriteAsync(image, path, formatId, token), path)),
            Sector(MediaImageWriterIds.IbmRaw, [DiskImageFileExtensions.Ima, DiskImageFileExtensions.Img], IbmRawConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteSingleAsync(ibmRaw.WriteAsync(image, path, formatId, token), path)),
            Sector(MediaImageWriterIds.MsxRaw, [DiskImageFileExtensions.Dsk], MsxRawConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteSingleAsync(msxRaw.WriteAsync(image, path, formatId, token), path)),
            Sector(MediaImageWriterIds.AppleSector, [DiskImageFileExtensions.TwoMg, DiskImageFileExtensions.D13, DiskImageFileExtensions.Do, DiskImageFileExtensions.Dsk, DiskImageFileExtensions.Po], AppleSectorConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteAppleSectorAsync(appleRaw, twoImg, image, path, formatId, token)),
            Sector(MediaImageWriterIds.AppleNibble, [DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz], AppleNibbleConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(appleNibble.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.Macintosh, [DiskImageFileExtensions.Img, DiskImageFileExtensions.Image, DiskImageFileExtensions.Dc42], MacintoshConversionService.CanCreate,
                (image, document, path, _, token) => WriteMacintoshAsync(macintoshRaw, diskCopy, image, document, path, token)),
            Sector(MediaImageWriterIds.LisaDiskCopy, [DiskImageFileExtensions.Image, DiskImageFileExtensions.Dc42], LisaConversionService.CanCreate,
                (image, document, path, _, token) => WriteSingleAsync(diskCopy.WriteAsync(image, path, DiskCopyMetadataFunctions.Restore(image, document.Metadata), token), path)),
            Sector(MediaImageWriterIds.AtariSt, [DiskImageFileExtensions.St, DiskImageFileExtensions.Msa], AtariStConversionService.CanCreate,
                (image, document, path, formatId, token) => WriteAtariStAsync(atariSt, msa, image, document, path, formatId, token)),
            Sector(MediaImageWriterIds.AtariAtr, [DiskImageFileExtensions.Atr], AtrConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteSingleAsync(atr.WriteAsync(image, path, formatId, token), path)),
            Sector(MediaImageWriterIds.CommodoreD81, [DiskImageFileExtensions.D81], D81ConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(d81.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.CommodoreDos, [DiskImageFileExtensions.D64, DiskImageFileExtensions.D71], CommodoreDosConversionService.CanCreate,
                (image, _, path, formatId, token) => WriteCommodoreDosAsync(commodoreDos, image, path, formatId, token)),
            Sector(MediaImageWriterIds.CoherentRaw, [DiskImageFileExtensions.Bin, DiskImageFileExtensions.Img], CoherentConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(coherent.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.AmstradDsk, [DiskImageFileExtensions.Dsk, DiskImageFileExtensions.Edsk], AmstradDskConversionService.CanCreate,
                (image, _, path, _, token) => WriteAmstradAsync(amstrad, image, path, token)),
            Sector(MediaImageWriterIds.EpsonQx10, [DiskImageFileExtensions.Img, DiskImageFileExtensions.Imd], EpsonQx10ConversionService.CanCreate,
                (image, document, path, formatId, token) => WriteEpsonAsync(epsonRaw, imd, image, document, path, formatId, token)),
            Sector(MediaImageWriterIds.DecRx02, [DiskImageFileExtensions.Img], DecRx02ConversionService.CanCreate,
                (image, _, path, _, token) => WriteSingleAsync(decRx02.WriteAsync(image, path, token), path)),
            Sector(MediaImageWriterIds.Ucsd, [DiskImageFileExtensions.Img, DiskImageFileExtensions.Td0], UcsdImgConversionService.CanCreate,
                (image, _, path, _, token) => WriteUcsdAsync(linear, td0, image, path, token)),
            Flux(MediaImageWriterIds.Scp, DiskImageFormatIds.RawScp, DiskImageFileExtensions.Scp,
                (image, metadata, path, token) => WriteSingleAsync(scp.WriteAsync(path, ProtectedTrackScpImageAdapter.Create(image, metadata), token), path)),
            Flux(MediaImageWriterIds.Hfe, DiskImageFormatIds.RawHfe, DiskImageFileExtensions.Hfe,
                (image, metadata, path, token) => WriteSingleAsync(hfe.WriteAsync(ProtectedTrackHfeImageAdapter.Create(image, metadata), path, token), path)),
            hardDiskRaw,
            hardDiskQcow2,
            hardDiskVhd,
            hardDiskVhdx,
            hardDiskVdi,
            hardDiskVmdk,
            hardDiskChd,
            opticalIso,
            opticalBinCue,
            tapeWav,
            tapeAtariCas,
            tapeTzx,
            tapeSpectrumTap,
            tapeCommodoreTap,
            tapeMsxCas,
            tapeUef,
            tapeSimh
        ];
        return new MediaWritingComposition(writers);
    }

    private static SectorMediaImageWriterAdapter Sector(
        string id,
        IReadOnlyList<string> extensions,
        Func<string, string, bool> supports,
        Func<SectorImage, MediaImageDocument, string, string, CancellationToken, Task<IReadOnlyList<string>>> write)
        => new(
            id,
            FormatIds(supports, extensions),
            extensions,
            (_, _, formatId, extension) => supports(formatId, extension),
            write);

    private static FluxMediaImageWriterAdapter Flux(
        string id,
        string formatId,
        string extension,
        Func<ProtectedTrackImage, IReadOnlyDictionary<string, string>, string, CancellationToken, Task<IReadOnlyList<string>>> write)
        => new(
            id,
            [formatId],
            [extension],
            (_, targetFormatId, targetExtension) =>
                targetFormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) &&
                targetExtension.Equals(extension, StringComparison.OrdinalIgnoreCase),
            (image, metadata, path, _, token) => write(image, metadata, path, token));

    private static IReadOnlyList<string> FormatIds(
        Func<string, string, bool> supports,
        IReadOnlyList<string> extensions)
        => Formats.Formats
            .Where(format => extensions.Any(extension => supports(format.Id, extension)))
            .Select(format => format.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static async Task<IReadOnlyList<string>> WriteSingleAsync(Task write, string outputPath)
    {
        await write.ConfigureAwait(false);
        return [outputPath];
    }

    private static Task<IReadOnlyList<string>> WriteAppleSectorAsync(
        AppleRawImageWriter raw,
        TwoImgWriter twoImg,
        SectorImage image,
        string path,
        string formatId,
        CancellationToken cancellationToken)
    {
        AppleSectorConversionValidationFunctions.Validate(image, formatId);
        return Path.GetExtension(path).Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase)
            ? WriteSingleAsync(twoImg.WriteAsync(image, path, formatId, cancellationToken), path)
            : WriteSingleAsync(raw.WriteAsync(image, path, formatId, cancellationToken), path);
    }

    private static Task<IReadOnlyList<string>> WriteMacintoshAsync(
        MacintoshRawImageWriter raw,
        DiskCopyWriter diskCopy,
        SectorImage image,
        MediaImageDocument document,
        string path,
        CancellationToken cancellationToken)
        => Path.GetExtension(path).Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase)
            ? WriteSingleAsync(raw.WriteAsync(image, path, cancellationToken), path)
            : WriteSingleAsync(diskCopy.WriteAsync(image, path, DiskCopyMetadataFunctions.Restore(image, document.Metadata), cancellationToken), path);

    private static Task<IReadOnlyList<string>> WriteAtariStAsync(
        AtariStWriter st,
        MsaWriter msa,
        SectorImage image,
        MediaImageDocument document,
        string path,
        string formatId,
        CancellationToken cancellationToken)
    {
        var sourceIsScp = Path.GetExtension(document.Source.PrimaryPath)
            .Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);
        var transformed = AtariStGeometryTransformer.Transform(image, formatId, sourceIsScp);
        return Path.GetExtension(path).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase)
            ? WriteSingleAsync(msa.WriteAsync(transformed, path, cancellationToken), path)
            : WriteSingleAsync(st.WriteAsync(transformed, path, cancellationToken), path);
    }

    private static Task<IReadOnlyList<string>> WriteCommodoreDosAsync(
        CommodoreDosContainerWriter writer,
        SectorImage image,
        string path,
        string formatId,
        CancellationToken cancellationToken)
    {
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Commodore source format '{image.FormatId}' cannot be written as '{formatId}' without a geometry transformation.");
        var errorMapMode = image.AvailableBlocks.All(block => block.DiagnosticCode.HasValue)
            ? CommodoreDosErrorMapMode.Preserve
            : CommodoreDosErrorMapMode.None;
        return WriteSingleAsync(writer.WriteAsync(image, path, errorMapMode, cancellationToken), path);
    }

    private static Task<IReadOnlyList<string>> WriteAmstradAsync(
        CpcDskWriter writer,
        SectorImage image,
        string path,
        CancellationToken cancellationToken)
    {
        var kind = Path.GetExtension(path).Equals(DiskImageFileExtensions.Edsk, StringComparison.OrdinalIgnoreCase)
            ? CpcDskContainerKind.Extended
            : CpcDskContainerKind.Standard;
        return WriteSingleAsync(writer.WriteAsync(CpcDskImageBuilder.Build(image, kind), path, cancellationToken), path);
    }

    private static Task<IReadOnlyList<string>> WriteEpsonAsync(
        EpsonQx10RawImageWriter raw,
        ImdWriter imd,
        SectorImage image,
        MediaImageDocument document,
        string path,
        string formatId,
        CancellationToken cancellationToken)
    {
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Epson source format '{image.FormatId}' does not match '{formatId}'.");
        return Path.GetExtension(path).Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase)
            ? WriteSingleAsync(imd.WriteAsync(ImdMetadataFunctions.Restore(image, document.Metadata) ?? ImdImageBuilder.BuildEpson(image), path, cancellationToken), path)
            : WriteSingleAsync(raw.WriteAsync(image, path, formatId, cancellationToken), path);
    }

    private static Task<IReadOnlyList<string>> WriteUcsdAsync(
        LinearSectorImageWriter linear,
        Td0Writer td0,
        SectorImage image,
        string path,
        CancellationToken cancellationToken)
    {
        var source = image.WithFormatId(DiskImageFormatIds.UcsdIbmMfm);
        return Path.GetExtension(path).Equals(DiskImageFileExtensions.Td0, StringComparison.OrdinalIgnoreCase)
            ? WriteSingleAsync(td0.WriteAsync(source, path, cancellationToken), path)
            : WriteSingleAsync(linear.WriteAsync(source, path, UcsdIbmMfmGeometry.SectorGeometry, cancellationToken), path);
    }
}
