using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Commandes typées d’un cœur dont le module décrit les médias et les périphériques.</summary>
public interface IEmulationCoreCommands<TMedia, TPeripheral> : IEmulationCoreStateCommands
{
    /// <summary>Insère le média décrit par la configuration propre au module.</summary>
    void InsertMedia(TMedia media);
    /// <summary>Éjecte le média du lecteur désigné par le slot commun.</summary>
    void EjectMedia(EmulationMediaSlot slot);
    /// <summary>Raccorde au port indiqué un périphérique reconnu par le module.</summary>
    void SetControllerPortDevice(int port, TPeripheral peripheral);
}
