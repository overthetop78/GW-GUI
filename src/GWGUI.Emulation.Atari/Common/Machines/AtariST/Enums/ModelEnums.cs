namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Enums;

public enum StAudioCapability
{
    ProgrammableSoundGenerator, StereoDma, Microwire, DigitalSignalProcessor, Microphone
}

public enum StVideoCapability
{
    Pal, Ntsc, Monochrome, Blitter, EnhancedPalette, HardwareScrolling, TtShifter, Videl
}

public enum StCpu { Motorola68000, Motorola68030 }

public enum StCpuPrecision { Compatible, CycleExact }

public enum StFpu { None, Motorola68881, Motorola68882 }

public enum StRegion
{
    UnitedStates, Germany, France, UnitedKingdom, Spain, Italy, Sweden, Switzerland,
    Finland, Norway, CzechRepublic, Russia, Greece, Multilingual
}

public enum StPortCapability
{
    Keyboard, Mouse, Joystick, EnhancedJoystick, Midi, Parallel, Serial, Cartridge,
    LocalAreaNetwork, Vme
}

public enum StStorageCapability
{
    FloppyDoubleDensity, FloppyHighDensity, Acsi, Ide, Scsi, GemdosDirectory
}

internal enum TosVariant { Atari, EmuTos, KaosTos }
