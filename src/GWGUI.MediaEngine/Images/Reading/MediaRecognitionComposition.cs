using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.AcornAtom;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apridisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atx;
using GWGUI.MediaEngine.Images.Formats.Floppy.BbcDfs;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.Cp2;
using GWGUI.MediaEngine.Images.Formats.Floppy.CpcDsk;
using GWGUI.MediaEngine.Images.Formats.Floppy.D64;
using GWGUI.MediaEngine.Images.Formats.Floppy.D71;
using GWGUI.MediaEngine.Images.Formats.Floppy.D81;
using GWGUI.MediaEngine.Images.Formats.Floppy.I86f;
using GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Msa;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Rx02;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Images.Formats.Floppy.TeleDisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Xfd;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Raw;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Chd;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Qcow2;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vdi;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vhd;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vhdx;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Vmdk;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Images.Formats.Optical.CloneCd;
using GWGUI.MediaEngine.Images.Formats.Optical.Chd;
using GWGUI.MediaEngine.Images.Formats.Optical.Iso;
using GWGUI.MediaEngine.Images.Formats.Optical.Alcohol;
using GWGUI.MediaEngine.Images.Formats.Tape.AtariCas;
using GWGUI.MediaEngine.Images.Formats.Tape.CommodoreTap;
using GWGUI.MediaEngine.Images.Formats.Tape.MsxCas;
using GWGUI.MediaEngine.Images.Formats.Tape.Simh;
using GWGUI.MediaEngine.Images.Formats.Tape.SpectrumTap;
using GWGUI.MediaEngine.Images.Formats.Tape.Tzx;
using GWGUI.MediaEngine.Images.Formats.Tape.Uef;
using GWGUI.MediaEngine.Images.Formats.Tape.Wav;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.MediaEngine.Images.Reading;

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
                new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AdfReader(),
                new AcornAtomDskReader(),
                new ApridiskReader(),
                new BbcDfsReader(),
                new CoherentRawImageReader(),
                new DecRx02Reader(),
                new AtariStReader(),
                new MsaReader(),
                new AtrReader(),
                new XfdReader(),
                new AtxReader(),
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
                new GWGUI.MediaEngine.Images.Formats.Floppy.Hfe.HfeReader(),
                scpReader,
                new BinCueReader(),
                new CloneCdReader(),
                new AlcoholMdsReader(),
                new ChdOpticalReader(),
                new IsoReader(),
                new WavTapeReader(),
                new UefReader(),
                new AtariCasReader(),
                new TzxReader(),
                new CommodoreTapReader(),
                new MsxCasReader(),
                new SpectrumTapReader(),
                new SimhTapeReader(),
                new Qcow2Reader(),
                new VhdxReader(),
                new VhdReader(),
                new VdiReader(),
                new VmdkReader(),
                new ChdHardDiskReader(),
                new RawHardDiskReader()
            ]);
    }
}
