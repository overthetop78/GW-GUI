namespace GWGUI.Emulation.Functions;

internal static class EmulationMachineModelFunctions
{
    internal static TModel Parse<TModel>(string id) where TModel : struct, Enum =>
        Enum.TryParse<TModel>(id, out var model) && Enum.IsDefined(model)
            ? model : throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
