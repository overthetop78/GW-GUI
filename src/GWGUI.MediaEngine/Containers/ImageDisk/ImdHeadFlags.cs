namespace GWGUI.MediaEngine.Containers.ImageDisk;

[Flags]
public enum ImdHeadFlags : byte
{
    HeadMask = 0x01,
    HasHeadMap = 0x40,
    HasCylinderMap = 0x80
}
