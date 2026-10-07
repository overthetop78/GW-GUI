namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Enums;

internal enum HostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose, SoftReset
}


