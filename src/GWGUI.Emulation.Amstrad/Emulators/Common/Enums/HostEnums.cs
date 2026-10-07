namespace GWGUI.Emulation.Amstrad.Emulators.Common.Enums;

internal enum HostCommand : byte
{
    Initialize = CoreHostConstants.FirstHostCommandId, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose, SoftReset
}
