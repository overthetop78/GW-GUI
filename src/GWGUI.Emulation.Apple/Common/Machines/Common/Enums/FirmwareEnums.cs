namespace GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

public enum FirmwareCategory { SystemRom }
public enum FirmwareCompatibility { Compatible, PartiallyCompatible, Incompatible }
public enum FirmwareDetectionStatus { Known, Unknown, Unreadable }
public enum FirmwareDistribution { UserSuppliedCopyrighted, EmbeddedCopyrighted, NoExternalFile }
public enum FirmwareEvidence { AppleWinSources, MiniVmacSources }
public enum FirmwareHashAlgorithm { Md5 }
public enum FirmwareProvision { RequiredExternal, OptionalExternal, Embedded, EmbeddedReplaceable, NotUsed }
