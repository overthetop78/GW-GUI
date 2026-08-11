namespace GWGUI.MediaEngine.Containers.I86f;

[Flags]
public enum I86fFileFlags : ushort
{
    None = 0,
    TwoSided = 0x0008,
    SpeedShiftMask = 0x0060,
    ExtraBitCellCount = 0x0080,
    ReverseByteOrder = 0x0800,
    SpeedupOrExplicitBitCount = 0x1000
}
