namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Dictionaries;

/// <summary>Associe les touches particulières émulées aux touches physiques proposées par défaut.</summary>
/// <remarks>Cette correspondance est utilisée lors de la description des réglages et de la conversion des entrées.
/// Les associations enregistrées par l’utilisateur peuvent ensuite la remplacer.</remarks>
internal static class ExampleMachineAKeyboardDictionary
{
    /// <summary>Table orientée de la touche émulée vers la touche de l’hôte : F0 vers F10, Copy vers Insert et ShiftLock vers PageDown.</summary>
    /// <remarks>Les touches du pavé numérique absentes de cette table n’ont pas d’affectation physique par défaut.</remarks>
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys = new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.AcornF0] = EmulationKey.F10,
            [EmulationKey.AcornCopy] = EmulationKey.Insert,
            [EmulationKey.AcornShiftLock] = EmulationKey.PageDown
        };
}
