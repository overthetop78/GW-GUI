namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Enums;

internal enum HostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose
}
