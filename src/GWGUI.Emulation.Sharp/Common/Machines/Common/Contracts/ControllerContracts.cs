namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Contracts;

/// <param name="OutputBindings">Associations des commandes de retour du cœur aux périphériques physiques et à leurs moteurs ; null en absence de réglage.</param>
public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);

/// <summary>Commande native de contrôleur et règle de couplage des directions analogiques.</summary>
/// <param name="Command">Commande typée conservée pour la traduction vers le protocole du cœur.</param>
/// <param name="Analog">Active les directions opposées et les commandes de gaz de la définition analogique.</param>
/// <remarks>La définition neutre expose les libellés à l’éditeur ; Command et Analog restent les données matérielles utilisées par le module.</remarks>
internal sealed record ControllerActionDefinition(ControllerCommand Command, bool Analog = false)
{
    /// <summary>Identifiant stable de la commande native, employé dans les associations.</summary>
    internal string Id => Command.ToString();
    /// <summary>Libellé et directions opposées conservés par la description matérielle du module.</summary>
    internal InputBindingDefinition Definition => new(Id, Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, string.Empty)
    {
        OppositeDirectionId = Analog ? Command switch
        {
            ControllerCommand.Up => ControllerCommand.Down.ToString(),
            ControllerCommand.Down => ControllerCommand.Up.ToString(),
            ControllerCommand.Left => ControllerCommand.Right.ToString(),
            ControllerCommand.Right => ControllerCommand.Left.ToString(),
            ControllerCommand.ThrottleIncrease => ControllerCommand.ThrottleDecrease.ToString(),
            ControllerCommand.ThrottleDecrease => ControllerCommand.ThrottleIncrease.ToString(),
            _ => null
        } : null
    };
}
