namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Disponibilité d’une commande de la machine pour son état courant.</summary>
public enum ShortcutAvailability
{
    /// <summary>La commande peut être exécutée.</summary>
    Available,
    /// <summary>La commande ne peut pas être exécutée dans cet état.</summary>
    Unavailable,
}
