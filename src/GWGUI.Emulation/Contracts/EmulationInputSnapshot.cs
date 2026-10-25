namespace GWGUI.Emulation.Contracts;

public sealed record EmulationInputSnapshot(IReadOnlySet<EmulationKey> Keys, EmulationPointerState Pointer, IReadOnlyList<EmulationControllerState> Controllers)
{
    /// <summary>Caractères produits par les touches hôtes maintenues, selon la disposition active et ses modificateurs.</summary>
    /// <remarks>Facultatif : null conserve le parcours par touches. Un caractère nul signifie une touche imprimable sans caractère représentable.
    /// Les adaptateurs qui prennent en charge la saisie logique traduisent ces caractères dans leur propre matrice ; les autres conservent Keys.</remarks>
    public IReadOnlyDictionary<EmulationKey, char>? HostCharacters { get; init; }

    public static EmulationInputSnapshot Empty { get; } = new(
        new HashSet<EmulationKey>(),
        new EmulationPointerState(0, 0, 0, false, false, false, false, false, 0),
        [
            EmulationControllerState.Empty,
            EmulationControllerState.Empty,
            EmulationControllerState.Empty,
            EmulationControllerState.Empty
        ]);
}
