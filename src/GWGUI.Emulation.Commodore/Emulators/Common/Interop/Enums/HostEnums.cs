namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Enums;

internal enum HostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose,
    /// <summary>Exécute la réinitialisation logicielle par le contrat commun.</summary>
    SoftReset = CoreHostConstants.SoftResetCommand
}
