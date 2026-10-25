namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Associations persistantes des sources d’entrée, sans propriété des périphériques physiques.</summary>
/// <param name="KeyboardMappings">Association de touches physiques aux touches émulées.</param>
/// <param name="Controllers">Associations des ports de contrôleurs.</param>
/// <param name="MouseDeviceId">Identité de la source physique de pointeur.</param>
/// <param name="CaptureMouse">Choix de capture du pointeur pendant l’émulation.</param>
/// <param name="ReleaseMouseKey">Touche qui libère le pointeur capturé.</param>
/// <param name="KeyboardBindings">Expressions configurables des actions de clavier.</param>
/// <param name="MouseMappings">Expressions des boutons et molettes de souris, indexées par leur commande.</param>
/// <param name="ParallelJoystickAdapterEnabled">Active deux ports supplémentaires ; false conserve les deux ports matériels de base.</param>
public sealed record InputConfiguration(IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null, IReadOnlyList<ControllerBinding>? Controllers = null, string? MouseDeviceId = null, bool CaptureMouse = false, EmulationKey ReleaseMouseKey = EmulationKey.Escape, IReadOnlyDictionary<string, string>? KeyboardBindings = null, IReadOnlyDictionary<string, string>? MouseMappings = null, bool ParallelJoystickAdapterEnabled = false);
