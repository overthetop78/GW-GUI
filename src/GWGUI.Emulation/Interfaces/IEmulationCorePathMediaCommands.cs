namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCorePathMediaCommands : IEmulationCoreStateCommands
{
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void InsertMedia(string path);
    void EjectMedia();
}