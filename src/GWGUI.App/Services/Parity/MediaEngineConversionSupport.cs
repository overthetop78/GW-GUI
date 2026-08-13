using GWGUI.MediaEngine.Conversion.Acorn;
using GWGUI.MediaEngine.Conversion.Amiga;
using GWGUI.MediaEngine.Conversion.Amstrad;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Conversion.Commodore;
using GWGUI.MediaEngine.Conversion.Dec;
using GWGUI.MediaEngine.Conversion.Epson;
using GWGUI.MediaEngine.Conversion.Flux;
using GWGUI.MediaEngine.Conversion.Hfe;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Conversion.Msx;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Conversion.Ucsd;

namespace GWGUI.App.Services.Parity;

internal static class MediaEngineConversionSupport
{
    public static bool CanConvert(string sourcePathOrExtension, string formatId, string targetExtension) =>
        FluxContainerConversionService.CanConvert(SourcePath(sourcePathOrExtension), formatId, targetExtension) ||
        CanCreate(formatId, targetExtension);

    public static bool CanCreate(string formatId, string targetExtension) =>
        SectorImageScpFileConversionService.CanCreate(formatId, targetExtension) ||
        AmigaAdfConversionService.CanCreate(formatId, targetExtension) ||
        AcornAdfConversionService.CanCreate(formatId, targetExtension) ||
        BbcDfsConversionService.CanCreate(formatId, targetExtension) ||
        IbmRawConversionService.CanCreate(formatId, targetExtension) ||
        MsxRawConversionService.CanCreate(formatId, targetExtension) ||
        AppleSectorConversionService.CanCreate(formatId, targetExtension) ||
        AppleNibbleConversionService.CanCreate(formatId, targetExtension) ||
        MacintoshConversionService.CanCreate(formatId, targetExtension) ||
        LisaConversionService.CanCreate(formatId, targetExtension) ||
        HfeConversionService.CanCreate(formatId, targetExtension) ||
        AtariStConversionService.CanCreate(formatId, targetExtension) ||
        D81ConversionService.CanCreate(formatId, targetExtension) ||
        AtrConversionService.CanCreate(formatId, targetExtension) ||
        CommodoreDosConversionService.CanCreate(formatId, targetExtension) ||
        CoherentConversionService.CanCreate(formatId, targetExtension) ||
        AmstradDskConversionService.CanCreate(formatId, targetExtension) ||
        EpsonQx10ConversionService.CanCreate(formatId, targetExtension) ||
        DecRx02ConversionService.CanCreate(formatId, targetExtension) ||
        UcsdImgConversionService.CanCreate(formatId, targetExtension);

    private static string SourcePath(string pathOrExtension) => pathOrExtension.StartsWith('.')
        ? "source" + pathOrExtension
        : pathOrExtension;
}
