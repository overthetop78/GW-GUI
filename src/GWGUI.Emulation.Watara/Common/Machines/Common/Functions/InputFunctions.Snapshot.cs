using GWGUI.Emulation;
using GWGUI.Emulation.Watara.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    /// <summary>Projette les entrées de session par le contrat Common et applique la bascule du pointeur.</summary>
    /// <param name="snapshot">Instantané physique emprunté, dont les collections restent inchangées.</param>
    /// <param name="configuration">Configuration complète et courante : modèle, cœur, options et associations.</param>
    /// <param name="mapper">Adaptateur emprunté chargé de traduire les commandes ; null utilise la projection de repli du module.</param>
    /// <param name="controllerPointerSwitchPressed">Impulsion de session ajoutant Contrôle droit aux touches projetées.</param>
    /// <returns>Instantané adapté au cœur ; sans impulsion, le résultat du projecteur est conservé tel quel.</returns>
    /// <remarks>La bascule est appliquée après la projection, comme pour les cœurs qui la prennent déjà en charge.
    /// Elle copie l'ensemble des touches avant ajout, conserve les autres entrées et ne modifie ni la source
    /// ni la configuration. Cette opération ne crée aucun cœur et ne possède aucune ressource native.</remarks>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper = null, bool controllerPointerSwitchPressed = false)
    {
        var mapped = mapper?.MapInput(snapshot, configuration) ?? InputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, configuration.Core);
        return controllerPointerSwitchPressed ? mapped with { Keys = new HashSet<EmulationKey>(mapped.Keys) { EmulationKey.RightControl } } : mapped;
    }


    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port);
            if (binding is null)
            {
                controllers[port] = EmulationControllerState.Empty;
                continue;
            }

            var source = EmulationSdk.InputProcessing.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var command in ControllerCatalog.Commands)
            {
                var mapping = binding.Mappings?.GetValueOrDefault(command.ToString()) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationSdk.InputProcessing.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (EmulationSdk.InputProcessing.IsMappedInputPressed(mapping, physical, snapshot))
                    buttons |= (uint)(command switch
                    {
                        ControllerCommand.A => ControllerButtons.A,
                        ControllerCommand.B => ControllerButtons.B,
                        ControllerCommand.Start => ControllerButtons.Start,
                        ControllerCommand.Select => ControllerButtons.Select,
                        _ => ControllerButtons.None
                    });
                var magnitude = EmulationSdk.InputProcessing.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (command)
                {
                    case ControllerCommand.Left:
                        horizontal -= magnitude;
                        break;
                    case ControllerCommand.Right:
                        horizontal += magnitude;
                        break;
                    case ControllerCommand.Up:
                        vertical -= magnitude;
                        break;
                    case ControllerCommand.Down:
                        vertical += magnitude;
                        break;
                }
            }

            controllers[port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Buttons = buttons,
                LeftX = EmulationSdk.InputProcessing.NormalizedAxis(horizontal),
                LeftY = EmulationSdk.InputProcessing.NormalizedAxis(vertical)
            };
        }

        return snapshot with
        {
            Keys = EmulationInputSnapshot.Empty.Keys,
            Pointer = EmulationInputSnapshot.Empty.Pointer,
            Controllers = controllers
        };
    }


}