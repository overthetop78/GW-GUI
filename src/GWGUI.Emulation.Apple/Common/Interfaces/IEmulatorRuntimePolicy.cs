namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorRuntimePolicy
{
    bool RequiresMediaRestart(EmulationMediaSlot slot, string? path);
    bool RequiresOptionRestart(string key);
}
