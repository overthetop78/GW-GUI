namespace GWGUI.Emulation.Functions;

internal static class EmulationMachineModelFunctions
{
    internal static string DisplayResourceKey(IReadOnlyList<EmulationMachineDefinition> models, IEmulationConfiguration configuration) =>
        models.Single(model => model.Id == configuration.MachineId).DisplayResourceKey;

    internal static TModel Parse<TModel>(string id) where TModel : struct, Enum =>
        Enum.TryParse<TModel>(id, out var model) && Enum.IsDefined(model)
            ? model : throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
