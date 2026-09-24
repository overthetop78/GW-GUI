namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

[Flags]
internal enum SpartaDosDirectoryFlags : byte
{
    None = 0,
    Protected = 1 << 0,
    Hidden = 1 << 1,
    Archived = 1 << 2,
    InUse = 1 << 3,
    Deleted = 1 << 4,
    Subdirectory = 1 << 5
}
