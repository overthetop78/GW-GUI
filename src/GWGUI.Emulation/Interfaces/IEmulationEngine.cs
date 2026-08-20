namespace GWGUI.Emulation;

public interface IEmulationEngine<in TConfiguration>
{
    IEmulatedMachine CreateMachine(TConfiguration configuration);
}
