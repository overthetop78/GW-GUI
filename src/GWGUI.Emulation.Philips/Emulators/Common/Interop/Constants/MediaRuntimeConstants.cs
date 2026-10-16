namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const int VideopacBiosBytes = 1024;
    internal const string OdysseyBiosFileName = "o2rom.bin";
    internal const string VideopacBiosFileName = "c52.bin";
    internal const string VideopacPlusBiosFileName = "g7400.bin";
    internal const string JopacBiosFileName = "jopac.bin";
    internal const string OdysseyBiosMd5 = "562d5ebf9e030a40d6fabfc2f33139fd";
    internal const string VideopacBiosMd5 = "f1071cdb0b6b10dde94d3bc8a6146387";
    internal const string VideopacPlusBiosMd5 = "c500ff71236068e0dc0d0603d265ae76";
    internal const string JopacBiosMd5 = "279008e4a0db2dc5f1c048853b033828";
    internal const string CdiBiosFileName = "cdimono1.zip";
    internal const string SameCdiDirectoryName = "same_cdi";
    internal const string CdiBiosDirectoryName = "bios";
    internal const string Cdi200Sha1 = "d961de803c89b3d1902d656ceb9ce7c02dccb40a";
    internal const string Cdi220Sha1 = "53360a1f21ddac952e95306ced64186a3fc0b93e";
    internal const string CdiServoSha1 = "fdf8d78d6a0df4a56b5b963d72eabd39fcec163f";
    internal const string CdiSlaveSha1 = "56d0acd7caad51c7de703247cd6d842b36173079";
    internal const int CdiCpuRomBytes = 512 * 1024;
    internal const int CdiControllerRomBytes = 8 * 1024;
    internal const string Cdi200RomFileName = "cdi200.rom";
    internal const string Cdi220RomFileName = "cdi220b.rom";
    internal const string CdiSlaveRomFileName = "zx405042p__cdi_slave_2.0__b43t__zzmk9213.mc68hc705c8a_withtestrom.7206";
    internal const string CdiServoRomFileName = "zx405037p__cdi_servo_2.1__b43t__llek9215.mc68hc705c8a_withtestrom.7201";
    internal const int VideopacRomBanks = 8;
    internal const int VideopacSmallBankBytes = 2048;
    internal const int VideopacLargeBankBytes = 3072;
    internal static readonly IReadOnlySet<long> MegaCartSizes = new HashSet<long> { 32768, 65536, 131072, 262144, 524288, 1048576 };
    internal const string PlaylistExtension = ".m3u";

    internal static string FileName(MachineModel model) => model switch
    {
        MachineModel.Odyssey2 => OdysseyBiosFileName,
        MachineModel.VideopacG7000 => VideopacBiosFileName,
        MachineModel.VideopacG7400 => VideopacPlusBiosFileName,
        MachineModel.JopacJo7400 => JopacBiosFileName,
        MachineModel.PhilipsCdi => CdiBiosFileName,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static string Md5(MachineModel model) => model switch
    {
        MachineModel.Odyssey2 => OdysseyBiosMd5,
        MachineModel.VideopacG7000 => VideopacBiosMd5,
        MachineModel.VideopacG7400 => VideopacPlusBiosMd5,
        MachineModel.JopacJo7400 => JopacBiosMd5,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
