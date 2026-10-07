namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Enums;

public enum CpuCompatibility
{
    Normal, Compatible, MemoryExact, Exact
}

public enum VideoStandard
{
    PALAuto, NTSCAuto, PAL, NTSC
}

public enum VideoResolution
{
    Automatic, AutomaticLow, AutomaticSuperHigh, Low, High, SuperHigh
}

public enum VideoAspect
{
    Automatic, PAL, NTSC, SquarePixels
}

public enum CropMode
{
    Disabled, Minimum, VerySmall, Small, Medium, Large, VeryLarge, Maximum, Automatic
}

public enum VideoLineMode
{
    Automatic, Single, Double
}

public enum RefreshRateChange
{
    Disabled, Enabled, Locked
}

public enum BlitterMode
{
    Normal, Immediate, Waiting
}

public enum CollisionMode
{
    None, Sprites, Playfields, Full
}

public enum AudioInterpolation
{
    None, Anti, Sinc, RH, Crux
}

public enum AudioFilter
{
    Emulated, Disabled, Enabled
}

public enum AudioFilterType
{
    Automatic, Standard, Enhanced
}

public enum AnalogMouseMode
{
    Disabled, Left, Right, Both
}
