using GWGUI.Emulation;


namespace GWGUI.App.Services.Emulation;

internal static class EmulationConfigurationDraftStore
{
    private static readonly Dictionary<(string ModuleId, string MachineId), IEmulationConfiguration> Drafts = [];

    internal static bool TryGet(string moduleId, string machineId, out IEmulationConfiguration configuration) =>
        Drafts.TryGetValue((moduleId, machineId), out configuration!);

    internal static void Set(string moduleId, IEmulationConfiguration configuration) =>
        Drafts[(moduleId, configuration.MachineId)] = configuration;

    internal static void Remove(string moduleId, string machineId) =>
        Drafts.Remove((moduleId, machineId));
}
