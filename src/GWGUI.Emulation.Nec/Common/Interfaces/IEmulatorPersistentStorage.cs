namespace GWGUI.Emulation.Nec.Common.Interfaces;

internal interface IEmulatorPersistentStorage
{
    void EnsurePersistentStorage(MachineConfiguration configuration, string directory);
}
