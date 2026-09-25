namespace GWGUI.Emulation.Atari.Common.Machines.AtariClassic.Enums;

public enum ClassicAudioCapability { Pokey, Tia, CartridgePokey, Mikey, Jerry }

public enum ClassicVideoCapability { Antic, Ctia, Gtia, Tia, Maria, Suzy, Mikey, Tom }

public enum ClassicCpu
{
    Mos6502B, Mos6502C, Mos6507, Sally6502C, Wdc65Sc02, Motorola68000,
    TomGraphicsProcessor, JerrySignalProcessor
}

public enum ClassicRegion { Ntsc, Pal, RegionFree }

public enum ClassicPortCapability
{
    Keyboard, Joystick, AnalogJoystick, Paddle, DrivingController, NumericKeypad,
    LightGun, ProLineController, EnhancedController
}

public enum ClassicStorageCapability { Floppy, Cassette, Cartridge, ExecutableFile, CompactDisc }
