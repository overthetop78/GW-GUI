namespace GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;

/// <param name="OutputBindings">Associations des commandes de retour du cœur aux périphériques physiques et à leurs moteurs ; null en absence de réglage.</param>
public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);

/// <summary>Commande native typée et définition neutre présentée dans l’éditeur des entrées.</summary>
/// <param name="Command">Commande matérielle conservée pour sa traduction vers le protocole du cœur.</param>
/// <remarks>Le catalogue conserve cette valeur typée ; la description commune des actions ne remplace pas le protocole natif.</remarks>
internal sealed record ControllerActionDefinition(ControllerCommand Command)
{
    /// <summary>Identifiant stable de la commande native, employé dans les associations.</summary>
    internal string Id => Command.ToString();
    /// <summary>Libellé et directions opposées conservés par la description matérielle du module.</summary>
    internal InputBindingDefinition Definition => new(Id, ResourceKey, string.Empty);
    /// <summary>Clé de traduction commune pour les directions et propre au module pour ses autres commandes.</summary>
    private string ResourceKey => Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id;
}
