using GWGUI.Emulation;
using GWGUI.Emulation.Example.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection des capacités de contrôleurs et application de leurs zones mortes aux entrées.</summary>
internal static class ControllerFunctions
{

    /// <summary>Applique la zone morte configurée aux contrôleurs associés à des ports valides.</summary>
    /// <param name="snapshot">Instantané emprunté ; ses collections ne sont pas modifiées.</param>
    /// <param name="bindings">Associations de ports ; null ou liste vide conserve l’instantané reçu.</param>
    /// <returns>Instantané avec un nouveau tableau de contrôleurs si des associations existent, sinon la même instance.</returns>
    /// <remarks>Les ports hors limites sont ignorés. Un port désactivé reçoit l’état vide ; les ports non concernés
    /// conservent leurs valeurs. Les touches et le pointeur de l’instantané restent partagés sans modification.</remarks>
    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot snapshot, IReadOnlyList<ControllerBinding>? bindings)
    {
        if (bindings is null || bindings.Count == BufferConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
            if (binding.Peripheral == PeripheralCategory.None)
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            var controller = controllers[binding.Port];
            controllers[binding.Port] = EmulationInputMappingFunctions.ApplyDeadZones(controller, binding.DeadZonePercent);
        }
        return snapshot with { Controllers = controllers };
    }

    /// <summary>Décrit les catégories proposées sur les ports de contrôleur du modèle.</summary>
    /// <param name="model">Modèle dont les ports matériels sont lus dans le catalogue.</param>
    /// <returns>Catégories issues des ports hors clavier, complétées par les choix désactivé et automatique.</returns>
    /// <remarks>L’ensemble supprime les catégories répétées lorsque plusieurs ports ont la même capacité.</remarks>
    internal static IReadOnlySet<PeripheralCategory> Peripherals(MachineModel model)
    {
        return new HashSet<PeripheralCategory>(HardwareModelCatalog.Get(model).Ports.Where(port => port.Capability != HardwarePortCapability.Keyboard).Select(port => FromCapability(port.Capability)).Append(PeripheralCategory.None)
            .Append(PeripheralCategory.Automatic));
    }

    /// <summary>Convertit une capacité de port matériel vers la catégorie de périphérique du module.</summary>
    /// <param name="capability">Capacité clavier ou joystick analogique.</param>
    /// <returns>Catégorie interne correspondant à cette capacité.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La capacité n’a pas de correspondance déclarée.</exception>
    private static PeripheralCategory FromCapability(HardwarePortCapability capability) => capability switch
    {
        HardwarePortCapability.Keyboard => PeripheralCategory.Keyboard,
        HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}


