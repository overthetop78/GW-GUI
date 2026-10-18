namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreStateCommands
{
    void HardReset();
    void Stop();
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
    void SelectDisk(int index);
}

