namespace GWGUI.Emulation.Atari.Common.Machines.Common.Enums;

public enum HardwareAudioCapability { Pokey, Tia, CartridgePokey, Mikey, Jerry }

public enum HardwareVideoCapability { Antic, Ctia, Gtia, Tia, Maria, Suzy, Mikey, Tom }

public enum HardwareCpu
{
    Mos6502B, Mos6502C, Mos6507, Sally6502C, Wdc65Sc02, Motorola68000,
    TomGraphicsProcessor, JerrySignalProcessor
}

public enum HardwareRegion { Ntsc, Pal, RegionFree }

public enum HardwarePortCapability
{
    Keyboard, Joystick, AnalogJoystick, Paddle, DrivingController, NumericKeypad,
    LightGun, ProLineController, EnhancedController
}

public enum HardwareStorageCapability { Floppy, Cassette, Cartridge, ExecutableFile, CompactDisc }
