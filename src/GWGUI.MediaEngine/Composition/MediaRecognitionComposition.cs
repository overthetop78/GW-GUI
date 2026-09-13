using GWGUI.MediaEngine.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Formats.Floppy.AcornAtom;
using GWGUI.MediaEngine.Formats.Floppy.Apridisk;
using GWGUI.MediaEngine.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Formats.Floppy.Atx;
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
using GWGUI.MediaEngine.Formats.Floppy.Xfd;
using GWGUI.MediaEngine.Formats.HardDisk.Raw;
using GWGUI.MediaEngine.Formats.HardDisk.Chd;
using GWGUI.MediaEngine.Formats.HardDisk.Qcow2;
using GWGUI.MediaEngine.Formats.HardDisk.Vdi;
using GWGUI.MediaEngine.Formats.HardDisk.Vhd;
using GWGUI.MediaEngine.Formats.HardDisk.Vhdx;
using GWGUI.MediaEngine.Formats.HardDisk.Vmdk;
using GWGUI.MediaEngine.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Formats.Optical.CloneCd;
using GWGUI.MediaEngine.Formats.Optical.Chd;
using GWGUI.MediaEngine.Formats.Optical.Iso;
using GWGUI.MediaEngine.Formats.Optical.Alcohol;
using GWGUI.MediaEngine.Formats.Tape.AtariCas;
using GWGUI.MediaEngine.Formats.Tape.CommodoreTap;
using GWGUI.MediaEngine.Formats.Tape.MsxCas;
using GWGUI.MediaEngine.Formats.Tape.Simh;
using GWGUI.MediaEngine.Formats.Tape.SpectrumTap;
using GWGUI.MediaEngine.Formats.Tape.Tzx;
using GWGUI.MediaEngine.Formats.Tape.Uef;
using GWGUI.MediaEngine.Formats.Tape.Wav;
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
                new Formats.Floppy.Hfe.HfeReader(),
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
