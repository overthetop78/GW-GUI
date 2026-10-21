namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Correspondance entre noms de commandes et positions de boutons dans l’état de contrôleur commun.</summary>
/// <remarks>Les entiers représentent des indices de bits, pas les masques eux-mêmes.
/// La projection d’entrée construit les masques avec un décalage de bit ; plusieurs noms peuvent désigner
/// le même bouton physique, comme Fire1 et A.</remarks>
internal static class InputSnapshotDictionary
{
    /// <summary>Indices de boutons reconnus sans distinguer majuscules et minuscules dans les noms de commandes.</summary>
    /// <remarks>Les positions proviennent du contrat commun d’entrée ; leur ordre ne dépend pas de la présentation de la manette.</remarks>
    internal static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.Fire1] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Y] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Up] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.Down] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.Left] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.Right] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.X] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.L] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.R] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.L2] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.R2] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.L3] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.R3] = EmulationInputMappingConstants.R3Index
        };
}
