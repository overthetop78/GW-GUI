namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{
    /// <summary>Nombre des images annoncé par le cœur ou son dernier statut de processus.</summary>
    int DiskCount { get; }
    /// <summary>Index sélectionné ; moins un en l’absence de sélection.</summary>
    int CurrentDiskIndex { get; }

}
