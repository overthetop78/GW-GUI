namespace GWGUI.Emulation.Amiga.Common.Enums;

public enum Emulator { External }

internal enum HostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose
}
