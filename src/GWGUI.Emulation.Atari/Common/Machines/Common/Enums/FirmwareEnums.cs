namespace GWGUI.Emulation.Atari.Common.Machines.Common.Enums;

public enum FirmwareCategory
{
    Tos,
    AtariSystemOs,
    AtariOsA,
    AtariOsB,
    AtariXlOs,
    AtariBasic,
    Atari5200Bios,
    AtariXegsBios,
    Atari7800Bios,
    LynxBootRom,
    JaguarBootRom,
    JaguarCdBios
}



public enum FirmwareDistribution { UserSuppliedCopyrighted, BuiltInOpenReplacement, NoExternalFile }

public enum FirmwareEvidence
{
    HatariCoreInformation, Atari800CoreInformation, StellaCoreInformation, ProSystemCoreInformation,
    BeetleLynxCoreInformation, VirtualJaguarCoreInformation
}

public enum FirmwareHashAlgorithm { Md5 }

public enum FirmwareProvision { RequiredExternal, OptionalExternal, Embedded, EmbeddedReplaceable, NotUsed }
