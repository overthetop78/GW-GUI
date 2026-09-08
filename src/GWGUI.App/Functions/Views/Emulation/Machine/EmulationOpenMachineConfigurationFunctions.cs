namespace GWGUI.App.Functions.Views.Emulation.Machine;

internal static class EmulationOpenMachineConfigurationFunctions
{
    internal static async Task<bool> TryApplyAsync<TValue>(
        IReadOnlyDictionary<(string ModuleId, Guid ConfigurationId), TValue> openMachines,
        string moduleId,
        Guid configurationId,
        Func<TValue, Task> apply)
    {
        if (!openMachines.TryGetValue((moduleId, configurationId), out var value)) return false;
        await apply(value);
        return true;
    }
}
