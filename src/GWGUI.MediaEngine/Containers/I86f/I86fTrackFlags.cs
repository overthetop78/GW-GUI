namespace GWGUI.MediaEngine.Containers.I86f;

[Flags]
public enum I86fTrackFlags : ushort
{
    None = 0,
    EncodingMask = 0x0018,
    MfmEncoding = 0x0008
}
