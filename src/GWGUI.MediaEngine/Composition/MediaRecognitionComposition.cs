using GWGUI.MediaEngine.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Formats.Floppy.BbcDfs;
using GWGUI.MediaEngine.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Formats.Floppy.Cp2;
using GWGUI.MediaEngine.Formats.Floppy.CpcDsk;
using GWGUI.MediaEngine.Formats.Floppy.D64;
using GWGUI.MediaEngine.Formats.Floppy.D71;
using GWGUI.MediaEngine.Formats.Floppy.D81;
using GWGUI.MediaEngine.Formats.Floppy.I86f;
using GWGUI.MediaEngine.Formats.Floppy.ImageDisk;
using GWGUI.MediaEngine.Formats.Floppy.Msa;
using GWGUI.MediaEngine.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Formats.Floppy.Rx02;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Formats.Floppy.St;
using GWGUI.MediaEngine.Formats.Floppy.TeleDisk;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered media image readers and their recognition service.</summary>
public sealed class MediaRecognitionComposition
{
    private MediaRecognitionComposition(ScpReader scpReader, IReadOnlyList<IMediaImageReader> readers)
    {
        ScpReader = scpReader;
        Readers = readers;
        Registry = new MediaRecognitionRegistry(readers);
        ReadingService = new MediaImageReadingService(Registry);
    }

    public ScpReader ScpReader { get; }

    public IReadOnlyList<IMediaImageReader> Readers { get; }

    public MediaRecognitionRegistry Registry { get; }

    public MediaImageReadingService ReadingService { get; }

    public static MediaRecognitionComposition CreateDefault()
    {
        var scpReader = new ScpReader();
        return new MediaRecognitionComposition(
            scpReader,
            [
                new Formats.Floppy.Adf.AdfReader(),
                new BbcDfsReader(),
                new CoherentRawImageReader(),
                new DecRx02Reader(),
                new AtariStReader(),
                new MsaReader(),
                new AtrReader(),
                new D64Reader(),
                new D71Reader(),
                new D81Reader(),
                new AppleDiskImageReader(),
                new MsxRawImageReader(),
                new CpcDskReader(),
                new RawImgReader(),
                new IbmRawImageReader(),
                new Td0Reader(),
                new I86fReader(),
                new Cp2Reader(),
                new ImdReader(),
                new EpsonQx10RawImageReader(),
                new UcsdRawImageReader(),
                new Formats.Floppy.Hfe.HfeReader(),
                scpReader
            ]);
    }
}
