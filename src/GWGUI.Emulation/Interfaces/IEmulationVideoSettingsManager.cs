namespace GWGUI.Emulation.Interfaces;

public interface IEmulationVideoSettingsManager
{
    IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi);
}
