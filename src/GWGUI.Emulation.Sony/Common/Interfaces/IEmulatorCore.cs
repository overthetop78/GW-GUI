namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    bool ShutdownRequested { get; }
    // La valeur vient du backend ; la boucle Common conserve la responsabilité de son arrêt.
    void SetControllerPortDevice(int port, PeripheralCategory type);
}
