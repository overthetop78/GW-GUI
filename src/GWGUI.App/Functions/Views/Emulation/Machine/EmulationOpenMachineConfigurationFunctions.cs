namespace GWGUI.App.Functions.Views.Emulation.Machine;

internal static class EmulationOpenMachineConfigurationFunctions
{
    internal static bool TryApply<TValue>(
        IReadOnlyDictionary<(string ModuleId, Guid ConfigurationId), TValue> openMachines,
        string moduleId,
        Guid configurationId,
        Action<TValue> apply)
    {
        if (!openMachines.TryGetValue((moduleId, configurationId), out var value)) return false;
        apply(value);
        return true;
    }
}
