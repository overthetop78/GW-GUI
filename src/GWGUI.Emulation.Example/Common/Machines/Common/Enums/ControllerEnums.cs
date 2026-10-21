namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Catégorie de périphérique émulé affectée à un port ou décrite par un modèle.</summary>
public enum PeripheralCategory
{
    /// <summary>Aucun périphérique émulé n’est raccordé au port.</summary>
    None,
    /// <summary>Le module résout le périphérique selon le modèle et le backend.</summary>
    Automatic,
    /// <summary>Clavier de la machine émulée.</summary>
    Keyboard,
    /// <summary>Joystick dont les axes peuvent conserver une amplitude analogique.</summary>
    AnalogJoystick
}
