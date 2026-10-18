using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void InsertMedia(string path);
    void EjectMedia();
}
