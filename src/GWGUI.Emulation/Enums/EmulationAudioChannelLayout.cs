namespace GWGUI.Emulation.Enums;

[Flags]
public enum EmulationAudioChannelLayout : uint
{
    None = 0,
    FrontLeft = 0x001,
    FrontRight = 0x002,
    FrontCenter = 0x004,
    LowFrequency = 0x008,
    BackLeft = 0x010,
    BackRight = 0x020,
    SideLeft = 0x200,
    SideRight = 0x400,
    Mono = FrontCenter,
    Stereo = FrontLeft | FrontRight,
    Surround51 = Stereo | FrontCenter | LowFrequency | SideLeft | SideRight,
    Surround71 = Surround51 | BackLeft | BackRight
}
