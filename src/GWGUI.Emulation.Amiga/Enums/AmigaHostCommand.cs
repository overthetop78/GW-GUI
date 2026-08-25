namespace GWGUI.Emulation.Amiga.Enums;

internal enum AmigaHostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose
}
