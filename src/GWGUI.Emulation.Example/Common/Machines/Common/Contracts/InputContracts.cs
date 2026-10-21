using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Associations persistantes utilisées pour convertir les entrées physiques vers celles de la machine.</summary>
/// <param name="KeyboardMappings">Correspondances entre identifiants de touches émulées et touches physiques du contrat SDK.</param>
/// <param name="Controllers">Associations des ports émulés avec leurs périphériques et commandes physiques.</param>
/// <param name="MouseDeviceId">Identité facultative de la souris physique sélectionnée.</param>
/// <param name="CaptureMouse">Demande de capture du pointeur pour les interactions de la machine.</param>
/// <param name="ReleaseMouseKey">Touche physique permettant de quitter la capture du pointeur.</param>
/// <param name="KeyboardBindings">Expressions d’association textuelles des touches émulées, traitées par la projection Common.</param>
/// <remarks>Les valeurs nulles laissent la résolution des associations initiales aux fonctions d’entrée.
/// Le record conserve les collections reçues par référence : les changements de configuration se font avec des
/// collections stables plutôt qu’en modifiant celles utilisées par une session en cours.</remarks>
public sealed record InputConfiguration(IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null, IReadOnlyList<ControllerBinding>? Controllers = null,
    string? MouseDeviceId = null, bool CaptureMouse = false, EmulationKey ReleaseMouseKey = EmulationKey.Escape, IReadOnlyDictionary<string, string>? KeyboardBindings = null);

