namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;

public enum FirmwareCategory { OperatingSystem, Basic, DiscFilingSystem, MasterMos, Language, Speech, Network, Application, CharacterGenerator }

public enum FirmwareCompatibility { Compatible, PartiallyCompatible, Incompatible }

public enum FirmwareDetectionStatus { Known, Unknown, Unreadable }

public enum FirmwareDistribution { UserSuppliedCopyrighted, EmbeddedCopyrighted, NoExternalFile }

public enum FirmwareEvidence { B2Sources }

public enum FirmwareHashAlgorithm { Md5 }

public enum FirmwareProvision { RequiredExternal, OptionalExternal, Embedded, EmbeddedReplaceable, NotUsed }
