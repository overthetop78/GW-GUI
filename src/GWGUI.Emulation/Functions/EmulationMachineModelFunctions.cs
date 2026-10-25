namespace GWGUI.Emulation.Functions;

/// <summary>Résolution des modèles décrits par un module indépendant, sans registre central de familles.</summary>
internal static class EmulationMachineModelFunctions
{
    /// <summary>Résout la ressource du modèle dont l’identifiant correspond à la configuration.</summary>
    public static string DisplayResourceKey(IReadOnlyList<EmulationMachineDefinition> models, IEmulationConfiguration configuration) =>
        models.Single(model => model.Id == configuration.MachineId).DisplayResourceKey;

    /// <summary>Convertit et valide une valeur de modèle dans l’enum propre au module.</summary>
    public static TModel Parse<TModel>(string id) where TModel : struct, Enum =>
        Enum.TryParse<TModel>(id, out var model) && Enum.IsDefined(model)
            ? model : throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
