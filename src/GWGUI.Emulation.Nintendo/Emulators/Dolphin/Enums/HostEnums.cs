namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Enums;

internal enum HostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose, SoftReset
}


